; Script generated for Banter App
; 1. Place 'banter_icon.ico' in this folder before compiling.
; 2. Open this file in Inno Setup and press Run (F9).

[Setup]
; --- BASIC SETTINGS ---
AppName=Banter
AppVersion=1.0
DefaultDirName={autopf}\Banter
DefaultGroupName=Banter
Compression=lzma2
SolidCompression=yes
OutputBaseFilename=BanterSetup
OutputDir=Output
PrivilegesRequired=admin

; --- LOOK AND FEEL ---
SetupIconFile=BanterLogo.ico
DisableDirPage=no

[Files]
; --- COPYING FILES ---
; 1. Copy the icon explicitly
Source: "BanterLogo.ico"; DestDir: "{app}"; Flags: ignoreversion

; 2. Copy EVERYTHING else (including reset.bat)
Source: "*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.iss,Output,*.ico,*.bak"

[Icons]
; --- SHORTCUTS ---
Name: "{group}\Banter"; Filename: "{app}\start_banter.bat"; WorkingDir: "{app}"; IconFilename: "{app}\BanterLogo.ico"
Name: "{autodesktop}\Banter"; Filename: "{app}\start_banter.bat"; WorkingDir: "{app}"; IconFilename: "{app}\BanterLogo.ico"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons"; Flags: unchecked

[Run]
; --- RUN AFTER INSTALL ---
Filename: "{app}\start_banter.bat"; Description: "Launch Banter"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; --- RUN BEFORE UNINSTALL ---
; This runs reset.bat BEFORE the files are deleted.
; "waituntilterminated" ensures the uninstaller pauses until the cleanup is done.
; "runhidden" keeps it invisible (remove this flag if you want to see the terminal).
Filename: "{app}\reset.bat"; Flags: waituntilterminated runhidden