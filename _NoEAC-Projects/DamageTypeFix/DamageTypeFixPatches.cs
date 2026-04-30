using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace DamageTypeFix
{
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.ExplosionServer))]
    internal static class Patch_GameManager_ExplosionServer
    {
        private static bool Prefix(ref ExplosionData _explosionData, ItemValue _itemValueExplosionSource)
        {
            try
            {
                if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                {
                    return true;
                }

                if (_explosionData.DamageType != EnumDamageTypes.Heat)
                {
                    return true;
                }

                if (TryResolveExplosionDamageType(_itemValueExplosionSource, out EnumDamageTypes resolvedType)
                    && resolvedType != EnumDamageTypes.Heat)
                {
                    _explosionData.DamageType = resolvedType;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[DamageTypeFix] Failed to resolve explosion damage type: " + ex.Message);
            }

            return true;
        }

        private static bool TryResolveExplosionDamageType(ItemValue itemValue, out EnumDamageTypes resolvedType)
        {
            resolvedType = EnumDamageTypes.Heat;
            if (itemValue == null || itemValue.ItemClass == null || itemValue.ItemClass.Properties == null)
            {
                return false;
            }

            DynamicProperties properties = itemValue.ItemClass.Properties;

            if (TryParseKey(properties, "Explosion.DamageType", out resolvedType))
            {
                return true;
            }

            if (properties.Values != null && properties.Values.Dict != null)
            {
                foreach (KeyValuePair<string, string> kvp in properties.Values.Dict)
                {
                    if (kvp.Key == null || kvp.Value == null)
                    {
                        continue;
                    }

                    if (!kvp.Key.EndsWith("Explosion.DamageType", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (EnumUtils.TryParse(kvp.Value, out resolvedType, true))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryParseKey(DynamicProperties properties, string key, out EnumDamageTypes damageType)
        {
            damageType = EnumDamageTypes.Heat;
            if (properties == null || properties.Values == null)
            {
                return false;
            }

            if (!properties.Values.TryGetValue(key, out string value) || string.IsNullOrEmpty(value))
            {
                return false;
            }

            return EnumUtils.TryParse(value, out damageType, true);
        }
    }
}
