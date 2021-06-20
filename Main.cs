using System;
using System.Collections.Generic;

using SDG.Framework.Modules;
using SDG.Unturned;
using UnityEngine;

namespace GhostiBlacklistItems
{
    public class Main : IModuleNexus {

        public static bool bEnableFiltering = true;
        public void initialize() {
            try {
                CommandWindow.Log("Starting Ghosticollis Blacklist Items Module...");

                Level.onPostLevelLoaded = (PostLevelLoaded)Delegate.Combine(Level.onPostLevelLoaded, new PostLevelLoaded(mOnPostLevelLoaded));

                CommandWindow.onCommandWindowInputted += (string text, ref bool shouldExecuteCommand) => {
                    try {
                        mOnCommandWindowInputted(text, ref shouldExecuteCommand);                       
                    } catch (Exception e) {
                        CommandWindow.LogError("GhostiBlacklistItems_error_PIN1005: exception22 got cought: " + e.Message);
                    }
                };
            } catch (Exception e) {
                CommandWindow.LogError("GhostiBlacklistItems_error_PIN1005: exception1 got cought: " + e.Message);
            }
        }

        private static void mOnPostLevelLoaded(int level) {
            try {
                //if (Provider.map.ToLower() == "kuwait") {
                    //printVehicleSpawnTables();
                    //getSpawnTablesThatHasItem(5010);
                    //printSpawnTableItems(7255);

                    if (level > Level.BUILD_INDEX_SETUP && Provider.isServer) {
                        MConfig.loadConfigData();
                        if (bEnableFiltering) {
                            mFilterSpawnTables(true);
                            mFilterVehicleSpawnTables(true);
                        }
                        CommandWindow.Log("GhostiBlacklistItems: done filtering spawn tables from the following blacklisted items: " + string.Join(@"|", spawn_blacklisted_items));
                        CommandWindow.Log("GhostiBlacklistItems: done filtering spawn tables from the following blacklisted vehicles: " + string.Join(@"|", spawn_blacklisted_vehicles));
                    }
                //}

            } catch (Exception e) {
                CommandWindow.LogError("GhostiBlacklistItems_error_PIN1005: exception2 got cought: " + e.Message);
            }
        }

        public static HashSet<ushort> spawn_blacklisted_vehicles = new HashSet<ushort>();
        static void mFilterVehicleSpawnTables(bool bDestroyExistingBlacklistedVehicles) {
            foreach (ushort blacklistedId in spawn_blacklisted_vehicles) {
                bool bDone = false;
                while (!bDone) {
                    bDone = mFilterVehicleSpawnTables_aux(blacklistedId);
                }
            }

            if (bDestroyExistingBlacklistedVehicles) {
                List<InteractableVehicle> vToDestroy = new List<InteractableVehicle>();
                foreach (ushort blacklistedId in spawn_blacklisted_vehicles) {
                    foreach (var v in VehicleManager.vehicles) {
                        if (v.id == blacklistedId) {
                            vToDestroy.Add(v);
                        }
                    }
                }
                foreach(var v in vToDestroy) {
                    VehicleManager.askVehicleDestroy(v);
                }
            }
        }

        static bool mFilterVehicleSpawnTables_aux(ushort blacklistedId) {
            foreach (var tb in LevelVehicles.tables) {
                for (byte tierIndex = 0; tierIndex < tb.tiers.Count; tierIndex++) {
                    int numOfTierVehicles = tb.tiers[tierIndex].table.Count;
                    for (byte vIndex = 0; vIndex < numOfTierVehicles; vIndex++) {
                        if (tb.tiers[tierIndex].table[vIndex].vehicle == blacklistedId) {
                            if (numOfTierVehicles > 1) {
                                tb.removeVehicle(tierIndex, vIndex);
                            } else {
                                tb.removeTier(tierIndex);
                                // it is ok if end up with 0 tiers at table
                            }
                            tb.buildTable();
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static HashSet<ushort> spawn_blacklisted_items = new HashSet<ushort>();
        static void mFilterSpawnTables(bool bRespawnItems) {

            // scan spawn assets and remove resricted items
            // since this might generate empty Spawn assets
            // which will cause a bug when trying to resolve item from it, which spams logs with messages like:
            //   > Failed to respawn an item with ID 0 from type Military_High [3]
            // so also keep scanning and filtering empty assets until it is clean

            var allSpawnAssets = Assets.find(EAssetType.SPAWN);
            HashSet<ushort> emptyOrBlacklistedAssets = new HashSet<ushort>(spawn_blacklisted_items);
            bool bEdited;
            do {
                bEdited = false;

                foreach (SpawnAsset spawnAsset in allSpawnAssets) {
                    if (spawnAsset != null && spawnAsset.tables.Count < 1) {
                        emptyOrBlacklistedAssets.Add(spawnAsset.id);
                    }
                }
                //CommandWindow.Log("restricted or empty: " + string.Join(@"|", emptyOrBlacklistedAssets));

                //for (ushort i = 0; i < 58000; i++) {
                //   var asset = Assets.find(EAssetType.SPAWN, i) as SpawnAsset;
                foreach (SpawnAsset spawnAsset in allSpawnAssets) {
                    if (spawnAsset != null) {
                        foreach (var id in emptyOrBlacklistedAssets) {
                            int index = SpawnAsset_GetEntyIndex(spawnAsset, id);
                            if (index >= 0) {
                                spawnAsset.removeTableAtIndex(index);
                                bEdited = true;
                            }
                        }

                        if (bEdited) {
                            spawnAsset.sortAndNormalizeWeights();
                        }
                    }
                }

                emptyOrBlacklistedAssets = new HashSet<ushort>();
            } while (bEdited);

            if (bRespawnItems) {
                ItemManager.askClearAllItems();
            }

            //SpawnAsset a = Assets.find(EAssetType.SPAWN, 216) as SpawnAsset;
            //CommandWindow.Log(a.name + " tables_num:" + a.tables.Count);
            //foreach (var t in a.tables) {
            //    CommandWindow.Log("assetID:" + t.assetID + " spawnID:" + t.spawnID + " weight:" + t.weight + " chance:" + t.chance );
            //}
        }

        static int SpawnAsset_GetEntyIndex(SpawnAsset spawnAsset, ushort entryID) {
            if (spawnAsset != null && entryID != 0) {
                for (int i = 0; i < spawnAsset.tables.Count; i++) {
                    if (spawnAsset.tables[i].assetID == entryID || spawnAsset.tables[i].spawnID == entryID) {
                        return i;
                    }
                }
            }
            return -1;
        }

        public void shutdown() {

        }





        // ///////////////////////////////////////////////////
        // Debug

        static void mPrintLine(string line) {
            DebugMode_ChatSay(line, Color.green);
            CommandWindow.Log(line);
        }

        public static void DebugMode_ChatSay(string text) {
            if (!Provider.serverName.Contains("SOD")) {
                ChatManager.say(text, Color.yellow);
            }
        }

        public static void DebugMode_ChatSay(string text, Color color) {
            if (!Provider.serverName.Contains("SOD")) {
                ChatManager.say(text, color);
            }
        }

        void mOnCommandWindowInputted(string text, ref bool shouldExecuteCommand) {
            if (text.StartsWith("scan for item ")) {
                shouldExecuteCommand = false;
                var a = text.Split(' ');
                if (ushort.TryParse(a[a.Length - 1], out ushort id)) {
                    getSpawnTablesThatHasItem(id, false);
                }
            } else if (text.StartsWith("recursive scan for item ")) {
                shouldExecuteCommand = false;
                var a = text.Split(' ');
                if (ushort.TryParse(a[a.Length - 1], out ushort id)) {
                    getSpawnTablesThatHasItem(id, true);
                }
            } else if (text.StartsWith("scan for table ")) {
                shouldExecuteCommand = false;
                var a = text.Split(' ');
                if (ushort.TryParse(a[a.Length - 1], out ushort id)) {
                    getSpawnTablesThatHasTable(id);
                }
            } else if (text.StartsWith("print vehicle spawn tables")) {
                shouldExecuteCommand = false;
                printVehicleSpawnTables();
            } else if (text.StartsWith("scan for empty tables")) {
                var allSpawnAssets = Assets.find(EAssetType.SPAWN);
                foreach (SpawnAsset spawnAsset in allSpawnAssets) {
                    if (spawnAsset != null && spawnAsset.tables.Count < 1) {
                        mPrintLine(spawnAsset.id.ToString());
                    }
                }
            } else if (text.StartsWith("print spawn table items")) {
                shouldExecuteCommand = false;
                var a = text.Split(' ');
                if (ushort.TryParse(a[a.Length - 1], out ushort id)) {
                    printSpawnTableItems(id);
                }
            } else if (text.StartsWith("help")) {
                //shouldExecuteCommand = false;
                CommandWindow.Log("==============================================");
                CommandWindow.Log("Blacklist Items Module Commands:");
                CommandWindow.Log("  scan for item [item_id]");
                CommandWindow.Log("  recursive scan for item [item_id]");
                CommandWindow.Log("  scan for table [table_id]");
                CommandWindow.Log("  print vehicle spawn tables");
                CommandWindow.Log("  scan for empty tables");
                CommandWindow.Log("  print spawn table items [table_id]");
                CommandWindow.Log("==============================================");
            }
        }

        public static void getSpawnTablesThatHasTable(ushort tableID) {
            mPrintLine("searching for tables that has reference for table " + tableID);

            mPrintLine("");
            mPrintLine("searching spawn assets ...");
            //for (ushort i = 57000; i < 57200; i++) {
            for (ushort i = 0; i < ushort.MaxValue; i++) {
                try {
                    SpawnAsset spawnAsset = (SpawnAsset)Assets.find(EAssetType.SPAWN, i);
                    if (spawnAsset != null) {
                        foreach (SpawnTable spawnTable in spawnAsset.tables) {
                             if (spawnTable.spawnID == tableID) {
                                mPrintLine("found spwan table: " + i.ToString() + "(" + spawnAsset.name + ")");
                            }
                        }
                    }
                } catch (Exception e) {
                    CommandWindow.LogError("GhostiBlacklistItems_error_PIN1005: exception23 got cought: " + e.Message);
                }
            }

            mPrintLine("");
            mPrintLine("searching animal tables (which contains animals ids) ...");
            foreach (var t in LevelAnimals.tables) {
                if (t.tableID == tableID) {
                    mPrintLine("Animal table: " + t.tableID + " name:" + t.name);
                }
            }

            mPrintLine("");
            mPrintLine("searching animal loot drop tables ...");
            var allAnimalAssets = Assets.find(EAssetType.ANIMAL);
            foreach(var a in allAnimalAssets) {
                AnimalAsset aa = (AnimalAsset)a;
                if(aa != null) {
                    if(aa.rewardID == tableID) {
                        mPrintLine("Animal loot table id:" + aa.id + " name:" + aa.name+ " reward id:" + aa.rewardID );
                    }
                }
            }

            mPrintLine("");
            mPrintLine("searching airdrop tables ...");
            for (int i = 0; i < LevelNodes.nodes.Count; i++) {
                Node node = LevelNodes.nodes[i];
                if (node.type == ENodeType.AIRDROP) {
                    AirdropNode airdropNode = (AirdropNode)node;
                    if (airdropNode.id == tableID) { // airdropNode.id is spawn table id
                        mPrintLine("airdrop node at location:"  + airdropNode.point + " id:" + airdropNode.id);
                    }
                }
            }

            mPrintLine("");
            mPrintLine("searching hordbeacon loot tables ...");
            var allItemAssets = Assets.find(EAssetType.ITEM);
            foreach(var a in allItemAssets) {
                if (a is ItemBeaconAsset) {
                    var hb = (ItemBeaconAsset)a;
                    if (hb != null && hb.rewardID == tableID) {
                        mPrintLine("hordebeacon id:" + hb.id + " name:" + hb.name + " loot spawntable id:" + hb.rewardID);
                    }
                }
            }

            mPrintLine("");
            mPrintLine("searching fisher tool loot tables ...");
            foreach (var a in allItemAssets) {
                if (a is ItemFisherAsset) {
                    var fa = (ItemFisherAsset)a;
                    if (fa != null && fa.rewardID == tableID) {
                        mPrintLine("fisher tool id:" + fa.id + " name:" + fa.name + " loot spawntable id:" + fa.rewardID);
                    }
                }
            }

            mPrintLine("");
            mPrintLine("searching Object Dropper loot tables ...");
            var allObjectAssets = Assets.find(EAssetType.OBJECT);
            foreach (var a in allObjectAssets) {
                var oa = (ObjectAsset)a;
                if (oa != null && oa.interactabilityRewardID == tableID) {
                    mPrintLine("Object Dropper id:" + oa.id + " name:" + oa.name + " loot spawntable id:" + oa.interactabilityRewardID);
                }
            }

            mPrintLine("");
            mPrintLine("searching Object Rubble loot tables ...");
            //var allObjectAssets = Assets.find(EAssetType.OBJECT);
            foreach (var a in allObjectAssets) {
                var oa = (ObjectAsset)a;
                if (oa != null && oa.rubbleRewardID == tableID) {
                    mPrintLine("Object Rubble id:" + oa.id + " name:" + oa.name + " loot spawntable id:" + oa.rubbleRewardID);
                }
            }

            mPrintLine("");
            mPrintLine("searching vehicle destroy loot tables ...");
            var allVehicleAssets = Assets.find(EAssetType.VEHICLE);
            foreach (var v in allVehicleAssets) {
                var va = (VehicleAsset)v;
                if (va != null && va.dropsTableId == tableID) {
                    mPrintLine("vehicle id:" + v.id + " name:" + va.name + " loot spawntable id:" + va.dropsTableId);
                }
            }

            mPrintLine("");
            mPrintLine("arenaLoadout tables ...");
            foreach (ArenaLoadout arenaLoadout in Level.info.configData.Arena_Loadouts) {    
                if (arenaLoadout.Table_ID == tableID) {
                    mPrintLine("arenaLoadout has table id:" + arenaLoadout.Table_ID);
                }
            }

            mPrintLine("");
            mPrintLine("searching NPCRandomItemReward tables ...");
            mPrintLine("NPCRandomItemReward not supported yet");

            mPrintLine("");
            mPrintLine("ItemConsumeableAsset/SpawnTableReward not supported yet");

            mPrintLine("");
            mPrintLine("searching Resource tables ...");
            var allResourceAssets = Assets.find(EAssetType.RESOURCE);
            foreach (var r in allResourceAssets) {
                var ra = (ResourceAsset)r;
                if (ra != null && ra.rewardID == tableID) {
                    mPrintLine("resource id:" + ra.id + " name:" + ra.name + " loot spawntable id:" + ra.rewardID);
                }
            }

            mPrintLine("");
            mPrintLine("searching Vehicle tables ...");
            foreach(var v in LevelVehicles.tables) {
                if(v.tableID == tableID) {
                    mPrintLine("Vehicle table name:" + v.name + " has the requested id:" + v.tableID);
                }
            }

            mPrintLine("");
            mPrintLine("searching zombie tables ...");
            foreach (var zt in LevelZombies.tables) {
                if (zt.lootID == tableID) {
                    mPrintLine("zombie table name:" + zt.name + " has the requested id:" + zt.lootID);
                }
            }

        }

        public static void getSpawnTablesThatHasItem(ushort itemID, bool bRecurssive = false) {
            string result = "";
            //for (ushort i = 57000; i < 57200; i++) {
            for (ushort i = 0; i < ushort.MaxValue; i++) {
                try {
                    if (isTableHasItem(i, itemID, bRecurssive)) {
                        result += i.ToString() + ", ";
                    }
                } catch (Exception e) {
                    if (e.Message != "wrong table id") {
                        CommandWindow.LogError("GhostiBlacklistItems_error_PIN1005: exception23 got cought: " + e.Message);
                    }
                }
            }
            mPrintLine("found tables: " + result);
        }

        public static bool isTableHasItem(ushort tableID, ushort itemID, bool bRecurssive = false) {
            SpawnAsset spawnAsset = (SpawnAsset)Assets.find(EAssetType.SPAWN, tableID);
            if (spawnAsset != null) {
                foreach (SpawnTable spawnTable in spawnAsset.tables) {
                    if (spawnTable.assetID == itemID) {
                        return true;
                    } else if (bRecurssive && spawnTable.spawnID != 0) {
                        if (isTableHasItem(spawnTable.spawnID, itemID, bRecurssive)) {
                            return true;
                        }
                    }
                }
            } else {
                throw new Exception("wrong table id");
            }
            return false;
        }

        public static void printSpawnTableItems(ushort tableID) {
            SpawnAsset spawnAsset = (SpawnAsset)Assets.find(EAssetType.SPAWN, tableID);
            if (spawnAsset != null) {
                foreach (SpawnTable spawnTable in spawnAsset.tables) {
                    if (spawnTable.assetID != 0) {
                        mPrintLine("item: " + spawnTable.assetID);
                    } else if ( spawnTable.spawnID != 0) {
                        mPrintLine("table: " + spawnTable.spawnID);
                    }
                }
            } else {
                throw new Exception("wrong table id");
            }
        }

        public static void printVehicleSpawnTables() { 
            foreach( var tb in LevelVehicles.tables) {
                mPrintLine("table id: " + tb.tableID);
                foreach(var tier in tb.tiers) {
                    mPrintLine("--tier: " + tier.name);
                    foreach( var v in tier.table) {
                        mPrintLine("--vehicle: " + v.vehicle);
                    }
                }
                mPrintLine("");
            }
        }

    }
}
