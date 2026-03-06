using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionSetupBossGroup : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string minionGroupName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string bossGroupName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string bossIcon1 = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public BossGroup.BossGroupTypes bossGroupType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMinionGroupName = "minion_group_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBossGroupName = "boss_group_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBossIcon1 = "boss_icon1";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGroupType = "group_type";

	public override ActionCompleteStates OnPerformAction()
	{
		List<Entity> entityGroup = base.Owner.GetEntityGroup(bossGroupName);
		List<Entity> entityGroup2 = base.Owner.GetEntityGroup(minionGroupName);
		List<EntityAlive> list = new List<EntityAlive>();
		EntityAlive boss = entityGroup[0] as EntityAlive;
		for (int i = 0; i < entityGroup2.Count; i++)
		{
			EntityAlive entityAlive = entityGroup2[i] as EntityAlive;
			if (entityAlive != null)
			{
				list.Add(entityAlive);
			}
		}
		if (base.Owner.Target is EntityPlayer target)
		{
			base.Owner.CurrentBossGroupID = GameEventManager.Current.SetupBossGroup(target, boss, list, bossGroupType, bossIcon1);
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropMinionGroupName, ref minionGroupName);
		properties.ParseString(PropBossGroupName, ref bossGroupName);
		properties.ParseString(PropBossIcon1, ref bossIcon1);
		properties.ParseEnum(PropGroupType, ref bossGroupType);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionSetupBossGroup
		{
			minionGroupName = minionGroupName,
			bossGroupName = bossGroupName,
			bossGroupType = bossGroupType,
			bossIcon1 = bossIcon1
		};
	}
}
