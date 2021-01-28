using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Management.Automation;
using System.IO;
using System.Runtime.InteropServices;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    [Cmdlet(VerbsCommon.New, "PassiveDesktopShortcut", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class NewPassiveDesktopShortcut : Cmdlet
    {
        //Adapted from https://gallery.technet.microsoft.com/scriptcenter/New-Shortcut-4d6fb3d8  
        private string _cmdlet = "New-PassiveDesktopShortcut";


        #region ParameterDeclarations
        [Parameter(Mandatory = true,
                    Position = 0,
                    HelpMessage = "Target, including full path, that the shortcut will point to. Enclose in quotation marks if path includes a space. Do not include arguments."
                )]
        [Alias("Target")]
        public string TargetPath
        {
            get { return _TargetPath; }
            set { _TargetPath = value; }
        }

        [Parameter(Mandatory = true,
                    Position = 1,
                    HelpMessage = "Name of the filename (without path) of the .lnk shortcut to create.  The .lnk extension will be added to this name if not supplied."
                )]
        [Alias("Shortcut")]
        public string ShortcutName
        {
            get { return _ShortcutName; }
            set { _ShortcutName = value; }
        }

        
        [Parameter(
                    Mandatory = false,
                    Position = 2,
                    HelpMessage = "Optional Command line arguments. May be supplied as a string or array of strings."
                )]
        [Alias("Args", "Argument")]
        public string[] Arguments
        {
            get { return _Arguments; }
            set { _Arguments = value; }
        }

        [Parameter(
                    Mandatory = false,
                    Position = 3,
                    HelpMessage = "Optional description of the shortcut to be added to the shortcut parameters."
                )]
        [Alias("Desc")]
        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        [Parameter(
                    Mandatory = false,
                    Position = 4,
                    HelpMessage = "Optional Working Directory for the target.  When not specified this will be the directory of the target."
                )]
        [Alias("WorkingDirectory", "WorkingDir")]
        public string WorkDir
        {
            get { return _WorkDir; }
            set { _WorkDir = value; }
        }


        [Parameter(
                    Mandatory = false,
                    Position = 5,
                    HelpMessage = "Optional path and filename of a ICO file (or WinPE file containing an icon resource). If a resource, a resource number may optionally be appended to the filename after a comma -- otherwise the first icon resource in the file is used."
                )]
        public string Icon
        {
            get { return _Icon; }
            set { _Icon = value; }
        }

        [Parameter(
                    Mandatory = false,
                    Position = 6,
                    HelpMessage = "Optional Window Style. Supported values: 1=Normal 3=Maximized 7=Minimized"
                )]
        public int WindowStyle
        {
            get { return _WindowStyle; }
            set { _WindowStyle = value; }
        }

        [Parameter(
                    Mandatory = false,
                    Position = 7,
                    HelpMessage = "Optional hotkey to trigger the shortcut. Example: \"Ctrl+Alt+e\"."
                )]
        public string HotKey
        {
            get { return _HotKey; }
            set { _HotKey = value; }
        }


        [Parameter(
                    Mandatory = false,
                    HelpMessage = "Optional switch to indicate that the shortcut should have the RunAsAdministrator property set."
                )]
        [Alias("RunAsAdmin")]
        public SwitchParameter Admin;

        #endregion

        #region ParameterStorage
        private string _TargetPath = null;
        private string _ShortcutName = null;
        private string[] _Arguments = null;
        private string _Description = null;
        private string _HotKey = null;
        private string _WorkDir = null;
        private int _WindowStyle = 0;
        private string _Icon = null;
        #endregion

        string output = "";
        bool needRefresh = false;

        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            if (_TargetPath == null ||
                _ShortcutName == null)
            {
                output += _cmdlet + ": ERROR Missing required parameters.\n";
                WriteObject(output);
                return;
            }
            try
            {
                WriteVerbose(_cmdlet + ": Starting to create " + _ShortcutName + " targeting " + _TargetPath);

                if (!_ShortcutName.ToUpper().EndsWith(".LNK"))
                    _ShortcutName += ".lnk";
                string UseShortcutFolder;
                UseShortcutFolder = "C:\\Users\\" + Environment.UserName + "\\Desktop\\";
                string FullShortcutPath = UseShortcutFolder + _ShortcutName;


                // Create the shortcut
                Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); //Windows Script Host Shell Object
                dynamic shell = Activator.CreateInstance(t);
                try
                {
                    if (this.ShouldProcess(_TargetPath, "CreateShortcut"))
                    {
                        var lnk = shell.CreateShortcut(FullShortcutPath);
                        try
                        {

                            lnk.TargetPath = _TargetPath;
                            if (_Arguments != null)
                            {
                                string args = "";
                                foreach (string arg in _Arguments)
                                {
                                    args += arg + " ";
                                }
                                lnk.Arguments = args;
                            }
                            if (_Description != null)
                                lnk.Descripton = _Description;
                            if (_HotKey != null)
                                lnk.HotKey = _HotKey;
                            if (_WorkDir != null)
                                lnk.WorkingDirectory = _WorkDir;
                            if (WindowStyle != 0)
                                lnk.WindowStyle = _WindowStyle;
                            if (_Icon != null)
                                lnk.IconLocation = _Icon;
                            lnk.Save();
                            output +=_cmdlet + ": SUCCESS: Shortcut \"" + FullShortcutPath + "\" created.\n";
                            needRefresh = true;
                        }
                        finally
                        {
                            Marshal.FinalReleaseComObject(lnk);
                        }
                    }
                    else
                    {
                        output += _cmdlet + ": Shortcut \"" + FullShortcutPath + "\" would have been created.\n";
                    }
                }
                finally
                {
                    Marshal.FinalReleaseComObject(shell);
                }

                // Check Admin switch then use technique in the ps1 powershell to modify the shortcut by manually replacing a byte (set offset decimal 22 to value decimal 34.
                if (Admin.IsPresent)
                {
                    try
                    {
                        if (this.ShouldProcess(_TargetPath, "SetAttribute"))
                        {
                            byte[] arr = File.ReadAllBytes(FullShortcutPath);
                            arr[22] = (byte)34;
                            File.WriteAllBytes(FullShortcutPath, arr);
                            output += _cmdlet + ": SUCCESS Setting RunAsAdministrator on shortcut " + FullShortcutPath + "\n";
                        }
                        else
                        {
                            output += _cmdlet + ": RunAsAdministrator on shortcut " + FullShortcutPath + " would have been set.\n";
                        }
                    }
                    catch (Exception ex)
                    {
                        output += _cmdlet + ": ERROR Unable to set RunAsAdministrator on \"" + FullShortcutPath + "\" " + ex.Message + "\n";
                    }
                }
            }
            catch (Exception ex)
            {
                output +=  _cmdlet + ": ERROR " + ex.Message + "\n";
            }
            WriteObject(output);
            if (needRefresh)
                SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero);
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}

