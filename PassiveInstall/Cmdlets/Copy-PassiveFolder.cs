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

        [Parameter(
            Mandatory = false,
            Position = 2,
            HelpMessage = "A switch to requrest overwriting existing files.  If file is in use a temporary copy is made and runonce key added."
          )]
        public SwitchParameter Overwrite;
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
#if LOCALDEBUG
                WriteVerbose(_cmdlet + ": Starting source=" + _sourceFolder + " Destination=" + _destinationFolder);
#endif

                // Determine if source folder exists
                if (Directory.Exists(_sourceFolder))
                {
                    // Source was an entire folder to copy
                    if (this.ShouldProcess(_destinationFolder, "CopyDirectory"))
                    {
                        if (CopyThisDirectory(_sourceFolder, _destinationFolder, 1))
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
        private bool CopyThisDirectory(string source, string dest,int level)
        {
            string destMinusSlash;
            string sourceMinusSlash;
            string lastfolderlevel;
#if LOCALDEBUG
            WriteVerbose(_cmdlet + "(" + level.ToString() + "): CopyThisDirectory " + source + " to " + dest);
#endif
            destMinusSlash = dest;
            if (dest.EndsWith("\\"))
                destMinusSlash = destMinusSlash.Substring(0, destMinusSlash.Length - 1);
            sourceMinusSlash = source;
            if (source.EndsWith("\\"))
                sourceMinusSlash = sourceMinusSlash.Substring(0, sourceMinusSlash.Length - 1);
            lastfolderlevel = sourceMinusSlash;
            if (lastfolderlevel.Contains('\\'))
            {
                lastfolderlevel = lastfolderlevel.Substring(lastfolderlevel.LastIndexOf('\\')+1);
            }
#if LOCALDEBUG
            WriteVerbose(_cmdlet + "(" + level.ToString() + "): initial string manipulation done");
#endif

            if (!Directory.Exists(destMinusSlash))
            {
                try
                {
                    WriteVerbose(_cmdlet + "(" + level.ToString() + "): Create Directory " + destMinusSlash);
                    Directory.CreateDirectory(destMinusSlash);
                }
                catch (IOException ex)
                {
                    output += _cmdlet + ": ERR: " + ex.Message;
                    return false;
                }
            }
#if LOCALDEBUG
            WriteVerbose(_cmdlet + "(" + level.ToString() + "): past base dircreate");
#endif

            if (!Directory.Exists(destMinusSlash + "\\" + lastfolderlevel))
            {
                try
                {
                    WriteVerbose(_cmdlet + "(" + level.ToString() + "): Create Directory " + destMinusSlash + "\\" + lastfolderlevel);
                    Directory.CreateDirectory(destMinusSlash + "\\" + lastfolderlevel);
                }
                catch (IOException ex)
                {
                    output += _cmdlet + ": ERR: " + ex.Message;
                    return false;
                }
            }
#if LOCALDEBUG
            WriteVerbose(_cmdlet + "(" + level.ToString() + "): past dir createcreations");
#endif


            foreach (string sd in Directory.GetDirectories(source))
            {
#if LOCALDEBUG
                WriteVerbose(_cmdlet + "(" + level.ToString() + "): subdir " + sd);
#endif
                try
                {
                    string sdMinus = sd;
                    if (sdMinus.EndsWith("\\"))
                        sdMinus = sd.Substring(0, sd.Length - 1);

                    //string td = sdMinus.Substring(sdMinus.LastIndexOf('\\'));
                    WriteVerbose(_cmdlet + "(" + level.ToString() + "): Copy Directory " + sdMinus + " to " + destMinusSlash + "\\" + lastfolderlevel);
                    CopyThisDirectory(sdMinus, destMinusSlash + "\\" + lastfolderlevel, level+1);
                }
                catch (IOException ex)
                {
                    output += _cmdlet + "(" + level.ToString() + "): ERROR: " + ex.Message;
                    return false;
                }
            }
#if LOCALDEBUG
            WriteVerbose(_cmdlet + "(" + level.ToString() + "): past subdirs");
#endif

            foreach (string sf in Directory.GetFiles(source))
            {
#if LOCALDEBUG
                WriteVerbose(_cmdlet + "(" + level.ToString() + "): subfile " + sf);
#endif
                string tfn = sf.Substring(sf.LastIndexOf('\\'));
                string td = dest + "\\" + lastfolderlevel + "\\" + tfn;
                //if (!td.StartsWith("\\\\?\\"))
                //        td = "\\\\?\\" + td;
                try
                {
                    WriteVerbose(_cmdlet + "(" + level.ToString() + "): Copy file " + sf + " to " + td);  
                    File.Copy(sf,td, (Overwrite.IsPresent == true));
                }               
                catch (IOException ex)
                {
                    if (Overwrite.IsPresent)
                    {
                        // Copy to temp file and do the RunOnce key thing
                        DelayCopyFile(sf, td);
                    }
                    else
                    {
                        output += _cmdlet + "(" + level.ToString() + "): ERROR: " + ex.Message;
                    }
                }
            }
#if LOCALDEBUG
            WriteVerbose(_cmdlet + "(" + level.ToString() + "): past filecopy");
#endif
            return true;
        }

        
        private void DelayCopyFile(string source, string dest)
        {
            string dd = dest.Substring(0, dest.LastIndexOf('\\'));
            string df = dest.Substring(dest.LastIndexOf('\\') + 1);
            string tfn = dest + ".tmp";

            output += _cmdlet + ": WARNING - '" + dest + "' IN USE, performing delayed copy using temp file and runonce key.";

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
