using System.IO;
using UnityEngine;

public class VoxelMeshExt3dModel(int _meshIndex, int _minSize = 500) : VoxelMesh(_meshIndex, _minSize)
{
	public VoxelMesh boundingBoxMesh = new VoxelMesh(-1, 0);

	public Bounds[] aabb = new Bounds[0];

	[PublicizedFrom(EAccessModifier.Private)]
	public ArrayListMP<Vector3> boundsVertices = new ArrayListMP<Vector3>(MemoryPools.poolVector3);

	[PublicizedFrom(EAccessModifier.Private)]
	public ArrayListMP<int> boundsTriangles = new ArrayListMP<int>(MemoryPools.poolInt);

	public VoxelMesh lod1Mesh = new VoxelMesh(-1, 0);

	public override void Write(BinaryWriter _bw)
	{
		base.Write(_bw);
		_bw.Write((uint)aabb.Length);
		Bounds[] array = aabb;
		foreach (Bounds bounds in array)
		{
			BoundsUtils.WriteBounds(_bw, bounds);
		}
		boundingBoxMesh.Write(_bw);
		lod1Mesh.Write(_bw);
	}

	public override void Read(BinaryReader _br)
	{
		base.Read(_br);
		uint num = _br.ReadUInt32();
		aabb = new Bounds[num];
		for (int i = 0; i < num; i++)
		{
			aabb[i] = BoundsUtils.ReadBounds(_br);
		}
		boundingBoxMesh.ClearMesh();
		boundingBoxMesh.Read(_br);
		lod1Mesh.ClearMesh();
		lod1Mesh.Read(_br);
		lod1Mesh.ClearMesh();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addColliders(Vector3 _drawPos, VoxelMesh _colliderMesh)
	{
		if (_colliderMesh.Vertices.Count != 0)
		{
			int count = boundsVertices.Count;
			for (int i = 0; i < _colliderMesh.Vertices.Count; i++)
			{
				boundsVertices.Add(_drawPos + _colliderMesh.Vertices[i]);
			}
			for (int j = 0; j < _colliderMesh.Indices.Count; j++)
			{
				boundsTriangles.Add(count + _colliderMesh.Indices[j]);
			}
		}
	}

	public override void AddMesh(Vector3 _drawPos, int _count, Vector3[] _vertices, Vector3[] _normals, ArrayListMP<int> _indices, ArrayListMP<Vector2> _uvs, byte _sunlight, byte _blocklight, VoxelMesh _specialColliders, int damage)
	{
		if (_specialColliders != null)
		{
			addColliders(_drawPos, _specialColliders);
		}
		base.AddMesh(_drawPos, _count, _vertices, _normals, _indices, _uvs, _sunlight, _blocklight, null, damage);
	}

	public override int CopyToColliders(int _clrIdx, MeshCollider _meshCollider, out Mesh mesh)
	{
		if (boundsVertices == null || boundsVertices.Count == 0)
		{
			GameManager.Instance.World.m_ChunkManager.BakeDestroyCancel(_meshCollider);
			mesh = null;
			return 0;
		}
		mesh = ResetMesh(_meshCollider);
		MeshUnsafeCopyHelper.CopyVertices(boundsVertices, mesh);
		MeshUnsafeCopyHelper.CopyTriangles(boundsTriangles, mesh);
		_meshCollider.tag = "T_Mesh_B";
		return boundsTriangles.Count / 3;
	}

	public override void ClearMesh()
	{
		base.ClearMesh();
		lod1Mesh.ClearMesh();
		boundsVertices.Clear();
		boundsTriangles.Clear();
	}

	public override void Finished()
	{
		base.Finished();
		lod1Mesh.Finished();
	}
}
