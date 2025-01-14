using HarmonyLib;

namespace SNCompany.Patches 
{
[HarmonyPatch]
	static class Penalties {
		[HarmonyPatch(typeof(HUDManager), "ApplyPenalty")]
		[HarmonyPrefix]
		public static bool RemovePenalty() {
			if (Plugin.BoundConfig.removeFines.Value == true) {
				HUDManager.Instance.statsUIElements.penaltyAddition.text = "COMPANY PROPERTY LOST\n\nYOU ARE UNSATISFACTORY ASSETS";
				HUDManager.Instance.statsUIElements.penaltyTotal.text = "DUE: 0";
				Plugin.Log.LogDebug($"Removed Fine");
				return false;
			}
			else return true;
		}
    }
}