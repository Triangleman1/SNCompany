using LethalLevelLoader;
using LethalModDataLib.Attributes;
using LethalModDataLib.Enums;

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
}