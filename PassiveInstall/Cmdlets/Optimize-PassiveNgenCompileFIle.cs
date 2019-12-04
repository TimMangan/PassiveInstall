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

    [Cmdlet(VerbsCommon.Optimize, "PassiveNgenCompileFile", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class OptimizePassiveNgenCompileFile : Cmdlet
    {
        private string _cmdlet = "Optimize-PassiveNgenCompileFile";


        #region ParameterDeclarations
        [Parameter(
                    Mandatory = true,
                    HelpMessage = "Indicates that the file is 64-bit and requires the 64-bit ngen compiler.")]
        public Boolean Is64Bit
        {
            get { return _Is64Bit; }
            set { _Is64Bit = value; }
        }

        [Parameter(
                    Mandatory = true,
                    HelpMessage = "String indicating which version to compile with.")]

        public string VersionString
        {
            get { return _VersionString; }
            set { _VersionString = value; }
        }

        [Parameter(
                    Mandatory = true,
                    HelpMessage = "The executable elemet to be natively compiled.")]
        public string FileName
        {
            get { return _FileName; }
            set { _FileName = value; }
        }

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
        public Boolean _Is64Bit;
        public string _VersionString;
        public string _FileName;
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
            string v4 = "v4.0.30319";
            string v2 = "v2.0.50727";

            WriteVerbose(_cmdlet + ": Starting...");
            int nErrors = 0;
            string ngen;
            try
            {
                //NB: On windows 7 there is a v2 ngen.exe and v4, but on 10 there is only a v4.
                if (_Is64Bit)
                {
                    ngen = Base + Frameworkx64 + _VersionString + "\\ngen.exe";
                    if (File.Exists(ngen))
                        nErrors = NgenCompile(ngen, _FileName);
                    else
                    {
                        if (_VersionString.Equals(v2))
                        {
                            ngen = Base + Frameworkx64 + v4 + "\\ngen.exe";
                            nErrors = NgenCompile(ngen, _FileName);
                        }
                        else
                            nErrors++;
                    }
                }
                else
                {
                    ngen = Base + Frameworkx86 + _VersionString + "\\ngen.exe";
                    if (File.Exists(ngen))
                        nErrors = NgenCompile(ngen, _FileName);
                    else
                    {
                        if (_VersionString.Equals(v2))
                        {
                            ngen = Base + Frameworkx86 + v4 + "\\ngen.exe";
                            nErrors = NgenCompile(ngen, _FileName);
                        }
                        else
                            nErrors++;
                    }
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

        private int NgenCompile(string ngen , string args)
        {
            int bError = 0;
            try
            {
                Process Proc = new Process();
                ProcessStartInfo StartInfo = new ProcessStartInfo();
                if (DoSilent.IsPresent)
                {
                    StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    StartInfo.FileName = ngen;
                    StartInfo.Arguments = "/silent install \"" + args + "\"";
                }
                else
                {
                    WriteVerbose(_cmdlet + ": Compile using " + ngen);
                    StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    StartInfo.FileName = ngen;
                    StartInfo.Arguments = "install \"" + args + "\"";
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
                bError = 1;
            }
            return bError;
        }
        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}
