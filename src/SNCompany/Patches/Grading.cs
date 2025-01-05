using System;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using UnityEngine.ProBuilder.MeshOperations;

namespace SNCompany.Patches 
{
    [HarmonyPatch]
	static class Grading {
		[HarmonyPatch(typeof(StartOfRound), "openingDoorsSequence")]
		[HarmonyPostfix]
		public static void SavePlayersAtStart() {
			Plugin.GradingInfo.playersAtRoundStart = HUDManager.Instance.playersManager.livingPlayers;
            Plugin.Log.LogInfo($"Number of players saved as {Plugin.GradingInfo.playersAtRoundStart}");
		}

        [HarmonyPatch(typeof(RoundManager), "GenerateNewFloor")]
		[HarmonyPostfix]
		public static void SaveDungeonSize() {
			Plugin.GradingInfo.dungeonLengthAtGeneration = RoundManager.Instance.currentLevel.factorySizeMultiplier;
			Plugin.Log.LogInfo($"mapSizeMultiplier is {RoundManager.Instance.mapSizeMultiplier}");
			Plugin.Log.LogInfo($"[currentDungeonType].MapTileSize as {RoundManager.Instance.dungeonFlowTypes[RoundManager.Instance.currentDungeonType].MapTileSize}");
			Plugin.Log.LogInfo($"Dungeon length saved as {Plugin.GradingInfo.dungeonLengthAtGeneration} (currentLevel.factorySizeMultiplier)");
		}

        [HarmonyPatch(typeof(RoundManager), "SyncScrapValuesClientRpc")]
		[HarmonyPostfix]
		public static void SaveTotalScrapAmount(GameObject[] spawnedScrap) {
			Plugin.GradingInfo.totalScrapObjects = spawnedScrap.Length;
            Plugin.Log.LogInfo($"Scrap Quantity saved as {Plugin.GradingInfo.totalScrapObjects}");
		}

		[HarmonyPatch(typeof(StartOfRound), "EndOfGameClientRpc")]
		[HarmonyPrefix]
		public static void SaveCollectedScrapAmount() {

			Plugin.Log.LogInfo($"Running SaveCollectedScrapAmount()");
			Plugin.GradingInfo.scrapObjectsCollected = RoundManager.Instance.scrapCollectedThisRound.Count;
            Plugin.Log.LogInfo($"Scrap Objects Collected saved as {Plugin.GradingInfo.scrapObjectsCollected}");
		}

		[HarmonyPatch(typeof(HUDManager), "FillEndGameStats")]
		[HarmonyPostfix]
		public static void FairGrading(int scrapCollected) {
            // [U] = Unimplemented
            double scrapValueRate;
            double scrapObjectRate;
            double weightedClearRate;
            double totalDungeonCleared;
            double branchDistance;
            double mainPathDistance;
            double efficiency;
            // [U] double efficiencyBalanced;
			double dungeonSize = Plugin.GradingInfo.dungeonLengthAtGeneration;
			int numPlayersAtTakeoff = RoundManager.Instance.playersManager.connectedPlayersAmount + 1;
            int numPlayersAtLanding = Plugin.GradingInfo.playersAtRoundStart;
            double numPlayers = ((double)numPlayersAtTakeoff+(double)numPlayersAtLanding)/2;
			double valueFactor = .4;
            double stemBranchFactor = 1; //Portion of dungeon time spent conveyoring items down main path (as opposed to initial exploration) while fully clearing dungeon size 1. Interior-dependent. Out of 2. (stemBranch = 1 means 50%). No fire exits or outdoor travel time.
			double interiorOffset = 0;
			double moonOffset = 0; //Conveyor time added by ship to entrance travel time, and subtracted due to fire exit availability
            double branchLengthIncreaseWithSize = 0;
			double groupInefficiency = .625;
            // [U] double moonDifficultyScalar = .2;
            // [U] double moonDifficulty;
            // [U] double interiorDifficulty;

			Plugin.Log.LogInfo($"ValueFactor: {valueFactor}");
			Plugin.Log.LogInfo($"DungeonSize: {dungeonSize}");
			Plugin.Log.LogInfo($"NumPlayers: {numPlayers}");
			Plugin.Log.LogInfo($"StemBranchFactor: {stemBranchFactor}");
			Plugin.Log.LogInfo($"InteriorOffset: {interiorOffset}");
			Plugin.Log.LogInfo($"MoonExponentialFactor: {moonOffset}");
            Plugin.Log.LogInfo($"BranchLengthIncreaseWithSize: {branchLengthIncreaseWithSize}");
			Plugin.Log.LogInfo($"groupInefficiency: {groupInefficiency}");
            // [U] Plugin.Log.LogInfo($"moonDifficultyScalar: {moonDifficultyScalar}");
			// [U] Plugin.Log.LogInfo($"moonDifficulty: {moonDifficulty}");
            // [U] Plugin.Log.LogInfo($"interiorDifficulty: {interiorDifficulty}");

			//TODO: Add bees to scrap rates. Terminal Code?

			if (StartOfRound.Instance.allPlayersDead) {
				efficiency = 0;
				Plugin.Log.LogInfo($"efficiency: 0 (All Players Lost)");
			}
			else {
            	scrapValueRate = scrapCollected / RoundManager.Instance.totalScrapValueInLevel;
            	scrapObjectRate = (double) Plugin.GradingInfo.scrapObjectsCollected / (double)Plugin.GradingInfo.totalScrapObjects;
            	weightedClearRate = valueFactor*scrapValueRate+(1-valueFactor)*scrapObjectRate;
				totalDungeonCleared = weightedClearRate*(dungeonSize+moonOffset-interiorOffset);
            	branchDistance = Math.Pow(totalDungeonCleared,1+branchLengthIncreaseWithSize)*(2-stemBranchFactor);
           		//Relationship between increasing dungeon size and the exponentially increasing distance players must travel to clear it
            	mainPathDistance = (1/(stemBranchFactor+1))*(totalDungeonCleared*((totalDungeonCleared*stemBranchFactor*stemBranchFactor)+stemBranchFactor));
            	efficiency = 95*(branchDistance+mainPathDistance)/Math.Pow(numPlayers, groupInefficiency);
            	// [U] efficiencyBalanced = efficiencyScaled*(1+(moonDifficultyFactor*moonDifficulty))*interiorDifficulty

            	Plugin.Log.LogInfo($"scrapValueRate: {scrapValueRate}");
				Plugin.Log.LogInfo($"scrapObjectRate: {scrapObjectRate}");
            	Plugin.Log.LogInfo($"WeightedClearRate: {weightedClearRate}");
            	Plugin.Log.LogInfo($"totalDungeonCleared: {totalDungeonCleared}");
				Plugin.Log.LogInfo($"branchDistance: {branchDistance}");
            	Plugin.Log.LogInfo($"mainPathDistance: {mainPathDistance}");
				Plugin.Log.LogInfo($"efficiency: {efficiency}");
            	// [U] Plugin.Log.LogInfo($"efficiencyBalanced: {efficiencyBalanced}");
			}

			if (efficiency < Plugin.GradingInfo.gradeThresholds[0]) HUDManager.Instance.statsUIElements.gradeLetter.text = "F";
            else if (efficiency < Plugin.GradingInfo.gradeThresholds[1]) HUDManager.Instance.statsUIElements.gradeLetter.text = "D";
            else if (efficiency < Plugin.GradingInfo.gradeThresholds[2]) HUDManager.Instance.statsUIElements.gradeLetter.text = "C";
            else if (efficiency < Plugin.GradingInfo.gradeThresholds[3]) HUDManager.Instance.statsUIElements.gradeLetter.text = "B";
            else if (efficiency < Plugin.GradingInfo.gradeThresholds[4]) HUDManager.Instance.statsUIElements.gradeLetter.text = "A";
			else  HUDManager.Instance.statsUIElements.gradeLetter.text = "S";
		}
	}
}