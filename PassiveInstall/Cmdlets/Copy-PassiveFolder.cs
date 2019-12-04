using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;
//using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    // Used to copy a folder, optionally changing the name.  It will ensure that the parent destination folders exist and create them if needed.

    [Cmdlet(VerbsCommon.Copy, "PassiveFolder", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class Copy_PassiveFolder : Cmdlet
    {
        private string _cmdlet = "Copy-PassiveFolder";

        #region ParameterDeclarations
        [Parameter(
            Mandatory = true,
            Position = 0,
            HelpMessage = "Folder to be copied/installed."
          )]
        public string Source
        {
            get { return _sourceFolder; }
            set { _sourceFolder = value; }
        }

        [Parameter(
            Mandatory = true,
            Position = 1,
            HelpMessage = "Destination Folder to copy into."
          )]
        public string Destination
        {
            get { return _destinationFolder; }
            set { _destinationFolder = value; }
        }
        #endregion

        #region ParameterData
        private string _sourceFolder = null;
        private string _destinationFolder = null;
        #endregion


        string output = "";

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            if (_sourceFolder != null && _destinationFolder != null)
            {
                WriteVerbose(_cmdlet + ": Starting source=" + _sourceFolder + " Destination=" + _destinationFolder);

                // Determine if source folder exists
                if (Directory.Exists(_sourceFolder))
                {
                    // Source was an entire folder to copy
                    if (this.ShouldProcess(_destinationFolder, "CopyDirectory"))
                    {
                        CopyThisDirectory(_sourceFolder, _destinationFolder);
                        output += _cmdlet + ": SUCCESS The folder \"" + _sourceFolder + "\" was copied to \"" + _destinationFolder + "\".\n";
                    }
                    else
                    {
                        output += _cmdlet + ": The folder \"" + _sourceFolder + "\" would have been copied to \"" + _destinationFolder + "\".\n";
                    }
                }
                else
                {
                    output += _cmdlet + ": ERROR Source folder \"" + _sourceFolder + "\" does not exist.\n";
                }
            }
            else
            {
                output +=_cmdlet + ": ERROR Missing required parameters.\n";
            }
            WriteObject(output);
        }
        private void CopyThisDirectory(string source, string dest)
        {
            if (!Directory.Exists(dest))
                Directory.CreateDirectory(dest);
            foreach (string sd in Directory.GetDirectories(source))
            {
                string td = dest.Substring(dest.LastIndexOf('\\'));
                CopyThisDirectory(sd, dest + td);
            }
            foreach (string sf in Directory.GetFiles(source))
            {
                string tf = dest.Substring(dest.LastIndexOf('\\'));
                try
                {
                    File.Copy(sf, dest + tf);
                }
                catch (IOException)
                {
                    // Copy to temp file and do the RunOnce key thing
                    DelayCopy(sf, dest + tf);
                }
            }
        }

        private void DelayCopy(string source, string dest)
        {
            string dd = dest.Substring(0, dest.LastIndexOf('\\'));
            string df = dest.Substring(dest.LastIndexOf('\\') + 1);
            string tfn = dest + ".tmp";

            WriteVerbose(_cmdlet + ": WARNING - '" + dest + "' IN USE, performing delayed copy using temp file and runonce key.");

            File.Copy(source, tfn);
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce", true);
            if (rk != null)
            {
                int cnt = rk.ValueCount + 1;
                rk.SetValue("Rename_" + cnt.ToString(), "cmd.exe /c rename \"" + tfn + "\" \"" + df + "\"");
            }
            WriteWarning(_cmdlet + ":  A file copy operation requires a RunOnce entry to complete.  A reboot is needed.");
        }
        protected override void EndProcessing()
        {

            base.EndProcessing();
        }

    }
}
