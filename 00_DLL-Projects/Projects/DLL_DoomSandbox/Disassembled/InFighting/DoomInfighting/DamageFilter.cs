using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace DoomInfighting;

[HarmonyPatch(typeof(EntityAlive), "DamageEntity")]
public static class DamageFilter
{
	public static EntityFlags Mask(EntityAlive attacker, EntityAlive victim)
	{
		if (attacker == null)
		{
			return EntityFlags.None;
		}
		EntityFlags flags = attacker.entityFlags;
		if (attacker == victim)
		{
			return flags;
		}
		if (!Demons.Is(attacker) || !Demons.Is(victim))
		{
			return flags;
		}
		if (Demons.SameSpecies(attacker, victim) && !Demons.NotMissile(attacker))
		{
			return flags;
		}
		return (EntityFlags)((uint)flags & 0xFFFFFFFDu);
	}

	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> _instructions)
	{
		FieldInfo field = AccessTools.Field(typeof(Entity), "entityFlags");
		MethodInfo mask = AccessTools.Method(typeof(DamageFilter), "Mask");
		List<CodeInstruction> code = new List<CodeInstruction>(_instructions);
		int found = -1;
		for (int i = 0; i < code.Count - 1; i++)
		{
			if (code[i].opcode == OpCodes.Ldfld && (FieldInfo)code[i].operand == field && code[i + 1].opcode == OpCodes.And)
			{
				if (found >= 0)
				{
					Log.Error("[DoomInfighting] EntityAlive.DamageEntity: zombie flag test is ambiguous, not patched");
					return code;
				}
				found = i;
			}
		}
		if (found < 0)
		{
			Log.Error("[DoomInfighting] EntityAlive.DamageEntity: zombie flag test not found, not patched");
			return code;
		}
		CodeInstruction self = new CodeInstruction(OpCodes.Ldarg_0);
		self.labels.AddRange(code[found].labels);
		self.blocks.AddRange(code[found].blocks);
		code[found] = new CodeInstruction(OpCodes.Call, mask);
		code.Insert(found, self);
		return code;
	}
}
