using System;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityCreationData
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int FileVersion = 35;

	public int entityClass;

	public Vector3 pos;

	public Vector3 rot;

	public int id;

	public bool onGround;

	public EntityStats stats;

	public int deathTime;

	public float lifetime = float.MaxValue;

	public int belongsPlayerId = -1;

	public int clientEntityId;

	public ItemValue holdingItem = ItemValue.None;

	public int teamNumber;

	public string entityName = "";

	public string skinTexture = "";

	public Bag bag;

	public TraderData traderData;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i homePosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public int homeRange = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumSpawnerSource spawnerSource;

	public ItemStack itemStack = ItemStack.Empty;

	public BlockValue[] blockValues;

	public Vector3i[] blockPositions;

	public TextureFullArray[] textureFullArrays;

	public Vector3i blockPos;

	public Vector3 fallTreeDir;

	public int subType;

	public byte sleeperPose = byte.MaxValue;

	public PlayerProfile playerProfile;

	public BodyDamage bodyDamage;

	public bool isSleeper;

	public bool isSleeperPassive;

	public string spawnByName = "";

	public int spawnById = -1;

	public bool spawnByAllowShare;

	public EModelBase.HeadStates headState;

	public float overrideSize = 1f;

	public float overrideHeadSize = 1f;

	public bool isDancing;

	public int orderState = -1;

	public byte readFileVersion;

	public MemoryStream entityData = new MemoryStream(0);

	public EntityCreationData()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityCreationData(EntityCreationData _other)
	{
		entityClass = _other.entityClass;
		pos = _other.pos;
		rot = _other.rot;
		id = _other.id;
		onGround = _other.onGround;
		stats = ((_other.stats != null) ? _other.stats.SimpleClone() : null);
		deathTime = _other.deathTime;
		lifetime = _other.lifetime;
		itemStack = _other.itemStack;
		bag = _other.bag?.Clone();
		belongsPlayerId = _other.belongsPlayerId;
		clientEntityId = _other.clientEntityId;
		holdingItem = _other.holdingItem;
		teamNumber = _other.teamNumber;
		entityName = _other.entityName;
		skinTexture = _other.skinTexture;
		subType = _other.subType;
		traderData = ((_other.traderData != null) ? _other.traderData.Clone() : null);
		homePosition = _other.homePosition;
		homeRange = _other.homeRange;
		entityData = _other.entityData;
		readFileVersion = _other.readFileVersion;
		playerProfile = _other.playerProfile;
		bodyDamage = _other.bodyDamage;
		sleeperPose = _other.sleeperPose;
		isSleeper = _other.isSleeper;
		isSleeperPassive = _other.isSleeperPassive;
		spawnByName = _other.spawnByName;
		spawnById = _other.spawnById;
		spawnByAllowShare = _other.spawnByAllowShare;
		headState = _other.headState;
		overrideSize = _other.overrideSize;
		overrideHeadSize = _other.overrideHeadSize;
		isDancing = _other.isDancing;
		orderState = _other.orderState;
	}

	public EntityCreationData(XmlElement _entityElement)
	{
		readXml(_entityElement);
	}

	public void ApplyToEntity(Entity _e)
	{
		EntityAlive entityAlive = _e as EntityAlive;
		if ((bool)entityAlive)
		{
			if (stats != null)
			{
				entityAlive.SetStats(stats);
			}
			if (entityAlive.Health <= 0)
			{
				entityAlive.HasDeathAnim = false;
			}
			entityAlive.SetDeathTime(deathTime);
			entityAlive.setHomeArea(homePosition, homeRange);
			EntityPlayer entityPlayer = _e as EntityPlayer;
			if ((bool)entityPlayer)
			{
				entityPlayer.playerProfile = playerProfile;
			}
			entityAlive.bodyDamage = bodyDamage;
			entityAlive.IsSleeper = isSleeper;
			if (entityAlive.IsSleeper)
			{
				entityAlive.IsSleeperPassive = isSleeperPassive;
			}
			entityAlive.CurrentHeadState = headState;
			entityAlive.IsDancing = isDancing;
		}
		_e.spawnByAllowShare = spawnByAllowShare;
		_e.spawnById = spawnById;
		_e.spawnByName = spawnByName;
		EntityTrader entityTrader = _e as EntityTrader;
		if ((bool)entityTrader)
		{
			if (traderData == null)
			{
				traderData = new TraderData();
			}
			entityTrader.TraderData = traderData.Clone();
		}
		if (sleeperPose != byte.MaxValue && (bool)entityAlive)
		{
			entityAlive.TriggerSleeperPose(sleeperPose);
		}
		if (_e is EntityDrone entityDrone)
		{
			entityDrone.OnApplyToEntity(orderState);
		}
		_e.SetSpawnerSource(spawnerSource);
		if (bag != null)
		{
			_e.bag = bag.Clone();
		}
		if (entityData.Length <= 0)
		{
			return;
		}
		entityData.Position = 0L;
		try
		{
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader.SetBaseStream(entityData);
			_e.Read(readFileVersion, pooledBinaryReader);
		}
		catch (Exception e)
		{
			Log.Exception(e);
			Log.Error("Error loading entity " + _e);
		}
	}

	public EntityCreationData(Entity _e, bool _bNetworkWrite = true)
	{
		entityClass = _e.entityClass;
		id = _e.entityId;
		pos = _e.position;
		rot = _e.rotation;
		onGround = _e.onGround;
		belongsPlayerId = _e.belongsPlayerId;
		clientEntityId = _e.clientEntityId;
		lifetime = _e.lifetime;
		spawnerSource = _e.GetSpawnerSource();
		spawnById = _e.spawnById;
		spawnByAllowShare = _e.spawnByAllowShare;
		spawnByName = _e.spawnByName;
		if (_e is EntityAlive)
		{
			EntityAlive entityAlive = _e as EntityAlive;
			if (entityAlive.inventory != null)
			{
				holdingItem = entityAlive.inventory.holdingItemItemValue;
			}
			stats = entityAlive.Stats;
			deathTime = entityAlive.GetDeathTime();
			teamNumber = entityAlive.TeamNumber;
			entityName = entityAlive.EntityName;
			skinTexture = string.Empty;
			homePosition = entityAlive.getHomePosition().position;
			homeRange = entityAlive.getMaximumHomeDistance();
			bodyDamage = entityAlive.bodyDamage;
			sleeperPose = (byte)(entityAlive.IsSleeping ? ((uint)entityAlive.lastSleeperPose) : 255u);
			isSleeper = entityAlive.IsSleeper;
			isSleeperPassive = entityAlive.IsSleeperPassive;
			if (_e is EntityPlayer)
			{
				EntityPlayer entityPlayer = _e as EntityPlayer;
				playerProfile = entityPlayer.playerProfile;
			}
			else if (_e is EntityTrader)
			{
				traderData = ((EntityTrader)_e).TraderData?.Clone();
			}
			else if (_e is EntityDrone entityDrone)
			{
				orderState = (int)entityDrone.OrderState;
			}
			headState = entityAlive.GetHeadState();
			overrideSize = entityAlive.OverrideSize;
			overrideHeadSize = entityAlive.OverrideHeadSize;
			isDancing = entityAlive.IsDancing;
		}
		else if (_e is EntityItem)
		{
			EntityItem entityItem = (EntityItem)_e;
			itemStack = entityItem.itemStack;
		}
		else if (_e is EntityFallingBlock)
		{
			EntityFallingBlock entityFallingBlock = _e as EntityFallingBlock;
			blockValues = new BlockValue[1] { entityFallingBlock.GetBlockValue() };
			textureFullArrays = new TextureFullArray[1] { entityFallingBlock.GetTextureFull() };
		}
		else if (_e is EntityFallingBlocks)
		{
			EntityFallingBlocks entityFallingBlocks = _e as EntityFallingBlocks;
			blockValues = entityFallingBlocks.GetBlockValues();
			blockPositions = entityFallingBlocks.GetBlockPositions();
			textureFullArrays = entityFallingBlocks.GetTextureFullArrays();
		}
		else if (_e is EntityFallingTree)
		{
			EntityFallingTree entityFallingTree = _e as EntityFallingTree;
			blockPos = entityFallingTree.GetBlockPos();
			fallTreeDir = entityFallingTree.GetFallTreeDir();
		}
		bag = _e.bag?.Clone();
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
		{
			pooledBinaryWriter.SetBaseStream(entityData);
			_e.Write(pooledBinaryWriter, _bNetworkWrite);
		}
		readFileVersion = 35;
	}

	public EntityCreationData Clone()
	{
		return new EntityCreationData(this);
	}

	public void read(PooledBinaryReader _br, bool _bNetworkRead)
	{
		readFileVersion = _br.ReadByte();
		byte b = readFileVersion;
		bag = null;
		entityClass = _br.ReadInt32();
		bool flag = entityClass == EntityClass.playerMaleClass || entityClass == EntityClass.playerFemaleClass;
		id = _br.ReadInt32();
		lifetime = _br.ReadSingle();
		pos.x = _br.ReadSingle();
		pos.y = _br.ReadSingle();
		pos.z = _br.ReadSingle();
		rot.x = _br.ReadSingle();
		rot.y = _br.ReadSingle();
		rot.z = _br.ReadSingle();
		onGround = _br.ReadBoolean();
		bodyDamage = BodyDamage.Read(_br, b);
		if (b >= 8)
		{
			if (_br.ReadBoolean())
			{
				stats = (flag ? new PlayerEntityStats() : new EntityStats());
				stats.Read(_br);
			}
		}
		else
		{
			_br.ReadInt16();
			_br.ReadInt16();
			if (b >= 7)
			{
				_br.ReadInt16();
				_br.ReadInt16();
			}
		}
		deathTime = _br.ReadInt16();
		if (b >= 35)
		{
			bool flag2 = _br.ReadBoolean();
			bag = (flag2 ? Bag.Read(_br) : null);
		}
		else if (b >= 2 && _br.ReadBoolean())
		{
			_br.ReadInt32();
			bag = TileEntityLegacyUtils.ReadLegacyLootIntoBag(_br);
		}
		if (b >= 3)
		{
			homePosition = new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32());
			homeRange = _br.ReadInt16();
		}
		if (b >= 5)
		{
			spawnerSource = (EnumSpawnerSource)_br.ReadByte();
		}
		if (entityClass == EntityClass.itemClass)
		{
			if (b <= 5)
			{
				belongsPlayerId = _br.ReadInt16();
			}
			else
			{
				belongsPlayerId = _br.ReadInt32();
			}
			if (b >= 27)
			{
				clientEntityId = _br.ReadInt32();
			}
			itemStack = ItemStack.Empty;
			if (b < 14)
			{
				itemStack.ReadOld(_br);
			}
			else
			{
				itemStack.Read(_br);
			}
			if (b >= 3)
			{
				_br.ReadSByte();
			}
		}
		else if (entityClass == EntityClass.fallingBlockClass)
		{
			blockValues = new BlockValue[1]
			{
				new BlockValue(_br.ReadUInt32())
			};
			textureFullArrays = new TextureFullArray[1];
			if (b < 29)
			{
				textureFullArrays[0].Fill(0L);
				textureFullArrays[0][0] = _br.ReadInt64();
			}
			else
			{
				textureFullArrays[0].Read(_br);
			}
		}
		else if (entityClass == EntityClass.fallingBlocksClass)
		{
			int num = _br.ReadInt32();
			blockValues = new BlockValue[num];
			for (int i = 0; i < blockValues.Length; i++)
			{
				blockValues[i] = new BlockValue(_br.ReadUInt32());
			}
			blockPositions = new Vector3i[num];
			for (int j = 0; j < blockPositions.Length; j++)
			{
				blockPositions[j] = StreamUtils.ReadVector3i(_br);
			}
			textureFullArrays = new TextureFullArray[num];
			for (int k = 0; k < textureFullArrays.Length; k++)
			{
				if (b < 29)
				{
					textureFullArrays[k].Fill(0L);
					textureFullArrays[k][0] = _br.ReadInt64();
				}
				else
				{
					textureFullArrays[k].Read(_br);
				}
			}
		}
		else if (entityClass == EntityClass.fallingTreeClass)
		{
			blockPos = StreamUtils.ReadVector3i(_br);
			fallTreeDir = StreamUtils.ReadVector3(_br);
		}
		else if (flag)
		{
			holdingItem.Read(_br);
			teamNumber = _br.ReadByte();
			entityName = _br.ReadString();
			skinTexture = _br.ReadString();
			if (b > 12)
			{
				if (_br.ReadBoolean())
				{
					playerProfile = PlayerProfile.Read(_br);
				}
				else
				{
					playerProfile = null;
				}
			}
		}
		if (b > 9)
		{
			int num2 = _br.ReadUInt16();
			if (num2 > 0)
			{
				byte[] buffer = _br.ReadBytes(num2);
				entityData = new MemoryStream(buffer);
			}
		}
		if (b > 23 && _br.ReadBoolean())
		{
			if (b >= 34)
			{
				if (traderData == null)
				{
					traderData = new TraderData();
				}
				traderData.Read(_br);
			}
			else
			{
				traderData = TileEntityLegacyUtils.ReadLegacyTileEntityTraderData(_br);
			}
		}
		if (_bNetworkRead)
		{
			sleeperPose = _br.ReadByte();
			isSleeper = _br.ReadBoolean();
			spawnById = _br.ReadInt32();
			spawnByName = _br.ReadString();
			spawnByAllowShare = _br.ReadBoolean();
			headState = (EModelBase.HeadStates)_br.ReadByte();
			overrideSize = _br.ReadSingle();
			overrideHeadSize = _br.ReadSingle();
			isDancing = _br.ReadBoolean();
			if (isSleeper)
			{
				isSleeperPassive = _br.ReadBoolean();
			}
		}
		if (b > 30 && entityClass == EntityClass.junkDroneClass)
		{
			belongsPlayerId = _br.ReadInt32();
		}
		if (b > 31 && entityClass == EntityClass.junkDroneClass)
		{
			orderState = _br.ReadInt32();
		}
	}

	public void write(PooledBinaryWriter _bw, bool _bNetworkWrite)
	{
		_bw.Write((byte)35);
		_bw.Write(entityClass);
		_bw.Write(id);
		_bw.Write(lifetime);
		_bw.Write(pos.x);
		_bw.Write(pos.y);
		_bw.Write(pos.z);
		_bw.Write(rot.x);
		_bw.Write(rot.y);
		_bw.Write(rot.z);
		_bw.Write(onGround);
		bodyDamage.Write(_bw);
		_bw.Write(stats != null);
		if (stats != null)
		{
			stats.Write(_bw);
		}
		_bw.Write((short)deathTime);
		bool flag = bag != null;
		_bw.Write(flag);
		if (flag)
		{
			bag.Write(_bw);
		}
		_bw.Write(homePosition.x);
		_bw.Write(homePosition.y);
		_bw.Write(homePosition.z);
		_bw.Write((short)homeRange);
		_bw.Write((byte)spawnerSource);
		if (entityClass == EntityClass.itemClass)
		{
			_bw.Write(belongsPlayerId);
			_bw.Write(clientEntityId);
			itemStack.Write(_bw);
			_bw.Write((sbyte)0);
		}
		else if (entityClass == EntityClass.fallingBlockClass)
		{
			_bw.Write(blockValues[0].rawData);
			textureFullArrays[0].Write(_bw);
		}
		else if (entityClass == EntityClass.fallingBlocksClass)
		{
			_bw.Write(blockValues.Length);
			for (int i = 0; i < blockValues.Length; i++)
			{
				_bw.Write(blockValues[i].rawData);
			}
			for (int j = 0; j < blockPositions.Length; j++)
			{
				StreamUtils.Write(_bw, blockPositions[j]);
			}
			for (int k = 0; k < textureFullArrays.Length; k++)
			{
				textureFullArrays[k].Write(_bw);
			}
		}
		else if (entityClass == EntityClass.fallingTreeClass)
		{
			StreamUtils.Write(_bw, blockPos);
			StreamUtils.Write(_bw, fallTreeDir);
		}
		else if (entityClass == EntityClass.playerMaleClass || entityClass == EntityClass.playerFemaleClass)
		{
			ItemValue.Write(holdingItem, _bw);
			_bw.Write((byte)teamNumber);
			_bw.Write(entityName);
			_bw.Write(skinTexture);
			_bw.Write(playerProfile != null);
			if (playerProfile != null)
			{
				playerProfile.Write(_bw);
			}
		}
		int num = (int)entityData.Length;
		_bw.Write((ushort)num);
		if (num > 0)
		{
			_bw.Write(entityData.ToArray());
		}
		_bw.Write(traderData != null);
		if (traderData != null)
		{
			traderData.Write(_bw);
		}
		if (_bNetworkWrite)
		{
			_bw.Write(sleeperPose);
			_bw.Write(isSleeper);
			_bw.Write(spawnById);
			_bw.Write(spawnByName);
			_bw.Write(spawnByAllowShare);
			_bw.Write((byte)headState);
			_bw.Write(overrideSize);
			_bw.Write(overrideHeadSize);
			_bw.Write(isDancing);
			if (isSleeper)
			{
				_bw.Write(isSleeperPassive);
			}
		}
		if (entityClass == EntityClass.junkDroneClass)
		{
			_bw.Write(belongsPlayerId);
			_bw.Write(orderState);
		}
	}

	public void readXml(XmlElement _entityElement)
	{
		if (!_entityElement.HasAttribute("type"))
		{
			throw new Exception("No 'type' element found in entity tag!");
		}
		entityClass = EntityClass.FromString(_entityElement.GetAttribute("type"));
		if (!_entityElement.HasAttribute("position"))
		{
			throw new Exception("No 'position' element found in entity tag!");
		}
		pos = StringParsers.ParseVector3(_entityElement.GetAttribute("position"));
		if (!_entityElement.HasAttribute("rotation"))
		{
			throw new Exception("No 'rotation' element found in entity tag!");
		}
		rot = StringParsers.ParseVector3(_entityElement.GetAttribute("rotation"));
		id = -1;
	}

	public void writeXml(StreamWriter _sw)
	{
		_sw.WriteLine("    <entity type=\"" + EntityClass.list[entityClass].entityClassName + "\" position=\"" + pos.x.ToCultureInvariantString() + "," + pos.y.ToCultureInvariantString() + "," + pos.z.ToCultureInvariantString() + "\" rotation=\"" + rot.x.ToCultureInvariantString() + "," + rot.y.ToCultureInvariantString() + "," + rot.z.ToCultureInvariantString() + "\" />");
	}

	public override string ToString()
	{
		return EntityClass.list[entityClass].entityClassName + " " + entityName + " id=" + id + " pos=" + pos.ToCultureInvariantString();
	}
}
