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
            HelpMessage = "Shortcut name(s) to be removed from the desktop. Do not include folders; the \".lnk\" or \".url\" is optional."
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
                        string trynameAddLnkExt = null;
                        string trynameAddUrlExt = null; ;
                        if (!sourceName.ToUpper().EndsWith(".LNK") &&
                            !sourceName.ToUpper().EndsWith(".URL"))
                        {
                            trynameAddLnkExt = tryname + ".LNK";
                            trynameAddUrlExt = tryname + ".URL";
                        }
                        
                        WriteVerbose(_cmdlet + ": Looking for '" + tryname + "'.");

                        // Look on user, all user's, and default desktops, remove if present, ignore otherwise.
                        // Note that sometimes the lnk or url extension is not on the filename!
                        output += ProcessThis("C:\\Users\\" + Environment.UserName + "\\Desktop", tryname, trynameAddLnkExt, trynameAddUrlExt);

                        output += ProcessThis("C:\\Users\\Public\\Desktop", tryname, trynameAddLnkExt, trynameAddUrlExt);

                        output += ProcessThis("C:\\Users\\Default\\Desktop", tryname, trynameAddLnkExt, trynameAddUrlExt);

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

        private string ProcessThis(string BaseFolder, string tryname, string trynameAddLnkExt, string trynameAddUrlExt)
        {
            string output = "";

            string DesktopName = BaseFolder + "\\" + tryname;
            string DesktopNameAddLnkExt = BaseFolder + "\\" + trynameAddLnkExt;
            string DesktopNameAddUrlExt = BaseFolder + "\\" + trynameAddUrlExt;

            if (File.Exists(DesktopName))
            {
                if (this.ShouldProcess(DesktopName, "Remove"))
                {
                    try
                    {
                        File.Delete(DesktopName);
                        output += _cmdlet + ": " + DesktopName + " deleted.\n";
                        needRefresh = true;
                    }
                    catch (Exception ex)
                    {
                        output += _cmdlet + ": Exception deleting " + DesktopName + ". " + ex.Message;
                    }
                }
                else
                {
                    output += _cmdlet + ": " + DesktopName + " would have been deleted.\n";
                }
            }
            if (trynameAddLnkExt != null && File.Exists(DesktopNameAddLnkExt))
            {
                if (this.ShouldProcess(DesktopNameAddLnkExt, "Remove"))
                {
                    try
                    {
                        File.Delete(DesktopNameAddLnkExt);
                        output += _cmdlet + ": " + DesktopNameAddLnkExt + " deleted.\n";
                        needRefresh = true;
                    }
                    catch (Exception ex)
                    {
                        output += _cmdlet + ": Exception deleting " + DesktopNameAddLnkExt + ". " + ex.Message;
                    }
                }
                else
                {
                    output += _cmdlet + ": " + DesktopNameAddLnkExt + " would have been deleted.\n";
                }
            }
            if (trynameAddUrlExt != null && File.Exists(DesktopNameAddUrlExt))
            {
                if (this.ShouldProcess(DesktopNameAddUrlExt, "Remove"))
                {
                    try
                    {
                        File.Delete(DesktopNameAddUrlExt);
                        output += _cmdlet + ": " + DesktopNameAddUrlExt + " deleted.\n";
                        needRefresh = true;
                    }
                    catch (Exception ex)
                    {
                        output += _cmdlet + ": Exception deleting " + DesktopNameAddUrlExt + ". " + ex.Message;
                    }
                }
                else
                {
                    output += _cmdlet + ": " + DesktopNameAddUrlExt + " would have been deleted.\n";
                }
            }
            return output;
        }
        protected override void EndProcessing()
        {
            base.EndProcessing();
        }

    }
}
