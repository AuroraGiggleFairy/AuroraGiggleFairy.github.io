using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityClass
{
	public enum CensorModeType
	{
		None,
		ZPrefab,
		Dismemberment,
		ZPrefabAndDismemberment
	}

	public enum UserSpawnType
	{
		None,
		Console,
		Menu
	}

	public struct LootDrop
	{
		public int entityClass;

		public float weight;
	}

	public struct ParticleData
	{
		public string fileName;

		public string shapeMesh;
	}

	public static string PropEntityFlags = "EntityFlags";

	public static string PropEntityType = "EntityType";

	public static string PropClass = "Class";

	public static string PropCensor = "Censor";

	public static string PropMesh = "Mesh";

	public static string PropMeshFP = "MeshFP";

	public static string PropPrefab = "Prefab";

	public static string PropPrefabCombined = "PrefabCombined";

	public static string PropParent = "Parent";

	public static string PropAvatarController = "AvatarController";

	public static string PropLocalAvatarController = "LocalAvatarController";

	public static string PropSkinTexture = "SkinTexture";

	public static string PropAltMats = "AltMats";

	public static string PropSwapMats = "SwapMats";

	public static string PropMatColor = "MatColor";

	public static string PropRightHandJointName = "RightHandJointName";

	public static string PropHandItem = "HandItem";

	public static string PropHandItemCrawler = "HandItemCrawler";

	public static string PropMaxHealth = "MaxHealth";

	public static string PropMaxStamina = "MaxStamina";

	public static string PropSickness = "Sickness";

	public static string PropGassiness = "Gassiness";

	public static string PropWellness = "Wellness";

	public static string PropFood = "Food";

	public static string PropWater = "Water";

	public static string PropMaxViewAngle = "MaxViewAngle";

	public static string PropWeight = "Weight";

	public static string PropPushFactor = "PushFactor";

	public static string PropTimeStayAfterDeath = "TimeStayAfterDeath";

	public static string PropImmunity = "Immunity";

	public static string PropIsMale = "IsMale";

	public static string PropIsChunkObserver = "IsChunkObserver";

	public static string PropAIFeralSense = "AIFeralSense";

	public static string PropAIGroupCircle = "AIGroupCircle";

	public static string PropAINoiseSeekDist = "AINoiseSeekDist";

	public static string PropAISeeOffset = "AISeeOffset";

	public static string PropAIPathCostScale = "AIPathCostScale";

	public static string PropAITask = "AITask-";

	public static string PropAITargetTask = "AITarget-";

	public static string PropMoveSpeed = "MoveSpeed";

	public static string PropMoveSpeedNight = "MoveSpeedNight";

	public static string PropMoveSpeedAggro = "MoveSpeedAggro";

	public static string PropMoveSpeedRand = "MoveSpeedRand";

	public static string PropMoveSpeedPanic = "MoveSpeedPanic";

	public static string PropMoveSpeedPattern = "MoveSpeedPattern";

	public static string PropSwimSpeed = "SwimSpeed";

	public static string PropSwimStrokeRate = "SwimStrokeRate";

	public static string PropCrouchType = "CrouchType";

	public static string PropDanceType = "DanceType";

	public static string PropWalkType = "WalkType";

	public static string PropCanClimbVertical = "CanClimbVertical";

	public static string PropCanClimbLadders = "CanClimbLadders";

	public static string PropJumpDelay = "JumpDelay";

	public static string PropJumpMaxDistance = "JumpMaxDistance";

	public static string PropIsEnemyEntity = "IsEnemyEntity";

	public static string PropIsAnimalEntity = "IsAnimalEntity";

	public static string PropSoundRandomTime = "SoundRandomTime";

	public static string PropSoundAlertTime = "SoundAlertTime";

	public static string PropSoundRandom = "SoundRandom";

	public static string PropSoundHurt = "SoundHurt";

	public static string PropSoundJump = "SoundJump";

	public static string PropSoundHurtSmall = "SoundHurtSmall";

	public static string PropSoundDrownPain = "SoundDrownPain";

	public static string PropSoundDrownDeath = "SoundDrownDeath";

	public static string PropSoundWaterSurface = "SoundWaterSurface";

	public static string PropSoundDeath = "SoundDeath";

	public static string PropSoundAttack = "SoundAttack";

	public static string PropSoundAlert = "SoundAlert";

	public static string PropSoundSense = "SoundSense";

	public static string PropSoundStamina = "SoundStamina";

	public static string PropSoundLiving = "SoundLiving";

	public static string PropSoundSpawn = "SoundSpawn";

	public static string PropSoundLand = "SoundLanding";

	public static string PropSoundStepType = "SoundStepType";

	public static string PropSoundGiveUp = "SoundGiveUp";

	public static string PropSoundExplodeWarn = "SoundExplodeWarn";

	public static string PropSoundTick = "SoundTick";

	public static string PropExplodeDelay = "ExplodeDelay";

	public static string PropExplodeHealthThreshold = "ExplodeHealthThreshold";

	public static string PropLootListOnDeath = "LootListOnDeath";

	public static string PropLootListAlive = "LootListAlive";

	public static string PropLootDropProb = "LootDropProb";

	public static string PropLootDropEntityClass = "LootDropEntityClass";

	public static string PropAttackTimeoutDay = "AttackTimeoutDay";

	public static string PropAttackTimeoutNight = "AttackTimeoutNight";

	public static string PropMapIcon = "MapIcon";

	public static string PropCompassIcon = "CompassIcon";

	public static string PropTrackerIcon = "TrackerIcon";

	public static string PropCompassUpIcon = "CompassUpIcon";

	public static string PropCompassDownIcon = "CompassDownIcon";

	public static string PropParticleOnSpawn = "ParticleOnSpawn";

	public static string PropParticleOnDeath = "ParticleOnDeath";

	public static string PropParticleOnDestroy = "ParticleOnDestroy";

	public static string PropItemsOnEnterGame = "ItemsOnEnterGame";

	public static string PropFallLandBehavior = "FallLandBehavior";

	public static string PropDestroyBlockBehavior = "DestroyBlockBehavior";

	public static string PropDropInventoryBlock = "DropInventoryBlock";

	public static string PropModelType = "ModelType";

	public static string PropRagdollOnDeathChance = "RagdollOnDeathChance";

	public static string PropHasRagdoll = "HasRagdoll";

	public static string PropMass = "Mass";

	public static string PropSizeScale = "SizeScale";

	public static string PropPhysicsBody = "PhysicsBody";

	public static string PropColliders = "Colliders";

	public static string PropLookAtAngle = "LookAtAngle";

	public static string PropCrouchYOffsetFP = "CrouchYOffsetFP";

	public static string PropRotateToGround = "RotateToGround";

	public static string PropCorpseBlock = "CorpseBlock";

	public static string PropCorpseBlockChance = "CorpseBlockChance";

	public static string PropCorpseBlockDensity = "CorpseBlockDensity";

	public static string PropRootMotion = "RootMotion";

	public static string PropExperienceGain = "ExperienceGain";

	public static string PropHasDeathAnim = "HasDeathAnim";

	public static string PropLegCrippleScale = "LegCrippleScale";

	public static string PropLegCrawlerThreshold = "LegCrawlerThreshold";

	public static string PropDismemberMultiplierHead = "DismemberMultiplierHead";

	public static string PropDismemberMultiplierArms = "DismemberMultiplierArms";

	public static string PropDismemberMultiplierLegs = "DismemberMultiplierLegs";

	public static string PropKnockdownKneelDamageThreshold = "KnockdownKneelDamageThreshold";

	public static string PropKnockdownKneelStunDuration = "KnockdownKneelStunDuration";

	public static string PropKnockdownProneDamageThreshold = "KnockdownProneDamageThreshold";

	public static string PropKnockdownProneStunDuration = "KnockdownProneStunDuration";

	public static string PropKnockdownProneRefillRate = "KnockdownProneRefillRate";

	public static string PropKnockdownKneelRefillRate = "KnockdownKneelRefillRate";

	public static string PropArmsExplosionDamageMultiplier = "ArmsExplosionDamageMultiplier";

	public static string PropLegsExplosionDamageMultiplier = "LegsExplosionDamageMultiplier";

	public static string PropChestExplosionDamageMultiplier = "ChestExplosionDamageMultiplier";

	public static string PropHeadExplosionDamageMultiplier = "HeadExplosionDamageMultiplier";

	public static string PropPainResistPerHit = "PainResistPerHit";

	public static string PropArchetype = "Archetype";

	public static string PropSwimOffset = "SwimOffset";

	public static string PropUMARace = "UMARace";

	public static string PropUMAGeneratedModelName = "UMAGeneratedModelName";

	public static string PropNPCID = "NPCID";

	public static string PropModelTransformAdjust = "ModelTransformAdjust";

	public static string PropAIPackages = "AIPackages";

	public static string PropBuffs = "Buffs";

	public static string PropStealthSoundDecayRate = "StealthSoundDecayRate";

	public static string PropSightRange = "SightRange";

	public static string PropSightLightThreshold = "SightLightThreshold";

	public static string PropSleeperSightToSenseMin = "SleeperSightToSenseMin";

	public static string PropSleeperSightToSenseMax = "SleeperSightToSenseMax";

	public static string PropSleeperSightToWakeMin = "SleeperSightToWakeMin";

	public static string PropSleeperSightToWakeMax = "SleeperSightToWakeMax";

	public static string PropSleeperNoiseToSense = "SleeperNoiseToSense";

	public static string PropSleeperNoiseToSenseSoundChance = "SleeperNoiseToSenseSoundChance";

	public static string PropSleeperNoiseToWake = "SleeperNoiseToWake";

	public static string PropSoundSleeperSense = "SoundSleeperSense";

	public static string PropSoundSleeperSnore = "SoundSleeperBackToSleep";

	public static string PropMaxTurnSpeed = "MaxTurnSpeed";

	public static string PropSearchRadius = "SearchRadius";

	public static string PropTags = "Tags";

	public static string PropNavObject = "NavObject";

	public static string PropNavObjectHeadOffset = "NavObjectHeadOffset";

	public static string PropStompsSpikes = "StompsSpikes";

	public static string PropUserSpawnType = "UserSpawnType";

	public static string PropHideInSpawnMenu = "HideInSpawnMenu";

	public static string PropCanBigHead = "CanBigHead";

	public static string PropOnActivateEvent = "ActivateEvent";

	public static string PropCustomCommandName = "CustomCommandName";

	public static string PropCustomCommandIcon = "CustomCommandIcon";

	public static string PropCustomCommandIconColor = "CustomCommandIconColor";

	public static string PropCustomCommandEvent = "CustomCommandEvent";

	public static string PropCustomCommandActivateTime = "CustomCommandActivateTime";

	public static readonly int itemClass = FromString("item");

	public static readonly int fallingBlockClass = FromString("fallingBlock");

	public static readonly int fallingTreeClass = FromString("fallingTree");

	public static readonly int playerMaleClass = FromString("playerMale");

	public static readonly int playerFemaleClass = FromString("playerFemale");

	public static readonly int playerNewMaleClass = FromString("playerNewMale");

	public static readonly int junkDroneClass = FromString("entityJunkDrone");

	public static Dictionary<string, Color> sColors = new Dictionary<string, Color>();

	public static DictionarySave<int, EntityClass> list = new DictionarySave<int, EntityClass>();

	public DynamicProperties Properties = new DynamicProperties();

	public Type classname;

	public int censorMode;

	public EntityFlags entityFlags;

	public int censorType;

	public string prefabPath;

	public Transform prefabT;

	public bool IsPrefabCombined;

	public string meshPath;

	public Transform mesh;

	public Transform meshFP;

	public string skinTexture;

	public string parentGameObjectName;

	public string entityClassName;

	public UserSpawnType userSpawnType = UserSpawnType.Menu;

	public bool bIsEnemyEntity;

	public bool bIsAnimalEntity;

	public ExplosionData explosionData;

	public Type modelType;

	public float MassKg;

	public float SizeScale;

	public float RagdollOnDeathChance;

	public bool HasRagdoll;

	public string CollidersRagdollAsset;

	public float LookAtAngle;

	public float crouchYOffsetFP;

	public string CorpseBlockId;

	public float CorpseBlockChance;

	public int CorpseBlockDensity;

	public float MaxTurnSpeed;

	public bool RootMotion;

	public bool HasDeathAnim;

	public bool bIsMale;

	public bool bIsChunkObserver;

	public int ExperienceValue;

	public PhysicsBodyLayout PhysicsBody;

	public int DeadBodyHitPoints;

	public List<LootDrop> lootDrops;

	public float LegCrippleScale;

	public float LegCrawlerThreshold;

	public float DismemberMultiplierHead;

	public float DismemberMultiplierArms;

	public float DismemberMultiplierLegs;

	public float LowerLegDismemberThreshold;

	public float LowerLegDismemberBonusChance;

	public float LowerLegDismemberBaseChance;

	public float UpperLegDismemberThreshold;

	public float UpperLegDismemberBonusChance;

	public float UpperLegDismemberBaseChance;

	public float LowerArmDismemberThreshold;

	public float LowerArmDismemberBonusChance;

	public float LowerArmDismemberBaseChance;

	public float UpperArmDismemberThreshold;

	public float UpperArmDismemberBonusChance;

	public float UpperArmDismemberBaseChance;

	public float KnockdownKneelDamageThreshold;

	public float LegsExplosionDamageMultiplier;

	public float ArmsExplosionDamageMultiplier;

	public float ChestExplosionDamageMultiplier;

	public float HeadExplosionDamageMultiplier;

	public float PainResistPerHit;

	public float PainResistPerHitLowHealth;

	public float PainResistPerHitLowHealthPercent;

	public float SearchRadius;

	public float SwimOffset;

	public float SightRange;

	public Vector2 SleeperSightToSenseMin;

	public Vector2 SleeperSightToSenseMax;

	public Vector2 SleeperSightToWakeMin;

	public Vector2 SleeperSightToWakeMax;

	public Vector2 sightLightThreshold;

	public Vector2 NoiseAlert;

	public Vector2 SleeperNoiseToSense;

	public float SleeperNoiseToSenseSoundChance;

	public Vector2 SleeperNoiseToWake;

	public string UMARace;

	public string UMAGeneratedModelName;

	public string[] AltMatNames;

	public string[] MatSwap;

	public ParticleData particleOnSpawn;

	public Vector2 KnockdownKneelStunDuration;

	public float KnockdownProneDamageThreshold;

	public Vector2 KnockdownProneStunDuration;

	public Vector2 KnockdownProneRefillRate;

	public Vector2 KnockdownKneelRefillRate;

	public Vector3 ModelTransformAdjust;

	public string ArchetypeName;

	public string[] AIPackages;

	public bool UseAIPackages;

	public Dictionary<EnumDropEvent, List<Block.SItemDropProb>> itemsToDrop = new EnumDictionary<EnumDropEvent, List<Block.SItemDropProb>>();

	public List<string> Buffs;

	public FastTags<TagGroup.Global> Tags;

	public string NavObject = "";

	public Vector3 NavObjectHeadOffset = Vector3.zero;

	public bool CanBigHead = true;

	public int DanceTypeID;

	public MinEffectController Effects;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] commaSeparator = new char[1] { ',' };

	public string onActivateEvent = "";

	public static void Add(string _entityClassname, EntityClass _entityClass)
	{
		_entityClass.entityClassName = _entityClassname;
		list[_entityClassname.GetHashCode()] = _entityClass;
	}

	public static EntityClass GetEntityClass(int entityClass)
	{
		list.TryGetValue(entityClass, out var _value);
		return _value;
	}

	public static string GetEntityClassName(int entityClass)
	{
		if (list.TryGetValue(entityClass, out var _value))
		{
			return _value.entityClassName;
		}
		return "null";
	}

	public static int GetId(string _name)
	{
		foreach (KeyValuePair<int, EntityClass> item in list.Dict)
		{
			if (item.Value.entityClassName == _name)
			{
				return item.Key;
			}
		}
		return -1;
	}

	public static int FromString(string _s)
	{
		return _s.GetHashCode();
	}

	public EntityClass Init()
	{
		censorType = 1;
		string text = "";
		if (Properties.Contains(PropCensor))
		{
			text = Properties.GetStringValue(PropCensor);
		}
		if (!string.IsNullOrEmpty(text) && text.Contains(","))
		{
			string[] array = text.Split(",");
			if (array.Length > 1)
			{
				StringParsers.TryParseSInt32(array[0], out censorMode);
				StringParsers.TryParseSInt32(array[1], out censorType);
			}
		}
		else
		{
			Properties.ParseInt(PropCensor, ref censorMode);
		}
		if (!Properties.Values.TryGetValue(PropPrefab, out prefabPath) || prefabPath.Length == 0)
		{
			throw new Exception("Mandatory property 'prefab' missing in entity_class '" + entityClassName + "'");
		}
		bool result = default(bool);
		if (Properties.Values.TryGetValue(PropPrefabCombined, out var _value) && bool.TryParse(_value, out result) && result)
		{
			IsPrefabCombined = true;
		}
		else if (prefabPath[0] == '/')
		{
			prefabPath = prefabPath.Substring(1);
			IsPrefabCombined = true;
		}
		else if (DataLoader.IsInResources(prefabPath))
		{
			prefabPath = "Prefabs/prefabEntity" + prefabPath;
		}
		if (Properties.Values.TryGetValue(PropMesh, out var _value2) && _value2.Length > 0)
		{
			if (censorMode != 0 && (censorType == 1 || censorType == 3) && (bool)GameManager.Instance && GameManager.Instance.IsGoreCensored())
			{
				_value2 = _value2.Replace(".", "_CGore.");
			}
			if (DataLoader.IsInResources(_value2))
			{
				_value2 = "Entities/" + _value2;
			}
			meshPath = _value2;
		}
		if (Properties.Values.ContainsKey(PropMeshFP))
		{
			string text2 = Properties.Values[PropMeshFP];
			if (DataLoader.IsInResources(text2))
			{
				text2 = "Entities/" + text2;
			}
			meshFP = DataLoader.LoadAsset<Transform>(text2);
			if (meshFP == null)
			{
				Log.Error("Could not load file '" + text2 + "' for entity_class '" + entityClassName + "'");
			}
		}
		entityFlags = EntityFlags.None;
		ParseEntityFlags(Properties.GetString(PropEntityFlags), ref entityFlags);
		if (Properties.Values.ContainsKey(PropClass))
		{
			classname = Type.GetType(Properties.Values[PropClass]);
			if (classname == null)
			{
				Log.Error("Could not instantiate class" + Properties.Values[PropClass] + "' for entity_class '" + entityClassName + "'");
			}
		}
		modelType = typeof(EModelCustom);
		string text3 = Properties.GetString(PropModelType);
		if (text3.Length > 0)
		{
			modelType = ReflectionHelpers.GetTypeWithPrefix("EModel", text3);
			if (modelType == null)
			{
				throw new Exception("Model class '" + text3 + "' not found!");
			}
		}
		string text4 = Properties.GetString(PropAltMats);
		if (text4.Length > 0)
		{
			AltMatNames = text4.Split(',');
		}
		string text5 = Properties.GetString(PropSwapMats);
		if (text5.Length > 0)
		{
			MatSwap = text5.Split(",");
		}
		if (Properties.Values.ContainsKey(PropParticleOnSpawn))
		{
			particleOnSpawn.fileName = Properties.Values[PropParticleOnSpawn];
			particleOnSpawn.shapeMesh = Properties.Params1[PropParticleOnSpawn];
			DataLoader.PreloadBundle(particleOnSpawn.fileName);
		}
		RagdollOnDeathChance = 0.5f;
		if (Properties.Values.ContainsKey(PropRagdollOnDeathChance))
		{
			RagdollOnDeathChance = StringParsers.ParseFloat(Properties.Values[PropRagdollOnDeathChance]);
		}
		if (Properties.Values.ContainsKey(PropHasRagdoll))
		{
			HasRagdoll = StringParsers.ParseBool(Properties.Values[PropHasRagdoll]);
		}
		if (Properties.Values.ContainsKey(PropColliders))
		{
			CollidersRagdollAsset = Properties.Values[PropColliders];
			DataLoader.PreloadBundle(CollidersRagdollAsset);
		}
		Properties.ParseFloat(PropLookAtAngle, ref LookAtAngle);
		if (Properties.Values.ContainsKey(PropCrouchYOffsetFP))
		{
			crouchYOffsetFP = StringParsers.ParseFloat(Properties.Values[PropCrouchYOffsetFP]);
		}
		if (Properties.Values.ContainsKey(PropParent))
		{
			parentGameObjectName = Properties.Values[PropParent];
		}
		if (Properties.Values.ContainsKey(PropSkinTexture))
		{
			skinTexture = Properties.Values[PropSkinTexture];
			DataLoader.PreloadBundle(skinTexture);
		}
		bIsEnemyEntity = false;
		if (Properties.Values.ContainsKey(PropIsEnemyEntity))
		{
			bIsEnemyEntity = StringParsers.ParseBool(Properties.Values[PropIsEnemyEntity]);
		}
		bIsAnimalEntity = false;
		if (Properties.Values.ContainsKey(PropIsAnimalEntity))
		{
			bIsAnimalEntity = StringParsers.ParseBool(Properties.Values[PropIsAnimalEntity]);
		}
		CorpseBlockId = null;
		if (Properties.Values.ContainsKey(PropCorpseBlock))
		{
			CorpseBlockId = Properties.Values[PropCorpseBlock];
		}
		CorpseBlockChance = 1f;
		if (Properties.Values.ContainsKey(PropCorpseBlockChance))
		{
			CorpseBlockChance = StringParsers.ParseFloat(Properties.Values[PropCorpseBlockChance]);
		}
		CorpseBlockDensity = MarchingCubes.DensityTerrain;
		if (Properties.Values.ContainsKey(PropCorpseBlockDensity))
		{
			CorpseBlockDensity = int.Parse(Properties.Values[PropCorpseBlockDensity]);
			CorpseBlockDensity = Math.Max(-128, Math.Min(127, CorpseBlockDensity));
		}
		RootMotion = false;
		if (Properties.Values.ContainsKey(PropRootMotion))
		{
			RootMotion = StringParsers.ParseBool(Properties.Values[PropRootMotion]);
		}
		HasDeathAnim = false;
		if (Properties.Values.ContainsKey(PropHasDeathAnim))
		{
			HasDeathAnim = StringParsers.ParseBool(Properties.Values[PropHasDeathAnim]);
		}
		ExperienceValue = 100;
		if (Properties.Values.ContainsKey(PropExperienceGain))
		{
			ExperienceValue = (int)StringParsers.ParseFloat(Properties.Values[PropExperienceGain]);
		}
		string text6 = Properties.GetString(PropLootDropEntityClass);
		if (text6.Length > 0)
		{
			lootDrops = new List<LootDrop>();
			LootDrop item = default(LootDrop);
			if (!text6.Contains(","))
			{
				item.entityClass = FromString(text6);
				item.weight = 1f;
				lootDrops.Add(item);
			}
			else
			{
				string[] array2 = text6.Split(commaSeparator);
				int num = array2.Length / 2;
				for (int i = 0; i < num; i++)
				{
					int num2 = i * 2;
					item.entityClass = FromString(array2[num2].Trim());
					item.weight = float.Parse(array2[num2 + 1]);
					lootDrops.Add(item);
				}
				float num3 = 0f;
				for (int j = 0; j < num; j++)
				{
					num3 += lootDrops[j].weight;
				}
				for (int k = 0; k < num; k++)
				{
					item = lootDrops[k];
					item.weight /= num3;
					lootDrops[k] = item;
				}
			}
		}
		bIsMale = false;
		if (Properties.Values.ContainsKey(PropIsMale))
		{
			bIsMale = StringParsers.ParseBool(Properties.Values[PropIsMale]);
		}
		bIsChunkObserver = false;
		if (Properties.Values.ContainsKey(PropIsChunkObserver))
		{
			bIsChunkObserver = StringParsers.ParseBool(Properties.Values[PropIsChunkObserver]);
		}
		SightRange = Constants.cDefaultMonsterSeeDistance;
		if (Properties.Values.ContainsKey(PropSightRange))
		{
			SightRange = StringParsers.ParseFloat(Properties.Values[PropSightRange]);
		}
		if (Properties.Values.ContainsKey(PropSightLightThreshold))
		{
			sightLightThreshold = StringParsers.ParseMinMaxCount(Properties.Values[PropSightLightThreshold]);
		}
		else
		{
			sightLightThreshold = new Vector2(30f, 100f);
		}
		SleeperNoiseToSense = new Vector2(15f, 15f);
		Properties.ParseVec(PropSleeperNoiseToSense, ref SleeperNoiseToSense);
		SleeperNoiseToSenseSoundChance = 1f;
		Properties.ParseFloat(PropSleeperNoiseToSenseSoundChance, ref SleeperNoiseToSenseSoundChance);
		SleeperNoiseToWake = new Vector2(15f, 15f);
		Properties.ParseVec(PropSleeperNoiseToWake, ref SleeperNoiseToWake);
		SleeperSightToSenseMin = new Vector2(25f, 25f);
		Properties.ParseVec(PropSleeperSightToSenseMin, ref SleeperSightToSenseMin);
		SleeperSightToSenseMax = new Vector2(200f, 200f);
		Properties.ParseVec(PropSleeperSightToSenseMax, ref SleeperSightToSenseMax);
		SleeperSightToWakeMin = new Vector2(15f, 15f);
		Properties.ParseVec(PropSleeperSightToWakeMin, ref SleeperSightToWakeMin);
		SleeperSightToWakeMax = new Vector2(200f, 200f);
		Properties.ParseVec(PropSleeperSightToWakeMax, ref SleeperSightToWakeMax);
		MassKg = 10f;
		if (Properties.Values.ContainsKey(PropMass))
		{
			MassKg = StringParsers.ParseFloat(Properties.Values[PropMass]);
		}
		MassKg *= 0.454f;
		SizeScale = 1f;
		if (Properties.Values.ContainsKey(PropSizeScale))
		{
			SizeScale = StringParsers.ParseFloat(Properties.Values[PropSizeScale]);
		}
		string text7 = Properties.GetString(PropPhysicsBody);
		if (text7.Length > 0)
		{
			PhysicsBody = PhysicsBodyLayout.Find(text7);
		}
		if (Properties.Values.ContainsKey("DeadBodyHitPoints"))
		{
			DeadBodyHitPoints = int.Parse(Properties.Values["DeadBodyHitPoints"]);
		}
		Properties.ParseFloat(PropLegCrippleScale, ref LegCrippleScale);
		Properties.ParseFloat(PropLegCrawlerThreshold, ref LegCrawlerThreshold);
		DismemberMultiplierHead = 1f;
		Properties.ParseFloat(PropDismemberMultiplierHead, ref DismemberMultiplierHead);
		DismemberMultiplierArms = 1f;
		Properties.ParseFloat(PropDismemberMultiplierArms, ref DismemberMultiplierArms);
		DismemberMultiplierLegs = 1f;
		Properties.ParseFloat(PropDismemberMultiplierLegs, ref DismemberMultiplierLegs);
		if (Properties.Values.ContainsKey(PropKnockdownKneelDamageThreshold))
		{
			KnockdownKneelDamageThreshold = StringParsers.ParseFloat(Properties.Values[PropKnockdownKneelDamageThreshold]);
		}
		if (Properties.Values.ContainsKey(PropKnockdownKneelStunDuration))
		{
			KnockdownKneelStunDuration = StringParsers.ParseMinMaxCount(Properties.Values[PropKnockdownKneelStunDuration]);
		}
		if (Properties.Values.ContainsKey(PropKnockdownProneDamageThreshold))
		{
			KnockdownProneDamageThreshold = StringParsers.ParseFloat(Properties.Values[PropKnockdownProneDamageThreshold]);
		}
		if (Properties.Values.ContainsKey(PropKnockdownProneStunDuration))
		{
			KnockdownProneStunDuration = StringParsers.ParseMinMaxCount(Properties.Values[PropKnockdownProneStunDuration]);
		}
		if (Properties.Values.ContainsKey(PropKnockdownKneelRefillRate))
		{
			KnockdownKneelRefillRate = StringParsers.ParseMinMaxCount(Properties.Values[PropKnockdownKneelRefillRate]);
		}
		if (Properties.Values.ContainsKey(PropKnockdownProneRefillRate))
		{
			KnockdownProneRefillRate = StringParsers.ParseMinMaxCount(Properties.Values[PropKnockdownProneRefillRate]);
		}
		LegsExplosionDamageMultiplier = 1f;
		if (Properties.Values.ContainsKey(PropLegsExplosionDamageMultiplier))
		{
			LegsExplosionDamageMultiplier = StringParsers.ParseFloat(Properties.Values[PropLegsExplosionDamageMultiplier]);
		}
		ArmsExplosionDamageMultiplier = 1f;
		if (Properties.Values.ContainsKey(PropArmsExplosionDamageMultiplier))
		{
			ArmsExplosionDamageMultiplier = StringParsers.ParseFloat(Properties.Values[PropArmsExplosionDamageMultiplier]);
		}
		HeadExplosionDamageMultiplier = 1f;
		if (Properties.Values.ContainsKey(PropHeadExplosionDamageMultiplier))
		{
			HeadExplosionDamageMultiplier = StringParsers.ParseFloat(Properties.Values[PropHeadExplosionDamageMultiplier]);
		}
		ChestExplosionDamageMultiplier = 1f;
		if (Properties.Values.ContainsKey(PropChestExplosionDamageMultiplier))
		{
			ChestExplosionDamageMultiplier = StringParsers.ParseFloat(Properties.Values[PropChestExplosionDamageMultiplier]);
		}
		Vector3 optionalValue = Vector3.zero;
		Properties.ParseVec(PropPainResistPerHit, ref optionalValue, 0f);
		PainResistPerHit = optionalValue.x;
		PainResistPerHitLowHealth = optionalValue.y;
		PainResistPerHitLowHealthPercent = optionalValue.z;
		if (Properties.Values.ContainsKey(PropArchetype))
		{
			ArchetypeName = Properties.Values[PropArchetype];
		}
		SwimOffset = 0.9f;
		if (Properties.Values.ContainsKey(PropSwimOffset))
		{
			SwimOffset = StringParsers.ParseFloat(Properties.Values[PropSwimOffset]);
		}
		SearchRadius = 6f;
		Properties.ParseFloat(PropSearchRadius, ref SearchRadius);
		if (Properties.Values.ContainsKey(PropUMARace))
		{
			UMARace = Properties.Values[PropUMARace];
		}
		if (Properties.Values.ContainsKey(PropUMAGeneratedModelName))
		{
			UMAGeneratedModelName = Properties.Values[PropUMAGeneratedModelName];
		}
		if (Properties.Values.ContainsKey(PropModelTransformAdjust))
		{
			ModelTransformAdjust = StringParsers.ParseVector3(Properties.Values[PropModelTransformAdjust]);
		}
		if (Properties.Values.ContainsKey(PropAIPackages))
		{
			AIPackages = Properties.Values[PropAIPackages].Split(',');
			for (int l = 0; l < AIPackages.Length; l++)
			{
				AIPackages[l] = AIPackages[l].Trim();
			}
			UseAIPackages = true;
		}
		if (Properties.Values.ContainsKey(PropBuffs))
		{
			string[] array3 = Properties.Values[PropBuffs].Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			if (array3.Length != 0)
			{
				Buffs = new List<string>(array3);
			}
		}
		if (Properties.Values.ContainsKey(PropMaxTurnSpeed))
		{
			MaxTurnSpeed = StringParsers.ParseFloat(Properties.Values[PropMaxTurnSpeed]);
		}
		if (Properties.Values.ContainsKey(PropTags))
		{
			Tags = FastTags<TagGroup.Global>.Parse(Properties.Values[PropTags]);
		}
		if (Properties.Values.ContainsKey(PropNavObject))
		{
			NavObject = Properties.Values[PropNavObject];
		}
		Properties.ParseVec(PropNavObjectHeadOffset, ref NavObjectHeadOffset);
		explosionData = new ExplosionData(Properties, Effects);
		bool optionalValue2 = false;
		Properties.ParseBool(PropHideInSpawnMenu, ref optionalValue2);
		if (optionalValue2)
		{
			userSpawnType = UserSpawnType.Console;
		}
		Properties.ParseEnum(PropUserSpawnType, ref userSpawnType);
		Properties.ParseBool(PropCanBigHead, ref CanBigHead);
		Properties.ParseInt(PropDanceType, ref DanceTypeID);
		Properties.ParseString(PropOnActivateEvent, ref onActivateEvent);
		return this;
	}

	public void CopyFrom(EntityClass _other, HashSet<string> _exclude = null)
	{
		foreach (KeyValuePair<string, string> item in _other.Properties.Values.Dict)
		{
			if (_exclude == null || !_exclude.Contains(item.Key))
			{
				Properties.Values[item.Key] = _other.Properties.Values[item.Key];
			}
		}
		foreach (KeyValuePair<string, string> item2 in _other.Properties.Params1.Dict)
		{
			if (_exclude == null || !_exclude.Contains(item2.Key))
			{
				Properties.Params1[item2.Key] = item2.Value;
			}
		}
		foreach (KeyValuePair<string, string> item3 in _other.Properties.Params2.Dict)
		{
			if (_exclude == null || !_exclude.Contains(item3.Key))
			{
				Properties.Params2[item3.Key] = item3.Value;
			}
		}
		foreach (KeyValuePair<string, string> item4 in _other.Properties.Data.Dict)
		{
			if (_exclude == null || !_exclude.Contains(item4.Key))
			{
				Properties.Data[item4.Key] = item4.Value;
			}
		}
		foreach (KeyValuePair<string, DynamicProperties> item5 in _other.Properties.Classes.Dict)
		{
			if (_exclude == null || !_exclude.Contains(item5.Key))
			{
				DynamicProperties dynamicProperties = new DynamicProperties();
				dynamicProperties.CopyFrom(item5.Value);
				Properties.Classes[item5.Key] = dynamicProperties;
			}
		}
	}

	public static void ParseEntityFlags(string _names, ref EntityFlags optionalValue)
	{
		if (_names.Length <= 0)
		{
			return;
		}
		EntityFlags _result2;
		if (_names.IndexOf(',') >= 0)
		{
			string[] array = _names.Split(commaSeparator, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				if (EnumUtils.TryParse<EntityFlags>(array[i], out var _result, _ignoreCase: true))
				{
					optionalValue |= _result;
				}
			}
		}
		else if (EnumUtils.TryParse<EntityFlags>(_names, out _result2, _ignoreCase: true))
		{
			optionalValue = _result2;
		}
	}

	public static void Cleanup()
	{
		list.Clear();
	}

	public void AddDroppedId(EnumDropEvent _eEvent, string _name, int _minCount, int _maxCount, float _prob, float _stickChance, string _toolCategory, string _tag)
	{
		List<Block.SItemDropProb> list = (itemsToDrop.ContainsKey(_eEvent) ? itemsToDrop[_eEvent] : null);
		if (list == null)
		{
			list = new List<Block.SItemDropProb>();
			itemsToDrop[_eEvent] = list;
		}
		list.Add(new Block.SItemDropProb(_name, _minCount, _maxCount, _prob, 1f, _stickChance, _toolCategory, _tag));
	}

	public int LootDropPick(GameRandom _rand)
	{
		int index = 0;
		if (lootDrops.Count >= 2)
		{
			float num = 0f;
			float randomFloat = _rand.RandomFloat;
			for (int i = 0; i < lootDrops.Count; i++)
			{
				num += lootDrops[i].weight;
				if (randomFloat <= num)
				{
					index = i;
					break;
				}
			}
		}
		return lootDrops[index].entityClass;
	}
}
