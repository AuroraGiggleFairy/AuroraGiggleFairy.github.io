using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using Utf8Json;
using Webserver.FileCache;

namespace MapRendering;

public class MapRenderer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static MapRenderer instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object lockObject = new object();

	public static bool renderingEnabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly MapTileCache cache = new MapTileCache(Constants.MapBlockSize);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Vector2i, Color32[]> dirtyChunks = new Dictionary<Vector2i, Color32[]>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly MicroStopwatch msw = new MicroStopwatch();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly MapRenderBlockBuffer[] zoomLevelBuffers;

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine renderCoroutineRef;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool renderingFullMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public float renderTimeout = float.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shutdown;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WaitForSeconds coroutineDelay = new WaitForSeconds(0.2f);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Vector2i> chunksToRender = new List<Vector2i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Vector2i> chunksRendered = new List<Vector2i>();

	public static bool Enabled
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return GamePrefs.GetBool(EnumGamePrefs.EnableMapRendering);
			}
			return false;
		}
	}

	public static bool HasInstance => instance != null;

	public static MapRenderer Instance => instance ?? (instance = new MapRenderer());

	[PublicizedFrom(EAccessModifier.Private)]
	public MapRenderer()
	{
		Constants.MapDirectory = GameIO.GetSaveGameDir() + "/map";
		if (!LoadMapInfo())
		{
			WriteMapInfo();
		}
		cache.SetZoomCount(Constants.Zoomlevels);
		zoomLevelBuffers = new MapRenderBlockBuffer[Constants.Zoomlevels];
		for (int i = 0; i < Constants.Zoomlevels; i++)
		{
			zoomLevelBuffers[i] = new MapRenderBlockBuffer(i, cache);
		}
		renderCoroutineRef = ThreadManager.StartCoroutine(renderCoroutine());
	}

	public static AbstractCache GetTileCache()
	{
		return Instance.cache;
	}

	public static void Shutdown()
	{
		if (instance != null)
		{
			instance.shutdown = true;
			if (instance.renderCoroutineRef != null)
			{
				ThreadManager.StopCoroutine(instance.renderCoroutineRef);
				instance.renderCoroutineRef = null;
			}
			instance = null;
		}
	}

	public static void RenderSingleChunk(Chunk _chunk)
	{
		if (!renderingEnabled || instance == null)
		{
			return;
		}
		ThreadPool.UnsafeQueueUserWorkItem([PublicizedFrom(EAccessModifier.Internal)] (object _o) =>
		{
			try
			{
				if (instance.renderingFullMap)
				{
					return;
				}
				lock (lockObject)
				{
					Chunk obj = (Chunk)_o;
					Vector3i worldPos = obj.GetWorldPos();
					Vector2i key = new Vector2i(worldPos.x / 16, worldPos.z / 16);
					ushort[] mapColors = obj.GetMapColors();
					if (mapColors != null)
					{
						Color32[] array = new Color32[256];
						for (int i = 0; i < mapColors.Length; i++)
						{
							array[i] = shortColorToColor32(mapColors[i]);
						}
						instance.dirtyChunks[key] = array;
					}
				}
			}
			catch (Exception arg)
			{
				Log.Out($"Exception in MapRendering.RenderSingleChunk(): {arg}");
			}
		}, _chunk);
	}

	public void RenderFullMap()
	{
		MicroStopwatch microStopwatch = new MicroStopwatch();
		string saveGameRegionDir = GameIO.GetSaveGameRegionDir();
		RegionFileManager regionFileManager = new RegionFileManager(saveGameRegionDir, saveGameRegionDir, 0, _bSaveOnChunkDrop: false);
		Texture2D texture2D = null;
		getWorldExtent(regionFileManager, out var _minChunk, out var _maxChunk, out var _minPos, out var _maxPos, out var _widthChunks, out var _heightChunks, out var _widthPix, out var _heightPix);
		Log.Out($"RenderMap: min: {_minChunk.ToString()}, max: {_maxChunk.ToString()}, minPos: {_minPos.ToString()}, maxPos: {_maxPos.ToString()}, w/h: {_widthChunks}/{_heightChunks}, wP/hP: {_widthPix}/{_heightPix}");
		lock (lockObject)
		{
			for (int i = 0; i < Constants.Zoomlevels; i++)
			{
				zoomLevelBuffers[i].ResetBlock();
			}
			if (Directory.Exists(Constants.MapDirectory))
			{
				Directory.Delete(Constants.MapDirectory, recursive: true);
			}
			WriteMapInfo();
			renderingFullMap = true;
			if (_widthPix <= 8192 && _heightPix <= 8192)
			{
				texture2D = new Texture2D(_widthPix, _heightPix);
			}
			Vector2i vector2i = default(Vector2i);
			Vector2i key = default(Vector2i);
			vector2i.x = 0;
			while (vector2i.x < _widthPix)
			{
				vector2i.y = 0;
				while (vector2i.y < _heightPix)
				{
					key.x = vector2i.x / 16 + _minChunk.x;
					key.y = vector2i.y / 16 + _minChunk.y;
					try
					{
						long key2 = WorldChunkCache.MakeChunkKey(key.x, key.y);
						if (regionFileManager.ContainsChunkSync(key2))
						{
							ushort[] mapColors = regionFileManager.GetChunkSync(key2).GetMapColors();
							if (mapColors != null)
							{
								Color32[] array = new Color32[256];
								for (int j = 0; j < mapColors.Length; j++)
								{
									array[j] = shortColorToColor32(mapColors[j]);
								}
								dirtyChunks[key] = array;
								if (texture2D != null)
								{
									texture2D.SetPixels32(vector2i.x, vector2i.y, 16, 16, array);
								}
							}
						}
					}
					catch (Exception arg)
					{
						Log.Out($"Exception: {arg}");
					}
					vector2i.y += 16;
				}
				while (dirtyChunks.Count > 0)
				{
					RenderDirtyChunks();
				}
				Log.Out($"RenderMap: {vector2i.x}/{_widthPix} ({(int)((float)vector2i.x / (float)_widthPix * 100f)}%)");
				vector2i.x += 16;
			}
		}
		regionFileManager.Cleanup();
		if (texture2D != null)
		{
			byte[] bytes = texture2D.EncodeToPNG();
			File.WriteAllBytes(Constants.MapDirectory + "/map.png", bytes);
			UnityEngine.Object.Destroy(texture2D);
		}
		renderingFullMap = false;
		Log.Out($"Generating map took: {microStopwatch.ElapsedMilliseconds} ms");
		Log.Out($"World extent: {_minPos} - {_maxPos}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveAllBlockMaps()
	{
		for (int i = 0; i < Constants.Zoomlevels; i++)
		{
			zoomLevelBuffers[i].SaveBlock();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator renderCoroutine()
	{
		while (!shutdown)
		{
			lock (lockObject)
			{
				if (dirtyChunks.Count > 0 && renderTimeout >= 1.7014117E+38f)
				{
					renderTimeout = Time.time + 0.5f;
				}
				if (Time.time > renderTimeout || dirtyChunks.Count > 200)
				{
					RenderDirtyChunks();
				}
			}
			yield return coroutineDelay;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderDirtyChunks()
	{
		msw.ResetAndRestart();
		if (dirtyChunks.Count <= 0)
		{
			return;
		}
		chunksToRender.Clear();
		chunksRendered.Clear();
		dirtyChunks.CopyKeysTo(chunksToRender);
		Vector2i vector2i = chunksToRender[0];
		chunksRendered.Add(vector2i);
		getBlockNumber(vector2i, out var _block, out var _, Constants.MAP_BLOCK_TO_CHUNK_DIV, 16);
		zoomLevelBuffers[Constants.Zoomlevels - 1].LoadBlock(_block);
		foreach (Vector2i item in chunksToRender)
		{
			getBlockNumber(item, out var _block2, out var _blockOffset2, Constants.MAP_BLOCK_TO_CHUNK_DIV, 16);
			if (_block2.Equals(_block))
			{
				chunksRendered.Add(item);
				if (dirtyChunks[item].Length != 256)
				{
					Log.Error($"Rendering chunk has incorrect data size of {dirtyChunks[item].Length} instead of {256}");
				}
				zoomLevelBuffers[Constants.Zoomlevels - 1].SetPart(_blockOffset2, 16, dirtyChunks[item]);
			}
		}
		foreach (Vector2i item2 in chunksRendered)
		{
			dirtyChunks.Remove(item2);
		}
		RenderZoomLevel(_block);
		SaveAllBlockMaps();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderZoomLevel(Vector2i _innerBlock)
	{
		int num = Constants.Zoomlevels - 1;
		while (num > 0)
		{
			getBlockNumber(_innerBlock, out var _block, out var _blockOffset, 2, Constants.MapBlockSize / 2);
			zoomLevelBuffers[num - 1].LoadBlock(_block);
			if ((zoomLevelBuffers[num].FormatSelf == TextureFormat.ARGB32 || zoomLevelBuffers[num].FormatSelf == TextureFormat.RGBA32) && zoomLevelBuffers[num].FormatSelf == zoomLevelBuffers[num - 1].FormatSelf)
			{
				zoomLevelBuffers[num - 1].SetPartNative(_blockOffset, Constants.MapBlockSize / 2, zoomLevelBuffers[num].GetHalfScaledNative());
			}
			else
			{
				zoomLevelBuffers[num - 1].SetPart(_blockOffset, Constants.MapBlockSize / 2, zoomLevelBuffers[num].GetHalfScaled());
			}
			num--;
			_innerBlock = _block;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void getBlockNumber(Vector2i _innerPos, out Vector2i _block, out Vector2i _blockOffset, int _scaleFactor, int _offsetSize)
	{
		_block = default(Vector2i);
		_blockOffset = default(Vector2i);
		_block.x = (_innerPos.x + 16777216) / _scaleFactor - 16777216 / _scaleFactor;
		_block.y = (_innerPos.y + 16777216) / _scaleFactor - 16777216 / _scaleFactor;
		_blockOffset.x = (_innerPos.x + 16777216) % _scaleFactor * _offsetSize;
		_blockOffset.y = (_innerPos.y + 16777216) % _scaleFactor * _offsetSize;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteMapInfo()
	{
		JsonWriter jsonWriter = default(JsonWriter);
		jsonWriter.WriteBeginObject();
		jsonWriter.WritePropertyName("blockSize");
		jsonWriter.WriteInt32(Constants.MapBlockSize);
		jsonWriter.WriteValueSeparator();
		jsonWriter.WritePropertyName("maxZoom");
		jsonWriter.WriteInt32(Constants.Zoomlevels - 1);
		jsonWriter.WriteEndObject();
		Directory.CreateDirectory(Constants.MapDirectory);
		File.WriteAllBytes(Constants.MapDirectory + "/mapinfo.json", jsonWriter.ToUtf8ByteArray());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool LoadMapInfo()
	{
		if (!File.Exists(Constants.MapDirectory + "/mapinfo.json"))
		{
			return false;
		}
		string json = File.ReadAllText(Constants.MapDirectory + "/mapinfo.json", Encoding.UTF8);
		try
		{
			IDictionary<string, object> dictionary = JsonSerializer.Deserialize<IDictionary<string, object>>(json);
			if (dictionary.TryGetValue("blockSize", out var value) && value is double num)
			{
				Constants.MapBlockSize = (int)num;
			}
			if (dictionary.TryGetValue("maxZoom", out value) && value is double num2)
			{
				Constants.Zoomlevels = (int)num2 + 1;
			}
			return true;
		}
		catch (Exception arg)
		{
			Log.Out($"Exception in LoadMapInfo: {arg}");
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void getWorldExtent(RegionFileManager _rfm, out Vector2i _minChunk, out Vector2i _maxChunk, out Vector2i _minPos, out Vector2i _maxPos, out int _widthChunks, out int _heightChunks, out int _widthPix, out int _heightPix)
	{
		_minChunk = default(Vector2i);
		_maxChunk = default(Vector2i);
		_minPos = default(Vector2i);
		_maxPos = default(Vector2i);
		long[] allChunkKeys = _rfm.GetAllChunkKeys();
		int num = int.MaxValue;
		int num2 = int.MaxValue;
		int num3 = int.MinValue;
		int num4 = int.MinValue;
		long[] array = allChunkKeys;
		foreach (long key in array)
		{
			int num5 = WorldChunkCache.extractX(key);
			int num6 = WorldChunkCache.extractZ(key);
			if (num5 < num)
			{
				num = num5;
			}
			if (num5 > num3)
			{
				num3 = num5;
			}
			if (num6 < num2)
			{
				num2 = num6;
			}
			if (num6 > num4)
			{
				num4 = num6;
			}
		}
		_minChunk.x = num;
		_minChunk.y = num2;
		_maxChunk.x = num3;
		_maxChunk.y = num4;
		_minPos.x = num * 16;
		_minPos.y = num2 * 16;
		_maxPos.x = num3 * 16;
		_maxPos.y = num4 * 16;
		_widthChunks = num3 - num + 1;
		_heightChunks = num4 - num2 + 1;
		_widthPix = _widthChunks * 16;
		_heightPix = _heightChunks * 16;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color32 shortColorToColor32(ushort _col)
	{
		byte r = (byte)(256 * ((_col >> 10) & 0x1F) / 32);
		byte g = (byte)(256 * ((_col >> 5) & 0x1F) / 32);
		byte b = (byte)(256 * (_col & 0x1F) / 32);
		return new Color32(r, g, b, byte.MaxValue);
	}
}
