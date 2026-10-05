/*Copyright 2021 Christopher Beda

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using StatControllers;
using System;
using System.Reflection;
using UnityEngine;

public class PlayerCurrentAmmoIcon : Binding
{
    public PlayerCurrentAmmoIcon(int value, string name) : base(value, name)
    {
    }

    public override string GetCurrentValue(EntityPlayer player)
    {
        return AmmoBindingUtil.GetCachedAmmoIcon(player);
    }
}

public class PlayerCurrentAmmoCount : Binding
{
    public PlayerCurrentAmmoCount(int value, string name) : base(value, name)
    {
    }

    public override string GetCurrentValue(EntityPlayer player)
    {
        return AmmoBindingUtil.GetCachedAmmoCount(player);
    }
}

public class PlayerCurrentAmmoVisible : Binding
{
    public PlayerCurrentAmmoVisible(int value, string name) : base(value, name)
    {
    }

    public override string GetCurrentValue(EntityPlayer player)
    {
        return AmmoBindingUtil.GetCachedAmmoVisible(player);
    }
}

internal static class AmmoBindingUtil
{
    private static int AmmoTypeIndex(ItemValue heldValue)
    {
        if (heldValue == null)
        {
            return 0;
        }

        PropertyInfo property = typeof(ItemValue).GetProperty("SelectedAmmoTypeIndex");
        if (property != null)
        {
            object value = property.GetValue(heldValue, null);
            return value == null ? 0 : Convert.ToInt32(value);
        }

        FieldInfo field = typeof(ItemValue).GetField("SelectedAmmoTypeIndex");
        if (field != null)
        {
            object value = field.GetValue(heldValue);
            return value == null ? 0 : Convert.ToInt32(value);
        }

        return 0;
    }

    private sealed class AmmoSnapshot
    {
        public int Frame = -1;
        public int PlayerEntityId = -1;
        public string Icon = "";
        public string Count = " ";
        public string Visible = "false";
    }

    private static readonly AmmoSnapshot Snapshot = new AmmoSnapshot();
    private static int callCounter = 0;

    public static string GetCachedAmmoIcon(EntityPlayer player)
    {
        RefreshSnapshot(player);
        return Snapshot.Icon;
    }

    public static string GetCachedAmmoCount(EntityPlayer player)
    {
        RefreshSnapshot(player);
        return Snapshot.Count;
    }

    public static string GetCachedAmmoVisible(EntityPlayer player)
    {
        RefreshSnapshot(player);
        return Snapshot.Visible;
    }

    public static void ResetCache()
    {
        Snapshot.Frame = -1;
        Snapshot.PlayerEntityId = -1;
        Snapshot.Icon = "";
        Snapshot.Count = " ";
        Snapshot.Visible = "false";
    }

    private static void RefreshSnapshot(EntityPlayer player)
    {
        int frame = Time.frameCount;
        int playerEntityId = player?.entityId ?? -1;
        if (Snapshot.Frame == frame && Snapshot.PlayerEntityId == playerEntityId)
        {
            return;
        }

        Snapshot.Frame = frame;
        Snapshot.PlayerEntityId = playerEntityId;
        Snapshot.Icon = "";
        Snapshot.Count = " ";
        Snapshot.Visible = "false";

        object ammoAction = GetAmmoAction(player, out ItemValue heldValue);
        if (ammoAction == null || heldValue == null)
        {
            return;
        }

        Snapshot.Icon = GetCurrentAmmoIconName(ammoAction, heldValue, player);
        GetCurrentAmmoInfo(player, ammoAction, heldValue, out ItemValue ammoItemValue, out int magazineCount);
        if (ammoItemValue == null || ammoItemValue.ItemClass == null)
        {
            return;
        }

        Snapshot.Visible = "true";
        string heldName = heldValue.ItemClass?.Name ?? "";
        if (heldName == "BFG9001")
        {
            int total = CountItemByName(player, "PlasmaCellDM");
            if (total < 0)
            {
                total = 0;
            }

            int y = total / 40;
            Snapshot.Count = total + "/" + y;
            return;
        }

        if (heldName == "FlameThrowerQ1" || heldName == "FlameThrowerQ2" || heldName == "FlameThrowerQ3" ||
            heldName == "FlameThrowerQ4" || heldName == "FlameThrowerQ5" || heldName == "FlameThrowerQ6")
        {
            int total = magazineCount;
            if (total < 0)
            {
                total = 0;
            }

            Snapshot.Count = total.ToString();
            return;
        }

        int normalTotal = magazineCount;
        if (normalTotal < 0)
        {
            normalTotal = 0;
        }

        Snapshot.Count = normalTotal.ToString();
    }
    public static string GetCurrentAmmoItemName(EntityPlayer player)
    {
        if (player == null)
        {
            return "";
        }
        object ammoAction = GetAmmoAction(player, out ItemValue heldValue);
        if (ammoAction == null || heldValue == null)
        {
            return "";
        }

        string ammoName = "";
        if (ammoAction is ItemActionRanged rangedAction)
        {
            if (rangedAction.MagazineItemNames != null && rangedAction.MagazineItemNames.Length > 0)
            {
                int index = AmmoTypeIndex(heldValue);
                if (index < 0 || index >= rangedAction.MagazineItemNames.Length)
                {
                    index = 0;
                }
                ammoName = rangedAction.MagazineItemNames[index];
            }

            if (string.IsNullOrEmpty(ammoName) && rangedAction.MagazineItem != null)
            {
                ammoName = rangedAction.MagazineItem.Value;
            }
        }
        else
        {
            string[] magazineItemNames = GetStringArrayMember(ammoAction, "MagazineItemNames");
            if (magazineItemNames != null && magazineItemNames.Length > 0)
            {
                int index = AmmoTypeIndex(heldValue);
                if (index < 0 || index >= magazineItemNames.Length)
                {
                    index = 0;
                }
                ammoName = magazineItemNames[index];
            }

            if (string.IsNullOrEmpty(ammoName))
            {
                ammoName = GetMemberValuePropertyString(ammoAction, "MagazineItem");
            }
        }

        if (string.IsNullOrEmpty(ammoName))
        {
            return "";
        }

        if (ammoName.IndexOf(',') >= 0)
        {
            int index = AmmoTypeIndex(heldValue);
            ammoName = SelectCsvValue(ammoName, index);
        }

        return ammoName ?? "";
    }

    public static string GetCurrentAmmoIconName(EntityPlayer player)
    {
        object ammoAction = GetAmmoAction(player, out ItemValue heldValue);
        if (ammoAction == null || heldValue == null)
        {
            return "";
        }

        return GetCurrentAmmoIconName(ammoAction, heldValue, player);
    }

    private static string GetCurrentAmmoIconName(object ammoAction, ItemValue heldValue, EntityPlayer player)
    {
        if (ammoAction == null || heldValue == null)
        {
            return "";
        }

        string iconName = "";
        if (ammoAction is ItemActionRanged rangedAction)
        {
            if (rangedAction.BulletIcon != null)
            {
                iconName = rangedAction.BulletIcon.Value;
                if (!string.IsNullOrEmpty(iconName) && iconName.IndexOf(',') >= 0)
                {
                    int index = AmmoTypeIndex(heldValue);
                    iconName = SelectCsvValue(iconName, index);
                }
            }
        }
        else
        {
            iconName = GetMemberValuePropertyString(ammoAction, "BulletIcon");
            if (!string.IsNullOrEmpty(iconName) && iconName.IndexOf(',') >= 0)
            {
                int index = AmmoTypeIndex(heldValue);
                iconName = SelectCsvValue(iconName, index);
            }
        }

        if (!string.IsNullOrEmpty(iconName))
        {
            return iconName;
        }

        string ammoName = GetCurrentAmmoItemName(player);
        if (string.IsNullOrEmpty(ammoName))
        {
            return "";
        }

        ItemClass ammoClass = ItemClass.GetItemClass(ammoName);
        if (ammoClass == null)
        {
            return "";
        }

        return ammoClass.GetIconName();
    }

    public static void GetCurrentAmmoInfo(EntityPlayer player, out ItemValue ammoItemValue, out int magazineCount)
    {
        callCounter++;
        ammoItemValue = null;
        magazineCount = 0;
        object ammoAction = GetAmmoAction(player, out ItemValue heldValue);
        if (ammoAction == null || heldValue == null)
        {
            return;
        }

        GetCurrentAmmoInfo(player, ammoAction, heldValue, out ammoItemValue, out magazineCount);
    }

    private static void GetCurrentAmmoInfo(EntityPlayer player, object ammoAction, ItemValue heldValue, out ItemValue ammoItemValue, out int magazineCount)
    {
        ammoItemValue = null;
        magazineCount = 0;
        if (player == null || ammoAction == null || heldValue == null)
        {
            return;
        }

        // Determine ammo item name (custom or vanilla)
        string heldName = heldValue.ItemClass?.Name ?? "";
        string ammoItemName = null;
        if (heldName.StartsWith("FlameThrowerQ"))
            ammoItemName = "ammoGasCan";
        else if (heldName.StartsWith("RocketLauncherQ"))
            ammoItemName = "RocketFragDM";
        else if (heldName.StartsWith("PlasmaRifleQ"))
            ammoItemName = "PlasmaCellDM";
        else if (heldName.StartsWith("ChainsawQ"))
            ammoItemName = "ammoGasCan";
        else
            ammoItemName = GetCurrentAmmoItemNameFromAction(ammoAction, heldValue);

        if (string.IsNullOrEmpty(ammoItemName))
        {
            return;
        }

        ammoItemValue = ItemClass.GetItem(ammoItemName);
        if (ammoItemValue == null)
        {
            return;
        }

        int total = CountItemById(player, ammoItemValue.ItemClass.Id);
        magazineCount = total;
        return;
    }

    private static string GetCurrentAmmoItemNameFromAction(object ammoAction, ItemValue heldValue)
    {
        if (ammoAction == null || heldValue == null)
        {
            return "";
        }

        string ammoName = "";
        if (ammoAction is ItemActionRanged rangedAction)
        {
            if (rangedAction.MagazineItemNames != null && rangedAction.MagazineItemNames.Length > 0)
            {
                int index = AmmoTypeIndex(heldValue);
                if (index < 0 || index >= rangedAction.MagazineItemNames.Length)
                {
                    index = 0;
                }
                ammoName = rangedAction.MagazineItemNames[index];
            }

            if (string.IsNullOrEmpty(ammoName) && rangedAction.MagazineItem != null)
            {
                ammoName = rangedAction.MagazineItem.Value;
            }
        }
        else
        {
            string[] magazineItemNames = GetStringArrayMember(ammoAction, "MagazineItemNames");
            if (magazineItemNames != null && magazineItemNames.Length > 0)
            {
                int index = AmmoTypeIndex(heldValue);
                if (index < 0 || index >= magazineItemNames.Length)
                {
                    index = 0;
                }
                ammoName = magazineItemNames[index];
            }

            if (string.IsNullOrEmpty(ammoName))
            {
                ammoName = GetMemberValuePropertyString(ammoAction, "MagazineItem");
            }
        }

        if (string.IsNullOrEmpty(ammoName))
        {
            return "";
        }

        if (ammoName.IndexOf(',') >= 0)
        {
            int index = AmmoTypeIndex(heldValue);
            ammoName = SelectCsvValue(ammoName, index);
        }

        return ammoName ?? "";
    }

    private static string SelectCsvValue(string csv, int index)
    {
        if (string.IsNullOrEmpty(csv))
        {
            return "";
        }

        if (index < 0)
        {
            index = 0;
        }

        int current = 0;
        int start = 0;
        for (int i = 0; i <= csv.Length; i++)
        {
            if (i == csv.Length || csv[i] == ',')
            {
                if (current == index)
                {
                    return csv.Substring(start, i - start);
                }

                current++;
                start = i + 1;
            }
        }

        int fallbackEnd = csv.IndexOf(',');
        if (fallbackEnd < 0)
        {
            return csv;
        }

        return csv.Substring(0, fallbackEnd);
    }

    private static int CountItemByName(EntityPlayer player, string itemClassName)
    {
        if (player == null || string.IsNullOrEmpty(itemClassName))
        {
            return 0;
        }

        int total = 0;
        total += CountInSlotsByName(ReadStacks(player.inventory), itemClassName);
        total += CountInSlotsByName(ReadStacks(player.bag), itemClassName);
        return total;
    }

    private static int CountItemById(EntityPlayer player, int itemClassId)
    {
        if (player == null || itemClassId <= 0)
        {
            return 0;
        }

        int total = 0;
        total += CountInSlotsById(ReadStacks(player.inventory), itemClassId);
        total += CountInSlotsById(ReadStacks(player.bag), itemClassId);
        return total;
    }

    private static int CountInSlotsById(ItemStack[] slots, int itemClassId)
    {
        if (slots == null || itemClassId <= 0)
        {
            return 0;
        }

        int total = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            ItemStack stack = slots[i];
            int count = StackCount(stack);
            ItemValue value = StackValue(stack);
            if (count <= 0 || value == null || value.ItemClass == null)
            {
                continue;
            }

            if (value.ItemClass.Id == itemClassId)
            {
                total += count;
            }
        }

        return total;
    }

    private static int CountInSlotsByName(ItemStack[] slots, string itemClassName)
    {
        if (slots == null || string.IsNullOrEmpty(itemClassName))
        {
            return 0;
        }

        int total = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            ItemStack stack = slots[i];
            int count = StackCount(stack);
            ItemValue value = StackValue(stack);
            if (count <= 0 || value == null || value.ItemClass == null)
            {
                continue;
            }

            if (value.ItemClass.Name == itemClassName)
            {
                total += count;
            }
        }

        return total;
    }

    // 3.2: GetSlots() and ItemStack fields. 3.3: ItemGrid plus properties. Direct calls throw on the other version.
    private static ItemStack[] ReadStacks(object container)
    {
        if (container == null)
        {
            return null;
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type type = container.GetType();
        MethodInfo getSlots = type.GetMethod("GetSlots", flags, null, Type.EmptyTypes, null);
        if (getSlots != null)
        {
            return getSlots.Invoke(container, null) as ItemStack[];
        }

        object grid = ReadMember(container, "ItemGrid");
        if (grid != null)
        {
            object items = ReadMember(grid, "items");
            if (items is ItemStack[] stacks)
            {
                return stacks;
            }
        }

        MethodInfo getAt = type.GetMethod("GetStackAt", flags);
        PropertyInfo countProp = type.GetProperty("SlotCount", flags);
        if (getAt == null || countProp == null)
        {
            return null;
        }

        int count = (int)countProp.GetValue(container, null);
        ItemStack[] built = new ItemStack[count];
        object[] args = new object[1];
        for (int i = 0; i < count; i++)
        {
            args[0] = i;
            built[i] = getAt.Invoke(container, args) as ItemStack;
        }

        return built;
    }

    private static int StackCount(ItemStack stack)
    {
        if (stack == null)
        {
            return 0;
        }

        object value = ReadMember(stack, "count");
        return value is int number ? number : 0;
    }

    private static ItemValue StackValue(ItemStack stack)
    {
        if (stack == null)
        {
            return null;
        }

        return ReadMember(stack, "itemValue") as ItemValue;
    }

    private static object ReadMember(object source, string name)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type type = source.GetType();
        PropertyInfo property = type.GetProperty(name, flags);
        if (property != null && property.GetIndexParameters().Length == 0)
        {
            return property.GetValue(source, null);
        }

        FieldInfo field = type.GetField(name, flags);
        if (field != null)
        {
            return field.GetValue(source);
        }

        return null;
    }

    public static int GetInventoryAmmoCount(Inventory inventory, int ammoItemId)
    {
        if (inventory == null || ammoItemId <= 0)
        {
            return 0;
        }

        int total = 0;
        ItemStack[] slots = ReadStacks(inventory);
        if (slots == null)
        {
            return 0;
        }
        for (int i = 0; i < slots.Length; i++)
        {
            ItemStack stack = slots[i];
            int count = StackCount(stack);
            ItemValue value = StackValue(stack);
            if (count <= 0 || value == null || value.ItemClass == null) continue;
            if (value.ItemClass.Id == ammoItemId) total += count;
        }
        LogError($"Total counted for ammoItemId {ammoItemId}: {total}");
        return total;
    }

    public static int GetTotalAmmoCount(EntityPlayer player, ItemValue ammoItemValue)
    {
        if (player == null || ammoItemValue == null || ammoItemValue.ItemClass == null)
        {
            return 0;
        }

        int total = 0;
        if (player.inventory != null)
        {
            total += GetInventoryAmmoCount(player.inventory, ammoItemValue.ItemClass.Id);
        }
        // For FlameThrowerQ weapons, do NOT count toolbelt/bag separately
        return total;
    }

    public static bool ShouldIgnoreMagazineCountForDisplay(EntityPlayer player, int magazineCount)
    {
        if (magazineCount < 10000)
        {
            return false;
        }
        object ammoAction = GetAmmoAction(player, out ItemValue heldValue);
        if (ammoAction == null || heldValue == null)
        {
            return false;
        }
        return GetBoolMember(ammoAction, "InfiniteAmmo");
    }

    private static object GetAmmoAction(EntityPlayer player, out ItemValue heldValue)
    {
        heldValue = null;
        try
        {
            if (player == null)
                return null;
            if (player.inventory == null)
                return null;
            heldValue = player.inventory.holdingItemItemValue;
            if (heldValue == null)
                return null;
            if (heldValue.ItemClass == null)
                return null;
            if (heldValue.ItemClass.Actions == null)
                return null;
            foreach (ItemAction action in heldValue.ItemClass.Actions)
            {
                if (action == null)
                    continue;
                if (action is ItemActionRanged)
                    return action;
                Type actionType = null;
                try { actionType = action.GetType(); } catch (Exception ex) { LogError("GetAmmoAction: action.GetType() failed: " + ex); }
                if (actionType != null)
                {
                    string typeName = null;
                    try { typeName = actionType.Name; } catch (Exception ex) { LogError("GetAmmoAction: actionType.Name failed: " + ex); }
                    if (!string.IsNullOrEmpty(typeName) && typeName.IndexOf("Launcher", StringComparison.OrdinalIgnoreCase) >= 0)
                        return action;
                }
            }
        }
        catch (Exception ex)
        {
            LogError("GetAmmoAction: Exception: " + ex);
            heldValue = null;
            return null;
        }
        return null;
    }

    // Simple logger for debugging nulls
    private static void LogError(string msg)
    {
        // Logging removed
    }

    private static string[] GetStringArrayMember(object source, string memberName)
    {
        if (source == null || string.IsNullOrEmpty(memberName))
        {
            return null;
        }

        Type sourceType = source.GetType();
        FieldInfo field = sourceType.GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
        if (field != null)
        {
            return field.GetValue(source) as string[];
        }

        PropertyInfo property = sourceType.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
        if (property != null)
        {
            return property.GetValue(source, null) as string[];
        }

        return null;
    }

    private static string GetMemberValuePropertyString(object source, string memberName)
    {
        if (source == null || string.IsNullOrEmpty(memberName))
        {
            return "";
        }

        object memberObject = null;
        Type sourceType = source.GetType();
        FieldInfo field = sourceType.GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
        if (field != null)
        {
            memberObject = field.GetValue(source);
        }
        else
        {
            PropertyInfo property = sourceType.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
            {
                memberObject = property.GetValue(source, null);
            }
        }

        if (memberObject == null)
        {
            return "";
        }

        Type memberType = memberObject.GetType();
        PropertyInfo valueProperty = memberType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
        if (valueProperty == null)
        {
            return "";
        }

        object value = valueProperty.GetValue(memberObject, null);
        return value as string ?? "";
    }

    private static bool GetBoolMember(object source, string memberName)
    {
        if (source == null || string.IsNullOrEmpty(memberName))
        {
            return false;
        }
        // Implementation missing, add as needed
        return false;
    }

}
