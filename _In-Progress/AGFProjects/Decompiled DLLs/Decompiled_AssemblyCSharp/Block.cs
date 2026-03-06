using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class Block
{
	public struct SItemDropProb(string _name, int _minCount, int _maxCount, float _prob, float _resourceScale, float _stickChance, string _toolCategory, string _tag)
	{
		public string name = _name;

		public int minCount = _minCount;

		public int maxCount = _maxCount;

		public float prob = _prob;

		public float resourceScale = _resourceScale;

		public float stickChance = _stickChance;

		public string toolCategory = _toolCategory;

		public string tag = _tag;
	}

	public struct SItemNameCount
	{
		public string ItemName;

		public int Count;
	}

	public class MultiBlockArray
	{
		public int Length;

		public Vector3i dim;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3i[] pos;

		public MultiBlockArray(Vector3i _dim, List<Vector3i> _pos)
		{
			dim = _dim;
			pos = _pos.ToArray();
			Length = _pos.Count;
		}

		public Vector3i Get(int _idx, int _blockId, int _rotation)
		{
			Vector3 vector = list[_blockId].shape.GetRotation(new BlockValue
			{
				type = _blockId,
				rotation = (byte)_rotation
			}) * pos[_idx].ToVector3();
			return new Vector3i(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
		}

		public Vector3i GetParentPos(Vector3i _childPos, BlockValue _blockValue)
		{
			return new Vector3i(_childPos.x + _blockValue.parentx, _childPos.y + _blockValue.parenty, _childPos.z + _blockValue.parentz);
		}

		public void AddChilds(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
		{
			ChunkCluster chunkCache = _world.ChunkCache;
			if (chunkCache == null)
			{
				return;
			}
			byte rotation = _blockValue.rotation;
			for (int num = Length - 1; num >= 0; num--)
			{
				Vector3i vector3i = Get(num, _blockValue.type, rotation);
				if (!(vector3i == Vector3i.zero))
				{
					Vector3i blockPos = _blockPos + vector3i;
					int x = World.toBlockXZ(blockPos.x);
					int z = World.toBlockXZ(blockPos.z);
					int y = blockPos.y;
					if (y >= 0 && y < 254)
					{
						Chunk chunk = (Chunk)chunkCache.GetChunkFromWorldPos(blockPos);
						if (chunk == null)
						{
							long num2 = WorldChunkCache.MakeChunkKey(World.toChunkXZ(blockPos.x), World.toChunkXZ(blockPos.z));
							if (_chunk.Key == num2)
							{
								chunk = _chunk;
							}
						}
						if (chunk != null)
						{
							BlockValue block = chunk.GetBlock(x, y, z);
							if (block.isair || !block.Block.shape.IsTerrain())
							{
								BlockValue blockValue = _blockValue;
								blockValue.ischild = true;
								blockValue.parentx = -vector3i.x;
								blockValue.parenty = -vector3i.y;
								blockValue.parentz = -vector3i.z;
								chunk.SetBlock(_world, x, y, z, blockValue, _notifyAddChange: false);
							}
						}
					}
				}
			}
		}

		public void RemoveChilds(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
		{
			ChunkCluster chunkCache = _world.ChunkCache;
			if (chunkCache == null)
			{
				return;
			}
			byte rotation = _blockValue.rotation;
			for (int num = Length - 1; num >= 0; num--)
			{
				Vector3i vector3i = Get(num, _blockValue.type, rotation);
				if ((vector3i.x != 0 || vector3i.y != 0 || vector3i.z != 0) && chunkCache.GetBlock(_blockPos + vector3i).type == _blockValue.type)
				{
					chunkCache.SetBlock(_blockPos + vector3i, _isChangeBV: true, BlockValue.Air, _isChangeDensity: true, MarchingCubes.DensityAir, _isNotify: false, _isUpdateLight: false, _isForceDensity: false, _wasChild: true);
				}
			}
		}

		public void RemoveParentBlock(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
		{
			ChunkCluster chunkCache = _world.ChunkCache;
			if (chunkCache != null)
			{
				Vector3i parentPos = GetParentPos(_blockPos, _blockValue);
				BlockValue block = chunkCache.GetBlock(parentPos);
				if (!block.ischild && block.type == _blockValue.type)
				{
					chunkCache.SetBlock(parentPos, BlockValue.Air, _isNotify: true, _isUpdateLight: true);
				}
			}
		}

		public bool ContainsPos(WorldBase _world, Vector3i _parentPos, BlockValue _blockValue, Vector3i _posToCheck)
		{
			if (_world.ChunkCache == null)
			{
				return false;
			}
			byte rotation = _blockValue.rotation;
			for (int num = Length - 1; num >= 0; num--)
			{
				if (_parentPos + Get(num, _blockValue.type, rotation) == _posToCheck)
				{
					return true;
				}
			}
			return false;
		}

		public Bounds CalcBounds(int _blockId, int _rotation)
		{
			Quaternion rotation = list[_blockId].shape.GetRotation(new BlockValue
			{
				type = _blockId,
				rotation = (byte)_rotation
			});
			Vector3 vector = Vector3.positiveInfinity;
			Vector3 vector2 = Vector3.negativeInfinity;
			for (int num = Length - 1; num >= 0; num--)
			{
				Vector3 rhs = rotation * pos[num].ToVector3();
				vector = Vector3.Min(vector, rhs);
				vector2 = Vector3.Max(vector2, rhs);
			}
			Bounds result = default(Bounds);
			result.SetMinMax(vector, vector2);
			return result;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct TextureInfo
	{
		public bool bTextureForEachSide;

		public int singleTextureId;

		public int[] sideTextureIds;
	}

	public enum UVMode : byte
	{
		Default,
		Global,
		Local
	}

	public enum EnumDisplayInfo
	{
		None,
		Name,
		Description,
		Custom
	}

	public enum DestroyedResult
	{
		Keep,
		Downgrade,
		Remove
	}

	public const int cAirId = 0;

	public const int cTerrainStartId = 1;

	public const int cWaterId = 240;

	public const int cWaterPOIId = 241;

	public const int cWaterDataId = 242;

	public const int cGeneralStartId = 256;

	public static int MAX_BLOCKS = 65536;

	public static int ItemsStartHere = MAX_BLOCKS;

	public static bool FallInstantly = false;

	public const int BlockFaceDrawn_Top = 1;

	public const int BlockFaceDrawn_Bottom = 2;

	public const int BlockFaceDrawn_North = 4;

	public const int BlockFaceDrawn_West = 8;

	public const int BlockFaceDrawn_South = 16;

	public const int BlockFaceDrawn_East = 32;

	public const int BlockFaceDrawn_AllORD = 63;

	public const int BlockFaceDrawn_All = 255;

	public static float cWaterLevel = 62.88f;

	public static string PropCanPickup = "CanPickup";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPickupTarget = "PickupTarget";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPickupSource = "PickupSource";

	public static string PropPlaceAltBlockValue = "PlaceAltBlockValue";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPlaceShapeCategories = "ShapeCategories";

	public static string PropSiblingBlock = "SiblingBlock";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropFuelValue = "FuelValue";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropWeight = "Weight";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCanMobsSpawnOn = "CanMobsSpawnOn";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCanPlayersSpawnOn = "CanPlayersSpawnOn";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIndexName = "IndexName";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCanBlocksReplace = "CanBlocksReplace";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCanDecorateOnSlopes = "CanDecorateOnSlopes";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSlopeMax = "SlopeMax";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIsTerrainDecoration = "IsTerrainDecoration";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIsDecoration = "IsDecoration";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDistantDecoration = "IsDistantDecoration";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBigDecorationRadius = "BigDecorationRadius";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSmallDecorationRadius = "SmallDecorationRadius";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGndAlign = "GndAlign";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIgnoreKeystoneOverlay = "IgnoreKeystoneOverlay";

	public static string PropUpgradeBlockClass = "UpgradeBlock";

	public static string PropUpgradeBlockClassToBlock = PropUpgradeBlockClass + ".ToBlock";

	public static string PropUpgradeBlockItemCount = "ItemCount";

	public static string PropUpgradeBlockClassItemCount = PropUpgradeBlockClass + ".ItemCount";

	public static string PropDowngradeBlock = "DowngradeBlock";

	public static string PropLockpickDowngradeBlock = "LockPickDowngradeBlock";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropLPScale = "LPHardnessScale";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMapColor = "Map.Color";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMapColor2 = "Map.Color2";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMapSpecular = "Map.Specular";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMapElevMinMax = "Map.ElevMinMax";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGroupName = "Group";

	public static string PropCustomIcon = "CustomIcon";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCustomIconTint = "CustomIconTint";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPlacementWireframe = "PlacementWireframe";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMultiBlockDim = "MultiBlockDim";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOversizedBounds = "OversizedBounds";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTerrainAlignment = "TerrainAlignment";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMultiBlockLayer = "MultiBlockLayer";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMultiBlockLayer0 = "MultiBlockLayer0";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIsPlant = "IsPlant";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropHeatMapStrength = "HeatMapStrength";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropFallDamage = "FallDamage";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBuffsWhenWalkedOn = "BuffsWhenWalkedOn";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRadiusEffect = "ActiveRadiusEffects";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCount = "Count";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAllowAllRotations = "AllowAllRotations";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropActivationDistance = "ActivationDistance";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPlacementDistance = "PlacementDistance";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIsReplaceRandom = "IsReplaceRandom";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCraftExpValue = "CraftComponentExpValue";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCraftTimeValue = "CraftComponentTimeValue";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropLootExpValue = "LootExpValue";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDestroyExpValue = "DestroyExpValue";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropParticleOnDeath = "ParticleOnDeath";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPlaceExpValue = "PlaceExpValue";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropUpgradeExpValue = "UpgradeExpValue";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropEconomicValue = "EconomicValue";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropEconomicSellScale = "EconomicSellScale";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropEconomicBundleSize = "EconomicBundleSize";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSellableToTrader = "SellableToTrader";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTraderStageTemplate = "TraderStageTemplate";

	public static string PropResourceScale = "ResourceScale";

	public static string PropMaxDamage = "MaxDamage";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropStartDamage = "StartDamage";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropStage2Health = "Stage2Health";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDamage = "Damage";

	public static string PropDescriptionKey = "DescriptionKey";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropActionSkillGroup = "ActionSkillGroup";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCraftingSkillGroup = "CraftingSkillGroup";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropShowModelOnFall = "ShowModelOnFall";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropLightOpacity = "LightOpacity";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropHarvestOverdamage = "HarvestOverdamage";

	public static string PropTintColor = "TintColor";

	public static string PropCreativeMode = "CreativeMode";

	public static string PropFilterTag = "FilterTags";

	public static string PropTag = "Tags";

	public static string PropCreativeSort1 = "SortOrder1";

	public static string PropCreativeSort2 = "SortOrder2";

	public static string PropDisplayType = "DisplayType";

	public static string PropUnlockedBy = "UnlockedBy";

	public static string PropNoScrapping = "NoScrapping";

	public static string PropVehicleHitScale = "VehicleHitScale";

	public static string PropItemTypeIcon = "ItemTypeIcon";

	public static string PropAutoShape = "AutoShape";

	public static string PropBlockAddedEvent = "AddedEvent";

	public static string PropBlockDestroyedEvent = "DestroyedEvent";

	public static string PropBlockDowngradeEvent = "DowngradeEvent";

	public static string PropBlockDowngradedToEvent = "DowngradedToEvent";

	public static string PropIsTemporaryBlock = "IsTemporaryBlock";

	public static string PropRefundOnUnload = "RefundOnUnload";

	public static string PropSoundPickup = "SoundPickup";

	public static string PropSoundPlace = "SoundPlace";

	public static string PropCustomCommandName = "CustomCommandName";

	public static string PropCustomCommandIcon = "CustomCommandIcon";

	public static string PropCustomCommandIconColor = "CustomCommandIconColor";

	public static string PropCustomCommandEvent = "CustomCommandEvent";

	public static string PropCustomCommandActivateTime = "CustomCommandActivateTime";

	public static NameIdMapping nameIdMapping;

	public static byte[] fullMappingDataForClients;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, Block> nameToBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, Block> nameToBlockCaseInsensitive;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, string[]> groupNameStringToGroupNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<string> autoShapeMaterials;

	public static Block[] list;

	public static DynamicPropertiesCache PropertiesCache;

	public static string defaultBlockDescriptionKey = "";

	public int blockID;

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicProperties dynamicProperties;

	public BlockShape shape;

	public int BlockingType;

	public BlockValue SiblingBlock;

	public BlockTags BlockTag;

	public BlockPlacement BlockPlacementHelper;

	public bool CanBlocksReplace;

	public float LPHardnessScale;

	public float MovementFactor;

	public bool CanPickup;

	public string PickedUpItemValue;

	public string PickupTarget;

	public string PickupSource;

	public byte BlocksMovement;

	public int FuelValue;

	public DataItem<int> Weight;

	public bool CanMobsSpawnOn;

	public bool CanPlayersSpawnOn;

	public string IndexName;

	public bool CanDecorateOnSlopes;

	public float SlopeMaxCos;

	public bool IsTerrainDecoration;

	public bool IsDecoration;

	public bool IsDistantDecoration;

	public int BigDecorationRadius;

	public int SmallDecorationRadius;

	public float GroundAlignDistance;

	public bool IgnoreKeystoneOverlay;

	public const int cPathScan = -1;

	public const int cPathSolid = 1;

	public int PathType;

	public float PathOffsetX;

	public float PathOffsetZ;

	public BlockFaceFlag WaterFlowMask = BlockFaceFlag.All;

	public bool WaterClipEnabled;

	public Plane WaterClipPlane;

	public BlockValue DowngradeBlock;

	public BlockValue LockpickDowngradeBlock;

	public BlockValue UpgradeBlock;

	public string[] GroupNames = new string[1] { "Decor/Miscellaneous" };

	public string CustomIcon;

	public Color CustomIconTint;

	public bool bHasPlacementWireframe;

	public bool bUserHidden;

	public float FallDamage;

	public float HeatMapStrength;

	public string[] BuffsWhenWalkedOn;

	public BlockRadiusEffect[] RadiusEffects;

	public string DescriptionKey;

	public string CraftingSkillGroup = "";

	public string ActionSkillGroup = "";

	public bool IsReplaceRandom = true;

	public float CraftComponentExp = 1f;

	public float CraftComponentTime = 1f;

	public float LootExp = 1f;

	public float DestroyExp = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string deathParticleName;

	public float EconomicValue;

	public float EconomicSellScale = 1f;

	public int EconomicBundleSize = 1;

	public bool SellableToTrader = true;

	public string TraderStageTemplate;

	public float PlaceExp = 1f;

	public float UpgradeExp = 1f;

	public int Count = 1;

	public int Stacknumber = 500;

	public bool HarvestOverdamage;

	public bool SelectAlternates;

	public EnumCreativeMode CreativeMode;

	public string[] FilterTags;

	public bool NoScrapping;

	public string SortOrder;

	public string DisplayType = "defaultBlock";

	[PublicizedFrom(EAccessModifier.Private)]
	public RecipeUnlockData[] unlockedBy;

	public string ItemTypeIcon = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public EAutoShapeType AutoShapeType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string autoShapeBaseName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string autoShapeShapeName;

	[PublicizedFrom(EAccessModifier.Private)]
	public Block autoShapeHelper;

	public float VehicleHitScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color MapColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bMapColorSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color MapColor2;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bMapColor2Set;

	[PublicizedFrom(EAccessModifier.Private)]
	public float MapSpecular;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i MapElevMinMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte lightValue;

	public int lightOpacity;

	public Color tintColor = Color.clear;

	public Color defaultTintColor = Color.clear;

	public Vector3 tintColorV = Vector3.one;

	public byte MeshIndex;

	public List<SItemNameCount> RepairItems;

	public List<SItemNameCount> RepairItemsMeshDamage;

	public bool bRestrictSubmergedPlacement;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string blockAddedEvent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string blockDestroyedEvent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string blockDowngradeEvent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string blockDowngradedToEvent;

	public bool IsTemporaryBlock;

	public bool RefundOnUnload;

	public string SoundPickup = "craft_take_item";

	public string SoundPlace = "craft_place_item";

	public bool isMultiBlock;

	public MultiBlockArray multiBlockPos;

	public bool isOversized;

	public Bounds oversizedBounds;

	public TerrainAlignmentMode terrainAlignmentMode;

	public const int BT_All = 255;

	public const int BT_None = 0;

	public const int BT_Sight = 1;

	public const int BT_Movement = 2;

	public const int BT_Bullets = 4;

	public const int BT_Rockets = 8;

	public const int BT_Melee = 16;

	public const int BT_Arrows = 32;

	public bool IsCheckCollideWithEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public TextureInfo[] textureInfos = new TextureInfo[1];

	[PublicizedFrom(EAccessModifier.Private)]
	public int uiBackgroundTextureId = -1;

	public int TerrainTAIndex = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bNotifyOnLoadUnload;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bIsPlant;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bShowModelOnFall;

	public Dictionary<EnumDropEvent, List<SItemDropProb>> itemsToDrop = new EnumDictionary<EnumDropEvent, List<SItemDropProb>>();

	public bool IsSleeperBlock;

	public bool IsRandomlyTick;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] placeAltBlockNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public Block[] placeAltBlockClasses;

	public MaterialBlock blockMaterial;

	public bool StabilitySupport = true;

	public bool StabilityIgnore;

	public bool StabilityFull;

	public sbyte Density;

	[PublicizedFrom(EAccessModifier.Private)]
	public string blockName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string localizedBlockName;

	public float ResourceScale;

	public int MaxDamage;

	public int MaxDamagePlusDowngrades;

	public int StartDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	public int Stage2Health;

	public float Damage;

	public EBlockRotationClasses AllowedRotations;

	public bool PlaceRandomRotation;

	public string CustomPlaceSound;

	public string UpgradeSound;

	public string DowngradeFX;

	public string DestroyFX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int activationDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public int placementDistance;

	public int cUVModeBits = 2;

	public int cUVModeMask = 3;

	public int cUVModeSides = 7;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly uint[] UVModesPerSide = new uint[1];

	public bool bImposterExclude;

	public bool bImposterExcludeAndStop;

	public int ImposterExchange;

	public byte ImposterExchangeTexIdx;

	public bool bImposterDontBlock;

	public int MergeIntoId;

	public int[] MergeIntoTexIds;

	public int MirrorSibling;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static List<Bounds> staticList_IntersectRayWithBlockList = new List<Bounds>();

	public BlockFace HandleFace = BlockFace.None;

	public bool EnablePassThroughDamage;

	public List<BlockFace> RemovePaintOnDowngrade;

	public FastTags<TagGroup.Global> Tags;

	public bool HasTileEntity;

	public EnumDisplayInfo DisplayInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("take", "hand", _enabled: false),
		new BlockActivationCommand("Search", "search", _enabled: false)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ItemStack> itemsDropped = new List<ItemStack>();

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockActivationCommand[] customCmds;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, int> fixedBlockIds = new Dictionary<string, int>
	{
		{ "air", 0 },
		{ "water", 240 },
		{ "terrWaterPOI", 241 },
		{ "waterdata", 242 }
	};

	public static bool BlocksLoaded => list != null;

	public DynamicProperties Properties
	{
		get
		{
			if (dynamicProperties != null)
			{
				return dynamicProperties;
			}
			return PropertiesCache.Cache(blockID);
		}
		set
		{
			dynamicProperties = value;
		}
	}

	public RecipeUnlockData[] UnlockedBy
	{
		get
		{
			if (unlockedBy == null)
			{
				if (Properties.Values.ContainsKey(PropUnlockedBy))
				{
					string[] array = Properties.Values[PropUnlockedBy].Split(',');
					if (array.Length != 0)
					{
						unlockedBy = new RecipeUnlockData[array.Length];
						for (int i = 0; i < array.Length; i++)
						{
							unlockedBy[i] = new RecipeUnlockData(array[i]);
						}
					}
				}
				else
				{
					unlockedBy = new RecipeUnlockData[0];
				}
			}
			return unlockedBy;
		}
	}

	public bool IsCollideMovement => (BlockingType & 2) != 0;

	public bool IsCollideSight => (BlockingType & 1) != 0;

	public bool IsCollideBullets => (BlockingType & 4) != 0;

	public bool IsCollideRockets => (BlockingType & 8) != 0;

	public bool IsCollideMelee => (BlockingType & 0x10) != 0;

	public bool IsCollideArrows => (BlockingType & 0x20) != 0;

	public bool IsNotifyOnLoadUnload
	{
		get
		{
			if (!bNotifyOnLoadUnload)
			{
				return shape.IsNotifyOnLoadUnload;
			}
			return true;
		}
		set
		{
			bNotifyOnLoadUnload = value;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public List<ShapesFromXml.ShapeCategory> ShapeCategories
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public virtual bool AllowBlockTriggers => false;

	public BlockActivationCommand[] CustomCmds
	{
		get
		{
			if (customCmds == null)
			{
				int num = 0;
				for (int i = 1; i <= 10 && Properties.Values.ContainsKey($"{PropCustomCommandName}{i}"); i++)
				{
					num++;
				}
				customCmds = new BlockActivationCommand[num];
				if (num > 0)
				{
					for (int j = 1; j <= num; j++)
					{
						if (Properties.Values.ContainsKey($"{PropCustomCommandName}{j}"))
						{
							BlockActivationCommand blockActivationCommand = new BlockActivationCommand
							{
								text = Properties.Values[$"{PropCustomCommandName}{j}"],
								icon = Properties.Values[$"{PropCustomCommandIcon}{j}"],
								eventName = Properties.Values[$"{PropCustomCommandEvent}{j}"]
							};
							string text = $"{PropCustomCommandIconColor}{j}";
							if (Properties.Values.ContainsKey(text))
							{
								blockActivationCommand.iconColor = StringParsers.ParseHexColor(Properties.Values[text]);
							}
							else
							{
								blockActivationCommand.iconColor = Color.white;
							}
							text = $"{PropCustomCommandActivateTime}{j}";
							if (Properties.Values.ContainsKey(text))
							{
								blockActivationCommand.activateTime = StringParsers.ParseFloat(Properties.Values[text]);
							}
							else
							{
								blockActivationCommand.activateTime = -1f;
							}
							blockActivationCommand.enabled = true;
							customCmds[j - 1] = blockActivationCommand;
						}
					}
				}
			}
			return customCmds;
		}
	}

	public Block()
	{
		shape = new BlockShapeCube();
		shape.Init(this);
		Properties = new DynamicProperties();
		blockMaterial = MaterialBlock.air;
		MeshIndex = 0;
	}

	public static Vector3 StringToVector3(string _input)
	{
		Vector3 zero = Vector3.zero;
		StringParsers.SeparatorPositions separatorPositions = StringParsers.GetSeparatorPositions(_input, ',', 2);
		int _result = 255;
		int _result2 = 255;
		int _result3 = 255;
		StringParsers.TryParseSInt32(_input, out _result, 0, separatorPositions.Sep1 - 1);
		if (separatorPositions.TotalFound > 0)
		{
			StringParsers.TryParseSInt32(_input, out _result2, separatorPositions.Sep1 + 1, separatorPositions.Sep2 - 1);
		}
		if (separatorPositions.TotalFound > 1)
		{
			StringParsers.TryParseSInt32(_input, out _result3, separatorPositions.Sep2 + 1, separatorPositions.Sep3 - 1);
		}
		zero.x = (float)_result / 255f;
		zero.y = (float)_result2 / 255f;
		zero.z = (float)_result3 / 255f;
		return zero;
	}

	public virtual void Init()
	{
		if (nameToBlockCaseInsensitive.ContainsKey(blockName))
		{
			Log.Error("Block " + blockName + " is found multiple times, overriding with latest definition!");
		}
		nameToBlock[blockName] = this;
		nameToBlockCaseInsensitive[blockName] = this;
		if (Properties.Values.ContainsKey(PropTag))
		{
			Tags = FastTags<TagGroup.Global>.Parse(Properties.Values[PropTag]);
		}
		if (Properties.Values.ContainsKey(PropLightOpacity))
		{
			int.TryParse(Properties.Values[PropLightOpacity], out lightOpacity);
		}
		else
		{
			lightOpacity = Math.Max(blockMaterial.LightOpacity, shape.LightOpacity);
		}
		Properties.ParseColorHex(PropTintColor, ref tintColor);
		StringParsers.TryParseBool(Properties.Values[PropCanPickup], out CanPickup);
		if (CanPickup && Properties.Params1.ContainsKey(PropCanPickup))
		{
			PickedUpItemValue = Properties.Params1[PropCanPickup];
		}
		if (Properties.Values.ContainsKey(PropFuelValue))
		{
			int.TryParse(Properties.Values[PropFuelValue], out FuelValue);
		}
		if (Properties.Values.ContainsKey(PropWeight))
		{
			int.TryParse(Properties.Values[PropWeight], out var result);
			Weight = new DataItem<int>(result);
		}
		CanMobsSpawnOn = false;
		Properties.ParseBool(PropCanMobsSpawnOn, ref CanMobsSpawnOn);
		CanPlayersSpawnOn = true;
		Properties.ParseBool(PropCanPlayersSpawnOn, ref CanPlayersSpawnOn);
		if (Properties.Values.ContainsKey(PropPickupTarget))
		{
			PickupTarget = Properties.Values[PropPickupTarget];
		}
		if (Properties.Values.ContainsKey(PropPickupSource))
		{
			PickupSource = Properties.Values[PropPickupSource];
		}
		if (Properties.Values.ContainsKey(PropPlaceAltBlockValue))
		{
			placeAltBlockNames = Properties.Values[PropPlaceAltBlockValue].Split(',');
		}
		if (Properties.Values.ContainsKey(PropPlaceShapeCategories))
		{
			string[] array = Properties.Values[PropPlaceShapeCategories].Split(',');
			ShapeCategories = new List<ShapesFromXml.ShapeCategory>();
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (ShapesFromXml.shapeCategories.TryGetValue(text, out var value))
				{
					ShapeCategories.Add(value);
				}
				else
				{
					Log.Error("Block " + blockName + " has unknown ShapeCategory " + text);
				}
			}
		}
		if (Properties.Values.ContainsKey(PropIndexName))
		{
			IndexName = Properties.Values[PropIndexName];
		}
		Properties.ParseBool(PropCanBlocksReplace, ref CanBlocksReplace);
		Properties.ParseBool(PropCanDecorateOnSlopes, ref CanDecorateOnSlopes);
		SlopeMaxCos = 90f;
		Properties.ParseFloat(PropSlopeMax, ref SlopeMaxCos);
		SlopeMaxCos = Mathf.Cos(SlopeMaxCos * (MathF.PI / 180f));
		if (Properties.Values.ContainsKey(PropIsTerrainDecoration))
		{
			IsTerrainDecoration = StringParsers.ParseBool(Properties.Values[PropIsTerrainDecoration]);
		}
		if (Properties.Values.ContainsKey(PropIsDecoration))
		{
			IsDecoration = StringParsers.ParseBool(Properties.Values[PropIsDecoration]);
		}
		if (Properties.Values.ContainsKey(PropDistantDecoration))
		{
			IsDistantDecoration = StringParsers.ParseBool(Properties.Values[PropDistantDecoration]);
		}
		if (Properties.Values.ContainsKey(PropBigDecorationRadius))
		{
			BigDecorationRadius = int.Parse(Properties.Values[PropBigDecorationRadius]);
		}
		if (Properties.Values.ContainsKey(PropSmallDecorationRadius))
		{
			SmallDecorationRadius = int.Parse(Properties.Values[PropSmallDecorationRadius]);
		}
		Properties.ParseFloat(PropGndAlign, ref GroundAlignDistance);
		Properties.ParseBool(PropIgnoreKeystoneOverlay, ref IgnoreKeystoneOverlay);
		LPHardnessScale = 1f;
		if (Properties.Values.ContainsKey(PropLPScale))
		{
			LPHardnessScale = StringParsers.ParseFloat(Properties.Values[PropLPScale]);
		}
		if (Properties.Values.ContainsKey(PropMapColor))
		{
			MapColor = StringParsers.ParseColor32(Properties.Values[PropMapColor]);
			bMapColorSet = true;
		}
		if (Properties.Values.ContainsKey(PropMapColor2))
		{
			MapColor2 = StringParsers.ParseColor32(Properties.Values[PropMapColor2]);
			bMapColor2Set = true;
		}
		if (Properties.Values.ContainsKey(PropMapElevMinMax))
		{
			MapElevMinMax = StringParsers.ParseVector2i(Properties.Values[PropMapElevMinMax]);
		}
		else
		{
			MapElevMinMax = Vector2i.zero;
		}
		if (Properties.Values.ContainsKey(PropMapSpecular))
		{
			MapSpecular = StringParsers.ParseFloat(Properties.Values[PropMapSpecular]);
		}
		if (Properties.Values.ContainsKey(PropGroupName) && !groupNameStringToGroupNames.TryGetValue(Properties.Values[PropGroupName], out GroupNames))
		{
			string[] array3 = Properties.Values[PropGroupName].Split(',');
			if (array3.Length != 0)
			{
				GroupNames = new string[array3.Length];
				for (int j = 0; j < array3.Length; j++)
				{
					GroupNames[j] = array3[j].Trim();
				}
			}
			groupNameStringToGroupNames.Add(Properties.Values[PropGroupName], GroupNames);
		}
		if (Properties.Values.ContainsKey(PropCustomIcon))
		{
			CustomIcon = Properties.Values[PropCustomIcon];
		}
		if (Properties.Values.ContainsKey(PropCustomIconTint))
		{
			CustomIconTint = StringParsers.ParseHexColor(Properties.Values[PropCustomIconTint]);
		}
		else
		{
			CustomIconTint = Color.white;
		}
		if (Properties.Values.ContainsKey(PropPlacementWireframe))
		{
			bHasPlacementWireframe = StringParsers.ParseBool(Properties.Values[PropPlacementWireframe]);
		}
		else
		{
			bHasPlacementWireframe = true;
		}
		isOversized = Properties.Values.ContainsKey(PropOversizedBounds);
		if (isOversized)
		{
			oversizedBounds = StringParsers.ParseBounds(Properties.Values[PropOversizedBounds]);
		}
		else
		{
			oversizedBounds = default(Bounds);
		}
		if (Properties.Values.ContainsKey(PropMultiBlockDim))
		{
			isMultiBlock = true;
			Vector3i dim = StringParsers.ParseVector3i(Properties.Values[PropMultiBlockDim]);
			List<Vector3i> list = new List<Vector3i>();
			if (Properties.Values.ContainsKey(PropMultiBlockLayer0))
			{
				int num = 0;
				while (Properties.Values.ContainsKey(PropMultiBlockLayer + num))
				{
					string[] array4 = Properties.Values[PropMultiBlockLayer + num].Split(',');
					for (int k = 0; k < array4.Length; k++)
					{
						array4[k] = array4[k].Trim();
						if (array4[k].Length > dim.x)
						{
							throw new Exception("Multi block layer entry " + k + " too long for block " + blockName);
						}
						for (int l = 0; l < array4[k].Length; l++)
						{
							if (array4[k][l] != ' ')
							{
								list.Add(new Vector3i(l, num, k));
							}
						}
					}
					num++;
				}
			}
			else
			{
				int num2 = dim.x / 2;
				int num3 = Mathf.RoundToInt((float)dim.x / 2f + 0.1f) - 1;
				int num4 = dim.z / 2;
				int num5 = Mathf.RoundToInt((float)dim.z / 2f + 0.1f) - 1;
				for (int m = -num2; m <= num3; m++)
				{
					for (int n = 0; n < dim.y; n++)
					{
						for (int num6 = -num4; num6 <= num5; num6++)
						{
							list.Add(new Vector3i(m, n, num6));
						}
					}
				}
			}
			multiBlockPos = new MultiBlockArray(dim, list);
		}
		if (Properties.Values.ContainsKey(PropTerrainAlignment))
		{
			terrainAlignmentMode = EnumUtils.Parse<TerrainAlignmentMode>(Properties.Values[PropTerrainAlignment]);
			if (terrainAlignmentMode != TerrainAlignmentMode.None)
			{
				bool flag = shape is BlockShapeModelEntity;
				bool flag2 = isOversized || isMultiBlock;
				if (!flag || !flag2)
				{
					Debug.LogWarning($"Failed to apply TerrainAlignmentMode \"{terrainAlignmentMode}\" to {blockName}. " + "Terrain alignment is only supported for oversized- and multi-blocks with shape type \"ModelEntity\".\n" + $"isModelEntity: {flag}. isOversized: {isOversized}. isMultiBlock: {isMultiBlock}. ");
					terrainAlignmentMode = TerrainAlignmentMode.None;
				}
			}
		}
		else
		{
			terrainAlignmentMode = TerrainAlignmentMode.None;
		}
		Properties.ParseFloat(PropHeatMapStrength, ref HeatMapStrength);
		FallDamage = 1f;
		if (Properties.Values.ContainsKey(PropFallDamage))
		{
			FallDamage = StringParsers.ParseFloat(Properties.Values[PropFallDamage]);
		}
		if (Properties.Values.ContainsKey(PropCount))
		{
			Count = int.Parse(Properties.Values[PropCount]);
		}
		if (Properties.Values.ContainsKey("ImposterExclude"))
		{
			bImposterExclude = StringParsers.ParseBool(Properties.Values["ImposterExclude"]);
		}
		if (Properties.Values.ContainsKey("ImposterExcludeAndStop"))
		{
			bImposterExcludeAndStop = StringParsers.ParseBool(Properties.Values["ImposterExcludeAndStop"]);
		}
		if (Properties.Values.ContainsKey("ImposterDontBlock"))
		{
			bImposterDontBlock = StringParsers.ParseBool(Properties.Values["ImposterDontBlock"]);
		}
		AllowedRotations = EBlockRotationClasses.No45;
		if (shape is BlockShapeModelEntity)
		{
			AllowedRotations |= EBlockRotationClasses.Basic45;
		}
		if (Properties.Values.ContainsKey(PropAllowAllRotations) && StringParsers.ParseBool(Properties.Values[PropAllowAllRotations]))
		{
			AllowedRotations |= EBlockRotationClasses.Basic45;
		}
		if (Properties.Values.ContainsKey("OnlySimpleRotations") && StringParsers.ParseBool(Properties.Values["OnlySimpleRotations"]))
		{
			AllowedRotations &= ~EBlockRotationClasses.Advanced;
		}
		if (Properties.Values.ContainsKey("AllowedRotations"))
		{
			AllowedRotations = EBlockRotationClasses.None;
			string[] array2 = Properties.Values["AllowedRotations"].Split(',');
			foreach (string text2 in array2)
			{
				if (EnumUtils.TryParse<EBlockRotationClasses>(text2, out var _result, _ignoreCase: true))
				{
					AllowedRotations |= _result;
					continue;
				}
				Log.Error("Rotation class '" + text2 + "' not found for block '" + blockName + "'");
			}
		}
		if (Properties.Values.ContainsKey("PlaceAsRandomRotation"))
		{
			PlaceRandomRotation = StringParsers.ParseBool(Properties.Values["PlaceAsRandomRotation"]);
		}
		if (Properties.Values.ContainsKey(PropIsPlant))
		{
			bIsPlant = StringParsers.ParseBool(Properties.Values[PropIsPlant]);
		}
		Properties.ParseString("CustomPlaceSound", ref CustomPlaceSound);
		Properties.ParseString("UpgradeSound", ref UpgradeSound);
		Properties.ParseString("DowngradeFX", ref DowngradeFX);
		Properties.ParseString("DestroyFX", ref DestroyFX);
		if (Properties.Values.ContainsKey(PropBuffsWhenWalkedOn))
		{
			BuffsWhenWalkedOn = Properties.Values[PropBuffsWhenWalkedOn].Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			if (BuffsWhenWalkedOn.Length < 1)
			{
				BuffsWhenWalkedOn = null;
			}
		}
		Properties.ParseBool(PropIsReplaceRandom, ref IsReplaceRandom);
		if (Properties.Values.ContainsKey(PropCraftExpValue))
		{
			StringParsers.TryParseFloat(Properties.Values[PropCraftExpValue], out CraftComponentExp);
		}
		if (Properties.Values.ContainsKey(PropCraftTimeValue))
		{
			StringParsers.TryParseFloat(Properties.Values[PropCraftTimeValue], out CraftComponentTime);
		}
		if (Properties.Values.ContainsKey(PropLootExpValue))
		{
			StringParsers.TryParseFloat(Properties.Values[PropLootExpValue], out LootExp);
		}
		if (Properties.Values.ContainsKey(PropDestroyExpValue))
		{
			StringParsers.TryParseFloat(Properties.Values[PropDestroyExpValue], out DestroyExp);
		}
		Properties.ParseString(PropParticleOnDeath, ref deathParticleName);
		if (Properties.Values.ContainsKey(PropPlaceExpValue))
		{
			StringParsers.TryParseFloat(Properties.Values[PropPlaceExpValue], out PlaceExp);
		}
		if (Properties.Values.ContainsKey(PropUpgradeExpValue))
		{
			StringParsers.TryParseFloat(Properties.Values[PropUpgradeExpValue], out UpgradeExp);
		}
		Properties.ParseFloat(PropEconomicValue, ref EconomicValue);
		Properties.ParseFloat(PropEconomicSellScale, ref EconomicSellScale);
		Properties.ParseInt(PropEconomicBundleSize, ref EconomicBundleSize);
		if (Properties.Values.ContainsKey(PropSellableToTrader))
		{
			StringParsers.TryParseBool(Properties.Values[PropSellableToTrader], out SellableToTrader);
		}
		Properties.ParseString(PropTraderStageTemplate, ref TraderStageTemplate);
		if (Properties.Values.ContainsKey(PropCreativeMode))
		{
			CreativeMode = EnumUtils.Parse<EnumCreativeMode>(Properties.Values[PropCreativeMode]);
		}
		if (Properties.Values.ContainsKey(PropFilterTag))
		{
			FilterTags = Properties.Values[PropFilterTag].Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			if (FilterTags.Length < 1)
			{
				FilterTags = null;
			}
		}
		SortOrder = Properties.GetString(PropCreativeSort1);
		SortOrder += Properties.GetString(PropCreativeSort2);
		if (Properties.Values.ContainsKey(PropDisplayType))
		{
			DisplayType = Properties.Values[PropDisplayType];
		}
		if (Properties.Values.ContainsKey(PropItemTypeIcon))
		{
			ItemTypeIcon = Properties.Values[PropItemTypeIcon];
		}
		if (Properties.Values.ContainsKey(PropAutoShape))
		{
			AutoShapeType = EnumUtils.Parse<EAutoShapeType>(Properties.Values[PropAutoShape]);
			if (AutoShapeType != EAutoShapeType.None)
			{
				string[] array5 = blockName.Split(':');
				autoShapeBaseName = array5[0];
				autoShapeShapeName = array5[1];
				autoShapeMaterials.Add(autoShapeBaseName);
			}
		}
		MaxDamage = blockMaterial.MaxDamage;
		Properties.ParseInt(PropMaxDamage, ref MaxDamage);
		Properties.ParseInt(PropStartDamage, ref StartDamage);
		Properties.ParseInt(PropStage2Health, ref Stage2Health);
		Properties.ParseFloat(PropDamage, ref Damage);
		if (Properties.Values.ContainsKey(PropActivationDistance))
		{
			int.TryParse(Properties.Values[PropActivationDistance], out activationDistance);
		}
		if (Properties.Values.ContainsKey(PropPlacementDistance))
		{
			int.TryParse(Properties.Values[PropPlacementDistance], out placementDistance);
		}
		if (Properties.Values.ContainsKey("PassThroughDamage"))
		{
			EnablePassThroughDamage = StringParsers.ParseBool(Properties.Values["PassThroughDamage"]);
		}
		if (Properties.Values.ContainsKey("CopyPaintOnDowngrade"))
		{
			string[] array6 = Properties.Values["CopyPaintOnDowngrade"].Split(',');
			HashSet<BlockFace> hashSet = new HashSet<BlockFace>();
			for (int num7 = 0; num7 < array6.Length; num7++)
			{
				switch (array6[num7][0])
				{
				case 'N':
					hashSet.Add(BlockFace.North);
					break;
				case 'S':
					hashSet.Add(BlockFace.South);
					break;
				case 'E':
					hashSet.Add(BlockFace.East);
					break;
				case 'W':
					hashSet.Add(BlockFace.West);
					break;
				case 'T':
					hashSet.Add(BlockFace.Top);
					break;
				case 'B':
					hashSet.Add(BlockFace.Bottom);
					break;
				}
			}
			RemovePaintOnDowngrade = new List<BlockFace>();
			for (int num8 = 0; num8 < 6; num8++)
			{
				if (!hashSet.Contains((BlockFace)num8))
				{
					RemovePaintOnDowngrade.Add((BlockFace)num8);
				}
			}
		}
		for (int num9 = 0; num9 < 1; num9++)
		{
			string useGlobalUV = ShapesFromXml.TextureLabelsByChannel[num9].UseGlobalUV;
			string text3 = Properties.GetString(useGlobalUV);
			if (text3.Length <= 0)
			{
				continue;
			}
			UVModesPerSide[num9] = 0u;
			if (!text3.Contains(","))
			{
				UVMode uVMode = text3[0] switch
				{
					'L' => UVMode.Local, 
					'G' => UVMode.Global, 
					_ => UVMode.Default, 
				};
				for (int num10 = 0; num10 < cUVModeSides; num10++)
				{
					UVModesPerSide[num9] |= (uint)uVMode << num10 * cUVModeBits;
				}
				continue;
			}
			int num11 = 0;
			foreach (char c in text3)
			{
				if (c != ',')
				{
					UVModesPerSide[num9] |= (uint)(c switch
					{
						'L' => 2, 
						'G' => 1, 
						_ => 0, 
					} << num11);
					num11 += cUVModeBits;
				}
			}
		}
		RadiusEffects = null;
		string text4 = Properties.GetString(PropRadiusEffect);
		if (text4.Length > 0)
		{
			List<BlockRadiusEffect> list2 = new List<BlockRadiusEffect>();
			string[] array2 = text4.Split(new string[1] { ";" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text5 in array2)
			{
				BlockRadiusEffect item = new BlockRadiusEffect
				{
					radiusSq = 1f,
					variable = text5
				};
				int num13 = text5.IndexOf(',');
				if (num13 >= 0)
				{
					float num14 = StringParsers.ParseFloat(text5, num13 + 1);
					item.radiusSq = num14 * num14;
					item.variable = text5.Substring(0, num13);
				}
				list2.Add(item);
			}
			RadiusEffects = list2.ToArray();
		}
		if (Properties.Values.ContainsKey(PropDescriptionKey))
		{
			DescriptionKey = Properties.Values[PropDescriptionKey];
		}
		else
		{
			DescriptionKey = $"{blockName}Desc";
			if (!Localization.Exists(DescriptionKey))
			{
				DescriptionKey = defaultBlockDescriptionKey;
			}
		}
		if (Properties.Values.ContainsKey(PropCraftingSkillGroup))
		{
			CraftingSkillGroup = Properties.Values[PropCraftingSkillGroup];
		}
		else
		{
			CraftingSkillGroup = "";
		}
		if (Properties.Values.ContainsKey(PropHarvestOverdamage))
		{
			HarvestOverdamage = StringParsers.ParseBool(Properties.Values[PropHarvestOverdamage]);
		}
		bShowModelOnFall = !Properties.Values.ContainsKey(PropShowModelOnFall) || StringParsers.ParseBool(Properties.Values[PropShowModelOnFall]);
		if (Properties.Values.ContainsKey("HandleFace"))
		{
			HandleFace = EnumUtils.Parse<BlockFace>(Properties.Values["HandleFace"]);
		}
		if (Properties.Values.ContainsKey("DisplayInfo"))
		{
			DisplayInfo = EnumUtils.Parse<EnumDisplayInfo>(Properties.Values["DisplayInfo"]);
		}
		if (Properties.Values.ContainsKey("SelectAlternates"))
		{
			SelectAlternates = StringParsers.ParseBool(Properties.Values["SelectAlternates"]);
		}
		if (Properties.Values.ContainsKey(PropNoScrapping))
		{
			NoScrapping = StringParsers.ParseBool(Properties.Values[PropNoScrapping]);
		}
		VehicleHitScale = 1f;
		Properties.ParseFloat(PropVehicleHitScale, ref VehicleHitScale);
		if (Properties.Values.ContainsKey("UiBackgroundTexture") && !StringParsers.TryParseSInt32(Properties.Values["UiBackgroundTexture"], out uiBackgroundTextureId))
		{
			uiBackgroundTextureId = -1;
		}
		Properties.ParseString(PropBlockAddedEvent, ref blockAddedEvent);
		Properties.ParseString(PropBlockDestroyedEvent, ref blockDestroyedEvent);
		Properties.ParseString(PropBlockDowngradeEvent, ref blockDowngradeEvent);
		Properties.ParseString(PropBlockDowngradedToEvent, ref blockDowngradedToEvent);
		Properties.ParseBool(PropIsTemporaryBlock, ref IsTemporaryBlock);
		Properties.ParseBool(PropRefundOnUnload, ref RefundOnUnload);
		Properties.ParseString(PropSoundPickup, ref SoundPickup);
		Properties.ParseString(PropSoundPlace, ref SoundPlace);
	}

	public virtual void LateInit()
	{
		shape.LateInit();
		if (AutoShapeType == EAutoShapeType.Shape)
		{
			autoShapeHelper = GetBlockByName(autoShapeBaseName + ":" + ShapesFromXml.VariantHelperName);
		}
		if (Properties.Values.ContainsKey(PropSiblingBlock))
		{
			SiblingBlock = ItemClass.GetItem(Properties.Values[PropSiblingBlock]).ToBlockValue();
			if (SiblingBlock.Equals(BlockValue.Air))
			{
				throw new Exception("Block with name '" + Properties.Values[PropSiblingBlock] + "' not found in block " + blockName);
			}
		}
		else
		{
			SiblingBlock = BlockValue.Air;
		}
		if (Properties.Values.ContainsKey("MirrorSibling"))
		{
			string text = Properties.Values["MirrorSibling"];
			MirrorSibling = ItemClass.GetItem(text).ToBlockValue().type;
			if (MirrorSibling == 0)
			{
				throw new Exception("Block with name '" + text + "' not found in block " + blockName);
			}
		}
		else
		{
			MirrorSibling = 0;
		}
		if (Properties.Values.ContainsKey(PropUpgradeBlockClassToBlock))
		{
			UpgradeBlock = GetBlockValue(Properties.Values[PropUpgradeBlockClassToBlock]);
			if (UpgradeBlock.isair)
			{
				throw new Exception("Block with name '" + Properties.Values[PropUpgradeBlockClassToBlock] + "' not found in block " + blockName);
			}
		}
		else
		{
			UpgradeBlock = BlockValue.Air;
		}
		if (Properties.Values.ContainsKey(PropDowngradeBlock))
		{
			DowngradeBlock = GetBlockValue(Properties.Values[PropDowngradeBlock]);
			if (DowngradeBlock.isair)
			{
				throw new Exception("Block with name '" + Properties.Values[PropDowngradeBlock] + "' not found in block " + blockName);
			}
		}
		else
		{
			DowngradeBlock = BlockValue.Air;
		}
		if (Properties.Values.ContainsKey(PropLockpickDowngradeBlock))
		{
			LockpickDowngradeBlock = GetBlockValue(Properties.Values[PropLockpickDowngradeBlock]);
			if (LockpickDowngradeBlock.isair)
			{
				throw new Exception("Block with name '" + Properties.Values[PropLockpickDowngradeBlock] + "' not found in block " + blockName);
			}
		}
		else
		{
			LockpickDowngradeBlock = DowngradeBlock;
		}
		if (Properties.Values.ContainsKey("ImposterExchange"))
		{
			ImposterExchange = GetBlockValue(Properties.Values["ImposterExchange"]).type;
			if (Properties.Params1.ContainsKey("ImposterExchange"))
			{
				ImposterExchangeTexIdx = (byte)int.Parse(Properties.Params1["ImposterExchange"]);
			}
		}
		if (Properties.Values.ContainsKey("MergeInto"))
		{
			MergeIntoId = GetBlockValue(Properties.Values["MergeInto"]).type;
			if (MergeIntoId == 0)
			{
				Log.Warning("Warning: MergeInto block with name '{0}' not found!", Properties.Values["MergeInto"]);
			}
			if (Properties.Params1.ContainsKey("MergeInto"))
			{
				string[] array = Properties.Params1["MergeInto"].Split(',');
				if (array.Length == 6)
				{
					MergeIntoTexIds = new int[6];
					for (int i = 0; i < MergeIntoTexIds.Length; i++)
					{
						MergeIntoTexIds[i] = int.Parse(array[i].Trim());
					}
				}
			}
		}
		if (PlatformOptimizations.FileBackedBlockProperties)
		{
			PropertiesCache.Store(blockID, dynamicProperties);
			dynamicProperties = null;
		}
	}

	public static void InitStatic()
	{
		nameToBlock = new Dictionary<string, Block>();
		nameToBlockCaseInsensitive = new CaseInsensitiveStringDictionary<Block>();
		list = new Block[MAX_BLOCKS];
		autoShapeMaterials = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		groupNameStringToGroupNames = new Dictionary<string, string[]>();
		if (PlatformOptimizations.FileBackedBlockProperties)
		{
			PropertiesCache = new DynamicPropertiesCache();
		}
	}

	public static void LateInitAll()
	{
		for (int i = 0; i < MAX_BLOCKS; i++)
		{
			if (list[i] != null)
			{
				list[i].LateInit();
			}
		}
		if (PlatformOptimizations.FileBackedBlockProperties)
		{
			GC.Collect();
		}
		int type = BlockValue.Air.type;
		for (int j = 0; j < MAX_BLOCKS; j++)
		{
			Block block = list[j];
			if (block == null)
			{
				continue;
			}
			int num = block.MaxDamage;
			int num2 = 0;
			int type2 = block.DowngradeBlock.type;
			while (type2 != type)
			{
				Block block2 = list[type2];
				num += block2.MaxDamage;
				type2 = block2.DowngradeBlock.type;
				if (++num2 > 10)
				{
					Log.Warning("Block '{0}' over downgrade limit", block.blockName);
					break;
				}
			}
			block.MaxDamagePlusDowngrades = num;
		}
	}

	public static void OnWorldUnloaded()
	{
		if (PlatformOptimizations.FileBackedBlockProperties)
		{
			PropertiesCache?.Cleanup();
			PropertiesCache = null;
		}
	}

	public virtual bool FilterIndexType(BlockValue bv)
	{
		return true;
	}

	public Vector2 GetPathOffset(int _rotation)
	{
		if (PathType != -1)
		{
			return Vector2.zero;
		}
		return shape.GetPathOffset(_rotation);
	}

	public static void Cleanup()
	{
		nameToBlock = null;
		nameToBlockCaseInsensitive = null;
		groupNameStringToGroupNames = null;
		list = null;
		autoShapeMaterials = null;
		fullMappingDataForClients = null;
	}

	public void CopyDroppedFrom(Block _other)
	{
		foreach (KeyValuePair<EnumDropEvent, List<SItemDropProb>> item in _other.itemsToDrop)
		{
			EnumDropEvent key = item.Key;
			List<SItemDropProb> value = item.Value;
			List<SItemDropProb> list = (itemsToDrop.ContainsKey(key) ? itemsToDrop[key] : null);
			if (list == null)
			{
				list = new List<SItemDropProb>();
				itemsToDrop[key] = list;
			}
			for (int i = 0; i < value.Count; i++)
			{
				bool flag = true;
				int num = 0;
				while (flag && num < list.Count)
				{
					if (list[num].name == value[i].name)
					{
						flag = false;
					}
					num++;
				}
				if (flag)
				{
					list.Add(value[i]);
				}
			}
		}
	}

	public virtual BlockFace getInventoryFace()
	{
		return BlockFace.North;
	}

	public virtual byte GetLightValue(BlockValue _blockValue)
	{
		return lightValue;
	}

	public virtual Block SetLightValue(float _lightValueInPercent)
	{
		lightValue = (byte)(15f * _lightValueInPercent);
		return this;
	}

	public virtual bool UseBuffsWhenWalkedOn(World world, Vector3i _blockPos, BlockValue _blockValue)
	{
		return true;
	}

	public virtual bool IsMovementBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFace _face)
	{
		if (isMultiBlock && _blockValue.ischild)
		{
			Vector3i parentPos = multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			if (block.ischild)
			{
				Log.Error("IsMovementBlocked {0} at {1} has child parent, {2} at {3} ", this, _blockPos, block.Block, parentPos);
				return true;
			}
			return IsMovementBlocked(_world, parentPos, block, _face);
		}
		if (!IsCollideMovement)
		{
			return false;
		}
		if (BlocksMovement == 0)
		{
			return shape.IsMovementBlocked(_blockValue, _face);
		}
		return BlocksMovement == 1;
	}

	public virtual bool IsSeeThrough(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (isMultiBlock && _blockValue.ischild)
		{
			Vector3i parentPos = multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			if (block.ischild)
			{
				Log.Error("IsSeeThrough {0} at {1} has child parent, {2} at {3} ", this, _blockPos, block.Block, parentPos);
				return true;
			}
			return IsSeeThrough(_world, _clrIdx, parentPos, block);
		}
		if (!IsCollideSight)
		{
			return !_world.IsWater(_blockPos);
		}
		return false;
	}

	public virtual bool IsMovementBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides)
	{
		if (isMultiBlock && _blockValue.ischild)
		{
			Vector3i parentPos = multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			if (block.ischild)
			{
				Log.Error("IsMovementBlocked {0} at {1} has child parent, {2} at {3} ", this, _blockPos, block.Block, parentPos);
				return true;
			}
			return IsMovementBlocked(_world, parentPos, block, _sides);
		}
		if (_sides == BlockFaceFlag.None)
		{
			return IsMovementBlocked(_world, _blockPos, _blockValue, BlockFace.None);
		}
		for (int i = 0; i <= 5; i++)
		{
			if (((uint)(1 << i) & (uint)_sides) != 0 && !IsMovementBlocked(_world, _blockPos, _blockValue, (BlockFace)i))
			{
				return false;
			}
		}
		return true;
	}

	public virtual bool IsWaterBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides)
	{
		return IsMovementBlocked(_world, _blockPos, _blockValue, _sides);
	}

	public bool IsMovementBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, Vector3 _entityPos)
	{
		if (isMultiBlock && _blockValue.ischild)
		{
			Vector3i parentPos = multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			if (block.ischild)
			{
				Log.Error("IsMovementBlocked {0} at {1} has child parent, {2} at {3} ", this, _blockPos, block.Block, parentPos);
				return true;
			}
			return IsMovementBlocked(_world, parentPos, block, _entityPos);
		}
		BlockFaceFlag blockFaceFlag = BlockFaceFlags.FrontSidesFromPosition(_blockPos, _entityPos);
		if (blockFaceFlag == BlockFaceFlag.None)
		{
			return IsMovementBlocked(_world, _blockPos, _blockValue, BlockFace.None);
		}
		for (int i = 2; i <= 5; i++)
		{
			if (((uint)(1 << i) & (uint)blockFaceFlag) != 0 && !IsMovementBlocked(_world, _blockPos, _blockValue, (BlockFace)i))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsMovementBlockedAny(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, Vector3 _entityPos)
	{
		if (isMultiBlock && _blockValue.ischild)
		{
			Vector3i parentPos = multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			if (block.ischild)
			{
				Log.Error("IsMovementBlockedAny {0} at {1} has child parent, {2} at {3} ", this, _blockPos, block.Block, parentPos);
				return true;
			}
			return IsMovementBlockedAny(_world, parentPos, block, _entityPos);
		}
		BlockFaceFlag blockFaceFlag = BlockFaceFlags.FrontSidesFromPosition(_blockPos, _entityPos);
		if (blockFaceFlag == BlockFaceFlag.None)
		{
			return IsMovementBlocked(_world, _blockPos, _blockValue, BlockFace.None);
		}
		for (int i = 2; i <= 5; i++)
		{
			if (((uint)(1 << i) & (uint)blockFaceFlag) != 0 && IsMovementBlocked(_world, _blockPos, _blockValue, (BlockFace)i))
			{
				return true;
			}
		}
		return false;
	}

	public virtual float GetStepHeight(IBlockAccess world, Vector3i blockPos, BlockValue blockDef, BlockFace stepFace)
	{
		if (!IsCollideMovement)
		{
			return 0f;
		}
		return shape.GetStepHeight(blockDef, stepFace);
	}

	public float MinStepHeight(BlockValue blockDef, BlockFaceFlag stepSides)
	{
		float num = -1f;
		for (int i = 2; i <= 5; i++)
		{
			if (((uint)(1 << i) & (uint)stepSides) != 0)
			{
				num = ((!(num < 0f)) ? Math.Min(num, GetStepHeight(null, Vector3i.zero, blockDef, (BlockFace)i)) : GetStepHeight(null, Vector3i.zero, blockDef, (BlockFace)i));
			}
		}
		return Math.Max(num, 0f);
	}

	public float MaxStepHeight(BlockValue blockDef, BlockFaceFlag stepSides)
	{
		float num = -1f;
		for (int i = 2; i <= 5; i++)
		{
			if (((uint)(1 << i) & (uint)stepSides) != 0)
			{
				num = ((!(num < 0f)) ? Math.Max(num, GetStepHeight(null, Vector3i.zero, blockDef, (BlockFace)i)) : GetStepHeight(null, Vector3i.zero, blockDef, (BlockFace)i));
			}
		}
		return Math.Max(num, 0f);
	}

	public float MinStepHeight(Vector3i blockPos, BlockValue blockDef, Vector3 entityPos)
	{
		BlockFaceFlag stepSides = BlockFaceFlags.FrontSidesFromPosition(blockPos, entityPos);
		return MinStepHeight(blockDef, stepSides);
	}

	public float MaxStepHeight(Vector3i blockPos, BlockValue blockDef, Vector3 entityPos)
	{
		BlockFaceFlag stepSides = BlockFaceFlags.FrontSidesFromPosition(blockPos, entityPos);
		return MaxStepHeight(blockDef, stepSides);
	}

	public virtual float GetHardness()
	{
		return blockMaterial.Hardness.Value;
	}

	public virtual int GetWeight()
	{
		int result = 0;
		if (Weight != null)
		{
			result = Weight.Value;
		}
		return result;
	}

	public UVMode GetUVMode(int side, int channel)
	{
		return (UVMode)((UVModesPerSide[channel] >> side * cUVModeBits) & cUVModeMask);
	}

	public virtual Rect getUVRectFromSideAndMetadata(int _meshIndex, BlockFace _side, Vector3[] _vertices, BlockValue _blockValue)
	{
		return getUVRectFromSideAndMetadata(_meshIndex, _side, (_vertices != null) ? _vertices[0] : Vector3.zero, _blockValue);
	}

	public virtual Rect getUVRectFromSideAndMetadata(int _meshIndex, BlockFace _side, Vector3 _worldPos, BlockValue _blockValue)
	{
		int sideTextureId = GetSideTextureId(_blockValue, _side, 0);
		if (sideTextureId < 0)
		{
			return UVRectTiling.Empty.uv;
		}
		UVRectTiling[] uvMapping = MeshDescription.meshes[_meshIndex].textureAtlas.uvMapping;
		if (sideTextureId >= uvMapping.Length)
		{
			return UVRectTiling.Empty.uv;
		}
		UVRectTiling uVRectTiling = uvMapping[sideTextureId];
		if (uVRectTiling.blockW == 1 && uVRectTiling.blockH == 1)
		{
			return uVRectTiling.uv;
		}
		float x = _worldPos.x;
		float y = _worldPos.y;
		float z = _worldPos.z;
		return _side switch
		{
			BlockFace.North => new Rect(uVRectTiling.uv.x + uVRectTiling.uv.width * (float)(uVRectTiling.blockW - 1) - (float)Utils.FastRoundToIntAndMod(x, uVRectTiling.blockW) * uVRectTiling.uv.width, uVRectTiling.uv.y + (float)Utils.FastRoundToIntAndMod(y, uVRectTiling.blockH) * uVRectTiling.uv.height, uVRectTiling.uv.width, uVRectTiling.uv.height), 
			BlockFace.South => new Rect(uVRectTiling.uv.x + (float)Utils.FastRoundToIntAndMod(x, uVRectTiling.blockW) * uVRectTiling.uv.width, uVRectTiling.uv.y + (float)Utils.FastRoundToIntAndMod(y, uVRectTiling.blockH) * uVRectTiling.uv.height, uVRectTiling.uv.width, uVRectTiling.uv.height), 
			BlockFace.West => new Rect(uVRectTiling.uv.x + uVRectTiling.uv.width * (float)(uVRectTiling.blockW - 1) - (float)Utils.FastRoundToIntAndMod(z, uVRectTiling.blockW) * uVRectTiling.uv.width, uVRectTiling.uv.y + (float)Utils.FastRoundToIntAndMod(y, uVRectTiling.blockH) * uVRectTiling.uv.height, uVRectTiling.uv.width, uVRectTiling.uv.height), 
			BlockFace.East => new Rect(uVRectTiling.uv.x + (float)Utils.FastRoundToIntAndMod(z, uVRectTiling.blockW) * uVRectTiling.uv.width, uVRectTiling.uv.y + (float)Utils.FastRoundToIntAndMod(y, uVRectTiling.blockH) * uVRectTiling.uv.height, uVRectTiling.uv.width, uVRectTiling.uv.height), 
			BlockFace.Top => new Rect(uVRectTiling.uv.x + (float)Utils.FastRoundToIntAndMod(x, uVRectTiling.blockW) * uVRectTiling.uv.width, uVRectTiling.uv.y + (float)Utils.FastRoundToIntAndMod(z, uVRectTiling.blockH) * uVRectTiling.uv.height, uVRectTiling.uv.width, uVRectTiling.uv.height), 
			BlockFace.Bottom => new Rect(uVRectTiling.uv.x + uVRectTiling.uv.width * (float)(uVRectTiling.blockW - 1) - (float)Utils.FastRoundToIntAndMod(x, uVRectTiling.blockW) * uVRectTiling.uv.width, uVRectTiling.uv.y + (float)Utils.FastRoundToIntAndMod(z, uVRectTiling.blockH) * uVRectTiling.uv.height, uVRectTiling.uv.width, uVRectTiling.uv.height), 
			_ => new Rect(0f, 0f, 0f, 0f), 
		};
	}

	public virtual void GetCollidingAABB(BlockValue _blockValue, int _x, int _y, int _z, float _distortedAddY, Bounds _aabb, List<Bounds> _aabbList)
	{
		staticList_IntersectRayWithBlockList.Clear();
		GetCollisionAABB(_blockValue, _x, _y, _z, _distortedAddY, staticList_IntersectRayWithBlockList);
		for (int i = 0; i < staticList_IntersectRayWithBlockList.Count; i++)
		{
			Bounds bounds = staticList_IntersectRayWithBlockList[i];
			if (_aabb.Intersects(bounds))
			{
				_aabbList.Add(bounds);
			}
		}
	}

	public virtual bool HasCollidingAABB(BlockValue _blockValue, int _x, int _y, int _z, float _distortedAddY, Bounds _aabb)
	{
		staticList_IntersectRayWithBlockList.Clear();
		GetCollisionAABB(_blockValue, _x, _y, _z, _distortedAddY, staticList_IntersectRayWithBlockList);
		for (int i = 0; i < staticList_IntersectRayWithBlockList.Count; i++)
		{
			Bounds bounds = staticList_IntersectRayWithBlockList[i];
			if (_aabb.Intersects(bounds))
			{
				return true;
			}
		}
		return false;
	}

	public virtual void GetCollisionAABB(BlockValue _blockValue, int _x, int _y, int _z, float _distortedAddY, List<Bounds> _result)
	{
		Vector3 vector = new Vector3(0f, _distortedAddY, 0f);
		Bounds[] bounds = shape.GetBounds(_blockValue);
		for (int i = 0; i < bounds.Length; i++)
		{
			Bounds item = bounds[i];
			item.center += new Vector3(_x, _y, _z);
			item.max += vector;
			_result.Add(item);
		}
	}

	public virtual IList<Bounds> GetClipBoundsList(BlockValue _blockValue, Vector3 _blockPos)
	{
		return shape.GetBounds(_blockValue);
	}

	public virtual bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		return false;
	}

	public virtual void DoExchangeAction(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, string _action, int _itemCount)
	{
	}

	public virtual void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (!_blockValue.ischild)
		{
			shape.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		}
	}

	public virtual void OnBlockUnloaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (!_blockValue.ischild)
		{
			shape.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
		}
		if (RefundOnUnload)
		{
			GameEventManager.Current.RefundSpawnedBlock(_blockPos);
		}
	}

	public virtual void OnNeighborBlockChange(WorldBase world, int _clrIdx, Vector3i _myBlockPos, BlockValue _myBlockValue, Vector3i _blockPosThatChanged, BlockValue _newNeighborBlockValue, BlockValue _oldNeighborBlockValue)
	{
	}

	public static bool CanFallBelow(WorldBase _world, int _x, int _y, int _z)
	{
		BlockValue block = _world.GetBlock(_x, _y - 1, _z);
		Block block2 = block.Block;
		if (block.isair || !block2.StabilitySupport)
		{
			return true;
		}
		return false;
	}

	public virtual ulong GetTickRate()
	{
		return 10uL;
	}

	public virtual void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		if (_blockValue.ischild)
		{
			return;
		}
		shape.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
		if (isMultiBlock && !MultiBlockManager.Instance.TryGetPOIMultiBlock(_blockPos, out var _))
		{
			multiBlockPos.AddChilds(_world, _chunk, _blockPos, _blockValue);
		}
		if (IsTemporaryBlock)
		{
			if (!_chunk.ChunkCustomData.dict.TryGetValue("temporaryblocks", out var value))
			{
				value = new ChunkBlockClearData("temporaryblocks", 0uL, _isSavedToNetwork: false, _world as World);
				_chunk.ChunkCustomData.Add("temporaryblocks", value);
			}
			(value as ChunkBlockClearData).BlockList.Add(World.toBlock(_blockPos));
		}
		if (!string.IsNullOrEmpty(blockAddedEvent))
		{
			GameEventManager.Current.HandleAction(blockAddedEvent, null, null, twitchActivated: false, _blockPos);
		}
	}

	public virtual void OnBlockReset(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
	}

	public virtual void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (!_blockValue.ischild)
		{
			shape.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
			if (isMultiBlock)
			{
				multiBlockPos.RemoveChilds(_world, _blockPos, _blockValue);
			}
			if (IsTemporaryBlock && _chunk.ChunkCustomData.dict.TryGetValue("temporaryblocks", out var value))
			{
				(value as ChunkBlockClearData).BlockList.Remove(World.toBlock(_blockPos));
			}
		}
		else if (isMultiBlock)
		{
			multiBlockPos.RemoveParentBlock(_world, _blockPos, _blockValue);
		}
	}

	public virtual void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		if (!_oldBlockValue.ischild)
		{
			shape.OnBlockValueChanged(_world, _blockPos, _clrIdx, _oldBlockValue, _newBlockValue);
			if (isMultiBlock && _oldBlockValue.rotation != _newBlockValue.rotation)
			{
				multiBlockPos.RemoveChilds(_world, _blockPos, _oldBlockValue);
				multiBlockPos.AddChilds(_world, _chunk, _blockPos, _newBlockValue);
			}
		}
	}

	public virtual void OnBlockEntityTransformBeforeActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		shape.OnBlockEntityTransformBeforeActivated(_world, _blockPos, _blockValue, _ebcd);
	}

	public virtual void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		shape.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
		_ebcd.UpdateTemperature();
		ForceAnimationState(_blockValue, _ebcd);
		if (GroundAlignDistance != 0f)
		{
			((World)_world).m_ChunkManager.AddGroundAlignBlock(_ebcd);
		}
		if (_world.TryRetrieveAndRemovePendingDowngradeBlock(_blockPos) && !string.IsNullOrEmpty(blockDowngradedToEvent))
		{
			GameEventManager.Current.HandleAction(blockDowngradedToEvent, null, null, twitchActivated: false, _blockPos);
		}
		if (terrainAlignmentMode != TerrainAlignmentMode.None)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				MultiBlockManager.Instance.TryRegisterTerrainAlignedBlock(_blockPos, _blockValue);
			}
			MultiBlockManager.Instance.SetTerrainAlignmentDirty(_blockPos);
		}
	}

	public virtual void ForceAnimationState(BlockValue _blockValue, BlockEntityData _ebcd)
	{
	}

	public virtual int DamageBlock(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _damagePoints, int _entityIdThatDamaged, ItemActionAttack.AttackHitInfo _attackHitInfo = null, bool _bUseHarvestTool = false, bool _bBypassMaxDamage = false)
	{
		return OnBlockDamaged(_world, _clrIdx, _blockPos, _blockValue, _damagePoints, _entityIdThatDamaged, _attackHitInfo, _bUseHarvestTool, _bBypassMaxDamage);
	}

	public virtual int OnBlockDamaged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _damagePoints, int _entityIdThatDamaged, ItemActionAttack.AttackHitInfo _attackHitInfo, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		ChunkCluster chunkCache = _world.ChunkCache;
		if (chunkCache == null)
		{
			return 0;
		}
		if (isMultiBlock && _blockValue.ischild)
		{
			Vector3i parentPos = multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = chunkCache.GetBlock(parentPos);
			if (block.ischild)
			{
				Log.Error("Block on position {0} with name '{1}' should be a parent but is not! (6)", parentPos, block.Block.blockName);
				return 0;
			}
			return block.Block.OnBlockDamaged(_world, _clrIdx, parentPos, block, _damagePoints, _entityIdThatDamaged, _attackHitInfo, _bUseHarvestTool, _bBypassMaxDamage, _recDepth + 1);
		}
		Block block2 = _blockValue.Block;
		int damage = _blockValue.damage;
		bool flag = damage >= block2.MaxDamage;
		int num = damage + _damagePoints;
		chunkCache.InvokeOnBlockDamagedDelegates(_blockPos, _blockValue, _damagePoints, _entityIdThatDamaged);
		if (num < 0)
		{
			if (!UpgradeBlock.isair)
			{
				BlockValue upgradeBlock = UpgradeBlock;
				upgradeBlock = BlockPlaceholderMap.Instance.Replace(upgradeBlock, _world.GetGameRandom(), _blockPos.x, _blockPos.z);
				upgradeBlock.rotation = convertRotation(_blockValue, upgradeBlock);
				upgradeBlock.meta = _blockValue.meta;
				upgradeBlock.damage = 0;
				Block block3 = upgradeBlock.Block;
				if (!block3.shape.IsTerrain())
				{
					_world.SetBlockRPC(_clrIdx, _blockPos, upgradeBlock);
					if (chunkCache.GetTextureFull(_blockPos) != 0L)
					{
						GameManager.Instance.SetBlockTextureServer(_blockPos, BlockFace.None, 0, _entityIdThatDamaged);
					}
				}
				else
				{
					_world.SetBlockRPC(_clrIdx, _blockPos, upgradeBlock, block3.Density);
				}
				DynamicMeshManager.ChunkChanged(_blockPos, _entityIdThatDamaged, _blockValue.type);
				return upgradeBlock.damage;
			}
			if (_blockValue.damage != 0)
			{
				_blockValue.damage = 0;
				_world.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
			}
			return 0;
		}
		if (Stage2Health > 0)
		{
			int num2 = block2.MaxDamage - Stage2Health;
			if (damage < num2 && num >= num2)
			{
				num = num2;
			}
		}
		if (!flag && num >= block2.MaxDamage)
		{
			int num3 = num - block2.MaxDamage;
			DynamicMeshManager.ChunkChanged(_blockPos, _entityIdThatDamaged, _blockValue.type);
			DestroyedResult destroyedResult = OnBlockDestroyedBy(_world, _clrIdx, _blockPos, _blockValue, _entityIdThatDamaged, _bUseHarvestTool);
			if (destroyedResult != DestroyedResult.Keep)
			{
				if (!DowngradeBlock.isair && destroyedResult == DestroyedResult.Downgrade)
				{
					if (_recDepth == 0)
					{
						SpawnDowngradeFX(_world, _blockValue, _blockPos, block2.tintColor, _entityIdThatDamaged);
					}
					BlockValue downgradeBlock = DowngradeBlock;
					downgradeBlock = BlockPlaceholderMap.Instance.Replace(downgradeBlock, _world.GetGameRandom(), _blockPos.x, _blockPos.z);
					downgradeBlock.rotation = _blockValue.rotation;
					downgradeBlock.meta = _blockValue.meta;
					Block block4 = downgradeBlock.Block;
					if (!block4.shape.IsTerrain())
					{
						_world.SetBlockRPC(_clrIdx, _blockPos, downgradeBlock);
						if (chunkCache.GetTextureFull(_blockPos) != 0L)
						{
							if (RemovePaintOnDowngrade == null)
							{
								GameManager.Instance.SetBlockTextureServer(_blockPos, BlockFace.None, 0, _entityIdThatDamaged);
							}
							else
							{
								for (int i = 0; i < RemovePaintOnDowngrade.Count; i++)
								{
									GameManager.Instance.SetBlockTextureServer(_blockPos, RemovePaintOnDowngrade[i], 0, _entityIdThatDamaged);
								}
							}
						}
						_world.AddPendingDowngradeBlock(_blockPos);
						if (!string.IsNullOrEmpty(blockDowngradeEvent))
						{
							Entity entity = _world.GetEntity(_entityIdThatDamaged);
							if (entity is EntityVehicle entityVehicle)
							{
								entity = entityVehicle.GetFirstAttached();
							}
							GameEventManager.Current.HandleAction(blockDowngradeEvent, null, entity as EntityPlayer, twitchActivated: false, _blockPos);
						}
					}
					else
					{
						_world.SetBlockRPC(_clrIdx, _blockPos, downgradeBlock, block4.Density);
					}
					if ((num3 > 0 && EnablePassThroughDamage) || _bBypassMaxDamage)
					{
						block4.OnBlockDamaged(_world, _clrIdx, _blockPos, downgradeBlock, num3, _entityIdThatDamaged, _attackHitInfo, _bUseHarvestTool, _bBypassMaxDamage, _recDepth + 1);
					}
				}
				else
				{
					Entity entity2 = _world.GetEntity(_entityIdThatDamaged);
					QuestEventManager.Current.BlockDestroyed(block2, _blockPos, entity2);
					SpawnDestroyFX(_world, _blockValue, _blockPos, GetColorForSide(_blockValue, BlockFace.Top), _entityIdThatDamaged);
					_world.SetBlockRPC(_clrIdx, _blockPos, BlockValue.Air);
					if (_world.GetTileEntity(_blockPos) is TileEntityLootContainer tileEntityLootContainer)
					{
						tileEntityLootContainer.OnDestroy();
						if (!GameManager.IsDedicatedServer)
						{
							XUiC_LootWindowGroup.CloseIfOpenAtPos(_blockPos);
						}
						if (_world.GetChunkFromWorldPos(_blockPos) is Chunk chunk)
						{
							chunk.RemoveTileEntityAt<TileEntityLootContainer>((World)_world, World.toBlock(_blockPos));
						}
					}
					if (!string.IsNullOrEmpty(blockDestroyedEvent))
					{
						Entity entity3 = _world.GetEntity(_entityIdThatDamaged);
						if (entity3 is EntityVehicle entityVehicle2)
						{
							entity3 = entityVehicle2.GetFirstAttached();
						}
						GameEventManager.Current.HandleAction(blockDestroyedEvent, null, entity3 as EntityPlayer, twitchActivated: false, _blockPos);
					}
				}
			}
			return block2.MaxDamage;
		}
		if (_blockValue.damage != num)
		{
			_blockValue.damage = num;
			if (!block2.shape.IsTerrain())
			{
				_world.SetBlocksRPC(new List<BlockChangeInfo>
				{
					new BlockChangeInfo(_blockPos, _blockValue, _updateLight: false, _bOnlyDamage: true)
				});
			}
			else
			{
				sbyte density = _world.GetDensity(_clrIdx, _blockPos);
				sbyte b = (sbyte)Utils.FastMin(-1f, (float)MarchingCubes.DensityTerrain * (1f - (float)num / (float)block2.MaxDamage));
				if ((_damagePoints > 0 && b > density) || (_damagePoints < 0 && b < density))
				{
					_world.SetBlockRPC(_clrIdx, _blockPos, _blockValue, b);
				}
				else
				{
					_world.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
				}
			}
			if (terrainAlignmentMode != TerrainAlignmentMode.None)
			{
				MultiBlockManager.Instance.SetTerrainAlignmentDirty(_blockPos);
			}
		}
		return _blockValue.damage;
	}

	public virtual bool IsHealthShownInUI(HitInfoDetails _hit, BlockValue _bv)
	{
		if (isMultiBlock && _bv.ischild)
		{
			Vector3i vector3i = _hit.blockPos + _bv.parent;
			BlockValue block = GameManager.Instance.World.ChunkCache.GetBlock(vector3i);
			if (block.ischild)
			{
				Log.Error("Block on position {0} with name '{1}' should be a parent but is not! (6)", vector3i, block.Block.blockName);
				return false;
			}
			return block.Block.IsHealthShownInUI(_hit, block);
		}
		if (Stage2Health > 0)
		{
			return _bv.Block.MaxDamage - _bv.damage > Stage2Health;
		}
		return _bv.damage > 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte convertRotation(BlockValue _oldBV, BlockValue _newBV)
	{
		return _oldBV.rotation;
	}

	public void AddDroppedId(EnumDropEvent _eEvent, string _name, int _minCount, int _maxCount, float _prob, float _resourceScale, float _stickChance, string _toolCategory, string _tag)
	{
		itemsToDrop.TryGetValue(_eEvent, out var value);
		if (value == null)
		{
			value = new List<SItemDropProb>();
			itemsToDrop[_eEvent] = value;
		}
		value.Add(new SItemDropProb(_name, _minCount, _maxCount, _prob, _resourceScale, _stickChance, _toolCategory, _tag));
	}

	public bool HasItemsToDropForEvent(EnumDropEvent _eEvent)
	{
		return itemsToDrop.ContainsKey(_eEvent);
	}

	public void DropItemsOnEvent(WorldBase _world, BlockValue _blockValue, EnumDropEvent _eEvent, float _overallProb, Vector3 _dropPos, Vector3 _randomPosAdd, float _lifetime, int _entityId, bool _bGetSameItemIfNoneFound)
	{
		GameRandom gameRandom = _world.GetGameRandom();
		itemsDropped.Clear();
		if (!itemsToDrop.TryGetValue(_eEvent, out var value))
		{
			if (_bGetSameItemIfNoneFound)
			{
				ItemValue itemValue = _blockValue.ToItemValue();
				itemsDropped.Add(new ItemStack(itemValue, 1));
			}
		}
		else
		{
			for (int i = 0; i < value.Count; i++)
			{
				SItemDropProb sItemDropProb = value[i];
				int num = gameRandom.RandomRange(sItemDropProb.minCount, sItemDropProb.maxCount + 1);
				if (num <= 0)
				{
					continue;
				}
				if (sItemDropProb.stickChance < 0.001f || gameRandom.RandomFloat > sItemDropProb.stickChance)
				{
					if (sItemDropProb.name.Equals("[recipe]"))
					{
						List<Recipe> recipes = CraftingManager.GetRecipes(_blockValue.Block.GetBlockName());
						if (recipes.Count <= 0)
						{
							continue;
						}
						for (int j = 0; j < recipes[0].ingredients.Count; j++)
						{
							if (recipes[0].ingredients[j].count / 2 > 0)
							{
								ItemStack item = new ItemStack(recipes[0].ingredients[j].itemValue, recipes[0].ingredients[j].count / 2);
								itemsDropped.Add(item);
							}
						}
					}
					else
					{
						ItemValue itemValue2 = (sItemDropProb.name.Equals("*") ? _blockValue.ToItemValue() : new ItemValue(ItemClass.GetItem(sItemDropProb.name).type));
						if (!itemValue2.IsEmpty() && sItemDropProb.prob > gameRandom.RandomFloat)
						{
							itemsDropped.Add(new ItemStack(itemValue2, num));
						}
					}
					continue;
				}
				Vector3i vector3i = World.worldToBlockPos(_dropPos);
				if (!GameManager.Instance.World.IsWithinTraderArea(vector3i) && (_overallProb >= 0.999f || gameRandom.RandomFloat < _overallProb))
				{
					BlockValue blockValue = GetBlockValue(sItemDropProb.name);
					if (!blockValue.isair && _world.GetBlock(vector3i).isair)
					{
						_world.SetBlockRPC(vector3i, blockValue);
					}
				}
			}
		}
		for (int k = 0; k < itemsDropped.Count; k++)
		{
			if (_overallProb >= 0.999f || gameRandom.RandomFloat < _overallProb)
			{
				ItemClass itemClass = itemsDropped[k].itemValue.ItemClass;
				_lifetime = ((_lifetime > 0.001f) ? _lifetime : (itemClass?.GetLifetimeOnDrop() ?? 0f));
				if (!(_lifetime <= 0.001f))
				{
					_world.GetGameManager().ItemDropServer(itemsDropped[k], _dropPos, _randomPosAdd, _entityId, _lifetime);
				}
			}
		}
	}

	public float GetExplosionResistance()
	{
		return blockMaterial.ExplosionResistance;
	}

	public bool intersectRayWithBlock(BlockValue _blockValue, int _x, int _y, int _z, Ray _ray, out Vector3 _hitPoint, World _world)
	{
		staticList_IntersectRayWithBlockList.Clear();
		GetCollisionAABB(_blockValue, _x, _y, _z, 0f, staticList_IntersectRayWithBlockList);
		for (int i = 0; i < staticList_IntersectRayWithBlockList.Count; i++)
		{
			if (staticList_IntersectRayWithBlockList[i].IntersectRay(_ray))
			{
				_hitPoint = new Vector3(_x, _y, _z);
				return true;
			}
		}
		_hitPoint = Vector3.zero;
		return false;
	}

	public virtual DestroyedResult OnBlockDestroyedByExplosion(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _playerThatStartedExpl)
	{
		_world.ChunkCache?.InvokeOnBlockDamagedDelegates(_blockPos, _blockValue, _blockValue.Block.MaxDamage, _playerThatStartedExpl);
		return DestroyedResult.Downgrade;
	}

	public virtual void OnBlockStartsToFall(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		_world.SetBlockRPC(_blockPos, BlockValue.Air);
	}

	public virtual bool CanPlaceBlockAt(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
	{
		if (_blockPos.y > 253)
		{
			return false;
		}
		Block block = _blockValue.Block;
		if (!GameManager.Instance.IsEditMode())
		{
			if (!block.isMultiBlock)
			{
				if (((World)_world).IsWithinTraderPlacingProtection(_blockPos))
				{
					return false;
				}
			}
			else
			{
				Bounds bounds = block.multiBlockPos.CalcBounds(_blockValue.type, _blockValue.rotation);
				bounds.center += _blockPos.ToVector3();
				if (((World)_world).IsWithinTraderPlacingProtection(bounds))
				{
					return false;
				}
			}
		}
		if (block.isMultiBlock && _blockPos.y + block.multiBlockPos.dim.y >= 254)
		{
			return false;
		}
		if (!GameManager.Instance.IsEditMode() && block.bRestrictSubmergedPlacement && IsUnderwater(_world, _blockPos, _blockValue))
		{
			return false;
		}
		if (!(GameManager.Instance.IsEditMode() || _bOmitCollideCheck))
		{
			return !overlapsWithOtherBlock(_world, _clrIdx, _blockPos, _blockValue);
		}
		return true;
	}

	public Vector3i GetFreePlacementPosition(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityPlacing)
	{
		Vector3i vector3i = _blockPos;
		int num = 15;
		while (_blockValue.Block.overlapsWithOtherBlock(_world, _clrIdx, vector3i, _blockValue))
		{
			Vector3 direction = _entityPlacing.getHeadPosition() - (vector3i.ToVector3() + Vector3.one * 0.5f);
			vector3i = Voxel.OneVoxelStep(vector3i, vector3i.ToVector3() + Vector3.one * 0.5f, direction, out var _, out var _);
			if (--num <= 0)
			{
				break;
			}
		}
		if (num <= 0)
		{
			vector3i = _blockPos;
		}
		return vector3i;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool overlapsWithOtherBlock(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (!isMultiBlock)
		{
			int type = _world.GetBlock(_clrIdx, _blockPos).type;
			if (type != 0)
			{
				return !list[type].CanBlocksReplaceOrGroundCover();
			}
			return false;
		}
		byte rotation = _blockValue.rotation;
		for (int num = multiBlockPos.Length - 1; num >= 0; num--)
		{
			Vector3i pos = _blockPos + multiBlockPos.Get(num, _blockValue.type, rotation);
			int type2 = _world.GetBlock(_clrIdx, pos).type;
			if (type2 != 0 && !list[type2].CanBlocksReplaceOrGroundCover())
			{
				return true;
			}
		}
		return false;
	}

	public bool CanBlocksReplaceOrGroundCover()
	{
		if (!CanBlocksReplace)
		{
			return blockMaterial.IsGroundCover;
		}
		return true;
	}

	public bool IsUnderwater(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (isMultiBlock)
		{
			int num = _blockPos.y + multiBlockPos.dim.y - 1;
			for (int i = 0; i < multiBlockPos.Length; i++)
			{
				Vector3i pos = _blockPos + multiBlockPos.Get(i, _blockValue.type, _blockValue.rotation);
				if (pos.y == num && _world.IsWater(pos))
				{
					return true;
				}
			}
		}
		else if (_world.IsWater(_blockPos))
		{
			return true;
		}
		return false;
	}

	public virtual BlockValue OnBlockPlaced(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, GameRandom _rnd)
	{
		return _blockValue;
	}

	public virtual void OnBlockPlaceBefore(WorldBase _world, ref BlockPlacement.Result _bpResult, EntityAlive _ea, GameRandom _rnd)
	{
		DynamicMeshManager.ChunkChanged(_bpResult.blockPos, _ea?.entityId ?? (-2), _bpResult.blockValue.type);
		if (SelectAlternates)
		{
			byte rotation = _bpResult.blockValue.rotation;
			_bpResult.blockValue = _bpResult.blockValue.Block.GetAltBlockValue(_ea.inventory.holdingItemItemValue.Meta);
			_bpResult.blockValue.rotation = rotation;
		}
		else
		{
			string placeAltBlockValue = GetPlaceAltBlockValue(_world);
			_bpResult.blockValue = ((placeAltBlockValue.Length == 0) ? _bpResult.blockValue : GetBlockValue(placeAltBlockValue));
		}
		Block block = _bpResult.blockValue.Block;
		if (block.PlaceRandomRotation)
		{
			int num;
			do
			{
				num = _rnd.RandomRange(28);
			}
			while (((num < 4) ? (block.AllowedRotations & EBlockRotationClasses.Basic90) : ((num < 8) ? (block.AllowedRotations & EBlockRotationClasses.Headfirst) : ((num >= 24) ? (block.AllowedRotations & EBlockRotationClasses.Basic45) : (block.AllowedRotations & EBlockRotationClasses.Sideways)))) == EBlockRotationClasses.None);
			_bpResult.blockValue.rotation = (byte)num;
		}
	}

	public virtual void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		Block block = _result.blockValue.Block;
		int changingEntityId = ((_ea == null) ? (-1) : _ea.entityId);
		if (block.shape.IsTerrain())
		{
			_world.SetBlockRPC(_result.clrIdx, _result.blockPos, _result.blockValue, Density, changingEntityId);
		}
		else if (!block.IsTerrainDecoration)
		{
			_world.SetBlockRPC(_result.clrIdx, _result.blockPos, _result.blockValue, MarchingCubes.DensityAir, changingEntityId);
		}
		else
		{
			_world.SetBlockRPC(_result.clrIdx, _result.blockPos, _result.blockValue, changingEntityId);
		}
		if (blockName.Equals("keystoneBlock") && _ea is EntityPlayerLocal)
		{
			PlatformManager.NativePlatform.AchievementManager?.SetAchievementStat(EnumAchievementDataStat.LandClaimPlaced, 1);
		}
	}

	public virtual DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		return DestroyedResult.Downgrade;
	}

	public virtual ItemStack OnBlockPickedUp(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId)
	{
		ItemStack itemStack = new ItemStack((PickedUpItemValue == null) ? _blockValue.ToItemValue() : ItemClass.GetItem(PickedUpItemValue), 1);
		return (PickupTarget == null) ? itemStack : new ItemStack(new ItemValue(ItemClass.GetItem(PickupTarget).type), 1);
	}

	public virtual bool OnBlockActivated(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_world.GetTileEntity(_blockPos) is TileEntityLootContainer tileEntityLootContainer)
		{
			_player.AimingGun = false;
			Vector3i blockPos = tileEntityLootContainer.ToWorldPos();
			tileEntityLootContainer.bWasTouched = tileEntityLootContainer.bTouched;
			_world.GetGameManager().TELockServer(_clrIdx, blockPos, tileEntityLootContainer.entityId, _player.entityId);
			return true;
		}
		bool flag = CanPickup;
		Block block = _blockValue.Block;
		if (EffectManager.GetValue(PassiveEffects.BlockPickup, null, 0f, _player, null, block.Tags) > 0f)
		{
			flag = true;
		}
		if (!flag)
		{
			return false;
		}
		if (!_world.CanPickupBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer()))
		{
			_player.PlayOneShot("keystone_impact_overlay");
			return false;
		}
		if (_blockValue.damage > 0)
		{
			GameManager.ShowTooltip(_player, Localization.Get("ttRepairBeforePickup"), string.Empty, "ui_denied");
			return false;
		}
		ItemStack itemStack = block.OnBlockPickedUp(_world, _clrIdx, _blockPos, _blockValue, _player.entityId);
		if (!_player.inventory.CanTakeItem(itemStack) && !_player.bag.CanTakeItem(itemStack))
		{
			GameManager.ShowTooltip(_player, Localization.Get("xuiInventoryFullForPickup"), string.Empty, "ui_denied");
			return false;
		}
		_world.GetGameManager().PickupBlockServer(_clrIdx, _blockPos, _blockValue, _player.entityId);
		return false;
	}

	public void PickupOrDrop(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayer _player, bool forcePickup)
	{
		if (!(_player == null))
		{
			bool flag = CanPickup;
			Block block = _blockValue.Block;
			if (EffectManager.GetValue(PassiveEffects.BlockPickup, null, 0f, _player, null, block.Tags) > 0f)
			{
				flag = true;
			}
			if ((flag || forcePickup) && _world.CanPickupBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer()))
			{
				_world.GetGameManager().PickupBlockServer(_clrIdx, _blockPos, _blockValue, _player.entityId);
			}
		}
	}

	public virtual bool OnEntityCollidedWithBlock(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, Entity _entity)
	{
		return false;
	}

	public virtual void OnEntityWalking(WorldBase _world, int _x, int _y, int _z, BlockValue _blockValue, Entity entity)
	{
	}

	public virtual bool CanPlantStay(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		return true;
	}

	public void SetBlockName(string _blockName)
	{
		blockName = _blockName;
	}

	public string GetBlockName()
	{
		return blockName;
	}

	public static HashSet<string> GetAutoShapeMaterials()
	{
		return autoShapeMaterials;
	}

	public EAutoShapeType GetAutoShapeType()
	{
		return AutoShapeType;
	}

	public string GetAutoShapeBlockName()
	{
		return autoShapeBaseName;
	}

	public string GetAutoShapeShapeName()
	{
		return autoShapeShapeName;
	}

	public Block GetAutoShapeHelperBlock()
	{
		return autoShapeHelper;
	}

	public string GetLocalizedAutoShapeShapeName()
	{
		return Localization.Get("shape" + GetAutoShapeShapeName());
	}

	public bool AutoShapeSupportsShapeName(string _shapeName)
	{
		if (AutoShapeType == EAutoShapeType.Helper)
		{
			return ContainsAlternateBlock(autoShapeBaseName + ":" + _shapeName);
		}
		return false;
	}

	public int AutoShapeAlternateShapeNameIndex(string _shapeName)
	{
		if (AutoShapeType == EAutoShapeType.Helper)
		{
			return GetAlternateBlockIndex(autoShapeBaseName + ":" + _shapeName);
		}
		return -1;
	}

	public virtual string GetLocalizedBlockName()
	{
		if (localizedBlockName != null)
		{
			return localizedBlockName;
		}
		if (AutoShapeType != EAutoShapeType.None)
		{
			return localizedBlockName = blockMaterial.GetLocalizedMaterialName() + " - " + GetLocalizedAutoShapeShapeName();
		}
		return localizedBlockName = Localization.Get(GetBlockName());
	}

	public virtual string GetLocalizedBlockName(ItemValue _itemValueRef)
	{
		if (AutoShapeType != EAutoShapeType.Helper || _itemValueRef.ToBlockValue().Equals(BlockValue.Air))
		{
			return GetLocalizedBlockName();
		}
		GetAltBlocks();
		return placeAltBlockClasses[_itemValueRef.Meta].GetLocalizedBlockName();
	}

	public string GetIconName()
	{
		return CustomIcon ?? GetBlockName();
	}

	public void SetSideTextureId(int _textureId, int channel)
	{
		textureInfos[channel].singleTextureId = _textureId;
		textureInfos[channel].bTextureForEachSide = false;
	}

	public void SetSideTextureId(string[] _texIds, int channel)
	{
		textureInfos[channel].sideTextureIds = new int[_texIds.Length];
		for (int i = 0; i < _texIds.Length; i++)
		{
			textureInfos[channel].sideTextureIds[i] = int.Parse(_texIds[i]);
		}
		textureInfos[channel].bTextureForEachSide = true;
	}

	public int GetSideTextureId(BlockValue _blockValue, BlockFace _side, int channel)
	{
		if (textureInfos[channel].bTextureForEachSide)
		{
			int num = shape.MapSideAndRotationToTextureIdx(_blockValue, _side);
			if (num >= textureInfos[channel].sideTextureIds.Length)
			{
				num = 0;
			}
			return textureInfos[channel].sideTextureIds[num];
		}
		return textureInfos[channel].singleTextureId;
	}

	public MaterialBlock GetMaterialForSide(BlockValue _blockValue, BlockFace _side)
	{
		MaterialBlock materialBlock = null;
		int sideTextureId = GetSideTextureId(_blockValue, _side, 0);
		Block block = _blockValue.Block;
		if (sideTextureId != -1 && MeshDescription.meshes[block.MeshIndex].textureAtlas.uvMapping.Length > sideTextureId)
		{
			materialBlock = MeshDescription.meshes[block.MeshIndex].textureAtlas.uvMapping[sideTextureId].material;
		}
		if (materialBlock == null)
		{
			materialBlock = block.blockMaterial;
		}
		return materialBlock;
	}

	public int GetUiBackgroundTextureId(BlockValue _blockValue, BlockFace _side, int channel = 0)
	{
		if (uiBackgroundTextureId < 0)
		{
			return GetSideTextureId(_blockValue, _side, channel);
		}
		return uiBackgroundTextureId;
	}

	public string GetParticleForSide(BlockValue _blockValue, BlockFace _side)
	{
		MaterialBlock materialForSide = GetMaterialForSide(_blockValue, _side);
		if (materialForSide != null && materialForSide.ParticleCategory != null)
		{
			return materialForSide.ParticleCategory;
		}
		if (materialForSide != null && materialForSide.SurfaceCategory != null)
		{
			return materialForSide.SurfaceCategory;
		}
		return null;
	}

	public string GetDestroyParticle(BlockValue _blockValue)
	{
		if (blockMaterial.ParticleDestroyCategory != null)
		{
			return blockMaterial.ParticleDestroyCategory;
		}
		if (blockMaterial.ParticleCategory != null)
		{
			return blockMaterial.ParticleCategory;
		}
		if (blockMaterial.SurfaceCategory != null)
		{
			return blockMaterial.SurfaceCategory;
		}
		return null;
	}

	public Color GetColorForSide(BlockValue _blockValue, BlockFace _side)
	{
		TextureAtlas textureAtlas = MeshDescription.meshes[_blockValue.Block.MeshIndex].textureAtlas;
		int sideTextureId = GetSideTextureId(_blockValue, _side, 0);
		if (sideTextureId != -1 && textureAtlas.uvMapping.Length > sideTextureId)
		{
			return textureAtlas.uvMapping[sideTextureId].color;
		}
		return Color.gray;
	}

	public Color GetMapColor(BlockValue _blockValue, Vector3 _normal, int _yPos)
	{
		Color color = (bMapColorSet ? MapColor : ((!(_normal.x > 0.5f) && !(_normal.z > 0.5f) && !(_normal.x < -0.5f) && !(_normal.z < -0.5f)) ? GetColorForSide(_blockValue, BlockFace.Top) : GetColorForSide(_blockValue, BlockFace.South)));
		float num = MapSpecular;
		if (bMapColor2Set && MapElevMinMax.y != MapElevMinMax.x)
		{
			float num2 = (float)Utils.FastMax(_yPos - MapElevMinMax.x, 0) / (float)(MapElevMinMax.y - MapElevMinMax.x);
			color = Color.Lerp(MapColor, MapColor2, num2);
			num = Utils.FastMax(num - num2 * 0.5f, 0f);
		}
		float num3 = (_normal.z + 1f) / 2f * (_normal.x + 1f) / 2f;
		num3 *= 2f;
		color = Utils.Saturate(color * 0.5f + color * num3);
		color.a = num;
		return color;
	}

	public static bool CanDrop(BlockValue _blockValue)
	{
		return !_blockValue.Equals(BlockValue.Air);
	}

	public virtual bool IsElevator()
	{
		return false;
	}

	public virtual bool IsElevator(int rotation)
	{
		return false;
	}

	public virtual bool IsPlant()
	{
		if (!blockMaterial.IsPlant)
		{
			return bIsPlant;
		}
		return true;
	}

	public bool HasTag(BlockTags _tag)
	{
		return BlockTag == _tag;
	}

	public bool HasAnyFastTags(FastTags<TagGroup.Global> _tags)
	{
		return Tags.Test_AnySet(_tags);
	}

	public bool HasAllFastTags(FastTags<TagGroup.Global> _tags)
	{
		return Tags.Test_AllSet(_tags);
	}

	public virtual bool CanRepair(BlockValue _blockValue)
	{
		return _blockValue.damage > 0;
	}

	public virtual string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		Block block = _blockValue.Block;
		if (_world.GetTileEntity(_blockPos) is TileEntityLootContainer tileEntityLootContainer)
		{
			string arg = block.GetLocalizedBlockName();
			PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
			string arg2 = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
			if (!tileEntityLootContainer.bTouched)
			{
				return string.Format(Localization.Get("lootTooltipNew"), arg2, arg);
			}
			if (tileEntityLootContainer.IsEmpty())
			{
				return string.Format(Localization.Get("lootTooltipEmpty"), arg2, arg);
			}
			return string.Format(Localization.Get("lootTooltipTouched"), arg2, arg);
		}
		if (!CanPickup && !(EffectManager.GetValue(PassiveEffects.BlockPickup, null, 0f, _entityFocusing, null, _blockValue.Block.Tags) > 0f))
		{
			return null;
		}
		if (!_world.CanPickupBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer()))
		{
			return null;
		}
		string key = block.GetBlockName();
		if (!string.IsNullOrEmpty(block.PickedUpItemValue))
		{
			key = block.PickedUpItemValue;
		}
		else if (!string.IsNullOrEmpty(block.PickupTarget))
		{
			key = block.PickupTarget;
		}
		return string.Format(Localization.Get("pickupPrompt"), Localization.Get(key));
	}

	public void SpawnDowngradeFX(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, Color _color, int _entityIdThatCaused)
	{
		Block block = _blockValue.Block;
		if (block.DowngradeFX != null)
		{
			SpawnFX(_world, _blockPos, 1f, _color, _entityIdThatCaused, block.DowngradeFX);
		}
		else
		{
			SpawnDestroyParticleEffect(_world, _blockValue, _blockPos, 1f, _color, _entityIdThatCaused);
		}
	}

	public void SpawnDestroyFX(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, Color _color, int _entityIdThatCaused)
	{
		Block block = _blockValue.Block;
		if (block.DestroyFX != null)
		{
			SpawnFX(_world, _blockPos, 1f, _color, _entityIdThatCaused, block.DestroyFX);
		}
		else
		{
			SpawnDestroyParticleEffect(_world, _blockValue, _blockPos, 1f, _color, _entityIdThatCaused);
		}
	}

	public virtual void SpawnDestroyParticleEffect(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, float _lightValue, Color _color, int _entityIdThatCaused)
	{
		if (deathParticleName != null)
		{
			_world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect(deathParticleName, World.blockToTransformPos(_blockPos) + new Vector3(0f, 0.5f, 0f), _lightValue, _color, blockMaterial.SurfaceCategory + "destroy", null, _OLDCreateColliders: true), _entityIdThatCaused, _forceCreation: false, _worldSpawn: true);
			return;
		}
		MaterialBlock materialForSide = GetMaterialForSide(_blockValue, BlockFace.Top);
		string destroyParticle = GetDestroyParticle(_blockValue);
		if (destroyParticle != null && materialForSide.SurfaceCategory != null)
		{
			_world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect("blockdestroy_" + destroyParticle, World.blockToTransformPos(_blockPos) + new Vector3(0f, 0.5f, 0f), _lightValue, _color, blockMaterial.SurfaceCategory + "destroy", null, _OLDCreateColliders: true), _entityIdThatCaused, _forceCreation: false, _worldSpawn: true);
		}
	}

	public void SpawnFX(WorldBase _world, Vector3i _blockPos, float _lightValue, Color _color, int _entityIdThatCaused, string _fxName)
	{
		string[] array = _fxName.Split(',');
		_world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect(array[0], World.blockToTransformPos(_blockPos) + new Vector3(0f, 0.5f, 0f), _lightValue, _color, array[1], null, _OLDCreateColliders: true), _entityIdThatCaused, _forceCreation: false, _worldSpawn: true);
	}

	public static BlockValue GetBlockValue(string _blockName, bool _caseInsensitive = false)
	{
		return GetBlockByName(_blockName, _caseInsensitive)?.ToBlockValue() ?? BlockValue.Air;
	}

	public static Block GetBlockByName(string _blockname, bool _caseInsensitive = false)
	{
		if (nameToBlock == null)
		{
			return null;
		}
		Block value;
		if (_caseInsensitive)
		{
			nameToBlockCaseInsensitive.TryGetValue(_blockname, out value);
		}
		else
		{
			nameToBlock.TryGetValue(_blockname, out value);
		}
		return value;
	}

	public BlockValue ToBlockValue()
	{
		return new BlockValue
		{
			type = blockID
		};
	}

	public static BlockValue GetBlockValue(int _blockType)
	{
		if (list[_blockType] == null)
		{
			return BlockValue.Air;
		}
		return new BlockValue
		{
			type = _blockType
		};
	}

	public BlockValue GetBlockValueFromProperty(string _propValue)
	{
		BlockValue air = BlockValue.Air;
		if (!Properties.Values.ContainsKey(_propValue))
		{
			throw new Exception("You need to specify a property with name '" + _propValue + "' for the block " + blockName);
		}
		air = GetBlockValue(Properties.Values[_propValue]);
		if (air.Equals(BlockValue.Air))
		{
			throw new Exception("Block with name '" + Properties.Values[_propValue] + "' not found!");
		}
		return air;
	}

	public virtual bool ShowModelOnFall()
	{
		return bShowModelOnFall;
	}

	public virtual bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.GetTileEntity(_blockPos) is TileEntityLootContainer;
		bool flag2 = CanPickup;
		if (EffectManager.GetValue(PassiveEffects.BlockPickup, null, 0f, _entityFocusing, null, _blockValue.Block.Tags) > 0f)
		{
			flag2 = true;
		}
		if (!(flag2 || flag))
		{
			return CustomCmds.Length != 0;
		}
		return true;
	}

	public virtual BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntityLootContainer obj = _world.GetTileEntity(_blockPos) as TileEntityLootContainer;
		bool flag = false;
		bool flag2 = CanPickup;
		if (EffectManager.GetValue(PassiveEffects.BlockPickup, null, 0f, _entityFocusing, null, _blockValue.Block.Tags) > 0f)
		{
			flag2 = true;
		}
		if (flag2)
		{
			cmds[0].enabled = true;
			flag = true;
		}
		if (obj != null)
		{
			cmds[1].enabled = true;
			flag = true;
		}
		if (!flag)
		{
			return BlockActivationCommand.Empty;
		}
		return cmds;
	}

	public virtual bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_commandName == cmds[0].text || _commandName == cmds[1].text)
		{
			OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
			return true;
		}
		return false;
	}

	public virtual void RenderDecorations(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, TextureFullArray _textureFullArray, VoxelMesh[] _meshes, INeighborBlockCache _nBlocks)
	{
		shape.renderDecorations(_worldPos, _blockValue, _drawPos, _vertices, _lightingAround, _textureFullArray, _meshes, _nBlocks);
	}

	public virtual bool IsExplosionAffected()
	{
		return true;
	}

	public int GetActivationDistanceSq()
	{
		int num = activationDistance;
		if (num == 0)
		{
			return (int)(Constants.cCollectItemDistance * Constants.cCollectItemDistance);
		}
		return num * num;
	}

	public int GetPlacementDistanceSq()
	{
		int num = placementDistance;
		if (num == 0)
		{
			num = activationDistance;
		}
		if (num == 0)
		{
			return (int)(Constants.cDigAndBuildDistance * Constants.cDigAndBuildDistance);
		}
		return num * num;
	}

	public virtual void CheckUpdate(BlockValue _oldBV, BlockValue _newBV, out bool bUpdateMesh, out bool bUpdateNotify, out bool bUpdateLight)
	{
		bUpdateMesh = (bUpdateNotify = (bUpdateLight = true));
	}

	public virtual bool RotateVerticesOnCollisionCheck(BlockValue _blockValue)
	{
		return true;
	}

	public virtual bool ActivateBlock(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool isOn, bool isPowered)
	{
		return false;
	}

	public virtual bool ActivateBlockOnce(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		return false;
	}

	public virtual void OnTriggerAddedFromPrefab(BlockTrigger _trigger, Vector3i _blockPos, BlockValue _blockValue, FastTags<TagGroup.Global> _questTags)
	{
	}

	public virtual void OnTriggerRefresh(BlockTrigger _trigger, BlockValue _bv, FastTags<TagGroup.Global> questTag)
	{
	}

	public virtual void OnTriggerChanged(BlockTrigger _trigger, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
	}

	public virtual void OnTriggerChanged(BlockTrigger _trigger, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges)
	{
	}

	public virtual void OnTriggered(EntityPlayer _player, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
	}

	public virtual void Refresh(WorldBase _world, Chunk _chunk, int _cIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
	}

	public void HandleTrigger(EntityPlayer _player, World _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageBlockTrigger>().Setup(_cIdx, _blockPos, _blockValue));
			return;
		}
		BlockTrigger blockTrigger = ((Chunk)_world.ChunkCache.GetChunkSync(World.toChunkXZ(_blockPos.x), _blockPos.y, World.toChunkXZ(_blockPos.z))).GetBlockTrigger(World.toBlock(_blockPos));
		if (blockTrigger != null)
		{
			_world.triggerManager.TriggerBlocks(_player, _player.prefab, blockTrigger);
		}
	}

	public override string ToString()
	{
		return blockName + " " + blockID;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void assignIdsLinear()
	{
		bool[] usedIds = new bool[MAX_BLOCKS];
		List<Block> list = new List<Block>(nameToBlock.Count);
		nameToBlock.CopyValuesTo(list);
		assignLeftOverBlocks(usedIds, list);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void assignId(Block _b, int _id, bool[] _usedIds)
	{
		list[_id] = _b;
		_b.blockID = _id;
		_usedIds[_id] = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void assignLeftOverBlocks(bool[] _usedIds, List<Block> _unassignedBlocks)
	{
		foreach (KeyValuePair<string, int> fixedBlockId in fixedBlockIds)
		{
			if (nameToBlock.ContainsKey(fixedBlockId.Key))
			{
				Block block = nameToBlock[fixedBlockId.Key];
				if (_unassignedBlocks.Contains(block))
				{
					_unassignedBlocks.Remove(block);
					assignId(block, fixedBlockId.Value, _usedIds);
				}
			}
		}
		int num = 0;
		int num2 = 255;
		foreach (Block _unassignedBlock in _unassignedBlocks)
		{
			if (_unassignedBlock.shape.IsTerrain())
			{
				while (_usedIds[++num])
				{
				}
				assignId(_unassignedBlock, num, _usedIds);
			}
			else
			{
				while (_usedIds[++num2])
				{
				}
				assignId(_unassignedBlock, num2, _usedIds);
			}
		}
		Log.Out("Block IDs total {0}, terr {1}, last {2}", nameToBlock.Count, num, num2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void assignIdsFromMapping()
	{
		List<Block> list = new List<Block>();
		bool[] usedIds = new bool[MAX_BLOCKS];
		foreach (KeyValuePair<string, Block> item in nameToBlock)
		{
			int idForName = nameIdMapping.GetIdForName(item.Key);
			if (idForName >= 0)
			{
				assignId(item.Value, idForName, usedIds);
			}
			else
			{
				list.Add(item.Value);
			}
		}
		assignLeftOverBlocks(usedIds, list);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void createFullMappingForClients()
	{
		NameIdMapping nameIdMapping = new NameIdMapping(null, MAX_BLOCKS);
		foreach (KeyValuePair<string, Block> item in nameToBlock)
		{
			nameIdMapping.AddMapping(item.Value.blockID, item.Key);
		}
		fullMappingDataForClients = nameIdMapping.SaveToArray();
	}

	public static void AssignIds()
	{
		if (nameToBlock.Count > MAX_BLOCKS)
		{
			throw new ArgumentOutOfRangeException($"Too many blocks defined ({nameToBlock.Count}, allowed {MAX_BLOCKS}");
		}
		if (nameIdMapping != null)
		{
			Log.Out("Block IDs with mapping");
			assignIdsFromMapping();
		}
		else
		{
			Log.Out("Block IDs withOUT mapping");
			assignIdsLinear();
		}
		createFullMappingForClients();
	}

	public virtual bool IsTileEntitySavedInPrefab()
	{
		return false;
	}

	public virtual string GetCustomDescription(Vector3i _blockPos, BlockValue _bv)
	{
		return "";
	}

	public string GetPlaceAltBlockValue(WorldBase _world)
	{
		if (placeAltBlockNames != null && placeAltBlockNames.Length != 0)
		{
			return placeAltBlockNames[_world.GetGameRandom().RandomRange(0, placeAltBlockNames.Length)];
		}
		return string.Empty;
	}

	public Block GetAltBlock(int _typeId)
	{
		GetAltBlocks();
		if (placeAltBlockClasses != null && placeAltBlockClasses.Length != 0)
		{
			return placeAltBlockClasses[_typeId];
		}
		return list[0];
	}

	public BlockValue GetAltBlockValue(int typeID)
	{
		return GetAltBlock(typeID).ToBlockValue();
	}

	public string[] GetAltBlockNames()
	{
		return placeAltBlockNames;
	}

	public Block[] GetAltBlocks()
	{
		if (placeAltBlockClasses == null && placeAltBlockNames != null)
		{
			placeAltBlockClasses = new Block[placeAltBlockNames.Length];
			for (int i = 0; i < placeAltBlockNames.Length; i++)
			{
				placeAltBlockClasses[i] = GetBlockByName(placeAltBlockNames[i]);
			}
		}
		return placeAltBlockClasses;
	}

	public int AlternateBlockCount()
	{
		return placeAltBlockNames.Length;
	}

	public bool ContainsAlternateBlock(string block)
	{
		for (int i = 0; i < placeAltBlockNames.Length; i++)
		{
			if (placeAltBlockNames[i] == block)
			{
				return true;
			}
		}
		return false;
	}

	public int GetAlternateBlockIndex(string block)
	{
		for (int i = 0; i < placeAltBlockNames.Length; i++)
		{
			if (placeAltBlockNames[i] == block)
			{
				return i;
			}
		}
		return -1;
	}

	public static void GetShapeCategories(IEnumerable<Block> _altBlocks, List<ShapesFromXml.ShapeCategory> _targetList)
	{
		_targetList.Clear();
		foreach (Block _altBlock in _altBlocks)
		{
			if (_altBlock.ShapeCategories == null)
			{
				continue;
			}
			foreach (ShapesFromXml.ShapeCategory shapeCategory in _altBlock.ShapeCategories)
			{
				if (!_targetList.Contains(shapeCategory))
				{
					_targetList.Add(shapeCategory);
				}
			}
		}
		_targetList.Sort();
	}

	public int GetShownMaxDamage()
	{
		if (this is BlockDoor)
		{
			return MaxDamagePlusDowngrades;
		}
		return MaxDamage;
	}

	public bool SupportsRotation(byte _rotation)
	{
		if (_rotation < 4)
		{
			return (AllowedRotations & EBlockRotationClasses.Basic90) != 0;
		}
		if (_rotation < 8)
		{
			return (AllowedRotations & EBlockRotationClasses.Headfirst) != 0;
		}
		if (_rotation < 24)
		{
			return (AllowedRotations & EBlockRotationClasses.Sideways) != 0;
		}
		return (AllowedRotations & EBlockRotationClasses.Basic45) != 0;
	}

	public void RotateHoldingBlock(ItemClassBlock.ItemBlockInventoryData _blockInventoryData, bool _increaseRotation, bool _playSoundOnRotation = true)
	{
		if (_blockInventoryData.mode == BlockPlacement.EnumRotationMode.Auto)
		{
			_blockInventoryData.mode = BlockPlacement.EnumRotationMode.Simple;
		}
		BlockValue bv = _blockInventoryData.itemValue.ToBlockValue();
		bv.rotation = _blockInventoryData.rotation;
		bv = BlockPlacementHelper.OnPlaceBlock(_blockInventoryData.mode, _blockInventoryData.localRot, _blockInventoryData.world, bv, _blockInventoryData.hitInfo.hit, _blockInventoryData.holdingEntity.position).blockValue;
		int rotation = _blockInventoryData.rotation;
		_blockInventoryData.rotation = BlockPlacementHelper.LimitRotation(_blockInventoryData.mode, ref _blockInventoryData.localRot, _blockInventoryData.hitInfo.hit, _increaseRotation, bv, bv.rotation);
		if (_playSoundOnRotation && rotation != _blockInventoryData.rotation)
		{
			_blockInventoryData.holdingEntity.PlayOneShot("rotateblock");
		}
	}

	public void GroundAlign(BlockEntityData _data)
	{
		if (!_data.bHasTransform)
		{
			return;
		}
		BlockValue blockValue = _data.blockValue;
		int type = blockValue.type;
		Transform transform = _data.transform;
		GameObject gameObject = transform.gameObject;
		gameObject.SetActive(value: false);
		Vector3 toDirection = Vector3.zero;
		int num = 0;
		Ray ray = new Ray(Vector3.zero, Vector3.down);
		Vector3 vector = new Vector3(0.5f, 0.75f, 0.5f) - Origin.position;
		float num2 = GroundAlignDistance + 0.5f;
		Vector3i vector3i = _data.pos;
		Vector3 position = transform.position;
		Vector3 vector2;
		RaycastHit hitInfo;
		if (!isMultiBlock)
		{
			vector2 = new Vector3(0f, float.MinValue, 0f);
			ray.origin = vector3i.ToVector3() + vector;
			bool flag = Physics.SphereCast(ray, 0.22f, out hitInfo, num2 - 0.22f + 0.25f, 1082195968);
			if (!flag)
			{
				flag = Physics.SphereCast(ray, 0.48f, out hitInfo, num2 - 0.48f + 0.25f, 1082195968);
			}
			if (flag)
			{
				vector2 = hitInfo.point;
				toDirection = hitInfo.normal;
				num = 1;
			}
		}
		else
		{
			if (blockValue.ischild)
			{
				vector3i = new Vector3i(blockValue.parentx, blockValue.parenty, blockValue.parentz);
			}
			vector2 = position;
			vector2.y = float.MinValue;
			byte rotation = blockValue.rotation;
			for (int num3 = multiBlockPos.Length - 1; num3 >= 0; num3--)
			{
				Vector3i vector3i2 = multiBlockPos.Get(num3, type, rotation);
				if (vector3i2.y == 0)
				{
					ray.origin = (vector3i + vector3i2).ToVector3() + vector;
					if (Physics.SphereCast(ray, 0.22f, out hitInfo, num2 - 0.22f + 0.25f, 1082195968))
					{
						if (vector2.y < hitInfo.point.y)
						{
							vector2.y = hitInfo.point.y;
						}
						toDirection += hitInfo.normal;
						num++;
					}
				}
			}
			if (num > 0)
			{
				toDirection *= 1f / (float)num;
				toDirection.Normalize();
			}
		}
		if (num > 0)
		{
			position = vector2;
			Quaternion rotation2 = transform.rotation;
			rotation2 = Quaternion.FromToRotation(Vector3.up, toDirection) * rotation2;
			transform.SetPositionAndRotation(position, rotation2);
		}
		gameObject.SetActive(value: true);
	}

	public static void CacheStats()
	{
		PropertiesCache?.Stats();
	}
}
