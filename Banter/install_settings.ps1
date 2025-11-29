# --- Installs custom Windows Terminal settings and local JetBrains Mono fonts ---

# Step 0: Check for Administrator permissions
# We need admin rights to write to the C:\Windows\Fonts folder.
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "Administrator permissions are required to install fonts."
    Write-Host "This script will exit. The .bat file should re-launch you as an admin."
    Read-Host "Press Enter to exit"
    exit
}

# Get the directory where this script is located
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition

# --- Step 1: Install JetBrains Mono fonts ---
Write-Host "Installing JetBrains Mono fonts..."

# Find the font files inside your folder
$FontDir = "$ScriptDir\JetBrainsMono-2.304\fonts"
if (-not (Test-Path $FontDir)) {
    Write-Error "Error: Font folder not found at '$FontDir'"
    Read-Host "Press Enter to exit"
    exit
}

# Look for .ttf files (TrueType Fonts)
$FontFiles = Get-ChildItem -Path $FontDir -Filter "*.ttf" -Recurse
if ($FontFiles.Count -eq 0) {
    Write-Error "Error: No .ttf font files found in '$FontDir'."
    Read-Host "Press Enter to exit"
    exit
}

# Copy fonts to the system fonts folder. Modern Windows will auto-register them.
$DestFontsDir = "$env:SystemRoot\Fonts"
$InstalledCount = 0

foreach ($FontFile in $FontFiles) {
    $DestFile = Join-Path -Path $DestFontsDir -ChildPath $FontFile.Name
    # Only copy if it doesn't already exist
    if (-not (Test-Path $DestFile)) {
        try {
            Copy-Item -Path $FontFile.FullName -Destination $DestFontsDir -Force -ErrorAction Stop
            Write-Host "Installed: $($FontFile.Name)"
            $InstalledCount++
        } catch {
            Write-Warning "Could not install $($FontFile.Name). $_"
        }
    } else {
        Write-Host "Skipped (already installed): $($FontFile.Name)"
    }
}
Write-Host "Font installation complete. $InstalledCount new fonts installed." -ForegroundColor Green


# --- Step 2: Copy Windows Terminal Settings ---
Write-Host "Installing Windows Terminal settings..."

# This is the dynamic path to the user's settings file
$DestinationPath = "$env:LOCALAPPDATA\Packages\Microsoft.WindowsTerminal_8wekyb3d8bbwe\LocalState"
$DestinationFile = "$DestinationPath\settings.json"

# Check if the Terminal folder exists (i.e., if it's installed)
if (-not (Test-Path $DestinationPath)) {
    Write-Error "Error: Windows Terminal settings folder not found!"
    Write-Error "Please make sure Windows Terminal is installed before running this script."
    Read-Host "Press Enter to exit"
    exit
}

# Back up the user's old settings
$BackupFile = "$DestinationFile.bak"
if (Test-Path $DestinationFile) {
    Write-Host "Backing up existing settings to $BackupFile..."
    Move-Item -Path $DestinationFile -Destination $BackupFile -Force
}

# Copy the new settings file (using your specific filename)
$SourceFile = "$ScriptDir\terminal_settings.json"

if (-not (Test-Path $SourceFile)) {
    Write-Error "Error: 'terminal_settings.json' was not found in the script folder."
    Read-Host "Press Enter to exit"
    exit
}

Write-Host "Copying new settings..."
Copy-Item -Path $SourceFile -Destination $DestinationFile -Force


# --- Step 3: All Done ---
Write-Host "---"
Write-Host "✅ Success! Your new terminal settings and fonts have been installed." -ForegroundColor Green
Write-Host "Please close and re-open all Windows Terminal windows to see the changes."
Write-Host "Your old settings are backed up as 'settings.json.bak' in the Terminal settings folder."
Read-Host "Press Enter to exit"