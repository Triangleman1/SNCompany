using System.Collections.Generic;
using LethalLevelLoader;
using Unity.Netcode;
using UnityEngine;
using static SNCompany.SNSave;

namespace SNCompany 
{
    public class SNNetworkHandler : NetworkBehaviour//, INetworkSerializable
    {
        public static GameObject snObject = null!;

        public static SNNetworkHandler Instance { get; private set; } = null!;

        public override void OnNetworkSpawn()
        {
            ((UnityEngine.Object)((Component)this).gameObject).name = "SNNetworkHandler";
            Instance = this;
            Plugin.Log.LogDebug($"{this.gameObject.name} Spawned"); 
        }

        /*
        [ClientRpc]
        public void LoadSNLevelClientRpc(ExtendedLevel extendedLevel, Level level) 
        {
            string moonName = extendedLevel.SelectableLevel.sceneName;
            SNLevel snLevel = new SNLevel(extendedLevel);

            //Subsidies
            snLevel.originalPrice = level.originalPrice;
            snLevel.subsidy = level.subsidy;
            snLevel.subsidized = level.subsidized;
            SNLevelManager.SNLevels.Add(snLevel.moonName, snLevel);
        }

        [ClientRpc]
        public void InitializeSNLevelClientRpc(ExtendedLevel extendedLevel) 
        {
            string moonName = extendedLevel.SelectableLevel.sceneName;
            SNLevel snLevel = new SNLevel(extendedLevel);

            //Subsidies
            snLevel.originalPrice = extendedLevel.RoutePrice;
            snLevel.subsidy = -1;
            snLevel.subsidized = false;
            SNLevelManager.SNLevels.Add(snLevel.moonName, snLevel);
        }*/

        /*
        
        [ServerRpc]
        public void RequestSNLevelsServerRpc()
        {
            Plugin.Log.LogDebug($"Server received request for levels");
            RequestSNLevelsClientRpc(SNLevelManager.SNLevels);
        }

        
        [ClientRpc]
        public void RequestSNLevelsClientRpc(Dictionary<string, SNLevel> serverSNLevels)
        {
            Plugin.Log.LogDebug($"Client request recieved. Refreshing all clients' SNLevels.");
            SNLevelManager.SNLevels = serverSNLevels;
        }
        */

        [ClientRpc]
        public void SyncFireExitClientRpc(int fireExits)
        {
            Plugin.Log.LogDebug($"{SubsidyNetworkHandler.WhoAmI()} Client updating number of fire exits to {fireExits} from server.");
            Grading.numOfFireExits = fireExits;
        }
    }
}