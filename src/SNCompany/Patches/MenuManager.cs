using System;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;

namespace SNCompany.Patches 
{
	[HarmonyPatch]
	static class ReplaceLogo 
	{
		[HarmonyPatch(typeof(MenuManager), "Awake")]
		[HarmonyPostfix]
		//Lower Harmony priority means code runs after higher priority patches. MoreCompany's priority is 0.
		[HarmonyPriority(-100)]
		public static void MainMenuLogo(MenuManager __instance)
		{
			if (__instance.isInitScene)
			{
				return;
			}
			try
			{
				//This code is shamelessly stolen from MoreCompany
				Sprite logoImage = Sprite.Create(Plugin.mainLogo, new Rect(0, 0, Plugin.mainLogo.width, Plugin.mainLogo.height), new Vector2(0.5f, 0.5f));
                GameObject parent = __instance.transform.parent.gameObject;
                
				Transform mainLogo = parent.transform.Find("MenuContainer/MainButtons/HeaderImage");
                if (mainLogo != null)
                {
					mainLogo.gameObject.GetComponent<Image>().sprite = logoImage;
                }

                Transform loadingScreen = parent.transform.Find("MenuContainer/LoadingScreen");
                if (loadingScreen != null)
                {
                    loadingScreen.localScale = new Vector3(1.02f, 1.06f, 1.02f);
                    Transform loadingLogo = loadingScreen.Find("Image");
                    if (loadingLogo != null)
                    {
                        loadingLogo.GetComponent<Image>().sprite = logoImage;
                    }
                }
			}
			catch (Exception ex)
			{
				Plugin.Log.LogError((object)ex);
			}
		}
	}

	[HarmonyPatch]
	static class HUD {
		[HarmonyPatch(typeof(HUDManager), "ApplyPenalty")]
		[HarmonyPrefix]
		public static bool RemovePenalty() {
			HUDManager.Instance.statsUIElements.penaltyAddition.text = "You guys suck\nStop dying";
    		HUDManager.Instance.statsUIElements.penaltyTotal.text = "DUE: 0";
			return false;
		}

	}
}