using System.Globalization;

namespace HabboBadgeCreator
{
    /// <summary>Two-language UI text lookup (English / German).</summary>
    internal static class L
    {
        public static bool De;

        static readonly CultureInfo EnCulture = new CultureInfo("en-GB");
        static readonly CultureInfo DeCulture = new CultureInfo("de-DE");

        public static CultureInfo Culture { get { return De ? DeCulture : EnCulture; } }

        public static string T(string en, string de)
        {
            return De ? de : en;
        }

        public static string F(string en, string de, params object[] args)
        {
            return string.Format(Culture, De ? de : en, args);
        }
    }
}
