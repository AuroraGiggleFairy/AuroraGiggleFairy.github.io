using System;
using HarmonyLib;
using UnityEngine;

namespace DamageTypeFix
{
    public class ModAPI : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            Harmony harmony = new Harmony("com.agfprojects.damagetypefix");
            PatchOne(harmony, typeof(Patch_NetPackageExplosionInitiate_read));
            PatchOne(harmony, typeof(Patch_Explosion_AttackEntites));
            PatchOne(harmony, typeof(Patch_EntityAlive_DamageEntity_Bfg));
        }

        static void PatchOne(Harmony harmony, Type type)
        {
            try
            {
                harmony.CreateClassProcessor(type).Patch();
                Debug.Log("[DamageTypeFix] Patched " + type.Name);
            }
            catch (Exception ex)
            {
                Debug.LogError("[DamageTypeFix] Failed " + type.Name + ": " + ex);
            }
        }
    }
}
