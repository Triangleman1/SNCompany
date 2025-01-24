using LethalModDataLib.Attributes;
using LethalModDataLib.Enums;
using Unity.Netcode;
using UnityEngine;

namespace SNCompany
{
    public class SubsidyNetworkHandler : NetworkBehaviour
    {
        public static GameObject subsidyObject = null!;
        [ModData(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.CurrentSave)]
        public static NetworkVariable<int> percentSubsidy = new NetworkVariable<int>();
        [ModData(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.CurrentSave)]
        public static NetworkVariable<int> amountSubsidy = new NetworkVariable<int>();
        public static bool onlyHostSync = false;

        public static SubsidyNetworkHandler Instance { get; private set; } = null!;

        public override void OnNetworkSpawn()
        {
            //(UnityEngine.Object)((Component)this).gameObject).name = "SubsidyNetworkHandler";
            Plugin.Log.LogDebug($"{this.gameObject.name} Spawned");
            /*if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) 
            {
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn(); 
            } */
            Instance = this; 
            
            percentSubsidy.OnValueChanged += OnPercentChanged;
            amountSubsidy.OnValueChanged += OnAmountChanged;
        }

        public override void OnNetworkDespawn()
        {
            percentSubsidy.OnValueChanged -= OnPercentChanged;
            amountSubsidy.OnValueChanged -= OnAmountChanged;
        }

        public void OnPercentChanged(int previous, int current)
        {
            string testOwnership;
            if (IsServer) testOwnership = "Server";
            else if (IsHost) testOwnership = "Host";
            else if (IsClient) testOwnership = "Client";
            else testOwnership = "No one?";
            Plugin.Log.LogDebug($"{testOwnership}: percentSubsidy changed from {previous} to {current}");
            if (!onlyHostSync || IsHost) 
            {
                PercentServerRpc(current);
            }
        }
        
        public void OnAmountChanged(int previous, int current)
        {
            string testOwnership;
            if (IsServer) testOwnership = "Server";
            else if (IsHost) testOwnership = "Host";
            else if (IsClient) testOwnership = "Client";
            else testOwnership = "No one?";
            Plugin.Log.LogDebug($"{testOwnership}: amountSubsidy changed from {previous} to {current}");
            if (!onlyHostSync || IsHost) 
            {
                AmountServerRpc(current);
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void PercentServerRpc(int current)
        {
            Plugin.Log.LogDebug($"[SRPC] percentSubsidy changed to {current}");
            percentSubsidy.Value = current;
        }

        [ServerRpc(RequireOwnership = false)]
        public void AmountServerRpc(int current)
        {
            Plugin.Log.LogDebug($"[SRPC] amountSubsidy changed to {current}");
            amountSubsidy.Value = current;
        }
    }
}