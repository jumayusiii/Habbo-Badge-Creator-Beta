@echo off
setlocal
cd /d "%~dp0"

set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (
  echo Could not find the .NET Framework C# compiler.
  exit /b 1
)

if not exist "dist" mkdir "dist"

"%CSC%" /nologo /target:winexe /platform:anycpu /optimize+ /debug- ^
  /win32icon:"assets\habbo.ico" /win32manifest:"app.manifest" ^
  /resource:"assets\logo.png,logo.png" /resource:"assets\habbo-wordmark.png,habbo-wordmark.png" ^
  /out:"dist\Habbo Badge Creator Beta.exe" ^
  /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll ^
  "src\*.cs"

if errorlevel 1 (
  echo.
  echo Build failed.
  exit /b 1
)

echo.
echo Built: dist\Habbo Badge Creator Beta.exe
exit /b 0
