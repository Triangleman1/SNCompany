using UnityEngine;
using UnityEngine.SceneManagement;

namespace SNCompany {
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