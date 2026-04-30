using System;
using UnityEngine;

public class Constants
{
	public enum EBiomePoiMap : byte
	{
		CityAsphalt = 1,
		CountryRoadAsphalt,
		RoadGravel,
		Sand,
		Free
	}

	public static bool IsWebPlayer = false;

	public static bool Is32BitOs = IntPtr.Size == 4;

	public const int cMinShadowDistance = 20;

	public const string cOptionsIni = "UserOptions.ini";

	public const string cProduct = "7 Days To Die";

	public const string cProductAbbrev = "7DTD";

	public const VersionInformation.EGameReleaseType cReleaseType = VersionInformation.EGameReleaseType.V;

	public const int cVersionMajor = 2;

	public const int cVersionMinor = 5;

	public const int cVersionBuild = 32;

	public static readonly VersionInformation cVersionInformation = new VersionInformation(VersionInformation.EGameReleaseType.V, 2, 5, 32);

	public const int cGameResetRevision = 13;

	public const int cGraphicsResetRevision = 4;

	public const int cControlsResetRevision = 7;

	public const int cBindingsResetRevision = 1;

	public const string cCopyright = "Copyright (c) 2014-2025 The Fun Pimps LLC All Rights Reserved.";

	public const int cMaxMPPlayers = 8;

	public const int cMaxCrossplayMPPlayers = 8;

	public const int cDefaultUserPermissionLevel = 1000;

	public const string cDefaultPlayerName = "Player";

	public const string cDirWorlds = "Data/Worlds";

	public const string cDirPrefabs = "Data/Prefabs";

	public const string cDirBluff = "Data/Bluffs";

	public const string cDirBlocks = "Data/Config";

	public const string cDirGroups = "Data/Config/Groups";

	public const string cDirItems = "Data/Config";

	public const string cDirWorldCreation = "Data/Config";

	public const string cDirConfig = "Data/Config";

	public const string cDirHeightmaps = "Data/Heightmaps";

	public const string cDirBackgroundImage = "Data/Textures/misc/background.jpg";

	public const string cDirAssetBundles = "Data/Bundles/Standalone";

	public const string cDirPrefabParts = "Data/Prefabs/Parts";

	public const string cFolderResourcesSounds = "Sounds";

	public const string cFolderResourcesConfig = "Data/Config";

	public const string cFolderResourcesTextures = "Textures";

	public const string cFolderResourcesEnvironment = "Textures/Environment";

	public const string cFolderResourcesTerrainTextures = "Textures/Terrain";

	public const string cFolderResourcesLocalizationEnglish = "GUI/Localization/English";

	public const string cFolderSaveGame = "Saves";

	public const string cFolderSaveGameLocal = "SavesLocal";

	public const string cFolderSaveRegion = "Region";

	public const string cFolderSavePlayer = "Player";

	public const string cFolderGeneratedWorlds = "GeneratedWorlds";

	public const string cFolderLocalPrefabs = "LocalPrefabs";

	public const string cFolderSaveTwitch = "Twitch";

	public const string cDirConfigInternal = "DataInternal/Config";

	public const string cExtLevels = ".ttw";

	public const string cExtPrefabs = ".tts";

	public const string cExtPrefabImposters = ".mesh";

	public const string cExtIdNameMappings = ".nim";

	public const string cExtPlayedLevel = ".played";

	public const string cExtSdf = ".sdf";

	public const string cExtFlag = ".flag";

	public const string cFileMainTTW = "main.ttw";

	public const string cFileWorldChecksums = "checksums.txt";

	public static readonly string cFileBlockMappings = "blockmappings.nim";

	public static readonly string cFileItemMappings = "itemmappings.nim";

	public const string cFileDecos = "decoration.7dt";

	public const string cFileMultiBlocks = "multiblocks.7dt";

	public const string cGameNameDefault = "Region";

	public const string cSdfFileName = "gameOptions.sdf";

	public const string cPersistentPlayersFileName = "players.xml";

	public const string cAchievementFilename = "achievements.bin";

	public const string cNewGameSdfFileName = "newGameOptions.sdf";

	public const string cFileArchivedFlag = "archived.flag";

	public static string cPrefixAtlas = "ta_";

	public const string cLevelPrefab = "prefabs";

	public const string cSpawnPoints = "spawnpoints";

	public const string cExtTexturePack = ".xml";

	public static string cArgDedicatedServer = "-dedicated";

	public static string cArgSubmissionBuild = "-submission";

	public const int cMaxBiomes = 50;

	public const string cTagLargeEntityBlocker = "LargeEntityBlocker";

	public const string cTagPhysics = "Physics";

	public const int cLayerDefault = 0;

	public const int cLayerTransparentFx = 1;

	public const int cLayerIgnoreRaycast = 2;

	public const int cLayerWater = 4;

	public const int cLayerNoShadow = 8;

	public const int cLayerBackgroundImage = 9;

	public const int cLayerHoldingItem = 10;

	public const int cLayerRenderInTexture = 11;

	public const int cLayerNGUI = 12;

	public const int cLayerItems = 13;

	public const int cLayerNoCollision = 14;

	public const int cLayerCCPhysics = 15;

	public const int cLayerTerrainCollision = 16;

	public const int cLayerPhysicsDead = 17;

	public const int cLayerGrass = 18;

	public const int cLayerLargeEntityBlocker = 19;

	public const int cLayerLocalCCPhysics = 20;

	public const int cLayerPhysics = 21;

	public const int cLayerUnderwaterEffects = 22;

	public const int cLayerTrees = 23;

	public const int cLayerLocalPlayer = 24;

	public const int cLayerPlayerRagdollsOLD = 27;

	public const int cLayerTerrain = 28;

	public const int cLayerWires = 29;

	public const int cLayerGlass = 30;

	public const int cLayerVolumes = 31;

	public const int cLayerMaskItems = 8192;

	public const int cLayerMaskIgnoreRayCast = 538480644;

	public const int cLayerMaskGrass = 262144;

	public const int cLayerMaskWater = 16;

	public const int cLayerMaskLocalPlayer = 17825792;

	public const int cLayerMaskAllLayers = -538480645;

	public const int cLayerMaskNoItems = -538488837;

	public const int cLayerMaskOnlyItemsAndCollision = 73728;

	public const int cLayerMaskNoItemsNoGrass = -538750981;

	public const int cLayerMaskNoItemsNoGrassNoWater = -538750997;

	public const int cLayerMaskNoItemsNoLocalPlayer = -555266053;

	public const int cLayerMaskAttackingBlocksMask = 1073807360;

	public const int cLayerMaskSight = -1612492821;

	public static int cDistanceRandomDisplayUpdates = 30;

	public static float cRunningFOVMultiplier = 1.05f;

	public static float cRunningFOVSpeedDown = 3f;

	public static float cRunningFOVSpeedUp = 1f;

	public static int cDefaultCameraFieldOfView = 65;

	public static int cMinCameraFieldOfView = 50;

	public static int cMaxCameraFieldOfView = 85;

	public static readonly Vector3 cDefaultCameraPlayerOffset = new Vector3(0f, 1.6f, 0f);

	public const int cMaxVertices = 786432;

	public static float cMinGlobalBackgroundOpacity = 0.55f;

	public static float cMinGlobalForegroundOpacity = 0.75f;

	public const int cTicksPerSecond = 20;

	public const float cTickDuration = 0.05f;

	public const float cPhysicsTicksPerSecond = 50f;

	public const float cPhysicsTickDuration = 0.02f;

	public static float cDefaultDistortionFactor = 1f;

	public static int cMaxEntitiesPerMobSpawner = 8;

	public const byte cMaxLightValue = 15;

	public static float cSizePlanesAround = 250f;

	public static int cDefaultPort = 26900;

	public static int cLevelServerPort = 6789;

	public static int cRandomSpawnPointsToPlace = 4;

	public static int cStartTeamTickets = 0;

	public static int cTeamTicketsAlarm = 10;

	public static float cDecreaseOneTicketTime = 5f;

	public static float cTimeGameOverButWaitSeconds = 5f;

	public static float cDigAndBuildDistance = 4f;

	public static float cCollectItemDistance = 2f;

	public static float cRespawnAfterDeathTime = 3f;

	public static float cRespawnEnterGameTime = 0f;

	public static float cRespawnAfterFallenDown = 3f;

	public static float cHitColorDuration = 0.15f;

	public static float cItemDroppedOnDeathLifetime = 300f;

	public const float cItemDroppedLifetime = 60f;

	public static float cItemExplosionLifetime = 30f;

	public static float cItemHealthDroppedLifetime = 60f;

	public static float cItemSpawnPointLifetime = float.MaxValue;

	public static float cItemPortalLifetime = float.MaxValue;

	public static float cItemItemSpawnerLifetime = float.MaxValue;

	public static int cHardenBrickTime = 20;

	public const int cItemQualityTierVariations = 1;

	public const ushort cItemMaxQuality = 6;

	public static float cMinHolsterTime = 0.1f;

	public static float cMinUnHolsterTime = 0.1f;

	public static float cEnergyJetpackPerBlock = 10f;

	public static int cHealthPotionAdd = 30;

	public static int cMaxPlayerFood = 100;

	public static int cFoodOversaturate = 100;

	public static int cMaxPlayerDrink = 100;

	public static int cDrinkOversaturate = 100;

	public static int cItemDropCountWhenDead = 3;

	public static float cBuildIntervall = 0.5f;

	public static float cSendWorldTickTimeToClients = 1.5f;

	public static float cCheckGameState = 0.5f;

	public static float cSneakDamageMultiplier = 2f;

	public static int cNumberOfTeams = 2;

	public static Color[] cTeamColors = new Color[3]
	{
		Color.white,
		new Color(0f, 0.8f, 1f),
		Color.red
	};

	public static string[] cTeamName = new string[3] { "No", "BLUE", "RED" };

	public static string[] cTeamSkinName = new string[3] { "Soldier Blue", "Soldier Blue", "Soldier Red" };

	public static float cDarkAtNightSubtraction = 12f;

	public static float cDefaultMonsterSeeDistance = 48f;

	public static float cPlayerSpeedModifierRunning = 1.6f;

	public static float cPlayerSpeedModifierWalking = 0.8f;

	public static float cPlayerSpeedModifierCrouching = 0.4f;

	public static int cPosInventoryYSub = 70;

	public static int cPosMinimapY = 10;

	public static int cPosMinimapSubRight = 20;

	public static Color cColorBlood = new Color(0.8f, 0f, 0.08f);

	public static Color cColorBorderBox = new Color(0.8f, 0f, 0f, 0.5f);

	public static Vector3 cStartPositionPlayerInLevel = new Vector3(0f, 200f, 0f);

	public static Vector3 cStartRotationPlayerInLevel = new Vector3(0f, 0f, 0f);

	public static BlockValue cTerrainBlockValue = new BlockValue(1u);

	public static string cTerrainFillerBlockName = "terrainFiller";

	public static string cTerrainFiller2BlockName = "terrainFillerAdaptive";

	public static string cPOIFillerBlock = "poiFillerBlock";

	public static string cQuestLootFetchContainerIndexName = "FetchContainer";

	public static string cQuestRestorePowerIndexName = "QuestRestorePower";

	public static Color[] TrackedFriendColors = new Color[8]
	{
		Color.green,
		Color.blue,
		Color.yellow,
		new Color(1f, 0f, 1f),
		new Color(0.5f, 0.25f, 0f),
		new Color(1f, 0.5f, 0f),
		new Color32(56, 35, 16, byte.MaxValue),
		new Color32(42, 59, 0, byte.MaxValue)
	};

	public const int cMaxViewDistanceOptions = 7;

	public const int cMinViewDistance = 4;

	public const int cMaxViewDistance = 12;

	public const float cBlockDamageLosesPaint = 1f;

	public static int cEnemySenseMemory = 60;

	public const int ChunkCompressionLevel = 3;

	public const int NetworkCompressionLevel = 3;

	public const string cSpecialWorldName_Empty = "Empty";

	public const string cSpecialWorldName_Playtesting = "Playtesting";

	public const string cSpecialWorldName_Navezgane = "Navezgane";

	public const float cMouseSensitivityMin = 0.05f;

	public const float cMouseSensitivityRange = 1.45f;

	public const float cMouseSensitivityMax = 1.5f;

	public const float cControllerSensitivityMin = 0.05f;

	public const float cControllerSensitivityMax = 1f;

	public const float cControllerModifierSensitivityMax = 2f;

	public const int cPlayTestingSpawnOffset = 10;

	public const float cAimAssistMaxDistance = 50f;

	public const float cAimAssistSlowDownEntity = 0.5f;

	public const float cAimAssistSlowDownItem = 0.6f;

	public const float cAimAssistSlowDownItemDistance = 10f;

	public const float cAimAssistSlowThreatLevelThreshold = 0.75f;

	public const float cAimAssistSnapScreenDistance = 0.15f;

	public const float cCameraSnapTime = 0.3f;

	public const float cAimAssistSnapMaximumAngle = 15f;

	public const float cAimAssistZoomSnapSpeed = 1f;

	public const float cAimAssistMeleeSnapAngle = 20f;

	public const float cAimAssistMeleeSnapSpeed = 1.5f;

	public const float cAimAssistMeleeHitSnapAngle = 30f;

	public const float cRapidTriggerFireDelay = 0.25f;

	public const float cMapViewControllerSpeed = 500f;

	public const float cRunToggleHoldTime = 0.2f;

	public const float cRecoveryPositionAttemptTime = 30f;

	public const float cRecoveryPositionMinSqrMagnitude = 10000f;

	public const int cMaxRecoveryPositions = 5;

	public const float cCursorSensitivityMin = 0.1f;

	public const float cCursorSensitivityMax = 1f;

	public const int cNavWorldSizeX = 6144;

	public const int cNavWorldSizeZ = 6144;

	public const float cPartyActivationRange = 15f;

	public const int cMaxPartySize = 8;

	public const int cDummyWaterTexId = 5000;

	public static int cMaxLoadTimePixelsPerTest = 4096;

	public static int cMaxLoadTimePerFrameMillis = 50;

	public const float cInputRepeatDelay = 0.1f;

	public const float cInputInitialRepeatDelay = 0.35f;

	public const int cConsoleMaxPersistentPlayerDataEntries = 100;

	public const int cVirtualKeyboardDefaultCharacterLimit = 200;
}
