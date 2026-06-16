using UnityEngine.Scripting;

[Preserve]
public class MinEventActionRemoveAllNegativeBuffs : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			for (int j = 0; j < targets[i].Buffs.ActiveBuffs.Count; j++)
			{
				if (targets[i].Buffs.ActiveBuffs[j].BuffClass.DamageType != EnumDamageTypes.None)
				{
					targets[i].Buffs.ActiveBuffs[j].Remove = true;
				}
			}
		}
	}
}
