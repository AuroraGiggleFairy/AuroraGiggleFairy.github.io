using System.Collections.Generic;
using UnityEngine;

public class SimpleMeshInfo
{
	public string[] meshNames;

	public Mesh[] meshes;

	public readonly float offsetY;

	public readonly Material mat;

	public List<ImposterCanvas> signs;

	public SimpleMeshInfo(string[] _meshNames, Mesh[] _meshes, float _offsetY, Material _mat, List<ImposterCanvas> _signs)
	{
		meshNames = _meshNames;
		meshes = _meshes;
		offsetY = _offsetY;
		mat = _mat;
		signs = _signs;
	}
}
