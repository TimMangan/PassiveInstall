@{
ModuleVersion = '2.5.0.0'
GUID = '7479f155-d1f0-420b-983a-a8844317896a'                
Author = 'TMurgent Technologies, LLP'
CompanyName = 'TMurgent Technologies, LLP'
Copyright = '© TMurgent Technologies, LLP. All rights reserved.'
Description = 'Passive and Silent Installation helper utilities PowerShell Module'
#RequiredAssemblies = 'PassiveInstall.dll'


# Location from which to download updateable help
#HelpInfoURI = "https://go.microsoft.com/fwlink/?LinkId=403112 "

FileList = @(
	'PassiveInstall.dll'
)

PowerShellVersion='3.0'
ClrVersion = '4.0'

FunctionsToExport = @()
VariablesToExport = @()
CmdletsToExport = @(
			'Approve-PassiveElevation',
			'Copy-PassiveFile', 'Copy-PassiveFolder',
			'Disable-PassiveWindowsServices',
			'Install-PassiveInstallFile',
			'Move-PassiveFolder', 'Move-PassiveRegistryKey',
			'New-PassiveAppPathSearch', 'New-PassiveDesktopShortcut', 'New-PassiveEnvironmentVariable', 'New-PassiveFolderIfNotPresent', 'New-PassiveRegistryKeyIfNotPresent', 'New-PassiveStartMenuShortcut',
			'Optimize-PassiveNgenCompileFile', 'Optimize-PassiveNgenQueues',
			'Remove-PasiveDesktopShortcuts', 'Remove-PassiveEnvironmentVariables', 'Remove-PassiveFiles', 'Remove-PassiveFolders', 'Remove-PassiveInstallerCacheFilePattern', 'Remove-PassiveInstallerCacheFolders', 'Remove-PassiveRegistryItem', 'Remove-PassiveStartMenuShortcuts', 'Remove-PassiveTopLevelFolderIfToday',
			'Set-PassiveServiceAccount', 'Set-PassiveShortcutFixupCmdBat', 'Set-PassiveShortcutFixupReg','Set-PassiveShortcutFixupSpaceAtEnd',  'Set-PassiveWinColors',  'Set-PassiveWinSize', 'Show-PassiveTimer' 
			)
AliasesToExport = @()
}
