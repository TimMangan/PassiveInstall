using Microsoft.Win32;
using PassiveInstall.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2026 TMurgent Technologies, LLP

    [Cmdlet(VerbsCommon.Redo, "PassiveNewFirewallRules", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class Redo_PassiveNewFirewallRules : Cmdlet
    {
        private string _cmdlet = "Redo-PassiveNewFirewallRules";

        #region ParameterDeclarations        
        [Parameter(
                    Mandatory = true,
                    Position = 0,
                    HelpMessage = "Initial list from Get-PassiveFirewallRules."
         )]
        public FirewallRules InitialList
        {
            get { return _initialList; }
            set { _initialList = value; }
        }

        [Parameter(
                    Mandatory = true,
                    Position = 1,
                    HelpMessage = "Updated list from Get-PassiveFirewallRules."
         )]
        public FirewallRules NewList
        {
            get { return _newList; }
            set { _newList = value; }
        }
        #endregion

        #region ParameterData
        public FirewallRules _initialList;
        public FirewallRules _newList;
        #endregion

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            // We are only worrying about newly added rules, not if a rule changed value
            string sDefinitions = "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Defaults\\FirewallPolicy\\FirewallRules";
            string sParameters = "SYSTEM\\CurrentControlSet\\Services\\SharedAccess\\Parameters\\FirewallPolicy\\FirewallRules";

            FirewallRules output = new FirewallRules();

            foreach (FirewallRule nRule in _newList.DefaultRules)
            {
                bool FoundInOldList = false;
                foreach (FirewallRule iRule in _initialList.DefaultRules)
                {
                    if (iRule.Name.Equals(nRule.Name))
                    {
                        FoundInOldList = true;
                        break;
                    }
                }
                if (! FoundInOldList)
                {
                    string key = sDefinitions;
                    string name = nRule.Name;
                    string value = nRule.Value;
                    RegistryKey kBase = Registry.LocalMachine.OpenSubKey(key, true);
                    if (kBase != null)
                    {
                        kBase.SetValue(name, value);
                        output.AddToDefault(name, value);
                        WriteVerbose(_cmdlet + ": Updated Definitions rule \'" + name + "\'");
                    }
                    else
                    {
                        InformationRecord r = new InformationRecord("Bad key: Definitions", name);
                        WriteInformation(r);
                    }
                }
            }

            foreach (FirewallRule nRule in NewList.ParameterRules)
            {
                bool FoundInOldList = false;
                foreach (FirewallRule iRule in InitialList.ParameterRules)
                {
                    if (iRule.Name.Equals(nRule.Name))
                    {
                        FoundInOldList = true;
                        break;
                    }
                }
                if (!FoundInOldList)
                {
                    string key = sParameters;
                    string name = nRule.Name;
                    string value = nRule.Value;
                    RegistryKey kBase = Registry.LocalMachine.OpenSubKey(key, true);
                    if (kBase != null)
                    {
                        kBase.SetValue(name, value);
                        output.AddToDefault(name, value);
                        WriteVerbose(_cmdlet + ": Updated Parameters rule \'" + name + "\'");
                    }
                    else
                    {
                        InformationRecord r = new InformationRecord("Bad key: Parameters", name);
                        WriteInformation(r);
                    }
                }
            }

            WriteObject(output);
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }       

    }
}
