using System.Threading;
using UnityEngine;

public class VoxelMeshLayer : IMemoryPoolableObject
{
	public int idx;

	public VoxelMesh[] meshes;

	public static int InstanceCount;

	public int pendingCopyOperations;

	public VoxelMeshLayer()
	{
		idx = 0;
		meshes = new VoxelMesh[MeshDescription.meshes.Length];
		for (int i = 0; i < MeshDescription.meshes.Length; i++)
		{
			meshes[i] = VoxelMesh.Create(i, MeshDescription.meshes[i].meshType, 0);
		}
		Interlocked.Increment(ref InstanceCount);
	}

	public void Reset()
	{
		for (int i = 0; i < meshes.Length; i++)
		{
			meshes[i].ClearMesh();
		}
		if (pendingCopyOperations != 0)
		{
			Log.Error("Resetting VML with pendingCopyOperations");
		}
		pendingCopyOperations = 0;
	}

	public void Cleanup()
	{
		Interlocked.Decrement(ref InstanceCount);
	}

	public static void StaticCleanup()
	{
		InstanceCount = 0;
	}

	public void SizeToChunkDefaults()
	{
		for (int i = 0; i < meshes.Length; i++)
		{
			meshes[i].SizeToChunkDefaults(i);
		}
	}

	public bool HasContent()
	{
		for (int i = 0; i < meshes.Length; i++)
		{
			if (meshes[i].Vertices.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	public int GetTris()
	{
		int num = 0;
		for (int i = 0; i < meshes.Length; i++)
		{
			num += meshes[i].Triangles;
		}
		return num;
	}

	public int GetTrisInMesh(int _idx)
	{
		return meshes[_idx].Triangles;
	}

	public int GetSizeOfMesh(int _idx)
	{
		return meshes[_idx].Size;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public int CopyToMesh(int _meshIdx, MeshFilter[] _mf, MeshRenderer[] _mr, int _lodLevel)
	{
		pendingCopyOperations++;
		return meshes[_meshIdx].CopyToMesh(_mf, _mr, _lodLevel, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			TryFree();
		});
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void StartCopyMeshes()
	{
		if (pendingCopyOperations != 0)
		{
			Log.Error("StartCopyMeshes called with pendingCopyOperations != 0");
		}
		pendingCopyOperations++;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void EndCopyMeshes()
	{
		TryFree();
	}

	public bool TryFree()
	{
		pendingCopyOperations--;
		if (pendingCopyOperations > 0)
		{
			return false;
		}
		MemoryPools.poolVML.FreeSync(this);
		return true;
	}
}
