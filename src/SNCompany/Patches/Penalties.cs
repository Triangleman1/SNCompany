using HarmonyLib;

namespace SNCompany.Patches 
{
[HarmonyPatch]
	static class Penalties {
		[HarmonyPatch(typeof(HUDManager), "ApplyPenalty")]
		[HarmonyPrefix]
		public static bool RemovePenalty() {
			HUDManager.Instance.statsUIElements.penaltyAddition.text = "You guys suck\nStop dying";
    		HUDManager.Instance.statsUIElements.penaltyTotal.text = "DUE: 0";
			Plugin.Log.LogInfo($"Removed Fine");
			return false;
		}
    }
}