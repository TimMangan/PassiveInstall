using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;
using System.IO;
using System.Runtime.InteropServices;


using IWshRuntimeLibrary;
using Shell32;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    [Cmdlet(VerbsCommon.Set, "PassiveShortcutFixupReg", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class Set_PassiveShortcutFixupReg : Cmdlet
    {
        private string _cmdlet = "Set-PassiveShortcutFixupReg";

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
                    if (System.IO.File.Exists(_linkPath))
                    {
                        try
                        {
                            if (this.ShouldProcess(_linkPath, "SetShortcut"))
                            {
                                WshShellClass wshShell = new WshShellClass();
                                IWshRuntimeLibrary.IWshShortcut lnk = (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(_linkPath);  // depends on this openening the existing shortcut!
                                string savetarget = lnk.TargetPath;
                                WriteVerbose(_cmdlet + ": Original targetpath " + Statics.StaticClass.DoubleQuoteMe(savetarget));
                                lnk.TargetPath = "C:\\Windows\\System32\\reg.exe";
                                lnk.Arguments = "/import " + savetarget + " " + lnk.Arguments;
                                lnk.Save();
                                output += _cmdlet + ": SUCCESS Shortcut \"" + _linkPath + "\" updated.\n";
                                needRefresh = true;
                            }
                            else
                            {
                                output += _cmdlet + ":  Shortcut \"" + _linkPath + "\" would have been updated.\n";
                            }
                        }
                        catch (Exception ex)
                        {
                            output += _cmdlet + ": ERROR Editing \"" + _linkPath + "\" " + ex.Message + "\n";
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
