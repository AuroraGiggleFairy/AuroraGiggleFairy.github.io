using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace LootTimerHolds
{
    [HarmonyPatch(typeof(TEFeatureStorage), nameof(TEFeatureStorage.UpdateTick))]
    public static class Patch_TEFeatureStorage_UpdateTick
    {
        private static readonly Action<TEFeatureStorage, World> CallBaseUpdateTick = CreateBaseCaller();

        private static MethodInfo FindBaseUpdateTick()
        {
            Type t = typeof(TEFeatureStorage).BaseType;
            while (t != null && t != typeof(object) && t != typeof(TEFeatureStorage))
            {
                MethodInfo declared = AccessTools.DeclaredMethod(t, "UpdateTick", new[] { typeof(World) });
                if (declared != null)
                    return declared;
                t = t.BaseType;
            }

            return null;
        }

        private static Action<TEFeatureStorage, World> CreateBaseCaller()
        {
            MethodInfo method = FindBaseUpdateTick();
            if (method == null)
                return null;

            var dyn = new DynamicMethod(
                "AgfCallBaseUpdateTick",
                typeof(void),
                new[] { typeof(TEFeatureStorage), typeof(World) },
                typeof(TEFeatureStorage),
                skipVisibility: true);
            ILGenerator il = dyn.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, method);
            il.Emit(OpCodes.Ret);
            return (Action<TEFeatureStorage, World>)dyn.CreateDelegate(typeof(Action<TEFeatureStorage, World>));
        }

        public static bool Prefix(TEFeatureStorage __instance, World _world)
        {
            if (__instance == null || _world == null)
                return false;

            CallBaseUpdateTick?.Invoke(__instance, _world);
            GameVersion.Initialize();
            if (GameVersion.UseV33)
                return HoldV33.Run(__instance, _world);
            return HoldV32.Run(__instance, _world);
        }

        private static class HoldV33
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static bool Run(TEFeatureStorage storage, World world)
            {
                ItemStackGrid grid = storage.ItemGrid;
                if (storage.Parent != null && storage.Parent.PlayerPlaced)
                    return false;
                if (grid == null || grid.PlayerOwned || !grid.Touched || !storage.IsEmpty())
                    return false;

                int respawnDays = GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays);
                if (respawnDays <= 0)
                    return false;

                int touchedHours = GameUtils.WorldTimeToTotalHours(grid.WorldTimeTouched);
                if ((GameUtils.WorldTimeToTotalHours(world.worldTime) - touchedHours) / 24 < respawnDays)
                    return false;

                grid.Reset();
                storage.SetModified();
                return false;
            }
        }

        private static class HoldV32
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static bool Run(TEFeatureStorage storage, World world)
            {
                if (storage.Parent != null && storage.Parent.PlayerPlaced)
                    return false;
                if (Flag(storage, "bPlayerStorage") || !Flag(storage, "bTouched") || !storage.IsEmpty())
                    return false;

                int respawnDays = GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays);
                if (respawnDays <= 0)
                    return false;

                ulong touched = Member(storage, "worldTimeTouched") is ulong value ? value : 0UL;
                int touchedHours = GameUtils.WorldTimeToTotalHours(touched);
                if ((GameUtils.WorldTimeToTotalHours(world.worldTime) - touchedHours) / 24 < respawnDays)
                    return false;

                SetFlag(storage, "bWasTouched", false);
                SetFlag(storage, "bTouched", false);
                storage.SetModified();
                return false;
            }

            private static bool Flag(object target, string name)
            {
                return Member(target, name) is bool value && value;
            }

            private static object Member(object target, string name)
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                PropertyInfo property = target.GetType().GetProperty(name, flags);
                if (property != null)
                    return property.GetValue(target);
                return target.GetType().GetField(name, flags)?.GetValue(target);
            }

            private static void SetFlag(object target, string name, bool value)
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                PropertyInfo property = target.GetType().GetProperty(name, flags);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(target, value);
                    return;
                }

                target.GetType().GetField(name, flags)?.SetValue(target, value);
            }
        }
    }
}
