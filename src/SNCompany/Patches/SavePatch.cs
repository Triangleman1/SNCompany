using HarmonyLib;

namespace SNCompany.Patches
{
    [HarmonyPatch]
    static class SavePatch
    {
        [HarmonyPatch(typeof(GameNetworkManager), "SaveGame")]
        [HarmonyPrefix]
        public static void PrepareForSave()
        {
            SNSave.Save();
        }
    }
}