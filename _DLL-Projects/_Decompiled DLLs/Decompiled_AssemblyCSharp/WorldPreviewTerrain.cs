using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using WorldGenerationEngineFinal;

public class WorldPreviewTerrain : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int TerrainSectorSize = 512;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshFilter meshFilter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshRenderer meshRenderer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Mesh mesh;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Vector3> vertices = new List<Vector3>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<int> triangles = new List<int>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Vector2> uvs = new List<Vector2>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Material TerrainPreviewMaterial;

	public static WorldBuilder worldBuilder
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return XUiC_WorldGenerationWindowGroup.Instance?.worldBuilder;
		}
	}

	public static void GenerateTerrain(Transform _parentTransform)
	{
		int metersPerDetailUnit = 1;
		switch (XUiC_WorldGenerationWindowGroup.Instance.PreviewQualityLevel)
		{
		case XUiC_WorldGenerationWindowGroup.PreviewQuality.NoPreview:
			return;
		case XUiC_WorldGenerationWindowGroup.PreviewQuality.Lowest:
			metersPerDetailUnit = 16;
			break;
		case XUiC_WorldGenerationWindowGroup.PreviewQuality.Low:
			metersPerDetailUnit = 8;
			break;
		case XUiC_WorldGenerationWindowGroup.PreviewQuality.Default:
			metersPerDetailUnit = 4;
			break;
		case XUiC_WorldGenerationWindowGroup.PreviewQuality.High:
			metersPerDetailUnit = 2;
			break;
		case XUiC_WorldGenerationWindowGroup.PreviewQuality.Highest:
			metersPerDetailUnit = 1;
			break;
		}
		TerrainPreviewMaterial = Resources.Load("Materials/TerrainPreview", typeof(Material)) as Material;
		TerrainPreviewMaterial.mainTexture = worldBuilder.PreviewImage;
		int num = worldBuilder.WorldSize / 512;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				GameObject obj = new GameObject(new Vector2i(i, j).ToString());
				obj.transform.SetParent(_parentTransform);
				WorldPreviewTerrain worldPreviewTerrain = obj.AddComponent<WorldPreviewTerrain>();
				worldPreviewTerrain.DrawMeshSector(new Vector2i(i, j), metersPerDetailUnit);
				worldPreviewTerrain.meshRenderer.sharedMaterial = TerrainPreviewMaterial;
			}
		}
		_parentTransform.localPosition = new Vector3(0f - (float)worldBuilder.WorldSize * 0.5f, 0f, 0f - (float)worldBuilder.WorldSize * 0.5f);
	}

	public static void Cleanup(GameObject _rootObj)
	{
		MeshFilter[] componentsInChildren = _rootObj.GetComponentsInChildren<MeshFilter>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i].sharedMesh);
		}
		Renderer[] componentsInChildren2 = _rootObj.GetComponentsInChildren<Renderer>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			UnityEngine.Object.Destroy(componentsInChildren2[j]);
		}
		int childCount = _rootObj.transform.childCount;
		for (int k = 0; k < childCount; k++)
		{
			UnityEngine.Object.Destroy(_rootObj.transform.GetChild(k).gameObject);
		}
		if ((bool)TerrainPreviewMaterial)
		{
			Resources.UnloadAsset(TerrainPreviewMaterial);
			TerrainPreviewMaterial = null;
			GCUtils.UnloadAndCollectStart();
		}
	}

	public void OnDestroy()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		meshFilter = base.gameObject.AddComponent<MeshFilter>();
		meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
		meshRenderer.receiveShadows = false;
		meshRenderer.lightProbeUsage = LightProbeUsage.Off;
		meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		meshFilter.sharedMesh = (mesh = new Mesh());
		base.gameObject.layer = 11;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawMeshSector(Vector2i terrainSectorIndex, int metersPerDetailUnit)
	{
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
		Vector2i vector2i = new Vector2i(terrainSectorIndex.x * 512, terrainSectorIndex.y * 512);
		int worldSize = worldBuilder.WorldSize;
		Vector3 item = default(Vector3);
		Vector2 item2 = default(Vector2);
		for (int i = vector2i.y; i <= vector2i.y + 512; i += metersPerDetailUnit)
		{
			item.z = i;
			for (int j = vector2i.x; j <= vector2i.x + 512; j += metersPerDetailUnit)
			{
				item.x = j;
				item.y = worldBuilder.GetHeight(j, i);
				vertices.Add(item);
				item2.x = (float)j / (float)worldSize;
				item2.y = (float)i / (float)worldSize;
				uvs.Add(item2);
			}
		}
		int num = 0;
		int num2 = 512 / metersPerDetailUnit;
		for (int k = 0; k < 512; k += metersPerDetailUnit)
		{
			for (int l = 0; l < 512; l += metersPerDetailUnit)
			{
				triangles.Add(num);
				triangles.Add(num + num2 + 1);
				triangles.Add(num + 1);
				triangles.Add(num + 1);
				triangles.Add(num + num2 + 1);
				triangles.Add(num + num2 + 2);
				num++;
			}
			num++;
		}
		mesh.indexFormat = IndexFormat.UInt32;
		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);
		mesh.SetUVs(0, uvs);
		mesh.RecalculateNormals();
		vertices.Clear();
		triangles.Clear();
		uvs.Clear();
	}
}
