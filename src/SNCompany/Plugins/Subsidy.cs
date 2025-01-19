using System.Collections.Generic;
using LethalLevelLoader;
using LethalModDataLib.Attributes;
using LethalModDataLib.Enums;

namespace SNCompany {

    public static class Subsidy 
    {
        [ModData(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.CurrentSave)]
        public static int percentSubsidy = 0;
        [ModData(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.CurrentSave)]
        public static int amountSubsidy = 0;
        [ModData(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.CurrentSave)]
        public static bool subsidized = false;
        [ModData(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.CurrentSave)]
        public static Dictionary<string, SNLevel> SNLevels = [];
        public static int lastRoutePrice;

        public static int CalculateSubsidy(int moonPrice) 
        {
            int discount;
            if (moonPrice == 0) return 0;
            discount = (int)(moonPrice*((double)percentSubsidy/100));
            if (discount > amountSubsidy) discount = amountSubsidy;
            if (discount > moonPrice) discount = moonPrice;
            Plugin.Log.LogDebug($"Subsidy: {moonPrice}-{discount}={moonPrice-discount}");

            return discount;
        }

        public static void SubsidizeAllMoons() 
        {
            if (!subsidized) 
            {
                foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                {
                    SNLevel SNLevel;
                    int basePrice = extendedLevel.RoutePrice;
                    int discount = CalculateSubsidy(basePrice);
                    string moonName = extendedLevel.SelectableLevel.sceneName;

                    if (!SNLevels.ContainsKey(moonName))
                    {
                        Plugin.Log.LogError($"Subsidy: {moonName} was not registered into dictionary");
                        return;
                    }

                    SNLevel = SNLevels[moonName];
                    SNLevel.originalPrice = basePrice;
                    SNLevel.subsidy = discount;
                    Plugin.Log.LogDebug($"Subsidized {moonName}");
                    extendedLevel.RoutePrice = basePrice-discount;
                }
                subsidized = true;
            }
        }

        public static void UnsubsidizeAllMoons() {
            if (subsidized) {
                foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                {
                    SNLevel SNLevel;
                    string moonName = extendedLevel.SelectableLevel.sceneName;
                    if (SNLevels.ContainsKey(moonName))
                    {
                        SNLevel = SNLevels[moonName];
                        extendedLevel.RoutePrice = SNLevel.originalPrice;
                        Plugin.Log.LogDebug($"Unsubsidized {moonName}");
                    }
                    else
                    {
                        Plugin.Log.LogDebug($"Could not unsubsidize {moonName}");
                    }
                }
                subsidized = false;
            }
        }
    }

    public class SNLevel 
    {
        [ModData(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.CurrentSave)]
        public ExtendedLevel extendedLevel;
        [ModData(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.CurrentSave)]
        public int originalPrice;
        [ModData(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.CurrentSave)]
        public int subsidy;
        //int x;
        //int y;

        public SNLevel(ExtendedLevel exLevel) 
        {
            extendedLevel = exLevel;
        }
    }
}