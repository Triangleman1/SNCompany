using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using Dissonance;

[HarmonyPatch(typeof(MenuManager), "Awake")]
[HarmonyPostfix]
[HarmonyPriority(1)]
public static void Awake_Postfix(MenuManager __instance)
{
	//IL_003c: Unknown result type (might be due to invalid IL or missing references)
	//IL_004b: Unknown result type (might be due to invalid IL or missing references)
	//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
	if (__instance.isInitScene)
	{
		return;
	}
	try
	{
		Sprite sprite = Sprite.Create(Plugin.mainLogo, new Rect(0f, 0f, (float)((Texture)Plugin.mainLogo).width, (float)((Texture)Plugin.mainLogo).height), new Vector2(0.5f, 0.5f));
		GameObject gameObject = ((Component)((Component)__instance).transform.parent).gameObject;
		Transform val = gameObject.transform.Find("MenuContainer/MainButtons/HeaderImage");
		//Maybe he meant system.object?
		if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
		{
			((Component)val).gameObject.GetComponent<Image>().sprite = sprite;
		}
		Transform val2 = gameObject.transform.Find("MenuContainer/LoadingScreen");
		if ((UnityEngine.Object)(object)val2 != (UnityEngine.Object)null)
		{
			val2.localScale = new Vector3(1.02f, 1.06f, 1.02f);
			Transform val3 = val2.Find("Image");
			if ((UnityEngine.Object)(object)val3 != (UnityEngine.Object)null)
			{
				((Component)val3).GetComponent<Image>().sprite = sprite;
			}
		}
	}
	catch (Exception ex)
	{
		Plugin.Log.LogError((object)ex);
	}
}