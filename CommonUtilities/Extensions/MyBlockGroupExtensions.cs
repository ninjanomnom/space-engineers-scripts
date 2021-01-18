using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI.Ingame;

namespace CommonUtilities.Extensions
{
    public static class MyBlockGroupExtensions
    {
        public static IEnumerable<T> GetBlocksOfType<T>(this IMyBlockGroup src) where T : class, IMyTerminalBlock
        {
            var temp = new List<IMyTerminalBlock>();
            src.GetBlocksOfType<T>(temp);
            foreach (var thing in temp)
            {
                yield return thing as T;
            }
        }
    }
}
