using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class ClientInfoCollection
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ClientInfo> list = new List<ClientInfo>();

	public readonly ReadOnlyCollection<ClientInfo> List;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<int, ClientInfo> clientNumberMap = new Dictionary<int, ClientInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<int, ClientInfo> entityIdMap = new Dictionary<int, ClientInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<long, ClientInfo> litenetPeerMap = new Dictionary<long, ClientInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<PlatformUserIdentifierAbs, ClientInfo> userIdMap = new Dictionary<PlatformUserIdentifierAbs, ClientInfo>();

	public int Count => list.Count;

	public ClientInfoCollection()
	{
		List = new ReadOnlyCollection<ClientInfo>(list);
	}

	public void Add(ClientInfo _cInfo)
	{
		list.Add(_cInfo);
		clientNumberMap.Add(_cInfo.ClientNumber, _cInfo);
	}

	public void Clear()
	{
		list.Clear();
		clientNumberMap.Clear();
		entityIdMap.Clear();
		litenetPeerMap.Clear();
		userIdMap.Clear();
	}

	public bool Contains(ClientInfo _cInfo)
	{
		return list.Contains(_cInfo);
	}

	public void Remove(ClientInfo _cInfo)
	{
		list.Remove(_cInfo);
		clientNumberMap.Remove(_cInfo.ClientNumber);
		entityIdMap.Remove(_cInfo.entityId);
		if (_cInfo.litenetPeerConnectId >= 0)
		{
			litenetPeerMap.Remove(_cInfo.litenetPeerConnectId);
		}
		if (_cInfo.PlatformId != null)
		{
			userIdMap.Remove(_cInfo.PlatformId);
		}
		if (_cInfo.CrossplatformId != null)
		{
			userIdMap.Remove(_cInfo.CrossplatformId);
		}
	}

	public ClientInfo ForClientNumber(int _clientNumber)
	{
		if (clientNumberMap.TryGetValue(_clientNumber, out var value))
		{
			return value;
		}
		return null;
	}

	public ClientInfo ForEntityId(int _entityId)
	{
		if (entityIdMap.TryGetValue(_entityId, out var value))
		{
			return value;
		}
		for (int i = 0; i < list.Count; i++)
		{
			ClientInfo clientInfo = list[i];
			if (clientInfo.entityId == _entityId)
			{
				entityIdMap.Add(_entityId, clientInfo);
				return clientInfo;
			}
		}
		return null;
	}

	public ClientInfo ForLiteNetPeer(long _peerConnectId)
	{
		if (litenetPeerMap.TryGetValue(_peerConnectId, out var value))
		{
			return value;
		}
		for (int i = 0; i < list.Count; i++)
		{
			ClientInfo clientInfo = list[i];
			if (clientInfo.litenetPeerConnectId == _peerConnectId)
			{
				litenetPeerMap.Add(_peerConnectId, clientInfo);
				return clientInfo;
			}
		}
		return null;
	}

	public ClientInfo ForUserId(PlatformUserIdentifierAbs _userIdentifier)
	{
		if (userIdMap.TryGetValue(_userIdentifier, out var value))
		{
			return value;
		}
		for (int i = 0; i < list.Count; i++)
		{
			ClientInfo clientInfo = list[i];
			if (_userIdentifier.Equals(clientInfo.PlatformId))
			{
				userIdMap[_userIdentifier] = clientInfo;
				return clientInfo;
			}
			if (_userIdentifier.Equals(clientInfo.CrossplatformId))
			{
				userIdMap[_userIdentifier] = clientInfo;
				return clientInfo;
			}
		}
		return null;
	}

	public ClientInfo GetForPlayerName(string _playerName, bool _ignoreCase = true, bool _ignoreBlanks = false)
	{
		if (_ignoreBlanks)
		{
			_playerName = _playerName.Replace(" ", "");
		}
		for (int i = 0; i < list.Count; i++)
		{
			ClientInfo clientInfo = list[i];
			string text = clientInfo.playerName ?? string.Empty;
			if (_ignoreBlanks)
			{
				text = text.Replace(" ", "");
			}
			if (string.Equals(text, _playerName, _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
			{
				return clientInfo;
			}
		}
		return null;
	}

	public ClientInfo GetForNameOrId(string _nameOrId, bool _ignoreCase = true, bool _ignoreBlanks = false)
	{
		if (int.TryParse(_nameOrId, out var result))
		{
			ClientInfo clientInfo = ForEntityId(result);
			if (clientInfo != null)
			{
				return clientInfo;
			}
		}
		if (PlatformUserIdentifierAbs.TryFromCombinedString(_nameOrId, out var _userIdentifier))
		{
			ClientInfo clientInfo2 = ForUserId(_userIdentifier);
			if (clientInfo2 != null)
			{
				return clientInfo2;
			}
		}
		return GetForPlayerName(_nameOrId, _ignoreCase, _ignoreBlanks);
	}
}
