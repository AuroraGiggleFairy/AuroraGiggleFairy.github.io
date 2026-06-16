using System;

public interface ILockTarget
{
	LockTargetType LockTargetType { get; }

	bool IsSharedLock(ushort _channel);

	bool CanLockOnServer(int _lockingPlayerID, ILockContext _context, ushort _channel);

	bool CanLockLocally(ILockContext _context, ushort _channel);

	void OnLockedServer(bool _success, int _lockingPlayerID, ILockContext _context, ushort _channel);

	void OnLockedLocal(bool _success, ILockContext _context, ushort _channel);

	void OnUnlockedServer(int _unlockingPlayerId, ushort _channel);

	static void WriteIdentifyingInfo(ILockTarget _target, PooledBinaryWriter _bw)
	{
		if (_target == null)
		{
			_bw.Write((byte)0);
			return;
		}
		_bw.Write((byte)1);
		LockTargetType lockTargetType = _target.LockTargetType;
		_bw.Write((byte)lockTargetType);
		switch (lockTargetType)
		{
		case LockTargetType.TileEntity:
			if (!(_target is TileEntity tileEntity))
			{
				throw new InvalidOperationException(string.Format("[{0}] Expected {1} for {2}.", "ILockTarget", "TileEntity", lockTargetType));
			}
			StreamUtils.Write(_bw, tileEntity.ToWorldPos());
			break;
		case LockTargetType.TEFeature:
			if (!(_target is TEFeatureAbs tEFeatureAbs))
			{
				throw new InvalidOperationException(string.Format("[{0}] Expected {1} for {2}.", "ILockTarget", "TEFeatureAbs", lockTargetType));
			}
			StreamUtils.Write(_bw, tEFeatureAbs.ToWorldPos());
			_bw.Write(tEFeatureAbs.FeatureData?.Name ?? string.Empty);
			break;
		case LockTargetType.Entity:
			if (!(_target is Entity entity))
			{
				throw new InvalidOperationException(string.Format("[{0}] Expected {1} for {2}.", "ILockTarget", "Entity", lockTargetType));
			}
			_bw.Write(entity.entityId);
			break;
		case LockTargetType.TransactionalInventory:
			if (!(_target is TransactionalInventory transactionalInventory))
			{
				throw new InvalidOperationException(string.Format("[{0}] Expected {1} for {2}.", "ILockTarget", "TransactionalInventory", lockTargetType));
			}
			StreamUtils.Write(_bw, transactionalInventory.Key);
			break;
		default:
			throw new NotSupportedException(string.Format("[{0}] Unsupported type {1}.", "ILockTarget", lockTargetType));
		}
	}

	static ILockTarget ReadIdentifyingInfo(PooledBinaryReader _br)
	{
		if (_br.ReadByte() == 0)
		{
			return null;
		}
		LockTargetType lockTargetType = (LockTargetType)_br.ReadByte();
		switch (lockTargetType)
		{
		case LockTargetType.TileEntity:
		{
			Vector3i vector3i2 = StreamUtils.ReadVector3i(_br);
			TileEntity tileEntity = GameManager.Instance.World.GetTileEntity(vector3i2);
			if (tileEntity == null)
			{
				Log.Warning(string.Format("[{0}] {1} not found (pos={2}).", "ILockTarget", "TileEntity", vector3i2));
			}
			return tileEntity;
		}
		case LockTargetType.TEFeature:
		{
			Vector3i vector3i = StreamUtils.ReadVector3i(_br);
			string text = _br.ReadString();
			if (!(GameManager.Instance.World.GetTileEntity(vector3i) is TileEntityComposite tileEntityComposite))
			{
				Log.Warning(string.Format("[{0}] {1} parent not found (pos={2}, name='{3}').", "ILockTarget", "TEFeatureAbs", vector3i, text));
				return null;
			}
			TEFeatureAbs obj = tileEntityComposite.GetFeature(text.AsMemory()) as TEFeatureAbs;
			if (obj == null)
			{
				Log.Warning("[ILockTarget] TEFeatureAbs '" + text + "' not found on parent.");
			}
			return obj;
		}
		case LockTargetType.Entity:
		{
			int num = _br.ReadInt32();
			ILockTarget lockTarget = null;
			if (num != -1)
			{
				lockTarget = GameManager.Instance.World.GetEntity(num);
			}
			if (lockTarget == null)
			{
				Log.Warning(string.Format("[{0}] {1} not found (id={2}).", "ILockTarget", "Entity", num));
			}
			return lockTarget;
		}
		case LockTargetType.TransactionalInventory:
		{
			Guid guid = StreamUtils.ReadGuid(_br);
			if (!InventoryManager.Instance.TryGetTransactionalInventory(guid, out var _inventory))
			{
				Log.Warning(string.Format("[{0}] {1} not found (key={2}).", "ILockTarget", "TransactionalInventory", guid));
				return null;
			}
			return _inventory;
		}
		default:
			Log.Warning(string.Format("[{0}] Unsupported type during read: {1}.", "ILockTarget", lockTargetType));
			return null;
		}
	}
}
