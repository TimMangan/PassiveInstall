using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;

using Microsoft.Win32;
namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    [Cmdlet(VerbsCommon.New, "PassiveAppPathSearch", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class New_PassiveAppPathSearch : PSCmdlet
    {
        // The OS has a registration mechanism for executables that serves several purposes:
        //  1) It provides a quick "search" capability for an executable in the windows start screen/cortona search.
        //     This registration allows the executable to be found by name without necessarily typing in the full name.
        //  2) It allows an app to add its own directories to the search path used when the program loads dlls without altering the path variable.
        //  This cmdlet allows you to create the first one.
        //  By default, it will add an entry for the name of the executable as the search term, but optionally you can specify an alias instead.
        private string _cmdlet = "New-PassiveAppPathSearch";

        #region ParameterDeclarations
        [Parameter(
            Mandatory = true,
            Position = 0,
            HelpMessage = "Path to the executable."
          )]
        public string Target
        {
            get { return _target; }
            set { _target = value; }
        }

        [Parameter(
            Mandatory = false,
            Position = 1,
            HelpMessage = "Optional alias search term."
          )]
        public string Alias
        {
            get { return _alias; }
            set { _alias = value; }
        }

        [Parameter(
            Mandatory = false,
            Position = 2,
            HelpMessage = "Optional AppPath string for pre-Path variable dll loading."
          )]
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }
        #endregion

        #region ParameterData
        private string _target = null;
        private string _alias = null;
        private string _path = null;
        #endregion

        string output = "";

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            if (_target == null)
            {
                output += _cmdlet + ":  ERROR A required parameter (Target) is missing.\n";
            }
            else
            {
                if (_alias == null)
                    WriteVerbose(_cmdlet + ": Starting with " + _target);
                else
                    WriteVerbose(_cmdlet + ": Starting with " + _target + " as " + _alias);
                try
                {
                    if (this.ShouldProcess(_target, "SetAppPathSearch"))
                    {
                        RegistryKey Kbase = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths", true);
                        string search;
                        if (_alias != null)
                            search = _alias;
                        else
                            search = _target.Substring(Target.LastIndexOf('\\') + 1);
                        bool found = false;
                        string[] subkeynames = Kbase.GetSubKeyNames();
                        if (subkeynames != null)
                        {
                            foreach (string subkeyname in subkeynames)
                            {
                                if (subkeyname.ToUpper().Equals(search.ToUpper()))
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                        RegistryKey KSearch;
                        if (found)
                        {
                            // ?? Maybe just updated the default
                            KSearch = Kbase.OpenSubKey(search, true);
                        }
                        else
                        {
                            KSearch = Kbase.CreateSubKey(search);
                        }
                        KSearch.SetValue("", _target); // Verify this

                        if (_path != null)
                        {
                            KSearch.SetValue("Path", _path);
                        }
                        else
                        {
                            KSearch.SetValue("Path", search);
                        }

                        output += _cmdlet + ": SUCCESS key " + search + " default and path values set.\n";
                    }
                    else
                    {
                        output += _cmdlet + ": AppPath for " + _target + " with alias " + _alias + " would have been registered.\n";
                    }
                }
                catch (Exception ex)
                {
                    output += _cmdlet + ": ERROR " + ex.Message + "\n";
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
