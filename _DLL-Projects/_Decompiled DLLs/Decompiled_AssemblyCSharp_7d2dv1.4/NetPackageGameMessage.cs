using UnityEngine.Scripting;

[Preserve]
public class NetPackageGameMessage : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EnumGameMessages msgType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string msg;

	[PublicizedFrom(EAccessModifier.Private)]
	public int mainEntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int secondaryEntityId;

	public NetPackageGameMessage Setup(EnumGameMessages _type, string _msg, int _mainEntityId, int _secondaryEntityId)
	{
		msgType = _type;
		msg = (string.IsNullOrEmpty(_msg) ? string.Empty : _msg);
		mainEntityId = _mainEntityId;
		secondaryEntityId = _secondaryEntityId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		msgType = (EnumGameMessages)_br.ReadByte();
		msg = _br.ReadString();
		mainEntityId = _br.ReadInt32();
		secondaryEntityId = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)msgType);
		_bw.Write(msg);
		_bw.Write(mainEntityId);
		_bw.Write(secondaryEntityId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			if (!_world.IsRemote())
			{
				GameManager.Instance.GameMessageServer(base.Sender, msgType, msg, mainEntityId, secondaryEntityId);
			}
			else
			{
				GameManager.Instance.DisplayGameMessage(msgType, mainEntityId, secondaryEntityId);
			}
		}
	}

	public override int GetLength()
	{
		return 0;
	}
}
