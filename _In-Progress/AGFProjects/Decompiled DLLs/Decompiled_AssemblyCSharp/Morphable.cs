using System;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class Morphable : MonoBehaviour
{
	public string MorphSetPath;

	public string MorphName;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static string cMeshPrefix = "MeshMorph-";

	public void MorphHeadgear(Archetype archetype, bool _ignoreDlcEntitlements)
	{
		SkinnedMeshRenderer component = GetComponent<SkinnedMeshRenderer>();
		MeshMorph meshMorph = DataLoader.LoadAsset<MeshMorph>(MorphSetPath + "/" + archetype.Race + archetype.Variant.ToString("00") + "/" + MorphName + ".asset", _ignoreDlcEntitlements);
		if (component == null || component.sharedMesh == null || meshMorph == null || meshMorph.Vertices == null || meshMorph.Vertices.Length == 0)
		{
			Debug.LogError("Morphable: SkinnedMeshRenderer or MeshMorph not found", this);
			return;
		}
		Mesh mesh = UnityEngine.Object.Instantiate(component.sharedMesh);
		mesh.name = cMeshPrefix + component.gameObject.name;
		mesh.SetVertices(meshMorph.Vertices);
		mesh.RecalculateBounds();
		component.sharedMesh = mesh;
	}
}
