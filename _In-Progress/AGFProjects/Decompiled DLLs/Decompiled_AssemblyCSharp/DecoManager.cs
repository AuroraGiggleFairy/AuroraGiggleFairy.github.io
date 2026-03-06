using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DecoManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct SAddDecoInfo
	{
		public World world;

		public BlockValue bv;

		public Vector3i pos;

		public bool bForceBlockYPos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int FILEVERSION = 6;

	public const int cChunkSize = 128;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cUpdateDelay = 1f;

	public const int cUpdateCoMaxTimeUs = 900;

	public static Vector3 cDecoMiddleOffset = new Vector3(0.5f, 0f, 0.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static DecoManager m_Instance;

	public bool IsEnabled = true;

	public bool IsHidden;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, DecoChunk> decoChunks = new Dictionary<int, DecoChunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<DecoChunk> visibleDecoChunks = new List<DecoChunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<DecoObject> loadedDecos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int worldWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int worldHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public int worldWidthHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public int worldHeightHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public DecoOccupiedMap occupiedMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public FileBackedDecoOccupiedMap fileBackedOccupiedMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public IChunkProvider chunkProvider;

	[PublicizedFrom(EAccessModifier.Private)]
	public ITerrainGenerator terrainGenerator;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFixedSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public string filenamePath;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PrefabInstance> overridePOIList;

	[PublicizedFrom(EAccessModifier.Private)]
	public PerlinNoise resourceNoise;

	[PublicizedFrom(EAccessModifier.Private)]
	public int checkDelayTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream writeStream = new MemoryStream();

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.TaskInfo writeTask;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SAddDecoInfo> addDecosFromThread = new List<SAddDecoInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3i> removeDecosFromThread = new List<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Rect> resetDecosInWorldRectFromThread = new List<Rect>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> resetDecosForWorldChunkFromThread = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<DecoObject> decoWriteList = new List<DecoObject>(4096);

	[PublicizedFrom(EAccessModifier.Private)]
	public MicroStopwatch msUpdate = new MicroStopwatch();

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine updateCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityPlayer> playersToCheck = new List<EntityPlayer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> chunksAroundPlayers = new HashSet<int>();

	public static DecoManager Instance
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = new DecoManager();
			}
			return m_Instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DecoManager()
	{
	}

	public IEnumerator OnWorldLoaded(int _worldWidth, int _worldHeight, World _world, IChunkProvider _chunkProvider)
	{
		if (!IsEnabled)
		{
			yield break;
		}
		MicroStopwatch mswYields = new MicroStopwatch();
		world = _world;
		worldWidth = _worldWidth;
		worldHeight = _worldHeight;
		worldWidthHalf = worldWidth / 2;
		worldHeightHalf = worldHeight / 2;
		occupiedMap = new DecoOccupiedMap(worldWidth, worldHeight);
		bFixedSize = _world.ChunkClusters[0].IsFixedSize;
		chunkProvider = _chunkProvider;
		terrainGenerator = ((chunkProvider != null) ? _chunkProvider.GetTerrainGenerator() : null);
		resourceNoise = new PerlinNoise(_world.Seed);
		yield return null;
		if (chunkProvider != null)
		{
			yield return chunkProvider.FillOccupiedMap(worldWidth, worldHeight, occupiedMap, overridePOIList);
		}
		yield return null;
		int num = DecoChunk.ToDecoChunkPos(-worldWidth / 2);
		int num2 = DecoChunk.ToDecoChunkPos(worldWidth / 2);
		int num3 = DecoChunk.ToDecoChunkPos(-worldHeight / 2);
		int num4 = DecoChunk.ToDecoChunkPos(worldHeight / 2);
		decoChunks = new Dictionary<int, DecoChunk>((num2 - num + 1) * (num4 - num3 + 1));
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				DecoChunk value = new DecoChunk(i, j, i, j);
				decoChunks.Add(DecoChunk.MakeKey16(i, j), value);
			}
		}
		yield return null;
		filenamePath = GameIO.GetSaveGameDir() + "/decoration.7dt";
		bool fileLoaded = TryLoad();
		Log.Out("[DECO] read {0}", loadedDecos?.Count);
		yield return null;
		int chunkStartX = -(_worldWidth / 2) / 128;
		int chunkEndX = _worldWidth / 2 / 128;
		int chunkStartZ = -(_worldHeight / 2) / 128;
		int chunkEndZ = _worldHeight / 2 / 128;
		int totalDecorated = 0;
		mswYields.ResetAndRestart();
		if (PlatformOptimizations.FileBackedArrays)
		{
			fileBackedOccupiedMap = new FileBackedDecoOccupiedMap(_worldWidth, _worldHeight);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !fileLoaded)
		{
			Log.Out("DecoManager chunks");
			for (int z = chunkStartZ; z <= chunkEndZ; z++)
			{
				for (int k = chunkStartX; k <= chunkEndX; k++)
				{
					GameRandom gameRandom = Utils.RandomFromSeedOnPos(k, z, world.Seed);
					DecoChunk decoChunk = decoChunks[DecoChunk.MakeKey16(k, z)];
					totalDecorated += decorateChunkRandom(decoChunk, gameRandom);
					GameRandomManager.Instance.FreeGameRandom(gameRandom);
				}
				if (mswYields.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					mswYields.ResetAndRestart();
				}
				if (PlatformOptimizations.FileBackedArrays && z > chunkStartZ)
				{
					fileBackedOccupiedMap.CopyDecoChunkRow(z - 1, occupiedMap.GetData());
				}
			}
			if (PlatformOptimizations.FileBackedArrays)
			{
				occupiedMap = null;
			}
		}
		yield return null;
		if (loadedDecos != null)
		{
			foreach (DecoObject loadedDeco in loadedDecos)
			{
				addLoadedDecoration(loadedDeco);
			}
			loadedDecos = null;
			for (int l = chunkStartZ; l <= chunkEndZ; l++)
			{
				for (int m = chunkStartX; m <= chunkEndX; m++)
				{
					decoChunks[DecoChunk.MakeKey16(m, l)].isDecorated = true;
				}
				if (PlatformOptimizations.FileBackedArrays && l > chunkStartZ)
				{
					fileBackedOccupiedMap.CopyDecoChunkRow(l - 1, occupiedMap.GetData());
				}
			}
			if (PlatformOptimizations.FileBackedArrays)
			{
				occupiedMap = null;
			}
		}
		bDirty = true;
	}

	public void OriginChanged(Vector3 _offset)
	{
		foreach (KeyValuePair<int, DecoChunk> decoChunk in decoChunks)
		{
			GameObject rootObj = decoChunk.Value.rootObj;
			if ((bool)rootObj)
			{
				rootObj.transform.position += _offset;
			}
		}
	}

	public void OnWorldUnloaded()
	{
		if (!IsEnabled)
		{
			return;
		}
		if (updateCoroutine != null)
		{
			ThreadManager.StopCoroutine(updateCoroutine);
			updateCoroutine = null;
		}
		foreach (KeyValuePair<int, DecoChunk> decoChunk in decoChunks)
		{
			decoChunk.Value.Destroy();
		}
		occupiedMap = null;
		if (PlatformOptimizations.FileBackedArrays)
		{
			fileBackedOccupiedMap?.Dispose();
			fileBackedOccupiedMap = null;
		}
		overridePOIList = null;
		checkDelayTicks = 0;
		decoChunks.Clear();
		visibleDecoChunks.Clear();
		loadedDecos = null;
		addDecosFromThread.Clear();
		removeDecosFromThread.Clear();
		if (writeTask != null)
		{
			writeTask.WaitForEnd();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryLoad()
	{
		if (GameManager.Instance.IsEditMode() || !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || !SdFile.Exists(filenamePath))
		{
			return false;
		}
		using (Stream baseStream = SdFile.OpenRead(filenamePath))
		{
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader.SetBaseStream(baseStream);
			byte b = pooledBinaryReader.ReadByte();
			if (b == 6)
			{
				Read(pooledBinaryReader, b);
				return true;
			}
			Log.Warning($"Saved decoration data is out of date. Saved version is ({b}). Current version is ({6}). " + "Decorations will be regenerated for this map, but it is recommended to start a new game.");
		}
		SdFile.Delete(filenamePath);
		return false;
	}

	public void Save()
	{
		if (IsEnabled && !GameManager.Instance.IsEditMode() && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && bDirty)
		{
			if (writeTask != null)
			{
				writeTask.WaitForEnd();
			}
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
			{
				writeStream.Position = 0L;
				pooledBinaryWriter.SetBaseStream(writeStream);
				pooledBinaryWriter.Write((byte)6);
				Write(pooledBinaryWriter, Block.nameIdMapping);
			}
			writeTask = ThreadManager.AddSingleTask(WriteTask);
			bDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteTask(ThreadManager.TaskInfo _taskInfo)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch();
		using (Stream destination = SdFile.Open(filenamePath, FileMode.Create, FileAccess.Write, FileShare.Read))
		{
			writeStream.Position = 0L;
			StreamUtils.StreamCopy(writeStream, destination);
			writeStream.SetLength(0L);
		}
		Log.Out($"[DECO] write thread {microStopwatch.ElapsedMilliseconds}ms");
	}

	public void Read(BinaryReader _br, int _version = int.MaxValue, bool _resetExisting = true)
	{
		if (_resetExisting)
		{
			loadedDecos = new HashSet<DecoObject>();
		}
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			DecoObject decoObject = new DecoObject();
			decoObject.Read(_br);
			loadedDecos.Add(decoObject);
		}
	}

	public void Write(BinaryWriter _bw, NameIdMapping _blockMap)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch();
		int num = 0;
		lock (decoWriteList)
		{
			GenerateDecoWriteList();
			num = decoWriteList.Count;
			_bw.Write(num);
			for (int i = 0; i < num; i++)
			{
				decoWriteList[i].Write(_bw, _blockMap);
			}
			decoWriteList.Clear();
		}
		Log.Out($"[DECO] written {num}, in {microStopwatch.ElapsedMilliseconds}ms");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GenerateDecoWriteList()
	{
		lock (decoWriteList)
		{
			decoWriteList.Clear();
			foreach (KeyValuePair<int, DecoChunk> decoChunk in decoChunks)
			{
				foreach (KeyValuePair<long, List<DecoObject>> decosPerSmallChunk in decoChunk.Value.decosPerSmallChunks)
				{
					List<DecoObject> value = decosPerSmallChunk.Value;
					for (int i = 0; i < value.Count; i++)
					{
						decoWriteList.Add(value[i]);
					}
				}
			}
		}
	}

	public void UpdateTick(World _world)
	{
		if (!IsEnabled)
		{
			return;
		}
		checkDelayTicks--;
		if (updateCoroutine != null)
		{
			return;
		}
		if (addDecosFromThread.Count > 0)
		{
			checkDelayTicks = 0;
			lock (addDecosFromThread)
			{
				for (int i = 0; i < addDecosFromThread.Count; i++)
				{
					AddDecorationAt(addDecosFromThread[i].world, addDecosFromThread[i].bv, addDecosFromThread[i].pos, addDecosFromThread[i].bForceBlockYPos);
				}
				addDecosFromThread.Clear();
			}
		}
		if (removeDecosFromThread.Count > 0)
		{
			checkDelayTicks = 0;
			lock (removeDecosFromThread)
			{
				for (int j = 0; j < removeDecosFromThread.Count; j++)
				{
					RemoveDecorationAt(removeDecosFromThread[j]);
				}
				removeDecosFromThread.Clear();
			}
		}
		if (resetDecosInWorldRectFromThread.Count > 0)
		{
			checkDelayTicks = 0;
			lock (resetDecosInWorldRectFromThread)
			{
				for (int k = 0; k < resetDecosInWorldRectFromThread.Count; k++)
				{
					ResetDecosInWorldRect(resetDecosInWorldRectFromThread[k]);
				}
				resetDecosInWorldRectFromThread.Clear();
			}
		}
		if (resetDecosForWorldChunkFromThread.Count > 0)
		{
			checkDelayTicks = 0;
			lock (resetDecosForWorldChunkFromThread)
			{
				for (int l = 0; l < resetDecosForWorldChunkFromThread.Count; l++)
				{
					ResetDecosForWorldChunk(resetDecosForWorldChunkFromThread[l]);
				}
				resetDecosForWorldChunkFromThread.Clear();
			}
		}
		if (checkDelayTicks > 0)
		{
			return;
		}
		checkDelayTicks = 20;
		new MicroStopwatch();
		playersToCheck.Clear();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			playersToCheck.AddRange(_world.Players.list);
		}
		else
		{
			EntityPlayerLocal primaryPlayer = _world.GetPrimaryPlayer();
			if (primaryPlayer == null)
			{
				return;
			}
			playersToCheck.Add(primaryPlayer);
		}
		if (IsHidden)
		{
			playersToCheck.Clear();
		}
		chunksAroundPlayers.Clear();
		for (int num = playersToCheck.Count - 1; num >= 0; num--)
		{
			EntityPlayer entityPlayer = playersToCheck[num];
			Vector3i blockPosition = entityPlayer.GetBlockPosition();
			int num2 = DecoChunk.ToDecoChunkPos(blockPosition.x);
			int num3 = DecoChunk.ToDecoChunkPos(blockPosition.z);
			int num4 = (entityPlayer.isEntityRemote ? 1 : GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTreeDistance));
			int num5 = num2 - num4;
			int num6 = num2 + num4;
			int num7 = num3 - num4;
			int num8 = num3 + num4;
			for (int m = num5; m <= num6; m++)
			{
				for (int n = num7; n <= num8; n++)
				{
					chunksAroundPlayers.Add(DecoChunk.MakeKey16(m, n));
				}
			}
		}
		updateCoroutine = ThreadManager.StartCoroutine(UpdateDecorationsCo());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator UpdateDecorationsCo()
	{
		int waitCount = 0;
		msUpdate.ResetAndRestart();
		DecoChunk decoChunk;
		for (int i = visibleDecoChunks.Count - 1; i >= 0; i--)
		{
			decoChunk = visibleDecoChunks[i];
			bool flag = chunksAroundPlayers.Contains(DecoChunk.MakeKey16(decoChunk.decoChunkX, decoChunk.decoChunkZ));
			decoChunk.SetVisible(flag);
			if (!flag)
			{
				decoChunk.Destroy();
			}
			if (msUpdate.ElapsedMicroseconds > 900)
			{
				waitCount++;
				yield return null;
				msUpdate.ResetAndRestart();
			}
		}
		visibleDecoChunks.Clear();
		foreach (int chunksAroundPlayer in chunksAroundPlayers)
		{
			decoChunks.TryGetValue(chunksAroundPlayer, out decoChunk);
			if (decoChunk == null)
			{
				continue;
			}
			visibleDecoChunks.Add(decoChunk);
			lock (decoChunk)
			{
				if (!decoChunk.isDecorated)
				{
					GameRandom gameRandom = Utils.RandomFromSeedOnPos(decoChunk.decoChunkX, decoChunk.decoChunkZ, world.Seed);
					decorateChunkRandom(decoChunk, gameRandom);
					GameRandomManager.Instance.FreeGameRandom(gameRandom);
				}
			}
			if (msUpdate.ElapsedMicroseconds > 900)
			{
				waitCount++;
				yield return null;
				msUpdate.ResetAndRestart();
			}
			if (decoChunk.decosPerSmallChunks.Count > 0 && !decoChunk.isGameObjectUpdated)
			{
				decoChunk.UpdateGameObject();
			}
			if (!decoChunk.isModelsUpdated)
			{
				yield return decoChunk.UpdateModels(world, msUpdate);
				if (msUpdate.ElapsedMicroseconds > 900)
				{
					waitCount++;
					yield return null;
					msUpdate.ResetAndRestart();
				}
			}
		}
		updateCoroutine = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DecoObject GetDecoObject()
	{
		return new DecoObject();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DecoChunk GetDecoChunkAt(int _x, int _z)
	{
		int key = DecoChunk.MakeKey16(_x, _z);
		if (!decoChunks.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int decorateChunkRandom(DecoChunk _decoChunk, GameRandom rnd)
	{
		if (bFixedSize)
		{
			_decoChunk.isDecorated = true;
			return 0;
		}
		if (chunkProvider == null)
		{
			return 0;
		}
		IBiomeProvider biomeProvider = chunkProvider.GetBiomeProvider();
		if (biomeProvider == null)
		{
			return 0;
		}
		int num = _decoChunk.decoChunkX * 128;
		int num2 = num + 128;
		int num3 = _decoChunk.decoChunkZ * 128;
		int num4 = num3 + 128;
		int num5 = 0;
		for (int i = 0; i < 1000; i++)
		{
			int num6 = rnd.RandomRange(num, num2 - 1);
			int num7 = rnd.RandomRange(num3, num4 - 1);
			if ((int)occupiedMap.Get(num6, num7) > 2 || occupiedMap.CheckArea(num6 - 2, num7 - 2, EnumDecoOccupied.POI, 5, 5))
			{
				continue;
			}
			BiomeDefinition biomeOrSubAt = biomeProvider.GetBiomeOrSubAt(num6, num7);
			if (biomeOrSubAt == null)
			{
				continue;
			}
			float num8 = -1f;
			BlockValue blockValue = BlockValue.Air;
			int num9 = 3;
			for (int num10 = biomeOrSubAt.m_DistantDecoBlocks.Count - 1; num10 >= 0; num10--)
			{
				BiomeBlockDecoration biomeBlockDecoration = biomeOrSubAt.m_DistantDecoBlocks[num10];
				if (rnd.RandomFloat > biomeBlockDecoration.prob * 0.125f * 16f)
				{
					continue;
				}
				if (biomeBlockDecoration.checkResourceOffsetY < int.MaxValue)
				{
					if (num8 < 0f)
					{
						num8 = terrainGenerator.GetTerrainHeightAt(num6, num7) + 1f;
					}
					if (!GameUtils.CheckOreNoiseAt(resourceNoise, num6, (int)num8 + biomeBlockDecoration.checkResourceOffsetY, num7))
					{
						continue;
					}
				}
				blockValue = biomeBlockDecoration.blockValues[0];
				num9 = biomeBlockDecoration.randomRotateMax;
				break;
			}
			Block block;
			if (blockValue.isair || (block = blockValue.Block) == null || !block.IsDistantDecoration)
			{
				continue;
			}
			BlockValue bv = new BlockValue((uint)blockValue.type);
			bv.rotation = BiomeBlockDecoration.GetRandomRotation(rnd.RandomFloat, (block.isMultiBlock && num9 > 3) ? 3 : num9);
			if (TryAddToOccupiedMap(block, num6, num7, bv.rotation, enableStopBigDecoCheck: true))
			{
				if (num8 < 0f)
				{
					num8 = terrainGenerator.GetTerrainHeightAt(num6, num7) + 1f;
				}
				int y = (int)(num8 + 0.5f);
				DecoObject decoObject = GetDecoObject();
				decoObject.Init(new Vector3i(num6, y, num7), num8, bv, DecoState.GeneratedActive);
				_decoChunk.AddDecoObject(decoObject);
				bDirty = true;
				num5++;
			}
		}
		_decoChunk.isDecorated = true;
		return num5;
	}

	public void GetDecorationsOnChunk(int _chunkX, int _chunkZ, List<SBlockPosValue> _multiBlockList)
	{
		int num = 0;
		try
		{
			_multiBlockList.Clear();
			num = 1;
			if (!IsEnabled)
			{
				return;
			}
			int x = DecoChunk.ToDecoChunkPos(_chunkX * 16);
			int z = DecoChunk.ToDecoChunkPos(_chunkZ * 16);
			if (!decoChunks.TryGetValue(DecoChunk.MakeKey16(x, z), out var value))
			{
				return;
			}
			num = 2;
			lock (value)
			{
				if (!value.isDecorated)
				{
					Log.Error("Decorating chunk, should not happen at this point!");
					GameRandom gameRandom = Utils.RandomFromSeedOnPos(value.decoChunkX, value.decoChunkZ, world.Seed);
					decorateChunkRandom(value, gameRandom);
					GameRandomManager.Instance.FreeGameRandom(gameRandom);
				}
			}
			num = 3;
			if (!value.decosPerSmallChunks.TryGetValue(WorldChunkCache.MakeChunkKey(_chunkX, _chunkZ), out var value2))
			{
				return;
			}
			num = 4;
			for (int i = 0; i < value2.Count; i++)
			{
				DecoObject decoObject = value2[i];
				if (decoObject == null)
				{
					Log.Warning("DecoManager decosInChunk #{0} null at {1}, {2}", i, _chunkX, _chunkZ);
				}
				else if (decoObject.state != DecoState.GeneratedInactive)
				{
					_multiBlockList.Add(new SBlockPosValue(new Vector3i(decoObject.pos.x, decoObject.pos.y, decoObject.pos.z), decoObject.bv));
				}
			}
		}
		catch (NullReferenceException ex)
		{
			Log.Error("Exception position: " + num);
			throw ex;
		}
	}

	public bool GetParentBlockOfDecoration(Transform _t, out Vector3i _blockPos, out DecoObject _decoObject)
	{
		_blockPos = Vector3i.zero;
		_decoObject = null;
		if (!IsEnabled)
		{
			return false;
		}
		Transform transform = RootTransformRefParent.FindRoot(_t);
		Vector3 position = transform.position;
		int num = Utils.Fastfloor(position.x - cDecoMiddleOffset.x + Origin.position.x);
		int num2 = Utils.Fastfloor(position.z - cDecoMiddleOffset.z + Origin.position.z);
		if (!decoChunks.TryGetValue(DecoChunk.MakeKey16(DecoChunk.ToDecoChunkPos(num), DecoChunk.ToDecoChunkPos(num2)), out var value))
		{
			return false;
		}
		if (!value.isModelsUpdated)
		{
			return false;
		}
		if (!value.decosPerSmallChunks.TryGetValue(WorldChunkCache.MakeChunkKey(World.toChunkXZ(num), World.toChunkXZ(num2)), out var value2))
		{
			return false;
		}
		for (int num3 = value2.Count - 1; num3 >= 0; num3--)
		{
			DecoObject decoObject = value2[num3];
			if (decoObject.state != DecoState.GeneratedInactive && (bool)decoObject.go && decoObject.go.transform == transform)
			{
				_decoObject = decoObject;
				_blockPos = decoObject.pos;
				return true;
			}
		}
		return false;
	}

	public Transform GetDecorationTransform(Vector3i _blockPos, bool _bDetachTransform = false)
	{
		if (!IsEnabled)
		{
			return null;
		}
		if (!decoChunks.TryGetValue(DecoChunk.MakeKey16(DecoChunk.ToDecoChunkPos(_blockPos.x), DecoChunk.ToDecoChunkPos(_blockPos.z)), out var value))
		{
			return null;
		}
		if (!value.decosPerSmallChunks.TryGetValue(WorldChunkCache.MakeChunkKey(World.toChunkXZ(_blockPos.x), World.toChunkXZ(_blockPos.z)), out var value2))
		{
			return null;
		}
		for (int num = value2.Count - 1; num >= 0; num--)
		{
			DecoObject decoObject = value2[num];
			if (decoObject.state != DecoState.GeneratedInactive && !(decoObject.pos != _blockPos))
			{
				if (decoObject.go == null)
				{
					return null;
				}
				Transform transform = decoObject.go.transform;
				if (_bDetachTransform)
				{
					if (OcclusionManager.Instance.cullDecorations)
					{
						OcclusionManager.Instance.RemoveDeco(value, decoObject.go.transform);
					}
					decoObject.go = null;
				}
				return transform;
			}
		}
		return null;
	}

	public void AddDecorationAt(World _world, BlockValue _blockValue, Vector3i _blockPos, bool _bForceBlockYPos = false)
	{
		if (!ThreadManager.IsMainThread())
		{
			lock (addDecosFromThread)
			{
				addDecosFromThread.Add(new SAddDecoInfo
				{
					world = _world,
					bv = _blockValue,
					pos = _blockPos,
					bForceBlockYPos = _bForceBlockYPos
				});
				return;
			}
		}
		float num = _blockPos.y;
		if (!_bForceBlockYPos && _blockPos.y > 0 && terrainGenerator != null)
		{
			BlockValue block = world.GetBlock(_blockPos - new Vector3i(0, 1, 0));
			if (block.Block != null && block.Block.shape.IsTerrain())
			{
				num = terrainGenerator.GetTerrainHeightAt(_blockPos.x, _blockPos.z) + 1f;
			}
		}
		bDirty = true;
		DecoChunk decoChunkAt = GetDecoChunkAt(DecoChunk.ToDecoChunkPos(_blockPos.x), DecoChunk.ToDecoChunkPos(_blockPos.z));
		if (decoChunkAt != null)
		{
			DecoObject decoObjectAt = decoChunkAt.GetDecoObjectAt(_blockPos);
			if (decoObjectAt != null)
			{
				if (decoObjectAt.realYPos == num && decoObjectAt.bv.Equals(_blockValue) && decoObjectAt.bv.rotation == _blockValue.rotation)
				{
					return;
				}
				decoChunkAt.RemoveDecoObject(decoObjectAt);
			}
		}
		DecoObject decoObject = GetDecoObject();
		decoObject.Init(_blockPos, num, _blockValue, DecoState.Dynamic);
		if (decoChunkAt == null)
		{
			if (loadedDecos == null)
			{
				loadedDecos = new HashSet<DecoObject>();
			}
			loadedDecos.Add(decoObject);
		}
		else
		{
			decoChunkAt.AddDecoObject(decoObject, _tryInstantiate: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addLoadedDecoration(DecoObject _decoObject)
	{
		if (!IsEnabled)
		{
			return;
		}
		DecoChunk decoChunkAt = GetDecoChunkAt(DecoChunk.ToDecoChunkPos(_decoObject.pos.x), DecoChunk.ToDecoChunkPos(_decoObject.pos.z));
		if (decoChunkAt != null)
		{
			decoChunkAt.AddDecoObject(_decoObject);
			if (_decoObject.state != DecoState.Dynamic)
			{
				TryAddToOccupiedMap(_decoObject.bv.Block, _decoObject.pos.x, _decoObject.pos.z, _decoObject.bv.rotation, enableStopBigDecoCheck: false);
			}
			bDirty = true;
		}
	}

	public static int CheckPosition(int worldWidth, int worldHeight, int _x, int _z)
	{
		int num = worldWidth / 2;
		int num2 = worldHeight / 2;
		if (_x < -num || _x >= num || _z < -num2 || _z >= num2)
		{
			return -1;
		}
		return _x + num + (_z + num2) * worldWidth;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryAddToOccupiedMap(Block block, int xWorld, int zWorld, byte rotationByte, bool enableStopBigDecoCheck)
	{
		int num = xWorld;
		int num2 = zWorld;
		int num3 = 1;
		int num4 = 1;
		if (!block.isMultiBlock)
		{
			occupiedMap.Set(xWorld, zWorld, EnumDecoOccupied.Deco);
		}
		else
		{
			switch (rotationByte)
			{
			default:
				num3 = block.multiBlockPos.dim.x;
				num4 = block.multiBlockPos.dim.z;
				break;
			case 1:
				num3 = block.multiBlockPos.dim.z;
				num4 = block.multiBlockPos.dim.x;
				break;
			case 2:
				num3 = block.multiBlockPos.dim.x;
				num4 = block.multiBlockPos.dim.z;
				break;
			case 3:
				num3 = block.multiBlockPos.dim.z;
				num4 = block.multiBlockPos.dim.x;
				break;
			}
			num = ((num3 % 2 == 0) ? (xWorld - num3 / 2 + 1) : (xWorld - num3 / 2));
			num2 = ((num4 % 2 == 0) ? (zWorld - num4 / 2 + 1) : (zWorld - num4 / 2));
			if (enableStopBigDecoCheck && occupiedMap.CheckArea(num, num2, EnumDecoOccupied.Stop_BigDeco, num3, num4))
			{
				return false;
			}
			occupiedMap.SetArea(num, num2, EnumDecoOccupied.Deco, num3, num4);
		}
		if (block.BigDecorationRadius > 0)
		{
			occupiedMap.SetArea(num - block.BigDecorationRadius, num2 - block.BigDecorationRadius, EnumDecoOccupied.Perimeter, block.BigDecorationRadius * 2 + num3, block.BigDecorationRadius * 2 + num4);
		}
		return true;
	}

	public bool RemoveDecorationAt(Vector3i _blockPos)
	{
		if (!IsEnabled)
		{
			return false;
		}
		int key = DecoChunk.MakeKey16(DecoChunk.ToDecoChunkPos(_blockPos.x), DecoChunk.ToDecoChunkPos(_blockPos.z));
		if (!decoChunks.TryGetValue(key, out var value))
		{
			return false;
		}
		if (!ThreadManager.IsMainThread())
		{
			lock (removeDecosFromThread)
			{
				removeDecosFromThread.Add(_blockPos);
				return true;
			}
		}
		bDirty = true;
		return value.RemoveDecoObject(_blockPos);
	}

	public EnumDecoOccupied GetDecoOccupiedAt(int _x, int _z)
	{
		if (!IsEnabled)
		{
			return EnumDecoOccupied.Free;
		}
		if (occupiedMap == null && !PlatformOptimizations.FileBackedArrays)
		{
			return EnumDecoOccupied.NoneAllowed;
		}
		int offs;
		if ((offs = CheckPosition(worldWidth, worldHeight, _x, _z)) < 0)
		{
			return EnumDecoOccupied.NoneAllowed;
		}
		if (!decoChunks.TryGetValue(DecoChunk.MakeKey16(DecoChunk.ToDecoChunkPos(_x), DecoChunk.ToDecoChunkPos(_z)), out var value))
		{
			return EnumDecoOccupied.NoneAllowed;
		}
		if (!value.isDecorated)
		{
			lock (value)
			{
				if (!value.isDecorated)
				{
					Log.Error("Should not be decorating here!");
					GameRandom gameRandom = Utils.RandomFromSeedOnPos(value.decoChunkX, value.decoChunkZ, world.Seed);
					decorateChunkRandom(value, gameRandom);
					GameRandomManager.Instance.FreeGameRandom(gameRandom);
				}
			}
		}
		if (PlatformOptimizations.FileBackedArrays)
		{
			return fileBackedOccupiedMap.Get(offs);
		}
		return occupiedMap.Get(offs);
	}

	public void SetChunkDistance(int _distance)
	{
		GamePrefs.Set(EnumGamePrefs.OptionsGfxTreeDistance, _distance);
	}

	public void SetBlock(World _world, Vector3i _blockPos, BlockValue _bv)
	{
		if (_bv.isair)
		{
			RemoveDecorationAt(_blockPos);
			return;
		}
		RemoveDecorationAt(_blockPos);
		AddDecorationAt(_world, _bv, _blockPos, _bForceBlockYPos: true);
	}

	public void SaveDebugTexture(string path, bool _includeFlatAreas = false)
	{
		if (PlatformOptimizations.FileBackedArrays)
		{
			fileBackedOccupiedMap.SaveAsTexture(path, includeFlatAreas: true);
		}
		else
		{
			occupiedMap?.SaveAsTexture(path, _includeFlatAreas);
		}
	}

	public void SaveDebugTexture(string path, List<FlatArea> flatAreas)
	{
		if (PlatformOptimizations.FileBackedArrays)
		{
			fileBackedOccupiedMap.SaveAsTexture(path, includeFlatAreas: true, flatAreas);
		}
		else
		{
			occupiedMap?.SaveAsTexture(path, includeFlatAreas: true, flatAreas);
		}
	}

	public void SaveStateDebugTexture(string _filename)
	{
		Color32[] array = new Color32[worldWidth * worldHeight];
		int num = -worldWidth / 2;
		int num2 = -worldHeight / 2;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Color.black;
			int num3 = i % worldWidth + num;
			int num4 = i / worldWidth + num2;
			int key = DecoChunk.MakeKey16(DecoChunk.ToDecoChunkPos(num3), DecoChunk.ToDecoChunkPos(num4));
			if (!decoChunks.TryGetValue(key, out var value))
			{
				continue;
			}
			long key2 = WorldChunkCache.MakeChunkKey(World.toChunkXZ(num3), World.toChunkXZ(num4));
			if (!value.decosPerSmallChunks.TryGetValue(key2, out var value2))
			{
				continue;
			}
			foreach (DecoObject item in value2)
			{
				if (item.pos.x == num3 && item.pos.z == num4)
				{
					switch (item.state)
					{
					case DecoState.GeneratedActive:
						array[i] = Color.green;
						break;
					case DecoState.GeneratedInactive:
						array[i] = Color.yellow;
						break;
					case DecoState.Dynamic:
						array[i] = Color.blue;
						break;
					}
					break;
				}
			}
		}
		Texture2D texture2D = new Texture2D(worldWidth, worldHeight);
		texture2D.SetPixels32(array);
		texture2D.Apply();
		TextureUtils.SaveTexture(texture2D, _filename);
		UnityEngine.Object.Destroy(texture2D);
	}

	public void SendDecosToClient(ClientInfo _cInfo)
	{
		lock (decoWriteList)
		{
			GenerateDecoWriteList();
			int _currentIndex = 0;
			while (_currentIndex < decoWriteList.Count)
			{
				_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageDecoUpdate>().Setup(decoWriteList, ref _currentIndex));
			}
		}
	}

	public void ResetDecosInWorldRect(Rect worldRect)
	{
		if (!ThreadManager.IsMainThread())
		{
			lock (resetDecosInWorldRectFromThread)
			{
				resetDecosInWorldRectFromThread.Add(worldRect);
				return;
			}
		}
		bDirty = true;
		int num = DecoChunk.ToDecoChunkPos(worldRect.x);
		int num2 = DecoChunk.ToDecoChunkPos(worldRect.y);
		int num3 = DecoChunk.ToDecoChunkPos(worldRect.xMax);
		int num4 = DecoChunk.ToDecoChunkPos(worldRect.yMax);
		for (int i = num; i <= num3; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				int key = DecoChunk.MakeKey16(i, j);
				if (decoChunks.TryGetValue(key, out var value))
				{
					value.RestoreGeneratedDecos([PublicizedFrom(EAccessModifier.Internal)] (DecoObject deco) => worldRect.Contains(new Vector2(deco.pos.x, deco.pos.z)));
				}
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageDecoResetWorldRect>().Setup(worldRect));
		}
	}

	public void ResetDecosForWorldChunk(long worldChunkKey)
	{
		if (!ThreadManager.IsMainThread())
		{
			lock (resetDecosForWorldChunkFromThread)
			{
				resetDecosForWorldChunkFromThread.Add(worldChunkKey);
				return;
			}
		}
		bDirty = true;
		int x = DecoChunk.ToDecoChunkPos(WorldChunkCache.extractX(worldChunkKey) * 16);
		int z = DecoChunk.ToDecoChunkPos(WorldChunkCache.extractZ(worldChunkKey) * 16);
		int key = DecoChunk.MakeKey16(x, z);
		if (decoChunks.TryGetValue(key, out var value))
		{
			value.RestoreGeneratedDecos(worldChunkKey, [PublicizedFrom(EAccessModifier.Internal)] (DecoObject decoObject) => true);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageDecoResetWorldChunk>().Setup(worldChunkKey));
		}
	}

	public EnumDecoOccupied GetDecoOccupiedFromMap(int xWorld, int zWorld)
	{
		if (PlatformOptimizations.FileBackedArrays)
		{
			int num = CheckPosition(worldWidth, worldHeight, xWorld, zWorld);
			if (num < 0)
			{
				return EnumDecoOccupied.NoneAllowed;
			}
			return fileBackedOccupiedMap.Get(num);
		}
		return occupiedMap.Get(xWorld, zWorld);
	}

	public void ResetAll()
	{
		Rect worldRect = new Rect(-worldWidth / 2, -worldHeight / 2, worldWidth, worldHeight);
		ResetDecosInWorldRect(worldRect);
	}
}
