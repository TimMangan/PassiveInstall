using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Management.Automation;
using System.IO;
using System.Diagnostics;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    [Cmdlet(VerbsCommon.Optimize, "PassiveNgenQueues", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class OptimizePassiveNgenQueues : Cmdlet
    {
        private string _cmdlet = "Optimize-PassiveNgenQueues";


        #region ParameterDeclarations

        [Parameter(
                    Mandatory = false,
                    HelpMessage = "Optional switch to indicate that the compilation window should not be shown."
                )]
        public SwitchParameter DoSilent;

        [Parameter(
                    Mandatory = false,
                    HelpMessage = "Optional switch to indicate that the compilation window should be shown.  This is the default setting and need not be specified."
                )]
        public SwitchParameter DoPassive;
        #endregion

        #region ParameterStorage
        #endregion

        string output = "";

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            string Base = Environment.GetEnvironmentVariable("Windir") + "\\Microsoft.Net\\";
            string Frameworkx86 = "Framework\\";
            string Frameworkx64 = "Framework64\\";
            string v4 = "v4.0.30319\\";
            string v2 = "v2.0.50727\\";
            string ngen = "ngen.exe";

            WriteVerbose(_cmdlet + ": Starting...");
            int nErrors = 0;

            try
            {

                if (Environment.Is64BitOperatingSystem)
                {
                    if (File.Exists(Base + Frameworkx64 + v4 + ngen))
                    {
                        if (RunNgen(Base + Frameworkx64 + v4 + ngen))
                            nErrors++;
                    }
                    if (File.Exists(Base + Frameworkx64 + v2 + ngen))
                    {
                        if (RunNgen(Base + Frameworkx64 + v2 + ngen))
                            nErrors++;
                    }
                }
                if (File.Exists(Base + Frameworkx86 + v4 + ngen))
                {
                    if (RunNgen(Base + Frameworkx86 + v4 + ngen))
                        nErrors++;
                }
                if (File.Exists(Base + Frameworkx86 + v2 + ngen))
                {
                    if (RunNgen(Base + Frameworkx86 + v2 + ngen))
                        nErrors++;
                }
            }
            catch (Exception ex)
            {
                output += _cmdlet + ": ERROR " + ex.Message + "\n";
                nErrors++;
            }
            if (nErrors == 0)
            {
                output += _cmdlet + ": SUCCESS with ngen optimization.\n";
            }
            WriteObject(output);
        }

        private bool RunNgen(string ngen)
        {
            string args = "EXECUTEQUEUEDITEMS";
            bool bError = false; ;
            try
            {
                Process Proc = new Process();
                ProcessStartInfo StartInfo = new ProcessStartInfo();
                if (DoSilent.IsPresent)
                {
                    StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    StartInfo.FileName = ngen;
                    StartInfo.Arguments = args;
                }
                else
                {
                    WriteVerbose(_cmdlet + ": Compile using " + ngen);
                    StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    StartInfo.FileName = Environment.GetEnvironmentVariable("Windir") + "\\system32\\cmd.exe";
                    StartInfo.Arguments = "/c \"" + ngen + "\" " + args;
                }

                if (this.ShouldProcess(args, "ngen"))
                {
                    Proc.StartInfo = StartInfo;
                    Proc.Start();
                    Proc.WaitForExit();
                }
                else
                {
                    output += _cmdlet + ": ngen on " + args + " would have been compiled.\n";

                }
            }
            catch (Exception ex)
            {
                output += _cmdlet + ": ERROR on \"" + ngen + "\" " + ex.Message + "\n";
                bError = true; ;
            }
            return bError;
        }
        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}
