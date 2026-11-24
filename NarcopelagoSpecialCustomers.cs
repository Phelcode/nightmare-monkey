using Harmony;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.NPCs.Relation;
using Il2CppScheduleOne.SpecialCustomers;
using Il2CppScheduleOne.UI.Handover;
using MelonLoader;
using System;
using System.Collections.Generic;

namespace Narcopelago
{
    /// <summary>
    /// Handles the v0.4.7 travelling special customer groups (Hippies, Party Bus, Businessmen, Bikers).
    /// - "X Unlocked" items tagged "Special Customer" unlock the matching special customer in-game
    ///   (used when randomize_special_customers is enabled).
    /// - Completing a bulk deal with a visiting group sends the "Special Customer Bulk Deal: {Group}"
    ///   location check. When randomize_special_customers is enabled, the check is only sent if the
    ///   player has received at least one "X Unlocked" item from that group (matches apworld logic).
    /// </summary>
    public static class NarcopelagoSpecialCustomers
    {
        /// <summary>
        /// Location name prefix for special customer bulk deal checks.
        /// </summary>
        public const string BULK_DEAL_LOCATION_PREFIX = "Special Customer Bulk Deal: ";

        /// <summary>
        /// Checks if an AP item is a special customer unlock item (e.g. "Amethyst Pidley Unlocked").
        /// </summary>
        public static bool IsSpecialCustomerItem(string itemName)
        {
            return !string.IsNullOrEmpty(itemName) &&
                   itemName.EndsWith(" Unlocked") &&
                   Data_Items.HasTag(itemName, "Special Customer");
        }

        /// <summary>
        /// Gets the SpecialCustomerManager singleton, or null if not available.
        /// </summary>
        public static SpecialCustomerManager GetManager()
        {
            try
            {
                if (NetworkSingleton<SpecialCustomerManager>.InstanceExists)
                    return NetworkSingleton<SpecialCustomerManager>.Instance;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[SpecialCustomers] Error getting SpecialCustomerManager: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Gets the special customer group that is currently visiting (or scheduled), or null.
        /// </summary>
        public static SpecialCustomerData GetVisitingGroup()
        {
            try
            {
                return GetManager()?.CurrentGroupData;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[SpecialCustomers] Error getting visiting group: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Checks if an NPC belongs to a special customer group (leader or member).
        /// </summary>
        private static bool IsNPCInGroup(NPC npc, SpecialCustomerData group)
        {
            if (npc == null || group == null)
                return false;

            try
            {
                if (group.Leader != null && group.Leader.gameObject == npc.gameObject)
                    return true;

                if (group.Customers != null)
                {
                    foreach (var member in group.Customers)
                    {
                        if (member != null && member.gameObject == npc.gameObject)
                            return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[SpecialCustomers] Error checking group membership: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Resolves the group name (e.g. "Hippies") for a special customer NPC.
        /// Prefers the currently visiting group, then falls back to the group definitions.
        /// Returns null if the NPC is not a special customer or no group matches.
        /// </summary>
        public static string GetGroupNameForNPC(NPC npc)
        {
            if (npc == null)
                return null;

            try
            {
                if (npc.TryCast<SpecialCustomer>() == null)
                    return null;

                var visiting = GetVisitingGroup();
                if (IsNPCInGroup(npc, visiting))
                    return visiting.GroupName;

                var manager = GetManager();
                if (manager?._specialCustomerGroups != null)
                {
                    foreach (var group in manager._specialCustomerGroups)
                    {
                        if (IsNPCInGroup(npc, group))
                            return group.GroupName;
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[SpecialCustomers] Error resolving group for NPC: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Checks if the player has received at least one "X Unlocked" item for the given group.
        /// Group membership comes from the group tag on the item (e.g. "Hippies").
        /// </summary>
        public static bool HasAnyGroupMemberUnlockedViaAP(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
                return false;

            foreach (var itemName in Data_Items.GetItemsByTag("Special Customer"))
            {
                var item = Data_Items.GetItem(itemName);
                if (item?.Tags == null)
                    continue;

                bool inGroup = false;
                foreach (var tag in item.Tags)
                {
                    if (string.Equals(tag, groupName, StringComparison.OrdinalIgnoreCase))
                    {
                        inGroup = true;
                        break;
                    }
                }
                if (!inGroup)
                    continue;

                if (NarcopelagoItems.HasReceivedItem(itemName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sends the "Special Customer Bulk Deal: {Group}" location check to Archipelago.
        /// When randomize_special_customers is enabled, the check is only sent if the player has
        /// received at least one member unlock from that group (matches the apworld location rule).
        /// </summary>
        public static void SendBulkDealCheck(string groupName)
        {
            try
            {
                if (string.IsNullOrEmpty(groupName))
                    return;

                if (NarcopelagoOptions.IsLoaded && NarcopelagoOptions.Randomize_special_customers &&
                    !HasAnyGroupMemberUnlockedViaAP(groupName))
                {
                    MelonLogger.Warning($"[SpecialCustomers] Bulk deal with '{groupName}' is out of logic - no member unlock received (randomize_special_customers on); not sending check");
                    return;
                }

                string locationName = BULK_DEAL_LOCATION_PREFIX + groupName;
                int locationId = Data_Locations.GetLocationId(locationName);

                // Case-insensitive fallback in case the game's GroupName casing differs
                if (locationId <= 0 && Data_Locations.Locations != null)
                {
                    foreach (var kvp in Data_Locations.Locations)
                    {
                        if (kvp.Key.StartsWith(BULK_DEAL_LOCATION_PREFIX, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(kvp.Key.Substring(BULK_DEAL_LOCATION_PREFIX.Length), groupName, StringComparison.OrdinalIgnoreCase))
                        {
                            locationName = kvp.Key;
                            locationId = kvp.Value.ModernId;
                            break;
                        }
                    }
                }

                if (locationId <= 0)
                {
                    MelonLogger.Warning($"[SpecialCustomers] No bulk deal location found for group '{groupName}' - not sending check");
                    return;
                }

                if (NarcopelagoLocations.IsLocationChecked(locationId))
                {
                    MelonLogger.Msg($"[SpecialCustomers] Bulk deal location '{locationName}' already checked");
                    return;
                }

                MelonLogger.Msg($"[SpecialCustomers] Sending location check: '{locationName}' (ID: {locationId})");
                NarcopelagoLocations.CompleteLocation(locationId);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[SpecialCustomers] Error in SendBulkDealCheck: {ex.Message}");
            }
        }

        /// <summary>
        /// Finds a special customer NPC by name in the NPC registry and unlocks it in-game.
        /// Special customers are not in Customer.LockedCustomers/UnlockedCustomers, so they are
        /// resolved through the NPC registry instead. Returns true if the NPC was found.
        /// </summary>
        public static bool TryUnlockSpecialCustomerByName(string customerName)
        {
            try
            {
                if (string.IsNullOrEmpty(customerName) || NPCManager.NPCRegistry == null)
                    return false;

                foreach (var npc in NPCManager.NPCRegistry)
                {
                    if (npc == null)
                        continue;
                    if (npc.TryCast<SpecialCustomer>() == null)
                        continue;

                    string name = npc.FullName ?? npc.FirstName ?? "";
                    if (!StringHelper.EqualsNormalized(name, customerName))
                        continue;

                    var customer = npc.GetComponent<Customer>();
                    if (customer == null)
                    {
                        MelonLogger.Msg($"[SpecialCustomers] '{customerName}' found in registry but has no Customer component - AP unlock tracking only");
                        return true;
                    }

                    var relationData = npc.RelationData;
                    if (relationData != null && !relationData.Unlocked)
                    {
                        NarcopelagoCustomers.Customer_OnCustomerUnlocked_Patch.SetArchipelagoUnlock(true);
                        try
                        {
                            relationData.Unlock(NPCRelationData.EUnlockType.DirectApproach, true);
                            MelonLogger.Msg($"[SpecialCustomers] '{customerName}' unlocked in-game via Unlock() method");
                        }
                        finally
                        {
                            NarcopelagoCustomers.Customer_OnCustomerUnlocked_Patch.SetArchipelagoUnlock(false);
                        }
                    }

                    if (!Customer.UnlockedCustomers.Contains(customer))
                        Customer.UnlockedCustomers.Add(customer);
                    Customer.LockedCustomers.Remove(customer);

                    NarcopelagoCustomers.QueueDelayedPOIUpdate(5);
                    return true;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[SpecialCustomers] Error unlocking '{customerName}': {ex.Message}");
                return true; // Don't requeue forever on errors
            }
            return false;
        }

        /// <summary>
        /// Harmony patch for Customer.ProcessHandover.
        /// When a bulk deal with a visiting special customer group is finalized, send the
        /// "Special Customer Bulk Deal: {Group}" check.
        /// </summary>
        [HarmonyPatch(typeof(Customer), "ProcessHandover")]
        public class Customer_ProcessHandover_Patch
        {
            static bool Prepare()
            {
                MelonLogger.Msg("[PATCH] Customer.ProcessHandover patch is being prepared");
                return true;
            }

            static void Postfix(Customer __instance, HandoverScreen.EHandoverOutcome outcome)
            {
                try
                {
                    if (outcome != HandoverScreen.EHandoverOutcome.Finalize)
                        return;

                    var npc = __instance?.NPC;
                    if (npc == null)
                        return;

                    string groupName = GetGroupNameForNPC(npc);
                    if (string.IsNullOrEmpty(groupName))
                        return;

                    MelonLogger.Msg($"[SpecialCustomers] Bulk deal finalized with special customer group '{groupName}' (NPC: {npc.FullName ?? npc.FirstName})");
                    SendBulkDealCheck(groupName);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[PATCH] Error in ProcessHandover Postfix: {ex.Message}");
                }
            }
        }
    }
}
