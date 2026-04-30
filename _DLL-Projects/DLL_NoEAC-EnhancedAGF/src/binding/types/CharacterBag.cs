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

public class BagUsedSlots : Binding
{
    private const float RefreshIntervalSeconds = 0.1f;
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
    private const float RefreshIntervalSeconds = 0.1f;
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

        cachedValue = MathUtils.Min(player.bag.MaxItemCount, player.bag.SlotCount).ToString();
        nextRefreshTime = now + RefreshIntervalSeconds;
        return cachedValue;
    }
}

public class BagMaxCarryCapacity : Binding
{
    private const float RefreshIntervalSeconds = 0.1f;
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

        cachedValue = player.bag.MaxItemCount.ToString();
        nextRefreshTime = now + RefreshIntervalSeconds;
        return cachedValue;
    }
}

public class BagSize: Binding
{
    private const float RefreshIntervalSeconds = 0.1f;
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

        cachedValue = player.bag.SlotCount.ToString();
        nextRefreshTime = now + RefreshIntervalSeconds;
        return cachedValue;
    }
}
