using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionBlockDoorState : ActionBaseBlockAction
{
	public enum OpenDoorStates
	{
		Open,
		Close,
		Toggle
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public OpenDoorStates setOpen;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool setLocked;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool traderOnly;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool animate = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool handleLock;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSetOpenState = "set_open";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSetLockState = "set_lock";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAnimate = "animate";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTraderOnly = "trader_only";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool AllowInTrader()
	{
		return traderOnly;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BlockChangeInfo UpdateBlock(World world, Vector3i currentPos, BlockValue blockValue)
	{
		if (!blockValue.isair)
		{
			if (traderOnly && !world.IsWithinTraderArea(currentPos))
			{
				return null;
			}
			TileEntityComposite te = world.GetTileEntity(currentPos) as TileEntityComposite;
			if (te.TryGetSelfOrFeature<TEFeatureDoor>(out var _typedTe))
			{
				bool open = setOpen switch
				{
					OpenDoorStates.Close => false, 
					OpenDoorStates.Toggle => !_typedTe.IsOpen(), 
					_ => true, 
				};
				_typedTe.SetOpen(open, animate);
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
		properties.ParseEnum(PropSetOpenState, ref setOpen);
		properties.ParseBool(PropTraderOnly, ref traderOnly);
		properties.ParseBool(PropAnimate, ref animate);
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
			handleLock = handleLock,
			traderOnly = traderOnly
		};
	}
}
