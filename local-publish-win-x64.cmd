@@echo off
set OUTFOLDERPATH=%~dp0%artifacts
set DOTNET_CLI_TELEMETRY_OPTOUT=1
set DOTNET_NOLOGO=1
set BUILD_CONFIGURATION=Release
if exist "%OUTFOLDERPATH%\" rd %OUTFOLDERPATH% /S/Q >NUL
dotnet clean --configuration %BUILD_CONFIGURATION% --verbosity quiet
echo.
dotnet publish --configuration "%BUILD_CONFIGURATION%" --verbosity quiet -o "%OUTFOLDERPATH%" --self-contained -r win-x64 -p:PublishSingleFile=true
echo.
pause
