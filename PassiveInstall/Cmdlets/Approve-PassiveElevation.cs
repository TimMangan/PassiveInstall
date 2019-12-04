using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Management.Automation;
using System.Diagnostics;
using System.Security.Principal;
using System.Reflection;

using PassiveInstall.Statics;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    // Often, even as an administrator, you need to start powershell with a "Run As Administrator" to be able to perform the actions
    // that you need to do.  By adding a call to this cmdlet at the beginning of your powershell script, you ensure that the script
    // will run elevated.
    // The cmdlet checks to see if you are elevated, and if not starts a new powershell window that is elevated and restarts your script.
    // This makes it possible to test your scripts by just right clicking on the PS1 file and selecteding Run with Powershell.
    [Cmdlet(VerbsLifecycle.Approve, "PassiveElevation", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class ApprovePassiveElevation : PSCmdlet
    {

        private string _cmdlet = "Approve-PassiveElevation";

        #region ParameterDeclarations
        [Parameter(
            Mandatory = false,
            Position = 0,
            HelpMessage = "Switch to request elevation (if needed)."
          )]
        public SwitchParameter AsAdmin;
        #endregion

        #region ParamaterData
        ///private bool bIsVerboseSet = false;
        #endregion

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            bool el = false;
            try
            {
                ///bIsVerboseSet = this.MyInvocation.Line.ToLower().Contains("-verbose");
                if (AsAdmin.IsPresent)
                    WriteVerbose(_cmdlet + ": Starting with argument -AsAdmin.");
                else
                    WriteVerbose(_cmdlet + ": Starting");

                el = StaticClass.IsElevated;
                WriteVerbose(_cmdlet + ": Initial elevation = " + el.ToString());
                if (AsAdmin.IsPresent && el == false)
                {
                    WriteVerbose(_cmdlet + ": Need to elevate");

                    string OrigCmdLine = Environment.CommandLine; //Assembly.GetEntryAssembly().Location
                    string[] OrigArgsArr = Environment.GetCommandLineArgs();
                    string OrigWorkingDirectory = Environment.CurrentDirectory;
                    string args = "";

                    bool first = true;
                    foreach (string arg in OrigArgsArr)
                    {

                        if (first)
                        {
                            first = false; // ignore the command name
                            OrigCmdLine = arg;  // removes quotes around it
                        }
                        else
                        {
                            ///WriteVerbose(_cmdlet + ": ARG=" + arg);
                            args += " " + arg;
                        }
                    }

                    WriteVerbose(_cmdlet + ": Original Command line = " + OrigCmdLine);
                    WriteVerbose(_cmdlet + ": Original Command Arguments = " + args);
                    WriteVerbose(_cmdlet + ": Original Working Directory = " + OrigWorkingDirectory);

                    var info = new ProcessStartInfo(
                         OrigCmdLine,
                         args)
                    {
                        Verb = "runas", // indicates to elevate privileges
                        WorkingDirectory = OrigWorkingDirectory
                    };

                    var process = new Process
                    {
                        EnableRaisingEvents = true, // enable WaitForExit()
                        StartInfo = info
                    };

                    if (this.ShouldProcess(OrigCmdLine, "RunAs"))
                    {
                        process.Start();
                        WriteVerbose(_cmdlet + ": Elevated Process Launched.");
                        process.WaitForExit(); // sleep calling process thread until evoked process exit
                        if (process.ExitCode != 0)
                            WriteVerbose(_cmdlet + ": Error code returned from new process = " + process.ExitCode.ToString("X"));
                    }
                    else
                    {
                        WriteObject(_cmdlet + ": Re-launching the script wirh RunAs would have been performed.");
                    }
                    Environment.Exit(0);
                }
                //WriteObject(el);
            }
            catch (Exception ex)
            {
                WriteObject(_cmdlet + ": ERROR " + "Elevation failure " + ex.Message);
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }


    }
}