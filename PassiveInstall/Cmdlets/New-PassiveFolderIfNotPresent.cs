using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;

using PassiveInstall.Statics;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    [Cmdlet(VerbsCommon.New, "PassiveFolderIfNotPresent", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class New_PassiveFolderIfNotPresent : Cmdlet
    {
        private string _cmdlet = "New-PassiveFolderIfNotPresent";


        #region ParameterDeclarations
        [Parameter(Mandatory = true,
                    Position = 0,
                    HelpMessage = "Full path to a folder to be created, if it does not currently exist."
                )]
        [Alias("Directory")]
        public string Folder
        {
            get { return _folder; }
            set { _folder = value; }
        }
        #endregion

        #region ParameterData
        private string _folder = null;
        #endregion

        string output = "";

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            if (_folder == null)
            {
                WriteObject(_cmdlet + ": ERROR Missing required parameters.");
                return;
            }
            WriteVerbose(_cmdlet + ": Starting with " + _folder);
            try
            {
                if (this.ShouldProcess(_folder, "CreateFolder"))
                {
                    bool b = StaticClass.MakeDirectoryIfNotExists(_folder);
                    if (b)
                        output += _cmdlet + ": IGNORED as directory already exists.\n";
                    else
                        output += _cmdlet + ": SUCCESS creating Folder \"" + _folder + "\".\n";
                }
                else
                {
                    output += _cmdlet + ": Folder \"" + _folder + "\" would have been created, if not present.\n";
                }
            }
            catch (Exception ex)
            {
                output += _cmdlet + ": ERROR " + ex.Message + "\n";
            }

            WriteObject(output);
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}
