#!/usr/bin/env bash
# Builds dist/Habbo Badge Creator.exe with the .NET Framework compiler (no SDK needed).
set -euo pipefail
cd "$(dirname "$0")"
CSC="/c/Windows/Microsoft.NET/Framework64/v4.0.30319/csc.exe"
[ -f "$CSC" ] || CSC="/c/Windows/Microsoft.NET/Framework/v4.0.30319/csc.exe"
W=$(cygpath -w "$(pwd)")
mkdir -p dist
MSYS2_ARG_CONV_EXCL='*' "$CSC" -nologo -target:winexe -platform:anycpu -optimize+ \
  "-win32icon:$W\assets\habbo.ico" \
  "-win32manifest:$W\app.manifest" \
  "-resource:$W\assets\logo.png,logo.png" \
  "-resource:$W\assets\habbo-wordmark.png,habbo-wordmark.png" \
  "-out:$W\dist\Habbo Badge Creator Beta.exe" \
  -r:System.dll -r:System.Core.dll -r:System.Drawing.dll -r:System.Windows.Forms.dll \
  "$W\src\*.cs"
echo "Built: dist/Habbo Badge Creator Beta.exe"
