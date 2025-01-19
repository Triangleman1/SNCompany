using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Reflection.Emit;
using System.Data;
using LethalLevelLoader;
using LethalModDataLib.Features;

namespace SNCompany.Patches 
{
    [HarmonyPatch]
	static class SubsidyPatches 
    {
        [HarmonyPriority(100)]
		[HarmonyPatch(typeof(StartOfRound), "Awake")]
		[HarmonyPostfix]
        public static void InitializeLevels() 
        {
            foreach(ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels) 
            {
                SNLevel SNLevel;
                string moonName = extendedLevel.SelectableLevel.sceneName;
                if (!Subsidy.SNLevels.ContainsKey(moonName)) 
                {
                    SNLevel = new SNLevel(extendedLevel);
                    Subsidy.SNLevels.Add(moonName, SNLevel);
                    ModDataHandler.RegisterInstance(SNLevel, moonName);
                }
                //TODO: Remove moons if they're no longer present
            }
        }

        [HarmonyPatch(typeof(StartOfRound), "openingDoorsSequence")]
		[HarmonyPostfix]
        [HarmonyPriority(2000)]
		public static void UndoSubsidies() 
        {
			Subsidy.UnsubsidizeAllMoons();
		}

        [HarmonyPatch(typeof(StartOfRound), "ChangeLevelClientRpc")]
		[HarmonyPrefix]
		public static void SaveRoutePrice() 
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
                    Subsidy.amountSubsidy -= SNLevel.subsidy;
			        Subsidy.SubsidizeAllMoons();
                    break;
                }
            }
		}

        /*
        [HarmonyPostfix]
		[HarmonyPatch(typeof(Terminal), "LoadNewNode")]
        [HarmonyPriority(-100)]
		public static void SetDisplayPrice(Terminal __instance, TerminalNode node)
		{
			if (node.buyRerouteToMoon == -2 && node.buyVehicleIndex == -1)
			{
				int discountedPrice = Subsidy.CalculateSubsidizedPrice(node.itemCost);
                Plugin.Log.LogDebug($"discountedPrice: {discountedPrice.ToString()}");
                Plugin.Log.LogDebug($"node.itemCost: {node.itemCost.ToString()}");
                Plugin.Log.LogDebug($"Old Screen Text: {__instance.screenText.text}");
                Plugin.Log.LogDebug($"Old Current Text: {__instance.currentText}");
                __instance.currentText =__instance.currentText.Replace(node.itemCost.ToString(), discountedPrice.ToString());
				__instance.screenText.text = __instance.currentText;
                Plugin.Log.LogDebug($"New Screen Text: {__instance.screenText.text}");
                Plugin.Log.LogDebug($"New Current Text: {__instance.currentText}");
                Plugin.Log.LogDebug($"Does contain node.itemCost: {__instance.currentText.Contains(node.itemCost.ToString())}");
                Plugin.Log.LogDebug($"Does contain 1500: {__instance.currentText.Contains("1500")}");
			}
		}

        [HarmonyTranspiler]
		[HarmonyPatch(typeof(Terminal), "LoadNewNodeIfAffordable")]
        [HarmonyPriority(-100)]
		public static IEnumerable<CodeInstruction> AllowRoutePurchase(IEnumerable<CodeInstruction> instructions)
		{
			Plugin.Log.LogDebug($"Attempting to subsidize purchase");
            FieldInfo itemCost = typeof(TerminalNode).GetField("itemCost");
			MethodInfo CalculateSubsidizedPrice = typeof(Subsidy).GetMethod("CalculateSubsidizedPrice");
            int instructionIndex = 0;
            int usageIndex = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            while (instructionIndex < instructions.Count()) 
            {
                if (codes[instructionIndex].opcode == OpCodes.Ldfld && (FieldInfo)codes[instructionIndex].operand == itemCost) 
                {
                    usageIndex++;
                    Plugin.Log.LogDebug($"Found usage of itemCost: {usageIndex}");
                    if (usageIndex == 3) 
                    {
                        codes.Insert(instructionIndex + 1, new CodeInstruction(OpCodes.Call, CalculateSubsidizedPrice));
                        Plugin.Log.LogDebug("Subsidized route purchase");
                    }
                }
                instructionIndex++;
            }
			return codes;
		}
        */
        
    }
}


    