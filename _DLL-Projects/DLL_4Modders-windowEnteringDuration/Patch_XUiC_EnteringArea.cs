using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace AGFProjects.windowEnteringDuration
{
    [HarmonyPatch]
    public class Patch_XUiC_EnteringArea
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Method("XUiC_EnteringArea:Update");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var getConfigInstance = AccessTools.PropertyGetter(typeof(Config), nameof(Config.Instance));
            var enteringAreaDurationField = AccessTools.Field(typeof(Config), nameof(Config.EnteringAreaDuration));
            var showTimeField = AccessTools.Field(typeof(XUiC_EnteringArea), "showTime");

            for (int i = 0; i < codes.Count - 1; i++)
            {
                // Match: ldc.r4 3 followed by stfld showTime
                if (codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 3f &&
                    codes[i + 1].opcode == OpCodes.Stfld && codes[i + 1].operand == showTimeField)
                {
                    codes[i] = new CodeInstruction(OpCodes.Call, getConfigInstance); // call Config.Instance
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldfld, enteringAreaDurationField)); // ldfld EnteringAreaDuration
                    i++; // skip over the inserted instruction
                }
            }
            return codes;
        }
    }
}
