using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class TileEntity : ITileEntity
{
	public enum StreamModeRead
	{
		Persistency,
		FromServer,
		FromClient
	}

	public enum StreamModeWrite
	{
		Persistency,
		ToServer,
		ToClient
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public const int CurrentSaveVersion = 16;

	public int entityId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i chunkPos;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int readVersion;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Chunk chunk;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong heapMapLastTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong heapMapUpdateTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bDisableModifiedCheck;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bUserAccessing;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ITileEntityChangedListener> _listeners = new List<ITileEntityChangedListener>();

	[PublicizedFrom(EAccessModifier.Private)]
	public byte handleCounter;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte lockHandleWaitingFor = byte.MaxValue;

	public int EntityId => entityId;

	public Vector3i localChunkPos
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return chunkPos;
		}
		set
		{
			chunkPos = value;
			OnSetLocalChunkPosition();
		}
	}

	public BlockValue blockValue => chunk.GetBlock(localChunkPos);

	public Block block
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Block.list[chunk.GetBlockId(localChunkPos.x, localChunkPos.y, localChunkPos.z)];
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsRemoving { get; set; }

	public List<ITileEntityChangedListener> listeners => _listeners;

	public bool bWaitingForServerResponse => lockHandleWaitingFor != byte.MaxValue;

	public event XUiEvent_TileEntityDestroyed Destroyed;

	public TileEntity(Chunk _chunk)
	{
		chunk = _chunk;
	}

	public virtual TileEntity Clone()
	{
		throw new NotImplementedException("Clone() not implemented yet");
	}

	public virtual void CopyFrom(TileEntity _other)
	{
		throw new NotImplementedException("CopyFrom() not implemented yet");
	}

	public virtual void UpdateTick(World world)
	{
	}

	public abstract TileEntityType GetTileEntityType();

	public virtual void OnSetLocalChunkPosition()
	{
	}

	public Vector3i ToWorldPos()
	{
		if (chunk != null)
		{
			return new Vector3i(chunk.X * 16, chunk.Y * 256, chunk.Z * 16) + localChunkPos;
		}
		return Vector3i.zero;
	}

	public Vector3 ToWorldCenterPos()
	{
		if (entityId != -1)
		{
			Entity entity = GameManager.Instance.World.GetEntity(entityId);
			if ((bool)entity)
			{
				return entity.position;
			}
		}
		if (chunk != null)
		{
			BlockValue blockNoDamage = chunk.GetBlockNoDamage(chunkPos.x, chunkPos.y, chunkPos.z);
			Block block = blockNoDamage.Block;
			Vector3 result = default(Vector3);
			result.x = chunk.X * 16 + chunkPos.x;
			result.y = chunk.Y * 256 + chunkPos.y;
			result.z = chunk.Z * 16 + chunkPos.z;
			if (!block.isMultiBlock)
			{
				result.x += 0.5f;
				result.y += 0.5f;
				result.z += 0.5f;
			}
			else if (block.shape is BlockShapeModelEntity blockShapeModelEntity)
			{
				Quaternion rotation = blockShapeModelEntity.GetRotation(blockNoDamage);
				result += blockShapeModelEntity.GetRotatedOffset(block, rotation);
				result.x += 0.5f;
				result.z += 0.5f;
			}
			return result;
		}
		return Vector3.zero;
	}

	public int GetClrIdx()
	{
		if (chunk == null)
		{
			return 0;
		}
		return chunk.ClrIdx;
	}

	public Chunk GetChunk()
	{
		return chunk;
	}

	public void SetChunk(Chunk _chunk)
	{
		chunk = _chunk;
	}

	public static TileEntity Instantiate(TileEntityType type, Chunk _chunk)
	{
		switch (type)
		{
		case TileEntityType.Collector:
			return new TileEntityCollector(_chunk);
		case TileEntityType.Loot:
			return new TileEntityLootContainer(_chunk);
		case TileEntityType.Forge:
			return new TileEntityForge(_chunk);
		case TileEntityType.SecureLoot:
			return new TileEntitySecureLootContainer(_chunk);
		case TileEntityType.SecureDoor:
			return new TileEntitySecureDoor(_chunk);
		case TileEntityType.Workstation:
			return new TileEntityWorkstation(_chunk);
		case TileEntityType.Trader:
			return new TileEntityTrader(_chunk);
		case TileEntityType.VendingMachine:
			return new TileEntityVendingMachine(_chunk);
		case TileEntityType.Sign:
			return new TileEntitySign(_chunk);
		case TileEntityType.GoreBlock:
			return new TileEntityGoreBlock(_chunk);
		case TileEntityType.Powered:
			return new TileEntityPoweredBlock(_chunk);
		case TileEntityType.PowerSource:
			return new TileEntityPowerSource(_chunk);
		case TileEntityType.PowerRangeTrap:
			return new TileEntityPoweredRangedTrap(_chunk);
		case TileEntityType.PowerMeleeTrap:
			return new TileEntityPoweredMeleeTrap(_chunk);
		case TileEntityType.Light:
			return new TileEntityLight(_chunk);
		case TileEntityType.Trigger:
			return new TileEntityPoweredTrigger(_chunk);
		case TileEntityType.Sleeper:
			return new TileEntitySleeper(_chunk);
		case TileEntityType.LandClaim:
			return new TileEntityLandClaim(_chunk);
		case TileEntityType.SecureLootSigned:
			return new TileEntitySecureLootContainerSigned(_chunk);
		case TileEntityType.Composite:
			return new TileEntityComposite(_chunk);
		default:
			Log.Warning("Dropping TE with unknown type: " + type.ToStringCached());
			return null;
		}
	}

	public virtual void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		if (_eStreamMode == StreamModeRead.Persistency)
		{
			readVersion = _br.ReadUInt16();
			chunkPos = StreamUtils.ReadVector3i(_br);
			entityId = _br.ReadInt32();
			if (readVersion > 1)
			{
				heapMapUpdateTime = _br.ReadUInt64();
				heapMapLastTime = heapMapUpdateTime - AIDirector.GetActivityWorldTimeDelay();
			}
		}
		else
		{
			chunkPos = StreamUtils.ReadVector3i(_br);
			entityId = _br.ReadInt32();
		}
	}

	public virtual void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		if (_eStreamMode == StreamModeWrite.Persistency)
		{
			_bw.Write((ushort)16);
			StreamUtils.Write(_bw, chunkPos);
			_bw.Write(entityId);
			_bw.Write(heapMapUpdateTime);
		}
		else
		{
			StreamUtils.Write(_bw, chunkPos);
			_bw.Write(entityId);
		}
	}

	public override string ToString()
	{
		return string.Format("[TE] " + GetTileEntityType().ToStringCached() + "/" + ToWorldPos().ToString() + "/" + entityId);
	}

	public virtual void OnRemove(World world)
	{
		OnDestroy();
	}

	public virtual void OnUnload(World world)
	{
	}

	public virtual void OnReadComplete()
	{
	}

	public void SetDisableModifiedCheck(bool _b)
	{
		bDisableModifiedCheck = _b;
	}

	public void SetModified()
	{
		setModified();
	}

	public void SetChunkModified()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && chunk != null)
		{
			chunk.isModified = true;
		}
	}

	public virtual bool IsActive(World world)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool IsByWater(World _world, Vector3i _blockPos)
	{
		return _world.IsWater(_blockPos.x, _blockPos.y + 1, _blockPos.z) | _world.IsWater(_blockPos.x + 1, _blockPos.y, _blockPos.z) | _world.IsWater(_blockPos.x - 1, _blockPos.y, _blockPos.z) | _world.IsWater(_blockPos.x, _blockPos.y, _blockPos.z + 1) | _world.IsWater(_blockPos.x, _blockPos.y, _blockPos.z - 1);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void emitHeatMapEvent(World world, EnumAIDirectorChunkEvent eventType)
	{
		if (world.worldTime < heapMapLastTime)
		{
			heapMapUpdateTime = 0uL;
		}
		if (world.worldTime >= heapMapUpdateTime && world.aiDirector != null)
		{
			Vector3i vector3i = ToWorldPos();
			Block block = world.GetBlock(vector3i).Block;
			if (block != null)
			{
				world.aiDirector.NotifyActivity(eventType, vector3i, block.HeatMapStrength);
				heapMapLastTime = world.worldTime;
				heapMapUpdateTime = world.worldTime + AIDirector.GetActivityWorldTimeDelay();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void setModified()
	{
		if (bDisableModifiedCheck)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SetChunkModified();
			Vector3? entitiesInRangeOfWorldPos = ToWorldCenterPos();
			if (entitiesInRangeOfWorldPos.Value == Vector3.zero)
			{
				entitiesInRangeOfWorldPos = null;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageTileEntity>().Setup(this, StreamModeWrite.ToClient, byte.MaxValue), _onlyClientsAttachedToAnEntity: true, -1, -1, -1, entitiesInRangeOfWorldPos);
		}
		else
		{
			if (++handleCounter == byte.MaxValue)
			{
				handleCounter = 0;
			}
			lockHandleWaitingFor = handleCounter;
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageTileEntity>().Setup(this, StreamModeWrite.ToServer, lockHandleWaitingFor));
		}
		NotifyListeners();
	}

	public override int GetHashCode()
	{
		if (entityId != -1)
		{
			return entityId | 0x8000000;
		}
		return ToWorldPos().GetHashCode() & 0x7FFFFFFF;
	}

	public override bool Equals(object obj)
	{
		if (base.Equals(obj))
		{
			return obj.GetHashCode() == GetHashCode();
		}
		return false;
	}

	public void NotifyListeners()
	{
		for (int i = 0; i < listeners.Count; i++)
		{
			listeners[i].OnTileEntityChanged(this);
		}
	}

	public virtual void UpgradeDowngradeFrom(TileEntity _other)
	{
		_other.OnDestroy();
	}

	public virtual void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
	}

	public virtual void SetUserAccessing(bool _bUserAccessing)
	{
		bUserAccessing = _bUserAccessing;
	}

	public bool IsUserAccessing()
	{
		return bUserAccessing;
	}

	public virtual void SetHandle(byte _handle)
	{
		if (lockHandleWaitingFor != byte.MaxValue && lockHandleWaitingFor == _handle)
		{
			lockHandleWaitingFor = byte.MaxValue;
		}
	}

	public virtual void OnDestroy()
	{
		if (this.Destroyed != null)
		{
			this.Destroyed(this);
		}
	}

	public virtual void Reset(FastTags<TagGroup.Global> questTags)
	{
	}
}
