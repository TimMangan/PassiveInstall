using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;
using System.IO;

using PassiveInstall.Statics;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    [Cmdlet(VerbsCommon.Remove, "PassiveInstallerCacheFolders", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class PassiveInstallerCacheFolder : Cmdlet
    {
        private string _cmdlet = "Remove-PassiveInstallerCacheFolders";

        #region ParameterDeclarations
        [Parameter(
            Mandatory = true,
            Position = 0,
            HelpMessage = "Folder(s) to check."
        )]
        public List<string> Folders
        {
            get { return _sourceFolderList; }
            set { _sourceFolderList = value; }
        }

        [Parameter(
            Mandatory = false,
            Position = 1,
            HelpMessage = "Optional Switch to check if empty first"
        )]
        public SwitchParameter IfEmpty;

        [Parameter(
            Mandatory = false,
            Position = 1,
            HelpMessage = "Optional Switch to check if created in last 24 hours"
        )]
        public SwitchParameter IfToday;

        #endregion

        #region ParameterData
        private List<string> _sourceFolderList = null;
        #endregion

        string output = "";

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            if (_sourceFolderList != null)
            {
                WriteVerbose(_cmdlet + ": Starting.");
                int Count = 0;
                int Errors = 0;
                foreach (string sourceFolder in _sourceFolderList)
                { 
                    if (System.IO.Directory.Exists(sourceFolder))
                    {
                        try
                        {
                            bool skip = false;
                            if (IfEmpty.IsPresent)
                            {
                                if (System.IO.Directory.EnumerateDirectories(sourceFolder).Count() != 0 ||
                                    System.IO.Directory.EnumerateFiles(sourceFolder).Count()       != 0   )
                                {
                                    WriteVerbose(_cmdlet + ":\tSkip folder because not empty " + StaticClass.DoubleQuoteMe(sourceFolder) + ".");
                                    skip = true;
                                }
                            }
                            if (IfToday.IsPresent)
                            {
                                if ((DateTime.Now - System.IO.File.GetCreationTime(sourceFolder)) >= new TimeSpan(24, 0, 0))
                                {
                                    WriteVerbose(_cmdlet + ":\tSkip folder because too old " + StaticClass.DoubleQuoteMe(sourceFolder) + ".");
                                    skip = true;
                                }
                            }
                            if (!skip)
                            {
                                if (this.ShouldProcess(sourceFolder, "Cleanup"))
                                {
                                    WriteVerbose(_cmdlet + ":\tdeleting Folder " + StaticClass.DoubleQuoteMe(sourceFolder) + ".");
                                    Directory.Delete(sourceFolder);
                                    output += _cmdlet + ":\tFolder " + StaticClass.DoubleQuoteMe(sourceFolder) + " deleted.\n";
                                }
                                else
                                {
                                    output += _cmdlet + ":\tFolder " + StaticClass.DoubleQuoteMe(sourceFolder) + " would have been deleted.\n";
                                }
                                Count++;
                            }
                        }
                        catch (Exception ex)
                        {
                            output += _cmdlet + ": ERROR \"" + sourceFolder + "\" " + ex.Message + "\n";
                            Errors++;
                        }
                    }
                    else
                    {
                        WriteVerbose(_cmdlet + ":\tFolder " + StaticClass.DoubleQuoteMe(sourceFolder) + " does not exist.");
                    }
                }
                if (Errors == 0)
                    output += _cmdlet + ": SUCCESS deleting " + Count.ToString() + " files.\n";
                else
                    output += _cmdlet + ": ERROR " + Count.ToString() + " files removed but " + Errors.ToString() + " errors while processing.\n";
            }
            else
            {
                output += _cmdlet + ": ERROR Missing required parameters.\n";
            }
            WriteObject(output);
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}
