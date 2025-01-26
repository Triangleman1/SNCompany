using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace SNCompany {
    public static class NetworkPrefab
    {
        internal static Lazy<GameObject> prefabParent = null!;
        
        public static GameObject GeneratePrefab(string name)
        {
            prefabParent = new Lazy<GameObject>((Func<GameObject>)delegate
            {
                GameObject val = new GameObject("SNParent");
                val.hideFlags = (HideFlags)61;
                val.SetActive(false);
                return val;
            });
            GameObject snPrefab = new GameObject(name);
			snPrefab.hideFlags = (HideFlags)61;
			snPrefab.transform.SetParent(prefabParent.Value.transform);
            snPrefab.AddComponent<NetworkObject>();
            byte[] value = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Assembly.GetCallingAssembly().GetName().Name + name));
			snPrefab.GetComponent<NetworkObject>().GlobalObjectIdHash = BitConverter.ToUInt32(value, 0);
			//subsidyPrefab.AddComponent<SubsidyNetworkHandler>();
            snPrefab.GetComponent<NetworkObject>().SceneMigrationSynchronization = true;
			snPrefab.GetComponent<NetworkObject>().DestroyWithScene = false;
            return snPrefab;
        }
    }
}