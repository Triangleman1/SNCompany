using System.Collections.Generic;
using LethalLevelLoader;

namespace SNCompany {
    public class SNLevel 
    {
        public ExtendedLevel extendedLevel;
        public string moonName;
        public int originalPrice;
        public int subsidy;
        public bool subsidized;
        //int x;
        //int y;

        public SNLevel(ExtendedLevel exLevel) 
        {
            extendedLevel = exLevel;
            moonName = exLevel.SelectableLevel.sceneName;
        }
    }

    public class SNLevelManager
    {
        public static Dictionary<string, SNLevel> SNLevels = [];

        public static void InitializeLevels() 
        {
            SNSave.Load();
            Subsidy.globalSubsidized = false;
            Subsidy.SubsidizeAllMoons();
        }
    }
}