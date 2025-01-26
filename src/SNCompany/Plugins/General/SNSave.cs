using System.Collections.Generic;
using LethalLevelLoader;
using LethalModDataLib.Attributes;
using LethalModDataLib.Enums;

namespace SNCompany 
{
    public static class SNSave
    {
        [ModData(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.CurrentSave)]
        public static Dictionary<string, Level> SNLevelSaves = new Dictionary<string, Level>();
        //[ModData(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.CurrentSave)]
        //public static int amountSubsidySaved;
        //[ModData(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.CurrentSave)]
        //public static int percentSubsidySaved;

        public struct Level {
            public string moonName;
            public int originalPrice;
            public int subsidy;
            public bool subsidized;
        }

        public static void Save() {
            SNLevelSaves.Clear();
            foreach (var entry in SNLevelManager.SNLevels)
            {
                //General
                SNLevel snLevel = entry.Value;
                Level level = new Level();
                level.moonName = snLevel.moonName;
                
                //Subsidies
                level.originalPrice = snLevel.originalPrice;
                level.subsidy = snLevel.subsidy;
                level.subsidized = snLevel.subsidized;
                
                //Position
                //level.x = snLevel.x
                //level.y = snLevel.y

                SNLevelSaves.Add(level.moonName, level);
                Plugin.Log.LogDebug($"Saved {SNLevelSaves[level.moonName].moonName}");
            }
            //Subsidies
            //amountSubsidySaved = SubsidyNetworkHandler.amountSubsidy.Value;
            //percentSubsidySaved = SubsidyNetworkHandler.percentSubsidy.Value;
        }

        public static void Load() {
            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
            {
                string moonName = extendedLevel.SelectableLevel.sceneName;
                SNLevel snLevel = SNLevelManager.SNLevels[moonName];

                if (SNLevelSaves.ContainsKey(moonName)) {
                    Plugin.Log.LogDebug($"Save Dictionary contains {moonName}");
                    Level level = SNLevelSaves[moonName];
                    
                    //Subsidies
                    snLevel.originalPrice = level.originalPrice;
                    snLevel.subsidy = level.subsidy;
                    snLevel.subsidized = level.subsidized;
                }
                else {
                    snLevel.originalPrice = extendedLevel.RoutePrice;
                    snLevel.subsidy = -1;
                    snLevel.subsidized = false;
                }
            }
            /*

            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
            {
                string moonName = extendedLevel.SelectableLevel.sceneName;

                if (SNLevelSaves.ContainsKey(moonName)) {
                    Plugin.Log.LogDebug($"Save Dictionary contains {moonName}");
                    Level level = SNLevelSaves[moonName];
                    SNNetworkHandler.Instance.LoadSNLevelClientRpc(extendedLevel, level);
                }
                else {
                    Plugin.Log.LogDebug($"Save Dictionary lacks {moonName}");
                    SNNetworkHandler.Instance.InitializeSNLevelClientRpc(extendedLevel);
                }
            }*/
        }
    }
}