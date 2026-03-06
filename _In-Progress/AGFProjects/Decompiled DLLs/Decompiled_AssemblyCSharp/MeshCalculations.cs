using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public static class MeshCalculations
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct VertexKey(Vector3 position)
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly long _x = (long)Mathf.Round(position.x * 100000f);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly long _y = (long)Mathf.Round(position.y * 100000f);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly long _z = (long)Mathf.Round(position.z * 100000f);

		[PublicizedFrom(EAccessModifier.Private)]
		public const int Tolerance = 100000;

		[PublicizedFrom(EAccessModifier.Private)]
		public const long FNV32Init = 2166136261L;

		[PublicizedFrom(EAccessModifier.Private)]
		public const long FNV32Prime = 16777619L;

		public override bool Equals(object obj)
		{
			VertexKey vertexKey = (VertexKey)obj;
			if (_x == vertexKey._x && _y == vertexKey._y)
			{
				return _z == vertexKey._z;
			}
			return false;
		}

		public override int GetHashCode()
		{
			long num = 2166136261L;
			num ^= _x;
			num *= 16777619;
			num ^= _y;
			num *= 16777619;
			num ^= _z;
			return (num * 16777619).GetHashCode();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct VertexEntry(int meshIndex, int triIndex, int vertIndex)
	{
		public int MeshIndex = meshIndex;

		public int TriangleIndex = triIndex;

		public int VertexIndex = vertIndex;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static object _lock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<VertexKey, List<VertexEntry>> dictionary;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ConcurrentQueue<List<VertexEntry>> VertexEntries = new ConcurrentQueue<List<VertexEntry>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static ConcurrentDictionary<int, ConcurrentQueue<List<VertexEntry>>> Cache = new ConcurrentDictionary<int, ConcurrentQueue<List<VertexEntry>>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<VertexEntry> GetCacheBySize(int capacity)
	{
		int num = 1024;
		int key = capacity / num + num;
		if (!Cache.TryGetValue(key, out var value))
		{
			value = new ConcurrentQueue<List<VertexEntry>>();
			Cache.TryAdd(key, value);
		}
		if (!value.TryDequeue(out var result))
		{
			return new List<VertexEntry>(num);
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AddToCacheBySize(List<VertexEntry> list)
	{
		int num = 1024;
		int key = list.Capacity / num + num;
		if (list.Capacity % num != 0)
		{
			Log.Warning("list not divisible by keysize: " + num + " vs " + list.Capacity);
		}
		if (!Cache.TryGetValue(key, out var value))
		{
			value = new ConcurrentQueue<List<VertexEntry>>();
			Cache.TryAdd(key, value);
		}
		value.Enqueue(list);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<VertexEntry> GetVertexEntryList()
	{
		if (!VertexEntries.TryDequeue(out var result))
		{
			return new List<VertexEntry>(4);
		}
		return result;
	}

	public static void CalculateMeshTangents(ArrayListMP<Vector3> _vertices, ArrayListMP<int> _indices, ArrayListMP<Vector3> _normals, ArrayListMP<Vector2> _uvs, ArrayListMP<Vector4> _tangents, bool _bIgnoreUvs = false)
	{
		lock (_lock)
		{
			int count = _indices.Count;
			int count2 = _vertices.Count;
			_tangents.Grow(count2);
			_tangents.Count = count2;
			Vector3[] array = null;
			Vector3[] array2 = null;
			if (array == null || count2 >= array.Length)
			{
				Log.Out("Increasing tangent cache: " + count2);
				array = new Vector3[count2 + 1];
				array2 = new Vector3[count2 + 1];
			}
			else
			{
				for (int i = 0; i < count2; i++)
				{
					array[i].Set(0f, 0f, 0f);
					array2[i].Set(0f, 0f, 0f);
				}
			}
			for (int j = 0; j < count; j += 3)
			{
				int num = _indices[j];
				int num2 = _indices[j + 1];
				int num3 = _indices[j + 2];
				Vector3 vector = _vertices[num];
				Vector3 vector2 = _vertices[num2];
				Vector3 vector3 = _vertices[num3];
				Vector2 vector4;
				Vector2 vector5;
				Vector2 vector6;
				if (_bIgnoreUvs)
				{
					vector4 = Vector2.zero;
					vector5 = Vector2.zero;
					vector6 = Vector2.zero;
				}
				else
				{
					vector4 = _uvs[num];
					vector5 = _uvs[num2];
					vector6 = _uvs[num3];
				}
				float num4 = vector2.x - vector.x;
				float num5 = vector3.x - vector.x;
				float num6 = vector2.y - vector.y;
				float num7 = vector3.y - vector.y;
				float num8 = vector2.z - vector.z;
				float num9 = vector3.z - vector.z;
				float num10 = vector5.x - vector4.x;
				float num11 = vector6.x - vector4.x;
				float num12 = vector5.y - vector4.y;
				float num13 = vector6.y - vector4.y;
				float num14 = num10 * num13 - num11 * num12;
				float num15 = ((num14 == 0f) ? 0f : (1f / num14));
				float num16 = (num13 * num4 - num12 * num5) * num15;
				float num17 = (num13 * num6 - num12 * num7) * num15;
				float num18 = (num13 * num8 - num12 * num9) * num15;
				float num19 = (num10 * num5 - num11 * num4) * num15;
				float num20 = (num10 * num7 - num11 * num6) * num15;
				float num21 = (num10 * num9 - num11 * num8) * num15;
				array[num].x += num16;
				array[num].y += num17;
				array[num].z += num18;
				array[num2].x += num16;
				array[num2].y += num17;
				array[num2].z += num18;
				array[num3].x += num16;
				array[num3].y += num17;
				array[num3].z += num18;
				array2[num].x += num19;
				array2[num].y += num20;
				array2[num].z += num21;
				array2[num2].x += num19;
				array2[num2].y += num20;
				array2[num2].z += num21;
				array2[num3].x += num19;
				array2[num3].y += num20;
				array2[num3].z += num21;
			}
			for (int k = 0; k < count2; k++)
			{
				Vector3 normal = _normals[k];
				Vector3 tangent = array[k];
				Vector3.OrthoNormalize(ref normal, ref tangent);
				Vector4 item = new Vector4(tangent.x, tangent.y, tangent.z, (Vector3.Dot(Vector3.Cross(normal, tangent), array2[k]) < 0f) ? (-1f) : 1f);
				_tangents.Add(item);
			}
		}
	}

	public static void CalculateMeshTangents(List<Vector3> _vertices, List<int> _indices, List<Vector3> _normals, List<Vector2> _uvs, List<Vector4> _tangents, bool _bIgnoreUvs = false)
	{
		lock (_lock)
		{
			int count = _indices.Count;
			int count2 = _vertices.Count;
			_tangents.Capacity = Math.Max(_tangents.Capacity, count2);
			_tangents.Clear();
			Vector3[] array = null;
			Vector3[] array2 = null;
			if (array == null || count2 >= array.Length)
			{
				Log.Out("Increasing tangent cache: " + count2);
				array = new Vector3[count2 + 1];
				array2 = new Vector3[count2 + 1];
			}
			else
			{
				for (int i = 0; i < count2; i++)
				{
					array[i].Set(0f, 0f, 0f);
					array2[i].Set(0f, 0f, 0f);
				}
			}
			for (int j = 0; j < count; j += 3)
			{
				int num = _indices[j];
				int num2 = _indices[j + 1];
				int num3 = _indices[j + 2];
				Vector3 vector = _vertices[num];
				Vector3 vector2 = _vertices[num2];
				Vector3 vector3 = _vertices[num3];
				Vector2 vector4;
				Vector2 vector5;
				Vector2 vector6;
				if (_bIgnoreUvs)
				{
					vector4 = Vector2.zero;
					vector5 = Vector2.zero;
					vector6 = Vector2.zero;
				}
				else
				{
					vector4 = _uvs[num];
					vector5 = _uvs[num2];
					vector6 = _uvs[num3];
				}
				float num4 = vector2.x - vector.x;
				float num5 = vector3.x - vector.x;
				float num6 = vector2.y - vector.y;
				float num7 = vector3.y - vector.y;
				float num8 = vector2.z - vector.z;
				float num9 = vector3.z - vector.z;
				float num10 = vector5.x - vector4.x;
				float num11 = vector6.x - vector4.x;
				float num12 = vector5.y - vector4.y;
				float num13 = vector6.y - vector4.y;
				float num14 = num10 * num13 - num11 * num12;
				float num15 = ((num14 == 0f) ? 0f : (1f / num14));
				float num16 = (num13 * num4 - num12 * num5) * num15;
				float num17 = (num13 * num6 - num12 * num7) * num15;
				float num18 = (num13 * num8 - num12 * num9) * num15;
				float num19 = (num10 * num5 - num11 * num4) * num15;
				float num20 = (num10 * num7 - num11 * num6) * num15;
				float num21 = (num10 * num9 - num11 * num8) * num15;
				array[num].x += num16;
				array[num].y += num17;
				array[num].z += num18;
				array[num2].x += num16;
				array[num2].y += num17;
				array[num2].z += num18;
				array[num3].x += num16;
				array[num3].y += num17;
				array[num3].z += num18;
				array2[num].x += num19;
				array2[num].y += num20;
				array2[num].z += num21;
				array2[num2].x += num19;
				array2[num2].y += num20;
				array2[num2].z += num21;
				array2[num3].x += num19;
				array2[num3].y += num20;
				array2[num3].z += num21;
			}
			for (int k = 0; k < count2; k++)
			{
				Vector3 normal = _normals[k];
				Vector3 tangent = array[k];
				Vector3.OrthoNormalize(ref normal, ref tangent);
				Vector4 item = new Vector4(tangent.x, tangent.y, tangent.z, (Vector3.Dot(Vector3.Cross(normal, tangent), array2[k]) < 0f) ? (-1f) : 1f);
				_tangents.Add(item);
			}
		}
	}

	public static List<Vector4> CalculateMeshTangents(Vector3[] _vertices, int[] _indices, Vector3[] _normals, Vector2[] _uvs, bool _bIgnoreUvs = false)
	{
		lock (_lock)
		{
			int num = _indices.Length;
			int num2 = _vertices.Length;
			List<Vector4> list = new List<Vector4>(_vertices.Length);
			Vector3[] array = null;
			Vector3[] array2 = null;
			if (array == null || num2 >= array.Length)
			{
				Log.Out("Increasing tangent cache: " + num2);
				array = new Vector3[num2 + 1];
				array2 = new Vector3[num2 + 1];
			}
			else
			{
				for (int i = 0; i < num2; i++)
				{
					array[i].Set(0f, 0f, 0f);
					array2[i].Set(0f, 0f, 0f);
				}
			}
			for (int j = 0; j < num; j += 3)
			{
				int num3 = _indices[j];
				int num4 = _indices[j + 1];
				int num5 = _indices[j + 2];
				Vector3 vector = _vertices[num3];
				Vector3 vector2 = _vertices[num4];
				Vector3 vector3 = _vertices[num5];
				Vector2 vector4;
				Vector2 vector5;
				Vector2 vector6;
				if (_bIgnoreUvs)
				{
					vector4 = Vector2.zero;
					vector5 = Vector2.zero;
					vector6 = Vector2.zero;
				}
				else
				{
					vector4 = _uvs[num3];
					vector5 = _uvs[num4];
					vector6 = _uvs[num5];
				}
				float num6 = vector2.x - vector.x;
				float num7 = vector3.x - vector.x;
				float num8 = vector2.y - vector.y;
				float num9 = vector3.y - vector.y;
				float num10 = vector2.z - vector.z;
				float num11 = vector3.z - vector.z;
				float num12 = vector5.x - vector4.x;
				float num13 = vector6.x - vector4.x;
				float num14 = vector5.y - vector4.y;
				float num15 = vector6.y - vector4.y;
				float num16 = num12 * num15 - num13 * num14;
				float num17 = ((num16 == 0f) ? 0f : (1f / num16));
				float num18 = (num15 * num6 - num14 * num7) * num17;
				float num19 = (num15 * num8 - num14 * num9) * num17;
				float num20 = (num15 * num10 - num14 * num11) * num17;
				float num21 = (num12 * num7 - num13 * num6) * num17;
				float num22 = (num12 * num9 - num13 * num8) * num17;
				float num23 = (num12 * num11 - num13 * num10) * num17;
				array[num3].x += num18;
				array[num3].y += num19;
				array[num3].z += num20;
				array[num4].x += num18;
				array[num4].y += num19;
				array[num4].z += num20;
				array[num5].x += num18;
				array[num5].y += num19;
				array[num5].z += num20;
				array2[num3].x += num21;
				array2[num3].y += num22;
				array2[num3].z += num23;
				array2[num4].x += num21;
				array2[num4].y += num22;
				array2[num4].z += num23;
				array2[num5].x += num21;
				array2[num5].y += num22;
				array2[num5].z += num23;
			}
			for (int k = 0; k < num2; k++)
			{
				Vector3 normal = _normals[k];
				Vector3 tangent = array[k];
				Vector3.OrthoNormalize(ref normal, ref tangent);
				Vector4 item = new Vector4(tangent.x, tangent.y, tangent.z, (Vector3.Dot(Vector3.Cross(normal, tangent), array2[k]) < 0f) ? (-1f) : 1f);
				list.Add(item);
			}
			return list;
		}
	}

	public static void RecalculateNormals(ArrayListMP<Vector3> _vertices, ArrayListMP<int> _indices, ArrayListMP<Vector3> normals, float angle = 60f)
	{
		lock (_lock)
		{
			float num = Mathf.Cos(angle * (MathF.PI / 180f));
			normals.Grow(_vertices.Count);
			normals.Count = _vertices.Count;
			int num2 = 1;
			if (MeshCalculations.dictionary == null)
			{
				MeshCalculations.dictionary = new Dictionary<VertexKey, List<VertexEntry>>(_vertices.Count);
			}
			List<List<Vector3>> list = null;
			Dictionary<VertexKey, List<VertexEntry>> dictionary = MeshCalculations.dictionary;
			if (list == null)
			{
				list = new List<List<Vector3>>();
			}
			for (int i = list.Count; i < num2; i++)
			{
				list.Add(new List<Vector3>());
			}
			for (int j = 0; j < num2; j++)
			{
				list[j].Capacity = Math.Max(_indices.Count / 3, list[j].Capacity);
				for (int k = list[j].Count; k < list[j].Capacity; k++)
				{
					list[j].Add(default(Vector3));
				}
				for (int l = 0; l < _indices.Count; l += 3)
				{
					int num3 = _indices[l];
					int num4 = _indices[l + 1];
					int num5 = _indices[l + 2];
					Vector3 lhs = _vertices[num4] - _vertices[num3];
					Vector3 rhs = _vertices[num5] - _vertices[num3];
					Vector3 normalized = Vector3.Cross(lhs, rhs).normalized;
					int num6 = l / 3;
					list[j][num6] = normalized;
					VertexKey key = new VertexKey(_vertices[num3]);
					if (!dictionary.TryGetValue(key, out var value))
					{
						value = GetVertexEntryList();
						dictionary.Add(key, value);
					}
					value.Add(new VertexEntry(j, num6, num3));
					key = new VertexKey(_vertices[num4]);
					if (!dictionary.TryGetValue(key, out value))
					{
						value = GetVertexEntryList();
						dictionary.Add(key, value);
					}
					value.Add(new VertexEntry(j, num6, num4));
					key = new VertexKey(_vertices[num5]);
					if (!dictionary.TryGetValue(key, out value))
					{
						value = GetVertexEntryList();
						dictionary.Add(key, value);
					}
					value.Add(new VertexEntry(j, num6, num5));
				}
			}
			foreach (List<VertexEntry> value2 in dictionary.Values)
			{
				for (int m = 0; m < value2.Count; m++)
				{
					Vector3 vector = default(Vector3);
					VertexEntry vertexEntry = value2[m];
					for (int n = 0; n < value2.Count; n++)
					{
						VertexEntry vertexEntry2 = value2[n];
						if (vertexEntry.VertexIndex == vertexEntry2.VertexIndex)
						{
							vector += list[vertexEntry2.MeshIndex][vertexEntry2.TriangleIndex];
						}
						else if (Vector3.Dot(list[vertexEntry.MeshIndex][vertexEntry.TriangleIndex], list[vertexEntry2.MeshIndex][vertexEntry2.TriangleIndex]) >= num)
						{
							vector += list[vertexEntry2.MeshIndex][vertexEntry2.TriangleIndex];
						}
					}
					normals[vertexEntry.VertexIndex] = vector.normalized;
				}
			}
			dictionary.Clear();
		}
	}

	public static List<Vector3> RecalculateNormals(Vector3[] _vertices, int[] _indices, float angle = 60f)
	{
		lock (_lock)
		{
			float num = Mathf.Cos(angle * (MathF.PI / 180f));
			List<Vector3> list = new List<Vector3>(_vertices);
			int num2 = 1;
			if (MeshCalculations.dictionary == null)
			{
				MeshCalculations.dictionary = new Dictionary<VertexKey, List<VertexEntry>>(_vertices.Length);
			}
			Dictionary<VertexKey, List<VertexEntry>> dictionary = MeshCalculations.dictionary;
			List<List<Vector3>> list2 = null;
			if (list2 == null)
			{
				list2 = new List<List<Vector3>>();
			}
			for (int i = list2.Count; i < num2; i++)
			{
				list2.Add(new List<Vector3>());
			}
			for (int j = 0; j < num2; j++)
			{
				list2[j].Capacity = Math.Max(_indices.Length / 3, list2[j].Capacity);
				for (int k = list2[j].Count; k < list2[j].Capacity; k++)
				{
					list2[j].Add(default(Vector3));
				}
				for (int l = 0; l < _indices.Length; l += 3)
				{
					int num3 = _indices[l];
					int num4 = _indices[l + 1];
					int num5 = _indices[l + 2];
					Vector3 lhs = _vertices[num4] - _vertices[num3];
					Vector3 rhs = _vertices[num5] - _vertices[num3];
					Vector3 normalized = Vector3.Cross(lhs, rhs).normalized;
					int num6 = l / 3;
					list2[j][num6] = normalized;
					VertexKey key = new VertexKey(_vertices[num3]);
					if (!dictionary.TryGetValue(key, out var value))
					{
						value = GetVertexEntryList();
						dictionary.Add(key, value);
					}
					value.Add(new VertexEntry(j, num6, num3));
					key = new VertexKey(_vertices[num4]);
					if (!dictionary.TryGetValue(key, out value))
					{
						value = GetVertexEntryList();
						dictionary.Add(key, value);
					}
					value.Add(new VertexEntry(j, num6, num4));
					key = new VertexKey(_vertices[num5]);
					if (!dictionary.TryGetValue(key, out value))
					{
						value = GetVertexEntryList();
						dictionary.Add(key, value);
					}
					value.Add(new VertexEntry(j, num6, num5));
				}
			}
			foreach (List<VertexEntry> value2 in dictionary.Values)
			{
				for (int m = 0; m < value2.Count; m++)
				{
					Vector3 vector = default(Vector3);
					VertexEntry vertexEntry = value2[m];
					for (int n = 0; n < value2.Count; n++)
					{
						VertexEntry vertexEntry2 = value2[n];
						if (vertexEntry.VertexIndex == vertexEntry2.VertexIndex)
						{
							vector += list2[vertexEntry2.MeshIndex][vertexEntry2.TriangleIndex];
						}
						else if (Vector3.Dot(list2[vertexEntry.MeshIndex][vertexEntry.TriangleIndex], list2[vertexEntry2.MeshIndex][vertexEntry2.TriangleIndex]) >= num)
						{
							vector += list2[vertexEntry2.MeshIndex][vertexEntry2.TriangleIndex];
						}
					}
					list[vertexEntry.VertexIndex] = vector.normalized;
				}
			}
			dictionary.Clear();
			return list;
		}
	}
}
