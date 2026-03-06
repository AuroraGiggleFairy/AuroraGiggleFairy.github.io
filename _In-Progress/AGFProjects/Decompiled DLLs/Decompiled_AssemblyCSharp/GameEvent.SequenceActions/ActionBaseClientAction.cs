using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionBaseClientAction : ActionBaseTargetAction
{
	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		EntityPlayer entityPlayer = target as EntityPlayer;
		if (entityPlayer != null)
		{
			OnServerPerform(entityPlayer);
			if (entityPlayer is EntityPlayerLocal)
			{
				OnClientPerform(entityPlayer);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(base.Owner.Name, entityPlayer.entityId, base.Owner.ExtraData, base.Owner.Tag, NetPackageGameEventResponse.ResponseTypes.ClientSequenceAction, -1, -1, _isDespawn: false, GetActionKey()), _onlyClientsAttachedToAnEntity: false, entityPlayer.entityId);
			}
		}
		return ActionCompleteStates.Complete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnServerPerform(Entity target)
	{
	}
}
