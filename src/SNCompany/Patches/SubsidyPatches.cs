using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace SNCompany.Patches 
{
    [HarmonyPatch]
	static class SubsidyPatches 
    {
        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
		[HarmonyPrefix]
		[HarmonyPriority(150)]
        public static void Networking(GameNetworkManager __instance) {
            //GameObject subsidyPrefab = Subsidy.SetUpNetworking();
            //__instance.GetComponent<NetworkManager>().AddNetworkPrefab(subsidyPrefab);
            Subsidy.SetUpNetworking();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
        static void SpawnNetworkHandler()
        {
            if(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) 
            {
                if (SubsidyNetworkHandler.subsidyObject == null)
                {
                    Plugin.Log.LogError("Subsidy Networking GameObject is null. Cannot instantiate");
                }
                else if (SubsidyNetworkHandler.subsidyObject.GetComponent<NetworkObject>() == null)
                {
                    Plugin.Log.LogError("Subsidy Networking NetworkObject is null. Cannot instantiate");
                }
                else
                {
                    Plugin.Log.LogDebug($"Instantiating SubsidyNetworkHandler");
                    GameObject go = Object.Instantiate(SubsidyNetworkHandler.subsidyObject);
                    Plugin.Log.LogDebug($"Spawning NetworkObject");
                    go.GetComponent<NetworkObject>().Spawn(false);
                    Plugin.Log.LogDebug($"Instantiated SubsidyNetworkHandler");
                }
            }
        }


        [HarmonyPriority(100)]
		[HarmonyPatch(typeof(StartOfRound), "Start")]
		[HarmonyPostfix]
        public static void Initialize() 
        {
            SNLevelManager.InitializeLevels();
        }

        [HarmonyPatch(typeof(RoundManager), "FinishGeneratingNewLevelClientRpc")]
		[HarmonyPostfix]
        [HarmonyPriority(1000)]
		public static void UndoSubsidies() 
        {
            Subsidy.UnsubsidizeAllMoons();
		}

        [HarmonyPatch(typeof(StartOfRound), "ChangeLevelClientRpc")]
		[HarmonyPrefix]
        [HarmonyPriority(1000)]
		public static void TempResetSubsidies() 
        {
            Subsidy.UnsubsidizeAllMoons();
		}

        [HarmonyPatch(typeof(StartOfRound), "ChangeLevelClientRpc")]
		[HarmonyPostfix]
        [HarmonyPriority(-100)]
		public static void AdjustSubsidies(int levelID) 
        { 
            SNLevel snLevel;
            foreach (var entry in SNLevelManager.SNLevels) 
            {
                snLevel = entry.Value;
                if (snLevel.extendedLevel.SelectableLevel.levelID == levelID) 
                {
                    Plugin.Log.LogDebug($"Routed to {snLevel.extendedLevel.SelectableLevel.sceneName}. Calculating remaining subsidy.");
                    if (snLevel.subsidy == -1) {
                        Plugin.Log.LogError($"Last routed planet had no stored subsidy. Cannot resubsidize.");
                        return;
                    }
                    Plugin.Log.LogDebug($"maxSubsidy: {SubsidyNetworkHandler.amountSubsidy.Value}");
                    Plugin.Log.LogDebug($"Previous subsidy: {snLevel.subsidy}");
                    SubsidyNetworkHandler.amountSubsidy.Value -= snLevel.subsidy;
                    Plugin.Log.LogDebug($"New maxSubsidy: {SubsidyNetworkHandler.amountSubsidy.Value}");
			        Subsidy.SubsidizeAllMoons();
                    break;
                }
            }
		}
    }
}