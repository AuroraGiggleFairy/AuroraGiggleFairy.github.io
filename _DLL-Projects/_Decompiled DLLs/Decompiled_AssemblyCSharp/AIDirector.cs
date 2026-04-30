using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Twitch;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIDirector
{
	public enum HordeEvent
	{
		None,
		Warn1,
		Warn2,
		Spawn
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cActivityDuration = 720f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cActivityNoiseDuration = 240f;

	public readonly World World;

	public GameRandom random;

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionaryList<string, AIDirectorComponent> components = new DictionaryList<string, AIDirectorComponent>();

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorPlayerManagementComponent playerManagementComponent;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorChunkEventComponent chunkEventComponent;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorBloodMoonComponent bloodMoonComponent;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> debugEntities = new List<Entity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDebugSendNameInfoTickRate = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<int> debugSendNameInfoToPlayerIds = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int debugNameInfoTicks;

	public static bool debugFreezePos;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cLatencyName = "DebugLatency";

	[PublicizedFrom(EAccessModifier.Private)]
	public static MemoryStream latencyStream = new MemoryStream();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<int> debugSendLatencyToPlayerIds = new List<int>();

	public AIDirectorBloodMoonComponent BloodMoonComponent => bloodMoonComponent;

	public AIDirector(World _world)
	{
		World = _world;
		CreateComponents();
		Init();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		random = GameRandomManager.Instance.CreateGameRandom();
		ComponentsInitNewGame();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateComponents()
	{
		CreateComponent<AIDirectorMarkerManagementComponent>();
		CreateComponent<AIDirectorPlayerManagementComponent>();
		CreateComponent<AIDirectorWanderingHordeComponent>();
		CreateComponent<AIDirectorAirDropComponent>();
		CreateComponent<AIDirectorChunkEventComponent>();
		CreateComponent<AIDirectorBloodMoonComponent>();
		playerManagementComponent = GetComponent<AIDirectorPlayerManagementComponent>();
		chunkEventComponent = GetComponent<AIDirectorChunkEventComponent>();
		bloodMoonComponent = GetComponent<AIDirectorBloodMoonComponent>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public T CreateComponent<T>() where T : AIDirectorComponent, new()
	{
		string fullName = typeof(T).FullName;
		if (components.dict.ContainsKey(fullName))
		{
			throw new Exception("Multiple instances of the same component type are not allowed!");
		}
		T val = new T
		{
			Director = this
		};
		components.Add(fullName, val);
		return val;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComponentsInitNewGame()
	{
		for (int i = 0; i < components.list.Count; i++)
		{
			components.list[i].InitNewGame();
		}
	}

	public T GetComponent<T>() where T : AIDirectorComponent
	{
		string fullName = typeof(T).FullName;
		if (components.dict.TryGetValue(fullName, out var value))
		{
			return value as T;
		}
		return null;
	}

	public void Load(BinaryReader stream)
	{
		int version = stream.ReadInt32();
		ComponentsLoad(stream, version);
		if (World.worldTime == 0L)
		{
			Init();
		}
	}

	public void Save(BinaryWriter stream)
	{
		stream.Write(10);
		ComponentsSave(stream);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComponentsLoad(BinaryReader reader, int version)
	{
		for (int i = 0; i < components.list.Count; i++)
		{
			components.list[i].Read(reader, version);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComponentsSave(BinaryWriter writer)
	{
		for (int i = 0; i < components.list.Count; i++)
		{
			components.list[i].Write(writer);
		}
	}

	public void Tick(double dt)
	{
		ComponentsTick(dt);
		DebugTick();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComponentsTick(double _dt)
	{
		for (int i = 0; i < components.list.Count; i++)
		{
			components.list[i].Tick(_dt);
		}
	}

	public static bool CanSpawn(float _priority = 1f)
	{
		return (float)GameStats.GetInt(EnumGameStats.EnemyCount) < (float)GamePrefs.GetInt(EnumGamePrefs.MaxSpawnedZombies) * _priority;
	}

	public static ulong GetActivityWorldTimeDelay()
	{
		float v = (float)GameStats.GetInt(EnumGameStats.TimeOfDayIncPerSec) / 6f;
		v = Utils.FastClamp(v, 0.2f, 5f);
		return (ulong)(1000f * v);
	}

	public void NotifyActivity(EnumAIDirectorChunkEvent type, Vector3i position, float value, float _duration = 720f)
	{
		if (value > 0f && GameStats.GetBool(EnumGameStats.ZombieHordeMeter) && GameStats.GetBool(EnumGameStats.IsSpawnEnemies) && !BloodMoonComponent.BloodMoonActive && !TwitchManager.BossHordeActive)
		{
			AIDirectorChunkEvent chunkEvent = new AIDirectorChunkEvent(type, position, value, _duration);
			chunkEventComponent.NotifyEvent(chunkEvent);
		}
	}

	public void NotifyNoise(Entity instigator, Vector3 position, string clipName, float volumeScale)
	{
		if (!AIDirectorData.FindNoise(clipName, out var noise) || instigator is EntityEnemy)
		{
			return;
		}
		AIDirectorPlayerState value = null;
		if ((bool)instigator)
		{
			if (instigator.IsIgnoredByAI())
			{
				return;
			}
			playerManagementComponent.trackedPlayers.dict.TryGetValue(instigator.entityId, out value);
		}
		if (instigator is EntityItem entityItem && ItemClass.GetForId(entityItem.itemStack.itemValue.type).ThrowableDecoy.Value)
		{
			return;
		}
		if (value != null)
		{
			if (value.Player.IsCrouching)
			{
				volumeScale *= noise.muffledWhenCrouched;
			}
			float volume = noise.volume * volumeScale;
			if (value.Player.Stealth.NotifyNoise(volume, noise.duration))
			{
				instigator.world.CheckSleeperVolumeNoise(position);
			}
		}
		if (noise.heatMapStrength > 0f)
		{
			NotifyActivity(EnumAIDirectorChunkEvent.Sound, World.worldToBlockPos(position), noise.heatMapStrength * volumeScale, 240f);
		}
	}

	public void NotifyIntentToAttack(EntityAlive zombie, EntityAlive player)
	{
	}

	public void UpdatePlayerInventory(EntityPlayerLocal player)
	{
		playerManagementComponent.UpdatePlayerInventory(player);
	}

	public void UpdatePlayerInventory(int entityId, AIDirectorPlayerInventory inventory)
	{
		playerManagementComponent.UpdatePlayerInventory(entityId, inventory);
	}

	public void OnSoundPlayedAtPosition(int _entityThatCausedSound, Vector3 _position, string clipName, float volumeScale)
	{
		Entity instigator = null;
		if (_entityThatCausedSound != -1)
		{
			instigator = World.GetEntity(_entityThatCausedSound);
		}
		NotifyNoise(instigator, _position, clipName, volumeScale);
	}

	public void AddEntity(Entity entity)
	{
		EntityPlayer player;
		if ((bool)(player = entity as EntityPlayer))
		{
			AddPlayer(player);
		}
	}

	public void RemoveEntity(Entity entity)
	{
		EntityPlayer player;
		if ((bool)(player = entity as EntityPlayer))
		{
			RemovePlayer(player);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddPlayer(EntityPlayer player)
	{
		playerManagementComponent.AddPlayer(player);
		BloodMoonComponent.AddPlayer(player);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemovePlayer(EntityPlayer player)
	{
		playerManagementComponent.RemovePlayer(player);
		BloodMoonComponent.RemovePlayer(player);
	}

	public static void LogAI(string _format, params object[] _args)
	{
		_format = $"AIDirector: {_format}";
		Log.Out(_format, _args);
	}

	public static void LogAIExtra(string _format, params object[] _args)
	{
		if (AIDirectorConstants.DebugOutput)
		{
			LogAI(_format, _args);
		}
	}

	public void DebugFrameLateUpdate()
	{
		if (debugSendLatencyToPlayerIds.Count > 0)
		{
			DebugSendLatency();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DebugTick()
	{
		if (debugSendNameInfoToPlayerIds.Count > 0)
		{
			DebugSendNameInfo();
		}
	}

	public static void DebugToggleSendNameInfo(int playerId)
	{
		if (debugSendNameInfoToPlayerIds.Remove(playerId))
		{
			Log.Out("DebugToggleSendNames {0} off", playerId);
			NetPackageDebug package = NetPackageManager.GetPackage<NetPackageDebug>().Setup(NetPackageDebug.Type.AINameInfoClientOff);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, playerId);
		}
		else
		{
			Log.Out("DebugToggleSendNames {0} on", playerId);
			debugSendNameInfoToPlayerIds.Add(playerId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DebugSendNameInfo()
	{
		if (--debugNameInfoTicks > 0)
		{
			return;
		}
		debugNameInfoTicks = 5;
		World world = GameManager.Instance.World;
		for (int i = 0; i < debugSendNameInfoToPlayerIds.Count; i++)
		{
			int num = debugSendNameInfoToPlayerIds[i];
			world.Players.dict.TryGetValue(num, out var value);
			if (!value)
			{
				continue;
			}
			ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(num);
			if (clientInfo == null)
			{
				continue;
			}
			world.GetEntitiesInBounds(_bb: new Bounds(value.position, new Vector3(50f, 50f, 50f)), _class: typeof(EntityAlive), _list: debugEntities);
			for (int num2 = debugEntities.Count - 1; num2 >= 0; num2--)
			{
				EntityAlive entityAlive = (EntityAlive)debugEntities[num2];
				if (entityAlive.aiManager != null)
				{
					string s = entityAlive.aiManager.MakeDebugName(value);
					NetPackageDebug package = NetPackageManager.GetPackage<NetPackageDebug>().Setup(NetPackageDebug.Type.AINameInfo, entityAlive.entityId, Encoding.UTF8.GetBytes(s));
					clientInfo.SendPackage(package);
				}
			}
			debugEntities.Clear();
		}
	}

	public static void DebugReceiveNameInfo(int entityId, byte[] _data)
	{
		World world = GameManager.Instance.World;
		if (world != null)
		{
			EntityAlive entityAlive = world.GetEntity(entityId) as EntityAlive;
			if ((bool)entityAlive)
			{
				entityAlive.SetupDebugNameHUD(_isAdd: true);
				string debugNameInfo = Encoding.UTF8.GetString(_data);
				entityAlive.DebugNameInfo = debugNameInfo;
			}
		}
	}

	public static void DebugToggleFreezePos()
	{
		debugFreezePos = !debugFreezePos;
		Log.Out("DebugToggleFreezePos {0}", debugFreezePos);
	}

	public static void DebugToggleSendLatency(int playerId)
	{
		if (debugSendLatencyToPlayerIds.Remove(playerId))
		{
			Log.Out("DebugToggleSendLatency {0} off", playerId);
			if (GameManager.Instance.World.GetPrimaryPlayerId() != playerId)
			{
				NetPackageDebug package = NetPackageManager.GetPackage<NetPackageDebug>().Setup(NetPackageDebug.Type.AILatencyClientOff);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, _onlyClientsAttachedToAnEntity: false, playerId);
			}
			else
			{
				DebugLatencyOff();
			}
		}
		else
		{
			Log.Out("DebugToggleSendLatency {0} on", playerId);
			debugSendLatencyToPlayerIds.Add(playerId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DebugSendLatency()
	{
		World world = GameManager.Instance.World;
		for (int i = 0; i < debugSendLatencyToPlayerIds.Count; i++)
		{
			int num = debugSendLatencyToPlayerIds[i];
			world.Players.dict.TryGetValue(num, out var value);
			if (!value)
			{
				continue;
			}
			ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(num);
			world.GetEntitiesInBounds(_bb: new Bounds(value.position, new Vector3(50f, 50f, 50f)), _class: typeof(EntityAlive), _list: debugEntities);
			for (int num2 = debugEntities.Count - 1; num2 >= 0; num2--)
			{
				EntityAlive entityAlive = (EntityAlive)debugEntities[num2];
				if (entityAlive.aiManager != null)
				{
					using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
					pooledBinaryWriter.SetBaseStream(latencyStream);
					latencyStream.Position = 0L;
					pooledBinaryWriter.Write(entityAlive.position.x);
					pooledBinaryWriter.Write(entityAlive.position.y);
					pooledBinaryWriter.Write(entityAlive.position.z);
					Vector3 vector = entityAlive.GetVelocityPerSecond();
					Vector3 vector2 = entityAlive.motion * 20f;
					if (vector.sqrMagnitude < vector2.sqrMagnitude)
					{
						vector = vector2;
					}
					pooledBinaryWriter.Write(vector.x);
					pooledBinaryWriter.Write(vector.y);
					pooledBinaryWriter.Write(vector.z);
					Quaternion rotation = entityAlive.transform.rotation;
					pooledBinaryWriter.Write(rotation.x);
					pooledBinaryWriter.Write(rotation.y);
					pooledBinaryWriter.Write(rotation.z);
					pooledBinaryWriter.Write(rotation.w);
					byte[] data = latencyStream.ToArray();
					if (clientInfo != null)
					{
						NetPackageDebug package = NetPackageManager.GetPackage<NetPackageDebug>().Setup(NetPackageDebug.Type.AILatency, entityAlive.entityId, data);
						clientInfo.SendPackage(package);
					}
					else
					{
						DebugReceiveLatency(entityAlive.entityId, data);
					}
				}
			}
			debugEntities.Clear();
		}
	}

	public static void DebugReceiveLatency(int entityId, byte[] _data)
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		EntityAlive entityAlive = world.GetEntity(entityId) as EntityAlive;
		if (!entityAlive)
		{
			return;
		}
		using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
		MemoryStream baseStream = new MemoryStream(_data);
		pooledBinaryReader.SetBaseStream(baseStream);
		Vector3 vector = default(Vector3);
		vector.x = pooledBinaryReader.ReadSingle();
		vector.y = pooledBinaryReader.ReadSingle();
		vector.z = pooledBinaryReader.ReadSingle();
		Vector3 vector2 = default(Vector3);
		vector2.x = pooledBinaryReader.ReadSingle();
		vector2.y = pooledBinaryReader.ReadSingle();
		vector2.z = pooledBinaryReader.ReadSingle();
		Quaternion rotation = default(Quaternion);
		rotation.x = pooledBinaryReader.ReadSingle();
		rotation.y = pooledBinaryReader.ReadSingle();
		rotation.z = pooledBinaryReader.ReadSingle();
		rotation.w = pooledBinaryReader.ReadSingle();
		Transform transform = entityAlive.transform;
		Transform parent = transform.parent;
		Transform transform2 = parent.Find("DebugLatency");
		if (!transform2)
		{
			GameObject obj = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Debug/DebugLatency"), parent);
			obj.name = "DebugLatency";
			transform2 = obj.transform;
		}
		Vector3 vector3 = (transform2.position = vector - Origin.position);
		transform2.rotation = rotation;
		LineRenderer component = transform2.GetComponent<LineRenderer>();
		component.SetPosition(0, Quaternion.Inverse(rotation) * (transform.position - vector3));
		float num = (float)world.GetPrimaryPlayer().pingToServer * 0.001f;
		if (num < 0f)
		{
			num = 0f;
		}
		num *= 2f;
		if (vector2.y < 0f)
		{
			vector2.y = 0f;
		}
		component.SetPosition(2, Quaternion.Inverse(rotation) * (vector2 * num));
	}

	public static void DebugLatencyOff()
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		for (int i = 0; i < world.Entities.list.Count; i++)
		{
			EntityAlive entityAlive = world.Entities.list[i] as EntityAlive;
			if ((bool)entityAlive)
			{
				Transform transform = entityAlive.transform.parent.Find("DebugLatency");
				if ((bool)transform)
				{
					UnityEngine.Object.Destroy(transform.gameObject);
				}
			}
		}
	}
}
