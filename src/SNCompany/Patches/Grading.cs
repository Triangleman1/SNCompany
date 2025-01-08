using System;
using UnityEngine;
using HarmonyLib;

namespace SNCompany.Patches 
{
    [HarmonyPatch]
	static class Grading {
		[HarmonyPatch(typeof(StartOfRound), "openingDoorsSequence")]
		[HarmonyPostfix]
		public static void SavePlayersAndFireExitsAtStart() {
			Utility.GradingInfo.playersAtRoundStart = HUDManager.Instance.playersManager.livingPlayers;
            Plugin.Log.LogInfo($"Number of players saved as {Utility.GradingInfo.playersAtRoundStart}");
			Plugin.Log.LogInfo($"FindNumOfFireExitsPatch Running");
			Utility.GradingInfo.numOfFireExits = Utility.FindNumOfFireExits();
            Plugin.Log.LogInfo($"Number of fire exits saved as {Utility.GradingInfo.numOfFireExits}");
		}

        [HarmonyPatch(typeof(RoundManager), "GenerateNewFloor")]
		[HarmonyPostfix]
		public static void SaveDungeonSize() {
			Utility.GradingInfo.MoonInteriorMapSize = RoundManager.Instance.currentLevel.factorySizeMultiplier;
			Plugin.Log.LogInfo($"mapSizeMultiplier is {RoundManager.Instance.mapSizeMultiplier}");
			Plugin.Log.LogInfo($"[currentDungeonType].MapTileSize as {RoundManager.Instance.dungeonFlowTypes[RoundManager.Instance.currentDungeonType].MapTileSize}");
			Plugin.Log.LogInfo($"Dungeon length saved as {Utility.GradingInfo.MoonInteriorMapSize} (currentLevel.factorySizeMultiplier)");
		}

        [HarmonyPatch(typeof(RoundManager), "SyncScrapValuesClientRpc")]
		[HarmonyPostfix]
		public static void SaveTotalScrapAmount(GameObject[] spawnedScrap) {
			Utility.GradingInfo.totalScrapObjects = spawnedScrap.Length;
            Plugin.Log.LogInfo($"Scrap Quantity saved as {Utility.GradingInfo.totalScrapObjects}");
		}

		[HarmonyPatch(typeof(StartOfRound), "EndOfGameClientRpc")]
		[HarmonyPrefix]
		public static void SaveCollectedScrapAmount() {

			Plugin.Log.LogInfo($"Running SaveCollectedScrapAmount()");
			Utility.GradingInfo.scrapObjectsCollected = RoundManager.Instance.scrapCollectedThisRound.Count;
            Plugin.Log.LogInfo($"Scrap Objects Collected saved as {Utility.GradingInfo.scrapObjectsCollected}");
		}

		[HarmonyPatch(typeof(HUDManager), "FillEndGameStats")]
		[HarmonyPostfix]
		public static void FairGrading(int scrapCollected) {
			//Assumes all interiors are balanced to take an equal amount of time to clear at a dungeon size of 1.
			//If not true, users should restrict their dungeon sizes through LLL.
			int totalScrapObjects;
			double scrapValueRate;
            double scrapObjectRate;
            double weightedClearRate;
            double totalDungeonClearedBranches;
			double totalDungeonClearedMainPath;
            double branchDistance;
            double mainPathDistance;
			double mainPathNormalizationFactor; //Ensures main path is equal to stemBranch factor (for experimentation).
            double efficiency;
			double dungeonSize = Utility.GradingInfo.MoonInteriorMapSize;
			int numPlayersAtTakeoff = RoundManager.Instance.playersManager.connectedPlayersAmount + 1;
            int numPlayersAtLanding = Utility.GradingInfo.playersAtRoundStart;
            double numPlayers = ((double)numPlayersAtTakeoff+(double)numPlayersAtLanding)/2;
			double valueFactor = .5;
			double branchLengthIncreaseWithSize = 0; //I don't believe DunGen does this. Probably unnecessary.
            double stemBranchFactor = .35; //Portion of dungeon time spent conveyoring items down main path (as opposed to exploring branches) while fully clearing experimentation. Interior-dependent (eventually). No using fire exits.
			double interiorOffset = .25; //Portion of scrap in branches directly connecting to main entrance at dungeon size 1. (No main path traversal required for these)
			double shipToEntranceTravelTime = 50; //Measured by averaging the time to walk between ship and all entrances (s)
			double mainPathTime = 62; //Time to travel down main path of facility and back with dungeon size of 1 (s)
			double fireExitLuck = .5; //0 assumes worst possible placement. 1 Assumes best
			double groupInefficiency = 1-.3;
            double moonDifficultyScalar = 0;
            double moonDifficulty = 0;
            double interiorDifficulty = 1;
			double k = 192; //NOT CORRECT - Chosen such that 4 players fully clearing moons can get an S rank for march, offense, and adamance, but not for experimentation, assurance, or vow
			int numofFireExits = Utility.GradingInfo.numOfFireExits;

			Plugin.Log.LogInfo($"ValueFactor: {valueFactor}");
			Plugin.Log.LogInfo($"DungeonSize: {dungeonSize}");
			Plugin.Log.LogInfo($"NumPlayers: {numPlayers}");
			Plugin.Log.LogInfo($"StemBranchFactor: {stemBranchFactor}");
			Plugin.Log.LogInfo($"InteriorOffset: {interiorOffset}");
			Plugin.Log.LogInfo($"MoonExponentialFactor: {shipToEntranceTravelTime}");
            Plugin.Log.LogInfo($"BranchLengthIncreaseWithSize: {branchLengthIncreaseWithSize}");
			Plugin.Log.LogInfo($"groupInefficiency: {groupInefficiency}");
            Plugin.Log.LogInfo($"moonDifficultyScalar: {moonDifficultyScalar}");
			Plugin.Log.LogInfo($"moonDifficulty: {moonDifficulty}");
            Plugin.Log.LogInfo($"interiorDifficulty: {interiorDifficulty}");
			Plugin.Log.LogInfo($"numofFireExits: {numofFireExits}");

			//TODO: Config
			//TODO: Exterior scrap? SellBodies? Bees?
			//TODO: Exact Fire Exit Effect 
			//TODO: stemBranch and interiorOffset for each interior, shipToEntranceTravelTime for each moon
			//TODO: Scaling with difficulty (will not happen by default)
			//TODO: + and - Grades

			if (StartOfRound.Instance.allPlayersDead) 
			{
				efficiency = 0;
				Plugin.Log.LogInfo($"efficiency: 0 (All Players Lost)");
			}
			else 
			{
            	scrapValueRate = scrapCollected / RoundManager.Instance.totalScrapValueInLevel;
            	totalScrapObjects = Utility.GradingInfo.totalScrapObjects;
				scrapObjectRate = (double) Utility.GradingInfo.scrapObjectsCollected / (double)totalScrapObjects;
            	weightedClearRate = valueFactor*scrapValueRate+(1-valueFactor)*scrapObjectRate;
				interiorOffset = interiorOffset*totalScrapObjects/(10*dungeonSize); //Portion of scrap at dungeon size 1 -> quantity of scrap
				totalDungeonClearedBranches = weightedClearRate*dungeonSize;
				totalDungeonClearedMainPath = weightedClearRate*((dungeonSize/((1+2*numofFireExits*fireExitLuck) *.5))+(shipToEntranceTravelTime/mainPathTime));
            	branchDistance = Math.Pow(totalDungeonClearedBranches,1+branchLengthIncreaseWithSize)*(1-stemBranchFactor);
            	mainPathNormalizationFactor = (1/Math.Pow(1.5,numofFireExits))+(shipToEntranceTravelTime/mainPathTime)*(1*((10-interiorOffset)/10)); 
				mainPathDistance = (stemBranchFactor)*(1+2*numofFireExits*fireExitLuck)*(totalDungeonClearedMainPath*((scrapCollected-interiorOffset)/10)/mainPathNormalizationFactor);
				if (mainPathDistance < 0)
				{
					mainPathDistance = 0; //If players fail to get as much scrap as what's directly connected to main entrance (or are really unlucky with spawn positions)
				}
            	efficiency = k*(branchDistance+mainPathDistance)/Math.Pow(numPlayers, groupInefficiency);

            	Plugin.Log.LogInfo($"scrapValueRate: {scrapValueRate}");
				Plugin.Log.LogInfo($"scrapObjectRate: {scrapObjectRate}");
            	Plugin.Log.LogInfo($"WeightedClearRate: {weightedClearRate}");
            	Plugin.Log.LogInfo($"totalDungeonCleared (branches): {totalDungeonClearedBranches}");
				Plugin.Log.LogInfo($"totalDungeonCleared (main path): {totalDungeonClearedMainPath}");
				Plugin.Log.LogInfo($"branchDistance: {branchDistance}");
				Plugin.Log.LogInfo($"mainPathNormalizationFactor: {mainPathNormalizationFactor}");
            	Plugin.Log.LogInfo($"mainPathDistance: {mainPathDistance}");
				Plugin.Log.LogInfo($"efficiency: {efficiency}");

				efficiency = efficiency*(1+(moonDifficultyScalar*moonDifficulty))*interiorDifficulty;
            	Plugin.Log.LogInfo($"efficiencyDifficultyBalanced: {efficiency}");
			}

			if (efficiency < Utility.GradingInfo.gradeThresholds[0]) HUDManager.Instance.statsUIElements.gradeLetter.text = "F";
            else if (efficiency < Utility.GradingInfo.gradeThresholds[1]) HUDManager.Instance.statsUIElements.gradeLetter.text = "D";
            else if (efficiency < Utility.GradingInfo.gradeThresholds[2]) HUDManager.Instance.statsUIElements.gradeLetter.text = "C";
            else if (efficiency < Utility.GradingInfo.gradeThresholds[3]) HUDManager.Instance.statsUIElements.gradeLetter.text = "B";
            else if (efficiency < Utility.GradingInfo.gradeThresholds[4]) HUDManager.Instance.statsUIElements.gradeLetter.text = "A";
			else  HUDManager.Instance.statsUIElements.gradeLetter.text = "S";
		}
	}
}