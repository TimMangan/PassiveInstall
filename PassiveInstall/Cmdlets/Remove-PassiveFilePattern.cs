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
    // Copyright 2018 TMurgent Technologies, LLP

    /*********************************************************************************************************************
    [Cmdlet(VerbsCommon.Remove, "PassiveFilePattern")]
    public class PassiveFilePattern : Cmdlet
    {
        private string _cmdlet = "Remove-PassiveFilePattern";

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
            HelpMessage = "String to pattern match, such as \"SourceHash*\", defaults to all."
        )]
        public string Pattern
        {
            get { return _pattern; }
            set { _pattern = value; }
        }

        [Parameter(
            Mandatory = false,
            Position = 2,
            HelpMessage = "Switch parameter to check creation date.")
            ]
        public SwitchParameter IfCreatedToday;

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
                string t = _cmdlet + ": Starting with " + StaticClass.DoubleQuoteMe(_sourceFolder) + " pattern " + StaticClass.DoubleQuoteMe(_pattern);
                if (IfCreatedToday.IsPresent)
                    t += " and -IfCreatedToday.";
                else
                    t += " without date check.";
                WriteVerbose(t);
                int Count = 0;
                int Errors = 0;
                string UpdatedSourceFolder;
                if (_sourceFolder.EndsWith("\\"))
                    UpdatedSourceFolder = _sourceFolder.Substring(0, _sourceFolder.Length - 1);  // help them out if they included a trailing slash on the folder name.
                else
                    UpdatedSourceFolder = _sourceFolder;

                if (System.IO.Directory.Exists(UpdatedSourceFolder))
                {
                    try
                    {
                        if (!_pattern.Contains('\\'))
                        {
                            Count += RemoveFilePattern(UpdatedSourceFolder, _pattern);
                        }
                        else
                        {
                            Count += CheckFolderPattern(UpdatedSourceFolder, _pattern);
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
        private int CheckFolderPattern(string currentfolder, string currentpattern)
        {
            WriteVerbose(_cmdlet + ":\tCheckFolderPattern " + StaticClass.DoubleQuoteMe(currentfolder) + " " + StaticClass.DoubleQuoteMe(currentpattern));
            // ex C:\foo with "*\k*\jkl\*.msi"  == 4 parts
            string[] folderparts = currentpattern.Split('\\');
            //string workingfolder = currentfolder;
            int Count = 0;
            for (int i = 0; i < folderparts.Length - 1; i++)
            {
                ///workingfolder += '\\' + folderparts[i];
                foreach (string folder in System.IO.Directory.EnumerateDirectories(currentfolder, folderparts[i]))
                {
                    WriteVerbose(_cmdlet + ":\tCheckFolderPattern  Subfolder to scan is " + StaticClass.DoubleQuoteMe(folder));
                    if (i < folderparts.Length - 2)
                    {
                        // There is another sub-directory to check
                        string remainingpattern = "";
                        for (int j = i + 1; j < folderparts.Length; j++)
                        {
                            if (remainingpattern.Length > 0)
                                remainingpattern += "\\";
                            remainingpattern += folderparts[j];
                            CheckFolderPattern(folder, remainingpattern);
                        }
                    }
                    else
                    {
                        Count += RemoveFilePattern(folder, folderparts[folderparts.Length - 1]);
                    }
                }
            }
            return Count;
        }

        private int RemoveFilePattern( string sourceFolder, string pattern)
        {
            int Count = 0;
            List<string> DeleteMes = new List<string>();
            WriteVerbose(_cmdlet + ":\tRemoveFilePattern " + StaticClass.SingleQuoteMe(sourceFolder) + " " + StaticClass.DoubleQuoteMe(pattern));
            foreach (string filename in System.IO.Directory.EnumerateFiles(sourceFolder, pattern))
            {
                try
                {
                    WriteVerbose(_cmdlet + ":\tRemoveFilePattern Found  " + StaticClass.DoubleQuoteMe(filename) + ".");
                    bool skip = false;
                    if (IfCreatedToday.IsPresent)
                    {
                        if ((DateTime.Now - System.IO.File.GetCreationTime(filename)) > new TimeSpan(24, 0, 0))
                            skip = true;
                    }
                    if (!skip)
                    {
                        //DeleteMes.Add(filename);
                        //WriteVerbose(_cmdlet + ":\tdelete scheduled " + filename + ".");
                        WriteVerbose(_cmdlet + ":\tdelete " + filename + ".");
                        File.SetAttributes(filename, FileAttributes.Normal);
                        File.Delete(filename);
                        WriteObject(_cmdlet + ":\tFile " + filename + " deleted.");
                        Count++;
                    }
                    else
                        WriteVerbose(_cmdlet + ":\tFile is more than 24 hours old, not deleted.");
                }
                catch (Exception ex)
                {
                    WriteObject(_cmdlet + ": ERROR \"" + filename + "\" " + ex.Message);
                    ///Errors++;
                }
            }
            foreach (string filename in DeleteMes)
            {
                try
                {
                    WriteVerbose(_cmdlet + ":\tdelete " + filename + " now.");
                    File.SetAttributes(filename, FileAttributes.Normal);
                    File.Delete(filename);
                    WriteObject(_cmdlet + ":\tFile " + filename + " deleted.");
                    Count++;
                }
                catch (Exception ex)
                {
                    WriteObject(_cmdlet + ": ERROR deleting \"" + filename + "\" " + ex.Message);
                }
            }
            return Count;
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
    **************************************************************************************************************************************/
}
