using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;
using System.IO;
//using System.Diagnostics;


namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    [Cmdlet(VerbsCommon.Remove, "PassiveFiles", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class RemovePassiveFiles : Cmdlet
    {
        private string _cmdlet = "Remove-PassiveFiles";

        #region ParameterDeclarations
        [Parameter(
            Mandatory = true,
            Position = 0,
            HelpMessage = "File or Files to be removed."
          )]
        public string[] Files
        {
            get { return _sourceFiles; }
            set { _sourceFiles = value; }
        }
        #endregion

        #region ParameterData
        private string[] _sourceFiles = null;
        #endregion

        string output = "";
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            if (_sourceFiles != null)
            {
                WriteVerbose(_cmdlet + ": Starting with " + _sourceFiles.Length.ToString() + " files to remove.");
                int Count = 0;
                foreach (string file in _sourceFiles)
                {
                    if (System.IO.File.Exists(file))
                    {
                        try
                        {
                            if (this.ShouldProcess(file, "Remove"))
                            {
                                WriteVerbose(_cmdlet + ": Removing " + file + "...");
                                System.IO.File.Delete(file);
                                output += _cmdlet + ": File \"" + file + "\" deleted.\n";
                            }
                            else
                            {
                                output += _cmdlet + ": File \"" + file + "\" would have been deleted.\n";
                            }
                            Count++;
                        }
                        catch (Exception ex)
                        {
                            output += _cmdlet + ": ERROR \"" +  file + "\" " + ex.Message + "\n";
                        }
                        // TODO
                    }
                    else
                    {
                        output += _cmdlet + ": ERROR File does not exist.\n";
                    }
                }
                if (Count == _sourceFiles.Length)
                    output += _cmdlet + ": SUCCESS in deleting " + Count.ToString() + " files.\n";
                else
                    output += _cmdlet + ": " + Count.ToString() + " files of " + _sourceFiles.Length.ToString() + " removed.\n";
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
