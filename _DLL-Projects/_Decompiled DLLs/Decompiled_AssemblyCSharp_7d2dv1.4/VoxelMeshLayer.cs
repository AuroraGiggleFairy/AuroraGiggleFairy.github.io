using System.Threading;

public class VoxelMeshLayer : IMemoryPoolableObject
{
	public int idx;

	public VoxelMesh[] meshes;

	public static int InstanceCount;

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
}
