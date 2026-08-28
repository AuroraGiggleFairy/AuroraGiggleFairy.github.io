using UnityEngine;

namespace DoomLevels;

public static class Activator
{
	public const int ActionRaise = 0;

	public const int ActionOpen = 1;

	public const int ActionClose = 2;

	public const int ActionStart = 3;

	public const int ActionStop = 4;

	public const int ActionShut = 5;

	public static bool Activate(WorldBase world, Vector3i pos, MoveData move)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Drive(world, pos, move) == DriveResult.Driven;
	}

	public static DriveResult Drive(WorldBase world, Vector3i pos, MoveData move)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (world == null)
		{
			return DriveResult.Missing;
		}
		Door door = Door.At(world, pos);
		if ((Object)(object)door != (Object)null)
		{
			if (!door.Trigger(move))
			{
				return DriveResult.Refused;
			}
			return DriveResult.Driven;
		}
		Mover mover = Mover.At(world, pos);
		if ((Object)(object)mover != (Object)null)
		{
			if (!mover.Trigger(move.Action))
			{
				return DriveResult.Refused;
			}
			return DriveResult.Driven;
		}
		Crusher crusher = Crusher.At(world, pos);
		if ((Object)(object)crusher != (Object)null)
		{
			if (!crusher.Trigger(move.Action))
			{
				return DriveResult.Refused;
			}
			return DriveResult.Driven;
		}
		return Command(world, pos, move.Action);
	}

	private unsafe static DriveResult Command(WorldBase world, Vector3i pos, int action)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		BlockValue block = world.GetBlock(pos);
		if (((BlockValue)(ref block)).isair)
		{
			return DriveResult.Missing;
		}
		if (((BlockValue)(ref block)).Block is BlockDoor)
		{
			BlockState.Set(pos, 1, action != 2);
			return DriveResult.Driven;
		}
		if (((BlockValue)(ref block)).Block is BlockMover blockMover)
		{
			if (!blockMover.Returns)
			{
				BlockState.Set(pos, 4, on: true);
			}
			return DriveResult.Driven;
		}
		if (((BlockValue)(ref block)).Block is BlockCrusher)
		{
			BlockState.Set(pos, 1, action != 4);
			return DriveResult.Driven;
		}
		string[] obj = new string[5] { "[DoomLevels] activation target at ", null, null, null, null };
		Vector3i val = pos;
		obj[1] = ((object)(*(Vector3i*)(&val))/*cast due to .constrained prefix*/).ToString();
		obj[2] = " is ";
		obj[3] = ((BlockValue)(ref block)).Block.GetBlockName();
		obj[4] = ", which is neither a door nor a mover";
		Log.Warning(string.Concat(obj));
		return DriveResult.Missing;
	}
}
