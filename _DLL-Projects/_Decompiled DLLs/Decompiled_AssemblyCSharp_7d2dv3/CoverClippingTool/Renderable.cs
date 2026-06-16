using UnityEngine;

namespace CoverClippingTool;

public struct Renderable
{
	public bool DestroyMesh;

	public Mesh RenderMesh;

	public Matrix4x4 LocalToWorldMatrix;
}
