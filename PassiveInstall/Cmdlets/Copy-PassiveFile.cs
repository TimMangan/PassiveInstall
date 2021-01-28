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

    // Used to copy a file, optionally changing the name.  It will ensure that the destination folder exists.
    [Cmdlet(VerbsCommon.Copy, "PassiveFile", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class CopyPassiveFile : Cmdlet
    {
        private string _cmdlet = "Copy-PassiveFile";

        #region ParameterDeclarations
        [Parameter(
            Mandatory = true,
            Position = 0,
            HelpMessage = "File to be installed/copied."
          )]
        [Alias("File")]
        public string Source
        {
            get { return _source; }
            set { _source = value; }
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

        [Parameter(
            Mandatory = false,
            Position = 2,
            HelpMessage = "Optional replacement for the filename when copied."
          )]
        public string NewName
        {
            get { return _renamedFilename; }
            set { _renamedFilename = value; }
        }

        [Parameter(
            Mandatory = false,
            Position = 3,
            HelpMessage = "Switch to disable overwrite if file exists. Cmdlet defaults to overwriting without it."
            )]
        public SwitchParameter NoOverWrite;
        #endregion

        #region ParameterData
        private string _source = null;
        private string _destinationFolder = null;
        private string _renamedFilename = null;
        #endregion

        string output = "";

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {

            if (_source != null && _destinationFolder != null)
            {
                if (_renamedFilename != null)
                    WriteVerbose(_cmdlet + ": Starting source=" + _source + " Destination=" + _destinationFolder + " Rename=" + _renamedFilename);
                else
                    WriteVerbose(_cmdlet + ": Starting source=" + _source + " Destination=" + _destinationFolder);
                        
                // Determine if source is a file or folder
                if (File.Exists(_source))
                {
                    // Determine is destination file exists.
                    string fulldestname = _destinationFolder;
                    if (!fulldestname.EndsWith("\\"))
                        fulldestname += "\\";
                    if (_renamedFilename != null && _renamedFilename.Length > 0)
                    {
                        fulldestname += _renamedFilename;
                    }
                    else
                    {
                        int i = _source.LastIndexOf('\\');
                        if (i < 0)
                            i = 0;
                        else
                            i += 1;
                        fulldestname += _source.Substring(i);
                    }
                    if (!Directory.Exists(_destinationFolder))
                    {
                        if (this.ShouldProcess(_destinationFolder, "CreateDirectory"))
                        {
                            Directory.CreateDirectory(_destinationFolder);
                            output += _cmdlet + ": CreateDirectory on '" + _destinationFolder + "'\n";
                        }
                        else
                        {
                            output += _cmdlet + ": CreateDirectory on '" + _destinationFolder + "' would have been performed.\n";
                        }
                    }
                    if (NoOverWrite.IsPresent)
                    {
                        if (File.Exists(fulldestname))
                        {
                            output += _cmdlet + ": WARNING file \"" + fulldestname + "\" already exists, copy not performed by request.\n";
                            WriteObject(output);
                            return;
                        }
                    }
                    try
                    {
                        if (this.ShouldProcess(fulldestname, "CopyTo"))
                        {
                            File.Copy(_source, fulldestname, true);
                            output += _cmdlet + ": SUCCESS File \"" + _source + "\" copied to \"" + fulldestname + "\".\n";
                        }
                        else
                        {
                            output += _cmdlet + ": File \"" + _source + "\" would have been copied to \"" + fulldestname + "\".\n";
                        }
                    }
                    catch (IOException)
                    {
                        // Was in use, do the temp and runonce thing...
                        if (this.ShouldProcess(fulldestname, "CopyTo via RunOnce"))
                        {
                            DelayCopy(_source, fulldestname);
                        }
                        else
                        {
                            output += _cmdlet + ": File \"" + _source + "\" would have been copied to \"" + fulldestname + "\" using a 'RunOnce'.\n";
                        }
                    }
                }
                else
                {
                    output += _cmdlet + ": ERROR Source file \"" + _source + "\" does not exist.\n";
                }
            }
            else
            {
                output += _cmdlet + ": ERROR Missing required parameters.\n";
            }
            WriteObject(output);
        }

        private void DelayCopy(string source, string dest)
        {
            WriteVerbose(_cmdlet + ": WARNING - '" + dest + "' IN USE, performing delayed copy using temp file and runonce key.");

            string dd = dest.Substring(0, dest.LastIndexOf('\\'));
            string df = dest.Substring(dest.LastIndexOf('\\') + 1);
            string tfn = dest + ".tmp";
            File.Copy(source, tfn);
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce",true);
            if (rk != null)
            {
                int cnt = rk.ValueCount + 1;
                rk.SetValue("Rename_" + cnt.ToString(), "cmd.exe /c rename \"" + tfn + "\" \"" + df + "\"");
            }
            //WriteObject(_cmdlet + ": A file copy operation requires a RunOnce entry to complete.  A reboot is needed");
            WriteWarning(_cmdlet + ":  A file copy operation requires a RunOnce entry to complete.  A reboot is needed.");
        }
        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}
