using System;
using HarmonyLib;
using UnityEngine;

namespace NoEACVisualEntityTracker
{
    [HarmonyPatch]
    public static class VETCompassClampPatch
    {
        private static bool edgeLeftVisible;
        private static bool edgeRightVisible;

        public static bool EdgeLeftVisible => edgeLeftVisible;
        public static bool EdgeRightVisible => edgeRightVisible;

        public static System.Reflection.MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(XUiC_CompassWindow), "updateNavObjects");
        }

        public static void Postfix(XUiC_CompassWindow __instance, EntityPlayerLocal localPlayer)
        {
            edgeLeftVisible = false;
            edgeRightVisible = false;

            if (__instance == null || localPlayer == null || __instance.waypointSpriteList == null)
            {
                return;
            }

            Entity baseEntity = localPlayer.AttachedToEntity != null ? localPlayer.AttachedToEntity : localPlayer;
            Transform cameraTransform = localPlayer.cameraTransform;
            if (baseEntity == null || cameraTransform == null)
            {
                return;
            }

            Vector3 entityPos = baseEntity.GetPosition();
            Vector2 playerPos2D = new Vector2(entityPos.x, entityPos.z);

            Vector3 forward3D = cameraTransform.forward;
            Vector2 forward2D = new Vector2(forward3D.x, forward3D.z);
            forward2D.Normalize();

            Vector3 right3D = cameraTransform.right;
            Vector2 right2D = new Vector2(right3D.x, right3D.z);
            right2D.Normalize();

            int spriteIndex = 0;
            var navObjects = NavObjectManager.Instance.NavObjectList;
            for (int i = 0; i < navObjects.Count; i++)
            {
                NavObject navObject = navObjects[i];
                if (navObject == null || navObject.hiddenOnCompass || !navObject.IsValid())
                {
                    continue;
                }

                if (spriteIndex >= __instance.waypointSpriteList.Count)
                {
                    break;
                }

                NavObjectCompassSettings settings = navObject.CurrentCompassSettings;
                if (settings == null)
                {
                    continue;
                }

                Vector3 navPos = navObject.GetPosition();
                Vector2 toNav = new Vector2(navPos.x + Origin.position.x, navPos.z + Origin.position.z) - playerPos2D;
                float distance = toNav.magnitude;
                if (distance < settings.MinDistance)
                {
                    continue;
                }

                float maxDistance = navObject.GetMaxDistance(settings, localPlayer);
                if (maxDistance != -1f && distance > maxDistance)
                {
                    continue;
                }

                bool usedHotZone = false;
                if (settings.HotZone != null)
                {
                    float hotZoneDistance = 1f;
                    if (settings.HotZone.HotZoneType == NavObjectCompassSettings.HotZoneSettings.HotZoneTypes.Treasure)
                    {
                        float extraData = navObject.ExtraData;
                        hotZoneDistance = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, extraData, localPlayer);
                        hotZoneDistance = Utils.FastClamp(hotZoneDistance, 0f, extraData);
                    }
                    else if (settings.HotZone.HotZoneType == NavObjectCompassSettings.HotZoneSettings.HotZoneTypes.Custom)
                    {
                        hotZoneDistance = settings.HotZone.CustomDistance;
                    }

                    if (distance < hotZoneDistance)
                    {
                        usedHotZone = true;
                    }
                }

                if (usedHotZone)
                {
                    ApplyVetDepth(__instance, spriteIndex, distance, maxDistance, navObject, settings);
                    spriteIndex++;
                    continue;
                }

                Vector2 normalized = toNav.normalized;
                float forwardDot = Vector2.Dot(normalized, forward2D);

                if (!settings.IconClamped && forwardDot < 0.75f)
                {
                    continue;
                }

                if (forwardDot < 0.75f && IsVetDllNavClass(navObject))
                {
                    float rightDot = Vector2.Dot(normalized, right2D);
                    if (rightDot < 0f)
                    {
                        edgeLeftVisible = true;
                    }
                    else
                    {
                        edgeRightVisible = true;
                    }
                }

                ApplyVetDepth(__instance, spriteIndex, distance, maxDistance, navObject, settings);
                spriteIndex++;
            }
        }

        private static void ApplyVetDepth(XUiC_CompassWindow compass, int spriteIndex, float distance, float maxDistance, NavObject navObject, NavObjectCompassSettings settings)
        {
            if (compass == null || compass.waypointSpriteList == null)
            {
                return;
            }

            if (spriteIndex < 0 || spriteIndex >= compass.waypointSpriteList.Count)
            {
                return;
            }

            if (!IsVetDllNavClass(navObject))
            {
                return;
            }

            int baseDepth = 12 + (settings?.DepthOffset ?? 0);
            float depthDistanceCap = maxDistance > 0f ? maxDistance : 60f;
            float nearRatio = 1f - Mathf.Clamp01(distance / Mathf.Max(1f, depthDistanceCap));

            // Keep VET in the same depth band as vanilla and only bias by a tiny near/far delta.
            int depth = baseDepth + Mathf.RoundToInt(nearRatio * 4f);
            compass.waypointSpriteList[spriteIndex].depth = depth;
        }

        private static bool IsVetDllNavClass(NavObject navObject)
        {
            string className = navObject?.NavObjectClass?.NavObjectClassName;
            return !string.IsNullOrEmpty(className) && className.StartsWith("VETDLL", StringComparison.Ordinal);
        }
    }
}