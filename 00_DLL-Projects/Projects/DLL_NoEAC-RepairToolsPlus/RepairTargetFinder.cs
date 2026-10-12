using UnityEngine;

namespace RepairToolsPlus
{
    public static class RepairTargetFinder
    {
        public static Vector3i CurrentTarget { get; private set; } = Vector3i.zero;

        public const float MaxPenetrationDepth = 3.5f;
        public const int AreaSearchRadius = 1;
        public const float MaxPlayerDistanceSq = 64f;

        public static bool CanRepairBlock(World world, Vector3i pos, BlockValue bv)
        {
            if (bv.isair || bv.damage <= 0) return false;
            Block block = bv.Block;
            if (block == null || !block.CanRepair(bv)) return false;

            bool isTerrain = block.shape.IsTerrain();
            if (RepairToolsSettings.CurrentFilter == DamageFilterMode.BlocksOnly && isTerrain) return false;
            if (RepairToolsSettings.CurrentFilter == DamageFilterMode.TerrainOnly && !isTerrain) return false;

            return true;
        }

        public static bool CanUpgradeBlock(World world, Vector3i pos, BlockValue bv)
        {
            if (bv.isair || bv.damage > 0) return false;
            Block block = bv.Block;
            if (block == null) return false;

            // Terrain cannot be upgraded
            if (block.shape.IsTerrain()) return false;

            string toBlock = block.Properties.GetString(Block.PropUpgradeBlockClass, Block.PropUpgradeBlockToBlock);
            if (string.IsNullOrEmpty(toBlock)) return false;

            return RepairToolsSettings.GetUpgradeTargetTier(block) <= RepairToolsSettings.CurrentCap;
        }

        public static Vector3i GetBestTarget(EntityPlayerLocal player, ItemActionRepair action = null, ItemActionData actionData = null)
        {
            CurrentTarget = Vector3i.zero;

            if (player == null || !player.IsAlive() || player.world == null)
            {
                return Vector3i.zero;
            }

            WorldRayHitInfo hitInfo = player.HitInfo;
            if (hitInfo == null || !hitInfo.bHitValid || !GameUtils.IsBlockOrTerrain(hitInfo.tag))
            {
                return Vector3i.zero;
            }

            World world = player.world;
            Vector3 playerPos = player.position;

            Vector3i surfacePos = hitInfo.hit.blockPos;
            BlockValue surfaceBv = world.GetBlock(surfacePos);

            if (surfaceBv.ischild && surfaceBv.Block?.multiBlockPos != null)
            {
                surfacePos = surfaceBv.Block.multiBlockPos.GetParentPos(surfacePos, surfaceBv);
                surfaceBv = world.GetBlock(surfacePos);
            }

            if (World.SandboxUseTraderArea == TraderAreaStates.Default && world.IsWithinTraderArea(surfacePos))
            {
                CurrentTarget = surfacePos;
                return surfacePos;
            }

            // If Damage Vision is toggled OFF (via F key):
            // Only target the single surface block you are aiming at
            if (!RepairToolsSettings.IsDamageVisionActive)
            {
                CurrentTarget = surfacePos;
                return surfacePos;
            }

            Vector3 hitPoint = hitInfo.hit.pos;
            Vector3 rayDir = hitInfo.ray.direction.normalized;

            // ==========================================
            // PRIORITY 1: REPAIRS (Damaged blocks first)
            // ==========================================

            // 1A. Surface block damaged
            if (CanRepairBlock(world, surfacePos, surfaceBv))
            {
                CurrentTarget = surfacePos;
                return surfacePos;
            }

            // 1B. Penetrating repair along gaze ray into wall
            for (float depth = 0.8f; depth <= MaxPenetrationDepth; depth += 0.8f)
            {
                Vector3 samplePoint = hitPoint + rayDir * depth;
                Vector3i deepPos = World.worldToBlockPos(samplePoint);

                if (deepPos == surfacePos) continue;

                BlockValue deepBv = world.GetBlock(deepPos);
                if (deepBv.ischild && deepBv.Block?.multiBlockPos != null)
                {
                    deepPos = deepBv.Block.multiBlockPos.GetParentPos(deepPos, deepBv);
                    deepBv = world.GetBlock(deepPos);
                }

                if (World.SandboxUseTraderArea == TraderAreaStates.Default && world.IsWithinTraderArea(deepPos)) continue;
                if ((deepPos.ToVector3() - playerPos).sqrMagnitude > MaxPlayerDistanceSq) continue;

                if (CanRepairBlock(world, deepPos, deepBv))
                {
                    CurrentTarget = deepPos;
                    return deepPos;
                }
            }

            // 1C. Auto-cycle repair in adjacent damaged blocks (3x3 area around crosshair)
            Vector3i bestDamagedAreaPos = Vector3i.zero;
            float closestDamagedDistSq = float.MaxValue;

            for (int x = surfacePos.x - AreaSearchRadius; x <= surfacePos.x + AreaSearchRadius; x++)
            {
                for (int y = surfacePos.y - AreaSearchRadius; y <= surfacePos.y + AreaSearchRadius; y++)
                {
                    for (int z = surfacePos.z - AreaSearchRadius; z <= surfacePos.z + AreaSearchRadius; z++)
                    {
                        Vector3i checkPos = new Vector3i(x, y, z);
                        if (checkPos == surfacePos) continue;

                        BlockValue checkBv = world.GetBlock(checkPos);
                        if (checkBv.ischild && checkBv.Block?.multiBlockPos != null)
                        {
                            checkPos = checkBv.Block.multiBlockPos.GetParentPos(checkPos, checkBv);
                            checkBv = world.GetBlock(checkPos);
                        }

                        if (World.SandboxUseTraderArea == TraderAreaStates.Default && world.IsWithinTraderArea(checkPos)) continue;
                        if ((checkPos.ToVector3() - playerPos).sqrMagnitude > MaxPlayerDistanceSq) continue;

                        if (CanRepairBlock(world, checkPos, checkBv))
                        {
                            float distSq = (checkPos.ToVector3Center() - hitPoint).sqrMagnitude;
                            if (distSq < closestDamagedDistSq)
                            {
                                closestDamagedDistSq = distSq;
                                bestDamagedAreaPos = checkPos;
                            }
                        }
                    }
                }
            }

            if (bestDamagedAreaPos != Vector3i.zero)
            {
                CurrentTarget = bestDamagedAreaPos;
                return bestDamagedAreaPos;
            }

            // ==========================================
            // PRIORITY 2: UPGRADES (Respecting selected cap)
            // ==========================================

            // 2A. Surface block can upgrade within cap
            if (CanUpgradeBlock(world, surfacePos, surfaceBv))
            {
                CurrentTarget = surfacePos;
                return surfacePos;
            }

            // 2B. Direct penetrating line upgrade along gaze ray into wall
            // (Upgrades directly in-line with your aim - NO 3x3 surrounding expansion)
            for (float depth = 0.8f; depth <= MaxPenetrationDepth; depth += 0.8f)
            {
                Vector3 samplePoint = hitPoint + rayDir * depth;
                Vector3i deepPos = World.worldToBlockPos(samplePoint);

                if (deepPos == surfacePos) continue;

                BlockValue deepBv = world.GetBlock(deepPos);
                if (deepBv.ischild && deepBv.Block?.multiBlockPos != null)
                {
                    deepPos = deepBv.Block.multiBlockPos.GetParentPos(deepPos, deepBv);
                    deepBv = world.GetBlock(deepPos);
                }

                if (World.SandboxUseTraderArea == TraderAreaStates.Default && world.IsWithinTraderArea(deepPos)) continue;
                if ((deepPos.ToVector3() - playerPos).sqrMagnitude > MaxPlayerDistanceSq) continue;

                if (CanUpgradeBlock(world, deepPos, deepBv))
                {
                    CurrentTarget = deepPos;
                    return deepPos;
                }
            }

            CurrentTarget = surfacePos;
            return surfacePos;
        }
    }
}
