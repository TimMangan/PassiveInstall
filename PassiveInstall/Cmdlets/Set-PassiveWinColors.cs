using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    [Cmdlet(VerbsCommon.Set, "PassiveWinColors")]
    public class Set_PassiveWinColors : PSCmdlet
    {
        // When debugging your scripts, I find it convenient to change the size and location of the powershell window so that
        // you can better see the outputs.  I also like to set the colors different between the dependency installers and the 
        // main installer.
        private string _cmdlet = "Set-PassiveWinColors";

        #region ParameterDeclarations
        [Parameter(
                    Mandatory = false,
                    HelpMessage = "Sets the Background color of the window. String must match one of the 16 standard console colors."
         )]
        public string Background
        {
            get { return _background.ToString(); }
            set { SetBackgroundConsoleColor(value); }
        }


        [Parameter(
            Mandatory = false,
            HelpMessage = "Sets the Foreground color of the window. String must match one of the 16 standard console colors."
        )]
        public string Foreground
        {
            get { return _foreground.ToString(); }
            set { SetForegroundConsoleColor(value); }
        }

        [Parameter(
            Mandatory = false,
            HelpMessage = "When set, the current screen will be cleared so that the entire background gets repainted.")]
        public SwitchParameter Clear;
        #endregion


        #region ParameterData
        ConsoleColor _background;
        bool _backgroundSet = false;
        ConsoleColor _foreground;
        bool _foregroundSet = false;

        #endregion


        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            WriteVerbose(_cmdlet + ": Starting.");

            if (Host.Name.ToUpper().Equals("CONSOLE") ||
                Host.Name.ToUpper().Equals("CONSOLEHOST"))
            {
                if (_backgroundSet)
                    try
                    {
                        Host.UI.RawUI.BackgroundColor = _background;
                    } catch { }
                if (_foregroundSet)
                    try
                    {
                        Host.UI.RawUI.ForegroundColor = _foreground;
                    } catch { }
                if (Clear.IsPresent)
                    try
                    {
                        System.Console.Clear();
                    } catch { }
                    
            }
        }


        protected override void EndProcessing()
        {
            base.EndProcessing();
        }

        private void SetBackgroundConsoleColor(string color)
        {
            try
            {
                ConsoleColor c = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), color);
                _background = c;
                _backgroundSet = true;
            }
            catch
            {
                try
                {
                    _background = FuzzyColorMatching(color);
                    _backgroundSet = true;
                }
                catch (Exception ex)
                {
                    WriteObject(_cmdlet + ": ERROR " + ex.Message);
                }
            }
        }
        private void SetForegroundConsoleColor(string color)
        {
            try
            {
                ConsoleColor c = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), color);
                _foreground = c;
                _foregroundSet = true;
            }
            catch
            {
                try
                {
                    _foreground = FuzzyColorMatching(color);
                    _foregroundSet = true;
                }
                catch (Exception ex)
                {
                    WriteObject(_cmdlet + ": ERROR " + ex.Message);
                }
            }
        }

        private ConsoleColor FuzzyColorMatching(string color)
        {
            // Powershell only accepts certain colors
            ConsoleColor ret;
            switch (color.ToUpper())
            {
                case "BLACK":
                    ret = ConsoleColor.Black;
                    break;
                case "BLUE":
                case "LIGHTBLUE":
                    ret = ConsoleColor.Blue;
                    break;
                case "CYAN":
                    ret = ConsoleColor.Cyan;
                    break;
                case "DARKBLUE":
                    ret = ConsoleColor.DarkBlue;
                    break;
                case "DARKCYAN":
                    ret = ConsoleColor.DarkCyan;
                    break;
                case "DARKGRAY":
                    ret = ConsoleColor.DarkGray;
                    break;
                case "DARKGREEN":
                    ret = ConsoleColor.DarkGreen;
                    break;
                case "DARKMAGENTA":
                    ret = ConsoleColor.DarkMagenta;
                    break;
                case "DARKRED":
                    ret = ConsoleColor.DarkRed;
                    break;
                case "DARKYELLOW":
                case "BROWN":
                    ret = ConsoleColor.DarkYellow;
                    break;
                case "GRAY":
                case "LIGHTGRAY":
                    ret = ConsoleColor.Gray;
                    break;
                case "GREEN":
                case "LIGHTGREEN":
                    ret = ConsoleColor.Green;
                    break;
                case "MAGENTA":
                    ret = ConsoleColor.Magenta;
                    break;
                case "RED":
                case "LIGHTRED":
                    ret = ConsoleColor.Red;
                    break;
                case "WHITE":
                    ret = ConsoleColor.White;
                    break;
                case "YELLOW":
                case "LIGHTYELLOW":
                    ret = ConsoleColor.Yellow;
                    break;
                default:
                    throw new Exception("Invalid color specified.");
            }
            return ret;
        }


    }


}
