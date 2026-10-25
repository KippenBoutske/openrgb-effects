#define MyAppName "OpenRGB Effects"
#define MyAppVersion "1.1.0"
#define MyAppPublisher "OpenRGB Effects"
#define MyAppExeName "OpenRgbEffects.exe"

[Setup]
AppId={{8F3C2A91-6B47-4E1D-9C55-2D8A71E0B4F6}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\OpenRGB Effects
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=..\dist
OutputBaseFilename=OpenRgbEffects-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupIconFile=..\src\OpenRgbEffects.App\Assets\app.ico
ArchitecturesInstallIn64BitMode=x64
DisableWelcomePage=no

[Languages]
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"

[Tasks]
Name: "desktopicon"; Description: "Bureaubladsnelkoppeling maken"; GroupDescription: "Extra snelkoppelingen:"; Flags: unchecked

[Files]
Source: "..\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{#MyAppName} starten"; Flags: nowait postinstall skipifsilent
