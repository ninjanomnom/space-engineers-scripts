using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.ModAPI.Ingame;

namespace CommonUtilities.Extensions
{
    public static class MyIntergridCommunicationSystemExtensions
    {
        public static void UnregisterBroadcastListener(this IMyIntergridCommunicationSystem src, string tag)
        {
            var listeners = src.GetBroadcastListeners();
            var taggedListener = listeners.FirstOrDefault(l => l.Tag == tag);
            if (taggedListener == null)
            {
                return;
            }
            src.DisableBroadcastListener(taggedListener);
        }

        public static IEnumerable<IMyBroadcastListener> GetBroadcastListeners(this IMyIntergridCommunicationSystem src)
        {
            var temp = new List<IMyBroadcastListener>();
            src.GetBroadcastListeners(temp);
            return temp;
        }
    }
}
