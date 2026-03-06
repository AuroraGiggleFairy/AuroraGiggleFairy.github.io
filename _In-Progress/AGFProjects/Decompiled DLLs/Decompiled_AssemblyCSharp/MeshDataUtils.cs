using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

public static class MeshDataUtils
{
	public const MeshUpdateFlags DoNothing = MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds;

	public static void CopyInterleaved(int elementSize, int offset, int stride, ReadOnlySpan<byte> src, Span<byte> dest)
	{
		int num = 0;
		int num2 = offset;
		while (num + elementSize <= src.Length && num2 + stride - offset <= dest.Length)
		{
			src.Slice(num, elementSize).CopyTo(dest.Slice(num2, elementSize));
			num += elementSize;
			num2 += stride;
		}
	}

	public static MeshDataLayout SetAttributes(Mesh.MeshData meshData, ReadOnlySpan<Vector3> vertices, ReadOnlySpan<int> indices, ReadOnlySpan<Vector3> normals, ReadOnlySpan<Vector4> tangents, ReadOnlySpan<Color> colors, ReadOnlySpan<Vector2> texCoord0s, ReadOnlySpan<Vector2> texCoord1s, NativeList<VertexAttributeDescriptor> vertexAttributesOut)
	{
		int num = 0;
		MeshDataLayout result = default(MeshDataLayout);
		result.PositionSize = 12;
		result.PositionOffset = num;
		num += 12;
		vertexAttributesOut.Add(new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0));
		if (normals.Length > 0)
		{
			result.NormalSize = 12;
			result.NormalOffset = num;
			num += 12;
			vertexAttributesOut.Add(new VertexAttributeDescriptor(VertexAttribute.Normal));
		}
		else
		{
			result.NormalSize = -1;
			result.NormalOffset = -1;
		}
		if (tangents.Length > 0)
		{
			result.TangentSize = 16;
			result.TangentOffset = num;
			num += 16;
			vertexAttributesOut.Add(new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4));
		}
		else
		{
			result.TangentSize = -1;
			result.TangentOffset = -1;
		}
		if (colors.Length > 0)
		{
			result.ColorSize = 16;
			result.ColorOffset = num;
			num += 16;
			vertexAttributesOut.Add(new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4));
		}
		else
		{
			result.ColorSize = -1;
			result.ColorOffset = -1;
		}
		if (texCoord0s.Length > 0)
		{
			result.TexCoord0Size = 8;
			result.TexCoord0Offset = num;
			num += 8;
			vertexAttributesOut.Add(new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));
		}
		else
		{
			result.TexCoord0Size = -1;
			result.TexCoord0Offset = -1;
		}
		if (texCoord1s.Length > 0)
		{
			result.TexCoord1Size = 8;
			result.TexCoord1Offset = num;
			num += 8;
			vertexAttributesOut.Add(new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2));
		}
		else
		{
			result.TexCoord1Size = -1;
			result.TexCoord1Offset = -1;
		}
		meshData.SetVertexBufferParams(vertices.Length, vertexAttributesOut.AsArray());
		result.Stride = num;
		meshData.SetIndexBufferParams(indices.Length, IndexFormat.UInt32);
		return result;
	}

	public static void SetSubmesh(Mesh.MeshData meshData, int vertexCount, int indexCount, Bounds bounds)
	{
		meshData.subMeshCount = 1;
		meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexCount)
		{
			bounds = bounds,
			vertexCount = vertexCount
		}, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds);
	}

	public static Bounds RecalculateBounds(ReadOnlySpan<Vector3> vertices)
	{
		Bounds result = new Bounds(vertices[0], Vector3.zero);
		for (int i = 1; i < vertices.Length; i++)
		{
			result.Encapsulate(vertices[i]);
		}
		return result;
	}

	public static void ApplyMeshDataCompression(Mesh.MeshData meshData, NativeArray<VertexAttributeDescriptor> vertexAttributes)
	{
		GameUtils.ApplyCompressedVertexAttributes(vertexAttributes, compressPosition: false);
		meshData.SetVertexBufferParams(meshData.vertexCount, vertexAttributes);
	}

	public static void CalculateNormals(ReadOnlySpan<Vector3> vertices, ReadOnlySpan<int> indices, Span<Vector3> normals)
	{
		int num = Math.Min(vertices.Length, normals.Length);
		for (int i = 0; i < normals.Length; i++)
		{
			normals[i] = Vector3.zero;
		}
		using NativeParallelMultiHashMap<Vector3, int> vertexToIndices = new NativeParallelMultiHashMap<Vector3, int>(num, Allocator.Temp);
		for (int j = 0; j < num; j++)
		{
			vertexToIndices.Add(vertices[j], j);
		}
		using NativeArray<int> normalsAdded = new NativeArray<int>(num, Allocator.Temp);
		for (int k = 0; k + 3 <= indices.Length; k += 3)
		{
			Vector3 vector = vertices[k];
			Vector3 vector2 = vertices[k + 1];
			Vector3 vector3 = vertices[k + 2];
			Vector3 normal = Vector3.Cross(vector2 - vector, vector3 - vector);
			normal.Normalize();
			AddTriangleNormal(ref normals, vertexToIndices, normalsAdded, vector, normal);
			AddTriangleNormal(ref normals, vertexToIndices, normalsAdded, vector2, normal);
			AddTriangleNormal(ref normals, vertexToIndices, normalsAdded, vector3, normal);
		}
		for (int l = 0; l < normals.Length; l++)
		{
			int num2 = normalsAdded[l];
			if (num2 > 0)
			{
				normals[l] /= (float)num2;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void AddTriangleNormal(ref Span<Vector3> normals, NativeParallelMultiHashMap<Vector3, int> vertexToIndices, NativeArray<int> normalsAdded, Vector3 vertex, Vector3 normal)
	{
		foreach (int item in vertexToIndices.GetValuesForKey(vertex))
		{
			normals[item] += normal;
			normalsAdded[item]++;
		}
	}

	public unsafe static AtomicSafeHandleScope CreateNativeArray<T>(T* ptr, int length, out NativeArray<T> array) where T : unmanaged
	{
		array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, length, Allocator.None);
		return default(AtomicSafeHandleScope);
	}
}
