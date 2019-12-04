# PassiveInstall
Module to support typical silent and passive style automated installation scripts

PassiveInstall is a module designed to make it easier to build automated scripts to install windows applications. This proves very useful in application packaging activities, as well as for application deployment.  

# Usage:
In a company environment, especially those at the "enterprise level", IT personel are responsible for preparing applications for deployment. This often involves re-packaging the vendor installer or just building scripts around them.  The purposes of this include:
  * Separation of dependencies
  * Customization of the installation features
  * Configuration of the app for the environment, often including disabling auto-updates
  * Setting confugurations typically used and eliminating "first use" activities for the end-user

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
    #####SilentInstall_EnsureElevated $PSCommandPath
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
Show-PassiveTimer 20000 "End of script, Ctrl-C to end now or wait for timer. Left-Click on window to pause."
Start-Sleep 20
#                    end
#---------------------------------------------------------------
````

# Download Binaries:
You can find pre-built and signed binaries at this link at [TMurgent Technologies]](http://www.tmurgent.com/AppV/en/resources/tools-downloads/tools-packaging/117-tools/packaging-tools/435-passiveinstall)
