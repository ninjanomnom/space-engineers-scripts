using System;
using System.Collections.Generic;
using System.Text;
using CommonUtilities.FeatureModules;
using CommonUtilities.ModularScripts;
using IngameScript.CommonUtilities.Helpers;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    /// <summary>
    /// The base class for scripts that make use of the modular scripts system
    /// Features are only activated when used to prevent unnecessary performance overhead
    /// </summary>
    public partial class Program : MyGridProgram, IScriptCore
    {
        public MasterController Master => _master.Value;
        private readonly LateLoad<MasterController> _master;

        public EchoManager EchoManager => _echoManager.Value;
        private readonly LateLoad<EchoManager> _echoManager;

        public ConfigHolder ConfigHolder => _configHolder.Value;
        private readonly LateLoad<ConfigHolder> _configHolder;

        private TimeSpan Wait = new TimeSpan(0, 0, 1);
        private DateTime _nextTick;

        private readonly List<Ticker> _tickers = new List<Ticker>();
        private readonly List<ISaver> _savers = new List<ISaver>();

        private readonly Queue<TimeSpan> _timeTracker = new Queue<TimeSpan>();
        private string _timeAverage;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;

            _master = new LateLoad<MasterController>(() => new MasterController(this));
            _echoManager = new LateLoad<EchoManager>(() => new EchoManager(this));
            _configHolder = new LateLoad<ConfigHolder>(() => new ConfigHolder(this));

            _nextTick = DateTime.Now;

            EchoManager.AddStatusLine("NIN.MC.AVG", () => $"Average tick duration: {_timeAverage}");

            Initialize();
        }

        public void Save()
        {
            foreach (var saver in _savers)
            {
                saver.Save();
            }

            SaveState();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            var start = DateTime.Now;

            foreach (var ticker in _tickers)
            {
                ticker.Run(argument, updateSource);
            }

            if ((updateSource & UpdateType.Update1) != 0)
            {
                if (DateTime.Now >= _nextTick)
                {
                    _nextTick = DateTime.Now + Wait;
                    Tick();
                }
            }

            if ((updateSource & UpdateType.Terminal) != 0)
            {
                UserInput(argument);
            }

            UpdateTimeTracker(DateTime.Now - start);
        }

        public void RegisterTicker(Ticker ticker)
        {
            ticker.Register(this);
            _tickers.Add(ticker);
        }

        public void RegisterSaver(ISaver saver)
        {
            _savers.Add(saver);
        }

        private void UpdateTimeTracker(TimeSpan duration)
        {
            const int maxCount = 100;
            while (_timeTracker.Count >= maxCount)
            {
                _timeTracker.Dequeue();
            }

            _timeTracker.Enqueue(duration);
            long total = 0;
            foreach (var time in _timeTracker)
            {
                total += time.Ticks;
            }

            var average = new TimeSpan(total / maxCount);
            _timeAverage = $"{average.ToString("fffffff").Insert(3, ".")}ms";
        }
    }
}
