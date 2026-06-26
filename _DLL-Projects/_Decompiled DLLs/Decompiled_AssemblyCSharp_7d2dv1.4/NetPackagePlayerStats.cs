using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerStats : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int experience;

	[PublicizedFrom(EAccessModifier.Private)]
	public int level;

	[PublicizedFrom(EAccessModifier.Private)]
	public int killed;

	[PublicizedFrom(EAccessModifier.Private)]
	public int killedZombies;

	[PublicizedFrom(EAccessModifier.Private)]
	public int killedPlayers;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack holdingItemStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte holdingItemIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int deathHealth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int teamNumber;

	[PublicizedFrom(EAccessModifier.Private)]
	public Equipment equipment;

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledExpandableMemoryStream progStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasProgression;

	[PublicizedFrom(EAccessModifier.Private)]
	public int attachedToEntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string entityName;

	[PublicizedFrom(EAccessModifier.Private)]
	public float distanceWalked;

	[PublicizedFrom(EAccessModifier.Private)]
	public uint totalItemsCrafted;

	[PublicizedFrom(EAccessModifier.Private)]
	public float longestLife;

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentLife;

	[PublicizedFrom(EAccessModifier.Private)]
	public float totalTimePlayed;

	[PublicizedFrom(EAccessModifier.Private)]
	public int vehiclePose;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSpectator;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPlayer;

	public NetPackagePlayerStats Setup(EntityAlive _entity)
	{
		entityId = _entity.entityId;
		killed = _entity.Died;
		holdingItemStack = _entity.inventory.holdingItemStack;
		holdingItemIndex = (byte)_entity.inventory.holdingItemIdx;
		deathHealth = _entity.DeathHealth;
		teamNumber = _entity.TeamNumber;
		equipment = _entity.equipment;
		if (GameManager.Instance.World.GetPrimaryPlayer() == _entity)
		{
			_entity.inventory.TurnOffLightFlares();
		}
		if (_entity.Progression != null && _entity.Progression.bProgressionStatsChanged)
		{
			_entity.Progression.bProgressionStatsChanged = false;
			hasProgression = true;
			using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
			pooledBinaryWriter.SetBaseStream(progStream);
			_entity.Progression.Write(pooledBinaryWriter, _IsNetwork: true);
		}
		attachedToEntityId = ((_entity.AttachedToEntity != null) ? _entity.AttachedToEntity.entityId : (-1));
		entityName = _entity.EntityName;
		EntityPlayer entityPlayer = _entity as EntityPlayer;
		if (entityPlayer != null)
		{
			isPlayer = true;
			killedPlayers = _entity.KilledPlayers;
			killedZombies = _entity.KilledZombies;
			experience = entityPlayer.Progression.ExpToNextLevel;
			level = entityPlayer.Progression.Level;
			totalItemsCrafted = entityPlayer.totalItemsCrafted;
			distanceWalked = entityPlayer.distanceWalked;
			longestLife = entityPlayer.longestLife;
			currentLife = entityPlayer.currentLife;
			totalTimePlayed = entityPlayer.totalTimePlayed;
			vehiclePose = entityPlayer.GetVehicleAnimation();
			isSpectator = entityPlayer.IsSpectator;
		}
		else
		{
			isPlayer = false;
			experience = 0;
			level = 1;
			distanceWalked = 0f;
			totalItemsCrafted = 0u;
			longestLife = 0f;
			currentLife = 0f;
			totalTimePlayed = 0f;
		}
		return this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~NetPackagePlayerStats()
	{
		MemoryPools.poolMemoryStream.FreeSync(progStream);
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		killed = _reader.ReadInt32();
		holdingItemStack = new ItemStack();
		holdingItemStack.Read(_reader);
		holdingItemIndex = _reader.ReadByte();
		deathHealth = _reader.ReadInt32();
		teamNumber = _reader.ReadByte();
		equipment = Equipment.Read(_reader);
		attachedToEntityId = _reader.ReadInt32();
		entityName = _reader.ReadString();
		isPlayer = _reader.ReadBoolean();
		if (isPlayer)
		{
			killedZombies = _reader.ReadInt32();
			killedPlayers = _reader.ReadInt32();
			experience = _reader.ReadInt32();
			level = _reader.ReadInt32();
			totalItemsCrafted = _reader.ReadUInt32();
			distanceWalked = _reader.ReadSingle();
			longestLife = _reader.ReadSingle();
			currentLife = _reader.ReadSingle();
			totalTimePlayed = _reader.ReadSingle();
			vehiclePose = _reader.ReadInt32();
			isSpectator = _reader.ReadBoolean();
		}
		hasProgression = _reader.ReadBoolean();
		if (hasProgression)
		{
			int length = _reader.ReadInt16();
			StreamUtils.StreamCopy(_reader.BaseStream, progStream, length);
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		_writer.Write(killed);
		holdingItemStack.Write(_writer);
		_writer.Write(holdingItemIndex);
		_writer.Write(deathHealth);
		_writer.Write((byte)teamNumber);
		equipment.Write(_writer);
		_writer.Write(attachedToEntityId);
		_writer.Write(entityName);
		_writer.Write(isPlayer);
		if (isPlayer)
		{
			_writer.Write(killedZombies);
			_writer.Write(killedPlayers);
			_writer.Write(experience);
			_writer.Write(level);
			_writer.Write(totalItemsCrafted);
			_writer.Write(distanceWalked);
			_writer.Write(longestLife);
			_writer.Write(currentLife);
			_writer.Write(totalTimePlayed);
			_writer.Write(vehiclePose);
			_writer.Write(isSpectator);
		}
		_writer.Write(hasProgression);
		if (hasProgression)
		{
			_writer.Write((short)progStream.Length);
			progStream.WriteTo(_writer.BaseStream);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = _world.GetEntity(entityId) as EntityAlive;
		if (!entityAlive)
		{
			Log.Out("Discarding " + GetType().Name + " for entity Id=" + entityId);
		}
		else
		{
			if (!ValidEntityIdForSender(entityId, _allowAttachedToEntity: true))
			{
				return;
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && entityAlive is EntityPlayer)
			{
				entityName = base.Sender.playerName;
			}
			entityAlive.Died = killed;
			entityAlive.DeathHealth = deathHealth;
			entityAlive.TeamNumber = teamNumber;
			entityAlive.inventory.bResetLightLevelWhenChanged = true;
			if (!entityAlive.inventory.GetItem(holdingItemIndex).Equals(holdingItemStack))
			{
				entityAlive.inventory.SetItem(holdingItemIndex, holdingItemStack);
			}
			if (entityAlive.inventory.holdingItemIdx != holdingItemIndex)
			{
				entityAlive.inventory.SetHoldingItemIdxNoHolsterTime(holdingItemIndex);
			}
			entityAlive.equipment.Apply(equipment, isLocal: false);
			if (hasProgression)
			{
				using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false))
				{
					lock (progStream)
					{
						pooledBinaryReader.SetBaseStream(progStream);
						progStream.Position = 0L;
						entityAlive.Progression = Progression.Read(pooledBinaryReader, entityAlive);
					}
				}
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && entityAlive.Progression != null)
				{
					entityAlive.Progression.bProgressionStatsChanged = true;
				}
			}
			entityAlive.SetEntityName(entityName);
			EntityPlayer entityPlayer = entityAlive as EntityPlayer;
			if (entityPlayer != null && isPlayer)
			{
				if (entityAlive.NavObject != null)
				{
					entityAlive.NavObject.name = entityName;
				}
				entityAlive.KilledZombies = killedZombies;
				entityAlive.KilledPlayers = killedPlayers;
				entityPlayer.Progression.ExpToNextLevel = experience;
				entityPlayer.Progression.Level = level;
				entityPlayer.totalItemsCrafted = totalItemsCrafted;
				entityPlayer.distanceWalked = distanceWalked;
				entityPlayer.longestLife = longestLife;
				entityPlayer.currentLife = currentLife;
				entityPlayer.totalTimePlayed = totalTimePlayed;
				entityPlayer.SetVehiclePoseMode(vehiclePose);
				entityPlayer.IsSpectator = isSpectator;
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(entityAlive), _onlyClientsAttachedToAnEntity: false, -1, base.Sender.entityId);
			}
		}
	}

	public override int GetLength()
	{
		return 60;
	}
}
