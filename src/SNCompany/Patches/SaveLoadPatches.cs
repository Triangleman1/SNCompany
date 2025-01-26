using HarmonyLib;

namespace SNCompany.Patches
{
    [HarmonyPatch]
    static class SaveLoadPatches
    {
        [HarmonyPatch(typeof(GameNetworkManager), "SaveGame")]
        [HarmonyPrefix]
        public static void PrepareForSave()
        {
            SNSave.Save();
        }

        [HarmonyPriority(100)]
		[HarmonyPatch(typeof(StartOfRound), "Start")]
		[HarmonyPostfix]
        public static void Initialize() 
        {
            SNLevelManager.InitializeLevels();
            if (SNNetworkHandler.Instance.IsServer) 
            {
                SNSave.Load();
            }
            else 
            {
                //SNNetworkHandler.Instance.RequestSNLevelsServerRpc();
            }
            if (Subsidy.globalSubsidized) Subsidy.SubsidizeAllMoons();
        }
    }
}