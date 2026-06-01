
param (
	[string] $Architecture,
	[string] $Configuration,
	[string] $SolutionDir,
	[string] $OutputPath
)

if ($Architecture -ne "AnyCPU") {
	Write-Host "Skipping post-release build steps for architecture: $architecture"
	return
}
if ($Configuration -ne "Release") {
	Write-Host "Skipping post-release build steps for configuration: $Configuration"
	return
}
Write-Host "Running post-release build steps for architecture: $Architecture and configuration: $Configuration"

$proc = start-process -FilePath "$($SolutionDir)Certificate\signtool.exe" -ArgumentList  "sign /sha1 890C5C4E81A72279B7D39F46DCC6831229133296 /tr http://timestamp.digicert.com /td sha256 /fd sha256  `"$($SolutionDir)PassiveInstall\bin\$($Configuration)\PassiveInstall.dll`""  -Wait -PassThru

if ($proc.ExitCode -ne 0) {
	Write-Host "Error signing the assembly. Exit code: $($proc.ExitCode)"
	exit $proc.ExitCode
}
else
{
	Write-Host "Successfully signed the assembly."
}

mkdir -Force -Path "$OutputPath"
Copy-Item -Path "$($SolutionDir)PassiveInstall\bin\$Configuration\*.dll" -Destination "$OutputPath\"
Copy-Item -Path "$($SolutionDir)About_PassiveInstall.help.txt" -Destination "$OutputPath\"
Copy-Item -Path "$($SolutionDir)License.rtf" -Destination "$OutputPath\"
Copy-Item -Path "$($SolutionDir)P_128x128.ico" -Destination "$OutputPath\"
Copy-Item -Path "$($SolutionDir)PassiveInstall.dll-help.xml" -Destination "$OutputPath\"
Copy-Item -Path "$($SolutionDir)README.md" -Destination "$OutputPath\"

Write-Host "Build innosetup installer"
$proc2 = start-process -FilePath "D:\ProgramsD\Inno Setup 6\ISCC.exe" -ArgumentList  "$($SolutionDir)InnoInstaller\InnoSetupPassiveInstall.iss /Qp"  -Wait -PassThru
if ($proc2.ExitCode -ne 0) {
	Write-Host "Error building the installer. Exit code: $($proc2.ExitCode)"
	exit $proc2.ExitCode
}
else
{
	Write-Host "Successfully built the installer."
}