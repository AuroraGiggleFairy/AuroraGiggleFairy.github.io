using System.Collections.Generic;
using System.IO;
using System.Xml;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class AllyStore
{
	public enum AllyStatus : byte
	{
		NotAllied,
		Allies,
		OutgoingInvite,
		IncomingInvite
	}

	public enum AllyEvent : byte
	{
		None,
		OutgoingSent,
		OutgoingCanceled,
		IncomingAccepted,
		IncomingDeclined,
		AllyRemoved,
		OutgoingAccepted,
		OutgoingDeclined,
		IncomingReceived,
		IncomingCanceled,
		RemovedByAlly
	}

	public delegate void AllyChangeEvent(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target, AllyEvent _allyEventSource);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<PlatformUserIdentifierAbs, Dictionary<PlatformUserIdentifierAbs, AllyStatus>> relationships = new Dictionary<PlatformUserIdentifierAbs, Dictionary<PlatformUserIdentifierAbs, AllyStatus>>();

	public event AllyChangeEvent OnAllyChangeEvent;

	public void AllyUpdateRequest(PlatformUserIdentifierAbs _target, bool _addAlly)
	{
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		if (internalLocalUserIdentifier != null && _target != null)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageAllyRequest>().Setup(internalLocalUserIdentifier, _target, _addAlly));
			}
			else
			{
				ProcessAllyRequest(internalLocalUserIdentifier, _target, _addAlly);
			}
		}
	}

	public void ProcessAllyRequest(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target, bool _addAlly)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && _source != null && _target != null)
		{
			ComputeTransition(GetStatus(_source, _target), _addAlly, out var _newStatus, out var _eventSource, out var _eventTarget);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageAllyResponse>().Setup(_source, _target, _newStatus, _eventSource, _eventTarget));
			AllyUpdateResponse(_source, _target, _newStatus, _eventSource, _eventTarget);
		}
	}

	public void ApplyTransition(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target, bool _addAlly)
	{
		if (_source != null && _target != null)
		{
			ComputeTransition(GetStatus(_source, _target), _addAlly, out var _newStatus, out var _, out var _);
			SetStatus(_source, _target, _newStatus);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ComputeTransition(AllyStatus _oldStatus, bool addAlly, out AllyStatus _newStatus, out AllyEvent _eventSource, out AllyEvent _eventTarget)
	{
		_newStatus = _oldStatus;
		_eventSource = AllyEvent.None;
		_eventTarget = AllyEvent.None;
		switch (_oldStatus)
		{
		case AllyStatus.NotAllied:
			if (addAlly)
			{
				_newStatus = AllyStatus.OutgoingInvite;
				_eventSource = AllyEvent.OutgoingSent;
				_eventTarget = AllyEvent.IncomingReceived;
			}
			break;
		case AllyStatus.OutgoingInvite:
			if (!addAlly)
			{
				_newStatus = AllyStatus.NotAllied;
				_eventSource = AllyEvent.OutgoingCanceled;
				_eventTarget = AllyEvent.IncomingCanceled;
			}
			break;
		case AllyStatus.IncomingInvite:
			if (addAlly)
			{
				_newStatus = AllyStatus.Allies;
				_eventSource = AllyEvent.IncomingAccepted;
				_eventTarget = AllyEvent.OutgoingAccepted;
			}
			if (!addAlly)
			{
				_newStatus = AllyStatus.NotAllied;
				_eventSource = AllyEvent.IncomingDeclined;
				_eventTarget = AllyEvent.OutgoingDeclined;
			}
			break;
		case AllyStatus.Allies:
			if (!addAlly)
			{
				_newStatus = AllyStatus.NotAllied;
				_eventSource = AllyEvent.AllyRemoved;
				_eventTarget = AllyEvent.RemovedByAlly;
			}
			break;
		}
	}

	public void AllyUpdateResponse(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target, AllyStatus _newStatus, AllyEvent _allyEventSource, AllyEvent _allyEventTarget)
	{
		SetStatus(_source, _target, _newStatus);
		this.OnAllyChangeEvent?.Invoke(_source, _target, _allyEventSource);
		this.OnAllyChangeEvent?.Invoke(_target, _source, _allyEventTarget);
	}

	public bool IsAlly(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target)
	{
		return GetStatus(_source, _target) == AllyStatus.Allies;
	}

	public IEnumerable<PlatformUserIdentifierAbs> EnumerateAllies(PlatformUserIdentifierAbs _id)
	{
		if (_id == null || !relationships.TryGetValue(_id, out var value))
		{
			yield break;
		}
		foreach (KeyValuePair<PlatformUserIdentifierAbs, AllyStatus> item in value)
		{
			if (item.Value == AllyStatus.Allies)
			{
				yield return item.Key;
			}
		}
	}

	public bool HasAllies(PlatformUserIdentifierAbs _id)
	{
		if (_id == null || !relationships.TryGetValue(_id, out var value))
		{
			return false;
		}
		foreach (KeyValuePair<PlatformUserIdentifierAbs, AllyStatus> item in value)
		{
			if (item.Value == AllyStatus.Allies)
			{
				return true;
			}
		}
		return false;
	}

	public AllyStatus GetStatus(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target)
	{
		if (_source == null || _target == null)
		{
			return AllyStatus.NotAllied;
		}
		if (relationships.TryGetValue(_source, out var value))
		{
			if (!value.TryGetValue(_target, out var value2))
			{
				return AllyStatus.NotAllied;
			}
			return value2;
		}
		return AllyStatus.NotAllied;
	}

	public void SetStatus(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target, AllyStatus _status)
	{
		if (_source == null || _target == null)
		{
			return;
		}
		if (_status == AllyStatus.NotAllied)
		{
			ClearStatus(_source, _target);
			return;
		}
		if (!relationships.ContainsKey(_source))
		{
			relationships.Add(_source, new Dictionary<PlatformUserIdentifierAbs, AllyStatus>());
		}
		if (!relationships.ContainsKey(_target))
		{
			relationships.Add(_target, new Dictionary<PlatformUserIdentifierAbs, AllyStatus>());
		}
		switch (_status)
		{
		case AllyStatus.Allies:
			relationships[_source][_target] = AllyStatus.Allies;
			relationships[_target][_source] = AllyStatus.Allies;
			break;
		case AllyStatus.OutgoingInvite:
			relationships[_source][_target] = AllyStatus.OutgoingInvite;
			relationships[_target][_source] = AllyStatus.IncomingInvite;
			break;
		case AllyStatus.IncomingInvite:
			relationships[_source][_target] = AllyStatus.IncomingInvite;
			relationships[_target][_source] = AllyStatus.OutgoingInvite;
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearStatus(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target)
	{
		if (relationships.TryGetValue(_source, out var value))
		{
			value.Remove(_target);
		}
		if (relationships.TryGetValue(_target, out var value2))
		{
			value2.Remove(_source);
		}
	}

	public void ClearAll()
	{
		relationships.Clear();
	}

	public void CopyFrom(AllyStore _other)
	{
		relationships.Clear();
		if (_other == null)
		{
			return;
		}
		foreach (KeyValuePair<PlatformUserIdentifierAbs, Dictionary<PlatformUserIdentifierAbs, AllyStatus>> relationship in _other.relationships)
		{
			foreach (KeyValuePair<PlatformUserIdentifierAbs, AllyStatus> item in relationship.Value)
			{
				SetStatus(relationship.Key, item.Key, item.Value);
			}
		}
	}

	public void ReadXml(XmlElement _alliesElement, int _readVersion)
	{
		if (_readVersion == 0)
		{
			return;
		}
		foreach (XmlNode childNode in _alliesElement.ChildNodes)
		{
			if (!(childNode is XmlElement { Name: "ally" } xmlElement))
			{
				continue;
			}
			PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromXml(xmlElement, _warnings: true, "a");
			PlatformUserIdentifierAbs platformUserIdentifierAbs2 = PlatformUserIdentifierAbs.FromXml(xmlElement, _warnings: true, "b");
			if (platformUserIdentifierAbs != null && platformUserIdentifierAbs2 != null)
			{
				if (xmlElement.GetAttribute("status") == "allies")
				{
					SetStatus(platformUserIdentifierAbs, platformUserIdentifierAbs2, AllyStatus.Allies);
				}
				else if (xmlElement.GetAttribute("status") == "pending")
				{
					SetStatus(platformUserIdentifierAbs, platformUserIdentifierAbs2, AllyStatus.OutgoingInvite);
				}
			}
		}
	}

	public void WriteXml(XmlElement _root)
	{
		bool flag = false;
		foreach (KeyValuePair<PlatformUserIdentifierAbs, Dictionary<PlatformUserIdentifierAbs, AllyStatus>> relationship in relationships)
		{
			if (relationship.Value.Count > 0)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		XmlElement node = _root.AddXmlElement("allies");
		foreach (KeyValuePair<PlatformUserIdentifierAbs, Dictionary<PlatformUserIdentifierAbs, AllyStatus>> relationship2 in relationships)
		{
			foreach (KeyValuePair<PlatformUserIdentifierAbs, AllyStatus> item in relationship2.Value)
			{
				PlatformUserIdentifierAbs key = relationship2.Key;
				PlatformUserIdentifierAbs key2 = item.Key;
				AllyStatus value = item.Value;
				if (value != AllyStatus.NotAllied && value != AllyStatus.IncomingInvite && (value != AllyStatus.Allies || string.CompareOrdinal(key.CombinedString, key2.CombinedString) <= 0))
				{
					XmlElement xmlElement = node.AddXmlElement("ally");
					key.ToXml(xmlElement, "a");
					key2.ToXml(xmlElement, "b");
					if (value == AllyStatus.Allies)
					{
						xmlElement.SetAttribute("status", "allies");
					}
					else
					{
						xmlElement.SetAttribute("status", "pending");
					}
				}
			}
		}
	}

	public void Read(BinaryReader _br)
	{
		relationships.Clear();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			PlatformUserIdentifierAbs source = PlatformUserIdentifierAbs.FromStream(_br);
			PlatformUserIdentifierAbs target = PlatformUserIdentifierAbs.FromStream(_br);
			AllyStatus status = (AllyStatus)_br.ReadByte();
			SetStatus(source, target, status);
		}
	}

	public void Write(BinaryWriter _bw)
	{
		int num = 0;
		foreach (KeyValuePair<PlatformUserIdentifierAbs, Dictionary<PlatformUserIdentifierAbs, AllyStatus>> relationship in relationships)
		{
			foreach (KeyValuePair<PlatformUserIdentifierAbs, AllyStatus> item in relationship.Value)
			{
				_ = item;
				num++;
			}
		}
		num /= 2;
		_bw.Write(num);
		foreach (KeyValuePair<PlatformUserIdentifierAbs, Dictionary<PlatformUserIdentifierAbs, AllyStatus>> relationship2 in relationships)
		{
			foreach (KeyValuePair<PlatformUserIdentifierAbs, AllyStatus> item2 in relationship2.Value)
			{
				if (string.CompareOrdinal(relationship2.Key.CombinedString, item2.Key.CombinedString) <= 0)
				{
					relationship2.Key.ToStream(_bw);
					item2.Key.ToStream(_bw);
					_bw.Write((byte)item2.Value);
				}
			}
		}
	}
}
