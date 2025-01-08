using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;
using UnityEngine.SceneManagement;

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
	}

	public class Utility 
	{
		public struct GradingInfo
		{
			public static int playersAtRoundStart;
			public static double MoonInteriorMapSize;
			public static int totalScrapObjects;
			public static int scrapObjectsCollected;
			public static int numOfFireExits;
			public static double[] gradeThresholds = [10,40,60,80,100];
		}

		public static int FindNumOfFireExits()
		{
			int num = 0;
			//StartOfRound.Instance.NetworkManager.
			Scene scene = SceneManager.GetSceneByName(StartOfRound.Instance.currentLevel.sceneName);
			GameObject[] rootGameObjects = scene.GetRootGameObjects();
			foreach (GameObject gameObject in rootGameObjects)
			{
				EntranceTeleport[] componentsInChildren = gameObject.GetComponentsInChildren<EntranceTeleport>();
				num += componentsInChildren.Length;
			}
			return (num/2)-1;
		}

	}
}