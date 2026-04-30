using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Challenges;
using DynamicMusic.Legacy.ObjectModel;
using Noemax.GZip;
using Platform;
using Twitch;
using UnityEngine;
using WorldGenerationEngineFinal;
using XMLData.Item;

public static class WorldStaticData
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum EClientFileState
	{
		None,
		Received,
		LoadLocal
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class XmlLoadInfo
	{
		public readonly string XmlName;

		public readonly string LoadStepLocalizationKey;

		public readonly bool LoadAtStartup;

		public readonly bool SendToClients;

		public readonly bool IgnoreMissingFile;

		public readonly bool AllowReloadDuringGame;

		public readonly Func<XmlFile, IEnumerator> LoadMethod;

		public readonly Action CleanupMethod;

		public readonly Func<IEnumerator> ExecuteAfterLoad;

		public readonly Action<XmlFile> ReloadDuringGameMethod;

		public byte[] CompressedXmlData;

		public bool LoadClientFile;

		public EClientFileState WasReceivedFromServer;

		public bool XmlFileExists()
		{
			return SdFile.Exists(GameIO.GetGameDir("Data/Config") + "/" + XmlName + ".xml");
		}

		public XmlLoadInfo(string _xmlName, bool _loadAtStartup, bool _sendToClients, Func<XmlFile, IEnumerator> _loadMethod, Action _cleanupMethod, Func<IEnumerator> _executeAfterLoad = null, bool _allowReloadDuringGame = false, Action<XmlFile> _reloadDuringGameMethod = null, bool _ignoreMissingFile = false, string _loadStepLocalizationKey = null)
		{
			XmlName = _xmlName;
			LoadStepLocalizationKey = _loadStepLocalizationKey;
			LoadAtStartup = _loadAtStartup;
			SendToClients = _sendToClients;
			IgnoreMissingFile = _ignoreMissingFile;
			AllowReloadDuringGame = _allowReloadDuringGame;
			LoadMethod = _loadMethod;
			CleanupMethod = _cleanupMethod;
			ExecuteAfterLoad = _executeAfterLoad;
			ReloadDuringGameMethod = _reloadDuringGameMethod;
			if (LoadMethod == null)
			{
				throw new ArgumentNullException("_loadMethod");
			}
		}
	}

	public delegate void ProgressDelegate(string _progressText, float _percentage);

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cExplosionPrefabMax = 100;

	public static bool LoadAllXmlsCoComplete;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly XmlLoadInfo[] xmlsToLoad = new XmlLoadInfo[47]
	{
		new XmlLoadInfo("events", _loadAtStartup: true, _sendToClients: true, EventsFromXml.Load, EventsFromXml.Cleanup),
		new XmlLoadInfo("materials", _loadAtStartup: false, _sendToClients: true, LoadMaterials, MaterialBlock.Cleanup, LoadTextureAtlases, _allowReloadDuringGame: false, null, _ignoreMissingFile: false, "loadActionMaterials"),
		new XmlLoadInfo("physicsbodies", _loadAtStartup: false, _sendToClients: true, PhysicsBodiesFromXml.Load, PhysicsBodyLayout.Reset),
		new XmlLoadInfo("painting", _loadAtStartup: false, _sendToClients: true, BlockTexturesFromXML.CreateBlockTextures, BlockTextureData.Cleanup),
		new XmlLoadInfo("shapes", _loadAtStartup: false, _sendToClients: true, ShapesFromXml.LoadShapes, null, null, _allowReloadDuringGame: true),
		new XmlLoadInfo("blocks", _loadAtStartup: false, _sendToClients: true, LoadBlocks, CleanupBlocks, null, _allowReloadDuringGame: true, null, _ignoreMissingFile: false, "loadActionBlocks"),
		new XmlLoadInfo("progression", _loadAtStartup: false, _sendToClients: true, ProgressionFromXml.Load, Progression.Cleanup, null, _allowReloadDuringGame: true),
		new XmlLoadInfo("buffs", _loadAtStartup: false, _sendToClients: true, BuffsFromXml.CreateBuffs, BuffManager.Cleanup, null, _allowReloadDuringGame: true, BuffsFromXml.Reload),
		new XmlLoadInfo("misc", _loadAtStartup: false, _sendToClients: true, LoadMisc, AnimationDelayData.Cleanup, null, _allowReloadDuringGame: true, ReloadMisc),
		new XmlLoadInfo("items", _loadAtStartup: false, _sendToClients: true, LoadItems, ItemClass.Cleanup, null, _allowReloadDuringGame: true, ReloadItems, _ignoreMissingFile: false, "loadActionItems"),
		new XmlLoadInfo("item_modifiers", _loadAtStartup: false, _sendToClients: true, LoadItemModifiers, null, LateInitItems, _allowReloadDuringGame: true, ReloadItemModifiers),
		new XmlLoadInfo("entityclasses", _loadAtStartup: false, _sendToClients: true, EntityClassesFromXml.LoadEntityClasses, EntityClass.Cleanup),
		new XmlLoadInfo("qualityinfo", _loadAtStartup: false, _sendToClients: true, QualityInfoFromXml.CreateQualityInfo, QualityInfo.Cleanup),
		new XmlLoadInfo("sounds", _loadAtStartup: false, _sendToClients: true, SoundsFromXml.CreateSounds, null),
		new XmlLoadInfo("recipes", _loadAtStartup: false, _sendToClients: true, LoadRecipes, CraftingManager.ClearAllRecipes, null, _allowReloadDuringGame: true, ReloadRecipes),
		new XmlLoadInfo("blockplaceholders", _loadAtStartup: false, _sendToClients: true, BlockPlaceholdersFromXml.Load, BlockPlaceholderMap.Cleanup),
		new XmlLoadInfo("loot", _loadAtStartup: false, _sendToClients: true, LoadLoot, LootContainer.Cleanup, null, _allowReloadDuringGame: true, ReloadLoot),
		new XmlLoadInfo("entitygroups", _loadAtStartup: false, _sendToClients: true, EntityGroupsFromXml.LoadEntityGroups, EntityGroups.Cleanup),
		new XmlLoadInfo("utilityai", _loadAtStartup: false, _sendToClients: true, UAIFromXml.Load, UAIFromXml.Cleanup),
		new XmlLoadInfo("vehicles", _loadAtStartup: false, _sendToClients: true, VehiclesFromXml.Load, Vehicle.Cleanup, null, _allowReloadDuringGame: true, VehiclesFromXml.Reload),
		new XmlLoadInfo("rwgmixer", _loadAtStartup: true, _sendToClients: false, WorldGenerationFromXml.Load, WorldGenerationFromXml.Cleanup),
		new XmlLoadInfo("weathersurvival", _loadAtStartup: false, _sendToClients: true, LoadWeather, null),
		new XmlLoadInfo("archetypes", _loadAtStartup: true, _sendToClients: true, LoadSDCSArchetypes, null, null, _allowReloadDuringGame: false, null, _ignoreMissingFile: true),
		new XmlLoadInfo("challenges", _loadAtStartup: false, _sendToClients: true, ChallengesFromXml.CreateChallenges, CleanupChallenges, null, _allowReloadDuringGame: true),
		new XmlLoadInfo("quests", _loadAtStartup: false, _sendToClients: true, QuestsFromXml.CreateQuests, null),
		new XmlLoadInfo("traders", _loadAtStartup: false, _sendToClients: true, LoadTraders, TraderInfo.Cleanup, null, _allowReloadDuringGame: true),
		new XmlLoadInfo("npc", _loadAtStartup: false, _sendToClients: true, LoadNpc, null),
		new XmlLoadInfo("dialogs", _loadAtStartup: false, _sendToClients: true, DialogFromXml.Load, Dialog.Cleanup),
		new XmlLoadInfo("ui_display", _loadAtStartup: false, _sendToClients: true, LoadUIDisplayInfo, UIDisplayInfoManager.Reset, null, _allowReloadDuringGame: true),
		new XmlLoadInfo("nav_objects", _loadAtStartup: false, _sendToClients: true, NavObjectClassesFromXml.Load, NavObjectClass.Reset, null, _allowReloadDuringGame: true),
		new XmlLoadInfo("gamestages", _loadAtStartup: false, _sendToClients: false, GameStagesFromXml.Load, CleanupGamestages),
		new XmlLoadInfo("gameevents", _loadAtStartup: false, _sendToClients: true, GameEventsFromXml.CreateGameEvents, CleanupGameEvents, null, _allowReloadDuringGame: true),
		new XmlLoadInfo("twitch", _loadAtStartup: false, _sendToClients: true, TwitchActionsFromXml.CreateTwitchActions, CleanupTwitch, null, _allowReloadDuringGame: true),
		new XmlLoadInfo("twitch_events", _loadAtStartup: false, _sendToClients: true, TwitchActionsFromXml.CreateTwitchEvents, CleanupTwitchEvents, null, _allowReloadDuringGame: true)
		{
			LoadClientFile = true
		},
		new XmlLoadInfo("dmscontent", _loadAtStartup: false, _sendToClients: true, DMSContentFromXml.Load, null),
		new XmlLoadInfo("XUi_Common/styles", _loadAtStartup: false, _sendToClients: true, XUiFromXml.Load, null),
		new XmlLoadInfo("XUi_Common/controls", _loadAtStartup: false, _sendToClients: true, XUiFromXml.Load, null),
		new XmlLoadInfo("XUi/styles", _loadAtStartup: false, _sendToClients: true, XUiFromXml.Load, null),
		new XmlLoadInfo("XUi/controls", _loadAtStartup: false, _sendToClients: true, XUiFromXml.Load, null),
		new XmlLoadInfo("XUi/windows", _loadAtStartup: false, _sendToClients: true, XUiFromXml.Load, null),
		new XmlLoadInfo("XUi/xui", _loadAtStartup: false, _sendToClients: true, XUiFromXml.Load, null),
		new XmlLoadInfo("biomes", _loadAtStartup: false, _sendToClients: true, LoadBiomes, WorldBiomes.CleanupStatic),
		new XmlLoadInfo("worldglobal", _loadAtStartup: false, _sendToClients: true, WorldGlobalFromXml.Load, null, null, _allowReloadDuringGame: true, WorldGlobalFromXml.Reload),
		new XmlLoadInfo("spawning", _loadAtStartup: false, _sendToClients: false, LoadSpawning, CleanupSpawning),
		new XmlLoadInfo("loadingscreen", _loadAtStartup: true, _sendToClients: false, XUiC_LoadingScreen.LoadXml, null, null, _allowReloadDuringGame: true),
		new XmlLoadInfo("subtitles", _loadAtStartup: true, _sendToClients: false, SoundsFromXml.LoadSubtitleXML, null),
		new XmlLoadInfo("videos", _loadAtStartup: true, _sendToClients: false, VideoFromXML.CreateVideos, null)
	};

	public static Transform[] prefabExplosions;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool bInitDone;

	[PublicizedFrom(EAccessModifier.Private)]
	public static MeshDescriptionCollection meshDescCol;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isDediServer;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Coroutine receivedConfigsHandlerCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int highestReceivedIndex = -1;

	public static void InitSync(bool _bForce, bool _bDediServer, bool _cleanup = true)
	{
		if (_cleanup)
		{
			Cleanup();
		}
		LoadManager.InitSync();
		ThreadManager.RunCoroutineSync(Init(_bForce, _bDediServer, null));
	}

	public static IEnumerator Init(bool _bForce, bool _bDediServer, ProgressDelegate _progressDelegate)
	{
		isDediServer = _bDediServer;
		if (!_bForce && bInitDone)
		{
			Log.Out("WorldStaticData.Init() was already done");
			yield break;
		}
		MicroStopwatch sw = new MicroStopwatch();
		_progressDelegate?.Invoke(Localization.Get("loadActionParticles"), 0f);
		yield return null;
		yield return null;
		yield return ParticleEffect.LoadResources();
		List<LoadManager.AssetRequestTask<GameObject>> explosionPrefabLoadTasks = new List<LoadManager.AssetRequestTask<GameObject>>();
		for (int i = 0; i < 100; i++)
		{
			explosionPrefabLoadTasks.Add(LoadManager.LoadAssetFromAddressables<GameObject>($"Prefabs/prefabExplosion{i}.prefab"));
			yield return null;
		}
		yield return LoadManager.WaitAll(explosionPrefabLoadTasks);
		prefabExplosions = new Transform[100];
		for (int j = 0; j < 100; j++)
		{
			LoadManager.AssetRequestTask<GameObject> assetRequestTask = explosionPrefabLoadTasks[j];
			if ((bool)assetRequestTask.Asset)
			{
				prefabExplosions[j] = assetRequestTask.Asset.transform;
			}
		}
		_progressDelegate?.Invoke(Localization.Get("loadActionBlockTextures"), 0f);
		yield return null;
		yield return null;
		if (!meshDescCol)
		{
			bool loadSync = !Application.isPlaying;
			LoadManager.AssetRequestTask<GameObject> request = LoadManager.LoadAssetFromAddressables<GameObject>("BlockTextureAtlases", "prefabMeshDescriptionCollection.prefab", null, null, _deferLoading: false, loadSync);
			yield return request;
			GameObject asset = request.Asset;
			if (asset == null)
			{
				throw new Exception("Missing resource bundle BlockTextureAtlases");
			}
			meshDescCol = asset.GetComponent<MeshDescriptionCollection>();
			meshDescCol.Init();
			bool coroutineHadException = false;
			yield return ThreadManager.CoroutineWrapperWithExceptionCallback(meshDescCol.LoadTextureArraysForQuality(), [PublicizedFrom(EAccessModifier.Internal)] (Exception _exception) =>
			{
				Log.Error("WSD.Init: Initializing MDC failed");
				Log.Exception(_exception);
				coroutineHadException = true;
			});
			if (coroutineHadException)
			{
				yield break;
			}
		}
		if (GameManager.IsDedicatedServer)
		{
			for (int num = 0; num < meshDescCol.meshes.Length; num++)
			{
				meshDescCol.meshes[num].TexDiffuse = null;
				meshDescCol.meshes[num].TexNormal = null;
				meshDescCol.meshes[num].TexSpecular = null;
				meshDescCol.meshes[num].TexEmission = null;
				meshDescCol.meshes[num].TexHeight = null;
				meshDescCol.meshes[num].TexOcclusion = null;
				meshDescCol.meshes[num].TexMask = null;
				meshDescCol.meshes[num].TexMaskNormal = null;
			}
		}
		yield return LoadAllXmlsCo(!_bForce, _progressDelegate);
		bInitDone = true;
		Log.Out($"WorldStaticData.Init() needed {sw.ElapsedMilliseconds / 1000:F3}s");
		GameOptionsManager.TextureQualityChanged += [PublicizedFrom(EAccessModifier.Internal)] (int _obj) =>
		{
			ThreadManager.RunCoroutineSync(meshDescCol.LoadTextureArraysForQuality(_isReload: true));
		};
		GameOptionsManager.TextureFilterChanged += [PublicizedFrom(EAccessModifier.Internal)] (int _obj) =>
		{
			meshDescCol.SetTextureArraysFilter();
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadTextureAtlases()
	{
		MeshDescription[] meshes = meshDescCol.Meshes;
		MeshDescription.meshes = new MeshDescription[meshes.Length];
		for (int i = 0; i < meshes.Length; i++)
		{
			MeshDescription.meshes[i] = meshes[i];
			string textureAtlasClass = meshes[i].TextureAtlasClass;
			Type type = Type.GetType(textureAtlasClass);
			if (type == null)
			{
				Log.Error($"Could not find type '{textureAtlasClass}' for texture atlas on mesh layer {i}");
				yield break;
			}
			TextureAtlas textureAtlas = (TextureAtlas)Activator.CreateInstance(type);
			textureAtlas.LoadTextureAtlas(i, meshDescCol, !isDediServer);
			yield return MeshDescription.meshes[i].Init(i, textureAtlas);
			yield return null;
		}
		MeshDescription.SetGrassQuality();
		MeshDescription.SetWaterQuality();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadMaterials(XmlFile _xmlFile)
	{
		AIDirectorData.InitStatic();
		yield return MaterialsFromXml.CreateMaterials(_xmlFile);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadBlocks(XmlFile _xmlFile)
	{
		Block.InitStatic();
		yield return BlocksFromXml.CreateBlocks(_xmlFile, _fillLookupTable: false, GameManager.Instance != null && GameManager.Instance.IsEditMode());
		Block.AssignIds();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadMisc(XmlFile _xmlFile)
	{
		AnimationDelayData.InitStatic();
		AnimationGunjointOffsetData.InitStatic();
		yield return MiscFromXml.Create(_xmlFile);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ReloadMisc(XmlFile _xmlFile)
	{
		AnimationDelayData.InitStatic();
		AnimationGunjointOffsetData.InitStatic();
		ThreadManager.RunCoroutineSync(MiscFromXml.Create(_xmlFile));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ReloadItems(XmlFile _xmlFile)
	{
		ItemClass.Cleanup();
		ThreadManager.RunCoroutineSync(LoadItems(_xmlFile));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ReloadItemModifiers(XmlFile _xmlFile)
	{
		ThreadManager.RunCoroutineSync(LoadItemModifiers(_xmlFile));
		ThreadManager.RunCoroutineSync(LateInitItems());
		if (GameManager.Instance != null && GameManager.Instance.World != null && GameManager.Instance.World.GetPrimaryPlayer() != null)
		{
			GameManager.Instance.World.GetPrimaryPlayer().inventory.ForceHoldingItemUpdate();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadItems(XmlFile _xmlFile)
	{
		CraftingManager.ClearLockedData();
		Equipment.SetupCosmeticMapping();
		ItemClass.InitStatic();
		ItemClassesFromXml.CreateItemsFromBlocks();
		yield return null;
		yield return ItemClassesFromXml.CreateItems(_xmlFile);
		ItemData.Parser.idOffset = Block.ItemsStartHere;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LateInitItems()
	{
		ItemClass.AssignIds();
		Block.LateInitAll();
		ItemClass.LateInitAll();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadItemModifiers(XmlFile _xmlFile)
	{
		yield return ItemModificationsFromXml.Load(_xmlFile);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadRecipes(XmlFile _xmlFile)
	{
		yield return RecipesFromXml.LoadRecipies(_xmlFile);
		List<Recipe> allRecipes = CraftingManager.GetAllRecipes();
		Dictionary<string, List<Recipe>> recipesByName = new Dictionary<string, List<Recipe>>();
		foreach (Recipe item in allRecipes)
		{
			string name = item.GetName();
			if (!recipesByName.TryGetValue(name, out var value))
			{
				value = (recipesByName[name] = new List<Recipe>());
			}
			value.Add(item);
		}
		MicroStopwatch msw = new MicroStopwatch(_bStart: true);
		List<string> recipeCalcStack = new List<string>();
		ItemClass[] list2 = ItemClass.list;
		foreach (ItemClass itemClass in list2)
		{
			if (itemClass != null)
			{
				recipeCalcStack.Clear();
				itemClass.AutoCalcWeight(recipesByName);
				if (itemClass.AutoCalcEcoVal(recipesByName, recipeCalcStack) < 0f)
				{
					Log.Warning("Loading recipes: Could not calculate eco value for item " + itemClass.GetItemName() + ": Only recursive recipes found");
				}
				if (recipeCalcStack.Count > 0)
				{
					Log.Warning("Loading recipes: Eco value calculation stack not empty for item " + itemClass.GetItemName() + ": " + string.Join(" > ", recipeCalcStack));
				}
				if (msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					msw.ResetAndRestart();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ReloadRecipes(XmlFile _xmlFile)
	{
		CraftingManager.ClearAllRecipes();
		ThreadManager.RunCoroutineSync(LoadRecipes(_xmlFile));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadLoot(XmlFile _xmlFile)
	{
		LootContainer.InitStatic();
		yield return LootFromXml.LoadLootContainers(_xmlFile);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ReloadLoot(XmlFile _xmlFile)
	{
		LootContainer.Cleanup();
		ThreadManager.RunCoroutineSync(LoadLoot(_xmlFile));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadWeather(XmlFile _xmlFile)
	{
		WeatherSurvivalParametersFromXml.Load(_xmlFile);
		LinkBuffs();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadSDCSArchetypes(XmlFile _xmlFile)
	{
		SDCSArchetypesFromXml.Load(_xmlFile);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadTraders(XmlFile _xmlFile)
	{
		TraderInfo.InitStatic();
		yield return TradersFromXml.LoadTraderInfo(_xmlFile);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadNpc(XmlFile _xmlFile)
	{
		NPCInfo.InitStatic();
		yield return NPCsFromXml.LoadNPCInfo(_xmlFile);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadMusic(XmlFile _xmlFile)
	{
		MusicGroup.InitStatic();
		yield return MusicDataFromXml.Load(_xmlFile);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadUIDisplayInfo(XmlFile _xmlFile)
	{
		UIDisplayInfoManager.Reset();
		yield return UIDisplayInfoFromXml.Load(_xmlFile);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadBiomes(XmlFile _xmlFile)
	{
		new WorldBiomes(_xmlFile.XmlDoc, _instantiateReferences: true);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadSpawning(XmlFile _xmlFile)
	{
		EntitySpawnerClassesFromXml.LoadEntitySpawnerClasses(_xmlFile.XmlDoc);
		yield return BiomeSpawningFromXml.Load(_xmlFile);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CleanupBlocks()
	{
		AIDirectorData.Cleanup();
		Block.Cleanup();
		TileEntityCompositeData.Cleanup();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CleanupGamestages()
	{
		GameStageDefinition.Clear();
		GameStageGroup.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CleanupChallenges()
	{
		ChallengeClass.Cleanup();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CleanupGameEvents()
	{
		if (GameEventManager.HasInstance)
		{
			GameEventManager.Current.Cleanup();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CleanupTwitch()
	{
		if (TwitchActionManager.HasInstance)
		{
			TwitchActionManager.Current.Cleanup();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CleanupTwitchEvents()
	{
		if (TwitchManager.HasInstance)
		{
			TwitchManager.Current.CleanupEventData();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CleanupSpawning()
	{
		EntitySpawnerClass.Cleanup();
		BiomeSpawningClass.Cleanup();
	}

	public static void LoadPhysicsBodies()
	{
		Reset("physicsbodies");
	}

	public static void SavePhysicsBodies()
	{
		PhysicsBodiesFromXml.Save(GameIO.GetGameDir("Data/Config") + "/physicsbodies.xml");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LinkBuffs()
	{
	}

	public static void Cleanup(string _xmlNameContaining)
	{
		XmlLoadInfo[] array = xmlsToLoad;
		foreach (XmlLoadInfo xmlLoadInfo in array)
		{
			if (string.IsNullOrEmpty(_xmlNameContaining) || xmlLoadInfo.XmlName.ContainsCaseInsensitive(_xmlNameContaining))
			{
				xmlLoadInfo.CleanupMethod?.Invoke();
			}
		}
	}

	public static void Cleanup()
	{
		Cleanup(null);
		if ((bool)meshDescCol)
		{
			meshDescCol.Cleanup();
			meshDescCol = null;
		}
		MeshDescription.Cleanup();
		AssetBundles.Cleanup();
		bInitDone = false;
	}

	public static void QuitCleanup()
	{
	}

	public static void Reset(string _xmlNameContaining)
	{
		Cleanup(_xmlNameContaining);
		MemoryStream memoryStream = new MemoryStream();
		DeflateOutputStream zipStream = new DeflateOutputStream(memoryStream, 3);
		XmlLoadInfo[] array = xmlsToLoad;
		foreach (XmlLoadInfo xmlLoadInfo in array)
		{
			if (!string.IsNullOrEmpty(_xmlNameContaining) && !xmlLoadInfo.XmlName.ContainsCaseInsensitive(_xmlNameContaining))
			{
				continue;
			}
			if (!xmlLoadInfo.XmlFileExists())
			{
				if (!xmlLoadInfo.IgnoreMissingFile)
				{
					Log.Error("XML loader: XML is missing: " + xmlLoadInfo.XmlName);
				}
			}
			else
			{
				ThreadManager.RunCoroutineSync(loadSingleXml(xmlLoadInfo, memoryStream, zipStream));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator cacheSingleXml(XmlLoadInfo _loadInfo, XmlFile _origXml, MemoryStream _memStream, DeflateOutputStream _zipStream)
	{
		MicroStopwatch timer = new MicroStopwatch(_bStart: true);
		_memStream.SetLength(0L);
		_origXml.SerializeToStream(_zipStream, _minified: true);
		_zipStream.Restart();
		if (timer.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
		{
			yield return null;
			timer.ResetAndRestart();
		}
		_loadInfo.CompressedXmlData = _memStream.ToArray();
		_memStream.SetLength(0L);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator loadSingleXml(XmlLoadInfo _loadInfo, MemoryStream _memStream, DeflateOutputStream _zipStream)
	{
		MicroStopwatch timer = new MicroStopwatch(_bStart: true);
		bool coroutineHadException = false;
		XmlFile xmlFile = null;
		yield return XmlPatcher.LoadAndPatchConfig(_loadInfo.XmlName, [PublicizedFrom(EAccessModifier.Internal)] (XmlFile _file) =>
		{
			xmlFile = _file;
		});
		if (xmlFile == null)
		{
			yield break;
		}
		yield return XmlPatcher.ApplyConditionalXmlBlocks(_loadInfo.XmlName, xmlFile, timer, XmlPatcher.EEvaluator.Host, [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			coroutineHadException = true;
		});
		if (coroutineHadException)
		{
			yield break;
		}
		if (timer.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
		{
			yield return null;
			timer.ResetAndRestart();
		}
		yield return DumpPatchedXml(_loadInfo, xmlFile, timer);
		xmlFile.RemoveComments();
		yield return CachePatchedXml(_loadInfo, xmlFile, timer, _memStream, _zipStream, [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			coroutineHadException = true;
		});
		if (coroutineHadException)
		{
			yield break;
		}
		if (timer.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
		{
			yield return null;
			timer.ResetAndRestart();
		}
		yield return XmlPatcher.ApplyConditionalXmlBlocks(_loadInfo.XmlName, xmlFile, timer, XmlPatcher.EEvaluator.Client, [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			coroutineHadException = true;
		});
		if (coroutineHadException)
		{
			yield break;
		}
		yield return ThreadManager.CoroutineWrapperWithExceptionCallback(_loadInfo.LoadMethod(xmlFile), [PublicizedFrom(EAccessModifier.Internal)] (Exception _exception) =>
		{
			Log.Error("XML loader: Loading and parsing '" + xmlFile.Filename + "' failed");
			Log.Exception(_exception);
			coroutineHadException = true;
		});
		if (coroutineHadException)
		{
			yield break;
		}
		if (_loadInfo.ExecuteAfterLoad != null)
		{
			if (timer.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				timer.ResetAndRestart();
			}
			yield return ThreadManager.CoroutineWrapperWithExceptionCallback(_loadInfo.ExecuteAfterLoad(), [PublicizedFrom(EAccessModifier.Internal)] (Exception _exception) =>
			{
				Log.Error("XML loader: Executing post load step on '" + xmlFile.Filename + "' failed");
				Log.Exception(_exception);
				coroutineHadException = true;
			});
			if (coroutineHadException)
			{
				yield break;
			}
		}
		xmlFile = null;
		Log.Out("Loaded (local): {0} in {1}", _loadInfo.XmlName, ((float)timer.ElapsedMicroseconds * 1E-06f).ToCultureInvariantString("f2"));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator DumpPatchedXml(XmlLoadInfo _loadInfo, XmlFile _xmlFile, MicroStopwatch _timer)
	{
		if (ThreadManager.IsMainThread() && Application.isPlaying && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			if (_timer.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				_timer.ResetAndRestart();
			}
			string path = GameIO.GetSaveGameDir() + "/ConfigsDump/" + _loadInfo.XmlName + ".xml";
			string directoryName = System.IO.Path.GetDirectoryName(path);
			if (!SdDirectory.Exists(directoryName))
			{
				SdDirectory.CreateDirectory(directoryName);
			}
			_xmlFile.SerializeToFile(path);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator CachePatchedXml(XmlLoadInfo _loadInfo, XmlFile _xmlFile, MicroStopwatch _timer, MemoryStream _memStream, DeflateOutputStream _zipStream, Action _errorCallback)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (_timer.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				_timer.ResetAndRestart();
			}
			bool coroutineHadException = false;
			yield return ThreadManager.CoroutineWrapperWithExceptionCallback(cacheSingleXml(_loadInfo, _xmlFile, _memStream, _zipStream), [PublicizedFrom(EAccessModifier.Internal)] (Exception _exception) =>
			{
				Log.Error("XML loader: Compressing XML data for '" + _xmlFile.Filename + "' failed");
				Log.Exception(_exception);
				coroutineHadException = true;
			});
			if (coroutineHadException)
			{
				_errorCallback?.Invoke();
			}
		}
	}

	[Conditional("UNITY_GAMECORE")]
	[Conditional("UNITY_XBOXONE")]
	[Conditional("UNITY_PS4")]
	[Conditional("UNITY_PS5")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void CollectGarbage()
	{
		GC.Collect();
	}

	public static IEnumerator LoadAllXmlsCo(bool _isStartup, ProgressDelegate _progressDelegate)
	{
		LoadAllXmlsCoComplete = false;
		MemoryStream memStream = new MemoryStream();
		DeflateOutputStream zipStream = new DeflateOutputStream(memStream, 3);
		XmlLoadInfo[] array = xmlsToLoad;
		foreach (XmlLoadInfo xmlLoadInfo in array)
		{
			if (!xmlLoadInfo.XmlFileExists())
			{
				if (!xmlLoadInfo.IgnoreMissingFile)
				{
					Log.Error("XML loader: XML is missing: " + xmlLoadInfo.XmlName);
				}
			}
			else if (!_isStartup || xmlLoadInfo.LoadAtStartup)
			{
				if (_progressDelegate != null && xmlLoadInfo.LoadStepLocalizationKey != null)
				{
					_progressDelegate(Localization.Get(xmlLoadInfo.LoadStepLocalizationKey), 0f);
				}
				yield return loadSingleXml(xmlLoadInfo, memStream, zipStream);
			}
		}
		LoadAllXmlsCoComplete = true;
	}

	public static void ReloadAllXmlsSync()
	{
		Cleanup(null);
		ThreadManager.RunCoroutineSync(LoadAllXmlsCo(_isStartup: false, null));
	}

	public static void SendXmlsToClient(ClientInfo _cInfo)
	{
		XmlLoadInfo[] array = xmlsToLoad;
		foreach (XmlLoadInfo xmlLoadInfo in array)
		{
			if (xmlLoadInfo.SendToClients && (xmlLoadInfo.LoadClientFile || xmlLoadInfo.CompressedXmlData != null))
			{
				_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConfigFile>().Setup(xmlLoadInfo.XmlName, xmlLoadInfo.LoadClientFile ? null : xmlLoadInfo.CompressedXmlData));
			}
		}
	}

	public static void SaveXmlsToFolder(string _exportPath)
	{
		XmlLoadInfo[] array = xmlsToLoad;
		foreach (XmlLoadInfo xmlLoadInfo in array)
		{
			if (xmlLoadInfo.CompressedXmlData == null)
			{
				continue;
			}
			byte[] bytes;
			using (MemoryStream input = new MemoryStream(xmlLoadInfo.CompressedXmlData))
			{
				using DeflateInputStream source = new DeflateInputStream(input);
				using MemoryStream memoryStream = new MemoryStream();
				StreamUtils.StreamCopy(source, memoryStream);
				bytes = memoryStream.ToArray();
			}
			string path = _exportPath + "/" + xmlLoadInfo.XmlName + ".xml";
			if (xmlLoadInfo.XmlName.IndexOf('/') >= 0)
			{
				string directoryName = System.IO.Path.GetDirectoryName(path);
				if (!SdDirectory.Exists(directoryName))
				{
					SdDirectory.CreateDirectory(directoryName);
				}
			}
			SdFile.WriteAllBytes(path, bytes);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XmlLoadInfo getLoadInfoForName(string _xmlName, out int _arrayIndex)
	{
		_arrayIndex = -1;
		for (int i = 0; i < xmlsToLoad.Length; i++)
		{
			XmlLoadInfo xmlLoadInfo = xmlsToLoad[i];
			if (xmlLoadInfo.XmlName.EqualsCaseInsensitive(_xmlName))
			{
				_arrayIndex = i;
				return xmlLoadInfo;
			}
		}
		return null;
	}

	public static void WaitForConfigsFromServer()
	{
		if (receivedConfigsHandlerCoroutine != null)
		{
			ThreadManager.StopCoroutine(receivedConfigsHandlerCoroutine);
		}
		receivedConfigsHandlerCoroutine = ThreadManager.StartCoroutine(handleReceivedConfigs());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator handleReceivedConfigs()
	{
		MicroStopwatch timer = new MicroStopwatch(_bStart: true);
		highestReceivedIndex = -1;
		XmlLoadInfo[] array = xmlsToLoad;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].WasReceivedFromServer = EClientFileState.None;
		}
		while (string.IsNullOrEmpty(GamePrefs.GetString(EnumGamePrefs.GameWorld)))
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
			{
				receivedConfigsHandlerCoroutine = null;
				yield break;
			}
			yield return null;
		}
		int waitingFor = 0;
		while (waitingFor < xmlsToLoad.Length)
		{
			XmlLoadInfo loadInfo = xmlsToLoad[waitingFor];
			if (!loadInfo.SendToClients)
			{
				waitingFor++;
				continue;
			}
			if (loadInfo.WasReceivedFromServer == EClientFileState.None)
			{
				if (loadInfo.IgnoreMissingFile && highestReceivedIndex > waitingFor)
				{
					waitingFor++;
				}
				else
				{
					yield return null;
				}
				continue;
			}
			waitingFor++;
			Cleanup(loadInfo.XmlName);
			if (loadInfo.WasReceivedFromServer == EClientFileState.LoadLocal)
			{
				yield return loadSingleXml(loadInfo, null, null);
				continue;
			}
			XmlFile xmlFile;
			using (MemoryStream input = new MemoryStream(loadInfo.CompressedXmlData))
			{
				using DeflateInputStream stream = new DeflateInputStream(input);
				xmlFile = new XmlFile(stream);
			}
			yield return null;
			bool coroutineHadException = false;
			yield return XmlPatcher.ApplyConditionalXmlBlocks(loadInfo.XmlName, xmlFile, timer, XmlPatcher.EEvaluator.Client, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				coroutineHadException = true;
			});
			if (coroutineHadException)
			{
				continue;
			}
			yield return ThreadManager.CoroutineWrapperWithExceptionCallback(loadInfo.LoadMethod(xmlFile), [PublicizedFrom(EAccessModifier.Internal)] (Exception _exception) =>
			{
				Log.Error("XML loader: Loading and parsing '" + loadInfo.XmlName + "' failed");
				Log.Exception(_exception);
				coroutineHadException = true;
			});
			if (coroutineHadException)
			{
				continue;
			}
			if (loadInfo.ExecuteAfterLoad != null)
			{
				yield return null;
				yield return ThreadManager.CoroutineWrapperWithExceptionCallback(loadInfo.ExecuteAfterLoad(), [PublicizedFrom(EAccessModifier.Internal)] (Exception _exception) =>
				{
					Log.Error("XML loader: Executing post load step on '" + loadInfo.XmlName + "' failed");
					Log.Exception(_exception);
					coroutineHadException = true;
				});
				if (coroutineHadException)
				{
					continue;
				}
			}
			Log.Out("Loaded (received): " + loadInfo.XmlName);
			yield return null;
			xmlFile = null;
		}
		receivedConfigsHandlerCoroutine = null;
	}

	public static bool AllConfigsReceivedAndLoaded()
	{
		return receivedConfigsHandlerCoroutine == null;
	}

	public static void ReceivedConfigFile(string _name, byte[] _data)
	{
		if (_data != null)
		{
			Log.Out($"Received config file '{_name}' from server. Len: {_data.Length}");
		}
		else
		{
			Log.Out("Loading config '" + _name + "' from local files");
		}
		int _arrayIndex;
		XmlLoadInfo loadInfoForName = getLoadInfoForName(_name, out _arrayIndex);
		if (loadInfoForName == null)
		{
			Log.Warning("XML loader: Received unknown config from server: " + _name);
			return;
		}
		loadInfoForName.CompressedXmlData = _data;
		loadInfoForName.WasReceivedFromServer = ((_data != null) ? EClientFileState.Received : EClientFileState.LoadLocal);
		highestReceivedIndex = MathUtils.Max(highestReceivedIndex, _arrayIndex);
	}

	public static void ReloadTextureArrays()
	{
		ThreadManager.RunCoroutineSync(meshDescCol.LoadTextureArrays(_isReload: true));
	}

	public static void ReloadInGameXML()
	{
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		XmlLoadInfo[] array = xmlsToLoad;
		foreach (XmlLoadInfo xmlLoadInfo in array)
		{
			if (xmlLoadInfo.AllowReloadDuringGame)
			{
				Log.Out("-- Reloading {0} --\n", xmlLoadInfo.XmlName);
				XmlFile xmlFile = new XmlFile(xmlLoadInfo.XmlName);
				if (xmlLoadInfo.ReloadDuringGameMethod != null)
				{
					xmlLoadInfo.ReloadDuringGameMethod(xmlFile);
				}
				else
				{
					ThreadManager.RunCoroutineSync(xmlLoadInfo.LoadMethod(xmlFile));
				}
			}
		}
		World world = GameManager.Instance.World;
		if (world != null)
		{
			List<Entity> list = world.Entities.list;
			for (int j = 0; j < list.Count; j++)
			{
				list[j].OnXMLChanged();
			}
		}
		Log.Out("-- ReloadInGameXML {0} seconds --\n", ((float)microStopwatch.ElapsedMicroseconds * 1E-06f).ToCultureInvariantString());
	}
}
