using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;
using System.IO;
using System.Runtime.InteropServices;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    [Cmdlet(VerbsCommon.Set, "PassiveShortcutFixupSpaceAtEnd", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class Set_PassiveShortcutFixupSpace : Cmdlet
    {
        private string _cmdlet = "Set-PassiveShortcutFixupSpaceAtEnd";

        #region ParameterDeclearations
        [Parameter(
            Mandatory = true,
            HelpMessage = "Fulll path to the Shortcut lnk file to be fixed."
          )]
        public string Shortcut
        {
            get { return _linkPath; }
            set { _linkPath = value; }
        }
        #endregion

        #region ParameterData
        private string _linkPath = null;
        #endregion

        string output = "";
        bool needRefresh = false;

        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            if (_linkPath != null)
            {
                try
                {
                    WriteVerbose(_cmdlet + ": Starting with" + _linkPath + ".");
                    if (File.Exists(_linkPath))
                    {
                        string newname = _linkPath.Substring(0, _linkPath.Length - 5) + ".lnk";
                        if (this.ShouldProcess(_linkPath, "RenameShortcut"))
                        {
                            File.Replace(_linkPath, newname, newname + ".bak");
                            if (File.Exists(newname + ".bak"))
                                File.Delete(newname + ".bak");
                            output += _cmdlet + ": '" + _linkPath + "' renamed.";
                            needRefresh = true;
                        }
                        else
                        {
                            output += _cmdlet + ": '" + _linkPath + "' would have been renamed.";
                        }
                    }
                    else
                    {
                        output += _cmdlet + ": ERROR File \"" + _linkPath + "\" Does not exist.\n";
                    }
                }
                catch (Exception ex)
                {
                    output += _cmdlet + ": ERROR Editing \"" + _linkPath + "\" " + ex.Message + "\n";
                }
            }
            else
            {
                output += _cmdlet + ": ERROR Missing required parameter.\n";
            }
            WriteObject(output);
            if (needRefresh)
                SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero);
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }

    }
}
