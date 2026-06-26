using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class PersistentPlayerList
{
	public ObservableDictionary<PlatformUserIdentifierAbs, PersistentPlayerData> Players = new ObservableDictionary<PlatformUserIdentifierAbs, PersistentPlayerData>();

	public Dictionary<int, PersistentPlayerData> EntityToPlayerMap = new Dictionary<int, PersistentPlayerData>();

	public Dictionary<PlatformUserIdentifierAbs, int> PlayerToEntityMap = new Dictionary<PlatformUserIdentifierAbs, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PersistentPlayerData.PlayerEventHandler> m_dispatch;

	public Dictionary<Vector3i, PersistentPlayerData> m_lpBlockMap = new Dictionary<Vector3i, PersistentPlayerData>();

	public void HandlePlayerDetailsUpdate(IPlatformUserData platformUserData, string name)
	{
		if (Players.TryGetValue(platformUserData.PrimaryId, out var value))
		{
			value.PlayerName.Update(name, platformUserData.PrimaryId);
		}
	}

	public void PlaceLandProtectionBlock(Vector3i pos, PersistentPlayerData owner)
	{
		if (m_lpBlockMap.TryGetValue(pos, out var value))
		{
			value.RemoveLandProtectionBlock(pos);
		}
		owner.AddLandProtectionBlock(pos);
		RemoveExtraLandClaims(owner);
		m_lpBlockMap[pos] = owner;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveExtraLandClaims(PersistentPlayerData owner)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		int num = GameStats.GetInt(EnumGameStats.LandClaimCount);
		int num2 = owner.LPBlocks.Count - num;
		for (int i = 0; i < num2; i++)
		{
			Vector3i blockPos = owner.LPBlocks[0];
			BlockLandClaim.HandleDeactivateLandClaim(blockPos);
			owner.LPBlocks.RemoveAt(0);
			if (GameManager.Instance.World != null)
			{
				NavObjectManager.Instance.UnRegisterNavObjectByPosition(blockPos.ToVector3(), "land_claim");
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.LandClaim, blockPos.ToVector3()));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveLandClaimsFromDictionaryForPlayer(PersistentPlayerData owner)
	{
		List<Vector3i> landProtectionBlocks = owner.GetLandProtectionBlocks();
		if (landProtectionBlocks != null)
		{
			for (int i = 0; i < landProtectionBlocks.Count; i++)
			{
				m_lpBlockMap.Remove(landProtectionBlocks[i]);
			}
		}
	}

	public void RemoveLandProtectionBlock(Vector3i pos)
	{
		if (!m_lpBlockMap.TryGetValue(pos, out var value))
		{
			return;
		}
		m_lpBlockMap.Remove(pos);
		value.RemoveLandProtectionBlock(pos);
		if (GameManager.Instance.World != null)
		{
			NavObjectManager.Instance.UnRegisterNavObjectByPosition(pos.ToVector3(), "land_claim");
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.LandClaim, pos.ToVector3()));
			}
		}
	}

	public PersistentPlayerData GetLandProtectionBlockOwner(Vector3i pos)
	{
		m_lpBlockMap.TryGetValue(pos, out var value);
		return value;
	}

	public void AddPlayerEventHandler(PersistentPlayerData.PlayerEventHandler handler)
	{
		if (m_dispatch == null)
		{
			m_dispatch = new List<PersistentPlayerData.PlayerEventHandler>();
		}
		m_dispatch.Add(handler);
	}

	public void RemovePlayerEventHandler(PersistentPlayerData.PlayerEventHandler handler)
	{
		m_dispatch.Remove(handler);
	}

	public void DispatchPlayerEvent(PersistentPlayerData player, PersistentPlayerData otherPlayer, EnumPersistentPlayerDataReason reason)
	{
		if (m_dispatch != null)
		{
			for (int i = 0; i < m_dispatch.Count; i++)
			{
				m_dispatch[i](player, otherPlayer, reason);
			}
		}
	}

	public PersistentPlayerData GetPlayerDataFromEntityID(int _entityId)
	{
		if (!EntityToPlayerMap.TryGetValue(_entityId, out var value))
		{
			return null;
		}
		return value;
	}

	public PersistentPlayerData GetPlayerData(PlatformUserIdentifierAbs _userIdentifier)
	{
		if (_userIdentifier == null)
		{
			return null;
		}
		if (!Players.TryGetValue(_userIdentifier, out var value))
		{
			return null;
		}
		return value;
	}

	public PersistentPlayerData CreatePlayerData(PlatformUserIdentifierAbs _primaryId, PlatformUserIdentifierAbs _nativeId, string _playerName, EPlayGroup _playGroup)
	{
		PersistentPlayerData persistentPlayerData = new PersistentPlayerData(_primaryId, _nativeId, new AuthoredText(_playerName, _primaryId), _playGroup);
		persistentPlayerData.EntityId = -1;
		persistentPlayerData.LastLogin = DateTime.Now;
		Players[_primaryId] = persistentPlayerData;
		return persistentPlayerData;
	}

	public void UnmapPlayer(PlatformUserIdentifierAbs _userIdentifier)
	{
		PersistentPlayerData playerData = GetPlayerData(_userIdentifier);
		if (playerData != null && playerData.EntityId != -1)
		{
			EntityToPlayerMap.Remove(playerData.EntityId);
			PlayerToEntityMap.Remove(_userIdentifier);
			playerData.EntityId = -1;
		}
	}

	public void MapPlayer(PersistentPlayerData ppd)
	{
		if (ppd.EntityId != -1)
		{
			EntityToPlayerMap[ppd.EntityId] = ppd;
			PlayerToEntityMap[ppd.PrimaryId] = ppd.EntityId;
		}
	}

	public void AutoFixNameCollisions()
	{
		if (EntityToPlayerMap.Count == 0)
		{
			return;
		}
		HashSet<string> hashSet = new HashSet<string>();
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> player in Players)
		{
			hashSet.Add(player.Value.PlayerName.AuthoredName.Text);
		}
		foreach (string item in hashSet)
		{
			FixNameCollisions(item);
		}
		GameManager.Instance.persistentPlayers.Players.EntryModified += NameCollisionEvent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NameCollisionEvent(object _sender, DictionaryChangedEventArgs<PlatformUserIdentifierAbs, PersistentPlayerData> _entry)
	{
		GameManager.Instance.persistentPlayers.FixNameCollisions(_entry.Value.PlayerName.AuthoredName.Text);
	}

	public void FixNameCollisions(string _name)
	{
		if (EntityToPlayerMap.Count == 0 || _name == null)
		{
			return;
		}
		int num = 0;
		string text = ((PlatformManager.MultiPlatform.User?.PlatformUserId != null && Players.ContainsKey(PlatformManager.MultiPlatform.User.PlatformUserId)) ? Players[PlatformManager.MultiPlatform.User.PlatformUserId].PlayerName.AuthoredName.Text : null);
		EPlatformIdentifier platformIdentifier = PlatformManager.NativePlatform.PlatformIdentifier;
		PlatformUserIdentifierAbs platformUserIdentifierAbs = null;
		if (_name == text)
		{
			platformUserIdentifierAbs = PlatformManager.MultiPlatform.User.PlatformUserId;
			num++;
		}
		else
		{
			foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> player in Players)
			{
				if (platformIdentifier == player.Value.PlatformData.NativeId.PlatformIdentifier && _name == player.Value.PlayerName.AuthoredName.Text)
				{
					platformUserIdentifierAbs = player.Value.PlatformData.PrimaryId;
					num++;
					break;
				}
			}
		}
		if (platformUserIdentifierAbs != null)
		{
			Players[platformUserIdentifierAbs].PlayerName.SetCollisionSuffix(0);
		}
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> player2 in Players)
		{
			if (!player2.Key.Equals(platformUserIdentifierAbs) && PlayerToEntityMap.ContainsKey(player2.Key) && player2.Value.PlayerName.AuthoredName.Text == _name)
			{
				player2.Value.PlayerName.SetCollisionSuffix(num++);
			}
		}
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> player3 in Players)
		{
			if (!player3.Key.Equals(platformUserIdentifierAbs) && !PlayerToEntityMap.ContainsKey(player3.Key) && player3.Value.PlayerName.AuthoredName.Text == _name)
			{
				player3.Value.PlayerName.SetCollisionSuffix(num++);
			}
		}
	}

	public void SetPlayerData(PersistentPlayerData ppData)
	{
		if (ppData.EntityId == -1)
		{
			UnmapPlayer(ppData.PrimaryId);
		}
		Players[ppData.PrimaryId] = ppData;
		if (ppData.LPBlocks != null)
		{
			for (int i = 0; i < ppData.LPBlocks.Count; i++)
			{
				Vector3i key = ppData.LPBlocks[i];
				m_lpBlockMap[key] = ppData;
			}
		}
		MapPlayer(ppData);
	}

	public PersistentPlayerList NetworkCloneRelevantForPlayer()
	{
		PersistentPlayerList persistentPlayerList = new PersistentPlayerList();
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> player in Players)
		{
			persistentPlayerList.Players[player.Value.PrimaryId] = player.Value;
		}
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> player2 in persistentPlayerList.Players)
		{
			if (player2.Value.LPBlocks != null)
			{
				for (int i = 0; i < player2.Value.LPBlocks.Count; i++)
				{
					Vector3i key = player2.Value.LPBlocks[i];
					persistentPlayerList.m_lpBlockMap[key] = player2.Value;
				}
			}
		}
		return persistentPlayerList;
	}

	public bool CleanupPlayers()
	{
		List<PersistentPlayerData> list = null;
		double num = (double)GameStats.GetInt(EnumGameStats.LandClaimExpiryTime) * 24.0;
		DateTime now = DateTime.Now;
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> player in Players)
		{
			if ((player.Value.ACL == null || player.Value.ACL.Count == 0) && !player.Value.HasBedrollPos && player.Value.EntityId == -1 && (now - player.Value.LastLogin).TotalHours > num)
			{
				if (list == null)
				{
					list = new List<PersistentPlayerData>();
				}
				list.Add(player.Value);
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				PersistentPlayerData persistentPlayerData = list[i];
				if (persistentPlayerData.LPBlocks != null)
				{
					for (int j = 0; j < persistentPlayerData.LPBlocks.Count; j++)
					{
						Vector3i key = persistentPlayerData.LPBlocks[j];
						m_lpBlockMap.Remove(key);
					}
				}
				Players.Remove(persistentPlayerData.PrimaryId);
			}
		}
		return list != null;
	}

	public void SpawnPointRemoved(Vector3i _pos)
	{
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> player in Players)
		{
			if (player.Value.BedrollPos.Equals(_pos))
			{
				player.Value.ClearBedroll();
				break;
			}
		}
	}

	public void Write(BinaryWriter stream)
	{
		stream.Write(Players.Count);
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> player in Players)
		{
			player.Value.Write(stream);
		}
		stream.Write(m_lpBlockMap.Count);
		foreach (KeyValuePair<Vector3i, PersistentPlayerData> item in m_lpBlockMap)
		{
			stream.Write(item.Key.x);
			stream.Write(item.Key.y);
			stream.Write(item.Key.z);
			item.Value.PrimaryId.ToStream(stream);
		}
	}

	public static PersistentPlayerList Read(BinaryReader stream)
	{
		PersistentPlayerList persistentPlayerList = new PersistentPlayerList();
		int num = stream.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			PersistentPlayerData persistentPlayerData = PersistentPlayerData.Read(stream);
			persistentPlayerList.Players.Add(persistentPlayerData.PrimaryId, persistentPlayerData);
			persistentPlayerList.MapPlayer(persistentPlayerData);
		}
		int num2 = stream.ReadInt32();
		for (int j = 0; j < num2; j++)
		{
			Vector3i key = new Vector3i(stream.ReadInt32(), stream.ReadInt32(), stream.ReadInt32());
			PlatformUserIdentifierAbs key2 = PlatformUserIdentifierAbs.FromStream(stream);
			if (persistentPlayerList.Players.TryGetValue(key2, out var value) && value != null)
			{
				persistentPlayerList.m_lpBlockMap[key] = value;
			}
		}
		return persistentPlayerList;
	}

	public static PersistentPlayerList ReadXML(string filePath)
	{
		Log.Out("Loading players.xml");
		PersistentPlayerList persistentPlayerList = new PersistentPlayerList();
		if (!SdFile.Exists(filePath))
		{
			return persistentPlayerList;
		}
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.SdLoad(filePath);
		}
		catch (XmlException ex)
		{
			Log.Error($"Failed loading players.xml: {ex.Message}");
			return persistentPlayerList;
		}
		if (xmlDocument.DocumentElement == null)
		{
			throw new Exception("malformed persistent player data xml file!");
		}
		foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
		{
			if (childNode.NodeType != XmlNodeType.Element || !(childNode.Name == "player"))
			{
				continue;
			}
			PersistentPlayerData persistentPlayerData = PersistentPlayerData.ReadXML(childNode as XmlElement);
			if (persistentPlayerData != null)
			{
				persistentPlayerList.Players.Add(persistentPlayerData.PrimaryId, persistentPlayerData);
				if (persistentPlayerData.LPBlocks != null)
				{
					for (int i = 0; i < persistentPlayerData.LPBlocks.Count; i++)
					{
						Vector3i key = persistentPlayerData.LPBlocks[i];
						persistentPlayerList.m_lpBlockMap[key] = persistentPlayerData;
					}
				}
				continue;
			}
			return null;
		}
		return persistentPlayerList;
	}

	public void Write(string filePath)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.CreateXmlDeclaration();
		XmlElement root = xmlDocument.AddXmlElement("persistentplayerdata");
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> player in Players)
		{
			player.Value.Write(root);
		}
		xmlDocument.SdSave(filePath);
	}

	public void Destroy()
	{
		PlatformUserManager.DetailsUpdated -= HandlePlayerDetailsUpdate;
	}
}
