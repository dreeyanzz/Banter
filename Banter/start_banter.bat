@echo off

REM --- SETUP CHECK (Unchanged) ---
SET "LOCAL_FLAG_FILE=%APPDATA%\Banter\.installed_flag"

IF NOT EXIST "%LOCAL_FLAG_FILE%" (
    ECHO "Running first-time setup for this user..."
    MKDIR "%APPDATA%\Banter"
    CALL "%~dp0install.bat"
    ECHO "Setup complete. Creating local flag file..."
    ECHO 1 > "%LOCAL_FLAG_FILE%"
    ECHO "Setup finished. Launching app."
)

REM --- LAUNCHER (Modified) ---
ECHO "Starting Banter in PowerShell..."

REM This command now does the following:
REM 1. Runs wt.exe with your "BanterProfile" (for colors/fonts)
REM 2. Sets the starting directory to your 'app' folder
REM 3. Explicitly starts powershell.exe
REM 4. Tells powershell to run your Banter.exe
START "Banter" wt.exe -p "Banter" -d "%~dp0app" powershell.exe -NoExit -Command "& '%~dp0app\Banter.exe'"

EXIT