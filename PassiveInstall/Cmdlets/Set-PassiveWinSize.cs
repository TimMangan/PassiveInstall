using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;
using System.Runtime.InteropServices;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    [Cmdlet(VerbsCommon.Set, "PassiveWinSize")]
    public class Set_PassiveWinSize : PSCmdlet
    {
        // When debugging your scripts, I find it convenient to change the size and location of the powershell window so that
        // you can better see the outputs.  I also like to set the colors different between the dependency installers and the 
        // main installer.
        private string _cmdlet = "Set-PassiveWinSize";

        #region ParameterDeclarations
        [Parameter(
                    Mandatory = false,
                    Position = 0,
                    HelpMessage = "Sets the Width of the window in pixels."                    
         ),
         ValidateRange(40, 500)
        ]
        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }


        [Parameter(
            Mandatory = false,
            Position = 1,
            HelpMessage = "Sets the Height of the window in pixels."
        ),
        ValidateRange(10, 80)
        ]
        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }


        [Parameter(
            Mandatory = false,
            Position = 3,
            HelpMessage = "Optional, Sets the X position of the window in pixels from left edge."
        ),
        ValidateRange(0,4000)
        ]
        [Alias("X")]
        public int PositionX
        {
            get { return _posX; }
            set { _posX = value; }
        }


        [Parameter(
            Mandatory = false,
            Position = 4,
            HelpMessage = "Optional, Sets the Y position of the window in pixels from top edge."
        ),
        ValidateRange(0, 3000)
        ]
        [Alias("Y")]
        public int PositionY
        {
            get { return _posY; }
            set { _posY = value; }
        }



        [Parameter(
            Mandatory = false,
            Position = 5,
            HelpMessage = "Optional, Sets the (powershell) window title."
        )]
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Optional switch to indicate that the window should not be shown."
        )]
        public SwitchParameter DoSilent;

        #endregion

        #region ParameterData
        int _width = -1;
        int _height = -1;
        int _posX = -1;
        int _posY = -1;
        string _title = null;
        #endregion


        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        //Sets window attributes
        [DllImport("USER32.DLL")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("USER32.DLL")]
        public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);
        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms633545(v=vs.85).aspx 


        protected override void ProcessRecord()
        {
            WriteVerbose(_cmdlet + ": Starting.");

            if (Host.Name.ToUpper().Equals("CONSOLE") ||
                Host.Name.ToUpper().Equals("CONSOLEHOST"))
            {
                System.Management.Automation.Host.Size s = new System.Management.Automation.Host.Size(Host.UI.RawUI.WindowSize.Width, Host.UI.RawUI.WindowSize.Height);
                System.Management.Automation.Host.Coordinates c = new System.Management.Automation.Host.Coordinates(Host.UI.RawUI.WindowPosition.X, Host.UI.RawUI.WindowPosition.Y);
                if (DoSilent.IsPresent)
                {
                    //s.Height = s.Width = 1;
                    //Host.UI.RawUI.WindowSize = s;
                    
                    // 0x80 hides the windows, the other options just say don't change anything else
                    IntPtr wh = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                    SetWindowPos(wh, (IntPtr)1, c.X, c.Y, s.Width, s.Height, 0x28F);

                }
                else
                {
                    try
                    {
                        if (_width > 0 || _height > 0)
                        {
                            if (_width > 0)
                                s.Width = _width;
                            if (_height > 0)
                                s.Height = _height;
                            //NB: Windows size cannot exceed the buffer size, so fix the buffer size if needed.
                            System.Management.Automation.Host.Size bs = new System.Management.Automation.Host.Size(Host.UI.RawUI.BufferSize.Width, Host.UI.RawUI.BufferSize.Height);
                            if (s.Height > bs.Height || s.Width > bs.Width)
                            {
                                if (s.Height > bs.Height)
                                    bs.Height = s.Height;
                                if (s.Width > bs.Width)
                                    bs.Width = s.Width;
                                Host.UI.RawUI.BufferSize = bs;
                            }
                            Host.UI.RawUI.WindowSize = s;
                        }
                        if (_posX >= 0 || _posY >= 0)
                        {
                            // The lack of 0x02 means move the window, other bits say don't change other stuff
                            IntPtr wh = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                            SetWindowPos(wh, (IntPtr)1, _posX, _posY, s.Width, s.Height, 0x20D);

                        }
                    }
                    catch
                    {
                        // Sometimes people write scripts using a bit monitor and set the values too large for someone remoting into a Vm showing fewer pixels,
                        // leading to an exception here when you try to make the window too big.  Rather than generate an error (done previously), we should
                        // just hide the error and move on.
                    }
                }
                if (_title != null)
                    Host.UI.RawUI.WindowTitle = _title;
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}
