using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtilities.Helpers;
using CommonUtilities.ModularScripts;
using IngameScript;
using Sandbox.ModAPI.Ingame;
using VRage;

namespace CommonUtilities.FeatureModules
{
    public class MasterController : Ticker
    {
        private Queue<Job> _jobQueue = new Queue<Job>();

        private const string MaxDurationConfigKey = "NIN.MC.MAX_TICK_DURATION";
        private TimeSpan _maxDuration
        {
            get
            {
                return _backerMaxDuration;
            }
            set
            {
                _backerMaxDuration = value;
                Owner.ConfigHolder.Set(MaxDurationConfigKey, _backerMaxDuration.Ticks.ToString());
            }
        }
        private TimeSpan _backerMaxDuration;

        public MasterController(Program owner) : base(owner)
        {
            Wait = TimeSpan.Zero;
            var configuredMaxDuration = Owner.ConfigHolder.Get(MaxDurationConfigKey, defaultValue: $"{TimeSpan.TicksPerMillisecond / 10}");
            _backerMaxDuration = new TimeSpan(long.Parse(configuredMaxDuration));
            Owner.EchoManager.AddStatusLine("NIN.MC.JOBS", () => $"Jobs in queue: {_jobQueue.Count}");
        }

        public Job<T> QueueJob<T>(IEnumerable<T> yieldJob)
        {
            var job = new Job<T>(yieldJob);
            _jobQueue.Enqueue(job);
            job.Status = Job.StatusType.Queued;
            return job;
        }

        protected override void Tick(string argument, UpdateType updateSource)
        {
            if (!_jobQueue.Any())
            {
                return;
            }

            var end = DateTime.Now + _maxDuration;
            while (_jobQueue.Any())
            {
                var job = _jobQueue.Dequeue();

                if (job.Run(end - DateTime.Now))
                {
                    continue;
                }

                _jobQueue.Enqueue(job);
                break;
            }
        }
    }
}
