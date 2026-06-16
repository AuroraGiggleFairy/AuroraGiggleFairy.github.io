using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

public class DynamicMeshUnity
{
	public const int RegionSize = 160;

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<DynamicMeshRegion> tempRegions = new HashSet<DynamicMeshRegion>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<long> tempRegionKeys = new HashSet<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static (string path, HashSetLong keys) cachedDynamicMeshChunksList = (path: string.Empty, keys: new HashSetLong());

	public static int RoundChunk(int value)
	{
		if (value < 0)
		{
			value -= 15;
		}
		return value / 16 * 16;
	}

	public static int RoundChunk(float value)
	{
		return RoundChunk((int)value);
	}

	public static string GetItemPath(long key)
	{
		return DynamicMeshFile.MeshLocation + key + ".update";
	}

	public static int GetChunkPositionFromWorldPosition(float pos)
	{
		return RoundChunk((int)pos);
	}

	public static int GetChunkPositionFromWorldPosition(int pos)
	{
		return RoundChunk(pos);
	}

	public static long GetRegionKeyFromItemKey(long itemKey)
	{
		return GetRegionKeyFromWorldPosition(GetWorldPosFromKey(itemKey));
	}

	public static long GetRegionKeyFromWorldPosition(Vector3i pos)
	{
		return WorldChunkCache.MakeChunkKey(World.toChunkXZ(RoundRegion(pos.x)), World.toChunkXZ(RoundRegion(pos.z)));
	}

	public static Vector2 GetXZFromKey(long key)
	{
		return new Vector2(WorldChunkCache.extractX(key) * 16, WorldChunkCache.extractZ(key) * 16);
	}

	public static Vector3i GetWorldPosFromKey(long key)
	{
		return new Vector3i(WorldChunkCache.extractX(key) * 16, 0, WorldChunkCache.extractZ(key) * 16);
	}

	public static string GetDebugPositionFromKey(long key)
	{
		return $"{WorldChunkCache.extractX(key) * 16},{WorldChunkCache.extractZ(key) * 16}";
	}

	public static string GetDebugPositionKey(long key)
	{
		return $"{WorldChunkCache.extractX(key) * 16},{WorldChunkCache.extractZ(key) * 16}";
	}

	public static int GetChunkSectionX(long key)
	{
		return WorldChunkCache.extractX(key);
	}

	public static int GetChunkSectionZ(long key)
	{
		return WorldChunkCache.extractZ(key);
	}

	public static int GetWorldXFromKey(long key)
	{
		return WorldChunkCache.extractX(key) * 16;
	}

	public static int GetWorldZFromKey(long key)
	{
		return WorldChunkCache.extractZ(key) * 16;
	}

	public static long GetRegionKeyFromWorldPosition(int worldX, int worldZ)
	{
		return WorldChunkCache.MakeChunkKey(World.toChunkXZ(RoundRegion(worldX)), World.toChunkXZ(RoundRegion(worldZ)));
	}

	public static Vector3i GetRegionPositionFromWorldPosition(int worldX, int worldZ)
	{
		return new Vector3i(RoundRegion(worldX), 0, RoundRegion(worldZ));
	}

	public static Vector3i GetRegionPositionFromWorldPosition(Vector3i worldPos)
	{
		return new Vector3i(RoundRegion(worldPos.x), 0, RoundRegion(worldPos.z));
	}

	public static int RoundRegion(int value)
	{
		if (value < 0)
		{
			value -= 159;
		}
		return value / 160 * 160;
	}

	public static int RoundRegion(float value)
	{
		return RoundRegion((int)value);
	}

	public static Vector3i GetRegionPositionFromWorldPosition(Vector3 worldPos)
	{
		return new Vector3i(RoundRegion(worldPos.x), 0, RoundRegion(worldPos.z));
	}

	public static long GetItemKey(int worldX, int worldZ)
	{
		return WorldChunkCache.MakeChunkKey(World.toChunkXZ(worldX), World.toChunkXZ(worldZ));
	}

	public static int GetItemPosition(int pos)
	{
		return World.toChunkXZ(pos) * 16;
	}

	public static float Distance(Vector3i a, Vector3i b)
	{
		return Mathf.Abs(Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2f) + Mathf.Pow(a.z - b.z, 2f)));
	}

	public static float Distance(Vector3i a, Vector3 b)
	{
		return Mathf.Abs(Mathf.Sqrt(Mathf.Pow((float)a.x - b.x, 2f) + Mathf.Pow((float)a.z - b.z, 2f)));
	}

	public static float Distance(int x1, int y1, int x2, int y2)
	{
		return Mathf.Abs(Mathf.Sqrt(Mathf.Pow(x1 - x2, 2f) + Mathf.Pow(y1 - y2, 2f)));
	}

	public static void log(string msg)
	{
		if (DynamicMeshManager.ShowDebug && DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg(msg);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogMsg(string msg)
	{
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg(msg);
		}
	}

	public static Vector3i GetChunkPositionFromWorldPosition(Vector3i worldPosition)
	{
		return new Vector3i(RoundChunk(worldPosition.x), worldPosition.y, RoundChunk(worldPosition.z));
	}

	public static bool IsInBuffer(float x, float z, int bufferSize, int xIndex, int zIndex)
	{
		int num = (int)(x / 160f);
		int num2 = (int)(z / 160f);
		if (x < 0f)
		{
			num--;
		}
		if (z < 0f)
		{
			num2--;
		}
		if (zIndex >= num2 - bufferSize && zIndex <= num2 + bufferSize && xIndex >= num - bufferSize)
		{
			return xIndex <= num + bufferSize;
		}
		return false;
	}

	public static void WaitCoroutine(IEnumerator func)
	{
		while (func.MoveNext())
		{
			if (func.Current == null)
			{
				continue;
			}
			IEnumerator func2;
			try
			{
				func2 = (IEnumerator)func.Current;
			}
			catch (InvalidCastException)
			{
				if (func.Current.GetType() == typeof(WaitForSeconds))
				{
					Log.Warning("Skipped call to WaitForSeconds. Use WaitForSecondsRealtime instead.");
				}
				break;
			}
			WaitCoroutine(func2);
		}
	}

	public static long GetMeshSize(GameObject go)
	{
		long num = 0L;
		MeshFilter component = go.GetComponent<MeshFilter>();
		if (component != null)
		{
			Mesh sharedMesh = component.sharedMesh;
			if (sharedMesh != null)
			{
				num += Profiler.GetRuntimeMemorySizeLong(sharedMesh);
			}
		}
		foreach (Transform item in go.transform)
		{
			num += GetMeshSize(item.gameObject);
		}
		return num;
	}

	public static Bounds GetBoundsFromVertsJustY(ArrayListMP<Vector3> verts, Bounds bounds)
	{
		float num = verts[0].y;
		float num2 = verts[0].y;
		for (int i = 0; i < verts.Count; i++)
		{
			Vector3 vector = verts[i];
			num = Math.Min(num, vector.y);
			num2 = Math.Max(num2, vector.y);
		}
		Vector3 min = new Vector3(0f, num, 0f);
		Vector3 max = new Vector3(0f, num2, 0f);
		bounds.SetMinMax(min, max);
		return bounds;
	}

	public static void DeleteDynamicMeshData(ICollection<long> chunks)
	{
		GetOrCreateDynamicMeshChunksList(out var meshLocation, out var keys);
		if (!keys.Overlaps(chunks))
		{
			return;
		}
		if (DynamicMeshManager.Instance != null && GamePrefs.GetBool(EnumGamePrefs.DynamicMeshEnabled))
		{
			tempRegions.Clear();
			foreach (long chunk in chunks)
			{
				if (!keys.Contains(chunk))
				{
					continue;
				}
				DynamicMeshItem dynamicMeshItem = DynamicMeshManager.Instance?.GetItemOrNull(chunk);
				if (dynamicMeshItem == null)
				{
					Log.Error($"Failed to retrieve valid DynamicMeshItem for cached dynamic mesh chunk key: {chunk}.");
					continue;
				}
				DynamicMeshManager.Instance.RemoveItem(dynamicMeshItem, removedFromWorld: true);
				string itemPath = GetItemPath(chunk);
				if (SdFile.Exists(itemPath))
				{
					SdFile.Delete(itemPath);
				}
				DynamicMeshRegion region = dynamicMeshItem.GetRegion();
				tempRegions.Add(region);
			}
			foreach (DynamicMeshRegion tempRegion in tempRegions)
			{
				tempRegion.CleanUp();
				if (tempRegion.LoadedItems.Count == 0 && tempRegion.UnloadedItems.Count == 0)
				{
					string path = tempRegion.Path;
					if (SdFile.Exists(path))
					{
						SdFile.Delete(path);
					}
				}
				else
				{
					DynamicMeshThread.AddRegionUpdateData(tempRegion.WorldPosition.x, tempRegion.WorldPosition.z, isUrgent: true);
				}
			}
			tempRegions.Clear();
			return;
		}
		tempRegionKeys.Clear();
		foreach (long chunk2 in chunks)
		{
			if (keys.Contains(chunk2))
			{
				string path2 = meshLocation + chunk2 + ".update";
				if (SdFile.Exists(path2))
				{
					SdFile.Delete(path2);
				}
				int value = WorldChunkCache.extractX(chunk2) * 16;
				long item = WorldChunkCache.MakeChunkKey(y: World.toChunkXZ(RoundRegion(WorldChunkCache.extractZ(chunk2) * 16)), x: World.toChunkXZ(RoundRegion(value)));
				tempRegionKeys.Add(item);
				keys.Remove(chunk2);
			}
		}
		foreach (long tempRegionKey in tempRegionKeys)
		{
			string path3 = meshLocation + tempRegionKey + ".group";
			if (SdFile.Exists(path3))
			{
				SdFile.Delete(path3);
			}
		}
		tempRegionKeys.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void GetOrCreateDynamicMeshChunksList(out string meshLocation, out HashSetLong keys)
	{
		string text = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? GameIO.GetSaveGameDir() : GameIO.GetSaveGameLocalDir());
		if (!cachedDynamicMeshChunksList.path.StartsWith(text, StringComparison.InvariantCultureIgnoreCase))
		{
			string path = (cachedDynamicMeshChunksList.path = text + "/DynamicMeshes/");
			cachedDynamicMeshChunksList.keys.Clear();
			SdDirectoryInfo sdDirectoryInfo = new SdDirectoryInfo(path);
			if (sdDirectoryInfo.Exists)
			{
				foreach (SdFileInfo item in sdDirectoryInfo.EnumerateFiles("*.update", SearchOption.TopDirectoryOnly))
				{
					if (long.TryParse(Path.GetFileNameWithoutExtension(item.Name), out var result))
					{
						cachedDynamicMeshChunksList.keys.Add(result);
					}
				}
			}
		}
		meshLocation = cachedDynamicMeshChunksList.path;
		keys = cachedDynamicMeshChunksList.keys;
	}

	public static void AddDisabledImposterChunk(long key)
	{
		GetOrCreateDynamicMeshChunksList(out var _, out var keys);
		keys.Add(key);
	}

	public static void RemoveDisabledImposterChunk(long key)
	{
		GetOrCreateDynamicMeshChunksList(out var _, out var keys);
		keys.Remove(key);
	}

	public static void ClearCachedDynamicMeshChunksList()
	{
		cachedDynamicMeshChunksList.path = string.Empty;
		cachedDynamicMeshChunksList.keys.Clear();
	}

	[Conditional("UNITY_STANDALONE")]
	public static void EnsureDMDirectoryExists()
	{
		if (!SdDirectory.Exists(DynamicMeshFile.MeshLocation))
		{
			SdDirectory.CreateDirectory(DynamicMeshFile.MeshLocation);
		}
	}
}
