using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorAirDropComponent : AIDirectorComponent
{
	[Preserve]
	public class SupplyCrateCache
	{
		public ChunkManager.ChunkObserver ChunkObserver;

		public int entityId;

		public Vector3i blockPos;

		public bool requiresObserver;

		public SupplyCrateCache(int id, Vector3i blockPos, bool requiresObserver)
		{
			entityId = id;
			this.blockPos = blockPos;
			this.requiresObserver = requiresObserver;
		}
	}

	public List<SupplyCrateCache> supplyCrates = new List<SupplyCrateCache>();

	[PublicizedFrom(EAccessModifier.Private)]
	public AIAirDrop activeAirDrop;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong nextAirDropTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong lastAirdropCheckTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong lastFrequency;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string NavObjectClass = "supply_drop";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] crateTypes = new string[1] { "sc_General" };

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong calcNextAirdrop(ulong currentTime, ulong dropFrequency)
	{
		ulong num = currentTime / 1000;
		if (dropFrequency % 24 == 0L)
		{
			return (dropFrequency + num) / 24 * 24000 + 12000;
		}
		return (dropFrequency + num) * 1000;
	}

	public override void Connect()
	{
		base.Connect();
	}

	public override void InitNewGame()
	{
		base.InitNewGame();
		activeAirDrop = null;
		lastAirdropCheckTime = Director.World.worldTime;
		nextAirDropTime = calcNextAirdrop(lastAirdropCheckTime, (ulong)GameStats.GetInt(EnumGameStats.AirDropFrequency));
		supplyCrates.Clear();
	}

	public override void Tick(double _dt)
	{
		base.Tick(_dt);
		if (activeAirDrop != null)
		{
			if (activeAirDrop.Tick((float)_dt))
			{
				activeAirDrop = null;
			}
			return;
		}
		ulong num = (ulong)GameStats.GetInt(EnumGameStats.AirDropFrequency);
		ulong worldTime = Director.World.worldTime;
		if (GameUtils.IsPlaytesting() || num == 0)
		{
			return;
		}
		if (worldTime >= nextAirDropTime)
		{
			if (SpawnAirDrop())
			{
				lastAirdropCheckTime = worldTime;
				nextAirDropTime = calcNextAirdrop(worldTime, num);
			}
		}
		else if (num != lastFrequency || worldTime < lastAirdropCheckTime)
		{
			nextAirDropTime = calcNextAirdrop(worldTime, num);
			lastFrequency = num;
			lastAirdropCheckTime = worldTime;
		}
	}

	public override void Read(BinaryReader _stream, int _version)
	{
		base.Read(_stream, _version);
		nextAirDropTime = _stream.ReadUInt64();
		if (_version >= 9)
		{
			lastFrequency = _stream.ReadUInt64();
		}
		else
		{
			lastFrequency = (ulong)GameStats.GetInt(EnumGameStats.AirDropFrequency);
		}
		supplyCrates.Clear();
		int num = _stream.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int num2 = _stream.ReadInt32();
			if (num2 > lastID)
			{
				lastID = num2;
			}
			Vector3i vector3i = StreamUtils.ReadVector3i(_stream);
			bool flag = false;
			if (_version >= 10)
			{
				flag = _stream.ReadBoolean();
			}
			SupplyCrateCache supplyCrateCache = new SupplyCrateCache(num2, vector3i, flag);
			if (flag)
			{
				supplyCrateCache.ChunkObserver = Director.World.GetGameManager().AddChunkObserver(vector3i, _bBuildVisualMeshAround: false, 3, -1);
			}
			AddSupplyCrate(supplyCrateCache);
		}
		RefreshCrates();
	}

	public override void Write(BinaryWriter _stream)
	{
		base.Write(_stream);
		_stream.Write(nextAirDropTime);
		_stream.Write(lastFrequency);
		_stream.Write(supplyCrates.Count);
		for (int i = 0; i < supplyCrates.Count; i++)
		{
			_stream.Write(supplyCrates[i].entityId);
			StreamUtils.Write(_stream, supplyCrates[i].blockPos);
			EntitySupplyCrate entitySupplyCrate = Director.World.GetEntity(supplyCrates[i].entityId) as EntitySupplyCrate;
			if (entitySupplyCrate != null)
			{
				supplyCrates[i].requiresObserver = entitySupplyCrate.RequiresChunkObserver();
			}
			_stream.Write(supplyCrates[i].requiresObserver);
		}
	}

	public bool SpawnAirDrop()
	{
		bool result = false;
		if (activeAirDrop == null)
		{
			List<EntityPlayer> list = new List<EntityPlayer>();
			DictionaryList<int, AIDirectorPlayerState> trackedPlayers = Director.GetComponent<AIDirectorPlayerManagementComponent>().trackedPlayers;
			for (int i = 0; i < trackedPlayers.list.Count; i++)
			{
				AIDirectorPlayerState aIDirectorPlayerState = trackedPlayers.list[i];
				if (!aIDirectorPlayerState.Player.IsDead())
				{
					list.Add(aIDirectorPlayerState.Player);
				}
			}
			if (list.Count > 0)
			{
				activeAirDrop = new AIAirDrop(this, Director.World, list);
				result = true;
			}
		}
		return result;
	}

	public void RemoveSupplyCrate(int entityId)
	{
		int num = -1;
		for (int i = 0; i < supplyCrates.Count; i++)
		{
			if (supplyCrates[i].entityId == entityId)
			{
				num = i;
				break;
			}
		}
		if (num > -1)
		{
			supplyCrates.RemoveAt(num);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageNavObject>().Setup(entityId));
		}
		else
		{
			Log.Warning($"{GameManager.frameCount} AIDirectorAirDropComponent: Attempted to remove supply crate cache with missing entityID {entityId}");
		}
	}

	public void SetSupplyCratePosition(int entityId, Vector3i blockPos)
	{
		foreach (SupplyCrateCache supplyCrate in supplyCrates)
		{
			if (supplyCrate.entityId == entityId)
			{
				supplyCrate.blockPos = blockPos;
				return;
			}
		}
		Log.Warning($"Supply crate {entityId} not in the list, can't set position");
	}

	public EntitySupplyCrate SpawnSupplyCrate(Vector3 spawnPos, ChunkManager.ChunkObserver chunkObserver)
	{
		if (Director.World == null)
		{
			return null;
		}
		if (supplyCrates.Count >= 12)
		{
			Entity entity = Director.World.GetEntity(supplyCrates[0].entityId);
			if (entity != null)
			{
				entity.MarkToUnload();
			}
			supplyCrates.RemoveAt(0);
		}
		Entity entity2 = EntityFactory.CreateEntity(EntityClass.FromString(crateTypes[base.Random.RandomRange(0, crateTypes.Length)]), spawnPos, new Vector3(base.Random.RandomFloat * 360f, 0f, 0f));
		Director.World.SpawnEntityInWorld(entity2);
		SupplyCrateCache supplyCrateCache = new SupplyCrateCache(entity2.entityId, World.worldToBlockPos(entity2.position), requiresObserver: true);
		supplyCrateCache.ChunkObserver = chunkObserver;
		AddSupplyCrate(supplyCrateCache);
		RefreshCrates();
		return entity2 as EntitySupplyCrate;
	}

	public void RefreshCrates(int _shareWithClient = -1)
	{
		foreach (SupplyCrateCache supplyCrate in supplyCrates)
		{
			if (supplyCrate.requiresObserver)
			{
				EntitySupplyCrate entitySupplyCrate = Director.World.GetEntity(supplyCrate.entityId) as EntitySupplyCrate;
				if (entitySupplyCrate != null)
				{
					supplyCrate.requiresObserver = entitySupplyCrate.RequiresChunkObserver();
				}
			}
			if (!supplyCrate.requiresObserver && supplyCrate.ChunkObserver != null)
			{
				Director.World.GetGameManager().RemoveChunkObserver(supplyCrate.ChunkObserver);
				supplyCrate.ChunkObserver = null;
			}
			if (!GameStats.GetBool(EnumGameStats.AirDropMarker))
			{
				continue;
			}
			NavObject navObject = ((supplyCrate.entityId == -1) ? null : NavObjectManager.Instance.GetNavObjectByEntityID(supplyCrate.entityId));
			if (navObject != null && navObject.TrackType == NavObject.TrackTypes.Entity)
			{
				if (_shareWithClient != -1)
				{
					Vector3 position = navObject.GetPosition() + Origin.position;
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageNavObject>().Setup(navObject.NavObjectClass.NavObjectClassName, navObject.DisplayName, position, _isAdd: true, navObject.usingLocalizationId, supplyCrate.entityId));
				}
			}
			else
			{
				navObject = NavObjectManager.Instance.RegisterNavObject("supply_drop", supplyCrate.blockPos);
				navObject.EntityID = supplyCrate.entityId;
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageNavObject>().Setup(navObject.NavObjectClass.NavObjectClassName, navObject.DisplayName, supplyCrate.blockPos, _isAdd: true, navObject.usingLocalizationId, supplyCrate.entityId));
			}
		}
	}

	public void AddSupplyCrate(int entityId)
	{
		foreach (SupplyCrateCache supplyCrate in supplyCrates)
		{
			if (supplyCrate.entityId == entityId)
			{
				return;
			}
		}
		supplyCrates.Add(new SupplyCrateCache(entityId, Vector3i.zero, requiresObserver: false));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddSupplyCrate(SupplyCrateCache scc)
	{
		foreach (SupplyCrateCache supplyCrate in supplyCrates)
		{
			if (supplyCrate.entityId == scc.entityId)
			{
				return;
			}
		}
		supplyCrates.Add(scc);
	}
}
