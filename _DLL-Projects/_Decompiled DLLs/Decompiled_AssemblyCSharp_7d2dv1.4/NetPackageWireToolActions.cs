using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWireToolActions : NetPackage
{
	public enum WireActions
	{
		AddWire,
		RemoveWire
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i tileEntityPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public WireActions currentOperation;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityID;

	public NetPackageWireToolActions Setup(WireActions _operation, Vector3i _tileEntityPosition, int _entityID)
	{
		currentOperation = _operation;
		tileEntityPosition = _tileEntityPosition;
		entityID = _entityID;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		currentOperation = (WireActions)_br.ReadByte();
		tileEntityPosition = StreamUtils.ReadVector3i(_br);
		entityID = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)currentOperation);
		StreamUtils.Write(_bw, tileEntityPosition);
		_bw.Write(entityID);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || !ValidEntityIdForSender(entityID))
		{
			return;
		}
		switch (currentOperation)
		{
		case WireActions.AddWire:
		{
			if (!(_world.GetChunkFromWorldPos(tileEntityPosition.x, tileEntityPosition.y, tileEntityPosition.z) is Chunk chunk))
			{
				break;
			}
			TileEntityPowered tileEntityPowered = _world.GetTileEntity(chunk.ClrIdx, tileEntityPosition) as TileEntityPowered;
			EntityPlayer entityPlayer2 = _world.GetEntity(entityID) as EntityPlayer;
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
			if (tileEntityPowered != null && entityPlayer2 != null)
			{
				Transform transform = entityPlayer2.RootTransform.FindInChilds(entityPlayer2.GetRightHandTransformName());
				if (transform != null)
				{
					ItemActionConnectPower.ConnectPowerData obj = (ItemActionConnectPower.ConnectPowerData)entityPlayer2.inventory.holdingItemData.actionData[1];
					WireNode component = ((GameObject)Object.Instantiate(Resources.Load("Prefabs/WireNode"))).GetComponent<WireNode>();
					component.LocalPosition = tileEntityPowered.ToWorldPos().ToVector3() - Origin.position;
					component.localOffset = tileEntityPowered.GetWireOffset();
					component.localOffset.x += 0.5f;
					component.localOffset.y += 0.5f;
					component.localOffset.z += 0.5f;
					component.Source = transform.gameObject;
					component.TogglePulse(isOn: false);
					component.SetPulseSpeed(360f);
					obj.wireNode = component;
				}
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(currentOperation, tileEntityPosition, entityID), _onlyClientsAttachedToAnEntity: false, -1, entityID);
			}
			break;
		}
		case WireActions.RemoveWire:
		{
			EntityPlayer entityPlayer = _world.GetEntity(entityID) as EntityPlayer;
			if (entityPlayer != null && entityPlayer.RootTransform.FindInChilds(entityPlayer.GetRightHandTransformName()) != null && entityPlayer.inventory.holdingItemData.actionData[1] is ItemActionConnectPower.ConnectPowerData connectPowerData && connectPowerData.wireNode != null)
			{
				Object.Destroy(connectPowerData.wireNode.gameObject);
				connectPowerData.wireNode = null;
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(currentOperation, tileEntityPosition, entityID), _onlyClientsAttachedToAnEntity: false, -1, entityID);
			}
			break;
		}
		}
	}

	public override int GetLength()
	{
		return 12;
	}
}
