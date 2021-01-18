using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CommonUtilities.Extensions;
using Sandbox.Game.World;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;
using UpdateFrequency = Sandbox.ModAPI.Ingame.UpdateFrequency;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        private const string _dateTimeFormat = "dd-MMM-yyyy:H:mm:ss";

        private readonly Dictionary<string, string> _dataOptions = new Dictionary<string, string>()
        {
            {"Live", "false"},
            {"Countdown", "30"},
            {"FailurePercent", "5"},
            {"AllowFaction", "true"},
            {"AllowNeutral", "false"},
            {"AllowAllied", "false"}
        };

        private readonly IEnumerable<IMyWarhead> _warheads;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            _warheads = GridTerminalSystem.GetBlocksOfType<IMyWarhead>();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            ParseDataOptions();

            Echo($"Current time is {DateTime.Now.ToString(_dateTimeFormat)}");
            if (string.IsNullOrEmpty(Storage))
            {
                Echo("We are not scheduled to detonate.");
            }
            else
            {
                Echo($"We are scheduled to detonate at {Storage}");
            }

            if (Me.IsBeingHacked)
            {
                Detonate(true);
                return;
            }

            var allowFaction = bool.Parse(_dataOptions["AllowFaction"]);
            var allowNeutral = bool.Parse(_dataOptions["AllowNeutral"]);
            var allowAllied = bool.Parse(_dataOptions["AllowAllied"]);

            var everything = GridTerminalSystem.GetBlocks().ToList();
            var failurePercent = int.Parse(_dataOptions["FailurePercent"]) * 0.01;
            var failureNumber = Math.Ceiling(everything.Count() * failurePercent);
            Echo($"{failureNumber} things must be hacked for self destruct to trigger.");
            var currentlyFailed = 0;
            foreach (var thing in everything)
            {
                if (thing.CubeGrid != Me.CubeGrid)
                {
                    continue;
                }

                var relation = thing.GetUserRelationToOwner(Me.OwnerId);

                if (relation == MyRelationsBetweenPlayerAndBlock.Owner)
                {
                    continue;
                }

                if (allowFaction && relation == MyRelationsBetweenPlayerAndBlock.FactionShare)
                {
                    continue;
                }

                if (allowNeutral && relation == MyRelationsBetweenPlayerAndBlock.Neutral)
                {
                    continue;
                }

                if (allowAllied && relation == MyRelationsBetweenPlayerAndBlock.Friends)
                {
                    continue;
                }

                if (thing.OwnerId != Me.OwnerId)
                {
                    currentlyFailed += 1;
                    if (currentlyFailed >= failureNumber)
                    {
                        Detonate();
                        return;
                    }

                    if (thing.CustomData.ToLower().Contains("important"))
                    {
                        Detonate();
                        return;
                    }
                }

                if (thing.CustomData.ToLower().Contains("critical") && thing.IsBeingHacked)
                {
                    Detonate();
                    return;
                }
            }

            Echo($"Detonation was not triggered, {currentlyFailed} blocks are under hostile control");

            Storage = "";
        }

        private void Detonate(bool immediately=false)
        {
            Echo("Detonation has been triggered!");

            var countdownTime = int.Parse(_dataOptions["Countdown"]);
            if (countdownTime >= 0 && !immediately)
            {
                DateTime detonateTime;
                if (!DateTime.TryParseExact(Storage, _dateTimeFormat, null,
                    0, out detonateTime))
                {
                    detonateTime = DateTime.Now + new TimeSpan(0, 0, countdownTime);
                    Storage = detonateTime.ToString(_dateTimeFormat);
                    return;
                }

                if (detonateTime > DateTime.Now)
                {
                    return;
                }
            }

            Echo("Detonating!");
            if (!bool.Parse(_dataOptions["Live"]))
            {
                return;
            }
            foreach (var bomb in _warheads)
            {
                bomb.Detonate();
            }
        }

        private void ParseDataOptions()
        {
            var splitData = Me.CustomData.Split('\n');
            foreach (var line in splitData)
            {
                if (line.StartsWith("//"))
                {
                    continue;
                }
                var splitOption = line.Split('=');
                if (_dataOptions.ContainsKey(splitOption[0]))
                {
                    _dataOptions[splitOption[0]] = splitOption[1];
                }
            }

            var newData = new List<string>();
            newData.Add("// Only the following options are available, entering anything else here will be removed");
            newData.Add("// Normally, self destruct is triggered when a percent of the ship has been hacked, configurable here.");
            newData.Add("// Other blocks on this ship can add the words 'important' or 'critical' to their data to modify how this works");
            newData.Add("// Important means that if that block is hacked, trigger self destruct");
            newData.Add("// Critical means that if that block even starts to get hacked, trigger self destruct");
            foreach(var entry in _dataOptions)
            {
                newData.Add($"{entry.Key}={entry.Value}");
            }

            Me.CustomData = string.Join("\n", newData);
        }
    }
}
