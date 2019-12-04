using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;
using Microsoft.Win32;

using PassiveInstall.Statics;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    [Cmdlet(VerbsCommon.New, "PassiveRegistryKeyIfNotPresent", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class New_PassiveRegistryKeyIfNotPresent : Cmdlet
    {
        private string _cmdlet = "New-PassiveRegistryKeyIfNotPresent";


        #region ParameterDeclarations
        [Parameter(Mandatory = true,
                    Position = 0,
                    HelpMessage = "Registry hive, such as 'HKLM' or 'HKCU'"
                )]
        public string Hive
        {
            get { return _hive; }
            set { _hive = value; }
        }

        [Parameter(Mandatory = true,
                     Position = 0,
                     HelpMessage = "Registry key, such as 'Software\vendor\foo'"
                 )]
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }
        #endregion

        #region ParameterData
        private string _hive = null;
        private string _key = null;
        #endregion

        string output = "";

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            if (_hive == null ||
                _key == null)
            {
                WriteObject(_cmdlet + ": ERROR Missing required parameters.");
                return;
            }
            try
            {
                WriteVerbose(_cmdlet + ": Starting with hive=" + _hive + " and key=" + _key); 
                if (this.ShouldProcess(_key, "CreateKey"))
                {
                    StaticClass.MakeOrReturnKey(_hive, _key);
                    output += _cmdlet + ": SUCCESS Key \"\\\\" + _hive + "\\" + _key + "\" created or already exists.\n";
                }
                else
                {
                    output += _cmdlet + ": Key \"\\\\" + _hive + "\\" + _key + "\" would have been created or already exists.\n";
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
