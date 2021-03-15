using System;
using System.Collections.Generic;

using SDG.Framework.Modules;
using SDG.Unturned;

namespace GhostiBlacklistItems
{
    public class Main : IModuleNexus
    {
        public void initialize() {
            try {
                CommandWindow.Log("Starting Ghosticollis Blacklist Items Module...");

                Level.onPostLevelLoaded = (PostLevelLoaded)Delegate.Combine(Level.onPostLevelLoaded, new PostLevelLoaded(mOnPostLevelLoaded));

            } catch (Exception e) {
                CommandWindow.LogError("GhostiBlacklistItems_error_PIN1005: exception1 got cought: " + e.Message);
            }
        }

        private static void mOnPostLevelLoaded(int level) {
            try {
                if (level > Level.BUILD_INDEX_SETUP && Provider.isServer) {
                    MConfig.loadConfigData();
                    mFilterSpawnTables(true);
                    CommandWindow.Log("GhostiBlacklistItems: done filtering spawn tables from the following blacklisted items: " + string.Join(@"|", spawn_blacklisted_items));
                }
            } catch (Exception e) {
                CommandWindow.LogError("GhostiBlacklistItems_error_PIN1005: exception2 got cought: " + e.Message);
            }
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
    }
}
