using System.Collections;
using System.Reflection;
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

        public static SubsidyNetworkHandler Instance { get; private set; } = null!;

        public void SetSubsidyParameters(int percent, int amount)
        {
            Plugin.Log.LogDebug("Setting Parameters");
            if (IsServer)
            {
                percentSubsidy.Value = percent;
                amountSubsidy.Value = amount;
                Plugin.Log.LogDebug("SubsidizingMoons");
                Subsidy.SubsidizeAllMoons();
            }
            else 
            {
                StartCoroutine(waitTicksBeforeSubsidy(5, .5));
            }
        }

        public IEnumerator waitTicksBeforeSubsidy(int ticks, double minTime)
        {
            float timeBetweenNetworkTicks = (float)(NetworkManager.LocalTime.TimeTicksAgo(1).Time - NetworkManager.LocalTime.TimeTicksAgo(2).Time);
            yield return new WaitForSeconds(ticks*timeBetweenNetworkTicks);
            Plugin.Log.LogDebug($"Waited {ticks} ticks, or {ticks*timeBetweenNetworkTicks} seconds");
            Plugin.Log.LogDebug("SubsidizingMoons");
            Subsidy.SubsidizeAllMoons();
        }

        public static string WhoAmI()
        {
            string testOwnership;
            if (Instance.IsServer) testOwnership = "Server";
            else if (Instance.IsHost) testOwnership = "Host";
            else if (Instance.IsClient) testOwnership = "Client";
            else testOwnership = "No one?";
            return testOwnership;
        }

        public override void OnNetworkSpawn()
        {
            ((UnityEngine.Object)((Component)this).gameObject).name = "SubsidyNetworkHandler";
            /*if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) 
            {
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn(); 
            }*/
            Instance = this;
            Plugin.Log.LogDebug($"{WhoAmI()}: {this.gameObject.name} Spawned"); 
            
            //percentSubsidy.OnValueChanged += OnPercentChanged;
            //amountSubsidy.OnValueChanged += OnAmountChanged;
        }

        public override void OnNetworkDespawn()
        {
            //percentSubsidy.OnValueChanged -= OnPercentChanged;
            //amountSubsidy.OnValueChanged -= OnAmountChanged;
        }
/*
        public void OnPercentChanged(int previous, int current)
        {
            Plugin.Log.LogDebug($"{WhoAmI()}: percentSubsidy was changed from {previous} to {current}");
            if (!IsServer) PercentServerRpc(current);
        }
        
        public void OnAmountChanged(int previous, int current)
        {
            Plugin.Log.LogDebug($"{WhoAmI()}: amountSubsidy was changed from {previous} to {current}");
            if (!IsServer) AmountServerRpc(current);
        }
*/
        [ServerRpc(RequireOwnership = false)]
        public void PercentServerRpc(int current)
        {
            if (percentSubsidy.Value == current)
            {
                Plugin.Log.LogDebug($"[ServerRPC]: percentSubsidy already {current}. Not running");
            }
            else
            {
                percentSubsidy.Value = current;
                Plugin.Log.LogDebug($"[ServerRPC]: percentSubsidy changed to {current}");
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AmountServerRpc(int current)
        {
            if (amountSubsidy.Value == current)
            {
                Plugin.Log.LogDebug($"[ServerRPC]: amountSubsidy already {current}. Not running");
            }
            else
            {
                amountSubsidy.Value = current;
                Plugin.Log.LogDebug($"[ServerRPC]: amountSubsidy changed to {current}");
            }
        }
    }
}