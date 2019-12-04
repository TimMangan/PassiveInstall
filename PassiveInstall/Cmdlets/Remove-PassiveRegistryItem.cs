using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;
using System.IO;

using Microsoft.Win32;

using PassiveInstall.Statics;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    [Cmdlet(VerbsCommon.Remove, "PassiveRegistryItem", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class PassiveRegistryItem : Cmdlet
    {
        private string _cmdlet = "Remove-PassiveRegistryItem";

        #region ParameterDeclarations
        [Parameter(
            Mandatory = true,
            Position = 0,
            HelpMessage = "Registry Hive (ex: HKLM or HKCU)."
        )]
        public string Hive
        {
            get { return _hive; }
            set { _hive = value; }
        }

        [Parameter(
            Mandatory = true,
            Position = 1,
            HelpMessage = "Registry key or item."
        )]
        public string Item
        {
            get { return _item; }
            set { _item = value; }
        }
        #endregion

        #region ParameterData
        private string _hive = null;
        private string _item = null;
        #endregion

        string output = "";
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            if (_hive != null && _item != null)
            {
                WriteVerbose(_cmdlet + ": Starting with " + StaticClass.DoubleQuoteMe(_hive) + " and " + StaticClass.DoubleQuoteMe(_item));
                try
                {
                    RegistryKey rk;
                    switch (_hive.ToUpper())
                    {
                        case "HKCR":
                        case "HKEY_CLASSES_ROOT":
                            rk = Registry.ClassesRoot;
                            break;
                        case "HKEY_CURRENT_CONFIG":
                            rk = Registry.CurrentConfig;
                            break;
                        case "HKCU":
                        case "HKEY_CURRENT_USER":
                            rk = Registry.CurrentUser;
                            break;
                        case "HKLM":
                        case "HKEY_LOCAL_MACHINE":
                            rk = Registry.LocalMachine;
                            break;
                        case "HKEY_PERFORMANCE_DATA":
                            rk = Registry.PerformanceData;
                            break;
                        case "HKEY_USERS":
                            rk = Registry.Users;
                            break;
                        default:
                            throw new Exception("Invalid format to specify a registry hive, should be in form of HKLM or HKEY_LOCAL_MACHINE.");
                    }
                    bool success = false;
                    try
                    {
                        if (this.ShouldProcess(_item, "RemoveRegistry"))
                        {
                            rk.DeleteSubKeyTree(_item);
                            output += _cmdlet + ": SUCCESS Registry key " + StaticClass.DoubleQuoteMe(_item) + " removed.\n";
                        }
                        else
                        {
                            output += _cmdlet + ": Registry key " + StaticClass.DoubleQuoteMe(_item) + " would have been removed.\n";
                        }
                        success = true;
                    }
                    catch
                    {
                        // ignore
                    }

                    if (!success)
                    {
                        try
                        {
                            if (this.ShouldProcess(_item, "DeleteValue"))
                            {
                                rk.DeleteValue(_item);
                                output += _cmdlet + ": SUCCESS Registry item " + StaticClass.DoubleQuoteMe(_item) + " removed.\n";
                            }
                            else
                            {
                                output += _cmdlet + ": Registry item " + StaticClass.DoubleQuoteMe(_item) + " would have been removed.\n";
                            }
                            success = true;
                        }
                        catch
                        {
                            // also ignore
                        }
                    }

                    if (!success)
                        output += _cmdlet + ": ERROR Unable to delete from registry " + StaticClass.DoubleQuoteMe(_item) + ".\n";
                    else
                        WriteVerbose(_cmdlet + ": Ending with " + StaticClass.DoubleQuoteMe(_item));
                }
                catch (Exception ex)
                {
                    output += _cmdlet + ": ERROR " + ex.Message + "\n";
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
