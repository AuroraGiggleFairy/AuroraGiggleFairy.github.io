using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionCallGameEvent : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<string> gameEventNames = new List<string>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public string targetGroup = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGameEventNames = "game_events";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTargetGroup = "target_group";

	public override ActionCompleteStates OnPerformAction()
	{
		GameEventActionSequence ownerSeq = ((base.Owner.OwnerSequence != null) ? base.Owner.OwnerSequence : base.Owner);
		if (targetGroup != "")
		{
			List<Entity> entityGroup = base.Owner.GetEntityGroup(targetGroup);
			if (entityGroup != null)
			{
				for (int i = 0; i < entityGroup.Count; i++)
				{
					GameEventManager.Current.HandleAction(gameEventNames[GameEventManager.Current.Random.RandomRange(0, gameEventNames.Count)], base.Owner.Requester, entityGroup[i], base.Owner.TwitchActivated, base.Owner.ExtraData, "", crateShare: false, allowRefunds: true, "", ownerSeq);
				}
			}
		}
		else
		{
			GameEventManager.Current.HandleAction(gameEventNames[GameEventManager.Current.Random.RandomRange(0, gameEventNames.Count)], base.Owner.Requester, base.Owner.Target, base.Owner.TwitchActivated, base.Owner.ExtraData, "", crateShare: false, allowRefunds: true, "", ownerSeq);
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropGameEventNames))
		{
			gameEventNames.AddRange(properties.Values[PropGameEventNames].Split(','));
		}
		properties.ParseString(PropTargetGroup, ref targetGroup);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionCallGameEvent
		{
			gameEventNames = gameEventNames,
			targetGroup = targetGroup
		};
	}
}
