using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionAddRandomBuff : ActionBaseTargetAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string addsBuff = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] buffNames;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string removesBuff = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] removesBuffs;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBuffName = "buff_names";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRemovesBuff = "removes_buff";

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		if (removesBuffs == null)
		{
			removesBuffs = removesBuff.Split(',');
		}
		if (buffNames == null)
		{
			buffNames = addsBuff.Split(',');
		}
		EntityAlive entityAlive = target as EntityAlive;
		if (entityAlive != null)
		{
			bool flag = false;
			for (int i = 0; i < removesBuffs.Length; i++)
			{
				if (entityAlive.Buffs.HasBuff(removesBuffs[i]))
				{
					entityAlive.Buffs.RemoveBuff(removesBuffs[i]);
					flag = true;
				}
			}
			if (!flag)
			{
				string name = buffNames[target.rand.RandomRange(buffNames.Length)];
				entityAlive.Buffs.AddBuff(name);
			}
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropBuffName, ref addsBuff);
		properties.ParseString(PropRemovesBuff, ref removesBuff);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionAddRandomBuff
		{
			addsBuff = addsBuff,
			removesBuff = removesBuff,
			targetGroup = targetGroup
		};
	}
}
