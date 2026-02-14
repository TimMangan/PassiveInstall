using Microsoft.Win32;
using PassiveInstall.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2026 TMurgent Technologies, LLP

    [Cmdlet(VerbsCommon.Get, "PassiveFirewallRules", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class Get_PassiveFirewallRules : Cmdlet
    {
        private string _cmdlet = "Get-PassiveFirewallRules";

        #region ParameterDeclarations        
        // None
        #endregion

        #region ParameterData
        #endregion

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            FirewallRules output = new FirewallRules();

            string sDefinitions = "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Defaults\\FirewallPolicy\\FirewallRules";
            string sParameters = "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\FirewallRules";

            RegistryKey KbaseDefinitions = Registry.LocalMachine.OpenSubKey(sDefinitions, false);
            if (KbaseDefinitions != null)
            {
                string[] subValueNames = KbaseDefinitions.GetValueNames();
                if (subValueNames != null)
                {
                    foreach (string subValueName in subValueNames)
                    {
                        if (subValueName.Length > 0)
                        {
                            string valueData = KbaseDefinitions.GetValue(subValueName) as string;
                            output.AddToDefault(subValueName, valueData);
                        }
                    }
                }
            }

            RegistryKey KbaseParameters = Registry.LocalMachine.OpenSubKey(sParameters, false);
            if (KbaseParameters != null)
            {
                string[] subValueNames = KbaseParameters.GetValueNames();
                if (subValueNames != null)
                {
                    foreach (string subValueName in subValueNames)
                    {
                        if (subValueName.Length > 0)
                        {
                            string valueData = KbaseParameters.GetValue(subValueName) as string;
                            output.AddToParameter(subValueName, valueData);
                        }
                    }
                }
            }
            WriteVerbose(_cmdlet + ": " + output.DefaultRules.Count().ToString() + " Definitions and " +
                                          output.ParameterRules.Count().ToString() + " Parameters captured.");
            WriteObject(output);
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }

    }
}
