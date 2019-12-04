using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;
using System.IO;
using System.Runtime.InteropServices;

using PassiveInstall.Statics;
using Microsoft.Win32;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    [Cmdlet(VerbsCommon.Set, "PassiveServiceAccount", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class Set_PassiveServiceAccount : Cmdlet
    {
        private string _cmdlet = "Set-PassiveServiceAccount";

        #region ParameterDeclearations
        [Parameter(
            Mandatory = true,
            HelpMessage = "Name of the Windows Service."
          )]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [Parameter(
            Mandatory = false,
            HelpMessage = "New Logon Account for the Service."
          )]
        public string Account
        {
            get { return _account; }
            set { _account = value; }
        }
        #endregion

        #region ParameterData
        private string _name = null;
        private string _account = "LocalSystem";
        #endregion

        string output = "";

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            if (_name != null)
            {
                try
                {
                    WriteVerbose(_cmdlet + ": Starting with" + StaticClass.DoubleQuoteMe(_name) + " to set to use account " + StaticClass.DoubleQuoteMe(_account) + ".");
                    RegistryKey rk = Registry.LocalMachine;
                    RegistryKey tk = rk.OpenSubKey("SOFTWARE\\SYSTEM\\CurrentControlSet\\Services\\" + _name);
                    if (tk != null)
                    {
                        if (this.ShouldProcess(_name, "SetLogonAccount"))
                        {
                            tk.SetValue("ObjectName", _account);
                            output += _cmdlet + ": SUCCESS Service account for " + StaticClass.DoubleQuoteMe(_name) + " set to use account " + StaticClass.DoubleQuoteMe(_account) + ".\n";
                        }
                        else
                        {
                            output += _cmdlet + ": Service account for " + StaticClass.DoubleQuoteMe(_name) + " would havve been set to use account " + StaticClass.DoubleQuoteMe(_account) + ".\n";
                        }
                    }
                    else
                    {
                        output += _cmdlet + ": ERROR Unable to find the named service.\n";
                    }
                }
                catch (Exception ex)
                {
                    output += _cmdlet + ": ERROR Editing Service " + StaticClass.DoubleQuoteMe(_name) + " " + ex.Message + "\n";
                }
            }
            else
            {
                output += _cmdlet + ": ERROR Missing required parameter.\n";
            }
            WriteObject(output);
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }

    }
}
