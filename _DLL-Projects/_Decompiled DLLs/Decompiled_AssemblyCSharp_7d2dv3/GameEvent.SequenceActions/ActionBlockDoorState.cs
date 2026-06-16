using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionBlockDoorState : ActionBaseBlockAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool setOpen = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool setLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool handleLock;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSetOpenState = "set_open";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSetLockState = "set_lock";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BlockChangeInfo UpdateBlock(World world, Vector3i currentPos, BlockValue blockValue)
	{
		if (!blockValue.isair)
		{
			TileEntityComposite te = world.GetTileEntity(currentPos) as TileEntityComposite;
			if (te.TryGetSelfOrFeature<TEFeatureDoor>(out var _typedTe))
			{
				_typedTe.SetOpen(setOpen);
				if (handleLock && te.TryGetSelfOrFeature<TEFeatureLockable>(out var _typedTe2))
				{
					_typedTe2.SetLocked(setLocked);
				}
				_typedTe.HandleOpenCloseSound(currentPos);
				return new BlockChangeInfo(currentPos, blockValue);
			}
		}
		return null;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseBool(PropSetOpenState, ref setOpen);
		if (properties.Contains(PropSetLockState))
		{
			handleLock = true;
			properties.ParseBool(PropSetLockState, ref setLocked);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionBlockDoorState
		{
			setOpen = setOpen,
			setLocked = setLocked,
			handleLock = handleLock
		};
	}
}
