using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Management.Automation;
using System.IO;

namespace PassiveInstall.Cmdlets
{
    // Copyright 2018 TMurgent Technologies, LLP

    [Cmdlet(VerbsCommon.Move, "PassiveFolder", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
    public class Move_PassiveFolder : Cmdlet
    {
        private string _cmdlet = "Move-PassiveFolder";

    #region ParameterDeclarations
    [Parameter(
        Mandatory = true,
        Position = 0,
        HelpMessage = "Folder to be moved."
      )]
    public string Source
    {
        get { return _sourceFolder; }
        set { _sourceFolder = value; }
    }

    [Parameter(
        Mandatory = true,
        Position = 1,
        HelpMessage = "Destination Folder to move onto."
      )]
    public string Destination
    {
        get { return _destinationFolder; }
        set { _destinationFolder = value; }
    }
    #endregion

    #region ParameterData
    private string _sourceFolder = null;
    private string _destinationFolder = null;
        #endregion


    string output = "";
    
    protected override void BeginProcessing()
    {
        base.BeginProcessing();
    }

    protected override void ProcessRecord()
    {
        if (_sourceFolder != null && _destinationFolder != null)
        {
            WriteVerbose(_cmdlet + ": Starting move of " + _sourceFolder + " to " + _destinationFolder);

            // Determine if source folder exists
            if (Directory.Exists(_sourceFolder))
            {
                    // Source was an entire folder to copy
                    if (this.ShouldProcess(_sourceFolder, "MoveFolder"))
                    {
                        MoveThisDirectory(_sourceFolder, _destinationFolder);
                        output += _cmdlet + ": SUCCESS The folder \"" + _sourceFolder + "\" was moved to \"" + _destinationFolder + "\".\n";
                    }
                    else
                    {
                        output += _cmdlet + ": The folder \"" + _sourceFolder + "\" would have been moved to \"" + _destinationFolder + "\".\n";
                    }
                }
            else
            {
                output += _cmdlet + ": ERROR Source folder \"" + _sourceFolder + "\" does not exist.\n";
            }
        }
        else
        {
            output += _cmdlet + ": ERROR Missing required parameters.\n";
        }
            WriteObject(output);
    }
    private void MoveThisDirectory(string source, string dest)
    {
        if (!Directory.Exists(dest))
            Directory.CreateDirectory(dest);

        try
        {
            Directory.Move(source, dest);
        }
        catch (Exception ex)
        {
            WriteObject(_cmdlet + ": ERROR " + ex.Message);
        }
    }

    
    protected override void EndProcessing()
    {

        base.EndProcessing();
    }

}
}
