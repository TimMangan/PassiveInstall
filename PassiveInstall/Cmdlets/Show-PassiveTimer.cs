using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;
using System.Runtime.InteropServices;

using System.Timers;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2021 TMurgent Technologies, LLP

    [Cmdlet(VerbsCommon.Show, "PassiveTimer")]
    public class Show_PassiveTimer : PSCmdlet
    {
        // When debugging your scripts, I find it convenient to change the size and location of the powershell window so that
        // you can better see the outputs.  I also like to set the colors different between the dependency installers and the 
        // main installer.
        private string _cmdlet = "Show-PassiveTimer";

        #region ParameterDeclarations
        [Parameter(
                    Mandatory = false,
                    Position = 0,
                    HelpMessage = "Sets the Length of the timer in ms."
         )]
        public int LengthMS
        {
            get { return _lengthMs; }
            set { _lengthMs = value; }
        }

        [Parameter(
            Mandatory = false,
            Position = 1,
            HelpMessage = "Sets the Length of the timer in ms."
 )]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        #endregion

        #region ParameterData
        int _lengthMs = 1000;
        string _message = null;
        #endregion

        int _lenghtCurrentMs;
        ProgressRecord _rcd;
        Timer _aTimer;
        bool _Paused = false;
        string _SavedStatus = null;

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }



        protected override void ProcessRecord()
        {

            if (_message != null)
            {
                WriteObject(_cmdlet + ": " + _message);
                _rcd = new ProgressRecord(1, _cmdlet, _message);
            }
            else
            {
                WriteObject(_cmdlet + ": " + "Sleeping for " + _lengthMs.ToString() + "ms");
                _rcd = new ProgressRecord(1, _cmdlet, _message);
            }

            _lenghtCurrentMs = _lengthMs;

            _aTimer = new Timer(200);
            //_ Hook up the Elapsed event for the timer. 
            _aTimer.Elapsed += OnTimedEvent;
            _aTimer.AutoReset = true;
            _aTimer.Enabled = true;

            //System.Threading.Thread.Sleep(_lengthMs);
        }

        protected void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            //Console.Read();
            if (System.Console.KeyAvailable)
            {
                ConsoleKeyInfo key = System.Console.ReadKey(true);//true don't print char on console
                if (key.Key == ConsoleKey.P ||
                    key.Key == ConsoleKey.CrSel ||
                    key.Key == ConsoleKey.ExSel ||
                    key.Key == ConsoleKey.Pause ||
                    key.Key == ConsoleKey.Select)
                {
                    _Paused = true;
                    if (_SavedStatus == null)
                        _SavedStatus = _rcd.StatusDescription;
                    _rcd.StatusDescription = " ***PAUSED*** Any key to resume, Q to quit sleep.";
                    int isat = ((_lengthMs - _lenghtCurrentMs) * 100) / _lengthMs;
                    if (isat > 0)
                        isat--;
                    _rcd.PercentComplete = isat;
                }
                else if (_Paused &&
                         key.Key == ConsoleKey.Q)
                {
                    _Paused = false;
                    if (_SavedStatus != null)
                        _rcd.StatusDescription = _SavedStatus;
                    _lenghtCurrentMs = 0;
                }
                else
                {
                    _Paused = false;
                    if (_SavedStatus != null)
                        _rcd.StatusDescription = _SavedStatus;
                }
            }
            if (!_Paused)
            {
                if (_lenghtCurrentMs > 200)
                    _lenghtCurrentMs -= 200;
                else
                    _lenghtCurrentMs = 0;
                if (_lenghtCurrentMs > 0)
                {
                    _rcd.PercentComplete = ((_lengthMs - _lenghtCurrentMs) * 100) / _lengthMs;
                    WriteProgress(_rcd);
                }
                else
                {
                    _rcd.PercentComplete = 100;
                    WriteProgress(_rcd);
                    ((Timer)source).Stop();
                    ((Timer)source).Dispose();
                }
            }
        }
        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}
