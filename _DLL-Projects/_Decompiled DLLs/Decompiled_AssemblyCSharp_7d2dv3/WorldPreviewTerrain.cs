using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using WorldGenerationEngineFinal;

public static class WorldPreviewTerrain
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int TerrainSectorSize = 512;

	[PublicizedFrom(EAccessModifier.Private)]
	public static WorldBuilder worldBuilder;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform parentTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Material terrainPreviewMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject[] objects;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Vector3> vertices = new List<Vector3>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<int> triangles = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Vector2> uvs = new List<Vector2>();

	public static void Init(WorldBuilder _worldBuilder, Transform _parentTransform)
	{
		worldBuilder = _worldBuilder;
		parentTransform = _parentTransform;
		terrainPreviewMaterial = Resources.Load("Materials/TerrainPreview", typeof(Material)) as Material;
	}

	public static void SetTexture(Texture2D _texture)
	{
		if ((bool)terrainPreviewMaterial)
		{
			terrainPreviewMaterial.mainTexture = _texture;
		}
	}

	public static void GenerateTerrain()
	{
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		parentTransform.localPosition = new Vector3(0f - (float)worldBuilder.WorldSize * 0.5f, 0f, 0f - (float)worldBuilder.WorldSize * 0.5f);
		if (XUiC_WorldGenerationWindow.Instance.PreviewQualityLevel == XUiC_WorldGenerationWindow.PreviewQuality.NoPreview)
		{
			return;
		}
		int num;
		switch (XUiC_WorldGenerationWindow.Instance.PreviewQualityLevel)
		{
		case XUiC_WorldGenerationWindow.PreviewQuality.Lowest:
			num = 16;
			break;
		case XUiC_WorldGenerationWindow.PreviewQuality.Low:
			num = 8;
			break;
		case XUiC_WorldGenerationWindow.PreviewQuality.Default:
			num = 4;
			break;
		case XUiC_WorldGenerationWindow.PreviewQuality.High:
		case XUiC_WorldGenerationWindow.PreviewQuality.Highest:
			num = 2;
			break;
		default:
			num = 1;
			break;
		}
		int metersPerDetailUnit = num;
		int num2 = worldBuilder.WorldSize / 512;
		int num3 = num2 * num2;
		if (objects == null || objects.Length != num3)
		{
			destroyTerrain();
			objects = new GameObject[num3];
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					Vector2i terrainSectorIndex = new Vector2i(j, i);
					GameObject gameObject = new GameObject(terrainSectorIndex.ToString());
					objects[j + i * num2] = gameObject;
					gameObject.transform.SetParent(parentTransform);
					createMesh(gameObject, terrainSectorIndex, metersPerDetailUnit);
				}
			}
		}
		else
		{
			for (int k = 0; k < num2; k++)
			{
				for (int l = 0; l < num2; l++)
				{
					updateMesh(_terrainSectorIndex: new Vector2i(l, k), _obj: objects[l + k * num2], _metersPerDetailUnit: metersPerDetailUnit);
				}
			}
		}
		Log.Warning("GenerateTerrain in {0}", (float)microStopwatch.ElapsedMilliseconds * 0.001f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void destroyTerrain()
	{
		if (!parentTransform)
		{
			return;
		}
		int childCount = parentTransform.childCount;
		if (childCount > 0)
		{
			MeshFilter[] componentsInChildren = parentTransform.GetComponentsInChildren<MeshFilter>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Object.Destroy(componentsInChildren[i].sharedMesh);
			}
			Renderer[] componentsInChildren2 = parentTransform.GetComponentsInChildren<Renderer>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				Object.Destroy(componentsInChildren2[j]);
			}
			for (int k = 0; k < childCount; k++)
			{
				Object.Destroy(parentTransform.GetChild(k).gameObject);
			}
		}
		objects = null;
	}

	public static void Cleanup()
	{
		destroyTerrain();
		if ((bool)terrainPreviewMaterial)
		{
			Resources.UnloadAsset(terrainPreviewMaterial);
			terrainPreviewMaterial = null;
			GCUtils.UnloadAndCollectStart();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void createMesh(GameObject _obj, Vector2i _terrainSectorIndex, int _metersPerDetailUnit)
	{
		MeshFilter meshFilter = _obj.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = _obj.AddComponent<MeshRenderer>();
		meshRenderer.receiveShadows = false;
		meshRenderer.lightProbeUsage = LightProbeUsage.Off;
		meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		meshRenderer.sharedMaterial = terrainPreviewMaterial;
		Mesh mesh = (meshFilter.sharedMesh = new Mesh());
		_obj.layer = 11;
		_obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		Vector2i vector2i = new Vector2i(_terrainSectorIndex.x * 512, _terrainSectorIndex.y * 512);
		int worldSize = worldBuilder.WorldSize;
		Vector3 item = default(Vector3);
		Vector2 item2 = default(Vector2);
		for (int i = vector2i.y; i <= vector2i.y + 512; i += _metersPerDetailUnit)
		{
			item.z = i;
			for (int j = vector2i.x; j <= vector2i.x + 512; j += _metersPerDetailUnit)
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
		int num2 = 512 / _metersPerDetailUnit;
		for (int k = 0; k < 512; k += _metersPerDetailUnit)
		{
			for (int l = 0; l < 512; l += _metersPerDetailUnit)
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
		mesh.SetUVs(0, uvs);
		mesh.SetTriangles(triangles, 0);
		mesh.RecalculateNormals();
		vertices.Clear();
		triangles.Clear();
		uvs.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void updateMesh(GameObject _obj, Vector2i _terrainSectorIndex, int _metersPerDetailUnit)
	{
		Vector2i vector2i = new Vector2i(_terrainSectorIndex.x * 512, _terrainSectorIndex.y * 512);
		Vector3 item = default(Vector3);
		for (int i = vector2i.y; i <= vector2i.y + 512; i += _metersPerDetailUnit)
		{
			item.z = i;
			for (int j = vector2i.x; j <= vector2i.x + 512; j += _metersPerDetailUnit)
			{
				item.x = j;
				item.y = worldBuilder.GetHeight(j, i);
				vertices.Add(item);
			}
		}
		Mesh sharedMesh = _obj.GetComponent<MeshFilter>().sharedMesh;
		sharedMesh.SetVertices(vertices);
		vertices.Clear();
		sharedMesh.RecalculateNormals();
	}
}
