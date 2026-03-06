using UnityEngine;

public static class WaterMeshUtils
{
	public static void RenderFace(Vector3[] _vertices, LightingAround _lightingAround, long _textureFull, VoxelMesh[] _meshes, Vector2 UVdata, bool _alternateWinding = false)
	{
		_meshes[1].AddBasicQuad(_vertices, Color.white, UVdata, bForceNormalsUp: true, _alternateWinding);
	}
}
