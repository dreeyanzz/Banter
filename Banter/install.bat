@echo off

::-------------------------------------
:: Check for Administrator permissions
::-------------------------------------
>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"

:: If error, we do not have admin. Go to UAC prompt.
if '%errorlevel%' NEQ '0' (
    echo Requesting administrative privileges...
    goto UACPrompt
) else ( goto gotAdmin )

:UACPrompt
    :: Create a VBScript to pop the UAC prompt
    echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
    echo UAC.ShellExecute "%~s0", "", "", "runas", 1 >> "%temp%\getadmin.vbs"

    :: Run the VBScript
    "%temp%\getadmin.vbs"
    del "%temp%\getadmin.vbs"
    exit /B

:gotAdmin
    :: We have admin! Proceed with the script.
::-------------------------------------

echo Running PowerShell installer...
powershell.exe -ExecutionPolicy Bypass -File "%~dp0\install_settings.ps1"