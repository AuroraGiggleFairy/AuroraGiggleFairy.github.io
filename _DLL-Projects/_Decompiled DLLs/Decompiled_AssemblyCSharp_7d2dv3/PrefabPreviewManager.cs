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
		public PrefabDataInstance PrefabInstance;

		public GameObject Go;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float PrefabYPosition = 4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float DisplayUpdateDelay = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float PrefabListUpdateDelay = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int LodPoiDistance = 100000;

	public bool ReadyToDisplay;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_WorldGenerationPreview previewWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<int, PrefabDataInstance> prefabsAround = new Dictionary<int, PrefabDataInstance>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<int, PrefabGameObject> displayedPrefabs = new Dictionary<int, PrefabGameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform parentTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastDisplayUpdate;

	public bool Initialized;

	public static WorldBuilder WorldBuilder
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return XUiC_WorldGenerationWindow.Instance?.WorldBuilder;
		}
	}

	public PrefabPreviewManager(XUiC_WorldGenerationPreview _previewWindow)
	{
		previewWindow = _previewWindow;
	}

	public void InitPrefabs()
	{
		if (!Initialized)
		{
			prefabsAround.Clear();
			WorldBuilder.PrefabManager.GetPrefabsAround(previewWindow.GetCameraPosition(), 100000f, prefabsAround);
			UpdatePrefabsAround(prefabsAround);
			Initialized = true;
		}
	}

	public void RemovePrefabs()
	{
		ClearDisplayedPrefabs();
		displayedPrefabs.Clear();
		prefabsAround.Clear();
		Initialized = false;
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
			if (WorldBuilder != null && XUiC_WorldGenerationWindow.Instance != null && WorldBuilder.PrefabManager.UsedPrefabsWorld != null)
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
			if (!(displayedPrefabs[item].Go == null))
			{
				MeshFilter[] componentsInChildren = displayedPrefabs[item].Go.GetComponentsInChildren<MeshFilter>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					UnityEngine.Object.Destroy(componentsInChildren[i].mesh);
				}
				Renderer[] componentsInChildren2 = displayedPrefabs[item].Go.GetComponentsInChildren<Renderer>();
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					UnityEngine.Object.Destroy(componentsInChildren2[j].material);
				}
				UnityEngine.Object.Destroy(displayedPrefabs[item].Go);
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
					PrefabGameObject value2 = new PrefabGameObject
					{
						PrefabInstance = value
					};
					displayedPrefabs.Add(value.id, value2);
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
			if (!(displayedPrefabs[item2].Go == null))
			{
				MeshFilter[] componentsInChildren = displayedPrefabs[item2].Go.GetComponentsInChildren<MeshFilter>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					UnityEngine.Object.Destroy(componentsInChildren[i].mesh);
				}
				Renderer[] componentsInChildren2 = displayedPrefabs[item2].Go.GetComponentsInChildren<Renderer>();
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					UnityEngine.Object.Destroy(componentsInChildren2[j].material);
				}
				UnityEngine.Object.Destroy(displayedPrefabs[item2].Go);
				displayedPrefabs.Remove(item2);
			}
		}
	}

	public void UpdateDisplay()
	{
		if (XUiC_WorldGenerationWindow.Instance.PreviewQualityLevel == XUiC_WorldGenerationWindow.PreviewQuality.NoPreview)
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
			PrefabDataInstance prefabInstance = value.PrefabInstance;
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
			if ((bool)value.Go)
			{
				continue;
			}
			XUiC_WorldGenerationWindow.PreviewQuality previewQuality = XUiC_WorldGenerationWindow.Instance.PreviewQualityLevel;
			if (((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() || SystemInfo.systemMemorySize < 1300) && previewQuality > XUiC_WorldGenerationWindow.PreviewQuality.Default)
			{
				previewQuality = XUiC_WorldGenerationWindow.PreviewQuality.Default;
			}
			GameObject gameObject;
			switch (previewQuality)
			{
			case XUiC_WorldGenerationWindow.PreviewQuality.Highest:
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
			case XUiC_WorldGenerationWindow.PreviewQuality.Low:
			case XUiC_WorldGenerationWindow.PreviewQuality.Default:
			case XUiC_WorldGenerationWindow.PreviewQuality.High:
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
			value.Go = gameObject;
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
			Vector2i vector2i = new Vector2i(((int)vector.x + WorldBuilder.WorldSize / 2) / 150, ((int)vector.z + WorldBuilder.WorldSize / 2) / 150);
			TextMesh textMesh = gameObject2.AddMissingComponent<TextMesh>();
			textMesh.alignment = TextAlignment.Center;
			textMesh.anchor = TextAnchor.MiddleCenter;
			textMesh.fontSize = (prefabInstance.prefab.Name.Contains("trader") ? 100 : 20);
			textMesh.color = (prefabInstance.prefab.Name.Contains("trader") ? Color.red : Color.green);
			textMesh.text = prefabInstance.prefab.Name + Environment.NewLine + $"pos {prefabInstance.boundingBoxPosition}{Environment.NewLine}" + $"yoffset {prefabInstance.prefab.yOffset}{Environment.NewLine}" + $"rots to north {prefabInstance.prefab.rotationToFaceNorth}, total left {prefabInstance.rotation}{Environment.NewLine}" + $"tile pos {vector2i}{Environment.NewLine}" + $"score {prefabInstance.prefab.DensityScore}";
			if (microStopwatch.ElapsedMilliseconds > 50)
			{
				lastDisplayUpdate = 0f;
				return;
			}
		}
		foreach (KeyValuePair<int, PrefabGameObject> displayedPrefab2 in displayedPrefabs)
		{
			if (!(displayedPrefab2.Value.Go == null))
			{
				Transform transform4 = displayedPrefab2.Value.Go.transform;
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
