using System;
using System.Collections.Generic;
using System.IO;

public class PlayerDataFile
{
	public static string EXT = "ttp";

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cFileVersion = 57;

	public bool bLoaded;

	public bool bModifiedSinceLastSave;

	public EntityCreationData ecd = new EntityCreationData();

	public ItemStack[] inventory = new ItemStack[0];

	public ItemStack[] bag = new ItemStack[0];

	public PackedBoolArray bagLockedSlots;

	public ItemStack dragAndDropItem = new ItemStack();

	public Equipment equipment = new Equipment();

	public int selectedInventorySlot;

	public List<Vector3i> spawnPoints = new List<Vector3i>();

	public long selectedSpawnPointKey;

	public HashSet<string> alreadyCraftedList = new HashSet<string>();

	public List<string> unlockedRecipeList = new List<string>();

	public List<string> favoriteRecipeList = new List<string>();

	public SpawnPosition lastSpawnPosition = SpawnPosition.Undef;

	public List<OwnedEntityData> ownedEntities = new List<OwnedEntityData>();

	public int playerKills;

	public int zombieKills;

	public int deaths;

	public int score;

	public int id = -1;

	public Vector3i markerPosition;

	public bool markerHidden;

	public bool bCrouchedLocked;

	public CraftingData craftingData = new CraftingData();

	public int deathUpdateTime;

	public bool bDead;

	public float distanceWalked;

	public uint totalItemsCrafted;

	public float longestLife;

	public float currentLife;

	public float totalTimePlayed;

	public ulong gameStageBornAtWorldTime = ulong.MaxValue;

	public MemoryStream progressionData = new MemoryStream(0);

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream buffData = new MemoryStream(0);

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream stealthData = new MemoryStream(0);

	public WaypointCollection waypoints = new WaypointCollection();

	public QuestJournal questJournal = new QuestJournal();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bModdedSaveGame;

	public ChallengeJournal challengeJournal = new ChallengeJournal();

	public Vector3i rentedVMPosition = Vector3i.zero;

	public ulong rentalEndTime;

	public int rentalEndDay;

	public List<ushort> favoriteCreativeStacks = new List<ushort>();

	public List<string> favoriteShapes = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerMetaInfo metadata;

	public void ToPlayer(EntityPlayer _player)
	{
		if (id != -1)
		{
			_player.entityId = id;
		}
		if (ecd.stats != null)
		{
			_player.SetStats(ecd.stats);
		}
		_player.position = ecd.pos;
		_player.rotation = ecd.rot;
		_player.inventory.SetSlots(inventory);
		_player.inventory.SetFocusedItemIdx(selectedInventorySlot);
		_player.inventory.SetHoldingItemIdx(selectedInventorySlot);
		_player.bag.SetSlots(bag);
		_player.bag.LockedSlots = bagLockedSlots?.Clone();
		if (spawnPoints.Count > 0)
		{
			_player.SpawnPoints.Set(spawnPoints[0]);
		}
		_player.onGround = ecd.onGround;
		_player.selectedSpawnPointKey = selectedSpawnPointKey;
		_player.lastSpawnPosition = lastSpawnPosition;
		_player.belongsPlayerId = id;
		_player.KilledPlayers = playerKills;
		_player.KilledZombies = zombieKills;
		_player.Died = deaths;
		_player.Score = score;
		_player.equipment.Apply(equipment);
		if (_player == GameManager.Instance.World.GetPrimaryPlayer())
		{
			_player.TurnOffLightFlares();
		}
		_player.navMarkerHidden = markerHidden;
		_player.markerPosition = markerPosition;
		_player.CrouchingLocked = bCrouchedLocked;
		_player.deathUpdateTime = deathUpdateTime;
		if (bDead)
		{
			_player.SetDead();
		}
		if (_player is EntityPlayerLocal entityPlayerLocal)
		{
			CraftingManager.AlreadyCraftedList = alreadyCraftedList;
			for (int i = 0; i < unlockedRecipeList.Count; i++)
			{
				CraftingManager.UnlockedRecipeList.Add(unlockedRecipeList[i]);
			}
			for (int j = 0; j < favoriteRecipeList.Count; j++)
			{
				CraftingManager.FavoriteRecipeList.Add(favoriteRecipeList[j]);
			}
			entityPlayerLocal.DragAndDropItem = dragAndDropItem;
		}
		if (progressionData.Length > 0)
		{
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader.SetBaseStream(progressionData);
			_player.Progression = Progression.Read(pooledBinaryReader, _player);
		}
		if (buffData.Length > 0)
		{
			if (_player.Buffs == null)
			{
				_player.Buffs = new EntityBuffs(_player);
			}
			using PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader2.SetBaseStream(buffData);
			_player.Buffs.Read(pooledBinaryReader2);
		}
		if (stealthData.Length > 0)
		{
			using PooledBinaryReader pooledBinaryReader3 = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader3.SetBaseStream(stealthData);
			_player.Stealth = PlayerStealth.Read(_player, pooledBinaryReader3);
		}
		if (ownedEntities.Count > 0)
		{
			for (int k = 0; k < ownedEntities.Count; k++)
			{
				_player.AddOwnedEntity(ownedEntities[k]);
			}
		}
		_player.totalItemsCrafted = totalItemsCrafted;
		_player.distanceWalked = distanceWalked;
		_player.longestLife = longestLife;
		_player.currentLife = currentLife;
		_player.totalTimePlayed = totalTimePlayed;
		ulong worldTime = _player.world.worldTime;
		if (worldTime != 0L && gameStageBornAtWorldTime > worldTime)
		{
			gameStageBornAtWorldTime = worldTime;
		}
		_player.gameStageBornAtWorldTime = gameStageBornAtWorldTime;
		_player.Waypoints = waypoints;
		_player.QuestJournal = questJournal;
		_player.QuestJournal.OwnerPlayer = _player as EntityPlayerLocal;
		_player.challengeJournal = challengeJournal;
		_player.challengeJournal.Player = _player as EntityPlayerLocal;
		_player.RentedVMPosition = rentedVMPosition;
		_player.RentalEndTime = rentalEndTime;
		_player.RentalEndDay = rentalEndDay;
		if (_player is EntityPlayerLocal)
		{
			for (int l = 0; l < _player.Waypoints.Collection.list.Count; l++)
			{
				Waypoint waypoint = _player.Waypoints.Collection.list[l];
				waypoint.navObject = NavObjectManager.Instance.RegisterNavObject("waypoint", waypoint.pos.ToVector3(), waypoint.icon, waypoint.hiddenOnCompass);
				waypoint.navObject.IsActive = waypoint.bTracked;
				waypoint.navObject.name = waypoint.name.Text;
				waypoint.navObject.usingLocalizationId = waypoint.bUsingLocalizationId;
				waypoint.navObject.hiddenOnMap = waypoint.HiddenOnMap;
			}
		}
		_player.favoriteCreativeStacks = favoriteCreativeStacks;
		_player.favoriteShapes = favoriteShapes;
	}

	public void FromPlayer(EntityPlayer _player)
	{
		ecd = new EntityCreationData(_player);
		inventory = ((_player.AttachedToEntity != null && _player.saveInventory != null) ? _player.saveInventory.CloneItemStack() : _player.inventory.CloneItemStack());
		bag = _player.bag.GetSlots();
		bagLockedSlots = _player.bag.LockedSlots?.Clone();
		equipment = _player.equipment.Clone();
		selectedInventorySlot = _player.inventory.holdingItemIdx;
		spawnPoints = new List<Vector3i>(new Vector3i[0]);
		selectedSpawnPointKey = _player.selectedSpawnPointKey;
		lastSpawnPosition = _player.lastSpawnPosition;
		playerKills = _player.KilledPlayers;
		zombieKills = _player.KilledZombies;
		deaths = _player.Died;
		score = _player.Score;
		deathUpdateTime = _player.deathUpdateTime;
		bDead = _player.IsDead();
		id = _player.entityId;
		markerPosition = _player.markerPosition;
		markerHidden = _player.navMarkerHidden;
		bCrouchedLocked = _player.CrouchingLocked;
		if (_player is EntityPlayerLocal entityPlayerLocal)
		{
			alreadyCraftedList = CraftingManager.AlreadyCraftedList;
			unlockedRecipeList.AddRange(CraftingManager.UnlockedRecipeList);
			favoriteRecipeList.AddRange(CraftingManager.FavoriteRecipeList);
			dragAndDropItem = entityPlayerLocal.DragAndDropItem;
		}
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_player as EntityPlayerLocal);
		if (_player is EntityPlayerLocal && uIForPlayer.xui != null && uIForPlayer.xui.isReady)
		{
			craftingData = uIForPlayer.xui.GetCraftingData();
		}
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
		{
			pooledBinaryWriter.SetBaseStream(progressionData);
			_player.Progression.Write(pooledBinaryWriter);
		}
		using (PooledBinaryWriter pooledBinaryWriter2 = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
		{
			pooledBinaryWriter2.SetBaseStream(buffData);
			_player.Buffs.Write(pooledBinaryWriter2);
		}
		using (PooledBinaryWriter pooledBinaryWriter3 = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
		{
			pooledBinaryWriter3.SetBaseStream(stealthData);
			_player.Stealth.Write(pooledBinaryWriter3);
		}
		ownedEntities = new List<OwnedEntityData>(_player.GetOwnedEntities());
		totalItemsCrafted = _player.totalItemsCrafted;
		distanceWalked = _player.distanceWalked;
		longestLife = _player.longestLife;
		currentLife = _player.currentLife;
		totalTimePlayed = _player.totalTimePlayed;
		if (_player.gameStageBornAtWorldTime > _player.world.worldTime)
		{
			_player.gameStageBornAtWorldTime = _player.world.worldTime;
		}
		gameStageBornAtWorldTime = _player.gameStageBornAtWorldTime;
		waypoints = _player.Waypoints.Clone();
		questJournal.ClearSharedQuestMarkers();
		questJournal = _player.QuestJournal.Clone();
		challengeJournal = _player.challengeJournal.Clone();
		rentedVMPosition = _player.RentedVMPosition;
		rentalEndTime = _player.RentalEndTime;
		rentalEndDay = _player.RentalEndDay;
		favoriteCreativeStacks = new List<ushort>(_player.favoriteCreativeStacks);
		favoriteShapes = new List<string>(_player.favoriteShapes);
		metadata = new PlayerMetaInfo((GameManager.Instance.persistentPlayers?.GetPlayerDataFromEntityID(_player.entityId))?.NativeId, _player.EntityName, _player.Progression.Level, _player.distanceWalked);
		bLoaded = true;
	}

	public void ToggleWaypointHiddenStatus(NavObject nav)
	{
		Waypoint waypointForNavObject = waypoints.GetWaypointForNavObject(nav);
		if (waypointForNavObject != null)
		{
			waypointForNavObject.hiddenOnCompass = nav.hiddenOnCompass;
		}
	}

	public void Load(string _dir, string _playerName)
	{
		try
		{
			string path = _dir + "/" + _playerName + "." + EXT;
			if (!SdFile.Exists(path))
			{
				return;
			}
			using Stream baseStream = SdFile.OpenRead(path);
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader.SetBaseStream(baseStream);
			if (pooledBinaryReader.ReadChar() == 't' && pooledBinaryReader.ReadChar() == 't' && pooledBinaryReader.ReadChar() == 'p' && pooledBinaryReader.ReadChar() == '\0')
			{
				uint version = pooledBinaryReader.ReadByte();
				Read(pooledBinaryReader, version);
				bLoaded = true;
			}
		}
		catch (Exception ex)
		{
			try
			{
				Log.Error("Loading player data failed for player '" + _playerName + "', rolling back: " + ex.Message + "\n" + ex.StackTrace);
				string path2 = _dir + "/" + _playerName + "." + EXT + ".bak";
				if (!SdFile.Exists(path2))
				{
					return;
				}
				using Stream baseStream2 = SdFile.OpenRead(path2);
				using PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
				pooledBinaryReader2.SetBaseStream(baseStream2);
				if (pooledBinaryReader2.ReadChar() == 't' && pooledBinaryReader2.ReadChar() == 't' && pooledBinaryReader2.ReadChar() == 'p' && pooledBinaryReader2.ReadChar() == '\0')
				{
					uint version2 = pooledBinaryReader2.ReadByte();
					Read(pooledBinaryReader2, version2);
					bLoaded = true;
				}
			}
			catch (Exception ex2)
			{
				Log.Error("Loading backup player data failed for player '" + _playerName + "', rolling back: " + ex2.Message + "\n" + ex2.StackTrace);
			}
		}
	}

	public void Read(PooledBinaryReader _br, uint _version)
	{
		if (_version <= 37)
		{
			return;
		}
		ecd = new EntityCreationData();
		ecd.read(_br, _bNetworkRead: false);
		if (_version < 10)
		{
			inventory = GameUtils.ReadItemStackOld(_br);
		}
		else
		{
			inventory = GameUtils.ReadItemStack(_br);
		}
		selectedInventorySlot = _br.ReadByte();
		bag = GameUtils.ReadItemStack(_br);
		switch (_version)
		{
		default:
			if (_br.ReadBoolean())
			{
				bagLockedSlots = new PackedBoolArray();
				bagLockedSlots.Read(_br);
			}
			else
			{
				bagLockedSlots = null;
			}
			break;
		case 55u:
		case 56u:
		{
			ushort num2 = _br.ReadUInt16();
			if (num2 == 0)
			{
				bagLockedSlots = null;
				break;
			}
			bagLockedSlots = new PackedBoolArray(num2);
			for (int j = 0; j < num2; j++)
			{
				bagLockedSlots[j] = _br.ReadBoolean();
			}
			break;
		}
		case 53u:
		case 54u:
		{
			int num = _br.ReadInt32();
			bagLockedSlots = new PackedBoolArray(num);
			for (int i = 0; i < num; i++)
			{
				bagLockedSlots[i] = true;
			}
			break;
		}
		case 0u:
		case 1u:
		case 2u:
		case 3u:
		case 4u:
		case 5u:
		case 6u:
		case 7u:
		case 8u:
		case 9u:
		case 10u:
		case 11u:
		case 12u:
		case 13u:
		case 14u:
		case 15u:
		case 16u:
		case 17u:
		case 18u:
		case 19u:
		case 20u:
		case 21u:
		case 22u:
		case 23u:
		case 24u:
		case 25u:
		case 26u:
		case 27u:
		case 28u:
		case 29u:
		case 30u:
		case 31u:
		case 32u:
		case 33u:
		case 34u:
		case 35u:
		case 36u:
		case 37u:
		case 38u:
		case 39u:
		case 40u:
		case 41u:
		case 42u:
		case 43u:
		case 44u:
		case 45u:
		case 46u:
		case 47u:
		case 48u:
		case 49u:
		case 50u:
		case 51u:
		case 52u:
			break;
		}
		if (_version >= 52)
		{
			ItemStack[] array = GameUtils.ReadItemStack(_br);
			if (array != null && array.Length != 0)
			{
				dragAndDropItem = array[0];
			}
		}
		alreadyCraftedList = new HashSet<string>();
		int num3 = _br.ReadUInt16();
		for (int k = 0; k < num3; k++)
		{
			alreadyCraftedList.Add(_br.ReadString());
		}
		byte b = _br.ReadByte();
		for (int l = 0; l < b; l++)
		{
			spawnPoints.Add(new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32()));
		}
		selectedSpawnPointKey = _br.ReadInt64();
		_br.ReadBoolean();
		_br.ReadInt16();
		bLoaded = _br.ReadBoolean();
		lastSpawnPosition = new SpawnPosition(new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32()), _br.ReadSingle());
		id = _br.ReadInt32();
		if (_version < 49)
		{
			_br.ReadInt32();
			_br.ReadInt32();
			_br.ReadInt32();
		}
		playerKills = _br.ReadInt32();
		zombieKills = _br.ReadInt32();
		deaths = _br.ReadInt32();
		score = _br.ReadInt32();
		equipment = Equipment.Read(_br);
		unlockedRecipeList = new List<string>();
		num3 = _br.ReadUInt16();
		for (int m = 0; m < num3; m++)
		{
			unlockedRecipeList.Add(_br.ReadString());
		}
		_br.ReadUInt16();
		markerPosition = StreamUtils.ReadVector3i(_br);
		if (_version > 49)
		{
			markerHidden = _br.ReadBoolean();
		}
		if (_version < 54)
		{
			Equipment.Read(_br);
		}
		bCrouchedLocked = _br.ReadBoolean();
		craftingData.Read(_br, _version);
		favoriteRecipeList = new List<string>();
		num3 = _br.ReadUInt16();
		for (int n = 0; n < num3; n++)
		{
			favoriteRecipeList.Add(_br.ReadString());
		}
		totalItemsCrafted = _br.ReadUInt32();
		distanceWalked = _br.ReadSingle();
		longestLife = _br.ReadSingle();
		gameStageBornAtWorldTime = _br.ReadUInt64();
		waypoints = new WaypointCollection();
		waypoints.Read(_br);
		questJournal = new QuestJournal();
		questJournal.Read(_br);
		deathUpdateTime = _br.ReadInt32();
		currentLife = _br.ReadSingle();
		bDead = _br.ReadBoolean();
		_br.ReadByte();
		bModdedSaveGame = _br.ReadBoolean();
		if (bModdedSaveGame)
		{
			Log.Out("Modded save game");
		}
		challengeJournal = new ChallengeJournal();
		challengeJournal.Read(_br);
		rentedVMPosition = StreamUtils.ReadVector3i(_br);
		if (_version <= 38)
		{
			rentalEndTime = _br.ReadUInt64();
		}
		else
		{
			rentalEndDay = _br.ReadInt32();
		}
		int num4;
		if (_version <= 55)
		{
			num4 = _br.ReadUInt16();
			for (int num5 = 0; num5 < num4; num5++)
			{
				_br.ReadInt32();
			}
		}
		num4 = _br.ReadInt32();
		progressionData = ((num4 > 0) ? new MemoryStream(_br.ReadBytes(num4)) : new MemoryStream());
		num4 = _br.ReadInt32();
		buffData = ((num4 > 0) ? new MemoryStream(_br.ReadBytes(num4)) : new MemoryStream());
		num4 = _br.ReadInt32();
		stealthData = ((num4 > 0) ? new MemoryStream(_br.ReadBytes(num4)) : new MemoryStream());
		favoriteCreativeStacks.Clear();
		num4 = _br.ReadUInt16();
		for (int num6 = 0; num6 < num4; num6++)
		{
			favoriteCreativeStacks.Add(_br.ReadUInt16());
		}
		if (_version > 50)
		{
			favoriteShapes.Clear();
			num4 = _br.ReadUInt16();
			for (int num7 = 0; num7 < num4; num7++)
			{
				favoriteShapes.Add(_br.ReadString());
			}
		}
		if (_version > 44)
		{
			num4 = _br.ReadUInt16();
			ownedEntities.Clear();
			for (int num8 = 0; num8 < num4; num8++)
			{
				if (_version > 47)
				{
					OwnedEntityData ownedEntityData = new OwnedEntityData();
					ownedEntityData.Read(_br);
					ownedEntities.Add(ownedEntityData);
					continue;
				}
				int entityId = _br.ReadInt32();
				int classId = -1;
				if (_version > 46)
				{
					classId = _br.ReadInt32();
				}
				ownedEntities.Add(new OwnedEntityData(entityId, classId));
			}
		}
		if (_version > 45)
		{
			totalTimePlayed = _br.ReadSingle();
		}
	}

	public void Save(string _dir, string _playerId)
	{
		try
		{
			if (!SdDirectory.Exists(_dir))
			{
				SdDirectory.CreateDirectory(_dir);
			}
			string text = _dir + "/" + _playerId + "." + EXT;
			if (SdFile.Exists(text))
			{
				SdFile.Copy(text, text + ".bak", overwrite: true);
			}
			if (SdFile.Exists(text + ".tmp"))
			{
				SdFile.Delete(text + ".tmp");
			}
			using (Stream baseStream = SdFile.Open(text + ".tmp", FileMode.CreateNew, FileAccess.Write, FileShare.Read))
			{
				using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
				pooledBinaryWriter.SetBaseStream(baseStream);
				pooledBinaryWriter.Write('t');
				pooledBinaryWriter.Write('t');
				pooledBinaryWriter.Write('p');
				pooledBinaryWriter.Write((byte)0);
				pooledBinaryWriter.Write((byte)57);
				Write(pooledBinaryWriter);
				bModifiedSinceLastSave = false;
			}
			if (SdFile.Exists(text + ".tmp"))
			{
				SdFile.Copy(text + ".tmp", text, overwrite: true);
				SdFile.Delete(text + ".tmp");
			}
			metadata.Write(text + ".meta");
		}
		catch (Exception ex)
		{
			Log.Error("Save PlayerData file: " + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void Write(PooledBinaryWriter _bw)
	{
		ecd.write(_bw, _bNetworkWrite: false);
		GameUtils.WriteItemStack(_bw, inventory);
		_bw.Write((byte)selectedInventorySlot);
		GameUtils.WriteItemStack(_bw, bag);
		_bw.Write(bagLockedSlots != null);
		bagLockedSlots?.Write(_bw);
		GameUtils.WriteItemStack(_bw, new List<ItemStack> { dragAndDropItem });
		_bw.Write((ushort)alreadyCraftedList.Count);
		foreach (string alreadyCrafted in alreadyCraftedList)
		{
			_bw.Write(alreadyCrafted);
		}
		_bw.Write((byte)0);
		_bw.Write(selectedSpawnPointKey);
		_bw.Write(true);
		_bw.Write((short)0);
		_bw.Write(bLoaded);
		_bw.Write((int)lastSpawnPosition.position.x);
		_bw.Write((int)lastSpawnPosition.position.y);
		_bw.Write((int)lastSpawnPosition.position.z);
		_bw.Write(lastSpawnPosition.heading);
		_bw.Write(id);
		_bw.Write(playerKills);
		_bw.Write(zombieKills);
		_bw.Write(deaths);
		_bw.Write(score);
		equipment.Write(_bw);
		_bw.Write((ushort)unlockedRecipeList.Count);
		foreach (string unlockedRecipe in unlockedRecipeList)
		{
			_bw.Write(unlockedRecipe);
		}
		_bw.Write((ushort)1);
		StreamUtils.Write(_bw, markerPosition);
		_bw.Write(markerHidden);
		_bw.Write(bCrouchedLocked);
		craftingData.Write(_bw);
		_bw.Write((ushort)favoriteRecipeList.Count);
		foreach (string favoriteRecipe in favoriteRecipeList)
		{
			_bw.Write(favoriteRecipe);
		}
		_bw.Write(totalItemsCrafted);
		_bw.Write(distanceWalked);
		_bw.Write(longestLife);
		_bw.Write(gameStageBornAtWorldTime);
		waypoints.Write(_bw);
		questJournal.Write(_bw);
		_bw.Write(deathUpdateTime);
		_bw.Write(currentLife);
		_bw.Write(bDead);
		_bw.Write((byte)88);
		_bw.Write(bModdedSaveGame);
		challengeJournal.Write(_bw);
		StreamUtils.Write(_bw, rentedVMPosition);
		_bw.Write(rentalEndDay);
		progressionData.Position = 0L;
		_bw.Write((int)progressionData.Length);
		StreamUtils.StreamCopy(progressionData, _bw.BaseStream);
		buffData.Position = 0L;
		_bw.Write((int)buffData.Length);
		StreamUtils.StreamCopy(buffData, _bw.BaseStream);
		stealthData.Position = 0L;
		_bw.Write((int)stealthData.Length);
		StreamUtils.StreamCopy(stealthData, _bw.BaseStream);
		_bw.Write((ushort)favoriteCreativeStacks.Count);
		for (int i = 0; i < favoriteCreativeStacks.Count; i++)
		{
			_bw.Write(favoriteCreativeStacks[i]);
		}
		_bw.Write((ushort)favoriteShapes.Count);
		for (int j = 0; j < favoriteShapes.Count; j++)
		{
			_bw.Write(favoriteShapes[j]);
		}
		_bw.Write((ushort)ownedEntities.Count);
		for (int k = 0; k < ownedEntities.Count; k++)
		{
			ownedEntities[k].Write(_bw);
		}
		_bw.Write(totalTimePlayed);
	}

	public static bool Exists(string _dir, string _playerName)
	{
		return SdFile.Exists(_dir + "/" + _playerName + "." + EXT);
	}
}
