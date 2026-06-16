using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageChat : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EChatType chatType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int senderEntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string msg;

	[PublicizedFrom(EAccessModifier.Private)]
	public EMessageSender msgSender;

	[PublicizedFrom(EAccessModifier.Private)]
	public GeneratedTextManager.BbCodeSupportMode bbMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> recipientEntityIds;

	public NetPackageChat Setup(EChatType _chatType, int _senderEntityId, string _msg, List<int> _recipientEntityIds, EMessageSender _msgSender, GeneratedTextManager.BbCodeSupportMode _bbMode)
	{
		chatType = _chatType;
		senderEntityId = _senderEntityId;
		msg = (string.IsNullOrEmpty(_msg) ? string.Empty : _msg);
		msgSender = _msgSender;
		bbMode = _bbMode;
		recipientEntityIds = _recipientEntityIds;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		chatType = (EChatType)_br.ReadByte();
		senderEntityId = _br.ReadInt32();
		msg = _br.ReadString();
		msgSender = (EMessageSender)_br.ReadByte();
		bbMode = (GeneratedTextManager.BbCodeSupportMode)_br.ReadByte();
		int num = _br.ReadInt32();
		if (num > 0)
		{
			recipientEntityIds = new List<int>();
			for (int i = 0; i < num; i++)
			{
				recipientEntityIds.Add(_br.ReadInt32());
			}
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)chatType);
		_bw.Write(senderEntityId);
		_bw.Write(msg);
		_bw.Write((byte)msgSender);
		_bw.Write((byte)bbMode);
		_bw.Write((recipientEntityIds != null) ? recipientEntityIds.Count : 0);
		if (recipientEntityIds != null && recipientEntityIds.Count > 0)
		{
			for (int i = 0; i < recipientEntityIds.Count; i++)
			{
				_bw.Write(recipientEntityIds[i]);
			}
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			if (!_world.IsRemote())
			{
				GameManager.Instance.ChatMessageServer(base.Sender, chatType, senderEntityId, msg, recipientEntityIds, msgSender, bbMode);
			}
			else
			{
				GameManager.Instance.ChatMessageClient(chatType, senderEntityId, msg, null, msgSender, bbMode);
			}
		}
	}

	public override int GetLength()
	{
		int num = ((recipientEntityIds != null) ? recipientEntityIds.Count : 0);
		return 7 + msg.Length + 4 * num;
	}
}
