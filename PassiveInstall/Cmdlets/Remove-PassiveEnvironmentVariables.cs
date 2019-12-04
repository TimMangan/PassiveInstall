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

    [Cmdlet(VerbsCommon.Remove, "PassiveEnvironmentVariables", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class RemovePassiveEnvironmentVariables : Cmdlet
    {
        private string _cmdlet = "Remove-PassiveEnvironmentVariables";

        #region ParameterDeclarations
        [Parameter(Mandatory = true,
                    ValueFromPipelineByPropertyName = true,
                    Position = 0,
                    HelpMessage = "One or more Environment Variable names."
                )]
        public string[] Names
        {
            get { return _Names; }
            set { _Names = value; }
        }
        #endregion

        #region ParameterData
        private string[] _Names = null;
        #endregion

        string output = "";
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            int Count = 0;
            if (_Names != null)
            {
                WriteVerbose(_cmdlet + ": Starting with " + _Names.Length.ToString() + " variables.");
                try
                {
                    foreach (string name in _Names)
                    {
                        try
                        {
                            string s = Environment.GetEnvironmentVariable(name);
                            if (s != null && s.Length > 0)
                            {
                                if (this.ShouldProcess(name, "Remove"))
                                {
                                    WriteVerbose(_cmdlet + ": Removing " + name);
                                    Environment.SetEnvironmentVariable(name, null);  // or ""?
                                    output += _cmdlet + ": Environment Variable " + name + " removed.\n";
                                }
                                else
                                {
                                    output += _cmdlet + ": Environment Variable " + name + " would have been removed.\n";
                                }
                                Count++;
                            }
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    output += _cmdlet + ": ERROR " + ex.Message + "\n";
                }
                if (Count == _Names.Length)
                    output += _cmdlet + ": SUCCESS in deleting " + Count.ToString() + " variables.\n";
                else
                    output += _cmdlet + ": " + Count.ToString() + " of " + _Names.Length.ToString() + " requested Environment variables removed.\n";
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
