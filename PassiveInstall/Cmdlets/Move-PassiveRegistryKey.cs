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

    [Cmdlet(VerbsCommon.Move, "PassiveRegistryKey", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class Move_PassiveRegistryKey : Cmdlet
    {
        private string _cmdlet = "Move-PassiveRegistryKey";

        #region ParameterDeclarations
        [Parameter(
                    Mandatory = true,
                    Position = 0,
                    HelpMessage = "Hive that the key is being moved from (i.e. 'HKLM' or 'HKCU')."
         )]
        public string FromHive
        {
            get { return _fromHive; }
            set { _fromHive = value; }
        }

        [Parameter(
                    Mandatory = true,
                    Position = 1,
                    HelpMessage = "Key (without hive) that is moved."
         )]
        public string ParentKey
        {
            get { return _parentKey; }
            set { _parentKey = value; }
        }

        [Parameter(
                    Mandatory = true,
                    Position = 2,
                    HelpMessage = "Key (without hive) that is used as the new name in the new hive (often same as ParentKey)."
         )]
        public string RelativeKey
        {
            get { return _relativeKey; }
            set { _relativeKey = value; }
        }

        [Parameter(
                    Mandatory = true,
                    Position = 3,
                    HelpMessage = "Hive that the key is being moved to (i.e. 'HKLM' or 'HKCU')."
         )]
        public string ToHive
        {
            get { return _toHive; }
            set { _toHive = value; }
        }

        #endregion

        #region ParameterData
        string _fromHive = null;
        string _parentKey = null;
        string _relativeKey = null;
        string _toHive = null;
        #endregion

        string output = "";

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            RegistryKey from;
            RegistryKey to;

            WriteVerbose(_cmdlet + ": Starting from=" + _fromHive + "\\" + _parentKey + " to=" + _toHive + "\\" + _relativeKey);

            try
            {
                from = GetKey(_fromHive, _parentKey);
                to = StaticClass.MakeOrReturnKey(_toHive, _relativeKey);
                if (to != null && from != null)
                {
                    if (this.ShouldProcess(_parentKey, "MoveKey"))
                    {
                        MoveKey(from, to);
                        output += _cmdlet + ": SUCCESS Registry key move completed.\n";
                    }
                    else
                    {
                        output += _cmdlet + ": The Registry key " + _parentKey + " would have been moved.\n";
                    }
                }
            }
            catch (Exception ex)
            {
                output += _cmdlet + ": ERROR " + ex.Message + "\n";
            }
            WriteObject(output);
        }

        private RegistryKey GetKey(string hive, string skey)
        {
            RegistryKey rk;
            switch (hive.ToUpper())
            {
                case "HKCR":
                    rk = Registry.ClassesRoot.OpenSubKey(skey);
                    break;
                case "HKCC":
                    rk = Registry.CurrentConfig.OpenSubKey(skey); ;
                    break;
                case "HKCU":
                    rk = Registry.CurrentUser.OpenSubKey(skey); ;
                    break;
                //case "HKDD":
                //    rk = Registry.DynData.OpenSubKey(skey); ;
                //    break;
                case "HKLM":
                    rk = Registry.LocalMachine.OpenSubKey(skey); ;
                    break;
                case "HKPD":
                    rk = Registry.PerformanceData.OpenSubKey(skey); ;
                    break;
                case "HKUS":
                    rk = Registry.Users.OpenSubKey(skey); ;
                    break;
                default:
                    throw new Exception("Invalid format to specify a registry hive.");
            }
            return rk;
        }

        /********************************************************************************************
        private RegistryKey MakeKey(string hive, string skey)
        {
            RegistryKey rk;
            switch (hive.ToUpper())
            {
                case "HKCR":
                    rk = Registry.ClassesRoot;
                    break;
                case "HKCC":
                    rk = Registry.CurrentConfig;
                    break;
                case "HKCU":
                    rk = Registry.CurrentUser;
                    break;
                //case "HKDD":
                //    rk = Registry.DynData;
                //    break;
                case "HKLM":
                    rk = Registry.LocalMachine;
                    break;
                case "HKPD":
                    rk = Registry.PerformanceData;
                    break;
                case "HKUS":
                    rk = Registry.Users;
                    break;
                default:
                    throw new Exception("Invalid format to specify a registry hive.");
            }
            string[] splits;
            if (skey.Contains('/'))
                splits = skey.Split('/');
            else if (skey.Contains('\\'))
                splits = skey.Split('\\');
            else
            {
                splits = new string[1];
                splits[0] = skey;
            }
            foreach (string split in splits)
            {
                RegistryKey rkchild = null;
                try
                {
                    rkchild = rk.OpenSubKey(split, true);
                    rk = rkchild;
                }
                catch
                {
                    rkchild = rk.CreateSubKey(split);
                    rk = rkchild;
                }
            }
            return rk;
        }

        private RegistryKey MakeKey(RegistryKey parent, string name)
        {
            RegistryKey rchild;
            try
            {
                rchild = parent.OpenSubKey(name, true);
                return rchild;
            }
            catch
            {
                rchild = parent.CreateSubKey(name);
                return rchild;
            }
        }
        *******************************************************************************/
        private void MoveKey(RegistryKey from, RegistryKey to)
        {
            foreach (string skname in from.GetSubKeyNames())
            {
                RegistryKey childkeyfrom = from.OpenSubKey(skname, false);
                RegistryKey childkeyto = StaticClass.MakeOrReturnKey(to, skname);
                MoveKey(childkeyfrom, childkeyto);
            }
            foreach (string skvaluename in from.GetValueNames())
            {
                object skvaluevalue = from.GetValue(skvaluename);
                to.SetValue(skvaluename, skvaluevalue);
            }

            foreach (string skname in from.GetSubKeyNames())
                from.DeleteSubKeyTree(skname);
            foreach (string skvaluename in from.GetValueNames())
                from.DeleteValue(skvaluename);
         }

        private void CopyKey(RegistryKey from, RegistryKey to)
        {
            foreach (string skname in from.GetSubKeyNames())
            {
                RegistryKey childkeyfrom = from.OpenSubKey(skname, false); 
                RegistryKey childkeyto = StaticClass.MakeOrReturnKey(to, skname);
                CopyKey(childkeyfrom, childkeyto);
            }
            foreach (string skvaluename in from.GetValueNames())
            {
                object skvaluevalue = from.GetValue(skvaluename);
                to.SetValue(skvaluename, skvaluevalue);
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}
