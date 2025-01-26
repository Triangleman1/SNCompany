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
        public static void SubsidyNetworking() {
            Subsidy.SetUpNetworking();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
        static void SpawnSubsidyNetworkHandler()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) 
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
                    GameObject subsidyNetworking = Object.Instantiate(SubsidyNetworkHandler.subsidyObject);
                    subsidyNetworking.GetComponent<NetworkObject>().Spawn(false);
                    Plugin.Log.LogDebug($"{SubsidyNetworkHandler.WhoAmI()}: Instantiated SubsidyNetworkHandler");
                }
            }
        }

        [HarmonyPatch(typeof(RoundManager), "FinishGeneratingNewLevelClientRpc")]
		[HarmonyPostfix]
        [HarmonyPriority(1000)]
		public static void UndoSubsidies() 
        {
            Subsidy.UnsubsidizeAllMoons();
		}

        [HarmonyPatch(typeof(StartOfRound), "ChangeLevelServerRpc")]
		[HarmonyPrefix]
		public static void PrepareForAdjustSubsidy(int levelID, int newGroupCreditsAmount, StartOfRound __instance) 
        { 
            if (!__instance.travellingToNewLevel && __instance.inShipPhase && newGroupCreditsAmount <= UnityEngine.Object.FindObjectOfType<Terminal>().groupCredits && !__instance.isChallengeFile)
            {
                SNLevel snLevel;
                foreach (var entry in SNLevelManager.SNLevels) 
                {
                    snLevel = entry.Value;
                    if (snLevel.extendedLevel.SelectableLevel.levelID == levelID) 
                    {
                        Plugin.Log.LogDebug($"{SubsidyNetworkHandler.WhoAmI()}: Routed to {snLevel.extendedLevel.SelectableLevel.sceneName}. Lowering remaining subsidy.");
                        if (snLevel.subsidy == -1) 
                        {
                            Plugin.Log.LogError($"Last routed planet had no stored subsidy. Cannot resubsidize.");
                            return;
                        }
                        SubsidyNetworkHandler.amountSubsidy.Value -= snLevel.subsidy;
                        break;
                    }
                }
            }
		}

        [HarmonyPatch(typeof(StartOfRound), "ChangeLevelClientRpc")]
		[HarmonyPrefix]
        [HarmonyPriority(1000)]
		public static void TempResetSubsidies() 
        {
            Plugin.Log.LogDebug($"{SubsidyNetworkHandler.WhoAmI()}: Erasing past subsidy.");
            Subsidy.UnsubsidizeAllMoons();
		}

        [HarmonyPatch(typeof(StartOfRound), "ChangeLevelClientRpc")]
		[HarmonyPostfix]
        [HarmonyPriority(-100)]
		public static void AdjustSubsidies(int levelID) 
        { 
            Plugin.Log.LogDebug($"{SubsidyNetworkHandler.WhoAmI()}: Applying updated subsidy");
			Subsidy.SubsidizeAllMoons();
		}
    }
}