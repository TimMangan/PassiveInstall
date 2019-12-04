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

    [Cmdlet(VerbsLifecycle.Disable, "PassiveWindowsServices", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class PassiveDisableWindowsServices : Cmdlet
    {
        private string _cmdlet = "Disable-PassiveWindowsServices";

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
            if (_Names != null)
            {
                WriteVerbose(_cmdlet + ": Starting with " + _Names.Length.ToString() + "services to disable.");
                int Count = 0;
                try
                {
                    foreach (string name in _Names)
                    {
                        try
                        {
                            WriteVerbose(_cmdlet + ": Attempting to Disable " + name);
                            ServiceController service = new ServiceController(name);
                            if (service != null)
                            {
                                if (this.ShouldProcess(name, "DisableService"))
                                {
                                    ChangeStartType(service, ServiceStartMode.Disabled);
                                    output += _cmdlet + ": Service " + name + " disabled.\n";
                                }
                                else
                                {
                                    output += _cmdlet + ": Service " + name + " would have been disabled.\n";
                                }
                                Count++;
                            }
                            else
                            {
                                output += _cmdlet + ": Service " + name + " not found.\n";
                            }
                        }
                        catch (Exception ex)
                        {
                            output += _cmdlet + ": ERROR Service " + name + " could not be disabled. " + ex.Message + "\n";
                        }
                    }
                }
                catch (Exception ex)
                {
                    output += _cmdlet + ": ERROR " + ex.Message + "\n";
                }
                if (Count == _Names.Length)
                {
                    output += _cmdlet = ": SUCCESS disabling all " + Count.ToString() + " services." + "\n";
                }
                else
                {
                    output += _cmdlet = ": " + Count.ToString() + " services would have been disabled." + "\n";
                }
            }
            else
            {
                output += _cmdlet + ": ERROR Missing required parameters.\n";
            }
            WriteObject(output);
        }


        // Courtesy Michael Taylor: https://social.msdn.microsoft.com/Forums/vstudio/en-US/b8f8061f-b015-4527-869e-f4baabaa3313/changing-starttype-of-an-existing-service?forum=csharpgeneral
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ChangeServiceConfig(SafeHandle hService, uint dwServiceType, uint dwStartType,
                                                       uint dwErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, out uint lpdwTagId,
                                                       string lpDependencies, string lpServiceStartName, string lpPassword, string lpDisplayName);
        private const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;
        static void ChangeStartType(ServiceController svc, ServiceStartMode mode)
        {
            uint nTag;
            ChangeServiceConfig(svc.ServiceHandle, SERVICE_NO_CHANGE, (uint)mode, SERVICE_NO_CHANGE,
                                      null, null, out nTag, null, null, null, null);
        }


        protected override void EndProcessing()
        {
            base.EndProcessing();
        }

    }
}
