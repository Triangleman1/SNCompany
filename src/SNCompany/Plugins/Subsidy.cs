using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using LethalLevelLoader;
using LethalModDataLib.Attributes;
using LethalModDataLib.Enums;
using Unity.Netcode;
using UnityEngine;

namespace SNCompany {

    public static class Subsidy 
    {
        //To-Do: Networking

        /*
        [ModData(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.CurrentSave)]
        public static int percentSubsidy = 0;
        [ModData(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.CurrentSave)]
        public static int amountSubsidy = 0;
        */
        public static bool globalSubsidized = false;

        public static void SetUpNetworking()
        {
            SubsidyNetworkHandler.subsidyObject = NetworkPrefab.GeneratePrefab("SubsidyNetworkHandler");
            SubsidyNetworkHandler.subsidyObject.AddComponent<SubsidyNetworkHandler>();
            if (!NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(SubsidyNetworkHandler.subsidyObject))
            {
                NetworkManager.Singleton.AddNetworkPrefab(SubsidyNetworkHandler.subsidyObject);
            }
            Plugin.Log.LogDebug("Set up and registered Subsidy's Networking GameObject");
        }
        
        public static void SetSubsidyParameters(int percent, int amount)
        {
            //Subsidy.onlyHostSync = true;
            SubsidyNetworkHandler.percentSubsidy.Value = percent;
            SubsidyNetworkHandler.amountSubsidy.Value = amount;
            //Subsidy.onlyHostSync = false;
            Plugin.Log.LogDebug($"percentSubsidy set to: {SubsidyNetworkHandler.percentSubsidy.Value}");
			Plugin.Log.LogDebug($"amountSubsidy set to: {SubsidyNetworkHandler.amountSubsidy.Value}");
        }

        public static void SubsidizeAllMoons() 
        {
            if (globalSubsidized == true) 
            {
                Plugin.Log.LogWarning("Tried to subsidize when already subsidized");
                return;
            }
            foreach (var entry in SNLevelManager.SNLevels)
            {
                SNLevel snLevel = entry.Value;
                ExtendedLevel extendedLevel = snLevel.extendedLevel;
                string moonName = snLevel.moonName;
                int basePrice = extendedLevel.RoutePrice;
                int discount = CalculateSubsidy(basePrice);

                if (!snLevel.subsidized) 
                {
                    snLevel.originalPrice = basePrice;
                    snLevel.subsidy = discount;
                    snLevel.subsidized = true;
                    extendedLevel.RoutePrice = basePrice-discount;

                    Plugin.Log.LogDebug($"Subsidized {moonName}");
                }
            }
            globalSubsidized = true;
        }

        public static int CalculateSubsidy(int moonPrice) 
        {
            int discount;
            if (moonPrice == 0) return 0;
            discount = (int)(moonPrice*((double)SubsidyNetworkHandler.percentSubsidy.Value/100));
            if (discount > SubsidyNetworkHandler.amountSubsidy.Value) discount = SubsidyNetworkHandler.amountSubsidy.Value;
            if (discount > moonPrice) discount = moonPrice;
            Plugin.Log.LogDebug($"Subsidy: {moonPrice}-{discount}={moonPrice-discount}");

            return discount;
        }

        public static void UnsubsidizeAllMoons() 
        {
            if (globalSubsidized == false) 
            {
                Plugin.Log.LogWarning("Tried to unsubsidize when not subsidized");
            }
            foreach (var entry in SNLevelManager.SNLevels)
            {
                SNLevel snLevel = entry.Value;
                ExtendedLevel extendedLevel = snLevel.extendedLevel;
                string moonName = snLevel.moonName;
                
                if (snLevel.subsidized == true) 
                {
                    extendedLevel.RoutePrice = snLevel.originalPrice;
                    snLevel.subsidized = false;
                    //Remember: snLevel.subsidy keeps its stored value, even after unsubsidization
                    Plugin.Log.LogDebug($"Unsubsidized {moonName}");
                }
            }
            globalSubsidized = false;
        }
    }
}