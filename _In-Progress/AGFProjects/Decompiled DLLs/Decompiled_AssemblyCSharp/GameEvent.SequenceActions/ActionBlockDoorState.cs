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
		if (!blockValue.isair && blockValue.Block is BlockDoor blockDoor)
		{
			blockValue.meta = (byte)((setOpen ? 1 : 0) | (blockValue.meta & -2));
			if (handleLock)
			{
				((TileEntitySecureDoor)world.GetTileEntity(0, currentPos))?.SetLocked(setLocked);
			}
			blockDoor.HandleOpenCloseSound(setOpen, currentPos);
			return new BlockChangeInfo(0, currentPos, blockValue);
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
