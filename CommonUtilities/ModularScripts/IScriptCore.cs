using System;
using System.Collections.Generic;
using System.Text;
using CommonUtilities.FeatureModules;

namespace CommonUtilities.ModularScripts
{
    public interface IScriptCore
    {
        MasterController Master { get; }

        ConfigHolder ConfigHolder { get; }

        EchoManager EchoManager { get; }

        void RegisterTicker(Ticker ticker);

        void RegisterSaver(ISaver saver);
    }
}
