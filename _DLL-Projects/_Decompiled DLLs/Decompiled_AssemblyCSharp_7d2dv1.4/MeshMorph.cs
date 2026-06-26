using System;
using UnityEngine;

[PreferBinarySerialization]
public class MeshMorph : ScriptableObject
{
	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public SkinnedMeshRenderer source;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] vertices;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static string cMeshPrefix = "MeshMorph-";

	public void Init(SkinnedMeshRenderer source, Vector3[] vertices)
	{
		this.source = source;
		this.vertices = vertices;
	}

	public GameObject GetMorphedSkinnedMesh()
	{
		Mesh mesh = UnityEngine.Object.Instantiate(source.sharedMesh);
		mesh.name = cMeshPrefix + source.gameObject.name;
		mesh.SetVertices(vertices);
		mesh.RecalculateBounds();
		GameObject gameObject = new GameObject(source.gameObject.name);
		gameObject.transform.localPosition = source.transform.localPosition;
		gameObject.transform.localRotation = source.transform.localRotation;
		gameObject.transform.localScale = source.transform.localScale;
		SkinnedMeshRenderer skinnedMeshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
		skinnedMeshRenderer.sharedMesh = mesh;
		skinnedMeshRenderer.sharedMaterials = source.sharedMaterials;
		skinnedMeshRenderer.rootBone = source.rootBone;
		skinnedMeshRenderer.bones = source.bones;
		return gameObject;
	}

	public static bool IsInstance(Mesh _mesh)
	{
		return _mesh.name.StartsWith(cMeshPrefix);
	}
}
