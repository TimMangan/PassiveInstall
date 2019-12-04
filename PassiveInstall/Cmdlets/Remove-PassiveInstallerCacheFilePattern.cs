using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;
using System.IO;

using PassiveInstall.Statics;
using System.Text.RegularExpressions;

using System.ComponentModel;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    [Cmdlet(VerbsCommon.Remove, "PassiveInstallerCacheFilePattern", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class PassiveInstallerCacheFilePattern : Cmdlet
    {
        private string _cmdlet = "Remove-PassiveInstallerCacheFilePattern";

        #region ParameterDeclarations
        [Parameter(
            Mandatory = true,
            Position = 0,
            HelpMessage = "Folder to check."
        )]
        public string Folder
        {
            get { return _sourceFolder; }
            set { _sourceFolder = value; }
        }

        [Parameter(
            Mandatory = false,
            Position = 1,
            HelpMessage = "Optional string to pattern match, such as \"SourceHash*\"."
        )]
        public string Pattern
        {
            get { return _pattern; }
            set { _pattern = value; }
        }

        [Parameter(
            Mandatory = false,
            Position = 2,
            HelpMessage = "Optional Switch to check subfolder tree"
        )]
        public SwitchParameter Recursive;

        [Parameter(
            Mandatory = false,
            Position = 3,
            HelpMessage = "Optional Switch to check if created in last 24 hours"
        )]
        public SwitchParameter IfToday;

        #endregion

        #region ParameterData
        private string _sourceFolder = null;
        private string _pattern = "*";
        #endregion

        #region PrivateData
        private int Count;
        private int Errors;
        BackgroundWorker bgworker;
        bool bgworking = false;
        List<string> VMessages;
        List<string> OMessages;
        #endregion

        string output = "";

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            WriteVerbose(_cmdlet + ": Starting with " + StaticClass.DoubleQuoteMe(_sourceFolder) + " and pattern " + StaticClass.DoubleQuoteMe(_pattern) + ".");
            Count = 0;
            Errors = 0;
            VMessages = new List<string>();
            OMessages = new List<string>();

            // AdobeReader is giving me a hard time by hanging, without error or exception, the attempting to delete certain files.
            // So put that logic in the background and kill it off after a while.
            bgworker = new BackgroundWorker();
            bgworker.DoWork += bgworker_DoWork;
            bgworker.RunWorkerCompleted += bgworker_RunWorkerCompleted;
            bgworker.WorkerSupportsCancellation = true;
            bgworker.RunWorkerAsync(this.ShouldProcess(_sourceFolder, "Cleanup"));

            bgworking = true;

            int countDownDS = 10*5; // 5 seconds
            while (bgworking && countDownDS >= 0)
            {
                System.Threading.Thread.Sleep(100);
                //WriteVerbose(_cmdlet + ":\ttimecheck "  + " @" + DateTime.Now.ToLongTimeString());
                countDownDS--;
                if (countDownDS <= 0)  
                {
                    WriteVerbose(_cmdlet + ": TIMEOUT Ocurred.");
                    bgworker.CancelAsync();
                    bgworking = false;
                }
            }
            ///TheBackgroundWork();

            WriteVerbose("Backgroundworkdone");
            foreach (string v in VMessages)
            {
                WriteVerbose(v);
            }
            foreach (string m in OMessages)
            {
                output += m + "\n";
            }
            if (Errors == 0)
                output += _cmdlet + ": SUCCESS deleting " + Count.ToString() + " files.\n";
            else
                output += _cmdlet + ": ERROR " + Count.ToString() + " files removed but " + Errors.ToString() + " errors while processing.\n";

            WriteObject(output);
        }

        private void bgworker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker helperBW = sender as BackgroundWorker;
            bool should = (bool)e.Argument;
            TheBackgroundWork(should);
            if (helperBW.CancellationPending)
            {
                e.Cancel = true;
            }
            bgworking = false;
        }
        private void bgworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bgworking = false;
        }

        private void TheBackgroundWork(bool Should)
        { 
            try
            {
                if (_sourceFolder != null)
                {
                   ProcessThisFolder(_sourceFolder, Should);
               }
                else
                {
                    OMessages.Add(_cmdlet + ": ERROR Missing required parameters.");
                }
                /////timer.Dispose();
            }
            catch (Exception ex)
            {
                OMessages.Add(_cmdlet + ": ERROR (Unexpected) " + " " + ex.Message);
                Errors++;
            }
        }
        protected void ProcessThisFolder(string currentFolder, bool Should)
        {
            VMessages.Add(_cmdlet + ":\tProcessThisFolder " + StaticClass.DoubleQuoteMe(currentFolder) + "." + " @" + DateTime.Now.ToLongTimeString());
            if (System.IO.Directory.Exists(currentFolder))
            {
                VMessages.Add(_cmdlet + ":\tFolder Exists " + StaticClass.DoubleQuoteMe(currentFolder) + "." + " @" + DateTime.Now.ToLongTimeString());
                try
                {
                    if (Recursive.IsPresent)
                    {
                        VMessages.Add(_cmdlet + ":\tProcessThisFolder Recursive requested." + " @" + DateTime.Now.ToLongTimeString());
                        foreach (string foldername in System.IO.Directory.EnumerateDirectories(currentFolder))
                        {
                            ProcessThisFolder(foldername,Should);
                        }
                    }
                    if (DoesPatternHaveWildCard(_pattern))
                    {
                        VMessages.Add(_cmdlet + ":\tPattern has wild card." + " @" + DateTime.Now.ToLongTimeString());
                        foreach (string filename in System.IO.Directory.EnumerateFiles(currentFolder))
                        {
                            try
                            {
                                VMessages.Add(_cmdlet + ":\tLooking at file " + StaticClass.DoubleQuoteMe(filename) + "." + " @" + DateTime.Now.ToLongTimeString());
                                if (DoesThisMatch(filename, currentFolder.Length + 1, _pattern))
                                {
                                    VMessages.Add(_cmdlet + ":\tIs a match.");
                                    bool skip = false;
                                    if (IfToday.IsPresent)
                                    {
                                        if ((DateTime.Now - System.IO.File.GetCreationTime(filename)) >= new TimeSpan(24, 0, 0))
                                        {
                                            VMessages.Add(_cmdlet + ":\tFile skipped due to age " + filename);
                                            skip = true;
                                        }
                                    }
                                    if (!skip)
                                    {
                                        VMessages.Add(_cmdlet + ":\tchecking attributes " + StaticClass.DoubleQuoteMe(filename) + " @" + DateTime.Now.ToLongTimeString());
                                        FileInfo info = new FileInfo(filename);
                                        if ((info.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                                        {
                                            if (Should)
                                            {
                                                VMessages.Add(_cmdlet + ":\tfixing attributes " + StaticClass.DoubleQuoteMe(filename) + " @" + DateTime.Now.ToLongTimeString());
                                                File.SetAttributes(filename, FileAttributes.Normal);
                                            }
                                            else
                                            {
                                                VMessages.Add(_cmdlet + ":\tfixing attributes would have been done " + StaticClass.DoubleQuoteMe(filename) + " @" + DateTime.Now.ToLongTimeString());
                                            }
                                        }
                                        VMessages.Add(_cmdlet + ":\tdeleting " + StaticClass.DoubleQuoteMe(filename) + " @" + DateTime.Now.ToLongTimeString());
                                        ///File.Delete(filename);
                                        if (Should)
                                        {
                                            info.Delete();
                                            OMessages.Add(_cmdlet + ":\tFile " + StaticClass.DoubleQuoteMe(filename) + " deleted." + " @" + DateTime.Now.ToLongTimeString());
                                        }
                                        else
                                        {
                                            OMessages.Add(_cmdlet + ":\tFile " + StaticClass.DoubleQuoteMe(filename) + " would have been deleted." + " @" + DateTime.Now.ToLongTimeString());
                                        }
                                        Count++;
                                    }
                                }
                                else
                                    VMessages.Add(_cmdlet + ":\tNo match.");
                            }
                            catch (Exception ex)
                            {
                                OMessages.Add(_cmdlet + ": ERROR " + StaticClass.DoubleQuoteMe(filename) + " " + ex.Message);
                                Errors++;
                            }
                        }
                    }
                    else
                    {
                        VMessages.Add(_cmdlet + ":\tPattern without Wildcard." + " @" + DateTime.Now.ToLongTimeString());
                        string lookat = currentFolder + "\\" + _pattern;
                        bool bExists = File.Exists(lookat);
                        if (bExists)
                        {
                            VMessages.Add(_cmdlet + ":\tFile exists " + StaticClass.DoubleQuoteMe(lookat) + " @" + DateTime.Now.ToLongTimeString());
                            bool skip = false;
                            FileInfo info = new FileInfo(lookat);
                            if (IfToday.IsPresent)
                            {
                                if ((DateTime.Now - info.CreationTime) >= new TimeSpan(24, 0, 0))
                                {
                                    OMessages.Add(_cmdlet + ":\tSKIP File due to age " + StaticClass.DoubleQuoteMe(lookat) + " @" + DateTime.Now.ToLongTimeString());
                                    skip = true;
                                }
                            }
                            if (!skip)
                            {
                                try
                                {
                                    VMessages.Add(_cmdlet + ":\tchecking attributes " + StaticClass.DoubleQuoteMe(lookat) + " @" + DateTime.Now.ToLongTimeString());
                                    if ((info.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                                    {
                                        // First clear any readonly attribute...
                                        if (Should)
                                        {
                                            VMessages.Add(_cmdlet + ":\tfix attributes " + StaticClass.DoubleQuoteMe(lookat) + " @" + DateTime.Now.ToLongTimeString());
                                            File.SetAttributes(lookat, FileAttributes.Normal);
                                        }
                                        else
                                        {
                                            VMessages.Add(_cmdlet + ":\tfix attributes would have been done " + StaticClass.DoubleQuoteMe(lookat) + " @" + DateTime.Now.ToLongTimeString());
                                        }
                                    }
                                    // attempt to see if file is locked
                                    VMessages.Add(_cmdlet + ":\tcheck if locked " + StaticClass.DoubleQuoteMe(lookat) + " @" + DateTime.Now.ToLongTimeString());
                                    FileStream stream = System.IO.File.Open(lookat, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                                    if (stream == null)
                                    {
                                        OMessages.Add(_cmdlet + ":\tSKIP File due to in use" + StaticClass.DoubleQuoteMe(lookat) + " @" + DateTime.Now.ToLongTimeString());
                                        skip = true;
                                    }
                                    else
                                        stream.Close();                                    
                                }
                                catch
                                {
                                    OMessages.Add(_cmdlet + ":\tSKIP File due to in use" + StaticClass.DoubleQuoteMe(lookat) + " @" + DateTime.Now.ToLongTimeString());
                                    skip = true;
                                }
                            }
                            if (!skip)
                            {
                                if (Should)
                                {
                                    VMessages.Add(_cmdlet + ":\tdeleting " + StaticClass.DoubleQuoteMe(lookat) + " @" + DateTime.Now.ToLongTimeString());
                                    info.Delete();
                                    OMessages.Add(_cmdlet + ":\tFile " + StaticClass.DoubleQuoteMe(lookat) + " deleted." + " @" + DateTime.Now.ToLongTimeString());
                                }
                                else
                                {
                                    OMessages.Add(_cmdlet + ":\tFile " + StaticClass.DoubleQuoteMe(lookat) + " would have been deleted." + " @" + DateTime.Now.ToLongTimeString());
                                }
                                Count++;
                            }
                            info = null;
                        }
                        else
                        {
                            OMessages.Add(_cmdlet + ":\tFile" + StaticClass.DoubleQuoteMe(lookat) + " does not exist");
                        }
                    }
                }
                catch (Exception ex)
                {
                    OMessages.Add(_cmdlet + ": ERROR " + StaticClass.DoubleQuoteMe(currentFolder) + " " + ex.Message);
                    Errors++;
                }
            }
            else
            {
                OMessages.Add(_cmdlet + ": WARNING Folder " + StaticClass.DoubleQuoteMe(currentFolder) + " does not exist.");
            }
        }

        private bool DoesThisMatch(string filename, int offset, string pattern)
        {
            if (DoesPatternHaveWildCard(pattern))
            {
                string RegExPattern = WildCardToRegular(pattern);

                string matchint001 = filename.Substring(offset);
                int rep = matchint001.LastIndexOf('.');
                if (rep > 0)
                    matchint001 = matchint001.Replace('.', ',');

                VMessages.Add("Updated pattern=" + RegExPattern + " versus " + matchint001);
                System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(RegExPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                VMessages.Add("before regex");
                bool m = rex.IsMatch(matchint001);
                VMessages.Add("after regex");
                if (m)
                    return true;
                return false;
            }
            else
            {
                if (filename.Substring(offset).ToUpper().Equals(pattern.ToUpper()))
                    return true;
                return false;
            }
        }
        private bool DoesPatternHaveWildCard(string pattern)
        {
            if (pattern.Contains('*') || pattern.Contains('?'))
                return true;
            return false;
        }
        private String WildCardToRegular(String value)
        {
            string nodot = value.Replace(".", ","); // "\\u002E");
            return "^" + Regex.Escape(nodot).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}
