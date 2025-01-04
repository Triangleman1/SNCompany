using System;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;

namespace SNCompany.Patches 
{
    [HarmonyPatch]
	static class StoreInfoAtStart {
		[HarmonyPatch(typeof(StartOfRound), "openingDoorsSequence")]
		[HarmonyPostfix]
		public static void SavePlayersAtStart() {
			Plugin.GradingInfo.playersAtRoundStart = HUDManager.Instance.playersManager.livingPlayers;
            Plugin.Log.LogInfo($"Players saved as {Plugin.GradingInfo.playersAtRoundStart}");
		}

        [HarmonyPatch(typeof(RoundManager), "GenerateNewFloor")]
		[HarmonyPostfix]
		public static void SaveDungeonSize() {
			Plugin.GradingInfo.dungeonLengthAtGeneration = RoundManager.Instance.dungeonGenerator.Generator.LengthMultiplier;
            Plugin.Log.LogInfo($"Dungeon length saved as {Plugin.GradingInfo.dungeonLengthAtGeneration}");
		}

        [HarmonyPatch(typeof(RoundManager), "SyncScrapValuesClientRpc")]
		[HarmonyPostfix]
		public static void SaveScrapAmount(GameObject[] spawnedScrap) {
			Plugin.GradingInfo.totalScrapObjects = spawnedScrap.Length;
            Plugin.Log.LogInfo($"Scrap Quantity saved as {Plugin.GradingInfo.totalScrapObjects}");
		}

	}
}