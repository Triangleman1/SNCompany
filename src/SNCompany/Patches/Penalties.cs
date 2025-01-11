using HarmonyLib;

namespace SNCompany.Patches 
{
[HarmonyPatch]
	static class Penalties {
		[HarmonyPatch(typeof(HUDManager), "ApplyPenalty")]
		[HarmonyPrefix]
		public static bool RemovePenalty() {
			HUDManager.Instance.statsUIElements.penaltyAddition.text = "COMPANY PROPERTY WAS LOST\nYOU ARE UNSATISFACTORY ASSETS";
    		HUDManager.Instance.statsUIElements.penaltyTotal.text = "DUE: 0";
			Plugin.Log.LogInfo($"Removed Fine");
			return false;
		}
    }
}