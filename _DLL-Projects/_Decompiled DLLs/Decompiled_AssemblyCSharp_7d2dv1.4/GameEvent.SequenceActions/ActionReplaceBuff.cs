using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionReplaceBuff : ActionBaseTargetAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string replaceBuff = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string replaceWithBuff = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropReplaceBuffName = "replace_buff";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropReplaceWithBuffName = "replace_with_buff";

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		EntityAlive entityAlive = target as EntityAlive;
		if (entityAlive != null && entityAlive.Buffs.HasBuff(replaceBuff))
		{
			entityAlive.Buffs.RemoveBuff(replaceBuff);
			entityAlive.Buffs.AddBuff(replaceWithBuff);
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropReplaceBuffName, ref replaceBuff);
		properties.ParseString(PropReplaceWithBuffName, ref replaceWithBuff);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionReplaceBuff
		{
			replaceBuff = replaceBuff,
			replaceWithBuff = replaceWithBuff,
			targetGroup = targetGroup
		};
	}
}
