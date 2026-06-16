using Twitch;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionTwitchAddEntitiesToSpawned : ActionBaseTargetAction
{
	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		if (base.Owner.Target is EntityPlayer { TwitchEnabled: false })
		{
			return ActionCompleteStates.Complete;
		}
		if (target is EntityAlive entityAlive)
		{
			if (target is EntityPlayer)
			{
				return ActionCompleteStates.Complete;
			}
			if (!TwitchManager.Current.LiveListContains(entityAlive.entityId))
			{
				entityAlive.SetSpawnByData(base.Owner.Target.entityId, base.Owner.ExtraData);
				GameEventManager.Current.RegisterSpawnedEntity(entityAlive, target, base.Owner.Requester, base.Owner);
				if (base.Owner.Requester != null)
				{
					if (base.Owner.Requester is EntityPlayerLocal)
					{
						GameEventManager.Current.HandleGameEntitySpawned(base.Owner.Name, entityAlive.entityId, base.Owner.Tag);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(base.Owner.Name, base.Owner.Target.entityId, base.Owner.ExtraData, base.Owner.Tag, NetPackageGameEventResponse.ResponseTypes.TwitchSetOwner, entityAlive.entityId), _onlyClientsAttachedToAnEntity: false, base.Owner.Requester.entityId);
					}
				}
			}
		}
		return ActionCompleteStates.Complete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionTwitchAddEntitiesToSpawned
		{
			targetGroup = targetGroup
		};
	}
}
