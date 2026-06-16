using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling;
using Unity.Profiling.LowLevel;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;

public static class ProfilerUtils
{
	public static string GetAvailableMetricsCsv()
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<ProfilerRecorderHandle> list = new List<ProfilerRecorderHandle>();
		ProfilerRecorderHandle.GetAvailable(list);
		stringBuilder.AppendLine("Name,Category,DataType,UnitType");
		foreach (ProfilerRecorderHandle item in list)
		{
			ProfilerRecorderDescription description = ProfilerRecorderHandle.GetDescription(item);
			stringBuilder.AppendFormat("{0},{1},{2},{3}", description.Name, description.Category.Name, Enum.GetName(typeof(ProfilerMarkerDataType), description.DataType), Enum.GetName(typeof(ProfilerMarkerDataUnit), description.UnitType));
			stringBuilder.AppendLine();
		}
		return stringBuilder.ToString();
	}

	public static void AppendLastValue(ProfilerRecorder recorder, StringBuilder builder)
	{
		switch (recorder.UnitType)
		{
		case ProfilerMarkerDataUnit.Undefined:
			builder.Append(recorder.LastValue);
			break;
		case ProfilerMarkerDataUnit.TimeNanoseconds:
			builder.AppendFormat("{0:F2}", recorder.LastValueAsDouble * 1E-06);
			break;
		case ProfilerMarkerDataUnit.Bytes:
			builder.AppendFormat("{0:F2}", recorder.LastValueAsDouble * 9.5367431640625E-07);
			break;
		case ProfilerMarkerDataUnit.Count:
			builder.Append(recorder.LastValue);
			break;
		case ProfilerMarkerDataUnit.Percent:
			builder.AppendFormat("{0:F2}", recorder.LastValueAsDouble);
			break;
		case ProfilerMarkerDataUnit.FrequencyHz:
			builder.Append(recorder.LastValueAsDouble);
			break;
		}
	}

	public static bool AreMipMapsStreamed(this Texture2D texture)
	{
		if (texture.streamingMipmaps)
		{
			return !texture.isReadable;
		}
		return false;
	}

	public static bool AreMipMapsStreamed(this Cubemap texture)
	{
		if (texture.streamingMipmaps)
		{
			return !texture.isReadable;
		}
		return false;
	}

	public static int CalculateTextureSizeBytes(Texture2D texture, int mipLevel = 0)
	{
		int num = CalculateTextureSizeBytes(texture.format, texture.width, texture.height, texture.mipmapCount, mipLevel);
		if (num < 0)
		{
			Debug.LogErrorFormat("Calculating size for {0}: unsupported texture type {1}", texture.name, texture.format);
		}
		return num;
	}

	public static int CalculateTextureSizeBytes(Cubemap texture, int mipLevel = 0)
	{
		int num = CalculateTextureSizeBytes(texture.format, texture.width, texture.height, texture.mipmapCount, mipLevel);
		if (num < 0)
		{
			Debug.LogErrorFormat("Calculating size for {0}: unsupported texture type {1}", texture.name, texture.format);
		}
		return num * 6;
	}

	public static int CalculateTextureSizeBytes(TextureFormat format, int width, int height, int mipCount, int mipLevel = 0)
	{
		int num = 0;
		mipLevel = Math.Max(mipLevel, 0);
		mipLevel = Math.Min(mipLevel, mipCount - 1);
		for (int i = mipLevel; i < mipCount; i++)
		{
			int num2 = 1 << i;
			num += CalculateTextureSizeBytes(format, width / num2, height / num2);
		}
		return num;
	}

	public static int CalculateTextureSizeBytes(TextureFormat format, int width, int height)
	{
		int num = width * height;
		switch (format)
		{
		case TextureFormat.Alpha8:
			return num;
		case TextureFormat.RGB24:
			return num * 3;
		case TextureFormat.RGBA32:
		case TextureFormat.ARGB32:
		case TextureFormat.BGRA32:
			return num * 4;
		case TextureFormat.RGBAFloat:
			return num * 16;
		case TextureFormat.RGBAHalf:
			return num * 8;
		case TextureFormat.ARGB4444:
		case TextureFormat.RGBA4444:
			return num * 2;
		case TextureFormat.R16:
			return num * 2;
		case TextureFormat.DXT1:
			return Mathf.CeilToInt(0.5f * (float)num);
		case TextureFormat.DXT5:
			return num;
		case TextureFormat.BC7:
			return num;
		default:
			return -1;
		}
	}

	public static int CalculateUnsafeParallelHashMapBytes<TKey, TValue>(UnsafeParallelHashMap<TKey, TValue> hashMap) where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
	{
		return CalculateUnsafeParallelHashMapBytes<TKey, TValue>(hashMap.Capacity);
	}

	public static int CalculateUnsafeParallelHashSetBytes<TKey>(UnsafeParallelHashSet<TKey> hashSet) where TKey : unmanaged, IEquatable<TKey>
	{
		return CalculateUnsafeParallelHashMapBytes<TKey, bool>(hashSet.Capacity);
	}

	public static int CalculateUnsafeParallelMultiHashMapBytes<TKey, TValue>(UnsafeParallelMultiHashMap<TKey, TValue> hashMap) where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
	{
		return CalculateUnsafeParallelHashMapBytes<TKey, TValue>(hashMap.Capacity);
	}

	public static int CalculateUnsafeParallelHashMapBytes<TKey, TValue>(int length) where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
	{
		int num = UnsafeUtility.SizeOf<UnsafeParallelHashMap<TKey, TValue>>() + 8256;
		int num2 = length * 2;
		int num3 = UnsafeUtility.SizeOf<TValue>();
		int num4 = UnsafeUtility.SizeOf<TKey>();
		int num5 = UnsafeUtility.SizeOf<int>();
		int num6 = CollectionHelper.Align(num3 * length, 64);
		int num7 = CollectionHelper.Align(num4 * length, 64);
		int num8 = CollectionHelper.Align(num5 * length, 64);
		int num9 = CollectionHelper.Align(num5 * num2, 64);
		return num + (num6 + num7 + num8 + num9);
	}

	public static int CalculateUnsafeBitArrayBytes(UnsafeBitArray bitArray)
	{
		return AlignUp(bitArray.Length, 64) / 8;
	}

	public static int AlignDown(int value, int alignPow2)
	{
		return value & ~(alignPow2 - 1);
	}

	public static int AlignUp(int value, int alignPow2)
	{
		return AlignDown(value + alignPow2 - 1, alignPow2);
	}

	public static int CalculateMeshBytes(Mesh mesh)
	{
		return 0;
	}

	public static void CollectDependencies(GameObject[] roots, List<Texture2D> textures = null, List<Mesh> meshes = null)
	{
	}

	public static void CalculateDependentBytes(GameObject[] roots, out long meshBytes, out long textureBytes)
	{
		meshBytes = 0L;
		textureBytes = 0L;
	}
}
