using UnityEngine;

public class SimpleMeshInfo
{
	public string[] meshNames;

	public Mesh[] meshes;

	public readonly float offsetY;

	public readonly Material mat;

	public SimpleMeshInfo(string[] _meshNames, Mesh[] _meshes, float _offsetY, Material _mat)
	{
		meshNames = _meshNames;
		meshes = _meshes;
		offsetY = _offsetY;
		mat = _mat;
	}
}
