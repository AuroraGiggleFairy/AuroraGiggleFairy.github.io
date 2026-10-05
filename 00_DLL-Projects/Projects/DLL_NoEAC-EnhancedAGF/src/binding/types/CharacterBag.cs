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
using UnityEngine;

static class BagEncumbrance
{
    public const string IconNormal = "ui_game_symbol_backpack";
    // Same glyph as vanilla buffEncumberedInv.
    public const string IconEncumbered = "ui_game_symbol_pack_mule";
    public const string ColorNormal = "[ff8000]";
    // HUDPlus lock-icon red (255,0,0), chosen so it stays readable on the dark compass bar.
    public const string ColorEncumbered = "[FF0000]";

    public static bool IsEncumbered(EntityPlayer player)
    {
        if (player?.bag == null)
        {
            return false;
        }

        int carry = (int)EffectManager.GetValue(PassiveEffects.CarryCapacity, null, 0f, player);
        return player.bag.GetUsedSlotCount() > carry;
    }
}

public class BagUsedSlots : Binding
{
    private const float RefreshIntervalSeconds = 0.15f;
    private float nextRefreshTime;
    private string cachedValue = "0";

    public BagUsedSlots(int value, string name) : base(value, name)
    {
    }

    public override string GetCurrentValue(EntityPlayer player)
    {
        float now = Time.realtimeSinceStartup;
        if (now < nextRefreshTime)
        {
            return cachedValue;
        }

        cachedValue = player.bag.GetUsedSlotCount().ToString();
        nextRefreshTime = now + RefreshIntervalSeconds;
        return cachedValue;
    }
}

public class BagCarryCapacity : Binding
{
    private const float RefreshIntervalSeconds = 0.15f;
    private float nextRefreshTime;
    private string cachedValue = "0";

    public BagCarryCapacity(int value, string name) : base(value, name)
    {
    }

    public override string GetCurrentValue(EntityPlayer player)
    {
        float now = Time.realtimeSinceStartup;
        if (now < nextRefreshTime)
        {
            return cachedValue;
        }

        cachedValue = MathUtils.Min(player.CarryCapacity, player.bag.SlotCount).ToString();
        nextRefreshTime = now + RefreshIntervalSeconds;
        return cachedValue;
    }
}

public class BagMaxCarryCapacity : Binding
{
    private const float RefreshIntervalSeconds = 0.15f;
    private float nextRefreshTime;
    private string cachedValue = "0";

    public BagMaxCarryCapacity(int value, string name) : base(value, name)
    {
    }

    public override string GetCurrentValue(EntityPlayer player)
    {
        float now = Time.realtimeSinceStartup;
        if (now < nextRefreshTime)
        {
            return cachedValue;
        }

        cachedValue = player.CarryCapacity.ToString();
        nextRefreshTime = now + RefreshIntervalSeconds;
        return cachedValue;
    }
}

public class BagSize: Binding
{
    private const float RefreshIntervalSeconds = 0.15f;
    private float nextRefreshTime;
    private string cachedValue = "0";

    public BagSize(int value, string name) : base(value, name)
    {
    }

    public override string GetCurrentValue(EntityPlayer player)
    {
        float now = Time.realtimeSinceStartup;
        if (now < nextRefreshTime)
        {
            return cachedValue;
        }

        // Bag size is the slot count. On 3.3 that starts at 48. Carry capacity (32 at a new game) is the encumbrance line, not the slot total.
        cachedValue = player.bag.SlotCount.ToString();
        nextRefreshTime = now + RefreshIntervalSeconds;
        return cachedValue;
    }
}

public class BagUsedSlotsColor : Binding
{
    private const float RefreshIntervalSeconds = 0.15f;
    private float nextRefreshTime;
    private string cachedValue = BagEncumbrance.ColorNormal;

    public BagUsedSlotsColor(int value, string name) : base(value, name)
    {
    }

    public override string GetCurrentValue(EntityPlayer player)
    {
        float now = Time.realtimeSinceStartup;
        if (now < nextRefreshTime)
        {
            return cachedValue;
        }

        cachedValue = BagEncumbrance.IsEncumbered(player)
            ? BagEncumbrance.ColorEncumbered
            : BagEncumbrance.ColorNormal;
        nextRefreshTime = now + RefreshIntervalSeconds;
        return cachedValue;
    }
}

public class BagIcon : Binding
{
    private const float RefreshIntervalSeconds = 0.15f;
    private float nextRefreshTime;
    private string cachedValue = BagEncumbrance.IconNormal;

    public BagIcon(int value, string name) : base(value, name)
    {
    }

    public override string GetCurrentValue(EntityPlayer player)
    {
        float now = Time.realtimeSinceStartup;
        if (now < nextRefreshTime)
        {
            return cachedValue;
        }

        cachedValue = BagEncumbrance.IsEncumbered(player)
            ? BagEncumbrance.IconEncumbered
            : BagEncumbrance.IconNormal;
        nextRefreshTime = now + RefreshIntervalSeconds;
        return cachedValue;
    }
}
