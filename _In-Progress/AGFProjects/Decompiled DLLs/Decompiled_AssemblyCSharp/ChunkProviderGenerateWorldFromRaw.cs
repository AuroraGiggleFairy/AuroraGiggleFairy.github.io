using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Collections;
using UnityEngine;

public class ChunkProviderGenerateWorldFromRaw : ChunkProviderGenerateWorld
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class Bluff
	{
		public int width;

		public int height;

		public float[] data;

		public static Bluff Load(string _name)
		{
			Texture2D texture2D = TextureUtils.LoadTexture(GameIO.GetGameDir("Data/Bluffs") + "/" + _name + ".tga");
			Color32[] pixels = texture2D.GetPixels32();
			Bluff bluff = new Bluff();
			bluff.width = texture2D.width;
			bluff.height = texture2D.height;
			bluff.data = new float[bluff.width * bluff.height];
			for (int num = pixels.Length - 1; num >= 0; num--)
			{
				bluff.data[num] = (int)pixels[num].r;
			}
			UnityEngine.Object.Destroy(texture2D);
			return bluff;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate void ProcessFilesCallback(Texture2D _splat3Tex, Texture2D _splat3Visual, Texture2D _splat4Tex, Texture2D _splat4Visual, WorldDecoratorPOIFromImage _decoratorPoiFromImage);

	public const string cRawProcessed = "_processed";

	public const string cHalf = "_half";

	public const int cMaxHeight = 255;

	[PublicizedFrom(EAccessModifier.Private)]
	public int heightMapWidth = 4096;

	[PublicizedFrom(EAccessModifier.Private)]
	public int heightMapHeight = 4096;

	[PublicizedFrom(EAccessModifier.Private)]
	public int heightMapScale = 1;

	public const float hmFac = 0.0038910506f;

	public IBackedArray<ushort> heightData;

	public WorldDecoratorPOIFromImage poiFromImage;

	[PublicizedFrom(EAccessModifier.Private)]
	public HeightMap heightMap;

	public Texture2D[] splats = new Texture2D[7];

	public Texture2D procBiomeMask1;

	public Texture2D procBiomeMask2;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, Bluff> bluffs = new Dictionary<string, Bluff>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFixedWaterLevel;

	public readonly Dictionary<string, uint> worldFileCrcs = new CaseInsensitiveStringDictionary<uint>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] FilesUsedForProcessing = new string[7] { "prefabs.xml", "dtm.raw", "splat3.tga", "splat3.png", "water_info.xml", "main.ttw", "biomes.png" };

	[field: PublicizedFrom(EAccessModifier.Private)]
	public long worldFileTotalSize
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public ChunkProviderGenerateWorldFromRaw(ChunkCluster _cc, string _levelName, bool _bClientMode = false, bool _bFixedWaterLevel = false)
		: base(_cc, _levelName, _bClientMode)
	{
		bFixedWaterLevel = _bFixedWaterLevel;
	}

	public override IEnumerator Init(World _world)
	{
		yield return base.Init(_world);
		world = _world;
		PathAbstractions.AbstractedLocation location = PathAbstractions.WorldsSearchPaths.GetLocation(levelName);
		string worldPath = location.FullPath;
		base.WorldInfo = GameUtils.WorldInfo.LoadWorldInfo(location);
		if (base.WorldInfo != null)
		{
			heightMapWidth = base.WorldInfo.HeightmapSize.x;
			heightMapHeight = base.WorldInfo.HeightmapSize.y;
			heightMapScale = base.WorldInfo.Scale;
			bFixedWaterLevel = base.WorldInfo.FixedWaterLevel;
		}
		MicroStopwatch ms = new MicroStopwatch();
		string dtmFilename = getFilenameDTM();
		if (!SdFile.Exists(dtmFilename + ".raw"))
		{
			if (SdFile.Exists(dtmFilename + ".tga"))
			{
				int width;
				int height;
				Color32[] array = TGALoader.LoadTGAAsArray(dtmFilename + ".tga", out width, out height);
				if (array != null)
				{
					float[,] data = HeightMapUtils.ConvertDTMToHeightData(array, heightMapWidth, heightMapHeight, _bFlip: true);
					HeightMapUtils.SaveHeightMapRAW(getFilenameDTM() + ".raw", heightMapWidth, heightMapHeight, data);
					HeightMapUtils.SaveHeightMapRAW(dtmFilename + ".raw", heightMapWidth, heightMapHeight, data);
				}
				Log.Out("GenWorldFromRaw tga to dtm took " + ms.ElapsedMilliseconds + "ms");
				ms.ResetAndRestart();
			}
			else
			{
				if (!SdFile.Exists(dtmFilename + ".png"))
				{
					throw new FileNotFoundException($"No height data found for world '{levelName}'");
				}
				float[,] data2 = HeightMapUtils.ConvertDTMToHeightDataExternal(levelName);
				HeightMapUtils.SmoothTerrain(7, data2);
				HeightMapUtils.SaveHeightMapRAW(dtmFilename + ".raw", heightMapWidth, heightMapHeight, data2);
				Log.Out("GenWorldFromRaw png to dtm took " + ms.ElapsedMilliseconds + "ms");
				ms.ResetAndRestart();
			}
		}
		yield return calcWorldFileCrcs(worldPath);
		WorldDecoratorPOIFromImage worldDecoratorPoiFromImage = null;
		Texture2D splat3Tex = null;
		Texture2D splat4Tex = null;
		Texture2D splat3Half = null;
		Texture2D splat4Half = null;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && filesNeedProcessing(worldPath, dtmFilename))
		{
			yield return processFiles(worldPath, [PublicizedFrom(EAccessModifier.Internal)] (Texture2D _splat3Tex, Texture2D _splat3Half, Texture2D _splat4Tex, Texture2D _splat4Half, WorldDecoratorPOIFromImage _poiFromImage) =>
			{
				splat3Tex = _splat3Tex;
				splat3Half = _splat3Half;
				splat4Tex = _splat4Tex;
				splat4Half = _splat4Half;
				worldDecoratorPoiFromImage = _poiFromImage;
			});
		}
		else
		{
			XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadHeightmap"));
			loadDTM();
			Log.Out("GenWorldFromRaw load dtm took " + ms.ElapsedMilliseconds + "ms");
			ms.ResetAndRestart();
		}
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadBiomes"));
		m_BiomeProvider = new WorldBiomeProviderFromImage(levelName, world.Biomes, heightMapWidth * heightMapScale);
		yield return m_BiomeProvider.InitData();
		Log.Out("GenWorldFromRaw biomes took " + ms.ElapsedMilliseconds + "ms");
		yield return GCUtils.UnloadAndCollectCo();
		ms.ResetAndRestart();
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadSplats"));
		string text = worldPath + "/splat3_processed.png";
		if (SdFile.Exists(text) && splat3Tex == null)
		{
			splat3Tex = TextureUtils.LoadTexture(text);
		}
		yield return null;
		text = worldPath + "/splat4_processed.png";
		if (SdFile.Exists(text) && splat4Tex == null)
		{
			splat4Tex = TextureUtils.LoadTexture(text);
		}
		int num = GetWorldSize().x / 8;
		int height2 = GetWorldSize().y / 8;
		procBiomeMask1 = new Texture2D(num, height2, TextureFormat.RGBA32, mipChain: false);
		procBiomeMask2 = new Texture2D(num, height2, TextureFormat.RGBA32, mipChain: false);
		NativeArray<Color32> pixelData = procBiomeMask1.GetPixelData<Color32>(0);
		NativeArray<Color32> pixelData2 = procBiomeMask2.GetPixelData<Color32>(0);
		GetWorldExtent(out var _minSize, out var _maxSize);
		for (int num2 = _minSize.z; num2 < _maxSize.z; num2 += 8)
		{
			int num3 = (num2 - _minSize.z) / 8 * num;
			for (int num4 = _minSize.x; num4 < _maxSize.x; num4 += 8)
			{
				BiomeDefinition biomeAt = m_BiomeProvider.GetBiomeAt(num4, num2);
				if (biomeAt != null)
				{
					Color32 value = default(Color32);
					Color32 value2 = default(Color32);
					int index = (num4 - _minSize.x) / 8 + num3;
					switch (biomeAt.m_Id)
					{
					case 1:
						value.r = byte.MaxValue;
						break;
					case 3:
						value.g = byte.MaxValue;
						break;
					case 9:
						value.b = byte.MaxValue;
						break;
					case 8:
						value.a = byte.MaxValue;
						break;
					case 5:
						value2.r = byte.MaxValue;
						break;
					}
					pixelData[index] = value;
					pixelData2[index] = value2;
				}
			}
		}
		yield return null;
		procBiomeMask1.filterMode = FilterMode.Bilinear;
		procBiomeMask1.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		procBiomeMask2.filterMode = FilterMode.Bilinear;
		procBiomeMask2.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		Log.Out("GenWorldFromRaw shader control textures took " + ms.ElapsedMilliseconds + "ms");
		ms.ResetAndRestart();
		m_TerrainGenerator = new TerrainFromRaw();
		if (heightMap == null)
		{
			heightMap = new HeightMap(heightMapWidth, heightMapHeight, 255f, heightData, heightMapWidth * heightMapScale);
		}
		yield return null;
		if (worldDecoratorPoiFromImage == null)
		{
			worldDecoratorPoiFromImage = new WorldDecoratorPOIFromImage(levelName, GetDynamicPrefabDecorator(), heightMapWidth, heightMapHeight, splat3Tex, _splat4Tex: splat4Tex, _bChangeWaterDensity: false, _worldScale: heightMapScale, _heightMap: heightMap);
			m_Decorators.Add(worldDecoratorPoiFromImage);
			yield return worldDecoratorPoiFromImage.InitData();
		}
		m_Decorators.Add(new WorldDecoratorBlocksFromBiome(m_BiomeProvider, GetDynamicPrefabDecorator()));
		yield return null;
		poiFromImage = m_Decorators[0] as WorldDecoratorPOIFromImage;
		((TerrainFromRaw)m_TerrainGenerator).Init(heightMap, m_BiomeProvider, levelName, world.Seed);
		yield return null;
		string text2 = (_world.IsEditor() ? null : GameIO.GetSaveGameRegionDir());
		if (!bClientMode)
		{
			m_RegionFileManager = new RegionFileManager(text2, text2, 0, !_world.IsEditor());
		}
		MultiBlockManager.Instance.Initialize(m_RegionFileManager);
		Log.Out("GenWorldFromRaw misc took " + ms.ElapsedMilliseconds + "ms");
		ms.ResetAndRestart();
		yield return null;
		if (GameOptionsManager.GetTextureQuality() > 0)
		{
			UnityEngine.Object.Destroy(splat3Tex);
			UnityEngine.Object.Destroy(splat4Tex);
			text = worldPath + "/splat3_half.png";
			if (SdFile.Exists(text))
			{
				if (splat3Half == null)
				{
					splat3Half = TextureUtils.LoadTexture(text);
				}
				yield return null;
				splats[0] = splat3Half;
				splats[0].filterMode = FilterMode.Bilinear;
			}
			yield return null;
			text = worldPath + "/splat4_half.png";
			if (SdFile.Exists(text))
			{
				if (splat4Half == null)
				{
					splat4Half = TextureUtils.LoadTexture(text);
				}
				yield return null;
				splats[1] = splat4Half;
				splats[1].filterMode = FilterMode.Bilinear;
			}
		}
		else
		{
			UnityEngine.Object.Destroy(splat3Half);
			UnityEngine.Object.Destroy(splat4Half);
			splats[0] = splat3Tex;
			splats[0].filterMode = FilterMode.Bilinear;
			splats[1] = splat4Tex;
			splats[1].filterMode = FilterMode.Bilinear;
		}
		yield return null;
		splats[0].Compress(highQuality: false);
		splats[0].Apply(updateMipmaps: false, makeNoLongerReadable: true);
		yield return null;
		splats[1].Compress(highQuality: false);
		splats[1].Apply(updateMipmaps: false, makeNoLongerReadable: true);
		Log.Out("GenWorldFromRaw splats took " + ms.ElapsedMilliseconds + "ms");
		ms.ResetAndRestart();
		yield return GCUtils.UnloadAndCollectCo();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void loadDTM(string _dtmFilename = null)
	{
		if (_dtmFilename == null)
		{
			_dtmFilename = getFilenameDTM();
		}
		heightData?.Dispose();
		heightData = HeightMapUtils.LoadHeightMapRAW(_dtmFilename + ".raw", heightMapWidth, heightMapHeight, 1f, 250);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool filesNeedProcessing(string _worldPath, string _dtmFilename)
	{
		if (_dtmFilename.EndsWith("_processed") && SdFile.Exists(_worldPath + "/splat3_processed.png") && SdFile.Exists(_worldPath + "/splat4_processed.png") && SdFile.Exists(_worldPath + "/splat3_half.png") && SdFile.Exists(_worldPath + "/splat4_half.png"))
		{
			return !verifyFileHashes(_worldPath);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator calcWorldFileCrcs(string _worldPath)
	{
		MicroStopwatch msw = new MicroStopwatch(_bStart: true);
		worldFileTotalSize = 0L;
		worldFileCrcs.Clear();
		byte[] buffer = new byte[32768];
		string[] files = SdDirectory.GetFiles(_worldPath);
		foreach (string text in files)
		{
			string filename = GameIO.GetFilenameFromPath(text);
			worldFileTotalSize += GameIO.FileSize(text);
			yield return IOUtils.CalcCrcCoroutine(text, [PublicizedFrom(EAccessModifier.Internal)] (uint _hash) =>
			{
				worldFileCrcs.Add(filename, _hash);
			}, Constants.cMaxLoadTimePerFrameMillis, buffer);
		}
		Log.Out("Calculating world hashes took {0} ms (world size {1} MiB)", msw.ElapsedMilliseconds, worldFileTotalSize / 1024 / 1024);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void saveFileHashes(string _worldPath, Dictionary<string, uint> _worldFileCrcs)
	{
		using Stream stream = SdFile.Open(_worldPath + "/checksums.txt", FileMode.Create, FileAccess.Write);
		using StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8);
		foreach (KeyValuePair<string, uint> worldFileCrc in worldFileCrcs)
		{
			streamWriter.Write(worldFileCrc.Key);
			streamWriter.Write("=");
			streamWriter.WriteLine(worldFileCrc.Value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, uint> loadStoredFileHashes(string _worldPath)
	{
		if (!SdFile.Exists(_worldPath + "/checksums.txt"))
		{
			return null;
		}
		Dictionary<string, uint> dictionary = new CaseInsensitiveStringDictionary<uint>();
		using StreamReader streamReader = SdFile.OpenText(_worldPath + "/checksums.txt");
		string text;
		while ((text = streamReader.ReadLine()) != null)
		{
			string[] array = text.Split('=');
			if (array.Length != 2)
			{
				Log.Warning("Invalid line in checksums.txt: {0}", text);
			}
			else
			{
				string key = array[0];
				uint value = StringParsers.ParseUInt32(array[1]);
				dictionary.Add(key, value);
			}
		}
		return dictionary;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool verifyFileHashes(string _worldPath)
	{
		Dictionary<string, uint> dictionary = loadStoredFileHashes(_worldPath);
		if (dictionary == null)
		{
			Log.Warning("No hashes for world");
			return false;
		}
		string[] filesUsedForProcessing = FilesUsedForProcessing;
		foreach (string text in filesUsedForProcessing)
		{
			if (!dictionary.ContainsKey(text) && SdFile.Exists(_worldPath + "/" + text))
			{
				Log.Warning("Missing hash for " + text);
				return false;
			}
			if (worldFileCrcs.ContainsKey(text) && worldFileCrcs[text] != dictionary[text])
			{
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator processFiles(string _worldPath, ProcessFilesCallback _callback)
	{
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadProcessWorldHeightmap"));
		MicroStopwatch yieldMs = new MicroStopwatch(_bStart: true);
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		Log.Out("Processing world files");
		string processedFileName = _worldPath + "/dtm_processed.raw";
		loadDTM(_worldPath + "/dtm");
		ushort[] topTexMap = new ushort[heightMapWidth * heightMapScale * heightMapHeight * heightMapScale];
		yield return GetDynamicPrefabDecorator().CopyPrefabHeightsIntoHeightMap(heightMapWidth, heightMapHeight, heightData, heightMapScale, topTexMap);
		ThreadManager.AddSingleTask([PublicizedFrom(EAccessModifier.Internal)] (ThreadManager.TaskInfo _taskInfo) =>
		{
			HeightMapUtils.SaveHeightMapRAW(processedFileName, heightMapWidth, heightMapHeight, heightData);
		});
		yield return null;
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadProcessWorldSplats"));
		Texture2D splat3Tex = null;
		string text = _worldPath + "/splat3";
		if (SdFile.Exists(text + ".png"))
		{
			splat3Tex = TextureUtils.LoadTexture(text + ".png", FilterMode.Point, _bMipmaps: true);
		}
		else if (SdFile.Exists(text + ".tga"))
		{
			splat3Tex = TextureUtils.LoadTexture(text + ".tga");
		}
		bool flag = splat3Tex.format == TextureFormat.ARGB32;
		if (!flag && splat3Tex.format != TextureFormat.RGBA32)
		{
			Log.Error("World's splat3 file is not in the correct format (needs to be either RGBA32 or ARGB32)!");
			yield break;
		}
		Texture2D splat4Tex = new Texture2D(heightMapWidth * heightMapScale, heightMapHeight * heightMapScale, TextureFormat.ARGB32, mipChain: true, linear: false);
		string text2 = _worldPath + "/splat4";
		bool splat4Loaded = false;
		if (SdFile.Exists(text2 + ".png"))
		{
			splat4Tex = TextureUtils.LoadTexture(text2 + ".png", FilterMode.Point, _bMipmaps: true);
			splat4Loaded = true;
		}
		else if (SdFile.Exists(text2 + ".tga"))
		{
			splat4Tex = TextureUtils.LoadTexture(text2 + ".tga");
			splat4Loaded = true;
		}
		NativeArray<TextureUtils.ColorARGB32> splat4Cols = splat4Tex.GetRawTextureData<TextureUtils.ColorARGB32>();
		if (flag)
		{
			NativeArray<TextureUtils.ColorARGB32> splat3Cols = splat3Tex.GetRawTextureData<TextureUtils.ColorARGB32>();
			for (int i = 0; i < topTexMap.Length; i++)
			{
				if (i % Constants.cMaxLoadTimePixelsPerTest == 0 && yieldMs.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					yieldMs.ResetAndRestart();
				}
				TextureUtils.ColorARGB32 value = splat3Cols[i];
				TextureUtils.ColorARGB32 value2 = default(TextureUtils.ColorARGB32);
				if (splat4Loaded)
				{
					value2 = splat4Cols[i];
				}
				switch (topTexMap[i])
				{
				case 10:
					value.r = byte.MaxValue;
					break;
				case 11:
					value.g = byte.MaxValue;
					break;
				case 8:
					value.b = byte.MaxValue;
					break;
				case 185:
					value.a = byte.MaxValue;
					break;
				case 200:
					value2.r = byte.MaxValue;
					break;
				case 2:
					value2.g = byte.MaxValue;
					break;
				}
				splat3Cols[i] = value;
				splat4Cols[i] = value2;
			}
		}
		else
		{
			NativeArray<Color32> splat3Cols2 = splat3Tex.GetRawTextureData<Color32>();
			for (int i = 0; i < topTexMap.Length; i++)
			{
				if (i % Constants.cMaxLoadTimePixelsPerTest == 0 && yieldMs.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					yieldMs.ResetAndRestart();
				}
				Color32 value3 = splat3Cols2[i];
				TextureUtils.ColorARGB32 value4 = default(TextureUtils.ColorARGB32);
				if (splat4Loaded)
				{
					value4 = splat4Cols[i];
				}
				switch (topTexMap[i])
				{
				case 10:
					value3.r = byte.MaxValue;
					break;
				case 11:
					value3.g = byte.MaxValue;
					break;
				case 8:
					value3.b = byte.MaxValue;
					break;
				case 185:
					value3.a = byte.MaxValue;
					break;
				case 200:
					value4.r = byte.MaxValue;
					break;
				case 2:
					value4.g = byte.MaxValue;
					break;
				}
				splat3Cols2[i] = value3;
				splat4Cols[i] = value4;
			}
		}
		yield return null;
		if (heightMap == null)
		{
			heightMap = new HeightMap(heightMapWidth, heightMapHeight, 255f, heightData, heightMapWidth * heightMapScale);
		}
		splat4Tex.SetPixelData(splat4Cols.ToArray(), 0);
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadProcessWorldDecos"));
		string text3 = levelName;
		DynamicPrefabDecorator dynamicPrefabDecorator = GetDynamicPrefabDecorator();
		int worldX = heightMapWidth;
		int worldZ = heightMapHeight;
		Texture2D splat3Tex2 = splat3Tex;
		Texture2D splat4Tex2 = (splat4Loaded ? splat4Tex : null);
		WorldDecoratorPOIFromImage worldDecoratorPoiFromImage = new WorldDecoratorPOIFromImage(text3, dynamicPrefabDecorator, worldX, worldZ, splat3Tex2, _bChangeWaterDensity: false, heightMapScale, heightMap, splat4Tex2);
		m_Decorators.Add(worldDecoratorPoiFromImage);
		yield return worldDecoratorPoiFromImage.InitData();
		GridCompressedData<byte> cols = worldDecoratorPoiFromImage.m_Poi.colors;
		int length = cols.width * cols.height;
		for (int i = 0; i < length; i++)
		{
			if (i % Constants.cMaxLoadTimePixelsPerTest == 0 && yieldMs.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				yieldMs.ResetAndRestart();
			}
			if (cols.GetValue(i) < 5)
			{
				continue;
			}
			TextureUtils.ColorARGB32 value5 = splat4Cols[i];
			if (Mathf.FloorToInt(heightMap.GetAt(i)) <= (byte)(cols.GetValue(i) - 5) + 1)
			{
				value5.g = byte.MaxValue;
				if (cols.GetValue(i) > 5)
				{
					value5.b = (byte)(cols.GetValue(i) - 5);
				}
			}
			splat4Cols[i] = value5;
		}
		yield return null;
		splat3Tex.Apply(updateMipmaps: true, makeNoLongerReadable: false);
		yield return null;
		splat4Tex.SetPixelData(splat4Cols.ToArray(), 0);
		splat4Tex.Apply(updateMipmaps: true, makeNoLongerReadable: false);
		yield return null;
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadProcessWorldWriting"));
		SdFile.WriteAllBytes(_worldPath + "/splat3_processed.png", splat3Tex.EncodeToPNG());
		yield return GCUtils.UnloadAndCollectCo();
		SdFile.WriteAllBytes(_worldPath + "/splat4_processed.png", splat4Tex.EncodeToPNG());
		yield return GCUtils.UnloadAndCollectCo();
		Texture2D splat3Half = generateHalfResTexture(splat3Tex);
		Texture2D splat4Half = generateHalfResTexture(splat4Tex);
		yield return null;
		SdFile.WriteAllBytes(_worldPath + "/splat3_half.png", splat3Half.EncodeToPNG());
		yield return GCUtils.UnloadAndCollectCo();
		SdFile.WriteAllBytes(_worldPath + "/splat4_half.png", splat4Half.EncodeToPNG());
		yield return GCUtils.UnloadAndCollectCo();
		yield return calcWorldFileCrcs(_worldPath);
		saveFileHashes(_worldPath, worldFileCrcs);
		yield return GCUtils.UnloadAndCollectCo();
		Log.Out("Loading and creating dtm raw file took " + ms.ElapsedMilliseconds + "ms");
		_callback(splat3Tex, splat3Half, splat4Tex, splat4Half, worldDecoratorPoiFromImage);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D generateHalfResTexture(Texture2D _tex)
	{
		if (_tex.mipmapCount < 1)
		{
			Log.Error("Attempted to generate half-res texture from a texture that does not have mip level 1. Returning the source texture instead.");
			return _tex;
		}
		Texture2D texture2D = new Texture2D(_tex.width >> 1, _tex.height >> 1);
		texture2D.SetPixels(_tex.GetPixels(1));
		texture2D.Apply();
		return texture2D;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string getFilenameDTM()
	{
		string fullPath = PathAbstractions.WorldsSearchPaths.GetLocation(levelName).FullPath;
		if (SdFile.Exists(fullPath + "/dtm_processed.raw"))
		{
			return fullPath + "/dtm_processed";
		}
		return fullPath + "/dtm";
	}

	public override void ReloadAllChunks()
	{
		loadDTM();
		((TerrainFromRaw)m_TerrainGenerator).Init(heightMap, m_BiomeProvider, levelName, world.Seed);
		base.ReloadAllChunks();
	}

	public override EnumChunkProviderId GetProviderId()
	{
		return EnumChunkProviderId.ChunkDataDriven;
	}

	public override bool GetWorldExtent(out Vector3i _minSize, out Vector3i _maxSize)
	{
		_minSize = new Vector3i(-heightMapWidth * heightMapScale / 2, 0, -heightMapHeight * heightMapScale / 2);
		_maxSize = new Vector3i(heightMapWidth * heightMapScale / 2, 255, heightMapHeight * heightMapScale / 2);
		return true;
	}

	public override Vector2i GetWorldSize()
	{
		return new Vector2i(heightMapWidth * heightMapScale, heightMapHeight * heightMapScale);
	}

	public override int GetPOIBlockIdOverride(int x, int z)
	{
		if (poiFromImage == null)
		{
			return 0;
		}
		WorldGridCompressedData<byte> poi = poiFromImage.m_Poi;
		byte b = 0;
		if (!poi.Contains(x, z) || (b = poi.GetData(x, z)) == byte.MaxValue || b == 0)
		{
			return 0;
		}
		PoiMapElement poiForColor = world.Biomes.getPoiForColor(b);
		if (poiForColor == null || (bFixedWaterLevel && poiForColor.m_BlockValue.Block.blockMaterial.IsLiquid))
		{
			return 0;
		}
		return poiForColor.m_BlockValue.type;
	}

	public override IEnumerator FillOccupiedMap(int width, int height, DecoOccupiedMap _occupiedMap, List<PrefabInstance> overridePOIList = null)
	{
		MicroStopwatch mswYields = new MicroStopwatch();
		EnumDecoOccupied[] occupiedMap = _occupiedMap.GetData();
		int length = heightData.Length;
		mswYields.ResetAndRestart();
		int y;
		if (m_Decorators[0] is WorldDecoratorPOIFromImage)
		{
			WorldDecoratorPOIFromImage worldDecoratorPOIFromImage = (WorldDecoratorPOIFromImage)m_Decorators[0];
			WorldGridCompressedData<byte> poi = worldDecoratorPOIFromImage.m_Poi;
			y = 0;
			while (y < height)
			{
				int num = y * width;
				int num2 = num + width;
				for (int i = num; i < num2; i++)
				{
					byte value = poi.colors.GetValue(i);
					if (value != 0 && value != byte.MaxValue)
					{
						occupiedMap[i] = EnumDecoOccupied.POI;
					}
				}
				if (mswYields.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					mswYields.ResetAndRestart();
				}
				int num3 = y + 1;
				y = num3;
			}
		}
		yield return null;
		DynamicPrefabDecorator dynamicPrefabDecorator = GetDynamicPrefabDecorator();
		overridePOIList = ((dynamicPrefabDecorator != null) ? dynamicPrefabDecorator.GetDynamicPrefabs() : overridePOIList);
		if (overridePOIList != null)
		{
			for (int j = 0; j < overridePOIList.Count; j++)
			{
				PrefabInstance prefabInstance = overridePOIList[j];
				_occupiedMap.SetArea(prefabInstance.boundingBoxPosition.x, prefabInstance.boundingBoxPosition.z, EnumDecoOccupied.POI, prefabInstance.boundingBoxSize.x, prefabInstance.boundingBoxSize.z);
			}
		}
		if (WorldBiomes.Instance.GetTotalBluffsCount() > 0)
		{
			IBiomeProvider biomeProvider = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider();
			int num4 = -width / 2;
			int num5 = width / 2;
			int num6 = -height / 2;
			int num7 = height / 2;
			GameRandom gameRandom = Utils.RandomFromSeedOnPos(0, 0, GameManager.Instance.World.Seed);
			int num8 = 0;
			using IBackedArrayView<ushort> backedArrayView = BackedArrays.CreateSingleView(heightData, BackedArrayHandleMode.ReadWrite);
			for (int k = num6; k < num7; k++)
			{
				for (int l = num4; l < num5; l++)
				{
					float _intensity;
					BiomeDefinition biomeAt = biomeProvider.GetBiomeAt(l, k, out _intensity);
					if (biomeAt == null)
					{
						continue;
					}
					for (int m = 0; m < biomeAt.m_DecoBluffs.Count; m++)
					{
						BiomeBluffDecoration biomeBluffDecoration = biomeAt.m_DecoBluffs[m];
						if (gameRandom.RandomFloat > biomeBluffDecoration.m_Prob)
						{
							continue;
						}
						if (!bluffs.TryGetValue(biomeAt.m_DecoBluffs[m].m_sName, out var value2))
						{
							value2 = Bluff.Load(biomeAt.m_DecoBluffs[m].m_sName);
							bluffs.Add(biomeAt.m_DecoBluffs[m].m_sName, value2);
						}
						int num9 = gameRandom.RandomRange(3);
						Vector3i vector3i = new Vector3i(value2.width, 1, value2.height);
						for (int n = 0; n < num9; n++)
						{
							int x = vector3i.x;
							vector3i.x = vector3i.z;
							vector3i.z = x;
						}
						if (_occupiedMap.CheckArea(l, k, EnumDecoOccupied.Stop_BigDeco, vector3i.x, vector3i.z))
						{
							continue;
						}
						_occupiedMap.SetArea(l, k, EnumDecoOccupied.Perimeter, vector3i.x, vector3i.z);
						float num10 = biomeAt.m_DecoBluffs[m].m_MinScale + gameRandom.RandomFloat * (biomeAt.m_DecoBluffs[m].m_MaxScale - biomeAt.m_DecoBluffs[m].m_MinScale);
						for (int num11 = 0; num11 < value2.height; num11++)
						{
							for (int num12 = 0; num12 < value2.width; num12++)
							{
								int num13 = num9 switch
								{
									1 => num11 + (value2.width - num12 - 1) * value2.height, 
									2 => value2.width - num12 - 1 + (value2.height - num11 - 1) * value2.width, 
									3 => value2.height - num11 - 1 + num12 * value2.height, 
									_ => num12 + num11 * value2.width, 
								};
								float num14 = value2.data[num13] * num10;
								int i2 = (num12 + l + width / 2 + (num11 + k + height / 2) * width) % length;
								float num15 = (float)(int)backedArrayView[i2] * 0.0038910506f + num14;
								if (num15 > 246f)
								{
									num15 = 246f;
								}
								backedArrayView[i2] = (ushort)(num15 / 0.0038910506f);
							}
						}
						num8++;
					}
				}
			}
			GameRandomManager.Instance.FreeGameRandom(gameRandom);
		}
		yield return null;
		mswYields.ResetAndRestart();
		int bigSlope = (int)(Mathf.Sin(MathF.PI * 13f / 36f) / Mathf.Cos(MathF.PI * 13f / 36f) / 1.5140275E-05f);
		int smallSlope = (int)(Mathf.Sin(MathF.PI * 4f / 15f) / Mathf.Cos(MathF.PI * 4f / 15f) / 1.5140275E-05f);
		ReadOnlyMemory<ushort> heightsRowMemory;
		IDisposable heightsRowMemoryHandle = heightData.GetReadOnlyMemory(0, width, out heightsRowMemory);
		IDisposable heightsNextRowMemoryHandle = null;
		int heightM1 = height - 1;
		int widthM1 = width - 1;
		y = 0;
		while (y < heightM1)
		{
			int num16 = y * width;
			heightsNextRowMemoryHandle = heightData.GetReadOnlyMemory(num16 + width, width, out var memory);
			ReadOnlySpan<ushort> span = heightsRowMemory.Span;
			ReadOnlySpan<ushort> span2 = memory.Span;
			Span<EnumDecoOccupied> span3 = occupiedMap.AsSpan(num16, widthM1);
			int num17 = span[0];
			for (int num18 = 0; num18 < widthM1; num18++)
			{
				int num19 = span[num18 + 1];
				int num20 = span2[num18];
				int num21 = num17 - num19;
				int num22 = num17 - num20;
				num17 = num19;
				int num23 = num21 * num21 + num22 * num22;
				if (num23 > smallSlope)
				{
					if (num23 > bigSlope)
					{
						span3[num18] = EnumDecoOccupied.BigSlope;
					}
					else
					{
						span3[num18] = EnumDecoOccupied.SmallSlope;
					}
				}
			}
			heightsRowMemoryHandle?.Dispose();
			heightsRowMemory = memory;
			heightsRowMemoryHandle = heightsNextRowMemoryHandle;
			if (mswYields.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				mswYields.ResetAndRestart();
			}
			int num3 = y + 1;
			y = num3;
		}
		heightsRowMemoryHandle?.Dispose();
		heightsNextRowMemoryHandle?.Dispose();
	}

	public override float GetPOIHeightOverride(int x, int z)
	{
		if (!(m_Decorators[0] is WorldDecoratorPOIFromImage { m_Poi: var poi } worldDecoratorPOIFromImage))
		{
			return 0f;
		}
		byte b = 0;
		if (!poi.Contains(x / worldDecoratorPOIFromImage.worldScale, z / worldDecoratorPOIFromImage.worldScale) || (b = poi.GetData(x / worldDecoratorPOIFromImage.worldScale, z / worldDecoratorPOIFromImage.worldScale)) == byte.MaxValue || b == 0)
		{
			return 0f;
		}
		PoiMapElement poiForColor = world.Biomes.getPoiForColor(b);
		if (poiForColor == null)
		{
			return 0f;
		}
		if (!poiForColor.m_BlockValue.Block.blockMaterial.IsLiquid)
		{
			return 0f;
		}
		return poiForColor.m_YPosFill;
	}

	public float GetHeight(int x, int z)
	{
		try
		{
			return heightMap.GetAt(x, z);
		}
		catch (Exception)
		{
			Log.Error("Get Height Error x: {0} z: {1}", x, z);
			return 0f;
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		for (int i = 0; i < splats.Length; i++)
		{
			UnityEngine.Object.Destroy(splats[i]);
		}
		UnityEngine.Object.Destroy(procBiomeMask1);
		UnityEngine.Object.Destroy(procBiomeMask2);
		heightMap?.Dispose();
		heightMap = null;
		heightData?.Dispose();
		heightData = null;
		m_BiomeProvider?.Cleanup();
	}

	public void GetWaterChunks16x16(out int _water16x16ChunksW, out byte[] _water16x16Chunks)
	{
		if (!(m_Decorators[0] is WorldDecoratorPOIFromImage worldDecoratorPOIFromImage))
		{
			_water16x16ChunksW = 0;
			_water16x16Chunks = null;
		}
		else
		{
			worldDecoratorPOIFromImage.GetWaterChunks16x16(out _water16x16ChunksW, out _water16x16Chunks);
		}
	}

	public override ChunkProtectionLevel GetChunkProtectionLevel(Vector3i worldPos)
	{
		return m_RegionFileManager.GetChunkProtectionLevelForWorldPos(worldPos);
	}
}
