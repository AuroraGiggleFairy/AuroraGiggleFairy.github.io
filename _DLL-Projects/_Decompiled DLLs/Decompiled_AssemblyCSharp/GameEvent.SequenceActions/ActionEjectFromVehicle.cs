using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionEjectFromVehicle : ActionBaseTargetAction
{
	public override ActionCompleteStates PerformTargetAction(Entity target)
	{
		EntityPlayer entityPlayer = target as EntityPlayer;
		if (entityPlayer != null && entityPlayer.AttachedToEntity != null)
		{
			if (entityPlayer.isEntityRemote)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageCloseAllWindows>().Setup(entityPlayer.entityId), _onlyClientsAttachedToAnEntity: false, entityPlayer.entityId);
			}
			else
			{
				(entityPlayer as EntityPlayerLocal).PlayerUI.windowManager.CloseAllOpenWindows();
			}
			entityPlayer.SendDetach();
		}
		return ActionCompleteStates.Complete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionEjectFromVehicle
		{
			targetGroup = targetGroup
		};
	}
}
