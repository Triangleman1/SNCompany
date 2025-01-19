using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;
using BepInEx.Configuration;

namespace SNCompany {
    class SNConfig
    {
        public readonly ConfigEntry<bool> vandalizeLogo;
        public readonly ConfigEntry<bool> removeFines;
        public readonly ConfigEntry<bool> advancedGrading;
        public readonly ConfigEntry<double> valueFactor;
        public readonly ConfigEntry<double> groupInefficiency;
        public readonly ConfigEntry<double> fireExitEffect;

        public SNConfig(ConfigFile cfg)
        {
            cfg.SaveOnConfigSet = false; 

            vandalizeLogo = cfg.Bind(
                "General",                     
                "VandalizeLogo",               
                false,                          
                "Whether I smear my full, actual name across your screen. I set this to true. Not sure why you'd want to." 
            );

            removeFines = cfg.Bind(
                "General",
                "RemoveFines",                
                true,                        
                "If true, removes fines."    
            );

            advancedGrading = cfg.Bind(
                "General",
                "AdvancedGrading",
                true,
                "If true, replaces vanilla grading algorithm with a custom one. Prioiritizes effort and efficiency over pure completion."
            );

            valueFactor = cfg.Bind(
                "Grading Parameters",
                "ValueFactor",
                0.5,
                new ConfigDescription("To determine dungeon clear rate, a weighted average is taken of the percentage of scrap objects found and the percentage of scrap value found. The value factor is that weight. A higher value factor prioritizes value, while a lower value factor prioritizes the quantity of objects. \nNote: Even with a value factor of 1, the amount of objects found still has an effect elsewhere, when modeling the number of trips made back to the ship. Therefore, quantity is still more influential than value with a value factor of 0.5.\nAcceptable Values: 0 to 1")
            );

            groupInefficiency = cfg.Bind(
                "Grading Parameters",
                "GroupInefficiency",
                0.3,
                new ConfigDescription("Ideally, twice as many players will be twice as efficient at scavenging and retrieving scrap. In practice, due to lower pressure, caution, chaos, and confusion, this rarely occurs. Group Inefficiency corrects the relationship between player count and score. At 0, it will expect a perfectly linear relationship, with twice as many players accomplishing twice as much. At 1, player count will not affect score at all.\nNote: Grade letter thresholds are calculated for 4 players. A lower groupInefficiency will grade easier for less players, and harder for many players. A high groupInefficiency will do the opposite.\nAcceptable Values: 0 to 1")
            );
            fireExitEffect = cfg.Bind(
                "Grading Parameters",
                "fireExitEffect",
                0.5,
                new ConfigDescription("A fireExitEffect of 1 assumes perfect placement and utilization of fire exits. A fireExitEffect of 0 will entirely remove fire exits from the equation.\nAcceptable Values: 0 to 1")
            );
            
            ClearOrphanedEntries(cfg); 
            cfg.Save(); 
            cfg.SaveOnConfigSet = true; 
        }

        static void ClearOrphanedEntries(ConfigFile cfg) 
        { 
            PropertyInfo orphanedEntriesProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries"); 
            var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(cfg); 
            orphanedEntries.Clear(); 
        }
    }
}