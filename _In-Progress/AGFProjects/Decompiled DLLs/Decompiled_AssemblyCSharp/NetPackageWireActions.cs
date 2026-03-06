using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWireActions : NetPackage
{
	public enum WireActions
	{
		SetParent,
		RemoveParent,
		SendWires
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i tileEntityPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public int wiringEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public WireActions currentOperation;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3i> wireChildren = new List<Vector3i>();

	public NetPackageWireActions Setup(WireActions _operation, Vector3i _tileEntityPosition, List<Vector3i> _wireChildren, int wiringEntity = -1)
	{
		currentOperation = _operation;
		tileEntityPosition = _tileEntityPosition;
		wireChildren = _wireChildren;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		currentOperation = (WireActions)_br.ReadByte();
		tileEntityPosition = StreamUtils.ReadVector3i(_br);
		int num = _br.ReadByte();
		wireChildren.Clear();
		for (int i = 0; i < num; i++)
		{
			wireChildren.Add(StreamUtils.ReadVector3i(_br));
		}
		if (currentOperation != WireActions.SendWires)
		{
			wiringEntityID = _br.ReadInt32();
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)currentOperation);
		StreamUtils.Write(_bw, tileEntityPosition);
		_bw.Write((byte)wireChildren.Count);
		for (int i = 0; i < wireChildren.Count; i++)
		{
			StreamUtils.Write(_bw, wireChildren[i]);
		}
		if (currentOperation != WireActions.SendWires)
		{
			_bw.Write(wiringEntityID);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			switch (currentOperation)
			{
			case WireActions.SetParent:
			{
				TileEntityPowered poweredTileEntity = GetPoweredTileEntity(_world, tileEntityPosition);
				PowerItem powerItem2 = null;
				ushort blockID = 0;
				powerItem2 = PowerManager.Instance.GetPowerItemByWorldPos(poweredTileEntity.ToWorldPos());
				if (powerItem2 == null)
				{
					powerItem2 = poweredTileEntity.CreatePowerItemForTileEntity(blockID);
					poweredTileEntity.SetModified();
					powerItem2.AddTileEntity(poweredTileEntity);
				}
				TileEntityPowered poweredTileEntity2 = GetPoweredTileEntity(_world, wireChildren[0]);
				PowerItem powerItem3 = PowerManager.Instance.GetPowerItemByWorldPos(poweredTileEntity2.ToWorldPos());
				if (powerItem3 == null)
				{
					powerItem3 = poweredTileEntity2.CreatePowerItemForTileEntity(blockID);
					poweredTileEntity2.SetModified();
					powerItem3.AddTileEntity(poweredTileEntity2);
				}
				PowerItem parent2 = powerItem2.Parent;
				PowerManager.Instance.SetParent(powerItem2, powerItem3);
				if (parent2 != null && parent2.TileEntity != null)
				{
					parent2.TileEntity.CreateWireDataFromPowerItem();
					parent2.TileEntity.SendWireData();
					parent2.TileEntity.RemoveWires();
					parent2.TileEntity.DrawWires();
				}
				if (powerItem3.TileEntity != null)
				{
					powerItem3.TileEntity.CreateWireDataFromPowerItem();
					powerItem3.TileEntity.SendWireData();
					powerItem3.TileEntity.RemoveWires();
					powerItem3.TileEntity.DrawWires();
				}
				break;
			}
			case WireActions.RemoveParent:
			{
				PowerItem powerItem = GetPoweredTileEntity(_world, tileEntityPosition).GetPowerItem();
				if (powerItem.Parent != null)
				{
					PowerItem parent = powerItem.Parent;
					powerItem.RemoveSelfFromParent();
					if (parent.TileEntity != null)
					{
						parent.TileEntity.CreateWireDataFromPowerItem();
						parent.TileEntity.SendWireData();
						parent.TileEntity.RemoveWires();
						parent.TileEntity.DrawWires();
					}
				}
				break;
			}
			case WireActions.SendWires:
				break;
			}
		}
		else if (_world.GetChunkFromWorldPos(tileEntityPosition.x, tileEntityPosition.y, tileEntityPosition.z) is Chunk chunk)
		{
			IPowered powered = _world.GetTileEntity(chunk.ClrIdx, tileEntityPosition) as IPowered;
			if (currentOperation == WireActions.SendWires)
			{
				powered?.SetWireData(wireChildren);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPowered GetPoweredTileEntity(World _world, Vector3i tileEntityPosition)
	{
		Chunk chunk = _world.GetChunkFromWorldPos(tileEntityPosition.x, tileEntityPosition.y, tileEntityPosition.z) as Chunk;
		TileEntityPowered tileEntityPowered = _world.GetTileEntity(chunk.ClrIdx, tileEntityPosition) as TileEntityPowered;
		if (tileEntityPowered == null)
		{
			Block block = _world.GetBlock(tileEntityPosition).Block;
			if (block is BlockPowered)
			{
				tileEntityPowered = (block as BlockPowered).CreateTileEntity(chunk);
			}
			tileEntityPowered.localChunkPos = World.toBlock(tileEntityPosition);
			BlockEntityData blockEntity = chunk.GetBlockEntity(tileEntityPosition);
			if (blockEntity != null)
			{
				tileEntityPowered.BlockTransform = blockEntity.transform;
			}
			tileEntityPowered.InitializePowerData();
			chunk.AddTileEntity(tileEntityPowered);
		}
		return tileEntityPowered;
	}

	public override int GetLength()
	{
		return 12;
	}
}
