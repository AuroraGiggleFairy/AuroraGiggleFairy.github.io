using System.Collections.Generic;
using UnityEngine;

public class MeshLists
{
	public static Stack<MeshLists> MeshListCache = new Stack<MeshLists>();

	public static int LastLargest = 0;

	public List<Vector3> Vertices = new List<Vector3>();

	public List<Vector2> Uvs = new List<Vector2>();

	public List<Vector2> Uvs2 = new List<Vector2>();

	public List<Vector2> Uvs3 = new List<Vector2>();

	public List<Vector2> Uvs4 = new List<Vector2>();

	public List<int> Triangles = new List<int>();

	public List<List<int>> TerrainTriangles = new List<List<int>>();

	public List<Color> Colours = new List<Color>();

	public List<Vector3> Normals = new List<Vector3>();

	public List<Vector4> Tangents = new List<Vector4>();

	public static MeshLists GetList()
	{
		if (MeshListCache.Count == 0)
		{
			return new MeshLists();
		}
		return MeshListCache.Pop();
	}

	public static void ReturnList(MeshLists list)
	{
		list.Reset();
		MeshListCache.Push(list);
		if (MeshListCache.Count > LastLargest)
		{
			LastLargest = MeshListCache.Count;
			Log.Out("Meshlist count is now " + MeshListCache.Count);
		}
	}

	public void Reset()
	{
		Vertices.Clear();
		Uvs.Clear();
		Uvs2.Clear();
		Uvs3.Clear();
		Uvs4.Clear();
		Triangles.Clear();
		foreach (List<int> terrainTriangle in TerrainTriangles)
		{
			terrainTriangle.Clear();
		}
		TerrainTriangles.Clear();
		Colours.Clear();
		Normals.Clear();
		Tangents.Clear();
	}
}
