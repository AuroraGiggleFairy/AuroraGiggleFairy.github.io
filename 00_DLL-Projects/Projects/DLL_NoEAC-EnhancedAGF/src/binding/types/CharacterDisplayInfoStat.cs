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
using System.Collections.Generic;

public class CharacterDisplayInfoStat : Binding
{
    private const float RefreshIntervalSeconds = 0.1f;
    protected DisplayInfoEntry displayInfoEntry;
    private int displayInfoIndex;

    // Shared snapshot cache for all CharacterDisplayInfoStat bindings.
    private static float nextRefreshTime;
    private static readonly Dictionary<int, string> cachedValuesByIndex = new Dictionary<int, string>();

    public CharacterDisplayInfoStat(int value, string name, int displayInfoIndex) : base(value, name)
    {
        this.displayInfoIndex = displayInfoIndex;
    }

    public override string GetCurrentValue(EntityPlayer player)
    {
        if (displayInfoEntry == null)
        {
            displayInfoEntry = GetStatEntry(displayInfoIndex);

            if (displayInfoEntry == null)
            {
                return "";
            }
        }

        float now = Time.realtimeSinceStartup;
        if (now >= nextRefreshTime || cachedValuesByIndex.Count == 0)
        {
            RefreshSnapshot(player);
        }

        if (cachedValuesByIndex.TryGetValue(displayInfoIndex, out string cachedValue))
        {
            return cachedValue;
        }

        // Fallback in case the display info list changed between snapshot updates.
        string fallbackValue = XUiM_Player.GetStatValue(displayInfoEntry.StatType, player, displayInfoEntry,
            FastTags<TagGroup.Global>.none);
        cachedValuesByIndex[displayInfoIndex] = fallbackValue;
        return fallbackValue;
    }

    protected DisplayInfoEntry GetStatEntry(int index)
    {
        var displayInfoList = UIDisplayInfoManager.Current.GetCharacterDisplayInfo();
        if (displayInfoList.Count <= index)
        {
            return null;
        }
        return displayInfoList[index];
    }

    private static void RefreshSnapshot(EntityPlayer player)
    {
        var manager = UIDisplayInfoManager.Current;
        if (manager == null)
        {
            cachedValuesByIndex.Clear();
            nextRefreshTime = Time.realtimeSinceStartup + RefreshIntervalSeconds;
            return;
        }

        var displayInfoList = manager.GetCharacterDisplayInfo();
        cachedValuesByIndex.Clear();
        for (int i = 0; i < displayInfoList.Count; i++)
        {
            var entry = displayInfoList[i];
            cachedValuesByIndex[i] = XUiM_Player.GetStatValue(entry.StatType, player, entry,
                FastTags<TagGroup.Global>.none);
        }

        nextRefreshTime = Time.realtimeSinceStartup + RefreshIntervalSeconds;
    }
}