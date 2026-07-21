using System;
using HarmonyLib;

namespace SkillCap
{
    public class ModAPI : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            try
            {
                var harmony = new Harmony("agf.skillcap");
                harmony.PatchAll();
                ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
                string mode = manager == null
                    ? "conn=null"
                    : "server=" + manager.IsServer + ", client=" + manager.IsClient + ", singlePlayer=" + manager.IsSinglePlayer;
                string capInfo = SkillCapSettings.HasXmlSkillPointCap
                    ? "xml_cap=" + SkillCapSettings.SkillPointCapLevel
                    : "xml_cap=disabled (missing skill_point_level_cap)";
                Console.WriteLine("[SkillCap] Harmony patches applied. " + capInfo + ", dedicated=" + GameManager.IsDedicatedServer + ", " + mode);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SkillCap] Failed to initialize: " + ex);
            }
        }
    }
}
