using System.Collections.Generic;
using LethalLevelLoader;
using LethalModDataLib.Attributes;
using LethalModDataLib.Enums;

namespace SNCompany 
{
    public static class SNSave
    {
        [ModData(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.CurrentSave)] //ERROR
        public static Dictionary<string, Level> SNLevelSaves = new Dictionary<string, Level>();

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
                SNLevel snLevel = entry.Value;
                Level level = new Level();
                level.moonName = snLevel.moonName;
                level.originalPrice = snLevel.originalPrice;
                level.subsidy = snLevel.subsidy;
                level.subsidized = snLevel.subsidized;
                SNLevelSaves.Add(level.moonName, level);
                Plugin.Log.LogDebug($"Saved {SNLevelSaves[level.moonName].moonName}");
            }
        }
        public static void Load() {
            SNLevelManager.SNLevels.Clear();
            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
            {
                string moonName = extendedLevel.SelectableLevel.sceneName;
                SNLevel snLevel = new SNLevel(extendedLevel);

                if (SNLevelSaves.ContainsKey(moonName)) {
                    Plugin.Log.LogDebug($"Save Dictionary contains {moonName}");
                    Level level = SNLevelSaves[moonName];
                    snLevel.originalPrice = level.originalPrice;
                    snLevel.subsidy = level.subsidy;
                    //snLevel.subsidized = level.subsidized;
                }
                else {
                    Plugin.Log.LogDebug($"Save Dictionary lacked {moonName}");
                    snLevel.originalPrice = extendedLevel.RoutePrice;
                    snLevel.subsidy = -1;
                    //snLevel.subsidized = false;
                }
                snLevel.subsidized = false;
                SNLevelManager.SNLevels.Add(snLevel.moonName, snLevel);
            }
        }
    }
}