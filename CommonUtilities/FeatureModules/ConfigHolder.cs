using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtilities.ModularScripts;
using IngameScript;
using Sandbox.ModAPI.Ingame;

namespace CommonUtilities.FeatureModules
{
    public class ConfigHolder : Ticker, ISaver
    {
        private const string BlockStart = "NIN.CONFIG_BLOCK_START";
        private const string BlockStop = "NIN.CONFIG_BLOCK_STOP";
        private const string BlockEntrySeparator = "|===|";

        private readonly Dictionary<string, string> _configs = new Dictionary<string, string>();

        public ConfigHolder(Program owner) : base(owner)
        {
            owner.RegisterSaver(this);
            ReloadConfigs();
        }

        public string Get(string keyName, bool addIfMissing = true, string defaultValue = "")
        {
            var output = defaultValue;
            if (_configs.ContainsKey(keyName))
            {
                output = _configs[keyName];
            } else if (addIfMissing)
            {
                Set(keyName, defaultValue);
                Save();
            }

            return output;
        }

        public void Set(string keyName, string value)
        {
            if (_configs.ContainsKey(keyName))
            {
                _configs[keyName] = value;
            }
            else
            {
                _configs.Add(keyName, value);
            }
        }

        public void Save()
        {
            var rawData = Owner.Me.CustomData.Split('\n').ToList();

            bool insideConfigBlock = false;
            int startIndex = -1;
            int stopIndex = -1;
            for (var i = 0; i < rawData.Count; i++)
            {
                var line = rawData[i];

                if (line.StartsWith(BlockStart))
                {
                    startIndex = i;
                    insideConfigBlock = true;
                }

                if (!insideConfigBlock)
                {
                    continue;
                }

                if (line.StartsWith(BlockStop))
                {
                    stopIndex = i;
                    break;
                }
            }

            var configText = _configs.Select(i => $"{i.Key}{BlockEntrySeparator}{i.Value}").ToList();
            configText.Insert(0, BlockStart);
            configText.Add(BlockStop);

            if (startIndex > -1 && stopIndex > -1)
            {
                rawData.RemoveRange(startIndex, stopIndex - startIndex + 1);

                rawData.InsertRange(
                    startIndex,
                    configText
                );
            }
            else
            {
                rawData = configText.ToList();
            }

            Owner.Me.CustomData = string.Join("\n", rawData);
        }

        protected override void Tick(string argument, UpdateType updateSource)
        {
            ReloadConfigs();
        }

        private void ReloadConfigs()
        {
            bool insideConfigBlock = false;
            var rawData = Owner.Me.CustomData.Split('\n');
            foreach (var line in rawData)
            {
                if (line.StartsWith(BlockStart))
                {
                    insideConfigBlock = true;
                    continue;
                }

                if (line.StartsWith(BlockStop))
                {
                    break;
                }

                if (!insideConfigBlock)
                {
                    continue;
                }

                var splitLine = line.Split(new[] {BlockEntrySeparator}, StringSplitOptions.None);
                if (_configs.ContainsKey(splitLine[0]))
                {
                    _configs[splitLine[0]] = splitLine[1];
                }
                else
                {
                    _configs.Add(splitLine[0], splitLine[1]);
                }
            }
        }
    }
}
