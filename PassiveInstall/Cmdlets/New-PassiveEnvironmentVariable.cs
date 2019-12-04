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

    [Cmdlet(VerbsCommon.New, "PassiveEnvironmentVariable", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class NewPassiveEnvironmentVariable : Cmdlet
    {
        private string _cmdlet = "New-PassiveEnvironmentVariable";

        #region ParameterDeclarations
        [Parameter(Mandatory = true,
                    Position = 0,
                    HelpMessage = "Environment Variable name."
                )]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        [Parameter(Mandatory = true,
                    Position = 1,
                    HelpMessage = "Environment Variable value."
                )]
        public string Value
        {
            get { return _Value; }
            set { _Value = value; }
        }
        #endregion

        #region ParameterStorage
        private string _Name = null;
        private string _Value = null;
        #endregion

        string output = "";
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            if (Name != null &&  _Value == null)
            {
                WriteVerbose(_cmdlet + ": Starting with Name=" + _Name + " and _Value=" + _Value);

                try
                {
                    if (this.ShouldProcess(_Name, "SetEnv"))
                    {
                        Environment.SetEnvironmentVariable(_Name, _Value);
                        output += _cmdlet + ": SUCCESS creating Environement Variable " + _Name + ".\n";
                    }
                    else
                    {
                        output += _cmdlet + ": Environement Variable " + _Name + " would have been created.\n";
                    }
                }
                catch (Exception ex)
                {
                    output +=  _cmdlet + ": ERROR " + ex.Message + "\n";
                }
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
