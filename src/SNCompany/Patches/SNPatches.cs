using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace SNCompany.Patches
{
    [HarmonyPatch]
    static class SNPatches
    {
        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
		[HarmonyPrefix]
		[HarmonyPriority(150)]
        public static void SNNetworking() {
            SNLevelManager.SetUpNetworking();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
        static void SpawnSNNetworkHandler()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) 
            {
                if (SNNetworkHandler.snObject == null)
                {
                    Plugin.Log.LogError("SN Networking GameObject is null. Cannot instantiate");
                }
                else if (SNNetworkHandler.snObject.GetComponent<NetworkObject>() == null)
                {
                    Plugin.Log.LogError("SN Networking NetworkObject is null. Cannot instantiate");
                }
                else
                {
                    Plugin.Log.LogDebug($"Instantiating SNNetworkHandler");
                    GameObject snNetworking = Object.Instantiate(SNNetworkHandler.snObject);
                    snNetworking.GetComponent<NetworkObject>().Spawn(false);
                    Plugin.Log.LogDebug($"Instantiated SNNetworkHandler");
                }
            }
        }
    }
}