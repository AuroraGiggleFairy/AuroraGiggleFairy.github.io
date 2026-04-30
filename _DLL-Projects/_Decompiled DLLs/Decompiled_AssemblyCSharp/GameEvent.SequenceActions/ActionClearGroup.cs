using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionClearGroup : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string groupName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGroupName = "group_name";

	public override ActionCompleteStates OnPerformAction()
	{
		base.Owner.ClearEntityGroup(groupName);
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropGroupName, ref groupName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionClearGroup
		{
			groupName = groupName
		};
	}
}
