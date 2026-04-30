using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Platform;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace WorldGenerationEngineFinal;

[BurstCompile(CompileSynchronously = true)]
public class WorldBuilder
{
	public enum BiomeLayout
	{
		CenterForest,
		CenterWasteland,
		Circle,
		Line
	}

	public struct Data
	{
		public int WorldSize;

		public NativeArray<float> HeightMap;

		public NativeArray<float> waterDest;

		public int PathTileGridWidth;

		public NativeArray<PathTile> PathTileGrid;

		public int StreetTileDataGridWidth;

		public NativeArray<StreetTileData> StreetTileDataGrid;

		public NativeArray<byte> poiHeightMask;

		public volatile int messageCnt;

		public void Init(int _worldSize)
		{
			WorldSize = _worldSize;
			int length = WorldSize * WorldSize;
			HeightMap = new NativeArray<float>(length, Allocator.Persistent);
			waterDest = new NativeArray<float>(length, Allocator.Persistent);
			PathTileGridWidth = WorldSize / 10;
			PathTileGrid = new NativeArray<PathTile>(PathTileGridWidth * PathTileGridWidth, Allocator.Persistent);
			StreetTileDataGridWidth = WorldSize / 150;
		}

		public void Cleanup()
		{
			HeightMap.Dispose();
			PathTileGrid.Dispose();
			StreetTileDataGrid.Dispose();
			poiHeightMask.Dispose();
			waterDest.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetHeight(Vector2i pos)
		{
			return GetHeight(pos.x, pos.y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetHeight(int x, int y)
		{
			if ((uint)x >= WorldSize || (uint)y >= WorldSize)
			{
				return 0f;
			}
			return HeightMap[x + y * WorldSize];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool InWorldBounds(int x, int y)
		{
			if ((uint)x < WorldSize)
			{
				return (uint)y < WorldSize;
			}
			return false;
		}

		public unsafe ref StreetTileData GetStreetTileDataWorld(int x, int y)
		{
			x /= 150;
			y /= 150;
			if ((uint)x >= StreetTileDataGridWidth || (uint)y >= StreetTileDataGridWidth)
			{
				x = 0;
				y = 0;
			}
			return ref UnsafeUtility.ArrayElementAsRef<StreetTileData>(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(StreetTileDataGrid), x + y * StreetTileDataGridWidth);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte GetWater(int x, int y)
		{
			if ((uint)x >= WorldSize || (uint)y >= WorldSize)
			{
				return 0;
			}
			return (byte)waterDest[x + y * WorldSize];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte GetWater(int _index)
		{
			if ((uint)_index >= WorldSize * WorldSize)
			{
				return 0;
			}
			return (byte)waterDest[_index];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class BiomeTypeData
	{
		public BiomeType Type;

		public float Percent;

		public int TileCount;

		public Vector2i Center;

		public BiomeTypeData(BiomeType _type, float _percent, int _totalTiles)
		{
			Type = _type;
			Percent = _percent;
			TileCount = Mathf.FloorToInt(_percent * (float)_totalTiles);
			if (Percent > 0f && TileCount == 0)
			{
				TileCount = 1;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct PreviewPoint
	{
		public Vector2i pos;

		public Color32 color;

		public int size;
	}

	public enum GenerationSelections
	{
		None,
		Few,
		Default,
		Many
	}

	public struct PlayerSpawn(Vector3 _position, float _yRotation)
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public const int cSafeDist = 60;

		public Vector3 Position = _position;

		public float Rotation = _yRotation;

		public bool IsTooClose(Vector3 _position)
		{
			float num = _position.x - Position.x;
			float num2 = _position.z - Position.z;
			return num * num + num2 * num2 < 3600f;
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public delegate void ClearWaterUnderTerrain_0000A3CE_0024PostfixBurstDelegate(ref Data _data, ref NativeArray<float> _terrain, int startX, int endX, int startY, int endY);

	[PublicizedFrom(EAccessModifier.Internal)]
	public static class ClearWaterUnderTerrain_0000A3CE_0024BurstDirectCall
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr Pointer;

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr DeferredCompilation;

		[BurstDiscard]
		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(ClearWaterUnderTerrain_0000A3CE_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		static ClearWaterUnderTerrain_0000A3CE_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(ref Data _data, ref NativeArray<float> _terrain, int startX, int endX, int startY, int endY)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Data, ref NativeArray<float>, int, int, int, int, void>)functionPointer)(ref _data, ref _terrain, startX, endX, startY, endY);
					return;
				}
			}
			ClearWaterUnderTerrain_0024BurstManaged(ref _data, ref _terrain, startX, endX, startY, endY);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public delegate void FinalizeWater_0000A3D1_0024PostfixBurstDelegate(ref Data _data, float _WaterHeight);

	[PublicizedFrom(EAccessModifier.Internal)]
	public static class FinalizeWater_0000A3D1_0024BurstDirectCall
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr Pointer;

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr DeferredCompilation;

		[BurstDiscard]
		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(FinalizeWater_0000A3D1_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		static FinalizeWater_0000A3D1_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(ref Data _data, float _WaterHeight)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Data, float, void>)functionPointer)(ref _data, _WaterHeight);
					return;
				}
			}
			FinalizeWater_0024BurstManaged(ref _data, _WaterHeight);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public delegate void SmoothRoadTerrainTask_0000A3D8_0024PostfixBurstDelegate(ref Data _data, ref NativeArray<Color32> roadMask, ref NativeArray<float> _heightMap, int WorldSize);

	[PublicizedFrom(EAccessModifier.Internal)]
	public static class SmoothRoadTerrainTask_0000A3D8_0024BurstDirectCall
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr Pointer;

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr DeferredCompilation;

		[BurstDiscard]
		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(SmoothRoadTerrainTask_0000A3D8_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		static SmoothRoadTerrainTask_0000A3D8_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(ref Data _data, ref NativeArray<Color32> roadMask, ref NativeArray<float> _heightMap, int WorldSize)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Data, ref NativeArray<Color32>, ref NativeArray<float>, int, void>)functionPointer)(ref _data, ref roadMask, ref _heightMap, WorldSize);
					return;
				}
			}
			SmoothRoadTerrainTask_0024BurstManaged(ref _data, ref roadMask, ref _heightMap, WorldSize);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public delegate void AdjustHeights_0000A3EA_0024PostfixBurstDelegate(ref NativeArray<float> _src, float _min);

	[PublicizedFrom(EAccessModifier.Internal)]
	public static class AdjustHeights_0000A3EA_0024BurstDirectCall
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr Pointer;

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr DeferredCompilation;

		[BurstDiscard]
		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(AdjustHeights_0000A3EA_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		static AdjustHeights_0000A3EA_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(ref NativeArray<float> _src, float _min)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<float>, float, void>)functionPointer)(ref _src, _min);
					return;
				}
			}
			AdjustHeights_0024BurstManaged(ref _src, _min);
		}
	}

	public const int HeightMax = 255;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int groundHeight = 35;

	public int WaterHeight = 30;

	public readonly DistrictPlanner DistrictPlanner;

	public readonly HighwayPlanner HighwayPlanner;

	public readonly PathingUtils PathingUtils;

	public readonly PathShared PathShared;

	public readonly POISmoother POISmoother;

	public readonly PrefabManager PrefabManager;

	public readonly StampManager StampManager;

	public readonly StreetTileShared StreetTileShared;

	public readonly TownPlanner TownPlanner;

	public readonly TownshipShared TownshipShared;

	public readonly WildernessPathPlanner WildernessPathPlanner;

	public readonly WildernessPlanner WildernessPlanner;

	public string WorldName;

	public string WorldSeedName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string WorldPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public MicroStopwatch totalMS;

	public bool IsCanceled;

	public bool IsFinished;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32[] roadDest;

	public Texture2D PreviewImage;

	public int WorldSize = 8192;

	public int WorldSizeDistDiv;

	public const int BiomeSizeDiv = 8;

	public int BiomeSize;

	public int RadSize;

	public int Seed = 12345;

	public int Plains = 4;

	public int Hills = 4;

	public int Mountains = 2;

	public GenerationSelections Canyons = GenerationSelections.Default;

	public GenerationSelections Craters = GenerationSelections.Default;

	public GenerationSelections Lakes = GenerationSelections.Default;

	public GenerationSelections Rivers = GenerationSelections.Default;

	public GenerationSelections Towns = GenerationSelections.Default;

	public GenerationSelections Wilderness = GenerationSelections.Default;

	public StreetTile[] StreetTileMap;

	public int StreetTileMapWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataMap<BiomeType> biomeMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public DataMap<TerrainType> terrainTypeMap;

	public List<Rect> waterRects = new List<Rect>();

	public List<Township> Townships = new List<Township>();

	public int WildernessPrefabCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public string worldSizeName;

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicProperties thisWorldProperties;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int WorldTileSize = 1024;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int BiomeTileSize = 256;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int TerrainTileSize = 256;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int terrainToBiomeTileScale = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int RadTileSize = 32;

	public BiomeLayout biomeLayout;

	public int ForestBiomeWeight = 13;

	[PublicizedFrom(EAccessModifier.Private)]
	public int BurntForestBiomeWeight = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	public int DesertBiomeWeight = 22;

	[PublicizedFrom(EAccessModifier.Private)]
	public int SnowBiomeWeight = 23;

	[PublicizedFrom(EAccessModifier.Private)]
	public int WastelandBiomeWeight = 24;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<BiomeType, Color32> biomeColors = new Dictionary<BiomeType, Color32>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cPlayerSpawnsNeeded = 12;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlayerSpawn> playerSpawns;

	public List<Path> highwayPaths = new List<Path>();

	public List<Path> wildernessPaths = new List<Path>();

	public List<Vector2i> TraderCenterPositions = new List<Vector2i>();

	public List<Vector2i> TraderForestCenterPositions = new List<Vector2i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public WaitForEndOfFrame endOfFrameHandle = new WaitForEndOfFrame();

	public bool UsePreviewer = true;

	public Data data;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly (string langKey, string fileName, Action<Stream> serializer)[] threadedSerializers;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly MemoryStream[] threadedSerializerBuffers;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly (string langKey, string fileName, Func<Stream, IEnumerator> serializer)[] mainThreadSerializers;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly MemoryStream[] mainThreadSerializerBuffers;

	[PublicizedFrom(EAccessModifier.Private)]
	public long serializedTotalSize;

	public readonly int[] biomeTagBits = new int[5]
	{
		FastTags<TagGroup.Poi>.GetBit("forest"),
		FastTags<TagGroup.Poi>.GetBit("burntforest"),
		FastTags<TagGroup.Poi>.GetBit("desert"),
		FastTags<TagGroup.Poi>.GetBit("snow"),
		FastTags<TagGroup.Poi>.GetBit("wasteland")
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32[] biomeDest;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32[] radDest;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StampGroup lowerLayer = new StampGroup("Lower Layer");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StampGroup terrainLayer = new StampGroup("Top Layer");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StampGroup radiationLayer = new StampGroup("Radiation Layer");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StampGroup biomeLayer = new StampGroup("Biome Layer");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StampGroup waterLayer = new StampGroup("Water Layer");

	[PublicizedFrom(EAccessModifier.Private)]
	public int BorderWaterMask;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PreviewPoint> previewPoints = new List<PreviewPoint>();

	[PublicizedFrom(EAccessModifier.Private)]
	public string setMessageLast = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public MicroStopwatch messageMS = new MicroStopwatch(_bStart: true);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector2i[] directions8way = new Vector2i[8]
	{
		Vector2i.up,
		Vector2i.up + Vector2i.right,
		Vector2i.right,
		Vector2i.right + Vector2i.down,
		Vector2i.down,
		Vector2i.down + Vector2i.left,
		Vector2i.left,
		Vector2i.left + Vector2i.up
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector2i[] directions4way = new Vector2i[4]
	{
		Vector2i.up,
		Vector2i.right,
		Vector2i.down,
		Vector2i.left
	};

	public int HalfWorldSize => WorldSize / 2;

	public Vector3i PrefabWorldOffset => new Vector3i(-HalfWorldSize, 0, -HalfWorldSize);

	public long SerializedSize
	{
		get
		{
			if (!SaveInfoProvider.DataLimitEnabled)
			{
				return 0L;
			}
			return serializedTotalSize;
		}
	}

	public WorldBuilder(string _seed, int _worldSize)
	{
		WorldSeedName = _seed;
		WorldSize = _worldSize;
		WorldName = GetGeneratedWorldName(WorldSeedName, WorldSize);
		WorldPath = GameIO.GetUserGameDataDir() + "/GeneratedWorlds/" + WorldName + "/";
		WorldSizeDistDiv = ((WorldSize > 4500) ? 1 : ((WorldSize > 3500) ? 2 : ((WorldSize > 2500) ? 3 : 4)));
		DistrictPlanner = new DistrictPlanner(this);
		HighwayPlanner = new HighwayPlanner(this);
		PathingUtils = new PathingUtils(this);
		PathShared = new PathShared(this);
		POISmoother = new POISmoother(this);
		PrefabManager = new PrefabManager(this);
		StampManager = new StampManager(this);
		StreetTileShared = new StreetTileShared(this);
		TownPlanner = new TownPlanner(this);
		TownshipShared = new TownshipShared(this);
		WildernessPathPlanner = new WildernessPathPlanner(this);
		WildernessPlanner = new WildernessPlanner(this);
		List<(string, string, Action<Stream>)> list = new List<(string, string, Action<Stream>)>();
		List<(string, string, Func<Stream, IEnumerator>)> list2 = new List<(string, string, Func<Stream, IEnumerator>)>();
		list.Add(("xuiBiomes", "biomes.png", [PublicizedFrom(EAccessModifier.Private)] (Stream stream) =>
		{
			stream.Write(ImageConversion.EncodeArrayToPNG(biomeDest, GraphicsFormat.R8G8B8A8_UNorm, (uint)BiomeSize, (uint)BiomeSize, (uint)(BiomeSize * 4)));
		}));
		list.Add(("xuiRadiation", "radiation.png", [PublicizedFrom(EAccessModifier.Private)] (Stream stream) =>
		{
			stream.Write(ImageConversion.EncodeArrayToPNG(radDest, GraphicsFormat.R8G8B8A8_UNorm, (uint)RadSize, (uint)RadSize, (uint)(RadSize * 4)));
		}));
		list.Add(("xuiRoads", "splat3.png", [PublicizedFrom(EAccessModifier.Private)] (Stream stream) =>
		{
			stream.Write(ImageConversion.EncodeArrayToPNG(roadDest, GraphicsFormat.R8G8B8A8_UNorm, (uint)WorldSize, (uint)WorldSize, (uint)(WorldSize * 4)));
		}));
		list.Add(("xuiWater", "splat4.png", SerializeWater));
		list.Add(("xuiHeightmap", "dtm.raw", serializeRawHeightmap));
		list.Add(("xuiPrefabs", "prefabs.xml", serializePrefabs));
		list.Add(("xuiPlayerSpawns", "spawnpoints.xml", serializePlayerSpawns));
		list.Add(("xuiLevelMetadata", "main.ttw", serializeRWGTTW));
		list.Add(("xuiMapInfo", "map_info.xml", serializeDynamicProperties));
		threadedSerializers = list.ToArray();
		mainThreadSerializers = list2.ToArray();
		threadedSerializerBuffers = new MemoryStream[threadedSerializers.Length];
		mainThreadSerializerBuffers = new MemoryStream[mainThreadSerializers.Length];
	}

	public WorldBuilder(int _worldSize)
	{
		WorldSize = _worldSize;
		data.Init(_worldSize);
		PathingUtils = new PathingUtils(this);
		RadSize = WorldSize / 32;
		radDest = new Color32[RadSize * RadSize];
		StreetTileShared = new StreetTileShared(this);
		InitStreetTiles();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator Init()
	{
		if (PlatformOptimizations.RestartAfterRwg)
		{
			PlatformApplicationManager.SetRestartRequired();
		}
		data.Init(WorldSize);
		int num = WorldSize * WorldSize;
		roadDest = new Color32[num];
		RadSize = WorldSize / 32;
		radDest = new Color32[RadSize * RadSize];
		yield return StampManager.LoadStamps();
		PrefabManager.PrefabInstanceId = 0;
		playerSpawns = new List<PlayerSpawn>();
		foreach (var (text2, vector2i2) in WorldBuilderStatic.WorldSizeMapper)
		{
			if (WorldSize >= vector2i2.x && WorldSize < vector2i2.y)
			{
				worldSizeName = text2;
			}
		}
		if (worldSizeName == null)
		{
			Log.Error($"There was an error finding rwgmixer world entry for the current world size! WorldSize: {WorldSize}/n Please make sure that the world size falls within the min/max ranges listed in xml.");
			yield break;
		}
		thisWorldProperties = WorldBuilderStatic.Properties[worldSizeName];
		Seed = WorldSeedName.GetHashCode() + WorldSize;
		Rand.Instance.SetSeed(Seed);
		biomeColors[BiomeType.forest] = WorldBuilderConstants.forestCol;
		biomeColors[BiomeType.burntForest] = WorldBuilderConstants.burntForestCol;
		biomeColors[BiomeType.desert] = WorldBuilderConstants.desertCol;
		biomeColors[BiomeType.snow] = WorldBuilderConstants.snowCol;
		biomeColors[BiomeType.wasteland] = WorldBuilderConstants.wastelandCol;
	}

	public void SetBiomeWeight(BiomeType _type, int _weight)
	{
		switch (_type)
		{
		case BiomeType.forest:
			ForestBiomeWeight = _weight;
			break;
		case BiomeType.burntForest:
			BurntForestBiomeWeight = _weight;
			break;
		case BiomeType.desert:
			DesertBiomeWeight = _weight;
			break;
		case BiomeType.snow:
			SnowBiomeWeight = _weight;
			break;
		case BiomeType.wasteland:
			WastelandBiomeWeight = _weight;
			break;
		}
	}

	public IEnumerator GenerateFromServer()
	{
		UsePreviewer = false;
		totalMS = new MicroStopwatch(_bStart: true);
		yield return GenerateData();
		yield return SaveData(canPrompt: false);
		Cleanup();
		SetMessage(null);
	}

	public IEnumerator GenerateFromUI()
	{
		IsCanceled = false;
		IsFinished = false;
		totalMS = new MicroStopwatch(_bStart: true);
		yield return SetMessage(Localization.Get("xuiStarting"));
		yield return new WaitForSeconds(0.1f);
		yield return GenerateData();
	}

	public IEnumerator FinishForPreview()
	{
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		yield return CreatePreviewTexture(roadDest);
		Log.Out("CreatePreviewTexture in {0}", (float)ms.ElapsedMilliseconds * 0.001f);
		if (!IsCanceled)
		{
			yield return SetMessage(Localization.Get("xuiRwgGenerationComplete"), _logToConsole: true);
		}
		else
		{
			yield return SetMessage(Localization.Get("xuiRwgGenerationCanceled"), _logToConsole: true, _ignoreCancel: true);
		}
		IsFinished = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator GenerateData()
	{
		yield return Init();
		yield return SetMessage(string.Format(Localization.Get("xuiWorldGenerationGenerating"), WorldName), _logToConsole: true);
		yield return GenerateTerrain();
		if (IsCanceled)
		{
			yield break;
		}
		InitStreetTiles();
		bool hasPOIs = Towns != GenerationSelections.None || Wilderness != GenerationSelections.None;
		if (hasPOIs)
		{
			yield return PrefabManager.LoadPrefabs();
			PrefabManager.ShufflePrefabData(Seed);
			yield return null;
			PathingUtils.SetupPathingGrid();
		}
		else
		{
			PrefabManager.ClearDisplayed();
		}
		if (Towns != GenerationSelections.None)
		{
			yield return TownPlanner.Plan(thisWorldProperties, Seed);
		}
		yield return GenerateTerrainLast();
		if (IsCanceled)
		{
			yield break;
		}
		yield return POISmoother.SmoothStreetTiles();
		if (IsCanceled)
		{
			yield break;
		}
		if (Wilderness != GenerationSelections.None)
		{
			yield return WildernessPlanner.Plan(thisWorldProperties, Seed);
			yield return SmoothWildernessTerrain();
			if (IsCanceled)
			{
				yield break;
			}
		}
		if (hasPOIs)
		{
			CalcTownshipsHeightMask();
			yield return HighwayPlanner.Plan(thisWorldProperties, Seed);
			yield return TownPlanner.SpawnPrefabs();
			if (IsCanceled)
			{
				yield break;
			}
		}
		if (Wilderness != GenerationSelections.None)
		{
			yield return WildernessPathPlanner.Plan(Seed);
		}
		int num = 12 - playerSpawns.Count;
		if (num > 0)
		{
			foreach (StreetTile item in CalcPlayerSpawnTiles())
			{
				if (CreatePlayerSpawn(item.WorldPositionCenter, _isFallback: true) && --num <= 0)
				{
					break;
				}
			}
		}
		yield return GCUtils.UnloadAndCollectCo();
		yield return SetMessage(Localization.Get("xuiRwgDrawRoads"), _logToConsole: true);
		yield return DrawRoads(roadDest);
		if (hasPOIs)
		{
			yield return SetMessage(Localization.Get("xuiRwgSmoothRoadTerrain"), _logToConsole: true);
			CalcWindernessPOIsHeightMask(roadDest);
			yield return SmoothRoadTerrain(roadDest, data.HeightMap, WorldSize, Townships);
		}
		foreach (Path highwayPath in highwayPaths)
		{
			highwayPath.Cleanup();
		}
		highwayPaths.Clear();
		foreach (Path wildernessPath in wildernessPaths)
		{
			wildernessPath.Cleanup();
		}
		wildernessPaths.Clear();
		yield return FinalizeWater();
		yield return SerializeData();
		yield return GCUtils.UnloadAndCollectCo();
		Log.Out("RWG final in {0}:{1:00}, r={2:x}", totalMS.Elapsed.Minutes, totalMS.Elapsed.Seconds, Rand.Instance.PeekSample());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SerializeData()
	{
		if (!SaveInfoProvider.DataLimitEnabled)
		{
			yield break;
		}
		MicroStopwatch totalMs = new MicroStopwatch(_bStart: true);
		Task[] threadedSerializerTasks = new Task[threadedSerializers.Length];
		for (int i = 0; i < threadedSerializers.Length; i++)
		{
			(string, string, Action<Stream>) tuple = threadedSerializers[i];
			string fileName = tuple.Item2;
			Action<Stream> serializer = tuple.Item3;
			MemoryStream buffer = new MemoryStream();
			threadedSerializerBuffers[i] = buffer;
			Task task = new Task(SerializeToBuffer);
			threadedSerializerTasks[i] = task;
			task.Start();
			[PublicizedFrom(EAccessModifier.Internal)]
			void SerializeToBuffer()
			{
				try
				{
					MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
					serializer(buffer);
					Log.Out($"Serialized '{fileName}' in {microStopwatch.Elapsed.TotalSeconds:F3} s (task thread)");
				}
				catch (Exception arg)
				{
					Log.Error($"Exception while serializing '{fileName}': {arg}");
				}
			}
		}
		for (int j = 0; j < mainThreadSerializers.Length; j++)
		{
			(string, string, Func<Stream, IEnumerator>) tuple2 = mainThreadSerializers[j];
			string item = tuple2.Item1;
			string fileName2 = tuple2.Item2;
			Func<Stream, IEnumerator> serializer2 = tuple2.Item3;
			MemoryStream buffer2 = new MemoryStream();
			mainThreadSerializerBuffers[j] = buffer2;
			yield return SetMessage(string.Format(Localization.Get("xuiRwgSerializing"), Localization.Get(item)));
			yield return ThreadManager.CoroutineWrapperWithExceptionCallback(SerializeToBuffer2(), [PublicizedFrom(EAccessModifier.Internal)] (Exception ex) =>
			{
				Log.Error($"Exception while serializing '{fileName2}': {ex}");
			});
			[PublicizedFrom(EAccessModifier.Internal)]
			IEnumerator SerializeToBuffer2()
			{
				yield return null;
				MicroStopwatch ms = new MicroStopwatch(_bStart: true);
				yield return serializer2(buffer2);
				Log.Out($"Serialized '{fileName2}' in {ms.Elapsed.TotalSeconds:F3} s (main thread)");
			}
		}
		object[] lastTaskNames = null;
		while (threadedSerializerTasks.Any([PublicizedFrom(EAccessModifier.Internal)] (Task x) => !x.IsCompleted))
		{
			object[] array = (from x in threadedSerializers.Where([PublicizedFrom(EAccessModifier.Internal)] ((string langKey, string fileName, Action<Stream> serializer) _, int num3) => !threadedSerializerTasks[num3].IsCompleted).Take(3)
				select Localization.Get(x.langKey)).Cast<object>().ToArray();
			if (lastTaskNames != null && Enumerable.SequenceEqual(array, lastTaskNames))
			{
				yield return null;
				continue;
			}
			lastTaskNames = array;
			yield return SetMessage(string.Format(Localization.Get("xuiRwgSerializing"), Localization.FormatListAnd(array)));
		}
		long num = 0L;
		MemoryStream[] array2 = threadedSerializerBuffers;
		foreach (MemoryStream memoryStream in array2)
		{
			num += memoryStream.Length;
		}
		array2 = mainThreadSerializerBuffers;
		foreach (MemoryStream memoryStream2 in array2)
		{
			num += memoryStream2.Length;
		}
		serializedTotalSize = num;
		Log.Out($"RWG SerializeData {serializedTotalSize.FormatSize(includeOriginalBytes: true)} in {totalMs.Elapsed.TotalSeconds:F3} s");
	}

	public bool CanSaveData()
	{
		return !SdDirectory.Exists(WorldPath);
	}

	public IEnumerator SaveData(bool canPrompt, XUiController parentController = null, bool autoConfirm = false, Action onCancel = null, Action onDiscard = null, Action onConfirm = null)
	{
		if (!CanSaveData())
		{
			if (canPrompt)
			{
				if (onCancel != null)
				{
					onCancel();
				}
				else
				{
					onDiscard?.Invoke();
				}
			}
			yield break;
		}
		if (canPrompt)
		{
			XUiC_SaveSpaceNeeded confirmationWindow = XUiC_SaveSpaceNeeded.Open(SerializedSize, WorldPath, parentController, autoConfirm, onCancel != null, onDiscard != null, null, "xuiRwgSaveWorld", null, null, "xuiSave");
			while (confirmationWindow.IsOpen)
			{
				yield return null;
			}
			switch (confirmationWindow.Result)
			{
			case XUiC_SaveSpaceNeeded.ConfirmationResult.Pending:
				Log.Error("Should not be pending.");
				yield break;
			case XUiC_SaveSpaceNeeded.ConfirmationResult.Cancelled:
				onCancel?.Invoke();
				yield break;
			case XUiC_SaveSpaceNeeded.ConfirmationResult.Discarded:
				onDiscard?.Invoke();
				yield break;
			case XUiC_SaveSpaceNeeded.ConfirmationResult.Confirmed:
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			onConfirm?.Invoke();
		}
		totalMS.ResetAndRestart();
		SdDirectory.CreateDirectory(WorldPath);
		Task[] threadedSaveTasks = new Task[threadedSerializers.Length];
		for (int i = 0; i < threadedSerializers.Length; i++)
		{
			MemoryStream buffer = threadedSerializerBuffers[i];
			(string, string, Action<Stream>) tuple = threadedSerializers[i];
			string fileName = tuple.Item2;
			Action<Stream> serializer = tuple.Item3;
			Task task = new Task(SaveToFile);
			threadedSaveTasks[i] = task;
			task.Start();
			[PublicizedFrom(EAccessModifier.Internal)]
			void SaveToFile()
			{
				try
				{
					MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
					using Stream stream = SdFile.Create(GameIO.MakeAbsolutePath(System.IO.Path.Join(WorldPath, fileName)));
					if (buffer != null)
					{
						using (buffer)
						{
							buffer.Position = 0L;
							buffer.CopyTo(stream);
						}
					}
					else
					{
						serializer(stream);
					}
					Log.Out($"Saved '{fileName}' in {microStopwatch.Elapsed.TotalSeconds:F3} s (task thread)");
				}
				catch (Exception arg)
				{
					Log.Error($"Exception while saving '{fileName}': {arg}");
				}
			}
		}
		for (int j = 0; j < mainThreadSerializers.Length; j++)
		{
			MemoryStream buffer2 = mainThreadSerializerBuffers[j];
			var (key, fileName2, serializer2) = mainThreadSerializers[j];
			yield return SetMessage(string.Format(Localization.Get("xuiRwgSaving"), Localization.Get(key)));
			yield return ThreadManager.CoroutineWrapperWithExceptionCallback(SerializeToFile(), [PublicizedFrom(EAccessModifier.Internal)] (Exception ex) =>
			{
				Log.Error($"Exception while saving '{fileName2}': {ex}");
			});
			[PublicizedFrom(EAccessModifier.Internal)]
			IEnumerator SerializeToFile()
			{
				yield return null;
				MicroStopwatch ms = new MicroStopwatch(_bStart: true);
				string path = GameIO.MakeAbsolutePath(System.IO.Path.Join(WorldPath, fileName2));
				using Stream outputStream = SdFile.Create(path);
				if (buffer2 != null)
				{
					using (buffer2)
					{
						buffer2.Position = 0L;
						buffer2.CopyTo(outputStream);
					}
				}
				else
				{
					yield return serializer2(outputStream);
				}
				Log.Out($"Saved '{fileName2}' in {ms.Elapsed.TotalSeconds:F3} s (main thread)");
			}
		}
		object[] lastTaskNames = null;
		while (threadedSaveTasks.Any([PublicizedFrom(EAccessModifier.Internal)] (Task x) => !x.IsCompleted))
		{
			object[] array = (from x in threadedSerializers.Where([PublicizedFrom(EAccessModifier.Internal)] ((string langKey, string fileName, Action<Stream> serializer) _, int num) => !threadedSaveTasks[num].IsCompleted).Take(3)
				select Localization.Get(x.langKey)).Cast<object>().ToArray();
			if (lastTaskNames != null && Enumerable.SequenceEqual(array, lastTaskNames))
			{
				yield return null;
				continue;
			}
			lastTaskNames = array;
			yield return SetMessage(string.Format(Localization.Get("xuiRwgSaving"), Localization.FormatListAnd(array)));
		}
		yield return SetMessage(Localization.Get("xuiDmCommitting"));
		yield return SaveDataUtils.SaveDataManager.CommitCoroutine();
		SaveInfoProvider.Instance.ClearResources();
		yield return SetMessage(null);
		Log.Out($"RWG SaveData in {totalMS.Elapsed.TotalSeconds:F3} s");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<StreetTile> CalcPlayerSpawnTiles()
	{
		List<StreetTile> list = (from StreetTile st in StreetTileMap
			where !st.OverlapsRadiation && !st.AllIsWater && st.Township == null && (st.District == null || st.District.name == "wilderness") && (ForestBiomeWeight == 0 || st.BiomeType == BiomeType.forest) && !st.Used
			select st).ToList();
		list.Sort([PublicizedFrom(EAccessModifier.Private)] (StreetTile _t1, StreetTile _t2) => CalcClosestTraderDistance(_t1).CompareTo(CalcClosestTraderDistance(_t2)));
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalcClosestTraderDistance(StreetTile _st)
	{
		float num = float.MaxValue;
		foreach (Vector2i traderCenterPosition in TraderCenterPositions)
		{
			float num2 = Vector2i.Distance(_st.WorldPositionCenter, traderCenterPosition);
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<StreetTile> getWildernessTilesToSmooth()
	{
		return (from StreetTile st in StreetTileMap
			where st.NeedsWildernessSmoothing
			select st).ToList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitStreetTiles()
	{
		StreetTileMapWidth = WorldSize / 150;
		data.StreetTileDataGrid = new NativeArray<StreetTileData>(StreetTileMapWidth * StreetTileMapWidth, Allocator.Persistent);
		StreetTileMap = new StreetTile[StreetTileMapWidth * StreetTileMapWidth];
		for (int i = 0; i < StreetTileMapWidth; i++)
		{
			for (int j = 0; j < StreetTileMapWidth; j++)
			{
				StreetTileMap[j + i * StreetTileMapWidth] = new StreetTile(this, new Vector2i(j, i));
			}
		}
	}

	public void CleanupGeneratedData()
	{
		roadDest = null;
		biomeDest = null;
		radDest = null;
		Townships.Clear();
		data.Cleanup();
		PrefabManager?.Clear();
		PathingUtils.Cleanup();
		Rand.Instance.Cleanup();
	}

	public void Cleanup()
	{
		serializedTotalSize = 0L;
		Span<MemoryStream> span = mainThreadSerializerBuffers.AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			ref MemoryStream reference = ref span[i];
			reference?.Dispose();
			reference = null;
		}
		span = threadedSerializerBuffers.AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			ref MemoryStream reference2 = ref span[i];
			reference2?.Dispose();
			reference2 = null;
		}
		CleanupGeneratedData();
		PrefabManager?.Cleanup();
		StampManager?.ClearStamps();
		if ((bool)PreviewImage)
		{
			UnityEngine.Object.Destroy(PreviewImage);
		}
		GCUtils.UnloadAndCollectStart();
		IsFinished = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator GenerateTerrain()
	{
		_ = WorldSize;
		_ = WorldSize;
		BiomeSize = WorldSize / 8;
		biomeDest = new Color32[BiomeSize * BiomeSize];
		BorderWaterMask = Rand.Instance.Int() & 0xF;
		Log.Out("generateBiomeTiles start at {0}, r={1:x}", (float)totalMS.ElapsedMilliseconds * 0.001f, Rand.Instance.PeekSample());
		GenerateBiomeTiles();
		yield return null;
		if (IsCanceled)
		{
			yield break;
		}
		Log.Out("GenerateTerrainTiles start at {0}, r={1:x}", (float)totalMS.ElapsedMilliseconds * 0.001f, Rand.Instance.PeekSample());
		GenerateTerrainTiles();
		yield return null;
		if (IsCanceled)
		{
			yield break;
		}
		Log.Out("GenerateBaseStamps start at {0}, r={1:x}", (float)totalMS.ElapsedMilliseconds * 0.001f, Rand.Instance.PeekSample());
		yield return GenerateBaseStamps();
		if (IsCanceled)
		{
			yield break;
		}
		yield return GenerateTerrainFromTiles(TerrainType.plains, 1024);
		if (IsCanceled)
		{
			yield break;
		}
		yield return GenerateTerrainFromTiles(TerrainType.hills, 512);
		if (!IsCanceled)
		{
			yield return GenerateTerrainFromTiles(TerrainType.mountains, 256);
			if (!IsCanceled)
			{
				Log.Out("WriteStampsToMaps start at {0}, r={1:x}", (float)totalMS.ElapsedMilliseconds * 0.001f, Rand.Instance.PeekSample());
				yield return WriteStampsToMaps();
				yield return SetMessage(Localization.Get("xuiRwgTerrainGenerationFinished"));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator GenerateBaseStamps()
	{
		for (int i = 0; i < data.HeightMap.Length; i++)
		{
			data.HeightMap[i] = 35f;
		}
		Vector2 sizeMinMax = new Vector2(0.6f, 0.85f);
		Task terrainBorderTask = new Task([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			MicroStopwatch microStopwatch = new MicroStopwatch();
			Rand rand = new Rand(Seed + 1);
			TranslationData transData = new TranslationData(0, 0, 0f, 0);
			for (int j = 0; j < WorldSize + 160; j += 160)
			{
				if (IsCanceled)
				{
					break;
				}
				if (BorderWaterMask != 15)
				{
					for (int k = 0; k < 4; k++)
					{
						transData.x = -9999;
						if (k == 0 && (BorderWaterMask & 1) == 0)
						{
							transData.x = j + rand.Range(0, 75);
							transData.y = rand.Range(0, 75);
						}
						else if (k == 1 && (BorderWaterMask & 2) == 0)
						{
							transData.x = j + rand.Range(0, 75);
							transData.y = WorldSize - rand.Range(0, 75);
						}
						else if (k == 2 && (BorderWaterMask & 4) == 0)
						{
							transData.x = rand.Range(0, 75);
							transData.y = j + rand.Range(0, 75);
						}
						else if (k == 3 && (BorderWaterMask & 8) == 0)
						{
							transData.x = WorldSize - rand.Range(0, 75);
							transData.y = j + rand.Range(0, 75);
						}
						if (transData.x != -9999)
						{
							transData.scale = rand.Range(sizeMinMax.x, sizeMinMax.y);
							transData.rotation = rand.Angle();
							int max = WorldSize / 1024 - 1;
							string text = biomeMap.Get(Mathf.Clamp(transData.x / 1024, 0, max), Mathf.Clamp(transData.y / 1024, 0, max)).ToStringCached();
							if (StampManager.TryGetStamp(text + "_land_border", out var _output, rand) || StampManager.TryGetStamp("land_border", out _output, rand))
							{
								StampManager.DrawStamp(ref data.HeightMap, new Stamp(this, _output, transData));
							}
						}
					}
				}
				if (BorderWaterMask > 0)
				{
					for (int l = 0; l < 4; l++)
					{
						transData.x = -9999;
						if (l == 0 && (BorderWaterMask & 1) > 0)
						{
							transData.x = j;
							transData.y = -40 + rand.Range(0);
						}
						else if (l == 1 && (BorderWaterMask & 2) > 0)
						{
							transData.x = j;
							transData.y = WorldSize - -40 - rand.Range(0);
						}
						else if (l == 2 && (BorderWaterMask & 4) > 0)
						{
							transData.x = -40 + rand.Range(0);
							transData.y = j;
						}
						else if (l == 3 && (BorderWaterMask & 8) > 0)
						{
							transData.x = WorldSize - -40 - rand.Range(0);
							transData.y = j;
						}
						if (transData.x != -9999)
						{
							transData.scale = rand.Range(sizeMinMax.x, sizeMinMax.y);
							transData.rotation = rand.Angle();
							if (StampManager.TryGetStamp("water_border", out var _output2, rand))
							{
								StampManager.DrawStamp(ref data.HeightMap, new Stamp(this, _output2, transData));
								Stamp stamp = new Stamp(this, _output2, transData, new Color32(0, 0, (byte)WaterHeight, byte.MaxValue), 0.1f, _isWater: true);
								waterLayer.Stamps.Add(stamp);
								StampManager.DrawWaterStamp(stamp, ref data.waterDest, WorldSize);
							}
						}
					}
				}
			}
			rand.Free();
			Log.Out("GenerateBaseStamps terrainBorderThread in {0}", (float)microStopwatch.ElapsedMilliseconds * 0.001f);
		});
		terrainBorderTask.Start();
		Task radTask = new Task([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
		});
		radTask.Start();
		Task[] biomeTasks = new Task[1]
		{
			new Task([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
				Rand rand = new Rand(Seed + 3);
				Color32 color = biomeColors[BiomeType.forest];
				for (int j = 0; j < biomeDest.Length; j++)
				{
					biomeDest[j] = color;
				}
				RawStamp stamp = StampManager.GetStamp("filler_biome", rand);
				if (stamp != null)
				{
					int num2 = WorldSize / 256;
					int num3 = 32 / 2;
					float num4 = 32f / (float)stamp.width * 1.5f;
					for (int k = 0; k < num2; k++)
					{
						int num5 = k * 256 / 8;
						for (int l = 0; l < num2; l++)
						{
							int num6 = l * 256 / 8;
							BiomeType biomeType = biomeMap.Get(l, k);
							if (biomeType != BiomeType.none)
							{
								float scale = num4 + rand.Range(0f, 0.2f);
								float angle = rand.Range(0, 4) * 90 + rand.Range(-20, 20);
								StampManager.DrawBiomeStamp(biomeDest, ref stamp.data.alphaPixels, num6 + num3, num5 + num3, BiomeSize, BiomeSize, stamp.width, stamp.height, scale, biomeColors[biomeType], 0.1f, angle);
							}
						}
					}
				}
				rand.Free();
				Log.Out("GenerateBaseStamps biomeThreads in {0}", (float)microStopwatch.ElapsedMilliseconds * 0.001f);
			})
		};
		Task[] array = biomeTasks;
		for (int num = 0; num < array.Length; num++)
		{
			array[num].Start();
		}
		bool isAnyAlive = true;
		while (isAnyAlive || !terrainBorderTask.IsCompleted || !radTask.IsCompleted)
		{
			isAnyAlive = false;
			array = biomeTasks;
			foreach (Task task in array)
			{
				isAnyAlive |= !task.IsCompleted;
			}
			if (!terrainBorderTask.IsCompleted && isAnyAlive)
			{
				yield return SetMessage(Localization.Get("xuiRwgCreatingTerrainAndBiomeStamps"));
			}
			else if (!terrainBorderTask.IsCompleted && !isAnyAlive)
			{
				yield return SetMessage(Localization.Get("xuiRwgCreatingTerrainStamps"));
			}
			else
			{
				yield return SetMessage(Localization.Get("xuiRwgCreatingBiomeStamps"));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator GenerateTerrainFromTiles(TerrainType _terrainType, int _tileSize)
	{
		Log.Out("GenerateTerrainFromTiles {0}, start at {1}, r={2:x}", _terrainType, (float)totalMS.ElapsedMilliseconds * 0.001f, Rand.Instance.PeekSample());
		int widthInTiles = WorldSize / 256;
		int t = 0;
		string terrainTypeName = _terrainType.ToStringCached();
		int step = _tileSize / 256;
		for (int tileX = 0; tileX < widthInTiles; tileX += step)
		{
			for (int tileY = 0; tileY < widthInTiles; tileY += step)
			{
				if (IsMessageElapsed())
				{
					yield return SetMessage(string.Format(Localization.Get("xuiRwgGeneratingTerrain"), Mathf.FloorToInt(100f * ((float)t / (float)(widthInTiles * widthInTiles)))));
				}
				t++;
				bool flag = true;
				for (int i = 0; i < step; i++)
				{
					for (int j = 0; j < step; j++)
					{
						if (terrainTypeMap.Get(tileX + i, tileY + j) == _terrainType)
						{
							flag = false;
							break;
						}
					}
					if (!flag)
					{
						break;
					}
				}
				if (flag)
				{
					continue;
				}
				BiomeType biomeType = biomeMap.Get(tileX, tileY);
				if (biomeType == BiomeType.none)
				{
					biomeType = BiomeType.forest;
				}
				if (_terrainType == TerrainType.mountains && biomeType == BiomeType.wasteland)
				{
					terrainTypeMap.Set(tileX, tileY, TerrainType.plains);
					continue;
				}
				int num = tileX * 256 + _tileSize / 2;
				int num2 = tileY * 256 + _tileSize / 2;
				string text = biomeType.ToStringCached();
				string comboTypeName = $"{text}_{terrainTypeName}";
				GetTerrainProperties(text, terrainTypeName, comboTypeName, out var _scaleMinMax, out var _clusterCount, out var _clusterRadius, out var _clusterStrength, out var useBiomeMask, out var biomeCutoff);
				_scaleMinMax *= (float)step;
				_clusterRadius *= step;
				int num3 = 0;
				float alpha = _clusterStrength;
				bool additive = false;
				for (int k = 0; k < _clusterCount; k++)
				{
					if (StampManager.TryGetStamp(terrainTypeName, comboTypeName, out var tmp))
					{
						Vector2 vector = Rand.Instance.RandomOnUnitCircle() * num3;
						TranslationData transData = new TranslationData(num + Mathf.RoundToInt(vector.x), num2 + Mathf.RoundToInt(vector.y), _scaleMinMax.x, _scaleMinMax.y);
						WorldBuilder worldBuilder = this;
						RawStamp stamp = tmp;
						string name = tmp.name;
						Stamp stamp2 = new Stamp(worldBuilder, stamp, transData, default(Color32), 0.1f, _isWater: false, name);
						stamp2.alpha = alpha;
						stamp2.additive = additive;
						terrainLayer.Stamps.Add(stamp2);
						if (useBiomeMask)
						{
							biomeLayer.Stamps.Add(new Stamp(this, tmp, transData, biomeColors[biomeType], biomeCutoff));
						}
						num3 = _clusterRadius;
						alpha = _clusterStrength * 0.45f;
						additive = true;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator WriteStampsToMaps()
	{
		Task biomeTask = new Task([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
			StampManager.DrawStampGroup(biomeLayer, biomeDest, BiomeSize, 0.125f);
			biomeLayer.Stamps.Clear();
			Log.Out("WriteStampsToMaps biome in {0}", (float)microStopwatch.ElapsedMilliseconds * 0.001f);
		});
		Task radnwatTask = new Task([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
			StampManager.DrawStampGroup(radiationLayer, radDest, RadSize);
			Log.Out("writeStampsToMaps rad in {0}", (float)microStopwatch.ElapsedMilliseconds * 0.001f);
			microStopwatch.ResetAndRestart();
			ClearWaterUnderTerrain();
			Log.Out("WriteStampsToMaps water #{0} in {1}", waterLayer.Stamps.Count, (float)microStopwatch.ElapsedMilliseconds * 0.001f);
			waterLayer.Stamps.Clear();
		});
		biomeTask.Start();
		radnwatTask.Start();
		while (!biomeTask.IsCompleted || !radnwatTask.IsCompleted)
		{
			yield return SetMessage(Localization.Get("xuiRwgWritingStampsToMap"));
		}
		Log.Out("WriteStampsToMaps end at {0}, r={1:x}", (float)totalMS.ElapsedMilliseconds * 0.001f, Rand.Instance.PeekSample());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearWaterUnderTerrain()
	{
		for (int i = 0; i < waterLayer.Stamps.Count; i++)
		{
			Stamp stamp = waterLayer.Stamps[i];
			int startX = Utils.FastMax(Mathf.FloorToInt(stamp.Area.min.x), 0);
			int endX = Utils.FastMin(Mathf.FloorToInt(stamp.Area.max.x), WorldSize - 1);
			int startY = Utils.FastMax(Mathf.FloorToInt(stamp.Area.min.y), 0);
			int endY = Utils.FastMin(Mathf.FloorToInt(stamp.Area.max.y), WorldSize - 1);
			ClearWaterUnderTerrain(ref data, ref data.HeightMap, startX, endX, startY, endY);
		}
	}

	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void ClearWaterUnderTerrain(ref Data _data, ref NativeArray<float> _terrain, int startX, int endX, int startY, int endY)
	{
		ClearWaterUnderTerrain_0000A3CE_0024BurstDirectCall.Invoke(ref _data, ref _terrain, startX, endX, startY, endY);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator GenerateTerrainLast()
	{
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		StampManager.GetStamp("base");
		if (Lakes > GenerationSelections.None)
		{
			generateTerrainFeature("lake", Lakes, isWaterFeature: true);
		}
		if (Rivers > GenerationSelections.None)
		{
			generateTerrainFeature("river", Rivers, isWaterFeature: true);
		}
		if (Canyons > GenerationSelections.None)
		{
			generateTerrainFeature("canyon", Canyons);
		}
		if (Craters > GenerationSelections.None)
		{
			generateTerrainFeature("crater", Craters);
		}
		Task terrainTask = new Task([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			StampManager.DrawStampGroup(lowerLayer, ref data.HeightMap, WorldSize);
			StampManager.DrawStampGroup(terrainLayer, ref data.HeightMap, WorldSize);
			AdjustHeights(ref data.HeightMap, 2f);
		});
		Task waterTask = new Task([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			StampManager.DrawWaterStampGroup(waterLayer, ref data.waterDest, WorldSize);
		});
		terrainTask.Start();
		waterTask.Start();
		while (!terrainTask.IsCompleted || !waterTask.IsCompleted)
		{
			if (!terrainTask.IsCompleted && waterTask.IsCompleted)
			{
				yield return SetMessage(Localization.Get("xuiRwgWritingTerrainStampsToMap"));
			}
			else if (terrainTask.IsCompleted && !waterTask.IsCompleted)
			{
				yield return SetMessage(Localization.Get("xuiRwgWritingWaterStampsToMap"));
			}
			else
			{
				yield return SetMessage(Localization.Get("xuiRwgWritingTerrainAndWaterStampsToMap"));
			}
		}
		Task waterMapTask = new Task([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
			ClearWaterUnderTerrain();
			Log.Out("WaterToMap last #{0} in {1}", waterLayer.Stamps.Count, (float)microStopwatch.ElapsedMilliseconds * 0.001f);
		});
		waterMapTask.Start();
		while (!waterMapTask.IsCompleted)
		{
			yield return SetMessage(Localization.Get("xuiRwgCleaningUpWaterMapData"));
		}
		Log.Out("GenerateTerrainLast done in {0}, r={1:x}", (float)ms.ElapsedMilliseconds * 0.001f, Rand.Instance.PeekSample());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator FinalizeWater()
	{
		Task waterTask = new Task([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
			FinalizeWater(ref data, WaterHeight);
			Log.Out("FinalizeWater in {0}", (float)microStopwatch.ElapsedMilliseconds * 0.001f);
		});
		waterTask.Start();
		while (!waterTask.IsCompleted)
		{
			yield return null;
		}
	}

	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void FinalizeWater(ref Data _data, float _WaterHeight)
	{
		FinalizeWater_0000A3D1_0024BurstDirectCall.Invoke(ref _data, _WaterHeight);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SerializeWater(Stream stream)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		Color32[] array = new Color32[WorldSize * WorldSize];
		Color32 color = new Color32(0, 0, 0, 0);
		for (int i = 0; i < WorldSize; i++)
		{
			for (int j = 0; j < WorldSize; j++)
			{
				int num = j + i * WorldSize;
				color.b = (byte)data.waterDest[num];
				array[num] = color;
			}
		}
		Log.Out($"Create water in {(float)microStopwatch.ElapsedMilliseconds * 0.001f}");
		stream.Write(ImageConversion.EncodeArrayToPNG(array, GraphicsFormat.R8G8B8A8_UNorm, (uint)WorldSize, (uint)WorldSize, (uint)(WorldSize * 4)));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void generateTerrainFeature(string featureName, GenerationSelections selection, bool isWaterFeature = false)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		Vector2 vector = new Vector2(0.5f, 1.5f);
		int num = 0;
		int num2 = 0;
		float num3 = 0f;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		Vector2i zero = Vector2i.zero;
		Vector2i zero2 = Vector2i.zero;
		Vector2i zero3 = Vector2i.zero;
		Vector2i zero4 = Vector2i.zero;
		Vector2 zero5 = Vector2.zero;
		Vector2 zero6 = Vector2.zero;
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(Seed + featureName.GetHashCode() + 1);
		GameRandom rnd2 = GameRandomManager.Instance.CreateGameRandom(Seed + featureName.GetHashCode() + 2);
		string input;
		if ((input = thisWorldProperties.GetString(featureName + "s.scale")) != string.Empty)
		{
			vector = StringParsers.ParseVector2(input);
		}
		int count = GetCount(featureName + "s", selection);
		for (int i = 0; i < count; i++)
		{
			if (!StampManager.TryGetStamp(featureName, out var _output))
			{
				if (featureName.Contains("river"))
				{
					Log.Out("Could not find stamp {0}", featureName);
				}
				continue;
			}
			num3 = gameRandom.RandomRange(vector.x, vector.y) * 1.4f;
			num4 = gameRandom.RandomRange(0, 360);
			num5 = (int)((float)_output.width * num3);
			num6 = (int)((float)_output.height * num3);
			num = -(num5 / 2);
			num2 = -(num6 / 2);
			zero = getRotatedPoint(num, num2, num + num5 / 2, num2 + num6 / 2, num4);
			zero2 = getRotatedPoint(num + num5, num2, num + num5 / 2, num2 + num6 / 2, num4);
			zero3 = getRotatedPoint(num, num2 + num6, num + num5 / 2, num2 + num6 / 2, num4);
			zero4 = getRotatedPoint(num + num5, num2 + num6, num + num5 / 2, num2 + num6 / 2, num4);
			zero5.x = Mathf.Min(Mathf.Min(zero.x, zero2.x), Mathf.Min(zero3.x, zero4.x));
			zero5.y = Mathf.Min(Mathf.Min(zero.y, zero2.y), Mathf.Min(zero3.y, zero4.y));
			zero6.x = Mathf.Max(Mathf.Max(zero.x, zero2.x), Mathf.Max(zero3.x, zero4.x));
			zero6.y = Mathf.Max(Mathf.Max(zero.y, zero2.y), Mathf.Max(zero3.y, zero4.y));
			Rect rect = new Rect(zero5, zero6 - zero5);
			foreach (StreetTile item in (from StreetTile st in StreetTileMap
				where (st.Township == null || st.District == null || st.District.name == "wilderness") && st.TerrainType != TerrainType.mountains && !st.HasFeature && st.GetNeighborCount() > 3
				orderby rnd2.RandomInt
				select st).ToList())
			{
				if (item.GridPosition.x == 0 || item.GridPosition.y == 0)
				{
					continue;
				}
				int num7 = item.WorldPositionCenter.x - (int)rect.width / 2;
				while (true)
				{
					if ((float)num7 < (float)item.WorldPositionCenter.x + rect.width / 2f)
					{
						for (int num8 = item.WorldPositionCenter.y - (int)rect.height / 2; (float)num8 < (float)item.WorldPositionCenter.y + rect.height / 2f; num8 += 150)
						{
							StreetTile streetTileWorld = GetStreetTileWorld(num7, num8);
							if (streetTileWorld == null || streetTileWorld.Township != null || streetTileWorld.District != null || streetTileWorld.Used || streetTileWorld.HasFeature)
							{
								goto end_IL_0408;
							}
						}
						num7 += 150;
						continue;
					}
					for (int num9 = item.WorldPositionCenter.x - (int)rect.width / 2; (float)num9 < (float)item.WorldPositionCenter.x + rect.width / 2f; num9 += 150)
					{
						for (int num10 = item.WorldPositionCenter.y - (int)rect.height / 2; (float)num10 < (float)item.WorldPositionCenter.y + rect.height / 2f; num10 += 150)
						{
							GetStreetTileWorld(num9, num10).HasFeature = true;
						}
					}
					TranslationData transData = new TranslationData(item.WorldPositionCenter.x, item.WorldPositionCenter.y, num3, num4);
					Stamp stamp = new Stamp(this, _output, transData);
					if (isWaterFeature)
					{
						bool flag = false;
						for (int num11 = 0; num11 < terrainLayer.Stamps.Count; num11++)
						{
							if (stamp.Name.Contains("mountain") && stamp.Area.Overlaps(terrainLayer.Stamps[num11].Area))
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							i--;
							break;
						}
						lowerLayer.Stamps.Add(stamp);
						waterLayer.Stamps.Add(new Stamp(this, _output, transData, new Color32(0, 0, (byte)WaterHeight, byte.MaxValue), 0.1f, _isWater: true));
						goto end_IL_06a3;
					}
					lowerLayer.Stamps.Add(stamp);
					bool flag2 = true;
					for (int num12 = 0; num12 < waterLayer.Stamps.Count; num12++)
					{
						if (stamp.Area.Overlaps(waterLayer.Stamps[num12].Area))
						{
							flag2 = false;
							break;
						}
					}
					if (flag2)
					{
						for (int num13 = 0; num13 < waterRects.Count; num13++)
						{
							if (stamp.Area.Overlaps(waterRects[num13]))
							{
								flag2 = false;
								break;
							}
						}
					}
					if (!flag2)
					{
						waterLayer.Stamps.Add(new Stamp(this, _output, transData, new Color32(0, 0, (byte)WaterHeight, byte.MaxValue), 0.05f, _isWater: true));
					}
					goto end_IL_06a3;
					continue;
					end_IL_0408:
					break;
				}
				continue;
				end_IL_06a3:
				break;
			}
		}
		GameRandomManager.Instance.FreeGameRandom(rnd2);
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
		Log.Out("generateTerrainFeature {0} in {1}", featureName, (float)microStopwatch.ElapsedMilliseconds * 0.001f);
	}

	public bool CreatePlayerSpawn(Vector2i worldPos, bool _isFallback = false)
	{
		Vector3 position = new Vector3(worldPos.x, GetHeight(worldPos), worldPos.y);
		if (!_isFallback)
		{
			for (int i = 0; i < playerSpawns.Count; i++)
			{
				if (playerSpawns[i].IsTooClose(position))
				{
					return false;
				}
			}
			StreetTile streetTileWorld = GetStreetTileWorld(worldPos);
			if (streetTileWorld != null && streetTileWorld.HasPrefabs)
			{
				if (ForestBiomeWeight > 0 && streetTileWorld.BiomeType != BiomeType.forest)
				{
					return false;
				}
				foreach (PrefabDataInstance streetTilePrefabData in streetTileWorld.StreetTilePrefabDatas)
				{
					if (streetTilePrefabData.prefab.DifficultyTier >= 2)
					{
						return false;
					}
				}
			}
			List<Vector2i> list = ((ForestBiomeWeight > 0) ? TraderForestCenterPositions : TraderCenterPositions);
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				if (Vector2i.DistanceSqr(list[j], worldPos) < 810000f)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		PlayerSpawn item = new PlayerSpawn(position, Rand.Instance.Range(0, 360));
		playerSpawns.Add(item);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcTownshipsHeightMask()
	{
		int worldSize = WorldSize;
		int worldSize2 = WorldSize;
		int length = worldSize * worldSize2;
		data.poiHeightMask = new NativeArray<byte>(length, Allocator.Persistent);
		if (Townships == null)
		{
			return;
		}
		for (int i = 0; i < Townships.Count; i++)
		{
			foreach (StreetTile value in Townships[i].Streets.Values)
			{
				int num = 0;
				int num2 = value.WorldPosition.x + value.WorldPosition.y * worldSize;
				for (int j = -num; j < 150 + num; j++)
				{
					for (int k = -num; k < 150 + num; k++)
					{
						data.poiHeightMask[k + num2] = 1;
					}
					num2 += worldSize;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcWindernessPOIsHeightMask(Color32[] roadMask)
	{
		int worldSize = WorldSize;
		for (int i = 0; i < StreetTileMapWidth; i++)
		{
			for (int j = 0; j < StreetTileMapWidth; j++)
			{
				StreetTile streetTile = StreetTileMap[j + i * StreetTileMapWidth];
				if (!streetTile.NeedsWildernessSmoothing)
				{
					continue;
				}
				int num = streetTile.WildernessPOIPos.x + streetTile.WildernessPOIPos.y * worldSize;
				for (int k = 0; k < streetTile.WildernessPOISize.y; k++)
				{
					for (int l = 0; l < streetTile.WildernessPOISize.x; l++)
					{
						data.poiHeightMask[l + num] = 1;
					}
					num += worldSize;
				}
			}
		}
	}

	public IEnumerator SmoothRoadTerrain(Color32[] _roadMask, NativeArray<float> _heightMap, int WorldSize, List<Township> _townships = null)
	{
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		NativeArray<Color32> roadMask = new NativeArray<Color32>(_roadMask.Length, Allocator.Persistent);
		roadMask.CopyFrom(_roadMask);
		Task task = new Task([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			SmoothRoadTerrainTask(ref data, ref roadMask, ref _heightMap, WorldSize);
		});
		task.Start();
		while (!task.IsCompleted)
		{
			yield return SetMessage(string.Format(Localization.Get("xuiRwgSmoothRoadTerrainCount"), data.messageCnt));
		}
		roadMask.Dispose();
		Log.Out("Smooth Road Terrain in {0}", (float)ms.ElapsedMilliseconds * 0.001f);
	}

	[BurstCompile(CompileSynchronously = true)]
	public static void SmoothRoadTerrainTask(ref Data _data, ref NativeArray<Color32> roadMask, ref NativeArray<float> _heightMap, int WorldSize)
	{
		SmoothRoadTerrainTask_0000A3D8_0024BurstDirectCall.Invoke(ref _data, ref roadMask, ref _heightMap, WorldSize);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SmoothWildernessTerrain()
	{
		yield return null;
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		foreach (StreetTile item in getWildernessTilesToSmooth())
		{
			item.SmoothWildernessTerrain();
		}
		Log.Out($"Smooth Wilderness Terrain in {(float)microStopwatch.ElapsedMilliseconds * 0.001f}, r={Rand.Instance.PeekSample():x}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GenerateTerrainTiles()
	{
		int tileWidth = WorldSize / 256;
		terrainTypeMap = new DataMap<TerrainType>(tileWidth, TerrainType.none);
		Rand instance = Rand.Instance;
		List<TileGroup> list = new List<TileGroup>();
		for (int i = 0; i < 5; i++)
		{
			list.Add(new TileGroup
			{
				Biome = (BiomeType)i
			});
		}
		for (int j = 0; j < biomeMap.width; j++)
		{
			for (int k = 0; k < biomeMap.width; k++)
			{
				Vector2i item = new Vector2i(j, k);
				BiomeType index = biomeMap.Get(j, k);
				list[(int)index].Positions.Add(item);
			}
		}
		float num = Plains + Hills + Mountains;
		if (num == 0f)
		{
			Plains = 1;
			num = 1f;
		}
		Vector2i item2 = default(Vector2i);
		foreach (TileGroup item3 in list)
		{
			int num2 = Mathf.FloorToInt((float)Plains / num * (float)item3.Positions.Count);
			int num3 = Mathf.FloorToInt((float)Hills / num * (float)item3.Positions.Count);
			int num4 = Mathf.FloorToInt((float)Mountains / num * (float)item3.Positions.Count);
			while (item3.Positions.Count > num2 + num3 + num4)
			{
				int num5 = instance.Range(3);
				if (num5 == 0)
				{
					if (Plains > 0)
					{
						num2++;
					}
					else
					{
						num5++;
					}
				}
				if (num5 == 1)
				{
					if (Hills > 0)
					{
						num3++;
					}
					else
					{
						num5++;
					}
				}
				if (num5 == 2 && Mountains > 0)
				{
					num4++;
				}
			}
			int index2 = instance.Range(item3.Positions.Count);
			while (item3.Positions.Count > 0)
			{
				Vector2i vector2i = item3.Positions[index2];
				item3.Positions.RemoveAt(index2);
				index2 = instance.Range(item3.Positions.Count);
				int num6 = vector2i.x / 1;
				int num7 = vector2i.y / 1;
				if (terrainTypeMap.Get(num6, num7) != TerrainType.none)
				{
					continue;
				}
				if (num3 > 0)
				{
					if (num3 >= 2)
					{
						num6 &= -2;
						num7 &= -2;
						terrainTypeMap.Set(num6, num7, TerrainType.hills);
						terrainTypeMap.Set(num6 + 1, num7, TerrainType.hills);
						terrainTypeMap.Set(num6, num7 + 1, TerrainType.hills);
						terrainTypeMap.Set(num6 + 1, num7 + 1, TerrainType.hills);
					}
					num3 -= 4;
				}
				else if (num4 > 0)
				{
					num4--;
					terrainTypeMap.Set(num6, num7, TerrainType.mountains);
					if (num4 <= 0 || !(instance.Float() < 0.8f))
					{
						continue;
					}
					int num8 = instance.Range(4);
					for (int l = 0; l < 4; l++)
					{
						num8 = (num8 + 1) & 3;
						item2.x = vector2i.x + directions4way[num8].x;
						item2.y = vector2i.y + directions4way[num8].y;
						int num9 = item3.Positions.IndexOf(item2);
						if (num9 >= 0)
						{
							num6 = item2.x / 1;
							num7 = item2.y / 1;
							if (terrainTypeMap.Get(num6, num7) == TerrainType.none)
							{
								index2 = num9;
								break;
							}
						}
					}
				}
				else
				{
					terrainTypeMap.Set(num6, num7, TerrainType.plains);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BiomeTypeData> CalcBiomeTileBiomeData(int totalTiles)
	{
		float num = ForestBiomeWeight + BurntForestBiomeWeight + DesertBiomeWeight + SnowBiomeWeight + WastelandBiomeWeight;
		List<BiomeTypeData> source = new List<BiomeTypeData>
		{
			new BiomeTypeData(BiomeType.forest, (float)ForestBiomeWeight / num, totalTiles),
			new BiomeTypeData(BiomeType.burntForest, (float)BurntForestBiomeWeight / num, totalTiles),
			new BiomeTypeData(BiomeType.desert, (float)DesertBiomeWeight / num, totalTiles),
			new BiomeTypeData(BiomeType.snow, (float)SnowBiomeWeight / num, totalTiles),
			new BiomeTypeData(BiomeType.wasteland, (float)WastelandBiomeWeight / num, totalTiles)
		};
		source = (from b in source
			where b.Percent > 0f
			orderby 0f - b.Percent
			select b).ToList();
		int num2 = 0;
		for (int num3 = 0; num3 < source.Count; num3++)
		{
			num2 += source[num3].TileCount;
		}
		int num4 = 0;
		for (int num5 = num2; num5 < totalTiles; num5++)
		{
			source[num4].TileCount++;
			num4 = (num4 + 1) % source.Count;
		}
		return source;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GenerateBiomeTiles()
	{
		int num = WorldSize / 256;
		float num2 = (float)num * 0.5f;
		int num3 = num * num;
		biomeMap = new DataMap<BiomeType>(num, BiomeType.none);
		List<BiomeTypeData> list = CalcBiomeTileBiomeData(num3);
		BiomeType biomeType = BiomeType.none;
		if (biomeLayout == BiomeLayout.CenterForest)
		{
			biomeType = BiomeType.forest;
		}
		if (biomeLayout == BiomeLayout.CenterWasteland)
		{
			biomeType = BiomeType.wasteland;
		}
		int num4 = 1;
		float num5 = num2;
		float num6 = num2;
		Vector2 a = default(Vector2);
		Vector2 b = default(Vector2);
		for (int i = 0; i < num4; i++)
		{
			if (biomeLayout == BiomeLayout.Line)
			{
				float num7 = (float)num * 0.4f;
				float f = (float)(Rand.Instance.Range(4) * 90) * (MathF.PI / 180f);
				a.x = num5 - Mathf.Cos(f) * num7;
				a.y = num6 - Mathf.Sin(f) * num7;
				b.x = num5 + Mathf.Cos(f) * num7;
				b.y = num6 + Mathf.Sin(f) * num7;
				float num8 = 0f;
				float num9 = 1f / (float)(list.Count - 1);
				for (int j = 0; j < list.Count; j++)
				{
					BiomeTypeData biomeTypeData = list[j];
					Vector2 vector = Vector2.Lerp(a, b, num8);
					biomeTypeData.Center = new Vector2i((int)vector.x, (int)vector.y);
					biomeTypeData.TileCount--;
					biomeMap.Set(biomeTypeData.Center.x, biomeTypeData.Center.y, biomeTypeData.Type);
					num8 += num9;
				}
			}
			else
			{
				int num10 = list.Count - 1;
				if (biomeLayout == BiomeLayout.Circle)
				{
					num10 = list.Count;
				}
				float num11 = Rand.Instance.Angle();
				float num12 = 360f / (float)num10;
				if (Rand.Instance.Float() < 0.5f)
				{
					num12 *= -1f;
				}
				for (int k = 0; k < list.Count; k++)
				{
					BiomeTypeData biomeTypeData2 = list[k];
					if (biomeTypeData2.Type == biomeType)
					{
						biomeTypeData2.Center = new Vector2i((int)num5, (int)num6);
					}
					else
					{
						float num13 = (float)num * 0.4f;
						float num14 = num5 + Mathf.Cos(num11 * (MathF.PI / 180f)) * num13;
						float num15 = num6 + Mathf.Sin(num11 * (MathF.PI / 180f)) * num13;
						num11 += num12;
						biomeTypeData2.Center = new Vector2i((int)num14, (int)num15);
					}
					biomeTypeData2.TileCount--;
					biomeMap.Set(biomeTypeData2.Center.x, biomeTypeData2.Center.y, biomeTypeData2.Type);
				}
			}
			num5 += 3f;
			num6 += 2f;
		}
		int num16 = num3 - list.Count;
		int num17 = 1 + WorldSize / 2048;
		int num18;
		do
		{
			num18 = num16;
			for (int l = 0; l < list.Count; l++)
			{
				BiomeTypeData biomeTypeData3 = list[l];
				if (biomeTypeData3.TileCount <= 0)
				{
					continue;
				}
				int edge = 0;
				if (biomeTypeData3.Type == biomeType)
				{
					edge = num17;
				}
				int num19 = 1 + (int)(biomeTypeData3.Percent * 4f);
				for (int m = 0; m < num19; m++)
				{
					if (!FindBiomeEmptyAndSet(biomeTypeData3, edge))
					{
						break;
					}
					biomeTypeData3.TileCount--;
					num16--;
					if (biomeTypeData3.TileCount <= 0)
					{
						break;
					}
				}
			}
		}
		while (num16 != num18);
		do
		{
			num18 = num16;
			for (int n = 0; n < list.Count; n++)
			{
				BiomeTypeData biomeTypeData4 = list[n];
				if (biomeTypeData4.Type != BiomeType.wasteland && FindBiomeEmptyAndSet(biomeTypeData4, 0))
				{
					num16--;
				}
			}
		}
		while (num16 != num18);
		for (int num20 = 0; num20 < biomeMap.width; num20++)
		{
			for (int num21 = 0; num21 < biomeMap.width; num21++)
			{
				if (biomeMap.Get(num20, num21) == BiomeType.none)
				{
					biomeMap.Set(num20, num21, BiomeType.wasteland);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool FindBiomeEmptyAndSet(BiomeTypeData _b, int _edge)
	{
		int v = WorldSize / 256 - 1 - _edge;
		for (int i = 1; i <= 39; i++)
		{
			int num = Utils.FastMax(_edge, _b.Center.x - i);
			int num2 = Utils.FastMin(_b.Center.x + i, v);
			int num3 = Utils.FastMax(_edge, _b.Center.y - i);
			int num4 = Utils.FastMin(_b.Center.y + i, v);
			for (int j = 0; j <= i; j++)
			{
				int num5 = _b.Center.y - i;
				int num6;
				if (num5 >= num3)
				{
					num6 = _b.Center.x - j;
					if (num6 >= num && biomeMap.Get(num6, num5) == BiomeType.none && HasBiomeNeighbor(num6, num5, _b.Type))
					{
						biomeMap.Set(num6, num5, _b.Type);
						return true;
					}
					num6 = _b.Center.x + j;
					if (num6 <= num2 && biomeMap.Get(num6, num5) == BiomeType.none && HasBiomeNeighbor(num6, num5, _b.Type))
					{
						biomeMap.Set(num6, num5, _b.Type);
						return true;
					}
				}
				num5 = _b.Center.y + i;
				if (num5 <= num4)
				{
					num6 = _b.Center.x - j;
					if (num6 >= num && biomeMap.Get(num6, num5) == BiomeType.none && HasBiomeNeighbor(num6, num5, _b.Type))
					{
						biomeMap.Set(num6, num5, _b.Type);
						return true;
					}
					num6 = _b.Center.x + j;
					if (num6 <= num2 && biomeMap.Get(num6, num5) == BiomeType.none && HasBiomeNeighbor(num6, num5, _b.Type))
					{
						biomeMap.Set(num6, num5, _b.Type);
						return true;
					}
				}
				num6 = _b.Center.x - i;
				if (num6 >= num)
				{
					num5 = _b.Center.y - j;
					if (num5 >= num3 && biomeMap.Get(num6, num5) == BiomeType.none && HasBiomeNeighbor(num6, num5, _b.Type))
					{
						biomeMap.Set(num6, num5, _b.Type);
						return true;
					}
					num5 = _b.Center.y + j;
					if (num5 <= num4 && biomeMap.Get(num6, num5) == BiomeType.none && HasBiomeNeighbor(num6, num5, _b.Type))
					{
						biomeMap.Set(num6, num5, _b.Type);
						return true;
					}
				}
				num6 = _b.Center.x + i;
				if (num6 <= num2)
				{
					num5 = _b.Center.y - j;
					if (num5 >= num3 && biomeMap.Get(num6, num5) == BiomeType.none && HasBiomeNeighbor(num6, num5, _b.Type))
					{
						biomeMap.Set(num6, num5, _b.Type);
						return true;
					}
					num5 = _b.Center.y + j;
					if (num5 <= num4 && biomeMap.Get(num6, num5) == BiomeType.none && HasBiomeNeighbor(num6, num5, _b.Type))
					{
						biomeMap.Set(num6, num5, _b.Type);
						return true;
					}
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HasBiomeNeighbor(int _x, int _y, BiomeType _biomeType)
	{
		int num = WorldSize / 256;
		int num2 = _x - 1;
		if (num2 >= 0 && biomeMap.Get(num2, _y) == _biomeType)
		{
			return true;
		}
		num2 = _x + 1;
		if (num2 < num && biomeMap.Get(num2, _y) == _biomeType)
		{
			return true;
		}
		int num3 = _y - 1;
		if (num3 >= 0 && biomeMap.Get(_x, num3) == _biomeType)
		{
			return true;
		}
		num3 = _y + 1;
		if (num3 < num && biomeMap.Get(_x, num3) == _biomeType)
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeType GetBiomeFromNeighbors(int _x, int _y)
	{
		int num = WorldSize / 256;
		int num2 = _x - 1;
		if (num2 >= 0)
		{
			BiomeType biomeType = biomeMap.Get(num2, _y);
			if (biomeType != BiomeType.none && biomeType != BiomeType.wasteland)
			{
				return biomeType;
			}
		}
		num2 = _x + 1;
		if (num2 < num)
		{
			BiomeType biomeType2 = biomeMap.Get(num2, _y);
			if (biomeType2 != BiomeType.none && biomeType2 != BiomeType.wasteland)
			{
				return biomeType2;
			}
		}
		int num3 = _y - 1;
		if (num3 >= 0)
		{
			BiomeType biomeType3 = biomeMap.Get(_x, num3);
			if (biomeType3 != BiomeType.none && biomeType3 != BiomeType.wasteland)
			{
				return biomeType3;
			}
		}
		num3 = _y + 1;
		if (num3 < num)
		{
			BiomeType biomeType4 = biomeMap.Get(_x, num3);
			if (biomeType4 != BiomeType.none && biomeType4 != BiomeType.wasteland)
			{
				return biomeType4;
			}
		}
		return BiomeType.none;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void serializeRWGTTW(Stream stream)
	{
		World world = new World();
		WorldState worldState = new WorldState();
		worldState.SetFrom(world, EnumChunkProviderId.ChunkDataDriven);
		worldState.ResetDynamicData();
		worldState.Save(stream);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void serializeDynamicProperties(Stream stream)
	{
		DynamicProperties dynamicProperties = new DynamicProperties();
		dynamicProperties.Values["Scale"] = "1";
		dynamicProperties.Values["HeightMapSize"] = string.Format("{0},{0}", WorldSize);
		dynamicProperties.Values["Modes"] = "Survival,SurvivalSP,SurvivalMP,Creative";
		dynamicProperties.Values["FixedWaterLevel"] = "false";
		dynamicProperties.Values["RandomGeneratedWorld"] = "true";
		dynamicProperties.Values["GameVersion"] = Constants.cVersionInformation.SerializableString;
		dynamicProperties.Values["Generation.Seed"] = WorldSeedName;
		dynamicProperties.Values["Seed"] = Seed.ToString();
		dynamicProperties.Values["Generation.Towns"] = Towns.ToString();
		dynamicProperties.Values["Generation.Wilderness"] = Wilderness.ToString();
		dynamicProperties.Values["Generation.Lakes"] = Lakes.ToString();
		dynamicProperties.Values["Generation.Rivers"] = Rivers.ToString();
		dynamicProperties.Values["Generation.Cracks"] = Canyons.ToString();
		dynamicProperties.Values["Generation.Craters"] = Craters.ToString();
		dynamicProperties.Values["Generation.Plains"] = Plains.ToString();
		dynamicProperties.Values["Generation.Hills"] = Hills.ToString();
		dynamicProperties.Values["Generation.Mountains"] = Mountains.ToString();
		dynamicProperties.Values["Generation.Forest"] = ForestBiomeWeight.ToString();
		dynamicProperties.Values["Generation.BurntForest"] = BurntForestBiomeWeight.ToString();
		dynamicProperties.Values["Generation.Desert"] = DesertBiomeWeight.ToString();
		dynamicProperties.Values["Generation.Snow"] = SnowBiomeWeight.ToString();
		dynamicProperties.Values["Generation.Wasteland"] = WastelandBiomeWeight.ToString();
		dynamicProperties.Save("MapInfo", stream);
	}

	public void AddPreviewLinePlus(Vector2i pos, Color32 color, int size)
	{
		PreviewPoint item = default(PreviewPoint);
		item.pos = pos;
		item.color = color;
		item.size = size;
		previewPoints.Add(item);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator CreatePreviewTexture(Color32[] roadMask)
	{
		yield return SetMessage(Localization.Get("xuiRwgCreatingPreview"), _logToConsole: true);
		MicroStopwatch msReset = new MicroStopwatch(_bStart: true);
		Color32[] dest = new Color32[roadMask.Length];
		Color32 color = new Color32(0, 0, 0, byte.MaxValue);
		int destOffsetY = 0;
		int biomeSteps = WorldSize / BiomeSize;
		for (int y = 0; y < BiomeSize; y++)
		{
			int num = destOffsetY;
			for (int i = 0; i < BiomeSize; i++)
			{
				Color32 color2 = biomeDest[i + y * BiomeSize];
				color.r = (byte)(color2.r / 2);
				color.g = (byte)(color2.g / 2);
				color.b = (byte)(color2.b / 2);
				for (int j = 0; j < biomeSteps; j++)
				{
					int num2 = num + j * WorldSize;
					for (int k = 0; k < biomeSteps; k++)
					{
						dest[num2 + k] = color;
					}
				}
				num += biomeSteps;
			}
			destOffsetY += biomeSteps * WorldSize;
			if (msReset.ElapsedMilliseconds > 500)
			{
				yield return null;
				msReset.ResetAndRestart();
			}
		}
		yield return null;
		msReset.ResetAndRestart();
		if (Townships != null)
		{
			StampGroup roadLayer = new StampGroup("Road Layer");
			foreach (Township township in Townships)
			{
				if (township.Streets.Count > 0)
				{
					foreach (StreetTile value in township.Streets.Values)
					{
						if (value.Township != null)
						{
							roadLayer.Stamps.Add(value.GetStamp());
						}
					}
				}
				if (msReset.ElapsedMilliseconds > 500)
				{
					yield return null;
					msReset.ResetAndRestart();
				}
			}
			StampManager.DrawStampGroup(roadLayer, dest, WorldSize);
		}
		yield return null;
		msReset.ResetAndRestart();
		Color32 waterColor = new Color32(0, 0, byte.MaxValue, byte.MaxValue);
		Color32 radColor = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);
		for (int y = 0; y < roadMask.Length; y++)
		{
			int x = y % WorldSize;
			int y2 = y / WorldSize;
			if (roadMask[y].a > 0)
			{
				dest[y] = roadMask[y];
			}
			if (data.GetWater(y) > 0)
			{
				dest[y] = waterColor;
			}
			if (GetRad(x, y2) > 0)
			{
				dest[y] = radColor;
			}
			if (y % 50000 == 0 && msReset.ElapsedMilliseconds > 500)
			{
				yield return null;
				msReset.ResetAndRestart();
			}
		}
		Color32 color3 = new Color32(200, 200, byte.MaxValue, byte.MaxValue);
		Color32 color4 = new Color32(0, 0, 50, byte.MaxValue);
		for (int l = 0; l < playerSpawns.Count; l++)
		{
			PlayerSpawn playerSpawn = playerSpawns[l];
			int num3 = (int)playerSpawn.Position.x + (int)playerSpawn.Position.z * WorldSize;
			dest[num3 - WorldSize - 1] = color4;
			dest[num3 - WorldSize] = color3;
			dest[num3 - WorldSize + 1] = color4;
			dest[num3 - 1] = color3;
			dest[num3] = color4;
			dest[num3 + 1] = color3;
			dest[num3 + WorldSize - 1] = color4;
			dest[num3 + WorldSize] = color3;
			dest[num3 + WorldSize + 1] = color4;
		}
		int num4 = dest.Length;
		for (int m = 0; m < previewPoints.Count; m++)
		{
			PreviewPoint previewPoint = previewPoints[m];
			int num5 = previewPoint.pos.x + previewPoint.pos.y * WorldSize;
			int num6 = previewPoint.size / 2;
			int num7 = num5 - num6;
			for (int n = 0; n < previewPoint.size; n++)
			{
				dest[num7 + n] = previewPoint.color;
			}
			num7 = num5 - num6 * WorldSize;
			for (int num8 = 0; num8 < previewPoint.size; num8++)
			{
				int num9 = num7 + num8 * WorldSize;
				if ((uint)num9 < num4)
				{
					dest[num9] = previewPoint.color;
				}
			}
		}
		previewPoints.Clear();
		yield return null;
		if (WorldSize < 0)
		{
			yield break;
		}
		XUiC_WorldGenerationWindowGroup.PreviewQuality previewQualityLevel = XUiC_WorldGenerationWindowGroup.Instance.PreviewQualityLevel;
		if (previewQualityLevel == XUiC_WorldGenerationWindowGroup.PreviewQuality.NoPreview)
		{
			UnityEngine.Object.Destroy(PreviewImage);
			PreviewImage = new Texture2D(1, 1);
			yield break;
		}
		UnityEngine.Object.Destroy(PreviewImage);
		PreviewImage = new Texture2D(WorldSize, WorldSize);
		PreviewImage.SetPixels32(dest);
		if (previewQualityLevel >= XUiC_WorldGenerationWindowGroup.PreviewQuality.Default)
		{
			PreviewImage.Apply(updateMipmaps: true, makeNoLongerReadable: true);
			PreviewImage.filterMode = FilterMode.Point;
			yield break;
		}
		PreviewImage.Apply(updateMipmaps: false);
		int num10 = Mathf.CeilToInt(previewQualityLevel switch
		{
			XUiC_WorldGenerationWindowGroup.PreviewQuality.Lowest => 0.25f, 
			XUiC_WorldGenerationWindowGroup.PreviewQuality.Low => 0.5f, 
			_ => 0.5f, 
		} * (float)WorldSize);
		Texture2D texture2D = new Texture2D(num10, num10);
		PreviewImage.PointScaleNoAlloc(texture2D);
		texture2D.Apply(updateMipmaps: true, makeNoLongerReadable: true);
		UnityEngine.Object.Destroy(PreviewImage);
		PreviewImage = texture2D;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public StreetTile GetStreetTileGrid(Vector2i pos)
	{
		return GetStreetTileGrid(pos.x, pos.y);
	}

	public StreetTile GetStreetTileGrid(int x, int y)
	{
		if ((uint)x >= StreetTileMapWidth)
		{
			return null;
		}
		if ((uint)y >= StreetTileMapWidth)
		{
			return null;
		}
		return StreetTileMap[x + y * StreetTileMapWidth];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public StreetTile GetStreetTileWorld(Vector2i pos)
	{
		return GetStreetTileWorld(pos.x, pos.y);
	}

	public StreetTile GetStreetTileWorld(int x, int y)
	{
		x /= 150;
		if ((uint)x >= StreetTileMapWidth)
		{
			return null;
		}
		y /= 150;
		if ((uint)y >= StreetTileMapWidth)
		{
			return null;
		}
		return StreetTileMap[x + y * StreetTileMapWidth];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float GetHeight(Vector2i pos)
	{
		return GetHeight(pos.x, pos.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float GetHeight(int x, int y)
	{
		if ((uint)x >= WorldSize || (uint)y >= WorldSize)
		{
			return 0f;
		}
		return data.HeightMap[x + y * WorldSize];
	}

	[BurstCompile(CompileSynchronously = true)]
	public static void AdjustHeights(ref NativeArray<float> _src, float _min)
	{
		AdjustHeights_0000A3EA_0024BurstDirectCall.Invoke(ref _src, _min);
	}

	public void SetHeight(int x, int y, float height)
	{
		if ((uint)x < WorldSize && (uint)y < WorldSize)
		{
			data.HeightMap[x + y * WorldSize] = height;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetHeightTrusted(int x, int y, float height)
	{
		data.HeightMap[x + y * WorldSize] = height;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TerrainType GetTerrainType(Vector2i pos)
	{
		return GetTerrainType(pos.x, pos.y);
	}

	public TerrainType GetTerrainType(int x, int y)
	{
		x /= 256;
		if ((uint)x >= terrainTypeMap.width)
		{
			return TerrainType.none;
		}
		y /= 256;
		if ((uint)y >= terrainTypeMap.width)
		{
			return TerrainType.none;
		}
		return terrainTypeMap.Get(x, y);
	}

	public BiomeType GetBiome(Vector2i pos)
	{
		return GetBiome(pos.x, pos.y);
	}

	public BiomeType GetBiome(int x, int y)
	{
		int num = x / 8 + y / 8 * BiomeSize;
		if ((uint)num >= BiomeSize * BiomeSize)
		{
			return BiomeType.forest;
		}
		Color32 color = biomeDest[num];
		BiomeType result = BiomeType.forest;
		if (color.g == WorldBuilderConstants.burntForestCol.g)
		{
			result = BiomeType.burntForest;
		}
		else if (color.g == WorldBuilderConstants.desertCol.g)
		{
			result = BiomeType.desert;
		}
		else if (color.g == WorldBuilderConstants.snowCol.g)
		{
			result = BiomeType.snow;
		}
		else if (color.g == WorldBuilderConstants.wastelandCol.g)
		{
			result = BiomeType.wasteland;
		}
		return result;
	}

	public void SetWater(int x, int y, byte height)
	{
		if ((uint)x < WorldSize && (uint)y < WorldSize)
		{
			data.waterDest[x + y * WorldSize] = (int)height;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public byte GetRad(int x, int y)
	{
		if ((uint)x >= WorldSize || (uint)y >= WorldSize)
		{
			return 0;
		}
		return radDest[x / 32 + y / 32 * RadSize].r;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void serializePrefabs(Stream stream)
	{
		PrefabManager.SavePrefabData(stream);
		if (!UsePreviewer)
		{
			PrefabManager.UsedPrefabsWorld.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void serializeRawHeightmap(Stream stream)
	{
		HeightMapUtils.SaveHeightMapRAW(stream, data.HeightMap.ToArray(), -1f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator DrawRoads(Color32[] dest)
	{
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		byte[] ids = new byte[WorldSize * WorldSize];
		for (int i = 0; i < wildernessPaths.Count; i++)
		{
			wildernessPaths[i].DrawPathToRoadIds(ids);
			if (IsMessageElapsed())
			{
				yield return SetMessage(string.Format(Localization.Get("xuiRwgDrawRoadsWilderness"), 100 * i / wildernessPaths.Count));
			}
		}
		for (int i = 0; i < highwayPaths.Count; i++)
		{
			highwayPaths[i].DrawPathToRoadIds(ids);
			if (IsMessageElapsed())
			{
				yield return SetMessage(string.Format(Localization.Get("xuiRwgDrawRoadsProgress"), 100 * i / highwayPaths.Count));
			}
		}
		PathShared.ConvertIdsToColors(ids, dest);
		Log.Out($"DrawRoads in {(float)ms.ElapsedMilliseconds * 0.001f}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void serializePlayerSpawns(Stream stream)
	{
		using StreamWriter streamWriter = new StreamWriter(stream, SdEncoding.UTF8NoBOM, 1024, leaveOpen: true);
		streamWriter.WriteLine("<spawnpoints>");
		if (playerSpawns != null)
		{
			for (int i = 0; i < playerSpawns.Count; i++)
			{
				PlayerSpawn playerSpawn = playerSpawns[i];
				streamWriter.WriteLine($"    <spawnpoint position=\"{(playerSpawn.Position.x - (float)WorldSize / 2f).ToCultureInvariantString()},{playerSpawn.Position.y.ToCultureInvariantString()},{(playerSpawn.Position.z - (float)WorldSize / 2f).ToCultureInvariantString()}\" rotation=\"0,{playerSpawn.Rotation},0\"/>");
			}
		}
		streamWriter.WriteLine("</spawnpoints>");
	}

	public IEnumerator SetMessage(string _message, bool _logToConsole = false, bool _ignoreCancel = false)
	{
		if (_message != null)
		{
			_message += string.Format("\n{0} {1}:{2:00}", Localization.Get("xuiTime"), totalMS.Elapsed.Minutes, totalMS.Elapsed.Seconds);
		}
		if (!GameManager.IsDedicatedServer)
		{
			if (!_ignoreCancel)
			{
				IsCanceled |= CheckCancel();
			}
			if (_message != null)
			{
				if (!_ignoreCancel && IsCanceled)
				{
					_message = "Canceling...";
				}
				if (!XUiC_ProgressWindow.IsWindowOpen())
				{
					XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, _message, null, _modal: true, _escClosable: false, _closeOpenWindows: false, _useShadow: true);
				}
				else if (_message != setMessageLast)
				{
					setMessageLast = _message;
					XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, _message);
				}
			}
			else
			{
				setMessageLast = string.Empty;
				XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
			}
			yield return endOfFrameHandle;
		}
		if (_logToConsole && _message != null)
		{
			Log.Out("WorldGenerator:" + _message.Replace("\n", ": "));
		}
		yield return null;
	}

	public bool IsMessageElapsed()
	{
		if (messageMS.ElapsedMilliseconds > 600)
		{
			messageMS.ResetAndRestart();
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckCancel()
	{
		if (UsePreviewer)
		{
			return PlatformManager.NativePlatform.Input.PrimaryPlayer.GUIActions.Cancel;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i getRotatedPoint(int x, int y, int cx, int cy, int angle)
	{
		return new Vector2i(Mathf.RoundToInt((float)((double)(x - cx) * Math.Cos(angle) - (double)(y - cy) * Math.Sin(angle) + (double)cx)), Mathf.RoundToInt((float)((double)(x - cx) * Math.Sin(angle) + (double)(y - cy) * Math.Cos(angle) + (double)cy)));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetTerrainProperties(string biomeTypeName, string terrainTypeName, string comboTypeName, out Vector2 _scaleMinMax, out int _clusterCount, out int _clusterRadius, out float _clusterStrength, out bool useBiomeMask, out float biomeCutoff)
	{
		_scaleMinMax = Vector2.one;
		string text = thisWorldProperties.GetString(comboTypeName + ".scale");
		if (text == string.Empty)
		{
			text = thisWorldProperties.GetString(terrainTypeName + ".scale");
		}
		if (text != string.Empty)
		{
			_scaleMinMax = StringParsers.ParseVector2(text);
		}
		_scaleMinMax *= 0.5f;
		_clusterCount = 3;
		_clusterRadius = 85;
		_clusterStrength = 1f;
		text = thisWorldProperties.GetString(comboTypeName + ".clusters");
		if (text == string.Empty)
		{
			text = thisWorldProperties.GetString(terrainTypeName + ".clusters");
		}
		if (text != string.Empty)
		{
			Vector3 vector = StringParsers.ParseVector3(text);
			_clusterCount = (int)vector.x;
			_clusterRadius = (int)(256f * vector.y);
			_clusterStrength = vector.z;
		}
		useBiomeMask = false;
		text = thisWorldProperties.GetString(comboTypeName + ".use_biome_mask");
		if (text == string.Empty)
		{
			text = thisWorldProperties.GetString(terrainTypeName + ".use_biome_mask");
		}
		if (text != string.Empty)
		{
			useBiomeMask = StringParsers.ParseBool(text);
		}
		biomeCutoff = 0.1f;
		text = thisWorldProperties.GetString(comboTypeName + ".biome_mask_min");
		if (text == string.Empty)
		{
			text = thisWorldProperties.GetString(terrainTypeName + ".biome_mask_min");
		}
		if (text != string.Empty)
		{
			biomeCutoff = StringParsers.ParseFloat(text);
		}
	}

	public static string GetGeneratedWorldName(string _worldSeedName, int _worldSize = 8192)
	{
		return RandomCountyNameGenerator.GetName(_worldSeedName.GetHashCode() + _worldSize);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int distanceSqr(Vector2i pointA, Vector2i pointB)
	{
		Vector2i vector2i = pointA - pointB;
		return vector2i.x * vector2i.x + vector2i.y * vector2i.y;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float distanceSqr(Vector2 pointA, Vector2 pointB)
	{
		Vector2 vector = pointA - pointB;
		return vector.x * vector.x + vector.y * vector.y;
	}

	public int GetCount(string _name, GenerationSelections _selection, GameRandom _rand = null)
	{
		float optionalValue = -1f;
		float optionalValue2 = 0f;
		float optionalValue3 = 0f;
		thisWorldProperties.ParseVec($"{_name}.count", ref optionalValue, ref optionalValue2, ref optionalValue3);
		if (optionalValue < 0f)
		{
			return -1;
		}
		float num = optionalValue;
		switch (_selection)
		{
		case GenerationSelections.Default:
			num = optionalValue2;
			break;
		case GenerationSelections.Many:
			num = optionalValue3;
			break;
		}
		int num2 = (int)num;
		if (_rand != null && _rand.RandomFloat < num - (float)num2)
		{
			num2++;
		}
		return num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TestGenerateHeights()
	{
		for (int i = 0; i < WorldSize; i++)
		{
			float num = 0f;
			for (int j = 0; j < WorldSize; j++)
			{
				int index = j + i * WorldSize;
				data.HeightMap[index] = num;
				if ((j & 3) == 3)
				{
					num += 2f;
					if (num > 255f)
					{
						num = 0f;
					}
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ClearWaterUnderTerrain_0024BurstManaged(ref Data _data, ref NativeArray<float> _terrain, int startX, int endX, int startY, int endY)
	{
		for (int i = startY; i <= endY; i++)
		{
			for (int j = startX; j <= endX; j++)
			{
				int index = j + i * _data.WorldSize;
				if (_terrain[index] - 0.5f > _data.waterDest[index])
				{
					_data.waterDest[index] = 0f;
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void FinalizeWater_0024BurstManaged(ref Data _data, float _WaterHeight)
	{
		for (int i = 0; i < _data.HeightMap.Length; i++)
		{
			if (_data.HeightMap[i] - 0.5f > _data.waterDest[i])
			{
				_data.waterDest[i] = 0f;
			}
			else
			{
				_data.waterDest[i] = _WaterHeight;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void SmoothRoadTerrainTask_0024BurstManaged(ref Data _data, ref NativeArray<Color32> roadMask, ref NativeArray<float> _heightMap, int WorldSize)
	{
		int length = WorldSize * WorldSize;
		NativeArray<ushort> nativeArray = new NativeArray<ushort>(length, Allocator.Persistent);
		for (int i = 0; i < WorldSize; i++)
		{
			for (int j = 0; j < WorldSize; j++)
			{
				int index = j + i * WorldSize;
				if (_data.poiHeightMask[index] > 0)
				{
					nativeArray[index] = 1000;
					continue;
				}
				int r = roadMask[index].r;
				if (r + roadMask[index].g <= 0)
				{
					continue;
				}
				_heightMap[index] += 0.0008f;
				nativeArray[index] = 200;
				int num = 80;
				int num2 = 3;
				int num3 = 30;
				if (r > 0)
				{
					nativeArray[index] = 255;
					num = 60;
					num2 = 6;
					num3 = 8;
				}
				for (int k = 1; k <= num2; k++)
				{
					for (int l = 0; l < 8; l++)
					{
						int num4 = j + directions8way[l].x * k;
						if ((uint)num4 >= WorldSize)
						{
							continue;
						}
						int num5 = i + directions8way[l].y * k;
						if ((uint)num5 < WorldSize)
						{
							int index2 = num4 + num5 * WorldSize;
							if (num > nativeArray[index2])
							{
								nativeArray[index2] = (ushort)num;
							}
						}
					}
					num -= num3;
				}
			}
		}
		int num6 = WorldSize - 1;
		int num7 = WorldSize - 1;
		NativeArray<float> array = new NativeArray<float>(length, Allocator.Persistent);
		int num8 = 6;
		while (num8-- > 0)
		{
			_heightMap.CopyTo(array);
			for (int m = 1; m < num7; m++)
			{
				int num9 = m * WorldSize;
				for (int n = 1; n < num6; n++)
				{
					int num10 = n + num9;
					if (roadMask[num10].r != 0 && nativeArray[num10] < 1000)
					{
						float num12;
						float num11 = (num12 = array[num10]);
						int num13 = num10 - WorldSize - 1;
						float num14 = num11;
						if (nativeArray[num13] >= 255)
						{
							num14 = array[num13];
						}
						num12 += num14 * 0.25f;
						num14 = num11;
						if (nativeArray[++num13] >= 255)
						{
							num14 = array[num13];
						}
						num12 += num14 * 0.5f;
						num14 = num11;
						if (nativeArray[++num13] >= 255)
						{
							num14 = array[num13];
						}
						num12 += num14 * 0.25f;
						num13 = num10 - 1;
						num14 = num11;
						if (nativeArray[num13] >= 255)
						{
							num14 = array[num13];
						}
						num12 += num14 * 0.5f;
						num13 += 2;
						num14 = num11;
						if (nativeArray[num13] >= 255)
						{
							num14 = array[num13];
						}
						num12 += num14 * 0.5f;
						num13 = num10 + WorldSize - 1;
						num14 = num11;
						if (nativeArray[num13] >= 255)
						{
							num14 = array[num13];
						}
						num12 += num14 * 0.25f;
						num14 = num11;
						if (nativeArray[++num13] >= 255)
						{
							num14 = array[num13];
						}
						num12 += num14 * 0.5f;
						num14 = num11;
						if (nativeArray[++num13] >= 255)
						{
							num14 = array[num13];
						}
						num12 += num14 * 0.25f;
						_heightMap[num10] = num12 / 4f;
					}
				}
			}
			_data.messageCnt++;
		}
		_data.messageCnt = 100;
		int num15 = 30;
		while (num15-- > 0)
		{
			_heightMap.CopyTo(array);
			for (int num16 = 1; num16 < num7; num16++)
			{
				int num17 = num16 * WorldSize;
				for (int num18 = 1; num18 < num6; num18++)
				{
					int num19 = num18 + num17;
					int num20 = nativeArray[num19];
					if (num20 == 0 || num20 > 200)
					{
						continue;
					}
					int num21 = 0;
					float num22 = 0f;
					int num23 = num19 - WorldSize - 1;
					int num24 = nativeArray[num23] / 2;
					num22 += array[num23] * (float)num24;
					num21 += num24;
					num24 = nativeArray[++num23];
					num22 += array[num23] * (float)num24;
					num21 += num24;
					num24 = nativeArray[++num23] / 2;
					num22 += array[num23] * (float)num24;
					num21 += num24;
					num23 = num19 - 1;
					num24 = nativeArray[num23];
					num22 += array[num23] * (float)num24;
					num21 += num24;
					num23 = num19 + 1;
					num24 = nativeArray[num23];
					num22 += array[num23] * (float)num24;
					num21 += num24;
					num23 = num19 + WorldSize - 1;
					num24 = nativeArray[num23] / 2;
					num22 += array[num23] * (float)num24;
					num21 += num24;
					num24 = nativeArray[++num23];
					num22 += array[num23] * (float)num24;
					num21 += num24;
					num24 = nativeArray[++num23] / 2;
					num22 += array[num23] * (float)num24;
					num21 += num24;
					if (num21 > 0)
					{
						if (num20 < 200)
						{
							float num25 = (float)num20 * 0.005f;
							_heightMap[num19] = _heightMap[num19] * (1f - num25) + num22 / (float)num21 * num25;
						}
						else
						{
							_heightMap[num19] = num22 / (float)num21;
						}
					}
				}
			}
			_data.messageCnt++;
		}
		nativeArray.Dispose();
		array.Dispose();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(CompileSynchronously = true)]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void AdjustHeights_0024BurstManaged(ref NativeArray<float> _src, float _min)
	{
		for (int i = 0; i < _src.Length; i++)
		{
			_src[i] = Utils.FastMax(_src[i], _min);
		}
	}
}
