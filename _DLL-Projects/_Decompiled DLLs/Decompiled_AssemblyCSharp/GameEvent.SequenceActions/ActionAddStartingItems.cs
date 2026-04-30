using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionAddStartingItems : ActionBaseClientAction
{
	public override void OnClientPerform(Entity target)
	{
		if (target is EntityPlayerLocal entityPlayerLocal)
		{
			entityPlayerLocal.SetupStartingItems();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionAddStartingItems
		{
			targetGroup = targetGroup
		};
	}
}
