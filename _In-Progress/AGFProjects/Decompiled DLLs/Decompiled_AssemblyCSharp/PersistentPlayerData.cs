using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class PersistentPlayerData
{
	public delegate void PlayerEventHandler(PersistentPlayerData ppData, PersistentPlayerData otherPlayer, EnumPersistentPlayerDataReason reason);

	public struct ProtectedBackpack(int entityID, Vector3i position, uint timestamp)
	{
		public readonly int EntityID = entityID;

		public readonly Vector3i Position = position;

		public readonly uint Timestamp = timestamp;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MaxTrackedBackpacks = 3;

	public PlayerData PlayerData;

	public readonly PersistentPlayerName PlayerName;

	public HashSet<PlatformUserIdentifierAbs> ACL;

	public DateTime LastLogin;

	public int EntityId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlayerEventHandler> m_dispatch;

	public List<Vector3i> LPBlocks;

	public const int cBedrollUnsetY = int.MaxValue;

	public Vector3i BedrollPos = new Vector3i(0, int.MaxValue, 0);

	public Vector3i Position;

	public List<QuestPositionData> QuestPositions = new List<QuestPositionData>();

	public bool questPositionsChanged;

	public List<Vector3i> OwnedVendingMachinePositions = new List<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, ProtectedBackpack> backpacksByID = new Dictionary<int, ProtectedBackpack>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool sortedBackpacksDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ProtectedBackpack> backpacksSortedByTimestamp = new List<ProtectedBackpack>();

	public PlatformUserIdentifierAbs PrimaryId => PlayerData.PrimaryId;

	public IPlatformUserData PlatformData => PlayerData.PlatformData;

	public PlatformUserIdentifierAbs NativeId => PlayerData.NativeId;

	public EPlayGroup PlayGroup => PlayerData.PlayGroup;

	public Vector3i MostRecentBackpackPosition
	{
		get
		{
			if (backpacksByID.Count == 0)
			{
				return Vector3i.zero;
			}
			RefreshSortedBackpacksList();
			return backpacksSortedByTimestamp[backpacksSortedByTimestamp.Count - 1].Position;
		}
	}

	public bool HasBedrollPos => BedrollPos.y != int.MaxValue;

	public double OfflineHours
	{
		get
		{
			if (EntityId != -1)
			{
				return -1.0;
			}
			return (DateTime.Now - LastLogin).TotalHours;
		}
	}

	public double OfflineMinutes
	{
		get
		{
			if (EntityId != -1)
			{
				return -1.0;
			}
			return (DateTime.Now - LastLogin).TotalMinutes;
		}
	}

	public PersistentPlayerData(PlatformUserIdentifierAbs _primaryId, PlatformUserIdentifierAbs _nativeId, AuthoredText _playerName, EPlayGroup _playGroup)
	{
		PlayerData = new PlayerData(_primaryId, _nativeId, _playerName, _playGroup);
		PlayerName = new PersistentPlayerName(_playerName);
	}

	public void Update(PlatformUserIdentifierAbs _nativeId, AuthoredText _playerName, EPlayGroup _playGroup)
	{
		PlayerData = new PlayerData(PrimaryId, _nativeId, _playerName, _playGroup);
		PlayerName.Update(_playerName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshSortedBackpacksList()
	{
		if (!sortedBackpacksDirty)
		{
			return;
		}
		backpacksSortedByTimestamp.Clear();
		foreach (KeyValuePair<int, ProtectedBackpack> item in backpacksByID)
		{
			backpacksSortedByTimestamp.Add(item.Value);
		}
		backpacksSortedByTimestamp.Sort([PublicizedFrom(EAccessModifier.Internal)] (ProtectedBackpack a, ProtectedBackpack b) =>
		{
			uint timestamp = a.Timestamp;
			return timestamp.CompareTo(b.Timestamp);
		});
		sortedBackpacksDirty = false;
	}

	public void AddDroppedBackpack(int backpackEntityId, Vector3i pos, uint timestamp)
	{
		backpacksByID[backpackEntityId] = new ProtectedBackpack(backpackEntityId, pos, timestamp);
		sortedBackpacksDirty = true;
		RefreshSortedBackpacksList();
		if (backpacksByID.Count > 3)
		{
			for (int i = 0; i < backpacksSortedByTimestamp.Count - 3; i++)
			{
				int entityID = backpacksSortedByTimestamp[i].EntityID;
				if (entityID == backpackEntityId)
				{
					Debug.LogError("AddDroppedBackpack failed: dropped backpack timestamp is older than other tracked backpacks and the tracking limit has been reached.");
				}
				TryRemoveDroppedBackpack(entityID);
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerSetBackpackPosition>().Setup(EntityId, GetDroppedBackpackPositions()), _onlyClientsAttachedToAnEntity: false, EntityId);
		}
	}

	public bool TryUpdateBackpackPosition(int entityID, Vector3i pos)
	{
		if (!backpacksByID.TryGetValue(entityID, out var value))
		{
			return false;
		}
		backpacksByID[entityID] = new ProtectedBackpack(entityID, pos, value.Timestamp);
		return true;
	}

	public bool TryRemoveDroppedBackpack(int entityID)
	{
		if (backpacksByID.Remove(entityID))
		{
			sortedBackpacksDirty = true;
			RefreshSortedBackpacksList();
			return true;
		}
		return false;
	}

	public void ProcessBackpacks(Action<ProtectedBackpack> action)
	{
		foreach (ProtectedBackpack value in backpacksByID.Values)
		{
			action(value);
		}
	}

	public void RemoveBackpacks(Predicate<ProtectedBackpack> shouldRemove)
	{
		RefreshSortedBackpacksList();
		for (int i = 0; i < backpacksSortedByTimestamp.Count; i++)
		{
			if (shouldRemove(backpacksSortedByTimestamp[i]))
			{
				TryRemoveDroppedBackpack(backpacksSortedByTimestamp[i].EntityID);
			}
		}
	}

	public void ClearDroppedBackpacks()
	{
		backpacksByID.Clear();
		backpacksSortedByTimestamp.Clear();
		sortedBackpacksDirty = true;
	}

	public List<Vector3i> GetDroppedBackpackPositions()
	{
		List<Vector3i> list = new List<Vector3i>();
		foreach (ProtectedBackpack item in backpacksSortedByTimestamp)
		{
			list.Add(item.Position);
		}
		return list;
	}

	public void AddVendingMachinePosition(Vector3i pos)
	{
		if (!OwnedVendingMachinePositions.Contains(pos))
		{
			OwnedVendingMachinePositions.Add(pos);
		}
	}

	public bool TryRemoveVendingMachinePosition(Vector3i pos)
	{
		return OwnedVendingMachinePositions.Remove(pos);
	}

	public void AddPlayerEventHandler(PlayerEventHandler handler)
	{
		if (m_dispatch == null)
		{
			m_dispatch = new List<PlayerEventHandler>();
		}
		m_dispatch.Add(handler);
	}

	public void RemovePlayerEventHandler(PlayerEventHandler handler)
	{
		m_dispatch.Remove(handler);
		if (m_dispatch.Count == 0)
		{
			m_dispatch = null;
		}
	}

	public void Dispatch(PersistentPlayerData otherPlayer, EnumPersistentPlayerDataReason reason)
	{
		if (m_dispatch != null)
		{
			for (int i = 0; i < m_dispatch.Count; i++)
			{
				m_dispatch[i](this, otherPlayer, reason);
			}
		}
	}

	public void AddPlayerToACL(PlatformUserIdentifierAbs _userIdentifier)
	{
		if (ACL == null)
		{
			ACL = new HashSet<PlatformUserIdentifierAbs>();
		}
		ACL.Add(_userIdentifier);
	}

	public void RemovePlayerFromACL(PlatformUserIdentifierAbs _userIdentifier)
	{
		if (ACL != null)
		{
			ACL.Remove(_userIdentifier);
			if (ACL.Count == 0)
			{
				ACL = null;
			}
		}
	}

	public void AddLandProtectionBlock(Vector3i pos)
	{
		if (LPBlocks == null)
		{
			LPBlocks = new List<Vector3i>();
		}
		LPBlocks.Add(pos);
	}

	public List<Vector3i> GetLandProtectionBlocks()
	{
		if (LPBlocks == null)
		{
			LPBlocks = new List<Vector3i>();
		}
		return LPBlocks;
	}

	public bool GetLandProtectionBlock(out Vector3i _blockPos)
	{
		_blockPos = Vector3i.zero;
		if (LPBlocks != null && LPBlocks.Count > 0)
		{
			_blockPos = LPBlocks[0];
			return true;
		}
		return false;
	}

	public void RemoveLandProtectionBlock(Vector3i pos)
	{
		LPBlocks.Remove(pos);
	}

	public void ClearBedroll()
	{
		Entity entity = GameManager.Instance.World.GetEntity(EntityId);
		if ((bool)entity)
		{
			NavObjectManager.Instance.UnRegisterNavObjectByOwnerEntity(entity, "sleeping_bag");
			BedrollPos.y = int.MaxValue;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.SleepingBag, EntityId));
			}
		}
	}

	public void ShowBedrollOnMap()
	{
		if (GameManager.Instance.IsEditMode() || BedrollPos.y == int.MaxValue)
		{
			return;
		}
		Entity entity = GameManager.Instance.World.GetEntity(EntityId);
		if ((bool)entity)
		{
			NavObject navObject = NavObjectManager.Instance.RegisterNavObject("sleeping_bag", BedrollPos.ToVector3());
			if (navObject != null)
			{
				navObject.OwnerEntity = entity;
			}
		}
	}

	public void AddQuestPosition(int questCode, Quest.PositionDataTypes positionDataType, Vector3 position)
	{
		Vector3i blockPosition = World.worldToBlockPos(position);
		foreach (QuestPositionData questPosition in QuestPositions)
		{
			if (questPosition.questCode == questCode && questPosition.positionDataType == positionDataType)
			{
				questPosition.blockPosition = blockPosition;
				return;
			}
		}
		if (positionDataType != Quest.PositionDataTypes.TreasureOffset && positionDataType != Quest.PositionDataTypes.POISize && positionDataType != Quest.PositionDataTypes.TraderPosition)
		{
			QuestPositions.Add(new QuestPositionData(questCode, positionDataType, blockPosition));
			questPositionsChanged = true;
		}
	}

	public void RemovePositionsForQuest(int questCode)
	{
		List<QuestPositionData> list = new List<QuestPositionData>();
		foreach (QuestPositionData questPosition in QuestPositions)
		{
			if (questPosition.questCode == questCode)
			{
				list.Add(questPosition);
			}
		}
		foreach (QuestPositionData item in list)
		{
			QuestPositions.Remove(item);
		}
		questPositionsChanged = true;
	}

	public void UpdatePositionFromEntity()
	{
		if (EntityId != -1)
		{
			Entity entity = GameManager.Instance.World.GetEntity(EntityId);
			if ((bool)entity)
			{
				Position = new Vector3i(entity.position);
			}
		}
	}

	public void Write(BinaryWriter stream)
	{
		UpdatePositionFromEntity();
		PrimaryId.ToStream(stream, _inclCustomData: true);
		NativeId.ToStream(stream, _inclCustomData: true);
		stream.Write((byte)PlayGroup);
		AuthoredText.ToStream(PlayerName.AuthoredName, stream);
		stream.Write(LastLogin.Ticks);
		stream.Write(Position.x);
		stream.Write(Position.y);
		stream.Write(Position.z);
		stream.Write(EntityId);
		stream.Write(ACL?.Count ?? 0);
		stream.Write(LPBlocks?.Count ?? 0);
		stream.Write(backpacksByID.Count);
		if (ACL != null)
		{
			foreach (PlatformUserIdentifierAbs item in ACL)
			{
				item.ToStream(stream);
			}
		}
		if (LPBlocks != null)
		{
			for (int i = 0; i < LPBlocks.Count; i++)
			{
				Vector3i vector3i = LPBlocks[i];
				stream.Write(vector3i.x);
				stream.Write(vector3i.y);
				stream.Write(vector3i.z);
			}
		}
		foreach (KeyValuePair<int, ProtectedBackpack> item2 in backpacksByID)
		{
			stream.Write(item2.Key);
			Vector3i position = item2.Value.Position;
			stream.Write(position.x);
			stream.Write(position.y);
			stream.Write(position.z);
			stream.Write(item2.Value.Timestamp);
		}
		stream.Write(BedrollPos.x);
		stream.Write(BedrollPos.y);
		stream.Write(BedrollPos.z);
		stream.Write(QuestPositions.Count);
		foreach (QuestPositionData questPosition in QuestPositions)
		{
			questPosition.Write(stream);
		}
		stream.Write(OwnedVendingMachinePositions.Count);
		foreach (Vector3i ownedVendingMachinePosition in OwnedVendingMachinePositions)
		{
			stream.Write(ownedVendingMachinePosition.x);
			stream.Write(ownedVendingMachinePosition.y);
			stream.Write(ownedVendingMachinePosition.z);
		}
	}

	public static PersistentPlayerData Read(BinaryReader stream)
	{
		PersistentPlayerData persistentPlayerData = new PersistentPlayerData(PlatformUserIdentifierAbs.FromStream(stream, _errorOnEmpty: false, _inclCustomData: true), PlatformUserIdentifierAbs.FromStream(stream, _errorOnEmpty: false, _inclCustomData: true), _playGroup: (EPlayGroup)stream.ReadByte(), _playerName: AuthoredText.FromStream(stream));
		persistentPlayerData.LastLogin = new DateTime(stream.ReadInt64());
		persistentPlayerData.Position.x = stream.ReadInt32();
		persistentPlayerData.Position.y = stream.ReadInt32();
		persistentPlayerData.Position.z = stream.ReadInt32();
		persistentPlayerData.EntityId = stream.ReadInt32();
		int num = stream.ReadInt32();
		int num2 = stream.ReadInt32();
		int num3 = stream.ReadInt32();
		if (num > 0)
		{
			persistentPlayerData.ACL = new HashSet<PlatformUserIdentifierAbs>();
			for (int i = 0; i < num; i++)
			{
				PlatformUserIdentifierAbs item = PlatformUserIdentifierAbs.FromStream(stream);
				persistentPlayerData.ACL.Add(item);
			}
		}
		if (num2 > 0)
		{
			persistentPlayerData.LPBlocks = new List<Vector3i>();
			for (int j = 0; j < num2; j++)
			{
				persistentPlayerData.LPBlocks.Add(new Vector3i(stream.ReadInt32(), stream.ReadInt32(), stream.ReadInt32()));
			}
		}
		for (int k = 0; k < num3; k++)
		{
			int backpackEntityId = stream.ReadInt32();
			Vector3i pos = new Vector3i(stream.ReadInt32(), stream.ReadInt32(), stream.ReadInt32());
			uint timestamp = stream.ReadUInt32();
			persistentPlayerData.AddDroppedBackpack(backpackEntityId, pos, timestamp);
		}
		persistentPlayerData.BedrollPos.x = stream.ReadInt32();
		persistentPlayerData.BedrollPos.y = stream.ReadInt32();
		persistentPlayerData.BedrollPos.z = stream.ReadInt32();
		int num4 = stream.ReadInt32();
		persistentPlayerData.QuestPositions = new List<QuestPositionData>();
		for (int l = 0; l < num4; l++)
		{
			persistentPlayerData.QuestPositions.Add(QuestPositionData.Read(stream));
		}
		int num5 = stream.ReadInt32();
		persistentPlayerData.OwnedVendingMachinePositions = new List<Vector3i>();
		for (int m = 0; m < num5; m++)
		{
			persistentPlayerData.OwnedVendingMachinePositions.Add(new Vector3i(stream.ReadInt32(), stream.ReadInt32(), stream.ReadInt32()));
		}
		return persistentPlayerData;
	}

	public void Write(XmlElement _root)
	{
		XmlElement xmlElement = _root.AddXmlElement("player");
		PrimaryId.ToXml(xmlElement);
		NativeId?.ToXml(xmlElement, "native");
		xmlElement.SetAttrib("playername", PlayerName.AuthoredName.Text);
		xmlElement.SetAttrib("playgroup", PlayGroup.ToStringCached());
		xmlElement.SetAttrib("lastlogin", LastLogin.ToCultureInvariantString());
		xmlElement.SetAttrib("position", $"{Position.x},{Position.y},{Position.z}");
		if ((ACL == null || ACL.Count == 0) && (LPBlocks == null || LPBlocks.Count == 0) && backpacksByID.Count == 0 && BedrollPos.y == int.MaxValue && QuestPositions.Count == 0 && OwnedVendingMachinePositions.Count == 0)
		{
			return;
		}
		if (ACL != null)
		{
			foreach (PlatformUserIdentifierAbs item in ACL)
			{
				XmlElement xmlElement2 = xmlElement.AddXmlElement("acl");
				item.ToXml(xmlElement2);
			}
		}
		if (LPBlocks != null)
		{
			for (int i = 0; i < LPBlocks.Count; i++)
			{
				Vector3i vector3i = LPBlocks[i];
				xmlElement.AddXmlElement("lpblock").SetAttrib("pos", $"{vector3i.x},{vector3i.y},{vector3i.z}");
			}
		}
		foreach (KeyValuePair<int, ProtectedBackpack> item2 in backpacksByID)
		{
			int key = item2.Key;
			Vector3i position = item2.Value.Position;
			uint timestamp = item2.Value.Timestamp;
			XmlElement element = xmlElement.AddXmlElement("backpack");
			element.SetAttrib("id", $"{key}");
			element.SetAttrib("pos", $"{position.x},{position.y},{position.z}");
			element.SetAttrib("timestamp", $"{timestamp}");
		}
		if (BedrollPos.y != int.MaxValue)
		{
			xmlElement.AddXmlElement("bedroll").SetAttrib("pos", $"{BedrollPos.x},{BedrollPos.y},{BedrollPos.z}");
		}
		if (QuestPositions != null && QuestPositions.Count > 0)
		{
			XmlElement node = xmlElement.AddXmlElement("questpositions");
			foreach (QuestPositionData questPosition in QuestPositions)
			{
				XmlElement xmlElement3 = node.AddXmlElement("position");
				xmlElement3.SetAttribute("id", questPosition.questCode.ToString());
				int positionDataType = (int)questPosition.positionDataType;
				xmlElement3.SetAttribute("positiondatatype", positionDataType.ToString());
				xmlElement3.SetAttrib("pos", $"{questPosition.blockPosition.x},{questPosition.blockPosition.y},{questPosition.blockPosition.z}");
			}
		}
		if (OwnedVendingMachinePositions == null || OwnedVendingMachinePositions.Count <= 0)
		{
			return;
		}
		XmlElement node2 = xmlElement.AddXmlElement("vendingmachinepositions");
		foreach (Vector3i ownedVendingMachinePosition in OwnedVendingMachinePositions)
		{
			node2.AddXmlElement("position").SetAttrib("pos", $"{ownedVendingMachinePosition.x},{ownedVendingMachinePosition.y},{ownedVendingMachinePosition.z}");
		}
	}

	public static PersistentPlayerData ReadXML(XmlElement root)
	{
		PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromXml(root);
		if (platformUserIdentifierAbs == null)
		{
			Log.Error("player-entry has missing or invalid user-identifier attributes: " + root.OuterXml);
			Application.Quit();
			return null;
		}
		if (root.HasAttribute("playername"))
		{
			AuthoredText playerName = new AuthoredText(root.GetAttribute("playername"), platformUserIdentifierAbs);
			EPlayGroup result;
			if (!root.TryGetAttribute("playgroup", out var _result))
			{
				result = EPlayGroup.Unknown;
			}
			else if (!Enum.TryParse<EPlayGroup>(_result, out result))
			{
				Log.Error("player-entry has missing or malformed 'playgroup' attribute: " + root.OuterXml);
				Application.Quit();
				return null;
			}
			PlatformUserIdentifierAbs nativeId = PlatformUserIdentifierAbs.FromXml(root, _warnings: true, "native");
			PersistentPlayerData persistentPlayerData = new PersistentPlayerData(platformUserIdentifierAbs, nativeId, playerName, result);
			if (!root.HasAttribute("lastlogin"))
			{
				Log.Error("player-entry is missing 'lastlogin' attribute: " + root.OuterXml);
				Application.Quit();
				return null;
			}
			if (!StringParsers.TryParseDateTime(root.GetAttribute("lastlogin"), out persistentPlayerData.LastLogin) && !DateTime.TryParse(root.GetAttribute("lastlogin"), out persistentPlayerData.LastLogin))
			{
				Log.Error("player-entry has malfored 'lastlogin' attribute: " + root.OuterXml);
				Application.Quit();
				return null;
			}
			if (root.HasAttribute("position"))
			{
				string[] array = root.GetAttribute("position").Split(',');
				if (array.Length < 3)
				{
					Log.Error("player-entry has invalid 'position' attribute: " + root.OuterXml);
					Application.Quit();
					return null;
				}
				persistentPlayerData.Position.x = int.Parse(array[0].Trim());
				persistentPlayerData.Position.y = int.Parse(array[1].Trim());
				persistentPlayerData.Position.z = int.Parse(array[2].Trim());
				{
					foreach (XmlNode childNode in root.ChildNodes)
					{
						if (childNode.NodeType != XmlNodeType.Element)
						{
							continue;
						}
						XmlElement xmlElement = (XmlElement)childNode;
						if (childNode.Name == "acl")
						{
							if (persistentPlayerData.ACL == null)
							{
								persistentPlayerData.ACL = new HashSet<PlatformUserIdentifierAbs>();
							}
							PlatformUserIdentifierAbs platformUserIdentifierAbs2 = PlatformUserIdentifierAbs.FromXml(xmlElement);
							if (platformUserIdentifierAbs2 == null)
							{
								Log.Warning("Ignoring malformed acl-entry: " + childNode.OuterXml);
							}
							else
							{
								persistentPlayerData.ACL.Add(platformUserIdentifierAbs2);
							}
						}
						else if (childNode.Name == "lpblock")
						{
							if (!xmlElement.HasAttribute("pos"))
							{
								Log.Warning("Ignoring lpblock-entry because of missing 'pos' attribute: " + childNode.OuterXml);
								continue;
							}
							string[] array2 = xmlElement.GetAttribute("pos").Split(',');
							if (array2.Length < 3)
							{
								Log.Warning("Ignoring lpblock-entry because of malformed 'pos' attribute: " + childNode.OuterXml);
								continue;
							}
							Vector3i item = new Vector3i
							{
								x = int.Parse(array2[0].Trim()),
								y = int.Parse(array2[1].Trim()),
								z = int.Parse(array2[2].Trim())
							};
							if (persistentPlayerData.LPBlocks == null)
							{
								persistentPlayerData.LPBlocks = new List<Vector3i>();
							}
							persistentPlayerData.LPBlocks.Add(item);
						}
						else if (childNode.Name == "backpack")
						{
							if (!xmlElement.HasAttribute("pos"))
							{
								Log.Warning("Ignoring backpack-entry because of missing 'pos' attribute: " + childNode.OuterXml);
								continue;
							}
							if (!xmlElement.HasAttribute("id"))
							{
								Log.Warning("Ignoring backpack-entry because of missing 'id' attribute: " + childNode.OuterXml);
								continue;
							}
							if (!xmlElement.HasAttribute("timestamp"))
							{
								Log.Warning("Ignoring backpack-entry because of missing 'timestamp' attribute: " + childNode.OuterXml);
								continue;
							}
							string[] array3 = xmlElement.GetAttribute("pos").Split(',');
							if (array3.Length < 3)
							{
								Log.Warning("Ignoring backpack-entry because of malformed 'pos' attribute: " + childNode.OuterXml);
								continue;
							}
							int backpackEntityId = int.Parse(xmlElement.GetAttribute("id"));
							Vector3i pos = new Vector3i
							{
								x = int.Parse(array3[0].Trim()),
								y = int.Parse(array3[1].Trim()),
								z = int.Parse(array3[2].Trim())
							};
							uint timestamp = uint.Parse(xmlElement.GetAttribute("timestamp"));
							persistentPlayerData.AddDroppedBackpack(backpackEntityId, pos, timestamp);
						}
						else if (childNode.Name == "bedroll")
						{
							string[] array4 = xmlElement.GetAttribute("pos").Split(',');
							if (array4.Length < 3)
							{
								Log.Warning("Ignoring bedroll-entry. Invalid 'pos' attribute: " + childNode.OuterXml);
								continue;
							}
							persistentPlayerData.BedrollPos.x = int.Parse(array4[0].Trim());
							persistentPlayerData.BedrollPos.y = int.Parse(array4[1].Trim());
							persistentPlayerData.BedrollPos.z = int.Parse(array4[2].Trim());
						}
						else if (childNode.Name == "questpositions")
						{
							persistentPlayerData.QuestPositions = new List<QuestPositionData>();
							foreach (XmlElement childNode2 in childNode.ChildNodes)
							{
								if (childNode2.Name == "position")
								{
									int questCode = int.Parse(childNode2.GetAttribute("id"));
									Vector3i blockPosition = default(Vector3i);
									int positionDataType = int.Parse(childNode2.GetAttribute("positiondatatype"));
									string[] array5 = childNode2.GetAttribute("pos").Split(',');
									if (array5.Length < 3)
									{
										Log.Warning("Ignoring bedroll-entry. Invalid 'pos' attribute: " + childNode.OuterXml);
										continue;
									}
									blockPosition.x = int.Parse(array5[0].Trim());
									blockPosition.y = int.Parse(array5[1].Trim());
									blockPosition.z = int.Parse(array5[2].Trim());
									persistentPlayerData.QuestPositions.Add(new QuestPositionData(questCode, (Quest.PositionDataTypes)positionDataType, blockPosition));
								}
							}
						}
						else
						{
							if (!(childNode.Name == "vendingmachinepositions"))
							{
								continue;
							}
							persistentPlayerData.OwnedVendingMachinePositions = new List<Vector3i>();
							foreach (XmlElement childNode3 in childNode.ChildNodes)
							{
								if (childNode3.Name == "position")
								{
									Vector3i item2 = default(Vector3i);
									string[] array6 = childNode3.GetAttribute("pos").Split(',');
									item2.x = int.Parse(array6[0].Trim());
									item2.y = int.Parse(array6[1].Trim());
									item2.z = int.Parse(array6[2].Trim());
									persistentPlayerData.OwnedVendingMachinePositions.Add(item2);
								}
							}
						}
					}
					return persistentPlayerData;
				}
			}
			Log.Error("player-entry is missing 'position' attribute: " + root.OuterXml);
			Application.Quit();
			return null;
		}
		Log.Error("player-entry is missing 'playername' attribute: " + root.OuterXml);
		return null;
	}
}
