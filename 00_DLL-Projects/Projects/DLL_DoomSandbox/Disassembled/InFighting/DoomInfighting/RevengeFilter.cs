using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace DoomInfighting;

[HarmonyPatch(typeof(EAISetAsTargetIfHurt), "CanExecute")]
public static class RevengeFilter
{
	public const float NeverSwitch = -1f;

	public static EntityType Mask(EntityAlive attacker)
	{
		if (attacker == null)
		{
			return EntityType.Unknown;
		}
		if (!Demons.Is(attacker) || Demons.NoRevenge(attacker))
		{
			return attacker.entityType;
		}
		return EntityType.Unknown;
	}

	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> _instructions)
	{
		FieldInfo field = AccessTools.Field(typeof(Entity), "entityType");
		MethodInfo mask = AccessTools.Method(typeof(RevengeFilter), "Mask");
		List<CodeInstruction> code = new List<CodeInstruction>(_instructions);
		int types = 0;
		int first = -1;
		int roll = -1;
		int rolls = 0;
		for (int i = 0; i < code.Count; i++)
		{
			if (code[i].opcode == OpCodes.Ldfld && (FieldInfo)code[i].operand == field)
			{
				types++;
				if (first < 0)
				{
					first = i;
				}
			}
			if (code[i].opcode == OpCodes.Ldc_R4 && (float)code[i].operand == 0.66f)
			{
				roll = i;
				rolls++;
			}
		}
		if (first < 0 || types != 2 || rolls != 1)
		{
			Log.Error("[DoomInfighting] EAISetAsTargetIfHurt.CanExecute: expected 2 entityType loads and 1 roll, found " + types + " and " + rolls + ", not patched");
			return code;
		}
		code[first] = new CodeInstruction(OpCodes.Call, mask)
		{
			labels = code[first].labels,
			blocks = code[first].blocks
		};
		code[roll] = new CodeInstruction(OpCodes.Ldc_R4, -1f)
		{
			labels = code[roll].labels,
			blocks = code[roll].blocks
		};
		return code;
	}
}
