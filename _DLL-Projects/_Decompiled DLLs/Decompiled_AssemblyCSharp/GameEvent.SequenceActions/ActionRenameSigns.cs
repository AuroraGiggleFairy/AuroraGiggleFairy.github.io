using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionRenameSigns : ActionBaseContainersAction
{
	public override bool CheckValidTileEntity(TileEntity te, out bool isEmpty)
	{
		isEmpty = true;
		if (te.TryGetSelfOrFeature<ITileEntitySignable>(out var _typedTe) && _typedTe.EntityId == -1)
		{
			isEmpty = _typedTe.GetAuthoredText().Text == base.ModifiedName;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool HandleContainerAction(List<TileEntity> tileEntityList)
	{
		bool result = false;
		for (int i = 0; i < tileEntityList.Count; i++)
		{
			if (tileEntityList[i].TryGetSelfOrFeature<ITileEntitySignable>(out var _typedTe) && _typedTe.EntityId == -1)
			{
				_typedTe.SetText(base.ModifiedName, _syncData: true, PlatformManager.MultiPlatform.User.PlatformUserId);
				result = true;
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionRenameSigns
		{
			TargetingType = TargetingType,
			maxDistance = maxDistance,
			newName = newName,
			changeName = changeName,
			tileEntityList = tileEntityList
		};
	}
}
