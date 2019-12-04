using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32;
using System.IO;

using System.Security.Principal;

namespace PassiveInstall.Statics
{
    public static partial class StaticClass
    {
        public static string RemoveQuotes(string input)
        {
            if (input.Length > 2 &&
                input.StartsWith("\"") &&
                input.EndsWith("\""))
                return input.Substring(1, input.Length - 2);
            return input;
        }

        public static RegistryKey MakeOrReturnKey(string hive, string skey)
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
                    if (rkchild == null)
                        rkchild = rk.CreateSubKey(split);
                    rk = rkchild;
                }
                catch //(Exception ex)
                {
                    rkchild = rk.CreateSubKey(split);
                    rk = rkchild;
                }
            }
            return rk;
        }

        public static RegistryKey MakeOrReturnKey(RegistryKey parent, string name)
        {
            RegistryKey rchild;
            try
            {
                rchild = parent.OpenSubKey(name, true);
                if (rchild == null)
                    rchild = parent.CreateSubKey(name);
                return rchild;
            }
            catch
            {
                rchild = parent.CreateSubKey(name);
                return rchild;
            }
        }

        public static bool MakeDirectoryIfNotExists(string name)
        {
            if (!Directory.Exists(name))
                Directory.CreateDirectory(name);
            else
                return false;
            return true;
        }

        public static bool IsElevated
        {
            get
            {
                return new WindowsPrincipal
                    (WindowsIdentity.GetCurrent()).IsInRole
                    (WindowsBuiltInRole.Administrator);
            }
        }


        public static string DoubleQuoteMe(string input)
        {
            return "\"" + input + "\"";
        }
        public static string SingleQuoteMe(string input)
        {
            return "'" + input + "'";
        }
    }
}
