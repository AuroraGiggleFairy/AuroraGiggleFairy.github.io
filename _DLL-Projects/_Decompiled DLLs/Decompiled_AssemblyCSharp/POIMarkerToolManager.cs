using System.Collections.Generic;
using UnityEngine;
using WorldGenerationEngineFinal;

public class POIMarkerToolManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct PrefabAndPos
	{
		public Transform prefabTrans;

		public Vector3i position;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PrefabManagerData m_prefabManagerData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<Vector3i, List<PrefabAndPos>> POIMarkers = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<SelectionBox> registeredPOIMarkers = new List<SelectionBox>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Material markerMat;

	public static SelectionBox currentSelectionBox;

	[PublicizedFrom(EAccessModifier.Private)]
	public static SelectionBox previousSelectionBox;

	public static PrefabManagerData prefabManagerData
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return m_prefabManagerData ?? (m_prefabManagerData = new PrefabManagerData());
		}
	}

	public static void CleanUp()
	{
		m_prefabManagerData?.Cleanup();
		m_prefabManagerData = null;
		if (POIMarkers != null)
		{
			foreach (KeyValuePair<Vector3i, List<PrefabAndPos>> pOIMarker in POIMarkers)
			{
				for (int i = 0; i < pOIMarker.Value.Count; i++)
				{
					Transform prefabTrans = pOIMarker.Value[i].prefabTrans;
					if ((bool)prefabTrans && prefabTrans.gameObject != null)
					{
						Object.Destroy(prefabTrans.gameObject);
					}
				}
			}
			POIMarkers.Clear();
		}
		ClearPOIMarkers();
	}

	public static void RegisterPOIMarker(SelectionBox _selBox)
	{
		if (!registeredPOIMarkers.Contains(_selBox))
		{
			registeredPOIMarkers.Add(_selBox);
			if ((bool)_selBox && _selBox.UserData is Prefab.Marker { MarkerType: Prefab.Marker.MarkerTypes.PartSpawn, PartToSpawn: not null } marker && marker.PartToSpawn.Length > 0 && marker.PartDirty)
			{
				spawnPrefabViz(marker, _selBox);
			}
		}
	}

	public static void DisplayPrefabPreviewForMarker(SelectionBox _selBox)
	{
		if (prefabManagerData.AllPrefabDatas.Count == 0)
		{
			ThreadManager.RunCoroutineSync(prefabManagerData.LoadPrefabs());
			prefabManagerData.ShufflePrefabData(GameRandomManager.Instance.BaseSeed);
		}
		if (!(_selBox.UserData is Prefab.Marker marker))
		{
			return;
		}
		string str = "ghosttown,countrytown";
		if (PrefabEditModeManager.Instance.VoxelPrefab != null)
		{
			str = PrefabEditModeManager.Instance.VoxelPrefab.PrefabName;
			str = str.Replace("rwg_tile_", "");
			str = str.Split('_')[0];
		}
		bool useAnySizeSmaller = !Prefab.Marker.MarkerSizes.Contains(new Vector3i(marker.Size.x, 0, marker.Size.z));
		Prefab previewPrefabWithAnyTags = prefabManagerData.GetPreviewPrefabWithAnyTags(FastTags<TagGroup.Poi>.Parse(str), -1, new Vector2i(marker.Size.x, marker.Size.z), useAnySizeSmaller);
		if (previewPrefabWithAnyTags != null)
		{
			previewPrefabWithAnyTags = previewPrefabWithAnyTags.Clone();
			int x = previewPrefabWithAnyTags.size.x;
			int z = previewPrefabWithAnyTags.size.z;
			previewPrefabWithAnyTags.RotateY(_bLeft: true, (previewPrefabWithAnyTags.rotationToFaceNorth + marker.Rotations) % 4);
			Transform transform = _selBox.transform.Find("PrefabPreview");
			if (transform != null)
			{
				Object.Destroy(transform.gameObject);
			}
			GameManager.Instance.StartCoroutine(previewPrefabWithAnyTags.ToTransform(_genBlockModels: true, _genTerrain: true, _genBlockShapes: true, _fillEmptySpace: false, _selBox.transform, "PrefabPreview", new Vector3(0f - (float)x / 2f, (float)previewPrefabWithAnyTags.yOffset + 0.15f, 0f - (float)z / 2f)));
		}
	}

	public static void UnRegisterPOIMarker(SelectionBox _selBox)
	{
		if (registeredPOIMarkers.Contains(_selBox))
		{
			registeredPOIMarkers.Remove(_selBox);
		}
	}

	public static void ClearPOIMarkers()
	{
		registeredPOIMarkers.Clear();
	}

	public static void SelectionChanged(SelectionBox selBox)
	{
		if ((bool)selBox && selBox != currentSelectionBox)
		{
			previousSelectionBox = currentSelectionBox;
			currentSelectionBox = selBox;
		}
		currentSelectionBox = selBox;
		if (XUiC_WoPropsPOIMarker.Instance != null && currentSelectionBox != null && currentSelectionBox.UserData is Prefab.Marker)
		{
			XUiC_WoPropsPOIMarker.Instance.CurrentMarker = currentSelectionBox.UserData as Prefab.Marker;
		}
		UpdateAllColors();
	}

	public static void spawnPrefabViz(Prefab.Marker _currentMarker, SelectionBox selBox)
	{
		Transform transform = selBox.transform.Find("PrefabPreview");
		if (transform != null)
		{
			Object.Destroy(transform.gameObject);
		}
		Prefab prefab = new Prefab();
		prefab.Load(_currentMarker.PartToSpawn, _applyMapping: true, _fixChildblocks: false, _allowMissingBlocks: true);
		prefab.Init(0, 0);
		prefab.RotateY(_bLeft: false, (prefab.rotationToFaceNorth + _currentMarker.Rotations) % 4);
		GameManager.Instance.StartCoroutine(prefab.ToTransform(_genBlockModels: true, _genTerrain: true, _genBlockShapes: true, _fillEmptySpace: false, SelectionBoxManager.Instance.Selection?.box.transform, "PrefabPreview", new Vector3(0f - (float)prefab.size.x / 2f, 0.1f, 0f - (float)prefab.size.z / 2f)));
	}

	public static void UpdateAllColors()
	{
		int num = 0;
		if ((bool)currentSelectionBox && currentSelectionBox.UserData is Prefab.Marker marker)
		{
			num = marker.GroupId;
		}
		for (int i = 0; i < registeredPOIMarkers.Count; i++)
		{
			SelectionBox selectionBox = registeredPOIMarkers[i];
			Prefab.Marker marker2 = selectionBox.UserData as Prefab.Marker;
			if (marker2.GroupId == num)
			{
				selectionBox.SetAllFacesColor(marker2.GroupColor + new Color(0.2f, 0.2f, 0.2f, 0f));
			}
			else
			{
				selectionBox.SetAllFacesColor(marker2.GroupColor);
			}
		}
		if ((bool)currentSelectionBox && currentSelectionBox.UserData is Prefab.Marker marker3)
		{
			if (marker3.MarkerType == Prefab.Marker.MarkerTypes.PartSpawn)
			{
				currentSelectionBox.SetAllFacesColor(new Color(0f, 0f, 0f, 0f));
			}
			else
			{
				currentSelectionBox.SetAllFacesColor(marker3.GroupColor + new Color(0.5f, 0.5f, 0.5f, 0f));
			}
		}
	}

	public static void ShowPOIMarkers(bool bShow = true)
	{
		if (POIMarkers == null)
		{
			return;
		}
		foreach (KeyValuePair<Vector3i, List<PrefabAndPos>> pOIMarker in POIMarkers)
		{
			for (int i = 0; i < pOIMarker.Value.Count; i++)
			{
				Transform prefabTrans = pOIMarker.Value[i].prefabTrans;
				if ((bool)prefabTrans && prefabTrans.gameObject != null)
				{
					prefabTrans.gameObject.SetActive(bShow);
				}
			}
		}
	}
}
