using HarmonyLib;
using System.Reflection;

namespace ExpandedInteractionPrompts
{
    [HarmonyPatch(typeof(PlayerMoveController))]
    [HarmonyPatch("Update")]
    public static class Patch_PlayerMoveController_Update
    {
        // Transpiler to replace string.Format calls for activation prompts with a bulletproof formatter
        static System.Collections.Generic.IEnumerable<HarmonyLib.CodeInstruction> Transpiler(System.Collections.Generic.IEnumerable<HarmonyLib.CodeInstruction> instructions)
        {
            var codes = new System.Collections.Generic.List<HarmonyLib.CodeInstruction>(instructions);
            // Support both string.Format(string, object) and string.Format(string, object[])
            var formatMethod1 = typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object) });
            var formatMethod3 = typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object), typeof(object) });
            var formatMethod2 = typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object[]) });
            var safeFormatMethod = typeof(SafeFormatter).GetMethod("BulletproofFormat", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var focusAwareFormatMethod = typeof(SafeFormatter).GetMethod("FocusAwareFormat", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (safeFormatMethod == null || focusAwareFormatMethod == null)
            {
                // If BulletproofFormat is missing, do not patch
                return codes;
            }
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == System.Reflection.Emit.OpCodes.Call)
                {
                    var operand = codes[i].operand as MethodInfo;
                    if (operand == formatMethod1)
                    {
                        // Replace with call to BulletproofFormat(string, string, string)
                        codes.Insert(i, new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Ldstr, ""));
                        codes[i + 1] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call, safeFormatMethod);
                        i++;
                    }
                    else if (operand == formatMethod3)
                    {
                        // Replace with a formatter that can append vehicle slot info for owner/ally viewers.
                        codes[i] = new HarmonyLib.CodeInstruction(System.Reflection.Emit.OpCodes.Call, focusAwareFormatMethod);
                    }
                    else if (operand == formatMethod2)
                    {
                        // Optionally handle string.Format(string, object[]), but skip for now (or add logic if needed)
                        // Could log or handle as needed
                    }
                }
            }
            return codes;
        }
    }
}
