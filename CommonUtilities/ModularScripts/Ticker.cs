using System;
using System.Collections.Generic;
using System.Text;
using IngameScript;
using Sandbox.ModAPI.Ingame;

namespace CommonUtilities.ModularScripts
{
    public abstract class Ticker
    {
        /// <summary>
        /// How long ticks wait before triggering again. If this is 0 then it ticks as fast as the script holder.
        /// </summary>
        protected TimeSpan Wait = new TimeSpan(0, 0, 10);

        protected Program Owner;

        private DateTime _nextTick = DateTime.Now;

        public Ticker(Program owner)
        {
            owner.RegisterTicker(this);
        }

        /// <summary>
        /// Called by Main() every tick of the outer script
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="updateSource"></param>
        public void Run(string argument, UpdateType updateSource)
        {
            if (Wait == TimeSpan.Zero)
            {
                Tick(argument, updateSource);
            }

            if (DateTime.Now < _nextTick)
            {
                return;
            }
            _nextTick = DateTime.Now + Wait;

            Tick(argument, updateSource);
        }

        public virtual void Register(Program owner)
        {
            Owner = owner;
        }

        /// <summary>
        /// Called by Main() once the wait time has passed
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="updateSource"></param>
        protected abstract void Tick(string argument, UpdateType updateSource);
    }
}
