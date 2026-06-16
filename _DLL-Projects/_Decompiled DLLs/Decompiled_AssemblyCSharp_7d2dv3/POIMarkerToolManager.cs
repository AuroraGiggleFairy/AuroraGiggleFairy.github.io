using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PrefabVolumes;
using UnityEngine;
using WorldGenerationEngineFinal;

public static class POIMarkerToolManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string PrefabPreviewTransformName = "PrefabPreview";

	[PublicizedFrom(EAccessModifier.Private)]
	public static PrefabManagerData prefabManagerData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, PrefabData> allPrefabData = new CaseInsensitiveStringDictionary<PrefabData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<SelectionBox> registeredPoiMarkers = new List<SelectionBox>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ReadOnlyCollection<SelectionBox> registeredPoiMarkersReadOnly = registeredPoiMarkers.AsReadOnly();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, string> lastSelectedBoxInGroup = new Dictionary<string, string>();

	public static PrefabManagerData PrefabManagerData
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (prefabManagerData != null)
			{
				return prefabManagerData;
			}
			prefabManagerData = new PrefabManagerData();
			if (prefabManagerData.AllPrefabDatas.Count != 0)
			{
				return prefabManagerData;
			}
			prefabManagerData.LoadPrefabs();
			prefabManagerData.ShufflePrefabData(GameRandomManager.Instance.BaseSeed);
			return prefabManagerData;
		}
	}

	public static Dictionary<string, PrefabData> AllPrefabData
	{
		get
		{
			if (allPrefabData.Count == 0)
			{
				ThreadManager.RunCoroutineSync(loadPrefabDatas());
			}
			return allPrefabData;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator loadPrefabDatas()
	{
		if (allPrefabData.Count != 0)
		{
			yield break;
		}
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		List<PathAbstractions.AbstractedLocation> prefabs = PathAbstractions.PrefabsSearchPaths.GetAvailablePathsList(null, _ignoreDuplicateNames: true);
		foreach (PathAbstractions.AbstractedLocation item in prefabs)
		{
			PrefabData value = PrefabData.LoadPrefabData(item);
			allPrefabData[item.Name] = value;
			if (ms.ElapsedMilliseconds > 500)
			{
				yield return null;
				ms.ResetAndRestart();
			}
		}
		Log.Out($"LoadPrefabDatas {allPrefabData.Count} of {prefabs.Count} in {(float)ms.ElapsedMilliseconds * 0.001f} s");
	}

	public static void CleanUp()
	{
		allPrefabData.Clear();
		prefabManagerData?.Cleanup();
		prefabManagerData = null;
		registeredPoiMarkers.Clear();
		lastSelectedBoxInGroup.Clear();
	}

	public static void RegisterPoiMarker(SelectionBox _selBox)
	{
		if (_selBox == null || !(_selBox.UserData is Marker marker))
		{
			return;
		}
		if (!registeredPoiMarkers.Contains(_selBox))
		{
			registeredPoiMarkers.Add(_selBox);
		}
		if (marker.MarkerType == Marker.MarkerTypes.PartSpawn && marker.PartDirty)
		{
			if (!string.IsNullOrEmpty(marker.PartToSpawn))
			{
				if (getCurrentPreviewName(_selBox) != marker.PartToSpawn)
				{
					ThreadManager.StartCoroutine(createPartSpawnPreview(marker, _selBox, _selBox.IsActive || isFirstMarkerInGroup(marker)));
				}
			}
			else
			{
				destroyPreview(_selBox);
			}
		}
		if (_selBox.IsActive)
		{
			SelectionChanged(_selBox);
		}
	}

	public static void UnRegisterPoiMarker(SelectionBox _selBox)
	{
		registeredPoiMarkers.Remove(_selBox);
	}

	public static void SelectionChanged(SelectionBox _selBox)
	{
		if ((bool)_selBox && _selBox.UserData is Marker marker)
		{
			lastSelectedBoxInGroup[marker.GroupName] = _selBox.name;
		}
		UpdateAllColors();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isFirstMarkerInGroup(Marker _currentMarker)
	{
		string groupName = _currentMarker.GroupName;
		foreach (SelectionBox registeredPoiMarker in registeredPoiMarkers)
		{
			if (registeredPoiMarker.UserData is Marker marker && !(marker.GroupName != groupName))
			{
				return marker == _currentMarker;
			}
		}
		return false;
	}

	public static void UpdateAllColors()
	{
		Marker marker = SelectionBoxManager.Instance.Selection?.UserData as Marker;
		int num = marker?.GroupId ?? (-1);
		foreach (SelectionBox registeredPoiMarker in registeredPoiMarkers)
		{
			if (!(registeredPoiMarker.UserData is Marker marker2))
			{
				continue;
			}
			bool flag = marker2 == marker;
			bool flag2 = marker2.GroupId == num;
			if (flag)
			{
				if (marker2.MarkerType == Marker.MarkerTypes.PartSpawn)
				{
					registeredPoiMarker.SetAllFacesColor(new Color(0f, 0f, 0f, 0f));
				}
				else
				{
					registeredPoiMarker.SetAllFacesColor(marker2.GroupColor + new Color(0.5f, 0.5f, 0.5f, 0f));
				}
			}
			else if (flag2)
			{
				registeredPoiMarker.SetAllFacesColor(marker2.GroupColor + new Color(0.2f, 0.2f, 0.2f, 0f));
			}
			else
			{
				registeredPoiMarker.SetAllFacesColor(marker2.GroupColor);
			}
			string value;
			if (flag2)
			{
				makePreviewVisible(registeredPoiMarker, flag);
			}
			else if (lastSelectedBoxInGroup.TryGetValue(marker2.GroupName, out value))
			{
				makePreviewVisible(registeredPoiMarker, registeredPoiMarker.name == value);
			}
			else
			{
				makePreviewVisible(registeredPoiMarker, isFirstMarkerInGroup(marker2));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator createPartSpawnPreview(Marker _currentMarker, SelectionBox _selBox, bool _makeVisible)
	{
		destroyPreview(_selBox);
		Prefab prefab = new Prefab();
		prefab.Load(_currentMarker.PartToSpawn, _applyMapping: true, _fixChildBlocks: false, _allowMissingBlocks: true);
		prefab.Init(0, 0);
		prefab.RotateY(_bLeft: false, (prefab.rotationToFaceNorth + _currentMarker.Rotations) % 4);
		yield return prefab.ToTransform(_genBlockModels: true, _genTerrain: true, _genBlockShapes: true, _fillEmptySpace: false, createPreviewTransform(_selBox), prefab.PrefabName, new Vector3(0f - (float)prefab.size.x / 2f, 0.1f, 0f - (float)prefab.size.z / 2f));
		makePreviewVisible(_selBox, _makeVisible);
	}

	public static void DisplayPrefabPreviewForMarker(SelectionBox _selBox)
	{
		if (_selBox.UserData is Marker marker)
		{
			string str = "ghosttown,countrytown";
			if (PrefabEditModeManager.Instance.VoxelPrefab != null)
			{
				str = PrefabEditModeManager.Instance.VoxelPrefab.PrefabName;
				str = str.Replace("rwg_tile_", "");
				str = str.Split('_')[0];
			}
			bool useAnySizeSmaller = !Marker.MarkerSizes.Contains(new Vector3i(marker.size.x, 0, marker.size.z));
			Prefab previewPrefabWithAnyTags = PrefabManagerData.GetPreviewPrefabWithAnyTags(FastTags<TagGroup.Poi>.Parse(str), -1, new Vector2i(marker.size.x, marker.size.z), useAnySizeSmaller);
			if (previewPrefabWithAnyTags != null)
			{
				previewPrefabWithAnyTags.RotateY(_bLeft: true, (previewPrefabWithAnyTags.rotationToFaceNorth + marker.Rotations) % 4);
				destroyPreview(_selBox);
				GameManager.Instance.StartCoroutine(previewPrefabWithAnyTags.ToTransform(_genBlockModels: true, _genTerrain: true, _genBlockShapes: true, _fillEmptySpace: false, createPreviewTransform(_selBox), previewPrefabWithAnyTags.PrefabName, new Vector3(0f - (float)previewPrefabWithAnyTags.size.x / 2f, (float)previewPrefabWithAnyTags.yOffset + 0.15f, 0f - (float)previewPrefabWithAnyTags.size.z / 2f)));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string getCurrentPreviewName(SelectionBox _selBox)
	{
		Transform previewTransform = getPreviewTransform(_selBox);
		if (previewTransform == null)
		{
			return null;
		}
		if (previewTransform.childCount == 0)
		{
			return null;
		}
		return previewTransform.GetChild(0).name;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void makePreviewVisible(SelectionBox _selBox, bool _visible)
	{
		Transform previewTransform = getPreviewTransform(_selBox);
		if (previewTransform != null)
		{
			previewTransform.gameObject.SetActive(_visible);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform createPreviewTransform(SelectionBox _selBox)
	{
		Transform transform = new GameObject("PrefabPreview").transform;
		transform.parent = _selBox.transform;
		transform.localPosition = Vector3.zero;
		return transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void destroyPreview(SelectionBox _selBox)
	{
		Transform previewTransform = getPreviewTransform(_selBox);
		if (previewTransform != null)
		{
			Object.DestroyImmediate(previewTransform.gameObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform getPreviewTransform(SelectionBox _selBox)
	{
		return _selBox.transform.Find("PrefabPreview");
	}

	public static ReadOnlyCollection<SelectionBox> GetRegisteredBoxes()
	{
		return registeredPoiMarkersReadOnly;
	}
}
