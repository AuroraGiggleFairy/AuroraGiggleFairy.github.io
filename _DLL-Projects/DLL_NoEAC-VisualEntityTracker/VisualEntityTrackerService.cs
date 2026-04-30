using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NoEACVisualEntityTracker
{
    public static class VisualEntityTrackerService
    {
        private const bool UseDllNavTracking = true;
        private const float ScanRadiusMeters = 20f;
        private const float ScanIntervalSeconds = 0.25f;
        private const int MissingScanGraceCount = 2;

        private const string CVarAnyNearby = "agf_vet_any_count_20m";
        private const string CVarEnemyNearby = "agf_vet_enemy_count_20m";
        private const string CVarZombieNearby = "agf_vet_zombie_count_20m";
        private const string CVarAnimalEnemyNearby = "agf_vet_enemy_animal_count_20m";
        private const string CVarNearestDistance = "agf_vet_nearest_distance_20m";
        private const string CVarNearestBearing = "agf_vet_nearest_bearing";

        private static readonly List<Entity> EntityBuffer = new List<Entity>(128);
        private static readonly Dictionary<int, NavObject> DllTrackedNavObjects = new Dictionary<int, NavObject>(128);
        private static readonly Dictionary<int, int> MissingScanCounts = new Dictionary<int, int>(128);
        private static readonly Dictionary<string, string> NavClassByEntityLabel = new Dictionary<string, string>(128);
        private static readonly HashSet<string> InvalidNavClasses = new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<int> SeenEntityIds = new HashSet<int>();
        private static readonly List<int> StaleEntityIds = new List<int>(128);
        private static readonly List<int> MissingCountRemovalIds = new List<int>(128);

        private static int lastAnyCount = int.MinValue;
        private static int lastEnemyCount = int.MinValue;
        private static int lastZombieCount = int.MinValue;
        private static int lastEnemyAnimalCount = int.MinValue;
        private static float lastNearestDistance = float.MinValue;
        private static float lastNearestBearing = float.MinValue;
        private static float lastScanTime;
        private static bool trackerEnabledLastFrame = true;
        private static int serverBuffEntityId = int.MinValue;
        private static bool? serverBuffEnabledState;

        public static void RefreshTrackerMode()
        {
            trackerEnabledLastFrame = false;
            serverBuffEntityId = int.MinValue;
            serverBuffEnabledState = null;
        }

        // Called immediately when the local player spawns/respawns so buffZAlert is
        // stripped before its first onSelfBuffUpdate cycle fires. Vanilla clients
        // (no DLL) never call this and keep their buffZAlert normally.
        public static void OnLocalPlayerSpawned()
        {
            if (GameManager.IsDedicatedServer)
            {
                return;
            }

            World world = GameManager.Instance?.World;
            EntityPlayerLocal player = world?.GetPrimaryPlayer();
            if (player?.Buffs == null)
            {
                return;
            }

            object buffs = player.Buffs;
            if (InvokeHasBuff(buffs, "buffZAlert"))
            {
                InvokeBuffMethod(buffs, "RemoveBuff", "buffZAlert");
            }

            RefreshTrackerMode();
        }

        // Called server-side from chat/console commands to apply buffZAlert directly
        // on the target player entity. DLL clients will suppress the buff themselves
        // on their next Tick(); vanilla clients (no DLL) rely on this entirely.
        public static void ApplyServerSideMode(int entityId, bool enabled)
        {
            if (entityId < 0)
            {
                return;
            }

            World world = GameManager.Instance?.World;
            if (world == null)
            {
                return;
            }

            EntityPlayer player = world.GetEntity(entityId) as EntityPlayer;
            if (player?.Buffs == null)
            {
                return;
            }

            bool hasBuff = InvokeHasBuff(player.Buffs, "buffZAlert");
            if (enabled && !hasBuff)
            {
                InvokeBuffMethod(player.Buffs, "AddBuff", "buffZAlert");
            }
            else if (!enabled && hasBuff)
            {
                InvokeBuffMethod(player.Buffs, "RemoveBuff", "buffZAlert");
            }
        }

        public static void Tick()
        {
            if (GameManager.IsDedicatedServer)
            {
                return;
            }

            if (Time.realtimeSinceStartup - lastScanTime < ScanIntervalSeconds)
            {
                return;
            }

            lastScanTime = Time.realtimeSinceStartup;

            World world = GameManager.Instance?.World;
            EntityPlayerLocal player = world?.GetPrimaryPlayer();
            if (world == null || player == null || player.Buffs == null || player.IsDead())
            {
                CleanupAllDllNavObjects();
                return;
            }

            bool trackerEnabled = VisualEntityTrackerModeSettings.GetModeForLocalPlayer(VisualEntityTrackerMode.On) != VisualEntityTrackerMode.Off;
            EnsureServerSideBuffState(player, trackerEnabled);
            if (!trackerEnabled)
            {
                if (trackerEnabledLastFrame)
                {
                    CleanupAllDllNavObjects();
                    SetTrackerCVars(player, 0, 0, 0, 0, 0f, 0f);
                }

                trackerEnabledLastFrame = false;
                return;
            }

            trackerEnabledLastFrame = true;

            Vector3 playerPos = player.GetPosition();
            float radius = ScanRadiusMeters;
            float radiusSq = radius * radius;
            var bounds = new Bounds(playerPos, new Vector3(radius * 2f, radius * 2f, radius * 2f));

            EntityBuffer.Clear();
            world.GetEntitiesInBounds(typeof(EntityAlive), bounds, EntityBuffer);

            int anyCount = 0;
            int enemyCount = 0;
            int zombieCount = 0;
            int enemyAnimalCount = 0;

            float nearestDistSq = float.MaxValue;
            Vector3 nearestOffset = Vector3.zero;
            SeenEntityIds.Clear();

            for (int i = 0; i < EntityBuffer.Count; i++)
            {
                Entity entity = EntityBuffer[i];
                if (!(entity is EntityAlive alive))
                {
                    continue;
                }

                if (alive.entityId == player.entityId || alive.IsDead())
                {
                    continue;
                }

                // Do not track sleeper-form zombies until they wake.
                if (alive is EntityZombie && (alive.IsSleeperPassive || alive.IsSleeping))
                {
                    continue;
                }

                Vector3 offset = alive.GetPosition() - playerPos;
                float distSq = offset.sqrMagnitude;
                if (distSq > radiusSq)
                {
                    continue;
                }

                anyCount++;

                if (alive is EntityEnemy)
                {
                    enemyCount++;
                }

                if (alive is EntityZombie)
                {
                    zombieCount++;
                }

                if (alive is EntityEnemyAnimal)
                {
                    enemyAnimalCount++;
                }

                if (distSq < nearestDistSq)
                {
                    nearestDistSq = distSq;
                    nearestOffset = offset;
                }

                if (UseDllNavTracking)
                {
                    string navClassName = ResolveDllNavClass(alive);
                    if (!string.IsNullOrEmpty(navClassName))
                    {
                        SeenEntityIds.Add(alive.entityId);
                        MissingScanCounts[alive.entityId] = 0;
                        UpsertDllNavObject(alive, navClassName);
                    }
                }
            }

            if (UseDllNavTracking)
            {
                RemoveStaleDllNavObjects();
            }

            float nearestDistance = nearestDistSq < float.MaxValue ? Mathf.Sqrt(nearestDistSq) : 0f;
            float nearestBearing = nearestDistSq < float.MaxValue ? ComputeSignedBearing(player, nearestOffset) : 0f;

            // These CVars are client-local signals for XML/NCalc rendering.
            if (anyCount != lastAnyCount)
            {
                player.Buffs.SetCustomVar(CVarAnyNearby, anyCount, false);
                lastAnyCount = anyCount;
            }

            if (enemyCount != lastEnemyCount)
            {
                player.Buffs.SetCustomVar(CVarEnemyNearby, enemyCount, false);
                lastEnemyCount = enemyCount;
            }

            if (zombieCount != lastZombieCount)
            {
                player.Buffs.SetCustomVar(CVarZombieNearby, zombieCount, false);
                lastZombieCount = zombieCount;
            }

            if (enemyAnimalCount != lastEnemyAnimalCount)
            {
                player.Buffs.SetCustomVar(CVarAnimalEnemyNearby, enemyAnimalCount, false);
                lastEnemyAnimalCount = enemyAnimalCount;
            }

            if (!Mathf.Approximately(nearestDistance, lastNearestDistance))
            {
                player.Buffs.SetCustomVar(CVarNearestDistance, nearestDistance, false);
                lastNearestDistance = nearestDistance;
            }

            if (!Mathf.Approximately(nearestBearing, lastNearestBearing))
            {
                player.Buffs.SetCustomVar(CVarNearestBearing, nearestBearing, false);
                lastNearestBearing = nearestBearing;
            }
        }

        private static void SetTrackerCVars(EntityPlayerLocal player, int anyCount, int enemyCount, int zombieCount, int enemyAnimalCount, float nearestDistance, float nearestBearing)
        {
            if (player?.Buffs == null)
            {
                return;
            }

            player.Buffs.SetCustomVar(CVarAnyNearby, anyCount, false);
            player.Buffs.SetCustomVar(CVarEnemyNearby, enemyCount, false);
            player.Buffs.SetCustomVar(CVarZombieNearby, zombieCount, false);
            player.Buffs.SetCustomVar(CVarAnimalEnemyNearby, enemyAnimalCount, false);
            player.Buffs.SetCustomVar(CVarNearestDistance, nearestDistance, false);
            player.Buffs.SetCustomVar(CVarNearestBearing, nearestBearing, false);

            lastAnyCount = anyCount;
            lastEnemyCount = enemyCount;
            lastZombieCount = zombieCount;
            lastEnemyAnimalCount = enemyAnimalCount;
            lastNearestDistance = nearestDistance;
            lastNearestBearing = nearestBearing;
        }

        private static void EnsureServerSideBuffState(EntityPlayerLocal player, bool enabled)
        {
            if (player?.Buffs == null)
            {
                return;
            }

            int entityId = player.entityId;
            if (serverBuffEntityId != entityId)
            {
                serverBuffEntityId = entityId;
                serverBuffEnabledState = null;
            }

            if (serverBuffEnabledState.HasValue && serverBuffEnabledState.Value == enabled)
            {
                return;
            }

            TrySetServerSideTrackerBuff(player, enabled);
            serverBuffEnabledState = enabled;
        }

        private static void TrySetServerSideTrackerBuff(EntityPlayerLocal player, bool enabled)
        {
            // DLL clients always suppress the server-side ZA buff chain.
            // VETDLL nav classes handle tracking when enabled; the on/off toggle
            // is enforced by gating Tick() instead.
            // Clients without the DLL receive ZA tracking passively via the
            // spawn buff added in entityclasses.xml — they are unaffected here.
            object buffs = player?.Buffs;
            if (buffs == null)
            {
                return;
            }

            if (InvokeHasBuff(buffs, "buffZAlert"))
            {
                InvokeBuffMethod(buffs, "RemoveBuff", "buffZAlert");
            }
        }

        private static bool InvokeHasBuff(object buffs, string buffName)
        {
            if (buffs == null || string.IsNullOrEmpty(buffName))
            {
                return false;
            }

            Type type = buffs.GetType();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (!string.Equals(method.Name, "HasBuff", StringComparison.Ordinal) || method.ReturnType != typeof(bool))
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                {
                    try
                    {
                        object result = method.Invoke(buffs, new object[] { buffName });
                        if (result is bool parsed)
                        {
                            return parsed;
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return false;
        }

        private static void InvokeBuffMethod(object buffs, string methodName, string buffName)
        {
            if (buffs == null || string.IsNullOrEmpty(methodName) || string.IsNullOrEmpty(buffName))
            {
                return;
            }

            Type type = buffs.GetType();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (!string.Equals(method.Name, methodName, StringComparison.Ordinal))
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                {
                    try
                    {
                        method.Invoke(buffs, new object[] { buffName });
                        return;
                    }
                    catch
                    {
                    }
                }

                if (parameters.Length == 2 && parameters[0].ParameterType == typeof(string) && parameters[1].ParameterType == typeof(bool))
                {
                    try
                    {
                        method.Invoke(buffs, new object[] { buffName, false });
                        return;
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static void UpsertDllNavObject(EntityAlive entity, string navClassName)
        {
            NavObjectManager navManager = NavObjectManager.Instance;
            if (navManager == null || entity == null)
            {
                return;
            }

            int entityId = entity.entityId;
            if (DllTrackedNavObjects.TryGetValue(entityId, out NavObject existing) && existing != null && existing.IsValid())
            {
                string existingClass = existing.NavObjectClass?.NavObjectClassName;
                if (string.Equals(existingClass, navClassName, StringComparison.Ordinal))
                {
                    existing.TrackedEntity = entity;
                    return;
                }

                navManager.UnRegisterNavObject(existing);
                DllTrackedNavObjects.Remove(entityId);
            }

            NavObject registered = TryRegisterNavObject(navManager, navClassName, entity);
            if (registered != null)
            {
                registered.hiddenOnCompass = false;
                DllTrackedNavObjects[entityId] = registered;
            }
        }

        private static NavObject TryRegisterNavObject(NavObjectManager navManager, string navClassName, EntityAlive entity)
        {
            if (navManager == null || entity == null || string.IsNullOrEmpty(navClassName))
            {
                return null;
            }

            // Nav classes can be unavailable briefly during load/reload windows.
            // Do not blacklist in that case so we can retry on later ticks.
            if (NavObjectClass.GetNavObjectClass(navClassName) == null)
            {
                return null;
            }

            if (!InvalidNavClasses.Contains(navClassName))
            {
                try
                {
                    return navManager.RegisterNavObject(navClassName, entity, "", false);
                }
                catch (Exception ex)
                {
                    InvalidNavClasses.Add(navClassName);
                    Debug.LogWarning("[VisualEntityTracker] Nav class registration failed for '" + navClassName + "': " + ex.Message);
                }
            }

            string fallbackClass = GetFallbackNavClass(navClassName);
            if (!string.IsNullOrEmpty(fallbackClass)
                && !InvalidNavClasses.Contains(fallbackClass)
                && NavObjectClass.GetNavObjectClass(fallbackClass) != null)
            {
                try
                {
                    return navManager.RegisterNavObject(fallbackClass, entity, "", false);
                }
                catch (Exception ex)
                {
                    InvalidNavClasses.Add(fallbackClass);
                    Debug.LogWarning("[VisualEntityTracker] Fallback nav class registration failed for '" + fallbackClass + "': " + ex.Message);
                }
            }

            return null;
        }

        private static string GetFallbackNavClass(string navClassName)
        {
            if (string.Equals(navClassName, "VETDLLSwarm", StringComparison.Ordinal))
            {
                // Swarm fallback uses the existing bat icon class.
                return "VETDLLVulture";
            }

            return "VETDLLBandit";
        }

        private static void RemoveStaleDllNavObjects()
        {
            if (DllTrackedNavObjects.Count == 0)
            {
                return;
            }

            StaleEntityIds.Clear();
            foreach (var pair in DllTrackedNavObjects)
            {
                bool missingFromScan = !SeenEntityIds.Contains(pair.Key);
                bool invalidNav = pair.Value == null || !pair.Value.IsValid();
                if (invalidNav)
                {
                    if (pair.Value != null)
                    {
                        NavObjectManager.Instance.UnRegisterNavObject(pair.Value);
                    }
                    StaleEntityIds.Add(pair.Key);
                    continue;
                }

                if (missingFromScan)
                {
                    int misses = 0;
                    MissingScanCounts.TryGetValue(pair.Key, out misses);
                    misses++;
                    MissingScanCounts[pair.Key] = misses;

                    if (misses > MissingScanGraceCount)
                    {
                        if (pair.Value != null)
                        {
                            NavObjectManager.Instance.UnRegisterNavObject(pair.Value);
                        }
                        StaleEntityIds.Add(pair.Key);
                    }
                }
            }

            for (int i = 0; i < StaleEntityIds.Count; i++)
            {
                int id = StaleEntityIds[i];
                DllTrackedNavObjects.Remove(id);
                MissingScanCounts.Remove(id);
            }

            MissingCountRemovalIds.Clear();
            foreach (var pair in MissingScanCounts)
            {
                if (!DllTrackedNavObjects.ContainsKey(pair.Key))
                {
                    MissingCountRemovalIds.Add(pair.Key);
                }
            }

            for (int i = 0; i < MissingCountRemovalIds.Count; i++)
            {
                MissingScanCounts.Remove(MissingCountRemovalIds[i]);
            }
        }

        private static void CleanupAllDllNavObjects()
        {
            if (DllTrackedNavObjects.Count == 0)
            {
                return;
            }

            NavObjectManager navManager = NavObjectManager.Instance;
            foreach (var pair in DllTrackedNavObjects)
            {
                if (pair.Value != null)
                {
                    navManager.UnRegisterNavObject(pair.Value);
                }
            }

            DllTrackedNavObjects.Clear();
            MissingScanCounts.Clear();
            NavClassByEntityLabel.Clear();
            InvalidNavClasses.Clear();
            SeenEntityIds.Clear();
            StaleEntityIds.Clear();
            MissingCountRemovalIds.Clear();

            lastAnyCount = int.MinValue;
            lastEnemyCount = int.MinValue;
            lastZombieCount = int.MinValue;
            lastEnemyAnimalCount = int.MinValue;
            lastNearestDistance = float.MinValue;
            lastNearestBearing = float.MinValue;
        }

        private static string ResolveDllNavClass(EntityAlive alive)
        {
            if (alive == null)
            {
                return null;
            }

            if (alive is EntityZombie)
            {
                return "VETDLLZombie";
            }

            if (alive is EntitySwarm)
            {
                return "VETDLLSwarm";
            }

            string typeName = alive.GetType().Name;
            string label = ((alive.EntityName ?? string.Empty) + " " + typeName).ToLowerInvariant();
            if (NavClassByEntityLabel.TryGetValue(label, out string cached))
            {
                return cached;
            }

            string navClassName;
            if (label.Contains("bear")) navClassName = "VETDLLBear";
            else if (label.Contains("chicken")) navClassName = "VETDLLChicken";
            else if (label.Contains("deer") || label.Contains("stag") || label.Contains("doe")) navClassName = "VETDLLDeer";
            else if (label.Contains("rabbit")) navClassName = "VETDLLRabbit";
            else if (label.Contains("mountainlion") || label.Contains("mountain_lion") || label.Contains("lion")) navClassName = "VETDLLMountainLion";
            else if (label.Contains("vulture")) navClassName = "VETDLLVulture";
            else if (label.Contains("snake")) navClassName = "VETDLLSnake";
            else if (label.Contains("boar") || label.Contains("pig") || label.Contains("grace")) navClassName = "VETDLLBoar";
            else if (label.Contains("swarm") || label.Contains("bee") || label.Contains("insect") || label.Contains("wasp") || label.Contains("hornet")) navClassName = "VETDLLSwarm";
            else if (typeName.IndexOf("Swarm", StringComparison.OrdinalIgnoreCase) >= 0) navClassName = "VETDLLSwarm";
            else if (label.Contains("wolf") || label.Contains("coyote") || label.Contains("dog")) navClassName = "VETDLLWolf";
            else if (alive is EntityEnemy) navClassName = "VETDLLBandit";
            else navClassName = null;

            // Do not cache null: some entities populate names lazily and become classifiable a few ticks later.
            if (!string.IsNullOrEmpty(navClassName))
            {
                NavClassByEntityLabel[label] = navClassName;
            }
            return navClassName;
        }

        private static float ComputeSignedBearing(EntityPlayerLocal player, Vector3 worldOffset)
        {
            Vector3 forward = player?.cameraTransform != null ? player.cameraTransform.forward : player.GetForwardVector();
            forward.y = 0f;
            worldOffset.y = 0f;

            if (forward.sqrMagnitude < 0.0001f || worldOffset.sqrMagnitude < 0.0001f)
            {
                return 0f;
            }

            forward.Normalize();
            worldOffset.Normalize();

            float dot = Mathf.Clamp(Vector3.Dot(forward, worldOffset), -1f, 1f);
            float det = forward.x * worldOffset.z - forward.z * worldOffset.x;
            return Mathf.Atan2(det, dot) * Mathf.Rad2Deg;
        }
    }
}
