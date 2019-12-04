using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Management.Automation;
using System.Diagnostics;

using PassiveInstall.Statics;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    // This cmdlet is the work horse of the installations.  It understannds many of the file types of files used for installation so that
    // you can call this with just the filename and any additional arguments needed.

    [Cmdlet (VerbsLifecycle.Install,"PassiveInstallFile", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class AddPassiveInstall : PSCmdlet
    {
        private string _cmdlet = "Install-PassiveInstallFile";


        #region ParameterDeclarations
        [Parameter(
            Mandatory = true,
            Position = 0,
            HelpMessage = "File to be installed. May be MSI,MSP,EXE,PS1,CMD,BAT,ZIP,REG"
          )] 
        [Alias("File")]
        public string Installer
        {
            get { return _Installer; }
            set { _Installer = value; }
        }

        [Parameter(
            Mandatory = false,
            Position = 1,
            HelpMessage = "Arguments or Destination depending on type of Installer."
        )]
        [Alias("Destination")]
        public string Arguments
        {
            get { return _args; }
            set { _args = value; }
        }


        [Parameter(
            Mandatory = false,
            HelpMessage = "Optional List of allowed error codes. If not specified, values 0 and 3010 are accepted."
        )]
        public int[] AllowedErrorCodes
        {
            get { return _allowedErrorCodes; }
            set { _allowedErrorCodes = value; }
        }


        [Parameter(
                    Mandatory = false,
                    HelpMessage = "Optional switch to indicate that the compilation window should not be shown."
                )]
        public SwitchParameter DoSilent;


        [Parameter(
                    Mandatory = false,
                    HelpMessage = "Optional switch to indicate that the compilation window should be shown. This is the default setting and need not be specified."
                )]
        public SwitchParameter DoPassive;
        #endregion

        #region ParameterData
        private string _Installer = null;
        private string _args = "";
        private int[] _allowedErrorCodes = null;
        #endregion

        private bool bIsVerboseSet = false;

        string output = "";

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }
        protected override void ProcessRecord()
        {

            bIsVerboseSet = this.MyInvocation.Line.ToLower().Contains("-verbose");

                
            if (_Installer != null )
            {
                if (_args != null && _args.Length > 0)
                    output += _cmdlet + ": Starting " + _Installer + " with arguments " + _args + ".\n";
                else
                    output += _cmdlet + ": Starting " + _Installer + ".\n";

                Process Proc = new Process();
                ProcessStartInfo StartInfo = new ProcessStartInfo();
                string keywithoutquotes = StaticClass.RemoveQuotes(_Installer);
                string filetype = keywithoutquotes.Substring(keywithoutquotes.LastIndexOf('.'));
                switch (filetype.ToUpper())
                {
                    case ".MSI":
                        ProcessThis("msiexec.exe",
                                    "/i \"" + keywithoutquotes + "\" " + _args,
                                    "Installing");
                        break;
                    case ".MSP":
                        ProcessThis("msiexec.exe",
                                    "/update \"" + keywithoutquotes + "\" " + _args,
                                    "Updating");
                        break;
                    case ".EXE":
                        ProcessThis("\"" + keywithoutquotes + "\"",
                                     _args,
                                    "Executing");
                        break;
                    case ".CMD":
                    case ".BAT":
                        ProcessThis("\"C:\\Windows\\System32\\cmd.exe\"",
                                    "/c \"" + keywithoutquotes + "\" " + _args,
                                    "Processing");
                        break;
                    case ".ZIP":
                        if (_args.Length > 0)
                        {
                            try
                            {
                                if (this.ShouldProcess(keywithoutquotes, "Unzip"))
                                {
                                    System.IO.Compression.ZipFile.ExtractToDirectory(keywithoutquotes, _args);
                                    output += _cmdlet + ": SUCCESS extracting \"" + keywithoutquotes + "\".\n";
                                }
                                else
                                {
                                    output += _cmdlet + ": \"" + keywithoutquotes + "\" would have been unzipped.\n";
                                }
                            }
                            catch (Exception ex)
                            {
                                output += _cmdlet + ": ERROR Decompressing \"" + keywithoutquotes + "\": " + ex.Message + "\n";
                            }
                        }
                        else
                        {
                            output += _cmdlet + ": ERROR To install a Zip file you must supply the destination folder.\n";
                        }
                        break;
                    case ".PS1":
                        if (_args.Length > 0)
                        {
                            string AdjustedArguments = _args;
                            if (bIsVerboseSet)
                                AdjustedArguments += " -Verbose";

                            ProcessThis("PowerShell.exe",
                                         "-NoProfile  -ExecutionPolicy ByPass -File \"" + keywithoutquotes + "\" " + AdjustedArguments,
                                        "Executing");
                        }
                        else
                        {
                            string AdjustedArguments = " ";
                            if (bIsVerboseSet)
                                AdjustedArguments += " -Verbose";
                            ProcessThis("PowerShell.exe",
                                         "-NoProfile  -ExecutionPolicy ByPass -File \"" + keywithoutquotes + "\"" + AdjustedArguments,
                                        "Executing");                           
                        }
                        break;
                    case ".REG":
                        ProcessThis("C:\\Windows\\System32\\reg.exe",
                                     " IMPORT \"" + keywithoutquotes + "\"",
                                    "Processing");
                        break;
                    default:
                        output += _cmdlet + ": ERROR Installer is not a recognized Installer File Type. It must be one of .MSI .MSP .EXE .PS1 .CMD .BAT .ZIP or .REG\n";
                        break;
                }
                output += _cmdlet + ": Done.\n";
            }
            else
            {
                output += _cmdlet + ": ERROR Missing required parameters.\n";
            }
            WriteObject(output);
        }

        private void ProcessThis(string Filename, string Arguments, string ActiveAction)
        {
            
            Process Proc = new Process();
            ProcessStartInfo StartInfo = new ProcessStartInfo();
            try
            {

                if (DoSilent.IsPresent)
                {
                    StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }
                else
                {
                    StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                }
                StartInfo.FileName = Filename;
                StartInfo.Arguments = Arguments;
                StartInfo.RedirectStandardError = true;
                StartInfo.RedirectStandardOutput = true;
                StartInfo.UseShellExecute = false;
                Proc.StartInfo = StartInfo;
                WriteVerbose(_cmdlet + ":\tActual command will be: " + Filename + " " + StartInfo.Arguments);
                if (this.ShouldProcess(Filename, "Install"))
                {
                    Proc.Start();
                    Proc.WaitForExit();

                    System.IO.StreamReader sr_so = Proc.StandardOutput;
                    bool doneS = false;
                    while (!doneS)
                    {
                        try
                        {
                            string s = sr_so.ReadLine();
                            if (s != null)
                            {
                                output += _cmdlet + ":\t" + s.TrimEnd('\n') + "\n";
                            }
                            else
                                doneS = true;
                        }
                        catch { doneS = true; }
                    }
                    sr_so.Close();

                    System.IO.StreamReader sr_se = Proc.StandardError;
                    bool doneE = false;
                    bool postedE = false;
                    while (!doneE)
                    {
                        try
                        {
                            string s = sr_se.ReadLine();
                            if (s != null)
                            {
                                if (!s.StartsWith("The operation completed successfully."))
                                {
                                    if (postedE || s.Length > 5)           // Reg, for example, returns a line with CR in stderr.
                                    {
                                        if (!postedE)
                                        {
                                            postedE = true;
                                            output += _cmdlet + ": NOTE: These errors are displayed out of sequence of the information messages!\n";
                                        }
                                        output += _cmdlet + ":\tERROR " + s.TrimEnd('\n') + "\n";
                                    }
                                }
                            }
                            else
                                doneE = true;
                        }
                        catch { doneE = true; }
                    }
                    sr_se.Close();

                    if (_allowedErrorCodes != null)
                    {
                        bool allowed = false;
                        foreach (int err in _allowedErrorCodes)
                        {
                            if (err == Proc.ExitCode)
                            {
                                allowed = true;
                                break;
                            }
                        }
                        if (allowed)
                        {
                            output += _cmdlet + ": SUCCESS " + ActiveAction + " of \"" + _Installer + "\" with arguments \"" + _args + "\".\n";
                        }
                        else
                        {
                            output += _cmdlet + ": ERROR code " + Proc.ExitCode.ToString() + " " + ActiveAction + " \"" + _Installer + "\".\n";
                        }
                    }
                    else
                    {
                        switch (Proc.ExitCode)
                        {
                            case 0:
                            case 3010:
                                output += _cmdlet + ": SUCCESS " + ActiveAction + " of \"" + _Installer + "\" with arguments \"" + _args + "\".\n";
                                break;
                            default:
                                output += _cmdlet + ": ERROR code " + Proc.ExitCode.ToString("x") + " while " + ActiveAction + " \"" + _Installer + "\".\n";
                                break;
                        }
                    }
                }
                else
                {
                    output += _cmdlet + ": INSTALL would have been done '" + Filename + "' " + Arguments + "\n";
                }
            }
            catch (Exception ex)
            {
                WriteObject(_cmdlet + ": ERROR " + ActiveAction + " \"" + _Installer + "\": " + ex.Message);
            }
        }

        protected override void EndProcessing()
        {

            base.EndProcessing();
        }
    }

}
