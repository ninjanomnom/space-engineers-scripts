using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI.Ingame;

namespace CommonUtilities.ModularScripts
{
    public interface IScriptTemplate
    {
        /// <summary>
        /// Called just after the constructor is complete
        /// </summary>
        void Initialize();

        /// <summary>
        /// Called when the program needs to save its state. Use
        /// this method to save your state to the Storage field
        /// or some other means. 
        /// </summary>
        void SaveState();

        /// <summary>
        /// The main entry point of the script, invoked every time
        /// one of the programmable block's Run actions are invoked,
        /// or the script updates itself. The updateSource argument
        /// describes where the update came from. Be aware that the
        /// updateSource is a  bitfield  and might contain more than 
        /// one update type.
        /// </summary>
        void Tick();

        /// <summary>
        /// Called when the user runs the script manually, usually with an argument
        /// </summary>
        /// <param name="argument">The argument given by the user</param>
        void UserInput(string argument);
    }
}
