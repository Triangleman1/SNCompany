using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;
using UnityEngine.Rendering.HighDefinition;

namespace SNCompany
{
	[BepInPlugin(LCMPluginInfo.PLUGIN_GUID, LCMPluginInfo.PLUGIN_NAME, LCMPluginInfo.PLUGIN_VERSION)]
	public class Plugin : BaseUnityPlugin
	{
		public static ManualLogSource Log = null!;
		public static Texture2D mainLogo = null!;
		private void Awake()
		{
			Log = Logger;
			Harmony val = new Harmony(LCMPluginInfo.PLUGIN_GUID);

			AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(((BaseUnityPlugin)this).Info.Location), "sncompany.assetbundle"));
			if (assetBundle != null) 
			{
				mainLogo = assetBundle.LoadAsset<Texture2D>("ShawnNewman.png");
			}
			else 
			{
				Log.LogError("Failed to load bundle");
			}

			try
			{
				val.PatchAll();
				
			}	
			catch (Exception ex)
			{
				Log.LogError((object)("Failed to patch: " + ex));
			}

			Log.LogInfo($"Plugin {LCMPluginInfo.PLUGIN_NAME} is loaded!");
		}
		public struct GradingInfo
		{
			public static int playersAtRoundStart;
			public static double dungeonLengthAtGeneration;
			public static int totalScrapObjects;
			public static int scrapObjectsCollected;
			public static double SThreshold = 100;
			public static double AThreshold = 50;
			public static double BThreshold = 30;
			public static double CThreshold = 20;
			public static double DThreshold = 10;
		}
	}
}