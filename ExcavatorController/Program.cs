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
using Sandbox.Game.Entities.Cube;
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
    partial class Program : MyGridProgram
    {
        private readonly IMyCockpit _mainCockpit;
        private readonly IEnumerable<IMyMotorStator> _hinges;
        private readonly IEnumerable<IMyMotorStator> _rotors;

        private float _targetXAngle;
        private float _targetYAngle;

        public Program()
        {
            string[] splitStorage;
            if (string.IsNullOrEmpty(Storage))
            {
                splitStorage = new[] {"", "90", "90"};
            }
            else
            {
                splitStorage = Storage.Split(';');
            }

            var cockpitId = splitStorage[0];
            _targetXAngle = float.Parse(splitStorage[1]);
            _targetYAngle = float.Parse(splitStorage[2]);
            
            IMyCockpit cockpit = null;
            if (!string.IsNullOrEmpty(cockpitId))
            {
                long id;
                long.TryParse(cockpitId, out id);
                cockpit = GridTerminalSystem.GetBlockWithId(id) as IMyCockpit;
                if (!cockpit.IsMainCockpit)
                {
                    cockpit = null;
                }
            }
            if (cockpit == null)
            {
                cockpit = GridTerminalSystem.GetMainCockpit();
            }
            _mainCockpit = cockpit;

            _hinges = GridTerminalSystem.GetBlockGroupWithName("ArmHinges").GetBlocksOfType<IMyMotorStator>();

            _rotors = GridTerminalSystem.GetBlockGroupWithName("ArmRotors").GetBlocksOfType<IMyMotorStator>();

            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Save()
        {
            Storage = $"{_mainCockpit.GetId()};{_targetXAngle};{_targetYAngle}";
        }

        public void Main(string argument, UpdateType updateSource)
        {
            UpdateTargetAngles();
            UpdateMotors();

            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from. Be aware that the
            // updateSource is a  bitfield  and might contain more than 
            // one update type.
            // 
            // The method itself is required, but the arguments above
            // can be removed if not needed.
        }

        private void UpdateTargetAngles()
        {
            if (Math.Abs(_mainCockpit.RotationIndicator.X) > 5)
            {
                _targetXAngle += 0.2f * _mainCockpit.RotationIndicator.X;
            }
            _targetXAngle = Math.Min(Math.Max(_targetXAngle, -90), 90);

            if (Math.Abs(_mainCockpit.RotationIndicator.Y) > 5)
            {
                _targetYAngle += 0.2f * _mainCockpit.RotationIndicator.Y;
            }
            _targetYAngle = Math.Min(Math.Max(_targetYAngle, -90), 90);
        }

        private void UpdateMotors()
        {
            Echo($"Target y angle: {_targetYAngle}");
            foreach (var hinge in _hinges)
            {
                Echo($"Updating hinge named: {hinge?.DisplayNameText}");

                float realAngle = _targetYAngle;
                if (hinge.CustomData.Contains("reverse"))
                {
                    realAngle *= -1;
                }

                hinge.LowerLimitDeg = realAngle;
                hinge.UpperLimitDeg = realAngle;
            }

            Echo($"Target x angle: {_targetXAngle}");
            foreach (var rotor in _rotors)
            {
                Echo($"Updating rotor named: {rotor?.DisplayNameText}");
                
                float realAngle = _targetXAngle;
                if (rotor.CustomData.Contains("reverse"))
                {
                    realAngle *= -1;
                }

                rotor.LowerLimitDeg = realAngle;
                rotor.UpperLimitDeg = realAngle;
            }
        }
    }
}
