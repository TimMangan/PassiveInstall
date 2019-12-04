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

    [Cmdlet(VerbsCommon.Remove, "PassiveStartMenuShortcuts", ConfirmImpact = ConfirmImpact.Medium,SupportsShouldProcess = true)]    
    public class RemovePassiveStartMenuShortcuts : Cmdlet
    {
       
        private string _cmdlet = "Remove-PassiveStartMenuShortcuts";
        
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
           
            int Count = 0;
            if (_sourceNames != null)
            {
                WriteVerbose(_cmdlet + ": Starting with " + _sourceNames.Length.ToString() + " shortcuts to remove.");
                foreach (string sourceName in _sourceNames)
                {
                    try
                    {
                        string tryname = sourceName;
                        if (!sourceName.ToUpper().EndsWith(".LNK"))
                            tryname += ".lnk";
                        WriteVerbose(_cmdlet + ": Looking for " + tryname);
                        // look on user, and all user's;  remove if present, ignore otherwise.
                        string UserStartMenuName = "C:\\Users\\" + Environment.UserName + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\";
                        Count += SearchForAndDeleteName(UserStartMenuName, tryname);

                        string SystemStartMenuName = Environment.GetEnvironmentVariable("ALLUSERSPROFILE") + "\\Microsoft\\Windows\\Start Menu\\";  
                        Count += SearchForAndDeleteName(SystemStartMenuName, tryname);

                        output += _cmdlet + ": " + Count.ToString() + " shortcuts with name " + tryname + " deleted.\n";

                    }
                    catch (Exception ex)
                    {
                        output += _cmdlet + ": ERROR \"" + sourceName + "\" " + ex.Message + "\n";
                    }
                }
                if (Count >= _sourceNames.Length)
                    output += _cmdlet + ": SUCCESS deleting " + Count.ToString() + " shortcuts.\n";
                else
                    output += _cmdlet + ": " + Count.ToString() + " deletions.\n";
            }
            else
            {
                output += _cmdlet + ": ERROR Missing required parameters.\n";
            }
            WriteObject(output);
            if (needRefresh)
                SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero);
        }

        private int SearchForAndDeleteName(string BaseFolder, string FileName)
        {
            int rCount = 0; ;
            try
            {
                if (Directory.Exists(BaseFolder))
                {
                    foreach (string subdir in Directory.GetDirectories(BaseFolder))
                    {
                        int subCount = SearchForAndDeleteName(subdir, FileName);
                        rCount += subCount;
                    }
                    foreach (string file in Directory.EnumerateFiles(BaseFolder))
                    {
                        if (file.ToUpper().EndsWith("\\" + FileName.ToUpper()))
                        {
                            try
                            {
                                if (this.ShouldProcess(file, "Remove"))
                                {
                                    File.Delete(file);
                                    output +=_cmdlet + ": \"" + file + "\" deleted.\n";
                                    needRefresh = true;
                                }
                                else
                                {
                                    output += _cmdlet + ": \"" + file + "\" would have been deleted.\n";
                                }
                                rCount += 1;
                            }
                            catch (Exception ex)
                            {
                                output += _cmdlet + ": ERROR \"" + file + "\" " + ex.Message + "\n";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                output +=_cmdlet + ": ERROR \"" + FileName + "\" " + ex.Message + "\n";
            }
            return rCount;
        }
        protected override void EndProcessing()
        {
            base.EndProcessing();
        }

    }
}
