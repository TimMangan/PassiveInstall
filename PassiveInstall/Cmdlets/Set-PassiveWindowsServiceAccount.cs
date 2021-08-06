using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    /************************************************************************************************************************************
    [Cmdlet(VerbsCommon.Set, "PassiveWindowsServiceAccount")]
    public class PassiveSetWindowsServiceAccount : Cmdlet
    {
        private string _cmdlet = "Set-PassiveWindowsServiceAccount";

        #region ParameterDeclarations
        [Parameter(Mandatory = true,
                    ValueFromPipelineByPropertyName = true,
                    Position = 0,
                    HelpMessage = "Windows Service Name or Names."
                )]
        public string[] Names
        {
            get { return _Names; }
            set { _Names = value; }
        }

        [Parameter(Mandatory = true,
                    ValueFromPipelineByPropertyName = false,
                    Position = 1,
                    HelpMessage = "Account Name."
                )]
        public string Account
        {
            get { return _AccountName; }
            set { _AccountName = value; }
        }

        #endregion

        #region ParameterData
        private string[] _Names = null;
        private string _AccountName = null;
        #endregion

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            if (_Names != null)
            {
                WriteVerbose(_cmdlet + ": Starting with " + _Names.Length.ToString() + "services to change to use account " + _AccountName + ".");
                int Count = 0;
                try
                {
                    foreach (string name in _Names)
                    {
                        try
                        {
                            WriteVerbose(_cmdlet + ":     Attempting to Set " + name);
                            ServiceController service = new ServiceController(name);
                            if (service != null)
                            {
                                ChangeServiceAccount(service, _AccountName);
                                WriteObject(_cmdlet + ":     Service " + name + " set.");
                                Count++;
                            }
                            else
                            {
                                WriteObject(_cmdlet + ":     Service " + name + " not found.");
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteObject(_cmdlet + ":     ERROR Service " + name + " could not be disabled. " + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteObject(_cmdlet + ": ERROR " + ex.Message);
                }
                if (Count == _Names.Length)
                    WriteObject(_cmdlet = ": SUCCESS disabling all " + Count.ToString() + " services.");
            }
            else
            {
                WriteObject(_cmdlet + ": ERROR Missing required parameters.");
            }
        }


        // Courtesy Michael Taylor: https://social.msdn.microsoft.com/Forums/vstudio/en-US/b8f8061f-b015-4527-869e-f4baabaa3313/changing-starttype-of-an-existing-service?forum=csharpgeneral
        [DllImport("Advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ChangeServiceConfig(SafeHandle hService, uint dwServiceType, uint dwStartType,
                                                       uint dwErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, out uint lpdwTagId,
                                                       string lpDependencies, string lpServiceStartName, string lpPassword, string lpDisplayName);
        private const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;
        static void ChangeServiceAccount(ServiceController svc, string AccountName)
        {
            uint nTag;
            ChangeServiceConfig(svc.ServiceHandle, SERVICE_NO_CHANGE, SERVICE_NO_CHANGE, SERVICE_NO_CHANGE,
                                      null, null, out nTag, null, AccountName, null, null);
        }


        protected override void EndProcessing()
        {
            base.EndProcessing();
        }

    }
    ***********************************************************************************************************************************************************/
}
