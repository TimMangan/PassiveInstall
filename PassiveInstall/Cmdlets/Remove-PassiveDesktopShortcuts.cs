using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Management.Automation;
using System.IO;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    [Cmdlet(VerbsCommon.Remove, "PassiveDesktopShortcuts", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class RemovePassiveDesktopShortcuts : Cmdlet
    {
        private string _cmdlet = "Remove-PassiveDesktopShortcuts";

        #region ParameterDeclarations
        [Parameter(
            Mandatory = true,
            Position = 0,
            HelpMessage = "Shortcut name(s) to be removed from the desktop. Do not include folders; the \".lnk\" is optional."
          )]
        [Alias("Files")]
        public string[] Names
        {
            get { return _sourceNames; }
            set { _sourceNames = value; }
        }


        #endregion

        #region ParameterData
        private string[] _sourceNames = null;
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

            if (_sourceNames != null)
            {
                output += _cmdlet + ": Starting with " + _sourceNames.Length.ToString() + " shortcut";
                if (_sourceNames.Length > 1)
                    output += "s.\n";
                else
                    output += ".\n";
                
                
                foreach (string sourceName in _sourceNames)
                {
                    try
                    {
                        string tryname = sourceName;
                        string trynameNoLnkExt;
                        if (!sourceName.ToUpper().EndsWith(".LNK"))
                        {
                            tryname += ".lnk";
                            trynameNoLnkExt = sourceName;
                        }
                        else
                        {
                            trynameNoLnkExt = sourceName.Substring(0, sourceName.LastIndexOf('.'));
                        }
                        WriteVerbose(_cmdlet + ": Looking for '" + tryname + "' or '" + trynameNoLnkExt + "'.");

                        // Look on user, all user's, and default desktops, remove if present, ignore otherwise.
                        // Note that sometimes the lnk extension is not on the filename!
                        
                        string UserDesktopName         = "C:\\Users\\" + Environment.UserName + "\\Desktop\\" + tryname;
                        string UserDesktopNameNoLnkExt = "C:\\Users\\" + Environment.UserName + "\\Desktop\\" + trynameNoLnkExt;
                        if (File.Exists(UserDesktopName))
                        {
                            if (this.ShouldProcess(UserDesktopName,"Remove"))
                            {
                                File.Delete(UserDesktopName);
                                output += _cmdlet + ": " + UserDesktopName + " deleted.\n";
                                needRefresh = true;
                            }
                            else
                            {
                                output += _cmdlet + ": " + UserDesktopName + " would have been deleted.\n";
                            }
                        }
                        else if (File.Exists(UserDesktopNameNoLnkExt))
                        {
                            if (this.ShouldProcess(UserDesktopNameNoLnkExt, "Remove"))
                            {
                                File.Delete(UserDesktopNameNoLnkExt);
                                output += _cmdlet + ": " + UserDesktopNameNoLnkExt + " deleted.\n";
                            }
                            else
                            {
                                output += _cmdlet + ": " + UserDesktopNameNoLnkExt + " would have been deleted.\n";
                            }
                        }

                        string PublicDesktopName         = "C:\\Users\\Public\\Desktop\\" + tryname;
                        string PublicDesktopNameNoLnkExt = "C:\\Users\\Public\\Desktop\\" + trynameNoLnkExt; 
                        if (File.Exists(PublicDesktopName))
                        {
                            if (this.ShouldProcess(PublicDesktopName, "Remove"))
                            {
                                File.Delete(PublicDesktopName);
                                output += _cmdlet + ": " + PublicDesktopName + " deleted.\n";
                                needRefresh = true;
                            }
                            else 
                            {
                                output += _cmdlet + ": " + PublicDesktopName + " would have been deleted.\n";
                            }
                        }
                        else if (File.Exists(PublicDesktopNameNoLnkExt))
                        {
                            if (this.ShouldProcess(PublicDesktopNameNoLnkExt, "Remove"))
                            {
                                File.Delete(PublicDesktopNameNoLnkExt);
                                output += _cmdlet + ": " + PublicDesktopNameNoLnkExt + " deleted.\n";
                                needRefresh = true;
                            }
                            else
                            {
                                output += _cmdlet + ": " + PublicDesktopNameNoLnkExt + " would have been deleted.\n";
                            }
                        }

                        string DefaultDesktopName         = "C:\\Users\\Default\\Desktop\\" + tryname;
                        string DefaultDesktopNameNoLnkExt = "C:\\Users\\Default\\Desktop\\" + trynameNoLnkExt;
                        if (File.Exists(DefaultDesktopName))
                        {
                            if (this.ShouldProcess(DefaultDesktopName, "Remove"))
                            {
                                File.Delete(DefaultDesktopName);
                                output += _cmdlet + ": " + DefaultDesktopName + " deleted.\n";
                                needRefresh = true;
                            }
                            else
                            {
                                output += _cmdlet + ": " + DefaultDesktopName + " would have been deleted.\n";
                            }
                        }
                        else if (File.Exists(DefaultDesktopNameNoLnkExt))
                        {
                            if (this.ShouldProcess(DefaultDesktopNameNoLnkExt, "Remove"))
                            {
                                File.Delete(DefaultDesktopNameNoLnkExt);
                                output += _cmdlet + ": " + DefaultDesktopNameNoLnkExt + " deleted.\n";
                                needRefresh = true;
                            }
                            else 
                            {
                                output += _cmdlet + ": " + DefaultDesktopNameNoLnkExt + " would have been deleted.\n";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        output += _cmdlet + ": ERROR on \"" + sourceName + "\" " + ex.Message + "\n";
                    }
                }
            }
            else
            {
                output += _cmdlet + ": ERROR Missing required parameters.\n";
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
