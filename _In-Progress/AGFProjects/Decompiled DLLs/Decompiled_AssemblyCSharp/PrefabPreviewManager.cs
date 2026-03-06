using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using WorldGenerationEngineFinal;

public class PrefabPreviewManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class PrefabGameObject
	{
		public PrefabDataInstance prefabInstance;

		public GameObject go;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPrefabYPosition = 4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDisplayUpdateDelay = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPrefabListUpdateDelay = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cLodPoiDistance = 100000;

	public static bool ReadyToDisplay;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, PrefabDataInstance> prefabsAround = new Dictionary<int, PrefabDataInstance>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, PrefabGameObject> displayedPrefabs = new Dictionary<int, PrefabGameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform parentTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastDisplayUpdate;

	public bool initialized;

	public static WorldBuilder worldBuilder
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return XUiC_WorldGenerationWindowGroup.Instance?.worldBuilder;
		}
	}

	public void InitPrefabs()
	{
		if (!initialized)
		{
			prefabsAround.Clear();
			worldBuilder.PrefabManager.GetPrefabsAround(XUiC_WorldGenerationWindowGroup.Instance.PreviewWindow.GetCameraPosition(), 100000f, prefabsAround);
			UpdatePrefabsAround(prefabsAround);
			initialized = true;
		}
	}

	public void RemovePrefabs()
	{
		ClearDisplayedPrefabs();
		displayedPrefabs.Clear();
		prefabsAround.Clear();
		initialized = false;
	}

	public void Update()
	{
		if (!(Time.time - lastDisplayUpdate < 2f))
		{
			lastDisplayUpdate = Time.time;
			ForceUpdate();
		}
	}

	public void ForceUpdate()
	{
		UpdateDisplay();
		if (!(Time.time - lastTime < 2f))
		{
			lastTime = Time.time;
			if (worldBuilder != null && XUiC_WorldGenerationWindowGroup.Instance != null && XUiC_WorldGenerationWindowGroup.Instance.PreviewWindow != null && worldBuilder.PrefabManager.UsedPrefabsWorld != null)
			{
				InitPrefabs();
			}
		}
	}

	public void ClearDisplayedPrefabs()
	{
		if (displayedPrefabs == null || displayedPrefabs.Count == 0)
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, PrefabGameObject> displayedPrefab in displayedPrefabs)
		{
			list.Add(displayedPrefab.Key);
		}
		foreach (int item in list)
		{
			if (!(displayedPrefabs[item].go == null))
			{
				MeshFilter[] componentsInChildren = displayedPrefabs[item].go.GetComponentsInChildren<MeshFilter>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					UnityEngine.Object.Destroy(componentsInChildren[i].mesh);
				}
				Renderer[] componentsInChildren2 = displayedPrefabs[item].go.GetComponentsInChildren<Renderer>();
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					UnityEngine.Object.Destroy(componentsInChildren2[j].material);
				}
				UnityEngine.Object.Destroy(displayedPrefabs[item].go);
				displayedPrefabs.Remove(item);
			}
		}
	}

	public void UpdatePrefabsAround(Dictionary<int, PrefabDataInstance> _prefabsAround)
	{
		foreach (KeyValuePair<int, PrefabDataInstance> item in _prefabsAround)
		{
			PrefabDataInstance value = item.Value;
			if (!displayedPrefabs.ContainsKey(value.id))
			{
				string name = value.location.Name;
				if (PathAbstractions.PrefabImpostersSearchPaths.GetLocation(name).Type != PathAbstractions.EAbstractedLocationType.None)
				{
					PrefabGameObject prefabGameObject = new PrefabGameObject();
					prefabGameObject.prefabInstance = value;
					displayedPrefabs.Add(value.id, prefabGameObject);
				}
			}
		}
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, PrefabGameObject> displayedPrefab in displayedPrefabs)
		{
			if (!_prefabsAround.ContainsKey(displayedPrefab.Key))
			{
				list.Add(displayedPrefab.Key);
			}
		}
		foreach (int item2 in list)
		{
			if (!(displayedPrefabs[item2].go == null))
			{
				MeshFilter[] componentsInChildren = displayedPrefabs[item2].go.GetComponentsInChildren<MeshFilter>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					UnityEngine.Object.Destroy(componentsInChildren[i].mesh);
				}
				Renderer[] componentsInChildren2 = displayedPrefabs[item2].go.GetComponentsInChildren<Renderer>();
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					UnityEngine.Object.Destroy(componentsInChildren2[j].material);
				}
				UnityEngine.Object.Destroy(displayedPrefabs[item2].go);
				displayedPrefabs.Remove(item2);
			}
		}
	}

	public void UpdateDisplay()
	{
		if (XUiC_WorldGenerationWindowGroup.Instance.PreviewQualityLevel == XUiC_WorldGenerationWindowGroup.PreviewQuality.NoPreview)
		{
			return;
		}
		MicroStopwatch microStopwatch = new MicroStopwatch();
		if (parentTransform == null)
		{
			parentTransform = new GameObject("PrefabsLOD").transform;
			parentTransform.gameObject.layer = 11;
		}
		foreach (KeyValuePair<int, PrefabGameObject> displayedPrefab in displayedPrefabs)
		{
			PrefabGameObject value = displayedPrefab.Value;
			PrefabDataInstance prefabInstance = value.prefabInstance;
			Vector3 vector = prefabInstance.boundingBoxPosition.ToVector3();
			Vector3 vector2 = prefabInstance.boundingBoxPosition.ToVector3();
			Vector3 size = prefabInstance.boundingBoxSize.ToVector3();
			if (prefabInstance.rotation % 2 == 0)
			{
				vector += new Vector3((float)prefabInstance.boundingBoxSize.x * 0.5f, 0f, (float)prefabInstance.boundingBoxSize.z * 0.5f);
				vector2 += new Vector3((float)prefabInstance.boundingBoxSize.x * 0.5f, 0f, (float)prefabInstance.boundingBoxSize.z * 0.5f);
			}
			else
			{
				vector += new Vector3((float)prefabInstance.boundingBoxSize.z * 0.5f, 0f, (float)prefabInstance.boundingBoxSize.x * 0.5f);
				vector2 += new Vector3((float)prefabInstance.boundingBoxSize.z * 0.5f, 0f, (float)prefabInstance.boundingBoxSize.x * 0.5f);
				size = new Vector3(size.z, size.y, size.x);
			}
			Vector3 vector3 = Vector3.zero;
			Quaternion rotation = Quaternion.identity;
			switch (prefabInstance.rotation)
			{
			case 1:
				vector3 = new Vector3(0.5f, 0f, -0.5f);
				rotation = Quaternion.Euler(0f, 270f, 0f);
				break;
			case 2:
				vector3 = new Vector3(0.5f, 0f, 0.5f);
				rotation = Quaternion.Euler(0f, 180f, 0f);
				break;
			case 3:
				vector3 = new Vector3(-0.5f, 0f, 0.5f);
				rotation = Quaternion.Euler(0f, 90f, 0f);
				break;
			case 0:
				vector3 = new Vector3(-0.5f, 0f, -0.5f);
				break;
			}
			if (Utils.FastAbs(vector.x - (float)(int)vector.x) > 0.001f)
			{
				vector.x += vector3.x;
			}
			if (Utils.FastAbs(vector.z - (float)(int)vector.z) > 0.001f)
			{
				vector.z += vector3.z;
			}
			float num = 0f;
			Utils.DrawBounds(new Bounds(vector2 + new Vector3(0f, (float)prefabInstance.boundingBoxSize.y * 0.5f + 0.1f + num, 0f) - Origin.position, size), Color.green, 2f);
			if ((bool)value.go)
			{
				continue;
			}
			GameObject gameObject = null;
			XUiC_WorldGenerationWindowGroup.PreviewQuality previewQuality = XUiC_WorldGenerationWindowGroup.Instance.PreviewQualityLevel;
			if (((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() || SystemInfo.systemMemorySize < 1300) && previewQuality > XUiC_WorldGenerationWindowGroup.PreviewQuality.Default)
			{
				previewQuality = XUiC_WorldGenerationWindowGroup.PreviewQuality.Default;
			}
			switch (previewQuality)
			{
			case XUiC_WorldGenerationWindowGroup.PreviewQuality.Highest:
			{
				string name = prefabInstance.location.Name;
				gameObject = SimpleMeshFile.ReadGameObject(PathAbstractions.PrefabImpostersSearchPaths.GetLocation(name));
				if (gameObject == null)
				{
					continue;
				}
				Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].gameObject.layer = 11;
				}
				break;
			}
			case XUiC_WorldGenerationWindowGroup.PreviewQuality.Low:
			case XUiC_WorldGenerationWindowGroup.PreviewQuality.Default:
			case XUiC_WorldGenerationWindowGroup.PreviewQuality.High:
				gameObject = new GameObject();
				if (!prefabInstance.prefab.Name.Contains("rwg_tile") && !prefabInstance.prefab.Name.Contains("part_driveway"))
				{
					Transform transform = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
					transform.SetParent(gameObject.transform);
					transform.localPosition = new Vector3(0f, prefabInstance.boundingBoxSize.y / 2, 0f);
					transform.localScale = prefabInstance.boundingBoxSize.ToVector3();
					if (prefabInstance.previewColor.r + prefabInstance.previewColor.g + prefabInstance.previewColor.b != 765)
					{
						transform.GetComponent<Renderer>().material.color = prefabInstance.previewColor;
					}
					Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].gameObject.layer = 11;
					}
				}
				break;
			default:
				gameObject = new GameObject();
				break;
			}
			value.go = gameObject;
			gameObject.layer = 11;
			Transform transform2 = gameObject.transform;
			transform2.name = prefabInstance.location.Name;
			transform2.SetParent(parentTransform, worldPositionStays: false);
			Vector3 position = vector + new Vector3(0f, 0.01f + num - 4f, 0f) - Origin.position;
			transform2.SetPositionAndRotation(position, rotation);
			GameObject gameObject2 = new GameObject(prefabInstance.prefab.Name);
			Transform transform3 = gameObject2.transform;
			transform3.SetParent(transform2);
			transform3.rotation = Quaternion.Euler(90f, gameObject.transform.rotation.eulerAngles.y, 0f);
			transform3.localPosition = new Vector3(0f, (float)(prefabInstance.boundingBoxSize.y + prefabInstance.prefab.yOffset) + 0.25f, 0f);
			gameObject2.layer = 11;
			Vector2i vector2i = new Vector2i(((int)vector.x + worldBuilder.WorldSize / 2) / 150, ((int)vector.z + worldBuilder.WorldSize / 2) / 150);
			TextMesh textMesh = gameObject2.AddMissingComponent<TextMesh>();
			textMesh.alignment = TextAlignment.Center;
			textMesh.anchor = TextAnchor.MiddleCenter;
			textMesh.fontSize = (prefabInstance.prefab.Name.Contains("trader") ? 100 : 20);
			textMesh.color = (prefabInstance.prefab.Name.Contains("trader") ? Color.red : Color.green);
			textMesh.text = prefabInstance.prefab.Name + Environment.NewLine + $"pos {prefabInstance.boundingBoxPosition}{Environment.NewLine}" + $"yoffset {prefabInstance.prefab.yOffset}{Environment.NewLine}" + $"rots to north {prefabInstance.prefab.RotationsToNorth}, total left {prefabInstance.rotation}{Environment.NewLine}" + $"tile pos {vector2i}{Environment.NewLine}" + $"score {prefabInstance.prefab.DensityScore}";
			if (microStopwatch.ElapsedMilliseconds > 50)
			{
				lastDisplayUpdate = 0f;
				return;
			}
		}
		foreach (KeyValuePair<int, PrefabGameObject> displayedPrefab2 in displayedPrefabs)
		{
			if (!(displayedPrefab2.Value.go == null))
			{
				Transform transform4 = displayedPrefab2.Value.go.transform;
				for (int j = 0; j < transform4.childCount; j++)
				{
					transform4.GetChild(j).gameObject.SetActive(value: true);
				}
			}
		}
	}

	public void ClearOldPreview()
	{
		RemovePrefabs();
	}

	public void Cleanup()
	{
		RemovePrefabs();
		if ((bool)parentTransform)
		{
			UnityEngine.Object.Destroy(parentTransform.gameObject);
			parentTransform = null;
		}
	}
}
