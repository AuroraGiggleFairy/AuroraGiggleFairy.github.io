using UnityEngine.Scripting;

[Preserve]
public class NetPackageTELock : NetPackage
{
	public enum TELockType : byte
	{
		LockServer,
		UnlockServer,
		AccessClient,
		DeniedAccess
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TELockType type;

	[PublicizedFrom(EAccessModifier.Private)]
	public int clrIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public int posX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int posY;

	[PublicizedFrom(EAccessModifier.Private)]
	public int posZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lootEntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityIdThatOpenedIt;

	[PublicizedFrom(EAccessModifier.Private)]
	public string customUi;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool allowEmptyDestroy;

	public NetPackageTELock Setup(TELockType _type, int _clrIdx, Vector3i _pos, int _lootEntityId, int _entityIdThatOpenedIt, string _customUi = null, bool _allowEmptyDestroy = true)
	{
		type = _type;
		clrIdx = _clrIdx;
		posX = _pos.x;
		posY = _pos.y;
		posZ = _pos.z;
		lootEntityId = _lootEntityId;
		entityIdThatOpenedIt = _entityIdThatOpenedIt;
		customUi = _customUi ?? "";
		allowEmptyDestroy = _allowEmptyDestroy;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		type = (TELockType)_br.ReadByte();
		clrIdx = _br.ReadInt16();
		posX = _br.ReadInt32();
		posY = _br.ReadInt32();
		posZ = _br.ReadInt32();
		lootEntityId = _br.ReadInt32();
		entityIdThatOpenedIt = _br.ReadInt32();
		customUi = _br.ReadString();
		allowEmptyDestroy = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)type);
		_bw.Write((short)clrIdx);
		_bw.Write(posX);
		_bw.Write(posY);
		_bw.Write(posZ);
		_bw.Write(lootEntityId);
		_bw.Write(entityIdThatOpenedIt);
		_bw.Write(customUi);
		_bw.Write(allowEmptyDestroy);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (type == TELockType.UnlockServer || ValidEntityIdForSender(entityIdThatOpenedIt))
		{
			switch (type)
			{
			case TELockType.LockServer:
				_world.GetGameManager().TELockServer(clrIdx, new Vector3i(posX, posY, posZ), lootEntityId, entityIdThatOpenedIt, customUi);
				break;
			case TELockType.UnlockServer:
				_world.GetGameManager().TEUnlockServer(clrIdx, new Vector3i(posX, posY, posZ), lootEntityId, allowEmptyDestroy);
				break;
			case TELockType.AccessClient:
				_world.GetGameManager().TEAccessClient(clrIdx, new Vector3i(posX, posY, posZ), lootEntityId, entityIdThatOpenedIt, customUi);
				break;
			case TELockType.DeniedAccess:
				_world.GetGameManager().TEDeniedAccessClient(clrIdx, new Vector3i(posX, posY, posZ), lootEntityId, entityIdThatOpenedIt);
				break;
			}
		}
	}

	public override int GetLength()
	{
		return 27 + customUi.Length * 2 + 1;
	}
}
