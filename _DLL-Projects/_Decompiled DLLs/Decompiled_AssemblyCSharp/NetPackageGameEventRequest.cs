using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageGameEventRequest : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string eventName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string extraData;

	[PublicizedFrom(EAccessModifier.Private)]
	public string tag;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTwitchEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool crateShare;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool allowRefunds = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 targetPos = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sequenceLink = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Tuple<string, string>> variables = new List<Tuple<string, string>>();

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageGameEventRequest Setup(string _event, int _entityId, bool _isTwitchEvent, Vector3 _targetPos, List<Tuple<string, string>> _variables, string _extraData = "", string _tag = "", bool _crateShare = false, bool _allowRefunds = true, string _sequenceLink = "")
	{
		eventName = _event;
		entityID = _entityId;
		extraData = _extraData;
		tag = _tag;
		isTwitchEvent = _isTwitchEvent;
		crateShare = _crateShare;
		targetPos = _targetPos;
		allowRefunds = _allowRefunds;
		sequenceLink = _sequenceLink ?? "";
		variables = _variables;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		variables.Clear();
		eventName = _br.ReadString();
		entityID = _br.ReadInt32();
		extraData = _br.ReadString();
		tag = _br.ReadString();
		isTwitchEvent = _br.ReadBoolean();
		crateShare = _br.ReadBoolean();
		allowRefunds = _br.ReadBoolean();
		sequenceLink = _br.ReadString();
		byte b = _br.ReadByte();
		for (int i = 0; i < b; i++)
		{
			string item = _br.ReadString();
			string item2 = _br.ReadString();
			variables.Add(new Tuple<string, string>(item, item2));
		}
		targetPos = StreamUtils.ReadVector3(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(eventName);
		_bw.Write(entityID);
		_bw.Write(extraData);
		_bw.Write(tag);
		_bw.Write(isTwitchEvent);
		_bw.Write(crateShare);
		_bw.Write(allowRefunds);
		_bw.Write(sequenceLink);
		int num = 0;
		if (variables != null)
		{
			num = variables.Count;
			if (num > 255)
			{
				num = 255;
			}
		}
		byte value = (byte)num;
		_bw.Write(value);
		for (int i = 0; i < num; i++)
		{
			Tuple<string, string> tuple = variables[i];
			_bw.Write(tuple.Item1);
			_bw.Write(tuple.Item2);
		}
		StreamUtils.Write(_bw, targetPos);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(base.Sender.entityId) as EntityPlayer;
		Entity entity = GameManager.Instance.World.GetEntity(entityID);
		EntityPlayer entityPlayer2 = entity as EntityPlayer;
		if (!(entityPlayer2 == null) && !(entityPlayer == entityPlayer2) && (entityPlayer.Party == null || !entityPlayer.Party.ContainsMember(entityPlayer2)))
		{
			return;
		}
		if (GameEventManager.Current.HandleAction(eventName, entityPlayer, entity, isTwitchEvent, targetPos, extraData, tag, crateShare, allowRefunds, sequenceLink, null, variables))
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(eventName, entity ? entity.entityId : (-1), extraData, tag, NetPackageGameEventResponse.ResponseTypes.Approved), _onlyClientsAttachedToAnEntity: false, base.Sender.entityId);
			if (!isTwitchEvent || entityPlayer.Party == null)
			{
				return;
			}
			for (int i = 0; i < entityPlayer.Party.MemberList.Count; i++)
			{
				EntityPlayer entityPlayer3 = entityPlayer.Party.MemberList[i];
				if (entityPlayer3 != entityPlayer && entityPlayer3.TwitchEnabled)
				{
					if (entityPlayer3 is EntityPlayerLocal)
					{
						GameEventManager.Current.HandleTwitchPartyGameEventApproved(eventName, entity ? entity.entityId : (-1), extraData, tag);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(eventName, entity ? entity.entityId : (-1), extraData, tag, NetPackageGameEventResponse.ResponseTypes.TwitchPartyActionApproved), _onlyClientsAttachedToAnEntity: false, entityPlayer3.entityId);
					}
				}
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(eventName, entity ? entity.entityId : (-1), extraData, tag, NetPackageGameEventResponse.ResponseTypes.Denied), _onlyClientsAttachedToAnEntity: false, base.Sender.entityId);
		}
	}

	public override int GetLength()
	{
		return 30;
	}
}
