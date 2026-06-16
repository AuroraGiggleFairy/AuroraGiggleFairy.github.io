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
		if (component == null || component.sharedMesh == null)
		{
			Log.Error("Morphable: SkinnedMeshRenderer not found", this);
			return;
		}
		LoadManager.AssetRequestTask<MeshMorph> assetRequestTask = LoadManager.LoadAsset<MeshMorph>(MorphSetPath + "/" + archetype.Race + archetype.Variant.ToString("00") + "/" + MorphName + ".asset", null, null, _deferLoading: false, _loadSync: true, _ignoreDlcEntitlements);
		MeshMorph asset = assetRequestTask.Asset;
		if (asset == null || asset.Vertices == null || asset.Vertices.Length == 0)
		{
			Log.Error("Morphable: MeshMorph not found", this);
			return;
		}
		Mesh mesh = UnityEngine.Object.Instantiate(component.sharedMesh);
		mesh.name = cMeshPrefix + component.gameObject.name;
		mesh.SetVertices(asset.Vertices);
		mesh.RecalculateBounds();
		component.sharedMesh = mesh;
		assetRequestTask.Release();
	}
}
