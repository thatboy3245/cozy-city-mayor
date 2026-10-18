; =====================================================================
; Inno Setup 6 Script for Cozy City Mayor (2D Pixel RPG Edition)
; Includes End-User License Agreement (EULA.txt)
; =====================================================================

#define MyAppName "Cozy City Mayor"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Maplewood Valley Interactive"
#define MyAppURL "https://github.com/thatboy3245/cozy-city-mayor"
#define MyAppExeName "CozyCityMayor.exe"

[Setup]
AppId={{8C7D49A2-3B1E-47D1-9C12-908F14E2A751}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=EULA.txt
OutputDir=installer_output
OutputBaseFilename=CozyCityMayor_Setup
SetupIconFile=assets\cozy_mayor.ico
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "build\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "EULA.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "assets\cozy_mayor.ico"; DestDir: "{app}\assets"; Flags: ignoreversion
Source: "src\*.cs"; DestDir: "{app}\src"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\assets\cozy_mayor.ico"
Name: "{group}\End-User License Agreement (EULA)"; Filename: "{app}\EULA.txt"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\assets\cozy_mayor.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
