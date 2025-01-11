using UnityEngine;
using HarmonyLib;

namespace SNCompany.Patches 
{
    [HarmonyPatch]
	static class GradingPatches {
		[HarmonyPatch(typeof(StartOfRound), "openingDoorsSequence")]
		[HarmonyPostfix]
		public static void SavePlayersAndFireExitsAtStart() {
			Grading.numPlayersAtLanding = HUDManager.Instance.playersManager.livingPlayers;
            Plugin.Log.LogInfo($"Number of players saved as {Grading.numPlayersAtLanding}");
			Plugin.Log.LogInfo($"FindNumOfFireExitsPatch Running");
			Grading.numOfFireExits = Grading.FindNumOfFireExits();
            Plugin.Log.LogInfo($"Number of fire exits saved as {Grading.numOfFireExits}");
		}

        [HarmonyPatch(typeof(RoundManager), "GenerateNewFloor")]
		[HarmonyPostfix]
		public static void SaveDungeonSize() {
			Grading.dungeonSize = RoundManager.Instance.currentLevel.factorySizeMultiplier;
			Plugin.Log.LogInfo($"mapSizeMultiplier is {RoundManager.Instance.mapSizeMultiplier}");
			Plugin.Log.LogInfo($"[currentDungeonType].MapTileSize as {RoundManager.Instance.dungeonFlowTypes[RoundManager.Instance.currentDungeonType].MapTileSize}");
			Plugin.Log.LogInfo($"Dungeon length saved as {Grading.dungeonSize} (currentLevel.factorySizeMultiplier)");
		}

        [HarmonyPatch(typeof(RoundManager), "SyncScrapValuesClientRpc")]
		[HarmonyPostfix]
		public static void SaveTotalScrapAmount(GameObject[] spawnedScrap) {
			Grading.totalScrapObjects = spawnedScrap.Length;
            Plugin.Log.LogInfo($"Scrap Quantity saved as {Grading.totalScrapObjects}");
		}

		[HarmonyPatch(typeof(StartOfRound), "EndOfGameClientRpc")]
		[HarmonyPrefix]
		public static void SaveCollectedScrapAmount() {

			Plugin.Log.LogInfo($"Running SaveCollectedScrapAmount()");
			Grading.scrapObjectsCollected = RoundManager.Instance.scrapCollectedThisRound.Count;
            Plugin.Log.LogInfo($"Scrap Objects Collected saved as {Grading.scrapObjectsCollected}");
		}

		[HarmonyPatch(typeof(HUDManager), "FillEndGameStats")]
		[HarmonyPostfix]
		public static void FairGrading(int scrapCollected) {
			double efficiency;

			if (StartOfRound.Instance.allPlayersDead) 
			{
				efficiency = 0;
				Plugin.Log.LogInfo($"efficiency: 0 (All Players Lost)");
			}
			else {
				Grading.PrepareForEfficiencyCalculation(scrapCollected);
				Grading.LogGradingVariables();
				efficiency = Grading.CalculateEfficiencyPerPlayer();
				Grading.LogGradingCalculations();
			}
			if (efficiency < Grading.gradeThresholds[0]) HUDManager.Instance.statsUIElements.gradeLetter.text = "F";
            else if (efficiency < Grading.gradeThresholds[1]) HUDManager.Instance.statsUIElements.gradeLetter.text = "D";
            else if (efficiency < Grading.gradeThresholds[2]) HUDManager.Instance.statsUIElements.gradeLetter.text = "C";
            else if (efficiency < Grading.gradeThresholds[3]) HUDManager.Instance.statsUIElements.gradeLetter.text = "B";
            else if (efficiency < Grading.gradeThresholds[4]) HUDManager.Instance.statsUIElements.gradeLetter.text = "A";
			else  HUDManager.Instance.statsUIElements.gradeLetter.text = "S";
		}
	}
}