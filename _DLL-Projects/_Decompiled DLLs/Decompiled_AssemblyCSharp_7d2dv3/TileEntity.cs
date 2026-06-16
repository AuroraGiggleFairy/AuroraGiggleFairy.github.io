using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class TileEntity : ITileEntity, ILockTarget
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

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 19;

	[PublicizedFrom(EAccessModifier.Private)]
	public int readVersion = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3i chunkPos;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Chunk chunk;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ulong heapMapLastTime;

	[PublicizedFrom(EAccessModifier.Protected)]
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

	public virtual LockTargetType LockTargetType => LockTargetType.TileEntity;

	public event XUiEvent_TileEntityDestroyed Destroyed;

	public bool UseLocalVersioning()
	{
		if (readVersion == -1)
		{
			Log.Error("[TileEntity] read must be called before using this.");
			return false;
		}
		return readVersion >= 18;
	}

	public int GetLegacyForkVersion()
	{
		if (readVersion == -1)
		{
			Log.Error("[TileEntity] read must be called before using this.");
		}
		return readVersion;
	}

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

	public Chunk GetChunk()
	{
		return chunk;
	}

	public void SetChunk(Chunk _chunk)
	{
		chunk = _chunk;
	}

	public static TileEntity InstantiateFromRead(PooledBinaryReader _br, StreamModeRead _eStreamMode, TileEntityType _type, Chunk _chunk, int[] _blockIdMapping, Func<int, int, int, BlockValue> _getBlock)
	{
		if (TileEntityLegacyUtils.TryReadLegacyType(_br, _eStreamMode, _type, _getBlock, out var _tileEntity))
		{
			_tileEntity?.SetChunk(_chunk);
			return _tileEntity;
		}
		switch (_type)
		{
		case TileEntityType.Collector:
			_tileEntity = new TileEntityCollector(_chunk);
			break;
		case TileEntityType.Forge:
			_tileEntity = new TileEntityForge(_chunk);
			break;
		case TileEntityType.Workstation:
			_tileEntity = new TileEntityWorkstation(_chunk);
			break;
		case TileEntityType.VendingMachine:
			_tileEntity = new TileEntityVendingMachine(_chunk);
			break;
		case TileEntityType.Powered:
			_tileEntity = new TileEntityPoweredBlock(_chunk);
			break;
		case TileEntityType.PowerSource:
			_tileEntity = new TileEntityPowerSource(_chunk);
			break;
		case TileEntityType.PowerRangeTrap:
			_tileEntity = new TileEntityPoweredRangedTrap(_chunk);
			break;
		case TileEntityType.PowerMeleeTrap:
			_tileEntity = new TileEntityPoweredMeleeTrap(_chunk);
			break;
		case TileEntityType.Light:
			_tileEntity = new TileEntityLight(_chunk);
			break;
		case TileEntityType.Trigger:
			_tileEntity = new TileEntityPoweredTrigger(_chunk);
			break;
		case TileEntityType.Sleeper:
			_tileEntity = new TileEntitySleeper(_chunk);
			break;
		case TileEntityType.Composite:
			_tileEntity = new TileEntityComposite(_chunk);
			break;
		default:
			Log.Warning("Dropping TE with unknown/outdated type: " + _type.ToStringCached());
			return null;
		}
		if (_tileEntity is TileEntityComposite tileEntityComposite)
		{
			tileEntityComposite.read(_br, _eStreamMode, _blockIdMapping);
			return tileEntityComposite;
		}
		_tileEntity.read(_br, _eStreamMode);
		return _tileEntity;
	}

	public virtual void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		if (_eStreamMode == StreamModeRead.Persistency)
		{
			readVersion = _br.ReadUInt16();
			chunkPos = StreamUtils.ReadVector3i(_br);
			if (readVersion <= 18)
			{
				_br.ReadInt32();
			}
			if (readVersion > 1)
			{
				heapMapUpdateTime = _br.ReadUInt64();
				heapMapLastTime = heapMapUpdateTime - AIDirector.GetActivityWorldTimeDelay();
			}
		}
		else
		{
			chunkPos = StreamUtils.ReadVector3i(_br);
		}
	}

	public virtual void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		if (_eStreamMode == StreamModeWrite.Persistency)
		{
			_bw.Write((ushort)19);
			StreamUtils.Write(_bw, chunkPos);
			_bw.Write(heapMapUpdateTime);
		}
		else
		{
			StreamUtils.Write(_bw, chunkPos);
		}
	}

	public override string ToString()
	{
		return string.Format("[TE] " + GetTileEntityType().ToStringCached() + "/" + ToWorldPos().ToString());
	}

	public virtual void OnRemove(World world)
	{
		OnDestroy();
	}

	public virtual void OnLoad()
	{
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
		return true;
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
		if (GameManager.Instance.IsEditMode() && chunk != null && block.IsTileEntitySavedInPrefab())
		{
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	public override int GetHashCode()
	{
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

	public LockTargetType GetLockTargetType()
	{
		return LockTargetType.TileEntity;
	}

	public virtual bool IsSharedLock(ushort _channel)
	{
		return false;
	}

	public virtual bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return true;
	}

	public virtual bool CanLockOnServer(int _lockingPlayerId, ILockContext _context, ushort _channel)
	{
		return true;
	}

	public virtual void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
	}

	public virtual void OnLockedServer(bool _success, int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
	}

	public virtual void OnUnlockedServer(int _unlockingPlayerID, ushort _channel)
	{
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
