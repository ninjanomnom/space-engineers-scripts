﻿using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI.Ingame;

namespace CommonUtilities.Extensions
{
    public static class GridTerminalSystemExtensions
    {
        public static IEnumerable<IMyTerminalBlock> GetBlocks(this IMyGridTerminalSystem src)
        {
            var temp = new List<IMyTerminalBlock>();
            src.GetBlocks(temp);
            return temp;
        }

        public static IEnumerable<T> GetBlocksOfType<T>(this IMyGridTerminalSystem src) where T : class, IMyTerminalBlock
        {
            var temp = new List<IMyTerminalBlock>();
            src.GetBlocksOfType<T>(temp);
            foreach (var thing in temp)
            {
                yield return thing as T;
            }
        }

        public static IMyCockpit GetMainCockpit(this IMyGridTerminalSystem src)
        {
            var cockpits = src.GetBlocksOfType<IMyCockpit>();
            foreach (var cockpit in cockpits)
            {
                if (cockpit.IsMainCockpit)
                {
                    return cockpit;
                }
            }

            return null;
        }
    }
}
