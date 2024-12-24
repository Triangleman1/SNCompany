using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;

/*
  Here are some basic resources on code style and naming conventions to help
  you in your first CSharp plugin!

  https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
  https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names
  https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces
*/

[BepInPlugin(LCMPluginInfo.PLUGIN_GUID, LCMPluginInfo.PLUGIN_NAME, LCMPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
  public static ManualLogSource Log = null!;
  public static Texture2D mainLogo;

  private void Awake()
  {
    /*
      BepinEx makes you a ManualLogSource for free called "Logger"
      and I created a static value above to hold on to it so other
      parts of your plugin's code can find it by using Plugin.Log

      We assign it here
    */
    Log = Logger;
    Harmony val = new Harmony(LCMPluginInfo.PLUGIN_GUID);
	try
	{
		val.PatchAll();
	}	
	catch (Exception ex)
	{
		Log.LogError((object)("Failed to patch: " + ex));
	}

    // Log our awake here so we can see it in LogOutput.txt file
    Log.LogInfo($"Plugin {LCMPluginInfo.PLUGIN_NAME} is loaded!");

	/*
    AssetBundle bundle = BundleUtilities.LoadBundleFromInternalAssembly("morecompany.assets", Assembly.GetExecutingAssembly());
    string name = "assets/morecompanyassets/morecompanytransparentred.png";
		if (Object.op_Implicit((Object)(object)bundle))
		{
			Object val = bundle.LoadAsset(name);
			if (val != (Object)null)
			{
				val.hideFlags= (HideFlags)32;
				mainLogo= (Texture2D)(object)val;
			}
			else mainLogo = default(Texture2D);
			bundle.Unload(false);
		}
	}*/
	
	/*string name = "assets/morecompanyassets/morecompanytransparentred.png";
	mainLogo = (Texture2D) Resources.Load(name);
	*/

	AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(((BaseUnityPlugin)this).Info.Location), "SNCompanyBundle"));
	if (assetBundle != null) 
	{
		mainLogo = assetBundle.LoadAsset<Texture2D>("SNCompany.png");
	}
	else 
	{
		Log.LogError("Failed to load bundle");
	}
  }

}