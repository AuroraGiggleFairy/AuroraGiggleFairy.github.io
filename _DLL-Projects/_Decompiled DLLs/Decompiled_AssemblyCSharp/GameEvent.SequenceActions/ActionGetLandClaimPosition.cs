using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionGetLandClaimPosition : BaseAction
{
	public override ActionCompleteStates OnPerformAction()
	{
		World world = GameManager.Instance.World;
		List<Vector3i> lPBlocks = world.GetGameManager().GetPersistentPlayerList().GetPlayerDataFromEntityID(base.Owner.Target.entityId)
			.LPBlocks;
		if (lPBlocks == null || lPBlocks.Count == 0)
		{
			return ActionCompleteStates.InCompleteRefund;
		}
		int num = (GameStats.GetInt(EnumGameStats.LandClaimSize) - 1) / 2;
		int num2 = num * num;
		bool flag = false;
		for (int i = 0; i < lPBlocks.Count; i++)
		{
			Vector3i vector3i = lPBlocks[i];
			if (BlockLandClaim.IsPrimary(world.GetBlock(vector3i)) && base.Owner.Target.GetDistanceSq(vector3i) < (float)num2)
			{
				flag = true;
				base.Owner.TargetPosition = vector3i;
			}
		}
		if (!flag)
		{
			return ActionCompleteStates.InCompleteRefund;
		}
		return ActionCompleteStates.Complete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionGetLandClaimPosition();
	}
}
