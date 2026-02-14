using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PassiveInstall.Structures
{
    public class FirewallRule
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public FirewallRule(string name, string value)
        {
            Name = name; 
            Value = value;
        }
    }
    public class FirewallRules
    {

        private readonly List<FirewallRule> _DefaultRules = new List<FirewallRule>();
        private readonly List<FirewallRule> _ParameterRules = new List<FirewallRule>();


        public FirewallRule[] DefaultRules => _DefaultRules.ToArray();
        public FirewallRule[] ParameterRules => _ParameterRules.ToArray();

        public void AddToDefault(string name, string value) => _DefaultRules.Add(new FirewallRule(name,value));
        public void AddToParameter(string name, string value) => _ParameterRules.Add(new FirewallRule(name,value));

        public string[] Show()
        {
            List<String> result = new List<string>();

            result.Add(_DefaultRules.Count.ToString() + " Default Rules:");
            foreach (FirewallRule rule in _DefaultRules)
            {
                result.Add("    " + rule.Name + " = " + rule.Value);
            }

            result.Add(_ParameterRules.Count.ToString() + " Parameter Rules:");
            foreach (FirewallRule rule in _ParameterRules)
            {
                result.Add("    " + rule.Name + " = " + rule.Value);
            }

            return result.ToArray();
        }
        
    }
}
