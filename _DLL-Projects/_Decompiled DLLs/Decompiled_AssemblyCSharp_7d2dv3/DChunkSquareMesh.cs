using UnityEngine;

public class DChunkSquareMesh
{
	public Vector3[] Normals;

	public Vector4[] Tangents;

	public Vector3[] EdgeCorNormals;

	public Color[] Colors;

	public int[] TextureId;

	public Bounds ChunkBound;

	public int WaterPlaneBlockId;

	public Vector3[] ColVertices;

	public float[] ColVerticesHeight;

	public VoxelMeshTerrain VoxelMesh = new VoxelMeshTerrain(0);

	public bool[] IsWater;

	public DChunkSquareMesh(DistantChunkMap DCMap, int LODLevel)
	{
		DistantChunkMapInfo distantChunkMapInfo = DCMap.ChunkMapInfoArray[LODLevel];
		Init(distantChunkMapInfo.BaseMesh.Vertices.Length, LODLevel, distantChunkMapInfo.ChunkResolution, distantChunkMapInfo.ColliderResolution, DCMap.NbResLevel, distantChunkMapInfo.BaseMesh.Triangles.Length);
	}

	public void Init(int NbVertices, int ResLevel, int Resolution, int ColliderResolution, int MaxNbResLevel, int NbTriangles)
	{
		Normals = new Vector3[NbVertices];
		Tangents = new Vector4[NbVertices];
		EdgeCorNormals = new Vector3[Resolution * 4];
		Colors = new Color[NbVertices];
		TextureId = new int[NbVertices];
		IsWater = new bool[NbVertices];
		ChunkBound = default(Bounds);
		ColVertices = new Vector3[ColliderResolution * ColliderResolution];
		ColVerticesHeight = new float[ColliderResolution * ColliderResolution];
	}
}
