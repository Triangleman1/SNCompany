using System.Collections.Generic;
using LethalLevelLoader;
using LethalModDataLib.Attributes;
using LethalModDataLib.Enums;
using Unity.Netcode;

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

    public static class SNLevelManager
    {
        public static Dictionary<string, SNLevel> SNLevels = [];
        
        public static void InitializeLevels() 
        {
            SNLevels.Clear();
            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
            {
                SNLevel snLevel = new SNLevel(extendedLevel);
                SNLevels.Add(snLevel.moonName, snLevel);
            }
        }

        public static void SetUpNetworking()
        {
            SNNetworkHandler.snObject = NetworkPrefab.GeneratePrefab("SNNetworkHandler");
            SNNetworkHandler.snObject.AddComponent<SNNetworkHandler>();
            if (!NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(SNNetworkHandler.snObject))
            {
                NetworkManager.Singleton.AddNetworkPrefab(SNNetworkHandler.snObject);
            }
            Plugin.Log.LogDebug($"Set up and registered SNNetworkHandler's Networking GameObject");
        }
    }
}