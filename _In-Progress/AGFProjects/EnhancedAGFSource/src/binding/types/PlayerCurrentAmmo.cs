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

public class PlayerCurrentAmmoIcon : Binding
{
    public PlayerCurrentAmmoIcon(int value, string name) : base(value, name)
    {
    }

    public override string GetCurrentValue(EntityPlayer player)
    {
        if (player == null)
            return "";
        var inventory = player.inventory;
        if (inventory == null)
            return "";
        var heldValue = inventory.holdingItemItemValue;
        if (heldValue == null)
            return "";
        var itemClass = heldValue.ItemClass;
        if (itemClass == null)
            return "";
        var actions = itemClass.Actions;
        if (actions == null)
            return "";
        bool isWeapon = false;
        foreach (var action in actions)
        {
            if (action is ItemActionRanged)
            {
                isWeapon = true;
                break;
            }
            var type = action?.GetType();
            if (type != null && type.Name.IndexOf("Launcher", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                isWeapon = true;
                break;
            }
        }
        if (!isWeapon)
            return "";
        return AmmoBindingUtil.GetCurrentAmmoIconName(player);
    }
}

public class PlayerCurrentAmmoCount : Binding
{
    public PlayerCurrentAmmoCount(int value, string name) : base(value, name)
    {
    }

    public override string GetCurrentValue(EntityPlayer player)
    {
        if (player == null)
            return " ";
        var inventory = player.inventory;
        if (inventory == null)
            return " ";
        var heldValue = inventory.holdingItemItemValue;
        if (heldValue == null)
            return " ";
        var itemClass = heldValue.ItemClass;
        if (itemClass == null)
            return " ";
        var actions = itemClass.Actions;
        if (actions == null)
            return " ";
        bool isWeapon = false;
        foreach (var action in actions)
        {
            if (action is ItemActionRanged)
            {
                isWeapon = true;
                break;
            }
            var type = action?.GetType();
            if (type != null && type.Name.IndexOf("Launcher", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                isWeapon = true;
                break;
            }
        }
        if (!isWeapon)
            return " ";
        AmmoBindingUtil.GetCurrentAmmoInfo(player, out ItemValue ammoItemValue, out int magazineCount);
        if (ammoItemValue == null || ammoItemValue.ItemClass == null)
        {
            return " ";
        }
        // Special handling for FlameThrowerQ weapons
        var heldValueFT = player?.inventory?.holdingItemItemValue;
        var heldNameFT = heldValueFT?.ItemClass?.Name ?? "";
            // Special display for BFG9001
            if (heldNameFT == "BFG9001")
            {
                // Explicitly count all PlasmaCellDM in toolbelt and bag
                int total = 0;
                string ammoName = "PlasmaCellDM";
                // Toolbelt
                if (player.inventory != null && player.inventory.GetSlots() != null)
                {
                    var toolbeltSlots = player.inventory.GetSlots();
                    for (int i = 0; i < toolbeltSlots.Length; i++)
                    {
                        var stack = toolbeltSlots[i];
                        if (stack != null && stack.count > 0 && stack.itemValue != null && stack.itemValue.ItemClass != null)
                        {
                            if (stack.itemValue.ItemClass.Name == ammoName)
                            {
                                total += stack.count;
                            }
                        }
                    }
                }
                // Backpack
                if (player.bag != null && player.bag.GetSlots() != null)
                {
                    var bagSlots = player.bag.GetSlots();
                    for (int i = 0; i < bagSlots.Length; i++)
                    {
                        var stack = bagSlots[i];
                        if (stack != null && stack.count > 0 && stack.itemValue != null && stack.itemValue.ItemClass != null)
                        {
                            if (stack.itemValue.ItemClass.Name == ammoName)
                            {
                                total += stack.count;
                            }
                        }
                    }
                }
                if (total < 0) total = 0;
                int y = total / 40;
                return $"{total} / {y}";
            }
            if (heldNameFT == "FlameThrowerQ1" || heldNameFT == "FlameThrowerQ2" || heldNameFT == "FlameThrowerQ3" || heldNameFT == "FlameThrowerQ4" || heldNameFT == "FlameThrowerQ5" || heldNameFT == "FlameThrowerQ6")
        {
            int total = magazineCount;
            if (total < 0) total = 0;
            return total.ToString();
        }
        // For all other weapons, use only magazineCount to avoid double-counting
        int totalAll = magazineCount;
        if (totalAll < 0) totalAll = 0;
        return totalAll.ToString();
    }
}

public class PlayerCurrentAmmoVisible : Binding
{
    public PlayerCurrentAmmoVisible(int value, string name) : base(value, name)
    {
    }

    public override string GetCurrentValue(EntityPlayer player)
    {
        if (player == null)
            return "false";
        var inventory = player.inventory;
        if (inventory == null)
            return "false";
        var heldValue = inventory.holdingItemItemValue;
        if (heldValue == null)
            return "false";
        var itemClass = heldValue.ItemClass;
        if (itemClass == null)
            return "false";
        var actions = itemClass.Actions;
        if (actions == null)
            return "false";
        bool isWeapon = false;
        foreach (var action in actions)
        {
            if (action is ItemActionRanged)
            {
                isWeapon = true;
                break;
            }
            var type = action?.GetType();
            if (type != null && type.Name.IndexOf("Launcher", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                isWeapon = true;
                break;
            }
        }
        if (!isWeapon)
            return "false";
        // Defensive: check again before calling GetCurrentAmmoInfo
        if (player == null || inventory == null || heldValue == null || itemClass == null || actions == null)
            return "false";
        try
        {
            AmmoBindingUtil.GetCurrentAmmoInfo(player, out ItemValue ammoItemValue, out int magazineCount);
            return (ammoItemValue != null && ammoItemValue.ItemClass != null) ? "true" : "false";
        }
        catch
        {
            return "false";
        }
    }
}

internal static class AmmoBindingUtil
{
    private static int callCounter = 0;
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
                int index = heldValue.SelectedAmmoTypeIndex;
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
                int index = heldValue.SelectedAmmoTypeIndex;
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
            int index = heldValue.SelectedAmmoTypeIndex;
            string[] ammoNames = ammoName.Split(',');
            if (index < 0 || index >= ammoNames.Length)
            {
                index = 0;
            }
            ammoName = ammoNames[index];
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

        string iconName = "";
        if (ammoAction is ItemActionRanged rangedAction)
        {
            if (rangedAction.BulletIcon != null)
            {
                iconName = rangedAction.BulletIcon.Value;
                if (!string.IsNullOrEmpty(iconName) && iconName.IndexOf(',') >= 0)
                {
                    int index = heldValue.SelectedAmmoTypeIndex;
                    string[] iconNames = iconName.Split(',');
                    if (index < 0 || index >= iconNames.Length)
                    {
                        index = 0;
                    }
                    iconName = iconNames[index];
                }
            }
        }
        else
        {
            iconName = GetMemberValuePropertyString(ammoAction, "BulletIcon");
            if (!string.IsNullOrEmpty(iconName) && iconName.IndexOf(',') >= 0)
            {
                int index = heldValue.SelectedAmmoTypeIndex;
                string[] iconNames = iconName.Split(',');
                if (index < 0 || index >= iconNames.Length)
                {
                    index = 0;
                }
                iconName = iconNames[index];
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
            ammoItemName = GetCurrentAmmoItemName(player);

        if (string.IsNullOrEmpty(ammoItemName))
        {
            return;
        }

        ammoItemValue = ItemClass.GetItem(ammoItemName);
        if (ammoItemValue == null)
        {
            return;
        }

        int total = 0;
        // Toolbelt
        if (player.inventory != null && player.inventory.GetSlots() != null)
        {
            var toolbeltSlots = player.inventory.GetSlots();
            for (int i = 0; i < toolbeltSlots.Length; i++)
            {
                var stack = toolbeltSlots[i];
                if (stack != null && stack.count > 0 && stack.itemValue != null && stack.itemValue.ItemClass != null)
                {
                    if (stack.itemValue.ItemClass.Id == ammoItemValue.ItemClass.Id)
                    {
                        total += stack.count;
                    }
                }
            }
        }
        // Backpack
        if (player.bag != null && player.bag.GetSlots() != null)
        {
            var bagSlots = player.bag.GetSlots();
            for (int i = 0; i < bagSlots.Length; i++)
            {
                var stack = bagSlots[i];
                if (stack != null && stack.count > 0 && stack.itemValue != null && stack.itemValue.ItemClass != null)
                {
                    if (stack.itemValue.ItemClass.Id == ammoItemValue.ItemClass.Id)
                    {
                        total += stack.count;
                    }
                }
            }
        }
        magazineCount = total;
        return;
    }

    public static int GetInventoryAmmoCount(Inventory inventory, int ammoItemId)
    {
        if (inventory == null || ammoItemId <= 0)
        {
            return 0;
        }

        int total = 0;
        ItemInventoryData[] slots = inventory.slots;
        if (slots == null)
        {
            return 0;
        }
        for (int i = 0; i < slots.Length; i++)
        {
            ItemInventoryData data = slots[i];
            if (data == null) continue;
            ItemStack stack = data.itemStack;
            if (stack.count <= 0 || stack.itemValue == null || stack.itemValue.ItemClass == null) continue;
            // Debug output: log item name and slot index
            string itemName = stack.itemValue.ItemClass.Name;
            LogError($"Slot {i}: {itemName}, count={stack.count}, itemId={stack.itemValue.ItemClass.Id}");
            if (stack.itemValue.ItemClass.Id == ammoItemId) total += stack.count;
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
