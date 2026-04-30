using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Noemax.GZip;
using UnityEngine;

public static class DynamicMeshVoxel
{
	public static void QefToFile(string path, string folderOut, string filename)
	{
		string[] array = SdFile.ReadAllLines(path);
		string[] array2 = array[3].Split(' ');
		int num = int.Parse(array2[0]);
		int num2 = int.Parse(array2[1]);
		int num3 = int.Parse(array2[2]);
		string path2 = folderOut + filename + ".7mesh";
		using (Stream output = SdFile.Open(path2, FileMode.Create, FileAccess.Write, FileShare.Read))
		{
			using DeflateOutputStream baseStream = new DeflateOutputStream(output, 3, leaveOpen: false);
			using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
			pooledBinaryWriter.SetBaseStream(baseStream);
			pooledBinaryWriter.Write((byte)num);
			pooledBinaryWriter.Write((byte)num2);
			pooledBinaryWriter.Write((byte)num3);
			for (int i = 6; i < array.Length; i++)
			{
				string text = array[i];
				if (!string.IsNullOrWhiteSpace(text))
				{
					string[] array3 = text.Split(' ');
					byte value = byte.Parse(array3[0]);
					byte value2 = byte.Parse(array3[1]);
					byte value3 = byte.Parse(array3[2]);
					pooledBinaryWriter.Write(value);
					pooledBinaryWriter.Write(value2);
					pooledBinaryWriter.Write(value3);
				}
			}
		}
		string contents = Convert.ToBase64String(SdFile.ReadAllBytes(path2));
		SdFile.WriteAllText(folderOut + filename + ".7base", contents);
	}

	public static string ToDebugLocation(this Vector3i pos)
	{
		return $"{pos.x},{pos.z}";
	}

	public static int GetByteLength(this VoxelMesh m)
	{
		if (m.Vertices.Count == 0)
		{
			return 0;
		}
		int num = (DynamicMeshManager.Allow32BitMeshes ? int.MaxValue : 65535);
		int num2 = ((m.Vertices.Count > 65535) ? 4 : 2);
		bool flag = m.Vertices.Count > num;
		int num3 = 4 + (flag ? num : m.Vertices.Count) * 6 + 4 + (flag ? num : m.Vertices.Count) * 8 + 4 + (flag ? num : m.Indices.Count) * num2;
		if (flag)
		{
			num3 = num3 + 4 + (m.Vertices.Count - num) * 6 + 4 + (m.Vertices.Count - num) * 8 + 4 + (m.Indices.Count - num) * num2;
		}
		return num3;
	}

	public static int GetByteLength(this VoxelMeshTerrain m)
	{
		if (m.Vertices.Count == 0)
		{
			return 0;
		}
		int num = (DynamicMeshManager.Allow32BitMeshes ? int.MaxValue : 65535);
		bool flag = m.Vertices.Count > num;
		return 4 + (flag ? num : m.Vertices.Count) * 6 + 4 + (flag ? num : m.Vertices.Count) * 8 + 4 + 4 * m.submeshes.Count + (flag ? num : m.submeshes.Sum([PublicizedFrom(EAccessModifier.Internal)] (TerrainSubMesh d) => d.triangles.Count)) * 4 + m.ColorVertices.Count * 16;
	}

	public static Transform FindRecursive(this Transform t, string name)
	{
		Queue<Transform> queue = new Queue<Transform>();
		queue.Enqueue(t);
		while (queue.Count > 0)
		{
			Transform transform = queue.Dequeue();
			if (transform.name.EqualsCaseInsensitive(name))
			{
				return transform;
			}
			foreach (Transform item in transform)
			{
				queue.Enqueue(item);
			}
		}
		return null;
	}

	public static Transform FindParent(this Transform t, string name)
	{
		Queue<Transform> queue = new Queue<Transform>();
		queue.Enqueue(t);
		while (queue.Count > 0)
		{
			Transform transform = queue.Dequeue();
			if (transform.name.EqualsCaseInsensitive(name))
			{
				return transform;
			}
			if (t.parent != null)
			{
				queue.Enqueue(t.parent);
			}
		}
		return null;
	}
}
