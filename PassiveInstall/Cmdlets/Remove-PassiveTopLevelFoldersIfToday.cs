using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;
using System.IO;

using PassiveInstall.Statics;

namespace PassiveInstall.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "PassiveTopLevelFoldersIfToday")]
    public class PassiveTopLevelFilesIfToday : Cmdlet
    {
        private string _cmdlet = "Remove-PassiveTopLevelFoldersIfToday";

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
            HelpMessage = "Optional string to pattern match, such as \"*SourceHash*\"."
        )]
        public string Pattern
        {
            get { return _pattern; }
            set { _pattern = value; }
        }
        #endregion

        #region ParameterData
        private string _sourceFolder = null;
        private string _pattern = "*";
        #endregion
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            if (_sourceFolder != null)
            {
                WriteVerbose(_cmdlet + ": Starting with " + StaticClass.DoubleQuoteMe(_sourceFolder) + " and pattern " + StaticClass.DoubleQuoteMe(_pattern) + ".");
                int Count = 0;
                int Errors = 0;
                if (System.IO.Directory.Exists(_sourceFolder))
                {
                    try
                    {
                        if (_pattern.Length == 0)
                        {
                            foreach (string filename in System.IO.Directory.EnumerateDirectories(_sourceFolder))
                            {
                                try
                                {
                                    WriteVerbose(_cmdlet + ":\tLooking at " + StaticClass.DoubleQuoteMe(filename) + ".");

                                    System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(_pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                    System.Text.RegularExpressions.Match m = rex.Match(filename);
                                    if (m.Success)
                                    {
                                        WriteVerbose(_cmdlet + ":\tIs a match.");
                                        if ((DateTime.Now - System.IO.File.GetCreationTime(_sourceFolder + "\\" + filename)) < new TimeSpan(24, 0, 0))
                                        {
                                            WriteVerbose(_cmdlet + ":\tdeleting " + _sourceFolder + "\\" + filename);
                                            File.Delete(_sourceFolder + "\\" + filename);
                                            WriteObject(_cmdlet + ":\tFile " + _sourceFolder + "\\" + filename + " deleted.");
                                            Count++;
                                        }
                                        else
                                            WriteVerbose(_cmdlet + ":\tFile skipped due to age " + _sourceFolder + "\\" + filename);
                                    }
                                    else
                                        WriteVerbose(_cmdlet + ":\tNo match.");
                                }
                                catch (Exception ex)
                                {
                                    WriteObject(_cmdlet + ": ERROR \"" + _sourceFolder + "\\" + filename + "\" " + ex.Message);
                                    Errors++;
                                }
                            }
                        }
                        else
                        {
                            // Looking to remove the folder itself, but only if empty
                            if (System.IO.Directory.EnumerateDirectories(_sourceFolder).Count() == 0 &&
                                System.IO.Directory.EnumerateFiles(_sourceFolder).Count() == 0)
                            {
                                if ((DateTime.Now - System.IO.File.GetCreationTime(_sourceFolder)) < new TimeSpan(24, 0, 0))
                                {
                                    WriteVerbose(_cmdlet + ":\tdeleting Folder " + _sourceFolder);
                                    Directory.Delete(_sourceFolder);
                                    WriteObject(_cmdlet + ":\tFolder " + _sourceFolder + " deleted.");
                                    Count++;
                                }
                                else
                                    WriteVerbose(_cmdlet + ":\tFolder skipped due to age " + _sourceFolder);
                            }
                            else
                                WriteVerbose(_cmdlet + ":\tFolder skipped because not empty " + _sourceFolder);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteObject(_cmdlet + ": ERROR \"" + _sourceFolder + "\" " + ex.Message);
                        Errors++;
                    }
                }
                else
                {
                    WriteObject(_cmdlet + ": ERROR Folder \"" + _sourceFolder + "\" does not exist.");
                }
                if (Errors == 0)
                    WriteObject(_cmdlet + ": SUCCESS deleting " + Count.ToString() + " files.");
                else
                    WriteObject(_cmdlet + ": ERROR " + Count.ToString() + " files removed but " + Errors.ToString() + " errors while processing.");
            }
            else
            {
                WriteObject(_cmdlet + ": ERROR Missing required parameters.");
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}
