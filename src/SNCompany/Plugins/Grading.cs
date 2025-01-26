using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SNCompany {
	//TODO: Make exterior objects only count towards value
	//TODO: Time distributions for each interior
	//TODO: Scaling with moon difficulty (will not be default)
	//TODO: + and - Grades
    public static class Grading 
	{
		public static int numPlayersAtTakeoff = 4;
		public static int numPlayersAtLanding = 4;
		public static double dungeonSize;
		public static int totalScrapObjects; 			//Only interior scrap
		public static int scrapObjectsCollected; 		//Includes exterior scrap
		public static int scrapValueCollected = 1000; 	//It seems like this is the same. Total -> interior. Collected -> Any
		public static int numOfFireExits;

		public static double valueFactor = .5;
		public static double fireExitEffect = .5; 		//0 assumes worst possible placement/utilization. 1 Assumes perfect placement and usage
		public static double groupInefficiency = 1-.3;	//Accounts for human nature
		public static double exteriorTime = 3.87;		//Average exterior time spent carrying items to ship when clearing experimentation.
		public static double k = 5.42; 					//Calculated such that 4 players fully clearing moons can get an S rank for march, offense, and adamance, but not for experimentation, assurance, or vow
		public static double moonDifficultyScalar = 0;	//Not used by default.

		//Currently these values are only for experimentation and facility. Some may need to be found experimentally
		//Interior-dependent
		public static double branchLengthIncreaseWithSize = 0;	//I don't believe vanilla DunGen does this. Left in for modded interiors with unique generation
		public static double branchTime = 4.8; 			//Average dungeon time spent exploring branches while fully clearing dungeon size 1, without fire exits 
		public static double mainPathTime = 2.41;		//Average dungeon time spent carrying items down main path for dungeon size 1 with average scrap of 10. 
		public static double interiorOffset = .24; 		//Portion of scrap contained in branches that directly connect to main entrance at dungeon size 1. (No main path traversal required) 
		public static double interiorDifficulty = 1;	//Not used by default

		//Moon-dependent
		public static double moonDifficulty = 0;		//Not used by default
		public static string moonName = string.Empty;
		public static double shipToEntranceTravelTime;

		public static double experimentationTravelTime;

		public static double numPlayers;
		public static double scrapValueRate;
		public static double scrapObjectRate;
		public static double weightedClearRate;
		public static double totalDungeonClearedBranches;
		public static double totalDungeonClearedMainPath;
		public static double numScrapInteriorOffset;
		public static double branchDistance;
		public static double mainPathDistance;
		public static double mainPathNormalization;
		public static double moonPathDistance;
		public static double totalDistanceCovered;
		public static double efficiency;
		public static double efficiencyAdjusted;

		public static string debug = string.Empty;
		public static string grade = string.Empty;

        public static Dictionary<string, double> shipToEntranceTimes = new Dictionary<string, double>
            {
				//Experimentally measured. Average time from ship to entrance and back, all entrances. No equipment/vehicles.
                {"Experimentation", 50},
				{"Assurance", 56.3},
				{"Vow", 61.15},
				{"Offense", 57.25},
				{"March", 50.375},
				{"Adamance", 56.5},
				{"Rend", 66.5},
				{"Dine", 50.6},
				{"Titan", 37},
				{"Artifice", 50},
				{"Embrion", 67},
				{"Unknown", 54.79}
            };

		public static Dictionary<string, int[]> grades = new Dictionary<string, int[]>
			{
				{"F", [0, 0, 0]},
				{"D", [10, 15, 50]},
				{"C", [40, 25, 100]},
				{"B", [60, 50, 250]},
				{"A", [80, 75, 400]},
				{"S", [100, 90, 600]}
			};
		//Indexes for grades' arrays
		static readonly int THRESHOLD = 0;
		static readonly int PERCENTSUBSIDY = 1;
		static readonly int AMOUNTSUBSIDY = 2;

		static Grading() {
			if(!shipToEntranceTimes.ContainsKey("Experimentation") || !shipToEntranceTimes.ContainsKey("Offense")) 
			{
				experimentationTravelTime = 50;
				shipToEntranceTravelTime = 57.25;
				Plugin.Log.LogError("Cannot find keys in dictionary");
			}
			else 
			{
				experimentationTravelTime = shipToEntranceTimes["Experimentation"];
				shipToEntranceTravelTime = shipToEntranceTimes["Offense"];
			}

			valueFactor = Plugin.BoundConfig.valueFactor.Value;
			groupInefficiency = 1-Plugin.BoundConfig.groupInefficiency.Value;
			fireExitEffect = Plugin.BoundConfig.fireExitEffect.Value;
			
			//Runs efficiency calculation with the S threshold data, to determine constant k needed to reach the S threshold
			numPlayers = 4;
			dungeonSize = 1.2;
			numOfFireExits = 1;
			weightedClearRate = 1;
			scrapObjectsCollected = 14;
			totalScrapObjects = 14;
			k = 1;
			//interiorOffset =
			//mainPathTime =
			//branchTime =

			efficiency = CalculateEfficiencyPerPlayer();
			k = grades["S"][THRESHOLD]/efficiency;
			Plugin.Log.LogDebug($"{SubsidyNetworkHandler.WhoAmI()}: Calculated k: {k}");
		}

		public static void PrepareForEfficiencyCalculation(int scrapCollected) {
			numPlayersAtTakeoff = RoundManager.Instance.playersManager.connectedPlayersAmount + 1;
			numPlayers = ((double)numPlayersAtTakeoff+(double)numPlayersAtLanding)/2;

			moonName = new string(StartOfRound.Instance.currentLevel.PlanetName.SkipWhile((char c) => !char.IsLetter(c)).ToArray());
			Plugin.Log.LogDebug($"Moon: {moonName}\n");
			if (shipToEntranceTimes.ContainsKey(moonName)) shipToEntranceTravelTime = shipToEntranceTimes[moonName];
			else shipToEntranceTravelTime = shipToEntranceTimes["Unknown"];

			scrapValueCollected = scrapCollected;
			scrapValueRate = (double)scrapValueCollected / (double)RoundManager.Instance.totalScrapValueInLevel;
			scrapObjectRate = (double) scrapObjectsCollected / (double)totalScrapObjects;
			weightedClearRate = valueFactor*scrapValueRate+(1-valueFactor)*scrapObjectRate;
			//interiorOffset =
			//mainPathTime =
			//branchTime =
		}

		public static void LogGradingVariables() {
			debug = $"{SubsidyNetworkHandler.WhoAmI()}: \ndungeonSize: {dungeonSize}\n";
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
			Plugin.Log.LogDebug(debug);
		}

        public static double CalculateEfficiencyPerPlayer() {
            //Assumes all interiors are balanced to take an equal amount of time to clear at a dungeon size of 1.
			// - If not true, users should probably restrict their dungeon sizes through LLL's config.

			//These equations result in an 'efficiency' (can be thought of as distance traveled or time spent) per player. The 3 "distance"
			//equations each establish the correct relationships between thier relevant variables, but do not initially have well-defined 
			//units. A chosen set of conditions (fully clearing a facility interior on experimentation) is used to experimentally determine 
			//constants. The main path, branch, and exterior distances are normalized by themselves with experimentation's moon-specific 
			//values plugged in, then multiplied by these constants (average time spent on each kind of distance for experimentation)
			//to obtain the experimentally determined balance between them, essentially hard-coding a starting point. 
			//Beyond this point, as conditions change, the relationships established by the equations will alter the result appropriately.

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
			debug = $"\ntotalDungeonCleared (branches): {totalDungeonClearedBranches}\n";
			debug += $"branchDistance: {branchDistance}\n";
			debug += $"totalDungeonCleared (main path): {totalDungeonClearedMainPath}\n";
			debug += $"numScrapInteriorOffset: {numScrapInteriorOffset}\n";
			debug += $"mainPathNormalizationFactor: {mainPathNormalization}\n";
			debug += $"mainPathDistance: {mainPathDistance}\n";
			debug += $"moonPathDistance: {moonPathDistance}\n";
			debug += $"totalDistanceCovered: {totalDistanceCovered}\n";
			debug += $"efficiency: {efficiency}\n";
			debug += $"efficiencyDifficultyAdjusted: {efficiencyAdjusted}\n";
			Plugin.Log.LogDebug(debug);
		}

		public static void RewardEfficiency(double efficiency) {
			if (efficiency >= grades["S"][THRESHOLD]) grade = "S";
			else if (efficiency >= grades["A"][THRESHOLD]) grade = "A";
			else if (efficiency >= grades["B"][THRESHOLD]) grade = "B";
			else if (efficiency >= grades["C"][THRESHOLD]) grade = "C";
			else if (efficiency >= grades["D"][THRESHOLD]) grade = "D";
			else grade = "F";
            
			SubsidyNetworkHandler.Instance.SetSubsidyParameters(grades[grade][PERCENTSUBSIDY], grades[grade][AMOUNTSUBSIDY]);
			HUDManager.Instance.statsUIElements.gradeLetter.text = grade;
			Plugin.Log.LogDebug($"{SubsidyNetworkHandler.WhoAmI()}: Grade: {grade}");
			//Subsidy.SubsidizeAllMoons(grades[grade][PERCENTSUBSIDY], grades[grade][AMOUNTSUBSIDY]);
		}

		public static int FindNumOfFireExits()
		{
			//Code partially sourced from IAmBatby's LethalLevelLoader
			int num = 0;
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