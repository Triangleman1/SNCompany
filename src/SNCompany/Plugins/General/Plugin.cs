using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace SNCompany
{
	[BepInPlugin(LCMPluginInfo.PLUGIN_GUID, LCMPluginInfo.PLUGIN_NAME, LCMPluginInfo.PLUGIN_VERSION)]
	[BepInDependency("imabatby.lethallevelloader", "1.4.0")]
	[BepInDependency("MaxWasUnavailable.LethalModDataLib", "1.2.0")] 
	public class Plugin : BaseUnityPlugin
	{
		public static ManualLogSource Log = null!;
		internal static SNConfig BoundConfig { get; private set; } = null!;
		public static Texture2D mainLogo = null!;
		
		private void Awake()
		{
			Log = Logger;
			Harmony val = new Harmony(LCMPluginInfo.PLUGIN_GUID);
			BoundConfig = new SNConfig(base.Config); 

			NetcodePatcher();

			if (BoundConfig.vandalizeLogo.Value == true) {
				AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(((BaseUnityPlugin)this).Info.Location), "sncompany.assetbundle"));
				if (assetBundle != null) 
				{
					mainLogo = assetBundle.LoadAsset<Texture2D>("ShawnNewman.png");
				}
				else 
				{
					Log.LogError("Failed to load bundle");
				}
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

		private static void NetcodePatcher()
		{
			var types = Assembly.GetExecutingAssembly().GetTypes();
			foreach (var type in types)
			{
				var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
				foreach (var method in methods)
				{
					var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
					if (attributes.Length > 0)
					{
						method.Invoke(null, null);
					}
				}
			}
		}
	}
}