using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SNCompany {
	//TODO: Config
	//TODO: Exterior scrap? SellBodies? Bees?
	//TODO: StemBranch and interiorOffset for each interior
	//TODO: Scaling with moon difficulty (will not be default)
	//TODO: + and - Grades
    public static class Grading 
	{
		public static int numPlayersAtLanding;
		public static double dungeonSize;
		public static int totalScrapObjects;
		public static int scrapObjectsCollected;
		public static int scrapValueCollected;
		public static int numOfFireExits;

		public static double[] gradeThresholds = [10,40,60,80,100];
		public static double valueFactor = .5;
		public static double fireExitEffect = .25; 		//0 assumes worst possible placement/utilization. 1 Assumes perfect placement and usage
		public static double groupInefficiency = 1-.3;	//Accounts for human nature
		public static double exteriorTime = 3.87;		//Average exterior time spent carrying items to ship when clearing experimentation.
		public static double k = 3; 					//Chosen such that 4 players fully clearing moons can get an S rank for march, offense, and adamance, but not for experimentation, assurance, or vow
		public static double moonDifficultyScalar = 0;	//Not used by default.

		//Currently these values are only for experimentation and facility. Some may need to be found experimentally
		//Interior-dependent
		public static double branchLengthIncreaseWithSize = 0;	//I don't believe DunGen does this. Left in for if an interior ever uses unique generation that requires it
		public static double branchTime = 4.8; 			//Average dungeon time spent exploring branches while fully clearing dungeon size 1, without fire exits 
		public static double mainPathTime = 2.41;		//Average dungeon time spent carrying items down main path 
		public static double interiorOffset = .24; 		//Portion of scrap contained in branches that directly connect to main entrance at dungeon size 1. (No main path traversal required) 
		public static double interiorDifficulty = 1;	//Not used by default

		public static double moonDifficulty = 0;		//Not used by default.
		public static string moonName = string.Empty;
		public static double shipToEntranceTravelTime;

		public static int numPlayersAtTakeoff;
		public static double numPlayers;
		public static double scrapValueRate;
		public static double scrapObjectRate;
		public static double weightedClearRate;
		public static double totalDungeonClearedBranches;
		public static double totalDungeonClearedMainPath;
		public static double numScrapInteriorOffset;
		public static double branchDistance;
		public static double mainPathDistance;
		public static double mainPathNormalization; 	//Ensures mainPathDistance is equal to 1 when fully clearing experimentation.
		public static double moonPathDistance;
		public static double totalDistanceCovered;
		public static double efficiency;
		public static double efficiencyAdjusted;

		public static string debug = string.Empty;

        public static Dictionary<string, double> shipToEntranceTimes = new Dictionary<string, double>
            {
				//Experimentally measured. Time from ship to entrance and back. No equipment/vehicles.
                {"experimentation", 50},
				{"assurance", 56.3},
				{"vow", 61.15},
				{"offense", 57.25},
				{"march", 50.375},
				{"adamance", 56.5},
				{"rend", 66.5},
				{"dine", 50.6},
				{"titan", 37},
				{"artifice", 50},
				{"embrion", 67},
				{"unknown", 54.79}
            };
		public static double experimentationTravelTime;

		static Grading() {
			experimentationTravelTime = shipToEntranceTimes["experimentation"];
			
			numPlayers = 4;
			shipToEntranceTravelTime = shipToEntranceTimes["offense"];
			dungeonSize = 1.2;
			numOfFireExits = 1;
			weightedClearRate = 1;
			scrapObjectsCollected = 14;
			totalScrapObjects = 14;

			//interiorOffset =
			//mainPathTime =
			//branchTime =

			efficiency = CalculateEfficiencyPerPlayer();
			k = 100/totalDistanceCovered/Math.Pow(4, groupInefficiency);
			Plugin.Log.LogInfo($"Calculated k: {k}");
		}

		public static void PrepareForEfficiencyCalculation(int scrapCollected) {
			numPlayersAtTakeoff = RoundManager.Instance.playersManager.connectedPlayersAmount + 1;
			numPlayers = ((double)numPlayersAtTakeoff+(double)numPlayersAtLanding)/2;

			moonName = new string(StartOfRound.Instance.currentLevel.PlanetName.SkipWhile((char c) => !char.IsLetter(c)).ToArray());
			Plugin.Log.LogInfo($"Moon: {moonName}\n");
			if (shipToEntranceTimes.ContainsKey(moonName)) shipToEntranceTravelTime = shipToEntranceTimes[moonName];
			else shipToEntranceTravelTime = shipToEntranceTimes["unknown"];

			scrapValueCollected = scrapCollected;
			scrapValueRate = (double)scrapValueCollected / (double)RoundManager.Instance.totalScrapValueInLevel;
			scrapObjectRate = (double) scrapObjectsCollected / (double)totalScrapObjects;
			weightedClearRate = valueFactor*scrapValueRate+(1-valueFactor)*scrapObjectRate;

			//interiorOffset =
			//mainPathTime =
			//branchTime =
		}

		public static void LogGradingVariables() {
			debug = $"dungeonSize: {dungeonSize}\n";
			debug += $"numofFireExits: {numOfFireExits}\n";
			debug += $"numPlayersAtTakeoff: {numPlayersAtTakeoff}\n";
			debug += $"numPlayersAtLanding: {numPlayersAtLanding}\n";
			debug += $"numPlayers: {numPlayers}\n";
			debug += $"moonName: {moonName}\n";
			debug += $"shipToEntranceTravelTime: {shipToEntranceTravelTime}\n";
			debug += $"scrapValueRate: {scrapValueRate}\n";
			debug += $"scrapObjectRate: {scrapObjectRate}\n";
			debug += $"weightedClearRate: {weightedClearRate}\n";
			debug += $"valueFactor: {valueFactor}\n";
			debug += $"branchLengthIncreaseWithSize: {branchLengthIncreaseWithSize}\n";
			debug += $"branchTime: {branchTime}\n";
			debug += $"mainPathTime: {mainPathTime}\n";
			debug += $"exteriorTime: {exteriorTime}\n";
			debug += $"interiorOffset: {interiorOffset}\n";
			debug += $"experimentationTravelTime: {experimentationTravelTime}\n";
			debug += $"fireExitEffect: {fireExitEffect}\n";
			debug += $"groupInefficiency: {groupInefficiency}\n";
            debug += $"moonDifficultyScalar: {moonDifficultyScalar}\n";
			debug += $"moonDifficulty: {moonDifficulty}\n";
            debug += $"interiorDifficulty: {interiorDifficulty}\n";
			debug += $"constant k: {k}\n";
			Plugin.Log.LogInfo(debug);
		}

        public static double CalculateEfficiencyPerPlayer() {
            //Assumes all interiors are balanced to take an equal amount of time to clear at a dungeon size of 1.
			// - If not true, users should restrict their dungeon sizes through LLL.

			//These equations result in an 'efficiency' (can be thought of as distance traveled or time spent) per player. The 3 "distance"
			//equations simply establish the correct relationships between each one's relevant variables, and do not initially have well-defined 
			//units. A chosen set of conditions (fully clearing a facility interior on experimentation) is used to experimentally determine 
			//constants. The main path, branch, and exterior distances are normalized by themselves with experimentation's moon-specific 
			//values plugged in, then multiplied by these constants (average time spent on each kind of distance for experimentation)
			//to obtain the experimentally determined balance between them, essentially hard-coding a starting point. 
			//Beyond this point, the relationships established by the equations will alter the result appropriately.

			totalDungeonClearedBranches = weightedClearRate*dungeonSize;
			branchDistance = Math.Pow(totalDungeonClearedBranches,1+branchLengthIncreaseWithSize);

			totalDungeonClearedMainPath = weightedClearRate*(dungeonSize/(1+2*numOfFireExits*fireExitEffect));
			numScrapInteriorOffset = interiorOffset*totalScrapObjects/dungeonSize; 
			mainPathNormalization = 1*(1/(1+2*1*fireExitEffect))*(10-(interiorOffset*10/1)); 
			mainPathDistance = totalDungeonClearedMainPath*(scrapObjectsCollected-numScrapInteriorOffset)/mainPathNormalization;
			if (mainPathDistance < 0) mainPathDistance = 0;

			moonPathDistance = shipToEntranceTravelTime/experimentationTravelTime*scrapObjectsCollected/10;

			totalDistanceCovered = branchTime*branchDistance+mainPathTime*mainPathDistance+exteriorTime*moonPathDistance;
			efficiency = k*totalDistanceCovered/Math.Pow(numPlayers, groupInefficiency);
			efficiencyAdjusted = efficiency*(1+(moonDifficultyScalar*moonDifficulty))*interiorDifficulty;

			return efficiencyAdjusted;
        }

		public static void LogGradingCalculations() {
			debug += $"totalDungeonCleared (branches): {totalDungeonClearedBranches}\n";
			debug += $"branchDistance: {branchDistance}\n";
			debug += $"totalDungeonCleared (main path): {totalDungeonClearedMainPath}\n";
			debug += $"numScrapInteriorOffset: {numScrapInteriorOffset}\n";
			debug += $"mainPathNormalizationFactor: {mainPathNormalization}\n";
			debug += $"mainPathDistance: {mainPathDistance}\n";
			debug += $"moonPathDistance: {moonPathDistance}\n";
			debug += $"efficiency: {efficiency}\n";
			debug += $"efficiencyDifficultyAdjusted: {efficiencyAdjusted}\n";
			Plugin.Log.LogInfo(debug);
		}

		public static int FindNumOfFireExits()
		{
			int num = 0;
			//StartOfRound.Instance.NetworkManager.
			Scene scene = SceneManager.GetSceneByName(StartOfRound.Instance.currentLevel.sceneName);
			GameObject[] rootGameObjects = scene.GetRootGameObjects();
			foreach (GameObject gameObject in rootGameObjects)
			{
				EntranceTeleport[] componentsInChildren = gameObject.GetComponentsInChildren<EntranceTeleport>();
				num += componentsInChildren.Length;
			}
			return (num/2)-1;
		}
	}
}