using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtilities.ModularScripts;
using IngameScript;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;

namespace CommonUtilities.FeatureModules
{
    public class EchoManager : Ticker
    {
        private readonly Dictionary<string, Func<string>> _statusLines = new Dictionary<string, Func<string>>();
        private readonly Queue<string> _currentOutput = new Queue<string>();
        private readonly IMyTextSurface _display;

        private string _cachedOutput;

        public EchoManager(Program owner) : base(owner)
        {
            Wait = TimeSpan.Zero;
            _display = Owner.Me.GetSurface(0);
            _display.FontSize = 0.4f;
        }

        public void Print(string line)
        {
            line = $"[{DateTime.Now:T}] {line}";
            _currentOutput.Enqueue(line);
            _cachedOutput = null;
            while (_currentOutput.Count > 29 - _statusLines.Count)
            {
                _currentOutput.Dequeue();
            }
        }

        public void AddStatusLine(string id, Func<string> statusLine)
        {
            _statusLines.Add(id, statusLine);
        }

        protected override void Tick(string argument, UpdateType updateSource)
        {
            var statusOutput = string.Join("\n", _statusLines.Select(kvp => kvp.Value.Invoke()));

            if (_cachedOutput == null)
            {
                _cachedOutput = string.Join("\n", _currentOutput);
            }

            var output = $"{statusOutput}\n{_cachedOutput}";

            Owner.Echo(output);
            _display.WriteText(output);
        }
    }
}
