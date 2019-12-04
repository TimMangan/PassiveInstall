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

    [Cmdlet(VerbsCommon.Remove, "PassiveFolders", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class Remove_PassiveFolders : Cmdlet
    {
        private string _cmdlet = "Remove-PassiveFolders";

        #region ParameterDeclarations
        [Parameter(
            Mandatory = true,
            Position = 0,
            HelpMessage = "Folder(s) to be removed."
        )]
        public string[] Folders
        {
            get { return _sourceFolders; }
            set { _sourceFolders = value; }
        }
        #endregion

        #region ParameterData
        private string[] _sourceFolders = null;
        #endregion

        string output = "";

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            if (_sourceFolders != null)
            {
                WriteVerbose(_cmdlet + ": Starting with list of " + _sourceFolders.Length.ToString() + " folders.");
                int Count = 0;
                foreach (string sourceFolder in _sourceFolders)
                {
                    WriteVerbose(_cmdlet + ": looking for " + sourceFolder);
                    if (System.IO.Directory.Exists(sourceFolder))
                    {
                        try
                        {
                            if (this.ShouldProcess(sourceFolder, "Remove"))
                            {
                                System.IO.Directory.Delete(sourceFolder, true);
                                output += _cmdlet + ": Folder \"" + sourceFolder + "\" deleted.\n";
                            }
                            else
                            {
                                output += _cmdlet + ": Folder \"" + sourceFolder + "\" would have been deleted.\n";
                            }
                            Count++;
                        }
                        catch (Exception ex)
                        {
                            output += _cmdlet + ": ERROR \"" + sourceFolder + "\" " + ex.Message + "\n";
                        }
                    }
                    else
                    {
                        output += _cmdlet + ": ERROR Folder \"" + sourceFolder + "\" does not exist.\n";
                    }
                }
                if (Count == _sourceFolders.Length)
                    output += _cmdlet + ": SUCCESS deleing " + Count.ToString() + " folders.\n";
                else
                    output += _cmdlet + ": " + Count.ToString() + " folders of " + _sourceFolders.Length.ToString() + " removed.\n";
            }
            else
            {
                output += _cmdlet + ": ERROR Missing required parameters.\n";
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}
