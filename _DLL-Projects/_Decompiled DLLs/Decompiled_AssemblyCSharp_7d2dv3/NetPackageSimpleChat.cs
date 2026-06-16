using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSimpleChat : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> recipientEntityIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public string msg;

	public NetPackageSimpleChat Setup(string _msg)
	{
		msg = (string.IsNullOrEmpty(_msg) ? string.Empty : _msg);
		return this;
	}

	public NetPackageSimpleChat Setup(string _msg, List<int> _recipientEntityIds)
	{
		msg = (string.IsNullOrEmpty(_msg) ? string.Empty : _msg);
		recipientEntityIds = _recipientEntityIds;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		msg = _br.ReadString();
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
		_bw.Write(msg);
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
		if (_world == null)
		{
			return;
		}
		if (!_world.IsRemote())
		{
			if (recipientEntityIds == null)
			{
				return;
			}
			{
				foreach (int recipientEntityId in recipientEntityIds)
				{
					if (_world.GetEntity(recipientEntityId) is EntityPlayerLocal entityPlayer)
					{
						LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayer);
						if (null != uIForPlayer && null != uIForPlayer.windowManager)
						{
							XUiC_ChatOutput.AddMessage(uIForPlayer.xui, EnumGameMessages.PlainTextLocal, msg, EChatType.Global, EChatDirection.Inbound, -1, null, null, EMessageSender.Server);
						}
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(recipientEntityId)?.SendPackage(NetPackageManager.GetPackage<NetPackageSimpleChat>().Setup(msg));
					}
				}
				return;
			}
		}
		List<EntityPlayerLocal> localPlayers = _callbacks.World.GetLocalPlayers();
		for (int i = 0; i < localPlayers.Count; i++)
		{
			LocalPlayerUI uIForPlayer2 = LocalPlayerUI.GetUIForPlayer(localPlayers[i]);
			if (null != uIForPlayer2 && null != uIForPlayer2.windowManager)
			{
				XUiC_ChatOutput.AddMessage(uIForPlayer2.xui, EnumGameMessages.PlainTextLocal, msg, EChatType.Global, EChatDirection.Inbound, -1, null, null, EMessageSender.Server);
			}
		}
	}

	public override int GetLength()
	{
		return 4 + msg.Length;
	}
}
