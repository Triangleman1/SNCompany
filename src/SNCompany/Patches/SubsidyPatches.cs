using HarmonyLib;
using System.Linq;
using LethalLevelLoader;
using LethalModDataLib.Features;

namespace SNCompany.Patches 
{
    [HarmonyPatch]
	static class SubsidyPatches 
    {
        [HarmonyPriority(100)]
		[HarmonyPatch(typeof(StartOfRound), "Start")]
		[HarmonyPostfix]
        public static void InitializeLevels() 
        {
            SNSave.Load();
            Subsidy.globalSubsidized = false;
            Subsidy.SubsidizeAllMoons();
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
            SNLevel SNLevel;
            foreach (var entry in Subsidy.SNLevels) 
            {
                SNLevel = entry.Value;
                if (SNLevel.extendedLevel.SelectableLevel.levelID == levelID) 
                {
                    Plugin.Log.LogDebug($"Routed to {SNLevel.extendedLevel.SelectableLevel.sceneName}. Calculating remaining subsidy.");
                    if (SNLevel.subsidy == -1) {
                        Plugin.Log.LogError($"Last routed planet had no stored subsidy. Cannot resubsidize.");
                        return;
                    }
                    Subsidy.amountSubsidy -= SNLevel.subsidy;
			        Subsidy.SubsidizeAllMoons();
                    break;
                }
            }
		}
    }
}