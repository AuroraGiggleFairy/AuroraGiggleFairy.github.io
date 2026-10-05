using System;
using HarmonyLib;
using Platform;
using UnityEngine;

namespace ExpandedInteractionPrompts
{
    internal static class LockableStationsClient
    {
        private delegate bool TryGetPromptLockDel(
            WorldBase world,
            Vector3i blockPos,
            out bool isLocked,
            out PlatformUserIdentifierAbs owner,
            out bool accessDenied);

        private static readonly TryGetPromptLockDel Query = BindQuery();
        private static int s_frame = -1;
        private static Vector3i s_pos;
        private static bool s_ok;
        private static bool s_locked;
        private static bool s_denied;
        private static PlatformUserIdentifierAbs s_owner;

        public static bool IsAvailable => Query != null;

        private static TryGetPromptLockDel BindQuery()
        {
            Type helpers = AccessTools.TypeByName("LockableWorkstations.LockableWorkstationHelpers");
            var method = helpers == null
                ? null
                : AccessTools.Method(
                    helpers,
                    "TryGetPromptLock",
                    new[]
                    {
                        typeof(WorldBase),
                        typeof(Vector3i),
                        typeof(bool).MakeByRefType(),
                        typeof(PlatformUserIdentifierAbs).MakeByRefType(),
                        typeof(bool).MakeByRefType()
                    });
            if (method == null)
                return null;

            return (TryGetPromptLockDel)Delegate.CreateDelegate(typeof(TryGetPromptLockDel), method);
        }

        public static Vector3i ResolveParentIfChild(WorldBase world, Vector3i blockPos)
        {
            if (world == null)
                return blockPos;

            BlockValue block = world.GetBlock(blockPos);
            if (!block.ischild || block.Block?.multiBlockPos == null)
                return blockPos;

            return block.Block.multiBlockPos.GetParentPos(blockPos, block);
        }

        public static bool TryGetLockState(
            WorldBase world,
            Vector3i blockPos,
            out bool isLocked,
            out PlatformUserIdentifierAbs owner,
            out bool accessDenied)
        {
            int frame = Time.frameCount;
            if (frame == s_frame && blockPos.Equals(s_pos))
            {
                isLocked = s_locked;
                owner = s_owner;
                accessDenied = s_denied;
                return s_ok;
            }

            s_frame = frame;
            s_pos = blockPos;
            if (Query == null || world == null)
            {
                s_ok = false;
                s_locked = false;
                s_owner = null;
                s_denied = false;
                isLocked = false;
                owner = null;
                accessDenied = false;
                return false;
            }

            s_ok = Query(world, blockPos, out s_locked, out s_owner, out s_denied);
            isLocked = s_locked;
            owner = s_owner;
            accessDenied = s_denied;
            return s_ok;
        }
    }
}
