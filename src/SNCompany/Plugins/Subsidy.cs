using System.Collections.Generic;
using LethalLevelLoader;
using LethalModDataLib.Attributes;
using LethalModDataLib.Enums;

namespace SNCompany {

    public static class Subsidy 
    {
        //To-Do: Networking

        [ModData(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.CurrentSave)]
        public static int percentSubsidy = 0;
        [ModData(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.CurrentSave)]
        public static int amountSubsidy = 0;
        [ModData(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.CurrentSave)]
        public static bool globalSubsidized = false;

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
            if (globalSubsidized == true) 
            {
                Plugin.Log.LogWarning("Tried to subsidize when already subsidized");
                return;
            }
            foreach (var entry in SNLevelManager.SNLevels)
            {
                SNLevel snLevel = entry.Value;
                ExtendedLevel extendedLevel = snLevel.extendedLevel;
                string moonName = snLevel.moonName;
                int basePrice = extendedLevel.RoutePrice;
                int discount = CalculateSubsidy(basePrice);

                if (!snLevel.subsidized) 
                {
                    snLevel.originalPrice = basePrice;
                    snLevel.subsidy = discount;
                    snLevel.subsidized = true;
                    extendedLevel.RoutePrice = basePrice-discount;

                    Plugin.Log.LogDebug($"Subsidized {moonName}");
                }
            }
            globalSubsidized = true;
        }

        public static void UnsubsidizeAllMoons() 
        {
            if (globalSubsidized == false) 
            {
                Plugin.Log.LogWarning("Tried to unsubsidize when not subsidized");
            }
            foreach (var entry in SNLevelManager.SNLevels)
            {
                SNLevel snLevel = entry.Value;
                ExtendedLevel extendedLevel = snLevel.extendedLevel;
                string moonName = snLevel.moonName;
                
                if (snLevel.subsidized == true) 
                {
                    extendedLevel.RoutePrice = snLevel.originalPrice;
                    snLevel.subsidy = 0;
                    snLevel.subsidized = false;
                    Plugin.Log.LogDebug($"Unsubsidized {moonName}");
                }
            }
            globalSubsidized = false;
        }
    }
}