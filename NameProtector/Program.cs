using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtilities.Extensions;
using CommonUtilities.FeatureModules;
using CommonUtilities.ModularScripts;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    public partial class Program : MyGridProgram, IScriptTemplate
    {
        private bool _enabled = true;
        private DateTime _disabledUntil;

        private string _broadcastTag;

        private const string PriorityConfigKey = "NIN.NAME.PRIORITY";
        /// <summary>
        /// The name script with the highest priority value on a grid applies its name
        /// </summary>
        private int _priority;

        private const string NameConfigKey = "NIN.NAME.SAVED_NAME";
        private string _savedName;

        private const string BroadcastEnabledConfigKey = "NIN.NAME.BROADCAST";
        private bool _broadcastEnabled;

        private const string IgnorePriorityConfigKey = "NIN.NAME.IGNORE_PRIORITY";
        private bool _ignorePriority;

        private const string IgnoreNameConfigKey = "NIN.NAME.IGNORE_NAME";
        private bool _ignoreName;

        private string _status = "Name Protector: Starting up...";
        private Dictionary<string, string> _statusSymbolAnimation = new Dictionary<string, string>
        {
            {"|", "/"},
            {"/", "--"},
            {"--", "\\"},
            {"\\", "|"}
        };
        private string _statusSymbol = "|";

        public void Initialize()
        {
            UpdateFromConfig();

            EchoManager.AddStatusLine("NIN.NAME.STATUS", () => _status);
            EchoManager.AddStatusLine("NIN.NAME.BROADCAST", () => $"Broadcast tag: {_broadcastTag}");
            EchoManager.Print("Monitoring grid for name changes...");
            EchoManager.Print("The name is configured inside the custom data of this block.");
            EchoManager.Print("Or run one of the following commands for a default configuration:");
            EchoManager.Print("'RESET SAFE' Makes sure the name wont cause the grid to be cleaned up but otherwise does nothing.");
            EchoManager.Print("'RESET NORMAL' Sets the grid name to the stored name unless another script is on the same grid with a higher priority.");
            EchoManager.Print("'RESET OBSTINATE' Sets the grid name to the stored name. Ignores priority and has max priority.");
            EchoManager.Print("Default priority is the logic block count of the grid.");
            EchoManager.Print("Default name is whatever the grid name was on configuration reset.");
        }

        public void SaveState() {}

        public void UserInput(string argument)
        {
            var command = argument.Split(' ');
            if (command.Length < 2)
            {
                return;
            }

            if (command[0] != "RESET")
            {
                return;
            }

            ConfigHolder.Set(PriorityConfigKey, $"{GridTerminalSystem.GetBlocks().Count()}");
            ConfigHolder.Set(NameConfigKey, Me.CubeGrid.CustomName);

            switch (command[1])
            {
                case "SAFE":
                {
                    ConfigHolder.Set(BroadcastEnabledConfigKey, "false");
                    ConfigHolder.Set(IgnorePriorityConfigKey, "true");
                    ConfigHolder.Set(IgnoreNameConfigKey, "true");
                    break;
                }
                case "NORMAL":
                {
                    ConfigHolder.Set(BroadcastEnabledConfigKey, "true");
                    ConfigHolder.Set(IgnorePriorityConfigKey, "false");
                    ConfigHolder.Set(IgnoreNameConfigKey, "false");
                    break;
                }
                case "OBSTINATE":
                {
                    ConfigHolder.Set(BroadcastEnabledConfigKey, "true");
                    ConfigHolder.Set(IgnorePriorityConfigKey, "true");
                    ConfigHolder.Set(IgnoreNameConfigKey, "false");
                    break;
                }
                default:
                {
                    EchoManager.Print($"Template {command[1]} does not exist");
                    return;
                }
            }

            EchoManager.Print($"Reset configuration to the template {command[1]}");
            ConfigHolder.Save();
        }

        public void Tick()
        {
            UpdateFromConfig();

            if (_broadcastEnabled)
            {
                Master.QueueJob(UpdateBroadcast());
            }

            if (!_enabled)
            {
                _status = "Name Protector: Disabled";
                return;
            }

            if (string.IsNullOrWhiteSpace(_savedName))
            {
                _status = "Name Protector: No name configured";
                return;
            }

            _statusSymbol = _statusSymbolAnimation[_statusSymbol];
            _status = $"Name Protector: Running {_statusSymbol}";

            if (!NameIsSafe())
            {
                UpdateName();
                return;
            }

            if (!_ignoreName && Me.CubeGrid.CustomName != _savedName)
            {
                UpdateName();
                return;
            }
        }

        private void UpdateName()
        {
            EchoManager.Print($"Changing grid name from: '{Me.CubeGrid.CustomName}' to: '{_savedName}'");
            Me.CubeGrid.CustomName = _savedName;
        }

        private void UpdateFromConfig()
        {
            _priority = int.Parse(ConfigHolder.Get(PriorityConfigKey, defaultValue: "-1"));
            _savedName = ConfigHolder.Get(NameConfigKey, defaultValue: Me.CubeGrid.CustomName);
            _ignorePriority = bool.Parse(ConfigHolder.Get(IgnorePriorityConfigKey, defaultValue: "true"));
            _broadcastEnabled = bool.Parse(ConfigHolder.Get(BroadcastEnabledConfigKey, defaultValue: "false"));
            _ignoreName = bool.Parse(ConfigHolder.Get(IgnoreNameConfigKey, defaultValue: "true"));
        }

        private void Disable()
        {
            _enabled = false;
            _disabledUntil = DateTime.Now + new TimeSpan(0, 0, 5);
        }

        private void EnableIfReady()
        {
            if (DateTime.Now > _disabledUntil)
            {
                _enabled = true;
            }
        }

        private bool NameIsSafe()
        {
            var name = Me.CubeGrid.CustomName.ToLower();

            return !name.Contains("grid");
        }

        private IEnumerable<IMyBroadcastListener> UpdateBroadcast()
        {
            var broadcastTag = $"{Me.CubeGrid.EntityId}.NIN.NAME";
            if (_broadcastTag != broadcastTag)
            {
                IGC.UnregisterBroadcastListener(_broadcastTag);
                _broadcastTag = broadcastTag;
            }
            var broadcastListener = IGC.RegisterBroadcastListener(broadcastTag);

            yield return null;

            IGC.SendBroadcastMessage(broadcastTag, _priority);

            yield return null;

            while (broadcastListener.HasPendingMessage)
            {
                var message = broadcastListener.AcceptMessage();
                var priorityReceived = -1;
                try
                {
                    priorityReceived = (int) message.Data;
                }
                catch
                {
                    EchoManager.Print($"Received an invalid message in broadcast channel: {message.Data}");
                }

                if (!_ignorePriority && priorityReceived > _priority)
                {
                    Disable();
                }

                yield return null;
            }

            EnableIfReady();

            yield return broadcastListener;
        }
    }
}
