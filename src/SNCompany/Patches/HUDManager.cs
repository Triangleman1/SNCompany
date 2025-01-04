using System;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;

namespace SNCompany.Patches 
{
[HarmonyPatch]
	static class HUD {
		[HarmonyPatch(typeof(HUDManager), "ApplyPenalty")]
		[HarmonyPrefix]
		public static bool RemovePenalty() {
			HUDManager.Instance.statsUIElements.penaltyAddition.text = "You guys suck\nStop dying";
    		HUDManager.Instance.statsUIElements.penaltyTotal.text = "DUE: 0";
			Plugin.Log.LogInfo($"Removed Fine");
			return false;
		}

		[HarmonyPatch(typeof(HUDManager), "FillEndGameStats")]
		[HarmonyPostfix]
		public static void FairGrading() {
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
			int numPlayersAtTakeoff = HUDManager.Instance.playersManager.allPlayerScripts.Length;
            int numPlayersAtLanding = Plugin.GradingInfo.playersAtRoundStart;
            double numPlayers = ((double)numPlayersAtTakeoff + (double)numPlayersAtLanding) / 2;
			double valueFactor = .4;
            double stemBranchFactor = 1; //Portion of dungeon that is main path (as opposed to branches) assuming optimal exploration, out of 2.
			double interiorOffset = 0;
			double moonOffset = 0;
            double branchLengthIncreaseWithSize = 0;
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
            // [U] Plugin.Log.LogInfo($"moonDifficultyScalar: {moonDifficultyScalar}");
			// [U] Plugin.Log.LogInfo($"moonDifficulty: {moonDifficulty}");
            // [U] Plugin.Log.LogInfo($"interiorDifficulty: {interiorDifficulty}");

            scrapValueRate = RoundManager.Instance.scrapCollectedInLevel / RoundManager.Instance.totalScrapValueInLevel;
            scrapObjectRate = Plugin.GradingInfo.totalScrapObjects / Plugin.GradingInfo.scrapObjectsCollected;
            weightedClearRate = 0.5*(valueFactor*scrapValueRate+(1-valueFactor)*scrapObjectRate);
            totalDungeonCleared = weightedClearRate*(dungeonSize-moonOffset-interiorOffset);
            branchDistance = totalDungeonCleared*((totalDungeonCleared*branchLengthIncreaseWithSize)*(2-stemBranchFactor));
            //Relationship between increasing dungeon size and the exponentially increasing distance players must travel to clear it
            mainPathDistance = (totalDungeonCleared)*(((totalDungeonCleared*stemBranchFactor*stemBranchFactor)-stemBranchFactor)/2);
            efficiency = 220*(branchDistance+mainPathDistance)/numPlayers;
            // [U] efficiencyBalanced = efficiencyScaled*(1+(moonDifficultyFactor*moonDifficulty))*interiorDifficulty

            Plugin.Log.LogInfo($"scrapValueRate: {scrapValueRate}");
			Plugin.Log.LogInfo($"scrapObjectRate: {scrapObjectRate}");
            Plugin.Log.LogInfo($"WeightedClearRate: {weightedClearRate}");
            Plugin.Log.LogInfo($"totalDungeonCleared: {totalDungeonCleared}");
			Plugin.Log.LogInfo($"branchDistance: {branchDistance}");
            Plugin.Log.LogInfo($"mainPathDistance: {mainPathDistance}");
            Plugin.Log.LogInfo($"efficiency: {efficiency}");
            // [U] Plugin.Log.LogInfo($"efficiencyBalanced: {efficiencyBalanced}");

            if (efficiency < Plugin.GradingInfo.DThreshold) HUDManager.Instance.statsUIElements.gradeLetter.text = "F";
            else if (efficiency < Plugin.GradingInfo.CThreshold) HUDManager.Instance.statsUIElements.gradeLetter.text = "D";
            else if (efficiency < Plugin.GradingInfo.BThreshold) HUDManager.Instance.statsUIElements.gradeLetter.text = "C";
            else if (efficiency < Plugin.GradingInfo.AThreshold) HUDManager.Instance.statsUIElements.gradeLetter.text = "B";
            else if (efficiency < Plugin.GradingInfo.SThreshold) HUDManager.Instance.statsUIElements.gradeLetter.text = "A";
            else  HUDManager.Instance.statsUIElements.gradeLetter.text = "S";
		}
	}
}