using System;
using System.Collections.Generic;
using HarmonyLib;

namespace BiomePaints
{
    [HarmonyPatch(typeof(XUiC_MaterialWindow), nameof(XUiC_MaterialWindow.FilterByName))]
    public static class Patch_XUiC_MaterialWindow
    {
        private static bool Matches(string source, string query)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(query))
            {
                return false;
            }
            return source.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool Prefix(
            XUiC_MaterialWindow __instance,
            string _name,
            List<BlockTextureData> ___currentItems,
            int ___length,
            XUiC_Paging ___pager,
            XUiV_Label ___resultCount,
            string ___lblTotal)
        {
            bool isCreative = GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) && GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled);
            ___currentItems.Clear();

            bool hasQuery = !string.IsNullOrEmpty(_name);

            if (BlockTextureData.list != null)
            {
                for (int i = 0; i < BlockTextureData.list.Length; i++)
                {
                    BlockTextureData blockTextureData = BlockTextureData.list[i];
                    if (blockTextureData == null || (blockTextureData.Hidden && !isCreative))
                    {
                        continue;
                    }

                    if (hasQuery)
                    {
                        string name = blockTextureData.Name;
                        string locName = blockTextureData.LocalizedName;
                        string group = blockTextureData.Group;
                        string locGroup = !string.IsNullOrEmpty(group) ? Localization.Get(group) : null;

                        bool matches = Matches(name, _name)
                            || Matches(locName, _name)
                            || Matches(group, _name)
                            || Matches(locGroup, _name);

                        // Also allow searching "biome" to match our biome terrain paints (ID 201..205)
                        if (!matches && blockTextureData.ID >= 201 && blockTextureData.ID <= 205)
                        {
                            if (Matches("biome", _name) || Matches(_name, "biome"))
                            {
                                matches = true;
                            }
                        }

                        if (matches)
                        {
                            ___currentItems.Add(blockTextureData);
                        }
                    }
                    else
                    {
                        ___currentItems.Add(blockTextureData);
                    }
                }
            }

            ___pager?.SetLastPageByElementsAndPageLength(___currentItems.Count, ___length);
            if (___resultCount != null && !string.IsNullOrEmpty(___lblTotal))
            {
                ___resultCount.Text = string.Format(___lblTotal, ___currentItems.Count.ToString());
            }

            return false; // Skip original method
        }
    }
}
