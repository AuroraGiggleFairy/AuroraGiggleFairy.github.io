using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionRemoveBuff : ActionBaseTargetAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string buffName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] BuffList;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBuffName = "buff_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
		BuffList = buffName.Split(',');
	}

	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		EntityAlive entityAlive = target as EntityAlive;
		if (entityAlive != null)
		{
			for (int i = 0; i < BuffList.Length; i++)
			{
				if (entityAlive.Buffs.HasBuff(BuffList[i]))
				{
					entityAlive.Buffs.RemoveBuff(BuffList[i]);
				}
			}
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropBuffName))
		{
			buffName = properties.Values[PropBuffName];
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionRemoveBuff
		{
			buffName = buffName,
			BuffList = BuffList,
			targetGroup = targetGroup
		};
	}
}
