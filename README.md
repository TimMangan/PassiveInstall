# PassiveInstall
Module to support typical silent and passive style automated installation scripts

PassiveInstall is a module designed to make it easier to build automated scripts to install windows applications. This proves very useful in application packaging activities, as well as for application deployment.  

# Usage:
In a company environment, especially those at the "enterprise level", IT personel are responsible for preparing applications for deployment. This often involves re-packaging the vendor installer or just building scripts around them.  The purposes of this include:
  * Separation of dependencies
  * Customization of the installation features
  * Configuration of the app for the environment, often including disabling auto-updates
  * Setting confugurations typically used and eliminating "first use" activities for the end-user

# Commandlets:
The following is a summary of the cmdlets.  See PassiveInstall.dll-help.xml for more details (or run About-PassiveInstall).
  * **Approve-PassiveElevation**
		Checks if elevation for RunAsAdministrator is present.  With the -AsAdmin switch it will restart your script with a RunAs.
        NOTE: When using Approve-PassiveElevation with scripts on shares, the elevation account might not have access to the share, especially when your machine is not domain joined.  You can always copy scripts locally or start an elevated PowerShell/ISE and run the file from there.
  * **Copy-PassiveFile**	
		Copies a file to the designated location, making sure intervening directories are created if needed and 
		optionaly renaming it on the fly.  Existing files over-written by default.
  * **Copy-PassiveFolder**
		Copies a folder to the destination, making sure intervening directories are created if needed. Existing files not over-written by default.
  * **Disable-PassiveWindowsService**
		Checks to see that the service exists and disables it.  Note: it does not stop the service if running.
  * **Install-PassiveInstallFile**
		Performs appropriate (file type dependent) installation activity on MSI, MSP, EXE, ZIP, PS1, BAT, CMD, REG
  * **Move-PassiveFolder**
		Moves the source folder underneath the destination folder.  The destination folder will automatically be 
		created if not present.
  * **Move-PassiveRegistryKey** 
		Moves the source key underneath the destination key.  Typically used to move HKLM to HKCU. 
		The destination key will automatically be created if not present.
  * **New-PassiveAppPathSearch** 
		Registers the application for windows start menu search.
  * **New-PassiveDesktopShortcut** 
		Creates a shortcut on the current user's desktop as specified.
  * **New-PassiveEnvironmentVariable** 
		Changes the value of an existing environment variable or create a new one with the value.
  * **New-PassiveAppPathSearch**
		Creates/updates an AppPath entry to support start menu search for the target.
  * **New-PassiveFolderIfNotPresent** 
		Checks if a folder exists, and if not it creates it.
  * **New-PassiveRegistryKeyIfNotPresent** 
		Checks if a registry key exists, and if not it creates it.
  * **New-PassiveStartmenuShortcut** 
		Creates a shortcut on the start menu as specified.
  * **Optimize-PassiveNgenCompileFile**
		Performs an ngen compilation of an MSIL file into the native interface format.
  * **Optimize-PassiveNgenQueues** 
		Locates and flushes each of the NGEN optimization queues.
  * **Remove-PassiveDesktopShortcuts** 
		Removes the named shortcuts by name without needing to know the path.
  * **Remove-PassiveEnvironmentVariables** 
		Deletes the named environment variables, if present.
  * **Remove-PassiveFiles**
		Removes a list of files.
  * **Remove-PassiveFolders** 
		Deletes the named Folders, if present.
  * **Remove-PassiveInstallerCacheFilePattern**
		Removes files matching a filename pattern if located in the named folder, but with option for only if created in the last 24 hours.
  * **Remove-PassiveInstallerCacheFolders**
		Removes the named folder, but with options for only if emoty and only if created in the last 24 hours.
  * **Remove-PassiveRegistryItem**
		Deletes a registry item or key (including child items/nodes).
  * **Remove-PassiveStartMenuShortcuts** 
		Removes the named shortcuts by name, without needing to know the path.
  * **Set-PassiveShortcutFixupBatCmd** 
		Fixes an issue with shortcuts that occurs with App-V 5. Shortcuts that simply link to a file ending 
		in .Bat or .Cmd fail to work correctly under App-V 5.x.
        This fixup re-writes the shortcut to use C:\Windows\System32\cmd.exe pointing to the cmd/bat file.
  * **Set-PassiveShortcutFixupReg**
		Fixes an issue with shortcuts that occurs with App-V 5. Shortcuts that simply link to a file ending 
		in .Reg fail to work correctly under App-V 5.x.
        This fixup re-writes the shortcut to use C:\Windows\System32\reg.exe pointing to the reg file.
  * **Set-PassiveServiceAccount**
		Changes the service (logon) account for the named windows service.
  * **Set-PassiveShortcutFixupSpaceAtEnd**
		Fixes an issue with shortcuts that occurs with App-V 5. Shortcuts that have a space at the end of the 
		filename fail to work correctly under App-V 5.x.
        This fixup re-writes the shortcut to remove the trailing space in the name.
  * **Set-PassiveWinColors** 
		Changes the color scheme in the PowerShell window, optionally clearing the buffer.
  * **Set-PassiveWinSize** 
		Changes the width, height, position, and/or title, of the PowerShell window, or hides it altogether.
  * **Show-PassiveTimer**
		Sleep in ms with progress bar.  Optional message.

# Example:
````
# PassiveInstall.ps1
#
# Copyright 2017 TMurgent Technologies, LLP 
#
#
# PURPOSE:
#    Automates a custom installation of an application as either a passive or silent installation.
#    This script can be further modified to add registry settings or add dependencies.
##########################################################################################################################

#----------------------------------------------------------------------------------
# Standard coding starting here (DO NOT MODIFY)
# Get folder that this PS1 file is in so that we can find files correctly from relative references
$executingScriptDirectory = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

# Bring in common utility code
Import-Module "C:\Program Files\WindowsPowerShell\Modules\PassiveInstall\PassiveInstall.dll"

#If here, we are (now) running as an admin!
#---------------------------------------------------------------------------------

#==================================================================================
#                    MAKE CUSTOMIZATIONS HERE
$AppName = 'Trimble_SketchupViewer'
$InstallerLogFolder = "c:\Users\Public\Documents\SequencedPackage"
$InstallerLogFile = $InstallerLogFolder+'\Log_'+$AppName+'_MainInstaller.txt'
$DoSilent = 0  #Set to 1 for silent instead of passive

function Run_CustomInstall
{    
    Write-Host ""
    Write-Host ""
    Write-Host ""
    Write-Host ""
    Write-Host ""
    Write-Host ""
    if ($DoSilent -eq 1)
    {
        Install-PassiveInstallFile -Installer "$($executingScriptDirectory)\vcredist_14\vcredist_x64.exe" -Arguments '/q'-DoSilent
        Install-PassiveInstallFile -Installer "$($executingScriptDirectory)\SketchUpViewer2019-x64.msi" -Arguments '/qn DISABLEADVTSHORTCUTS=1' -DoSilent
    }
    else
    {
        Install-PassiveInstallFile -Installer "$($executingScriptDirectory)\vcredist_14\vcredist_x64.exe" -Arguments '/q'
        Install-PassiveInstallFile -Installer "$($executingScriptDirectory)\SketchUpViewer2019-x64.msi" -Arguments '/qn DISABLEADVTSHORTCUTS=1'
    }
    Copy-PassiveFile -Source "$($executingScriptDirectory)\PrivatePreferences.json" -Destination "$($env:LOCALAPPDATA)\SketchUp\SketchUp 2019\Sketchup"
    Remove-PassiveDesktopShortcuts -Names 'SketchUp Viewer.lnk'


    if ($DoSilent -eq 1)
    {
        Optimize-PassiveNgenQueues  -DoSilent
    }
    else
    {
        Optimize-PassiveNgenQueues 
    }
}
#                   end of CUSTOMIZATION area
#=================================================================================
#  Do NOT Modify below this line

New-PassiveFolderIfNotPresent $InstallerLogFolder
if ($DoSilent -eq 1)
{
    Set-PassiveWinSize 10 7 5 5 -Title 'PowerShell - PassiveInstall.ps1' -DoSilent
    # Ensure we are running elevated
    Approve-PassiveElevation -AsAdmin
}
else
{
    Set-PassiveWinSize 80 40 5 5 -Title "PowerShell - PassiveInstall.ps1 $($AppName)"
    Set-PassiveWinColors  -Background 'DarkGray' -Foreground 'White' 

    # Ensure we are running elevated
    Approve-PassiveElevation -AsAdmin

    Set-PassiveWinSize  100 44 10 10 -Title "[Elevated] PowerShell - PassiveInstall.ps1 $($AppName)"
    Set-PassiveWinColors  -Background 'DarkGray' -Foreground 'White' 
    
    Write-host 'Starting - PassiveInstall.ps1'
}

#If here, we are (now) running as an admin!
#---------------------------------------------------------------------------------

$err = Run_CustomInstall *>&1
if ($DoSilent -ne 1) 
{ 
    Write-Output $err 
}
Write-Output $err >> $InstallerLogFile

#---------------------------------------------------------------
#                  Standard wrapup area (DO NOT MODIFY)
write-host -ForegroundColor "Green"  "Done."
Show-PassiveTimer 20000 "End of script, P to pause."
Start-Sleep 20
#                    end
#---------------------------------------------------------------
````

# Download Binaries:
You can find pre-built and signed binaries at this link at [TMurgent Technologies]](http://www.tmurgent.com/AppV/en/resources/tools-downloads/tools-packaging/117-tools/packaging-tools/435-passiveinstall)

# Building from source:
  * Download the sources into Visual Studio with Wix Installer and extensions added. No NuGet packages are used.
  * Add a code signing certificate file into the root folder of the project (next to the solution) called "strongname.pfx".
  * If the certificate is password protected, edit all three project properties and in the post build script add /p password to the command lines.
  * Build x86 and x64 release versions of the PassiveInstall project.
  * Build the PassiveInstallx86 and PassiveInstallx64 Windows Installer projects. 