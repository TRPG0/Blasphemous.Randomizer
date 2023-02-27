﻿using HarmonyLib;
using UnityEngine;
using Framework.Managers;
using Framework.Inventory;
using Tools.Playmaker2.Action;
using HutongGames.PlayMaker;
using Tools.Level.Utils;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Entities;

namespace BlasphemousRandomizer.Patches
{
    // Most item pickups in the game use this function
    [HarmonyPatch(typeof(InteractableInvAdd), "OnUsePost")]
    public class InteractableInvAdd_Patch
    {
        public static bool Prefix(string ___item)
        {
            Main.Randomizer.Log("InteractableInvAdd(" + ___item + ")");
            Main.Randomizer.itemShuffler.giveItem(___item, true);
            return false;
        }
    }

    // A few item pickups in the game use this function
    [HarmonyPatch(typeof(InteractableInventoryAdd), "OnUsePost")]
    public class InteractableInventoryAdd_Patch
    {
        public static bool Prefix(string ___item)
        {
            Main.Randomizer.Log("InteractableInventoryAdd(" + ___item + ")");
            Main.Randomizer.itemShuffler.giveItem(___item, true);
            return false;
        }
    }

    // This function is used for giving items in cutscenes / dialog
    [HarmonyPatch(typeof(ItemAddition), "executeAction")]
    public class ItemAddition_Patch
    {
        public static bool Prefix(string objectIdStting, FsmBool ___showMessage, ref bool __result)
        {
            Main.Randomizer.Log("ItemAddition(" + objectIdStting + ")");
            Main.Randomizer.itemShuffler.giveItem(objectIdStting, ___showMessage != null && ___showMessage.Value);
            __result = true;
            return false;
        }
    }

    // This function is used to display an item given from ItemAddition at a later time
    [HarmonyPatch(typeof(ItemAdditionMessage), "executeAction")]
    public class ItemAdditionMessage_Patch
    {
        public static bool Prefix(string objectIdStting, ref bool __result)
        {
            if (objectIdStting == "QI110")
            {
                Main.Randomizer.Log("SpecialVerseAddition(" + objectIdStting + ")");
                Main.Randomizer.itemShuffler.giveItem(objectIdStting, false);
            }
            Main.Randomizer.Log("ItemAdditionMessage(" + objectIdStting + ")");
            Main.Randomizer.itemShuffler.displayItem(objectIdStting);
            __result = true;
            return false;
        }
    }

    // Used for cherubs
    [HarmonyPatch(typeof(CherubCaptorPersistentObject), "OnCherubKilled")]
    public class CherubCaptorPersistentObject_Patch
    {
        public static bool Prefix(ref CherubCaptorPersistentObject __instance)
        {
            Main.Randomizer.Log("OnCherubKilled(" + __instance.cherubId + ")");
            __instance.destroyed = true;
            __instance.spawner.DisableCherubSpawn();
            Main.Randomizer.itemShuffler.giveItem(__instance.cherubId, true);
            return false;
        }
    }

    // Lady of Sorrows
    [HarmonyPatch(typeof(LifeUpgrade), "OnEnter")]
    public class LifeUpgrade_Patch
    {
        public static bool Prefix(LifeUpgrade __instance)
        {
            Main.Randomizer.Log("LifeUpgrade()");
            Main.Randomizer.itemShuffler.giveItem($"Lady[{Core.LevelManager.currentLevel.LevelName}]", true);
            __instance.Finish();
            return false;
        }
    }

    // Oil of Pilgrims
    [HarmonyPatch(typeof(FervourUpgrade), "OnEnter")]
    public class FervourUpgrade_Patch
    {
        public static bool Prefix(FervourUpgrade __instance)
        {
            Main.Randomizer.Log("FervourUpgrade()");
            Main.Randomizer.itemShuffler.giveItem($"Oil[{Core.LevelManager.currentLevel.LevelName}]", true);
            __instance.Finish();
            return false;
        }
    }

    // Sword Shrine
    [HarmonyPatch(typeof(StrengthUpgrade), "OnEnter")]
    public class StrengthUpgrade_Patch
    {
        public static bool Prefix(StrengthUpgrade __instance)
        {
            Main.Randomizer.Log("StrengthUpgrade()");
            Main.Randomizer.itemShuffler.giveItem($"Sword[{Core.LevelManager.currentLevel.LevelName}]", true);
            __instance.Finish();
            return false;
        }
    }

    // Used when giving tears of atonement to the player
    [HarmonyPatch(typeof(PurgeAdd), "OnEnter")]
    public class PurgeAdd_Patch
    {
        public static bool Prefix(PurgeAdd __instance)
        {
            int tears = __instance.value == null ? 0 : (int)__instance.value.Value;
            if (tears > 0)
            {
                //Tears are being added as a reward
                bool showMessage = __instance.ShowMessage != null && __instance.ShowMessage.Value;
                if (tears == 30000 || tears == 18000 || (tears == 5000 && __instance.Owner.name == "BossTrigger"))
                    showMessage = true;

                Main.Randomizer.Log("PurgeAdd(" + tears + ")");
                if (tears == 18000)
                {
                    Main.Randomizer.itemShuffler.giveItem($"{Core.LevelManager.lastLevel.LevelName}[18000]", showMessage);
                }
                else
                {
                    Main.Randomizer.itemShuffler.giveItem($"{__instance.Owner.name}[{tears}]", showMessage);
                }
            }
            else
            {
                // Tears are being subtracted from buying something
                Core.Logic.Penitent.Stats.Purge.Current += tears;
            }

            __instance.Finish();
            return false;
        }
    }

    // Used when a boss is killed to give tears
    [HarmonyPatch(typeof(Penitent), "GetPurge")]
    public class Penitent_Patch
    {
        public static bool Prefix(Enemy enemy)
        {
            if (enemy.Id != "" && "BS01BS03BS04BS05BS06BS12BS13BS14BS16".Contains(enemy.Id))
            {
                Main.Randomizer.Log("GetPurge(" + enemy.Id + ")");
                Main.Randomizer.itemShuffler.giveItem(enemy.Id, true);
                return false;
            }
            return true;
        }
    }

    // Autotracking

    // Beads
    [HarmonyPatch(typeof(InventoryManager), "AddRosaryBead", typeof(RosaryBead))]
    public class InventoryBead_Patch
    {
        public static void Postfix(RosaryBead rosaryBead)
        {
            Main.Randomizer.tracker.NewItem(rosaryBead.id, 0);
        }
    }
    // Prayers
    [HarmonyPatch(typeof(InventoryManager), "AddPrayer", typeof(Prayer))]
    public class InventoryPrayer_Patch
    {
        public static void Postfix(Prayer prayer)
        {
            Main.Randomizer.tracker.NewItem(prayer.id, 1);
        }
    }
    // Relics
    [HarmonyPatch(typeof(InventoryManager), "AddRelic", typeof(Relic))]
    public class InventoryRelic_Patch
    {
        public static void Postfix(Relic relic)
        {
            Main.Randomizer.tracker.NewItem(relic.id, 2);
        }
    }
    // Hearts
    [HarmonyPatch(typeof(InventoryManager), "AddSword", typeof(Sword))]
    public class InventorySword_Patch
    {
        public static void Postfix(Sword sword)
        {
            Main.Randomizer.tracker.NewItem(sword.id, 3);
        }
    }
    // Collectibles
    [HarmonyPatch(typeof(InventoryManager), "AddCollectibleItem", typeof(Framework.Inventory.CollectibleItem))]
    public class InventoryCollectible_Patch
    {
        public static void Postfix()
        {
            Main.Randomizer.tracker.NewItem("CO", 4);
        }
    }
    // Quest items
    [HarmonyPatch(typeof(InventoryManager), "AddQuestItem", typeof(QuestItem))]
    public class InventoryQuestItem_Patch
    {
        public static void Postfix(QuestItem questItem)
        {
            Main.Randomizer.tracker.NewItem(questItem.id, 5);
        }
    }
}
