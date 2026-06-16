using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionUpdateBossGroup : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public BossGroup.BossGroupTypes bossGroupType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGroupType = "group_type";

	public override ActionCompleteStates OnPerformAction()
	{
		GameEventManager.Current.UpdateBossGroupType(base.Owner.CurrentBossGroupID, bossGroupType);
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseEnum(PropGroupType, ref bossGroupType);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionUpdateBossGroup
		{
			bossGroupType = bossGroupType
		};
	}
}
