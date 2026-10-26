using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace HabboBadgeCreator
{
    /// <summary>
    /// Minimal Discord Rich Presence client speaking the local IPC protocol directly
    /// (\\.\pipe\discord-ipc-N) — no SDK, no extra files.
    /// A free Discord application ID + an uploaded art asset are required by Discord.
    /// </summary>
    internal static class DiscordRpc
    {
        const int OpHandshake = 0, OpFrame = 1, OpClose = 2, OpPing = 3, OpPong = 4;

        static readonly object writeLock = new object();
        static readonly int Pid = Process.GetCurrentProcess().Id;

        static Thread worker;
        static volatile bool running;
        static volatile bool connected;
        static NamedPipeClientStream pipe;
        static BinaryReader reader;
        static BinaryWriter writer;
        static ManualResetEventSlim handshakeDone;
        static string handshakeError = "";
        static volatile bool handshakeOk;

        internal static int TestPipeIndex = -1;

        static string appId = "", imageKey = "logo";
        static string detailsText = "", stateText = "";
        static volatile bool dirty = true;
        static long startUnix;
        static DateTime lastSend = DateTime.MinValue;

        static string status = "off";
        static string statusDetail = "";

        public static bool IsConnected { get { return connected; } }
        public static string Status { get { return status; } }         // off|connecting|connected|no-discord|error
        public static string StatusDetail { get { return statusDetail; } }
        public static event Action Changed;

        static void SetStatus(string s, string detail)
        {
            s = s ?? "off";
            detail = detail ?? "";
            if (status == s && statusDetail == detail) return;
            status = s;
            statusDetail = detail;
            if (Changed != null) Changed();
        }

        public static void Start(string id, string key)
        {
            Stop();
            if (string.IsNullOrEmpty(id) || id.Trim().Length == 0)
            {
                SetStatus("off", "");
                return;
            }
            appId = id.Trim();
            imageKey = string.IsNullOrEmpty(key) ? "logo" : key.Trim();
            startUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            running = true;
            dirty = true;
            worker = new Thread(Loop);
            worker.IsBackground = true;
            worker.Start();
        }

        public static void Stop()
        {
            running = false;
            connected = false;
            ClosePipe();
            Thread t = worker;
            worker = null;
            if (t != null && t.IsAlive)
            {
                try { t.Join(800); } catch { }
            }
            SetStatus("off", "");
        }

        /// <summary>Updates the two text lines shown in Discord.</summary>
        public static void SetActivity(string details, string state)
        {
            detailsText = details == null ? "" : details;
            stateText = state == null ? "" : state;
            dirty = true;
        }

        // ------------------------------------------------------------- worker
        static void Loop()
        {
            while (running)
            {
                SetStatus("connecting", "");
                if (!OpenPipe())
                {
                    SetStatus("no-discord", "");
                    Sleep(8000);
                    continue;
                }
                if (!Handshake())
                {
                    ClosePipe();
                    Sleep(6000);
                    continue;
                }
                connected = true;
                dirty = true;
                SetStatus("connected", "");
                while (running && connected)
                {
                    try
                    {
                        // Discord accepts ~5 activity updates per 20 s; keep a floor between
                        // frames so fast typing in the name field cannot get us rate limited.
                        double since = (DateTime.UtcNow - lastSend).TotalSeconds;
                        if ((dirty || since > 25) && (lastSend == DateTime.MinValue || since >= 3.5))
                            SendActivity();
                    }
                    catch
                    {
                        connected = false;
                    }
                    Sleep(350);
                }
                ClosePipe();
                connected = false;
                if (running)
                {
                    SetStatus("connecting", "");
                    Sleep(4000);
                }
            }
            SetStatus("off", "");
        }

        // The pipe is opened for asynchronous I/O on purpose: a pending synchronous read on
        // the same pipe handle would block our writes from another thread, so the activity frame
        // would never leave once the reader sits waiting for the next message.
        static bool OpenPipe()
        {
            if (TestPipeIndex >= 0)
            {
                NamedPipeClientStream tp = null;
                try
                {
                    tp = new NamedPipeClientStream(".", "discord-ipc-" + TestPipeIndex, PipeDirection.InOut, PipeOptions.Asynchronous);
                    tp.Connect(1500);
                }
                catch
                {
                    if (tp != null) tp.Dispose();
                    return false;
                }
                pipe = tp;
                reader = new BinaryReader(tp, new UTF8Encoding(false));
                writer = new BinaryWriter(tp, new UTF8Encoding(false));
                return true;
            }

            for (int i = 0; i < 10 && running; i++)
            {
                NamedPipeClientStream p = null;
                try
                {
                    p = new NamedPipeClientStream(".", "discord-ipc-" + i, PipeDirection.InOut, PipeOptions.Asynchronous);
                    p.Connect(300);
                }
                catch
                {
                    if (p != null) p.Dispose();
                    continue;
                }
                pipe = p;
                reader = new BinaryReader(p, new UTF8Encoding(false));
                writer = new BinaryWriter(p, new UTF8Encoding(false));
                return true;
            }
            return false;
        }

        static bool Handshake()
        {
            handshakeError = "";
            handshakeOk = false;
            handshakeDone = new ManualResetEventSlim(false);
            WriteFrame(OpHandshake, "{\"v\":1,\"client_id\":\"" + Esc(appId) + "\"}");

            Thread rt = new Thread(ReaderLoop);
            rt.IsBackground = true;
            rt.Start();

            handshakeDone.Wait(4000);
            if (handshakeError.Length > 0)
            {
                SetStatus("error", handshakeError);
                return false;
            }
            if (!handshakeOk)
            {
                SetStatus("error", "timeout");
                return false;
            }
            return true;
        }

        static void ReaderLoop()
        {
            try
            {
                while (running && pipe != null)
                {
                    int op;
                    string json;
                    if (!ReadFrame(out op, out json)) break;
                    if (op == OpPing) { WriteFrame(OpPong, json); continue; }
                    if (op == OpClose) break;
                    if (op != OpFrame) continue;

                    if (json.IndexOf("\"evt\":\"READY\"", StringComparison.Ordinal) >= 0)
                    {
                        handshakeOk = true;
                        if (handshakeDone != null) handshakeDone.Set();
                    }
                    else if (json.IndexOf("\"evt\":\"ERROR\"", StringComparison.Ordinal) >= 0)
                    {
                        string msg = ExtractMessage(json);
                        if (!connected)
                        {
                            handshakeError = msg.Length > 0 ? msg : "handshake failed";
                            if (handshakeDone != null) handshakeDone.Set();
                        }
                        else
                        {
                            SetStatus("connected", msg);
                        }
                    }
                }
            }
            catch { }

            connected = false;
            if (handshakeDone != null) handshakeDone.Set();
        }

        static string ExtractMessage(string json)
        {
            int i = json.IndexOf("\"message\"", StringComparison.Ordinal);
            if (i < 0) return "";
            i = json.IndexOf('"', json.IndexOf(':', i) + 1);
            if (i < 0) return "";
            StringBuilder sb = new StringBuilder();
            for (int j = i + 1; j < json.Length; j++)
            {
                if (json[j] == '\\' && j + 1 < json.Length) { sb.Append(json[++j]); continue; }
                if (json[j] == '"') break;
                sb.Append(json[j]);
            }
            return sb.ToString();
        }

        static void SendActivity()
        {
            string d = detailsText;
            string s = stateText;
            if (d == null || d.Length < 2) d = "Habbo Badge Creator";
            if (s == null || s.Length < 2) s = "40 × 40 px";
            WriteFrame(OpFrame, BuildActivityJson(Pid, startUnix, d, s, imageKey));
            lastSend = DateTime.UtcNow;
            dirty = false;
        }

        /// <summary>The SET_ACTIVITY payload (also used by the self-test).</summary>
        public static string BuildActivityJson(int pid, long start, string details, string state, string image)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"cmd\":\"SET_ACTIVITY\",\"args\":{\"pid\":").Append(pid);
            sb.Append(",\"activity\":{");
            sb.Append("\"details\":\"").Append(Esc(details)).Append("\",");
            sb.Append("\"state\":\"").Append(Esc(state)).Append("\",");
            sb.Append("\"assets\":{\"large_image\":\"").Append(Esc(image)).Append("\",");
            sb.Append("\"large_text\":\"Habbo Badge Creator\"},");
            sb.Append("\"timestamps\":{\"start\":").Append(start).Append("},");
            sb.Append("\"instance\":true}},");
            sb.Append("\"nonce\":\"").Append(Guid.NewGuid().ToString()).Append("\"}");
            return sb.ToString();
        }

        static string Esc(string s)
        {
            if (s == null) return "";
            StringBuilder sb = new StringBuilder();
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ') sb.Append(' ');
                        else sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        // ------------------------------------------------------------- framing
        static void WriteFrame(int op, string json)
        {
            byte[] payload = Encoding.UTF8.GetBytes(json);
            lock (writeLock)
            {
                if (writer == null) return;
                writer.Write(op);
                writer.Write(payload.Length);
                writer.Write(payload);
                writer.Flush();
            }
        }

        static bool ReadFrame(out int op, out string json)
        {
            op = -1;
            json = "";
            byte[] head = new byte[8];
            if (!ReadExact(head, 8)) return false;
            op = BitConverter.ToInt32(head, 0);
            int len = BitConverter.ToInt32(head, 4);
            if (len < 0 || len > 8 * 1024 * 1024) return false;
            byte[] payload = new byte[len];
            if (len > 0 && !ReadExact(payload, len)) return false;
            json = Encoding.UTF8.GetString(payload);
            return true;
        }

        static bool ReadExact(byte[] buf, int count)
        {
            int off = 0;
            while (off < count)
            {
                int n;
                try { n = reader.Read(buf, off, count - off); }
                catch { return false; }
                if (n <= 0) return false;
                off += n;
            }
            return true;
        }

        static void ClosePipe()
        {
            // Deliberately not taking writeLock: a write blocked on a dead pipe would
            // otherwise deadlock the caller. Disposing the stream first unblocks it.
            BinaryReader r = reader;
            BinaryWriter w = writer;
            NamedPipeClientStream p = pipe;
            reader = null;
            writer = null;
            pipe = null;
            try { if (p != null) p.Dispose(); } catch { }
            try { if (r != null) r.Dispose(); } catch { }
            try { if (w != null) w.Dispose(); } catch { }
        }

        static void Sleep(int ms)
        {
            for (int i = 0; i < ms && running; i += 100)
                Thread.Sleep(100);
        }
    }
}
