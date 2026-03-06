using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class DynamicPrefabDecorator : IDynamicDecorator, ISelectionBoxCallback
{
	public class TraderComparer : IComparer<TraderArea>
	{
		public int Compare(TraderArea _ta1, TraderArea _ta2)
		{
			return _ta1.ProtectPosition.x - _ta2.ProtectPosition.x;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cPrefabMaxRadius = 200;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, Prefab> prefabCache = new Dictionary<string, Prefab>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, Prefab[]> prefabCacheRotations = new Dictionary<string, Prefab[]>();

	public List<PrefabInstance> allPrefabs = new List<PrefabInstance>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PrefabInstance> poiPrefabs = new List<PrefabInstance>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSortNeeded = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PrefabInstance> allPrefabsSorted = new List<PrefabInstance>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TraderArea> traderAreas = new List<TraderArea>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int id;

	public PrefabInstance ActivePrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, bool> prefabMeshExisting = new Dictionary<string, bool>();

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> streetTileTag = FastTags<TagGroup.Poi>.Parse("streettile");

	public int ProtectSizeXMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public TraderComparer traderComparer = new TraderComparer();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PrefabInstance> decorateChunkPIs = new List<PrefabInstance>();

	public static int PrefabPreviewLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Vector3 boundsPad = new Vector3(0.001f, 0.001f, 0.001f);

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockValueTerrainFiller;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockValueTerrainFiller2;

	public event Action<PrefabInstance> OnPrefabLoaded;

	public event Action<PrefabInstance> OnPrefabChanged;

	public event Action<PrefabInstance> OnPrefabRemoved;

	public IEnumerator Load(string _path)
	{
		if (!SdFile.Exists(_path + "/prefabs.xml"))
		{
			yield break;
		}
		MicroStopwatch msw = new MicroStopwatch(_bStart: true);
		XmlFile xmlFile;
		try
		{
			id = 0;
			xmlFile = new XmlFile(_path, "prefabs");
		}
		catch (Exception ex)
		{
			Log.Error("Loading prefabs xml file for level '" + Path.GetFileName(_path) + "': " + ex.Message);
			Log.Exception(ex);
			yield break;
		}
		int i = 0;
		int totalPrefabs = xmlFile.XmlDoc.Root.Elements("decoration").Count();
		LocalPlayerUI ui = LocalPlayerUI.primaryUI;
		bool progressWindowOpen = (bool)ui && ui.windowManager.IsWindowOpen(XUiC_ProgressWindow.ID);
		foreach (XElement item in xmlFile.XmlDoc.Root.Elements("decoration"))
		{
			try
			{
				i++;
				if (item.HasAttribute("name"))
				{
					string attribute = item.GetAttribute("name");
					Vector3i vector3i = Vector3i.Parse(item.GetAttribute("position"));
					StringParsers.TryParseBool(item.GetAttribute("y_is_groundlevel"), out var _result);
					byte rotation = 0;
					if (item.HasAttribute("rotation"))
					{
						rotation = byte.Parse(item.GetAttribute("rotation"));
					}
					Prefab prefabRotated = GetPrefabRotated(attribute, rotation);
					if (prefabRotated == null)
					{
						Log.Warning("Could not load prefab '" + attribute + "'. Skipping it");
						continue;
					}
					if (_result)
					{
						vector3i.y += prefabRotated.yOffset;
					}
					if (prefabRotated.bTraderArea && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						AddTrader(new TraderArea(vector3i, prefabRotated.size, prefabRotated.TraderAreaProtect, prefabRotated.TeleportVolumes));
					}
					PrefabInstance prefabInstance = new PrefabInstance(id++, prefabRotated.location, vector3i, rotation, prefabRotated, 0);
					AddPrefab(prefabInstance, prefabInstance.prefab.HasQuestTag());
				}
			}
			catch (Exception ex2)
			{
				Log.Error("Loading prefabs xml file for level '" + Path.GetFileName(_path) + "': " + ex2.Message);
				Log.Exception(ex2);
			}
			if (msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
			{
				if (progressWindowOpen)
				{
					XUiC_ProgressWindow.SetText(ui, string.Format(Localization.Get("uiLoadCreatingWorldPrefabs"), Math.Min(100.0, 105.0 * (double)i / (double)totalPrefabs).ToString("0")));
				}
				yield return null;
				msw.ResetAndRestart();
			}
		}
		if (progressWindowOpen)
		{
			XUiC_ProgressWindow.SetText(ui, string.Format(Localization.Get("uiLoadCreatingWorldPrefabs"), "100"));
			yield return null;
		}
		SortPrefabs();
		XUiC_ProgressWindow.SetText(ui, Localization.Get("uiLoadCreatingWorld"));
		yield return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SortPrefabs()
	{
		lock (allPrefabsSorted)
		{
			allPrefabsSorted.Clear();
			allPrefabsSorted.AddRange(allPrefabs);
			allPrefabsSorted.Sort([PublicizedFrom(EAccessModifier.Internal)] (PrefabInstance a, PrefabInstance b) => a.boundingBoxPosition.x.CompareTo(b.boundingBoxPosition.x));
			isSortNeeded = false;
		}
	}

	public int GetNextId()
	{
		return id++;
	}

	public bool Save(string _path)
	{
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.CreateXmlDeclaration();
			XmlElement node = xmlDocument.AddXmlElement("prefabs");
			for (int i = 0; i < allPrefabs.Count; i++)
			{
				PrefabInstance prefabInstance = allPrefabs[i];
				if (prefabInstance != null)
				{
					string value = "";
					Vector3i boundingBoxPosition = prefabInstance.boundingBoxPosition;
					if (prefabInstance.prefab != null && prefabInstance.prefab.location.Type != PathAbstractions.EAbstractedLocationType.None)
					{
						value = prefabInstance.prefab.PrefabName;
						boundingBoxPosition.y -= prefabInstance.prefab.yOffset;
					}
					else if (prefabInstance.location.Type != PathAbstractions.EAbstractedLocationType.None)
					{
						value = prefabInstance.location.Name;
					}
					node.AddXmlElement("decoration").SetAttrib("type", "model").SetAttrib("name", value)
						.SetAttrib("position", boundingBoxPosition.ToStringNoBlanks())
						.SetAttrib("rotation", prefabInstance.rotation.ToString())
						.SetAttrib("y_is_groundlevel", "true");
				}
			}
			xmlDocument.SdSave(_path + "/prefabs.xml");
			return true;
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
			Log.Error(ex.StackTrace);
			return false;
		}
	}

	public void Cleanup()
	{
		prefabCache.Clear();
		prefabCacheRotations.Clear();
		prefabMeshExisting.Clear();
	}

	public List<PrefabInstance> GetDynamicPrefabs()
	{
		return allPrefabs;
	}

	public void AddPrefab(PrefabInstance _pi, bool _isPOI = false)
	{
		allPrefabs.Add(_pi);
		if (_isPOI)
		{
			poiPrefabs.Add(_pi);
		}
		isSortNeeded = true;
	}

	public void RemovePrefab(PrefabInstance _pi)
	{
		allPrefabs.Remove(_pi);
		poiPrefabs.Remove(_pi);
		lock (allPrefabsSorted)
		{
			allPrefabsSorted.Remove(_pi);
		}
	}

	public List<PrefabInstance> GetPOIPrefabs()
	{
		return poiPrefabs;
	}

	public void ClearTraders()
	{
		traderAreas.Clear();
	}

	public void AddTrader(TraderArea _ta)
	{
		ProtectSizeXMax = Utils.FastMax(ProtectSizeXMax, _ta.ProtectSize.x);
		traderAreas.Add(_ta);
		traderAreas.Sort(traderComparer);
	}

	public List<TraderArea> GetTraderAreas()
	{
		return traderAreas;
	}

	public bool IsWithinTraderArea(Vector3i _minPos, Vector3i _maxPos)
	{
		for (int i = 0; i < traderAreas.Count; i++)
		{
			if (traderAreas[i].Overlaps(_minPos, _maxPos))
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int TraderBinarySearch(int x)
	{
		int num = x - ProtectSizeXMax;
		int num2 = 0;
		int num3 = traderAreas.Count;
		while (num2 < num3)
		{
			int num4 = (num2 + num3) / 2;
			if (traderAreas[num4].ProtectPosition.x < num)
			{
				num2 = num4 + 1;
			}
			else
			{
				num3 = num4;
			}
		}
		return num2;
	}

	public TraderArea GetTraderAtPosition(Vector3i _pos, int _padding)
	{
		TraderArea result = null;
		int num = -_padding;
		int i = TraderBinarySearch(_pos.x - _padding);
		for (int count = traderAreas.Count; i < count; i++)
		{
			TraderArea traderArea = traderAreas[i];
			int num2 = _pos.x - traderArea.ProtectPosition.x;
			if (num2 < num)
			{
				break;
			}
			if (num2 < traderArea.ProtectSize.x + _padding)
			{
				int num3 = _pos.z - traderArea.ProtectPosition.z;
				if (num3 >= num && num3 < traderArea.ProtectSize.z + _padding)
				{
					result = traderArea;
					break;
				}
			}
		}
		return result;
	}

	public void CopyAllPrefabsIntoWorld(World _world, bool _bOverwriteExistingBlocks = false)
	{
		for (int i = 0; i < allPrefabs.Count; i++)
		{
			if (allPrefabs[i].standaloneBlockSize == 0)
			{
				allPrefabs[i].CopyIntoWorld(_world, _CopyEntities: true, _bOverwriteExistingBlocks, FastTags<TagGroup.Global>.none);
				continue;
			}
			Log.Warning("Prefab with standaloneBlockSize={0} not supported", allPrefabs[i].standaloneBlockSize);
		}
	}

	public void CleanAllPrefabsFromWorld(World _world)
	{
		for (int i = 0; i < allPrefabs.Count; i++)
		{
			allPrefabs[i].CleanFromWorld(_world, _bRemoveEntities: true);
		}
	}

	public void ClearAllPrefabs()
	{
		foreach (PrefabInstance allPrefab in allPrefabs)
		{
			CallPrefabRemovedEvent(allPrefab);
		}
		allPrefabs.Clear();
		poiPrefabs.Clear();
		lock (allPrefabsSorted)
		{
			allPrefabsSorted.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CallPrefabRemovedEvent(PrefabInstance _prefabInstance)
	{
		if (this.OnPrefabRemoved != null)
		{
			this.OnPrefabRemoved(_prefabInstance);
		}
	}

	public void CallPrefabChangedEvent(PrefabInstance _prefabInstance)
	{
		isSortNeeded = true;
		if (this.OnPrefabChanged != null)
		{
			this.OnPrefabChanged(_prefabInstance);
		}
	}

	public Prefab GetPrefab(string _name, bool _applyMapping = true, bool _fixChildblocks = true, bool _allowMissingBlocks = false)
	{
		lock (prefabCache)
		{
			if (prefabCache.ContainsKey(_name))
			{
				return prefabCache[_name];
			}
			Prefab prefab = new Prefab();
			if (prefab.Load(_name, _applyMapping, _fixChildblocks, _allowMissingBlocks))
			{
				prefabCache[_name] = prefab;
				return prefab;
			}
			return null;
		}
	}

	public Prefab GetPrefabRotated(string _name, int _rotation, bool _applyMapping = true, bool _fixChildblocks = true, bool _allowMissingBlocks = false)
	{
		_rotation &= 3;
		lock (prefabCache)
		{
			if (prefabCacheRotations.TryGetValue(_name, out var value))
			{
				if (value[_rotation] != null)
				{
					return value[_rotation];
				}
			}
			else
			{
				value = new Prefab[4];
				prefabCacheRotations[_name] = value;
			}
			Prefab prefab = GetPrefab(_name, _applyMapping, _fixChildblocks && _rotation == 0, _allowMissingBlocks);
			if (prefab == null)
			{
				return null;
			}
			if (_rotation > 0)
			{
				prefab = prefab.Clone(sharedData: true);
				prefab.RotateY(_bLeft: true, _rotation);
			}
			value[_rotation] = prefab;
			return prefab;
		}
	}

	public void CreateBoundingBoxes()
	{
		for (int i = 0; i < allPrefabs.Count; i++)
		{
			allPrefabs[i].CreateBoundingBox(_alsoCreateOtherBoxes: false);
		}
	}

	public PrefabInstance GetPrefab(int _id)
	{
		for (int i = 0; i < allPrefabs.Count; i++)
		{
			if (allPrefabs[i].id == _id)
			{
				return allPrefabs[i];
			}
		}
		return null;
	}

	public bool IsActivePrefab(int _id)
	{
		PrefabInstance prefab = GetPrefab(_id);
		if (prefab != null)
		{
			return prefab == ActivePrefab;
		}
		return false;
	}

	public PrefabInstance CreateNewPrefabAndActivate(PathAbstractions.AbstractedLocation _location, Vector3i _position, Prefab _bad, bool _bSetActive = true)
	{
		if (_bad == null)
		{
			_bad = new Prefab(new Vector3i(3, 3, 3));
		}
		PrefabInstance prefabInstance = new PrefabInstance(GetNextId(), _location, _position, 0, _bad, 0);
		prefabInstance.CreateBoundingBox();
		AddPrefab(prefabInstance);
		if (_bSetActive)
		{
			SelectionBoxManager.Instance.SetActive("DynamicPrefabs", prefabInstance.name, _bActive: true);
		}
		if (this.OnPrefabLoaded != null)
		{
			this.OnPrefabLoaded(prefabInstance);
		}
		return prefabInstance;
	}

	public PrefabInstance RemoveActivePrefab(World _world)
	{
		if (ActivePrefab == null)
		{
			return null;
		}
		PrefabInstance activePrefab = ActivePrefab;
		RemovePrefabAndSelection(_world, activePrefab, _bCleanFromWorld: true);
		ActivePrefab = null;
		return activePrefab;
	}

	public void RemovePrefabAndSelection(World _world, PrefabInstance _prefab, bool _bCleanFromWorld)
	{
		if (_bCleanFromWorld)
		{
			_prefab.CleanFromWorld(_world, _bRemoveEntities: true);
		}
		RemovePrefab(_prefab);
		SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").RemoveBox(_prefab.name);
		SelectionBoxManager.Instance.GetCategory("TraderTeleport").RemoveBox(_prefab.name);
		SelectionBoxManager.Instance.GetCategory("InfoVolume").RemoveBox(_prefab.name);
		SelectionBoxManager.Instance.GetCategory("WallVolume").RemoveBox(_prefab.name);
		SelectionBoxManager.Instance.GetCategory("TriggerVolume").RemoveBox(_prefab.name);
		for (int i = 0; i < _prefab.prefab.SleeperVolumes.Count; i++)
		{
			if (_prefab.prefab.SleeperVolumes[i].used)
			{
				SelectionBoxManager.Instance.GetCategory("SleeperVolume").RemoveBox(_prefab.name + "_" + i);
			}
		}
		SelectionBoxManager.Instance.GetCategory("POIMarker").Clear();
		SelectionBoxManager.Instance.GetCategory("SleeperVolume").RemoveBox(_prefab.name);
		CallPrefabRemovedEvent(_prefab);
	}

	public virtual void DecorateChunk(World _world, Chunk _chunk)
	{
		DecorateChunk(_world, _chunk, false);
	}

	public void DecorateChunk(World _world, Chunk _chunk, bool _bForceOverwriteBlocks = false)
	{
		int blockWorldPosX = _chunk.GetBlockWorldPosX(0);
		int blockWorldPosZ = _chunk.GetBlockWorldPosZ(0);
		GetPrefabsAtXZ(blockWorldPosX, blockWorldPosX + 15, blockWorldPosZ, blockWorldPosZ + 15, decorateChunkPIs);
		decorateChunkPIs.Sort(prefabInstanceSizeComparison);
		for (int i = 0; i < decorateChunkPIs.Count; i++)
		{
			PrefabInstance prefabInstance = decorateChunkPIs[i];
			if (prefabInstance.Overlaps(_chunk))
			{
				prefabInstance.CopyIntoChunk(_world, _chunk, _bForceOverwriteBlocks);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int prefabInstanceSizeComparison(PrefabInstance _a, PrefabInstance _b)
	{
		int value = _a.boundingBoxSize.x * _a.boundingBoxSize.z;
		return (_b.boundingBoxSize.x * _b.boundingBoxSize.z).CompareTo(value);
	}

	public bool IsEntityInPrefab(int _entityId)
	{
		for (int i = 0; i < allPrefabs.Count; i++)
		{
			if (allPrefabs[i].Contains(_entityId))
			{
				return true;
			}
		}
		return false;
	}

	public bool OnSelectionBoxActivated(string _category, string _name, bool _bActivated)
	{
		if (_bActivated)
		{
			SelectionBox selectionBox = SelectionBoxManager.Instance.Selection?.box;
			if (selectionBox == null)
			{
				Log.Error("Prefab SelectionBox selected but no prefab defined (OSBA)!");
				return true;
			}
			if (selectionBox.UserData is PrefabInstance activePrefab)
			{
				ActivePrefab = activePrefab;
				ActivePrefab.UpdateImposterView();
			}
			else
			{
				Log.Error("Selected prefab SelectionBox has no PrefabInstance assigned");
				StringParsers.SeparatorPositions separatorPositions = StringParsers.GetSeparatorPositions(_name, '.', 1);
				int _result = 0;
				if (separatorPositions.TotalFound >= 1 && StringParsers.TryParseSInt32(_name, out _result, separatorPositions.Sep1 + 1, separatorPositions.Sep2 - 1))
				{
					ActivePrefab = GetPrefab(_result);
				}
			}
		}
		else
		{
			ActivePrefab = null;
		}
		return true;
	}

	public void OnSelectionBoxMoved(string _category, string _name, Vector3 _moveVector)
	{
		if (ActivePrefab != null)
		{
			if (SelectionBoxManager.Instance.Selection?.box == null)
			{
				Log.Error("Prefab SelectionBox selected but no prefab defined (OSBM)!");
				return;
			}
			ActivePrefab.MoveBoundingBox(new Vector3i(_moveVector));
			ActivePrefab.UpdateImposterView();
		}
	}

	public void OnSelectionBoxSized(string _category, string _name, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		if ((!GameManager.Instance.IsEditMode() || PrefabEditModeManager.Instance.IsActive()) && ActivePrefab != null)
		{
			ActivePrefab.ResizeBoundingBox(new Vector3i(_dEast + _dWest, _dTop + _dBottom, _dNorth + _dSouth));
			ActivePrefab.MoveBoundingBox(new Vector3i(-_dWest, -_dBottom, -_dSouth));
		}
	}

	public void OnSelectionBoxMirrored(Vector3i _axis)
	{
	}

	public bool OnSelectionBoxDelete(string _category, string _name)
	{
		SelectionBox selectionBox = SelectionBoxManager.Instance.GetCategory(_category)?.GetBox(_name);
		if (selectionBox == null)
		{
			Log.Error("SelectionBox null (OSBD)");
			return false;
		}
		(selectionBox.UserData as PrefabInstance)?.DestroyImposterView();
		return false;
	}

	public bool OnSelectionBoxIsAvailable(string _category, EnumSelectionBoxAvailabilities _criteria)
	{
		if (_criteria == EnumSelectionBoxAvailabilities.CanResize)
		{
			return PrefabEditModeManager.Instance.IsActive();
		}
		return _criteria == EnumSelectionBoxAvailabilities.CanShowProperties;
	}

	public void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager)
	{
		XUiC_EditorPanelSelector childByType = _windowManager.playerUI.xui.FindWindowGroupByName(XUiC_EditorPanelSelector.ID).GetChildByType<XUiC_EditorPanelSelector>();
		if (childByType != null)
		{
			childByType.SetSelected("prefabList");
			_windowManager.SwitchVisible(XUiC_InGameMenuWindow.ID);
		}
	}

	public void OnSelectionBoxRotated(string _category, string _name)
	{
		if (SelectionBoxManager.Instance.Selection?.box == null)
		{
			Log.Error("Prefab SelectionBox selected but no prefab defined (OSBR)!");
			return;
		}
		ActivePrefab.RotateAroundY();
		ActivePrefab.UpdateImposterView();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int PrefabBinarySearch(float x)
	{
		if (isSortNeeded)
		{
			SortPrefabs();
		}
		int num = (int)x - 200;
		int num2 = 0;
		int num3 = allPrefabsSorted.Count;
		while (num2 < num3)
		{
			int num4 = (num2 + num3) / 2;
			if (allPrefabsSorted[num4].boundingBoxPosition.x < num)
			{
				num2 = num4 + 1;
			}
			else
			{
				num3 = num4;
			}
		}
		return num2;
	}

	public PrefabInstance GetPrefabAtPosition(Vector3 _position, bool _checkTags = true)
	{
		PrefabInstance prefabInstance = null;
		Vector3i vector3i = Vector3i.Floor(_position);
		int i = PrefabBinarySearch(vector3i.x);
		for (int count = allPrefabsSorted.Count; i < count; i++)
		{
			PrefabInstance prefabInstance2 = allPrefabsSorted[i];
			int num = vector3i.x - prefabInstance2.boundingBoxPosition.x;
			if (num < 0)
			{
				break;
			}
			if (num >= prefabInstance2.boundingBoxSize.x)
			{
				continue;
			}
			int num2 = vector3i.z - prefabInstance2.boundingBoxPosition.z;
			if (num2 < 0 || num2 >= prefabInstance2.boundingBoxSize.z)
			{
				continue;
			}
			int num3 = vector3i.y - prefabInstance2.boundingBoxPosition.y;
			if (num3 < 0 || num3 >= prefabInstance2.boundingBoxSize.y || (_checkTags && prefabInstance2.prefab.Tags.Test_AnySet(streetTileTag)))
			{
				continue;
			}
			prefabInstance = prefabInstance2;
			for (i++; i < count; i++)
			{
				prefabInstance2 = allPrefabsSorted[i];
				num = vector3i.x - prefabInstance2.boundingBoxPosition.x;
				if (num < 0)
				{
					break;
				}
				if (num >= prefabInstance2.boundingBoxSize.x)
				{
					continue;
				}
				num2 = vector3i.z - prefabInstance2.boundingBoxPosition.z;
				if (num2 < 0 || num2 >= prefabInstance2.boundingBoxSize.z)
				{
					continue;
				}
				num3 = vector3i.y - prefabInstance2.boundingBoxPosition.y;
				if (num3 >= 0 && num3 < prefabInstance2.boundingBoxSize.y && (!_checkTags || !prefabInstance2.prefab.Tags.Test_AnySet(streetTileTag)))
				{
					if (prefabInstance.boundingBoxPosition.x != prefabInstance2.boundingBoxPosition.x || prefabInstance.boundingBoxSize.x >= prefabInstance2.boundingBoxPosition.x)
					{
						prefabInstance = prefabInstance2;
					}
					break;
				}
			}
			break;
		}
		return prefabInstance;
	}

	public void GetPrefabsAtXZ(int _xMin, int _xMax, int _zMin, int _zMax, List<PrefabInstance> _list)
	{
		lock (allPrefabsSorted)
		{
			_list.Clear();
			if (isSortNeeded)
			{
				_list.AddRange(allPrefabsSorted);
				return;
			}
			int count = allPrefabsSorted.Count;
			int num = Utils.Fastfloor(_xMin) - 200;
			int num2 = 0;
			int num3 = count;
			while (num2 < num3)
			{
				int num4 = (num2 + num3) / 2;
				if (allPrefabsSorted[num4].boundingBoxPosition.x < num)
				{
					num2 = num4 + 1;
				}
				else
				{
					num3 = num4;
				}
			}
			for (int i = num2; i < count; i++)
			{
				PrefabInstance prefabInstance = allPrefabsSorted[i];
				if (prefabInstance.boundingBoxPosition.x > _xMax)
				{
					break;
				}
				if (prefabInstance.boundingBoxPosition.x + prefabInstance.boundingBoxSize.x > _xMin && prefabInstance.boundingBoxPosition.z <= _zMax && prefabInstance.boundingBoxPosition.z + prefabInstance.boundingBoxSize.z > _zMin)
				{
					_list.Add(prefabInstance);
				}
			}
		}
	}

	public virtual void GetPrefabsAround(Vector3 _position, float _distance, Dictionary<int, PrefabInstance> _prefabs)
	{
		float num = _distance * _distance;
		for (int i = 0; i < allPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance = allPrefabs[i];
			float num2 = _position.x - ((float)prefabInstance.boundingBoxPosition.x + (float)prefabInstance.boundingBoxSize.x * 0.5f);
			float num3 = _position.z - ((float)prefabInstance.boundingBoxPosition.z + (float)prefabInstance.boundingBoxSize.z * 0.5f);
			if (num2 * num2 + num3 * num3 <= num)
			{
				string text = ((prefabInstance.prefab.distantPOIOverride == null) ? prefabInstance.prefab.PrefabName : prefabInstance.prefab.distantPOIOverride);
				if (!prefabMeshExisting.TryGetValue(text, out var value))
				{
					value = PathAbstractions.PrefabImpostersSearchPaths.GetLocation(text).Type != PathAbstractions.EAbstractedLocationType.None;
					prefabMeshExisting[text] = value;
				}
				if (value)
				{
					_prefabs.Add(prefabInstance.id, prefabInstance);
				}
			}
		}
	}

	public virtual void GetPrefabsAround(Vector3 _position, float _nearDistance, float _farDistance, Dictionary<int, PrefabInstance> _prefabsFar, Dictionary<int, PrefabInstance> _prefabsNear)
	{
		Vector2 vector = new Vector2(_position.x, _position.z);
		float num = _farDistance * _farDistance;
		Vector2 vector2 = default(Vector2);
		Vector2 vector3 = default(Vector2);
		Vector2 vector4 = default(Vector2);
		Vector2 vector5 = default(Vector2);
		for (int i = PrefabBinarySearch(vector.x - _farDistance); i < allPrefabsSorted.Count; i++)
		{
			PrefabInstance prefabInstance = allPrefabsSorted[i];
			if (_position.x - (float)prefabInstance.boundingBoxPosition.x < 0f - _farDistance)
			{
				break;
			}
			float num2 = _position.x - ((float)prefabInstance.boundingBoxPosition.x + (float)prefabInstance.boundingBoxSize.x * 0.5f);
			float num3 = _position.z - ((float)prefabInstance.boundingBoxPosition.z + (float)prefabInstance.boundingBoxSize.z * 0.5f);
			if (num2 * num2 + num3 * num3 > num)
			{
				continue;
			}
			vector2.x = prefabInstance.boundingBoxPosition.x;
			vector2.y = prefabInstance.boundingBoxPosition.z;
			vector3.x = vector2.x + (float)prefabInstance.boundingBoxSize.x;
			vector3.y = vector2.y;
			vector4.x = vector2.x;
			vector4.y = vector2.y + (float)prefabInstance.boundingBoxSize.z;
			vector5.x = vector3.x;
			vector5.y = vector4.y;
			if (DynamicMeshManager.IsOutsideDistantTerrain(vector2.x, vector3.x, vector2.y, vector4.y))
			{
				continue;
			}
			Vector2 vector6 = vector2 - vector;
			if (Utils.FastMax(Utils.FastAbs(vector6.x), Utils.FastAbs(vector6.y)) < _nearDistance)
			{
				Vector2 vector7 = vector3 - vector;
				if (Utils.FastMax(Utils.FastAbs(vector7.x), Utils.FastAbs(vector7.y)) < _nearDistance)
				{
					Vector2 vector8 = vector4 - vector;
					if (Utils.FastMax(Utils.FastAbs(vector8.x), Utils.FastAbs(vector8.y)) < _nearDistance)
					{
						Vector2 vector9 = vector5 - vector;
						if (Utils.FastMax(Utils.FastAbs(vector9.x), Utils.FastAbs(vector9.y)) < _nearDistance)
						{
							_prefabsNear.Add(prefabInstance.id, prefabInstance);
							continue;
						}
					}
				}
			}
			string text = ((prefabInstance.prefab.distantPOIOverride == null) ? prefabInstance.prefab.PrefabName : prefabInstance.prefab.distantPOIOverride);
			if (!prefabMeshExisting.TryGetValue(text, out var value))
			{
				value = PathAbstractions.PrefabImpostersSearchPaths.GetLocation(text).Type != PathAbstractions.EAbstractedLocationType.None;
				prefabMeshExisting[text] = value;
			}
			if (value)
			{
				_prefabsFar.Add(prefabInstance.id, prefabInstance);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool ValidPrefabForQuest(EntityTrader trader, PrefabInstance prefab, FastTags<TagGroup.Global> questTag, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1, BiomeFilterTypes biomeFilterType = BiomeFilterTypes.SameBiome, string biomeFilter = "")
	{
		if (!prefab.prefab.bSleeperVolumes || !prefab.prefab.GetQuestTag(questTag))
		{
			return false;
		}
		Vector2 vector = new Vector2(prefab.boundingBoxPosition.x, prefab.boundingBoxPosition.z);
		if (usedPOILocations != null && usedPOILocations.Contains(vector))
		{
			return false;
		}
		if (QuestEventManager.Current.CheckForPOILockouts(entityIDforQuests, vector, out var _) != QuestEventManager.POILockoutReasonTypes.None)
		{
			return false;
		}
		new Vector2((float)prefab.boundingBoxPosition.x + (float)prefab.boundingBoxSize.x / 2f, (float)prefab.boundingBoxPosition.z + (float)prefab.boundingBoxSize.z / 2f);
		if (biomeFilterType != BiomeFilterTypes.AnyBiome)
		{
			string[] array = null;
			BiomeDefinition biomeAt = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt((int)vector.x, (int)vector.y);
			switch (biomeFilterType)
			{
			case BiomeFilterTypes.OnlyBiome:
				if (biomeAt.m_sBiomeName != biomeFilter)
				{
					return false;
				}
				break;
			case BiomeFilterTypes.ExcludeBiome:
			{
				if (array == null)
				{
					array = biomeFilter.Split(',');
				}
				bool flag = false;
				for (int i = 0; i < array.Length; i++)
				{
					if (biomeAt.m_sBiomeName == array[i])
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					return false;
				}
				break;
			}
			case BiomeFilterTypes.SameBiome:
				if (trader != null)
				{
					BiomeDefinition biomeAt2 = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt((int)trader.position.x, (int)trader.position.z);
					if (biomeAt != biomeAt2)
					{
						return false;
					}
				}
				break;
			}
		}
		return true;
	}

	public virtual PrefabInstance GetRandomPOINearTrader(EntityTrader trader, FastTags<TagGroup.Global> questTag, byte difficulty, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1, BiomeFilterTypes biomeFilterType = BiomeFilterTypes.SameBiome, string biomeFilter = "")
	{
		QuestEventManager current = QuestEventManager.Current;
		GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
		int num = trader.PreferredDistanceIndex;
		for (int i = 0; i < 3; i++)
		{
			num %= 3;
			List<PrefabInstance> prefabsForTrader = current.GetPrefabsForTrader(trader.traderArea, difficulty, num, gameRandom);
			if (prefabsForTrader != null)
			{
				for (int j = 0; j < prefabsForTrader.Count; j++)
				{
					PrefabInstance prefabInstance = prefabsForTrader[j];
					if (ValidPrefabForQuest(trader, prefabInstance, questTag, usedPOILocations, entityIDforQuests, biomeFilterType, biomeFilter))
					{
						return prefabInstance;
					}
				}
			}
			num++;
		}
		return null;
	}

	public virtual PrefabInstance GetRandomPOINearWorldPos(Vector2 worldPos, int minSearchDistance, int maxSearchDistance, FastTags<TagGroup.Global> questTag, byte difficulty, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1, BiomeFilterTypes biomeFilterType = BiomeFilterTypes.SameBiome, string biomeFilter = "")
	{
		List<PrefabInstance> prefabsByDifficultyTier = QuestEventManager.Current.GetPrefabsByDifficultyTier(difficulty);
		if (prefabsByDifficultyTier == null)
		{
			return null;
		}
		string[] array = null;
		BiomeDefinition biomeAt = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt((int)worldPos.x, (int)worldPos.y);
		World world = GameManager.Instance.World;
		for (int i = 0; i < 50; i++)
		{
			int index = world.GetGameRandom().RandomRange(prefabsByDifficultyTier.Count);
			PrefabInstance prefabInstance = prefabsByDifficultyTier[index];
			if (!prefabInstance.prefab.bSleeperVolumes || !prefabInstance.prefab.GetQuestTag(questTag) || prefabInstance.prefab.DifficultyTier != difficulty)
			{
				continue;
			}
			Vector2 vector = new Vector2(prefabInstance.boundingBoxPosition.x, prefabInstance.boundingBoxPosition.z);
			if ((usedPOILocations != null && usedPOILocations.Contains(vector)) || QuestEventManager.Current.CheckForPOILockouts(entityIDforQuests, vector, out var _) != QuestEventManager.POILockoutReasonTypes.None)
			{
				continue;
			}
			Vector2 vector2 = new Vector2((float)prefabInstance.boundingBoxPosition.x + (float)prefabInstance.boundingBoxSize.x / 2f, (float)prefabInstance.boundingBoxPosition.z + (float)prefabInstance.boundingBoxSize.z / 2f);
			if (biomeFilterType != BiomeFilterTypes.AnyBiome)
			{
				BiomeDefinition biomeAt2 = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt((int)vector.x, (int)vector.y);
				switch (biomeFilterType)
				{
				case BiomeFilterTypes.OnlyBiome:
					if (biomeAt2.m_sBiomeName != biomeFilter)
					{
						continue;
					}
					break;
				case BiomeFilterTypes.ExcludeBiome:
				{
					if (array == null)
					{
						array = biomeFilter.Split(',');
					}
					bool flag = false;
					for (int j = 0; j < array.Length; j++)
					{
						if (biomeAt2.m_sBiomeName == array[j])
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
					break;
				}
				case BiomeFilterTypes.SameBiome:
					if (biomeAt2 != biomeAt)
					{
						continue;
					}
					break;
				}
			}
			float sqrMagnitude = (worldPos - vector2).sqrMagnitude;
			if (sqrMagnitude < (float)maxSearchDistance && sqrMagnitude > (float)minSearchDistance)
			{
				return prefabInstance;
			}
		}
		return null;
	}

	public virtual PrefabInstance GetClosestPOIToWorldPos(FastTags<TagGroup.Global> questTag, Vector2 worldPos, List<Vector2> excludeList = null, int maxSearchDistanceSquared = -1, bool ignoreCurrentPOI = false, BiomeFilterTypes biomeFilterType = BiomeFilterTypes.SameBiome, string biomeFilter = "", string questKey = "")
	{
		PrefabInstance prefabInstance = null;
		List<Tuple<PrefabInstance, Vector2>> list = new List<Tuple<PrefabInstance, Vector2>>();
		string[] array = null;
		Vector3 pos = new Vector3(worldPos.x, 0f, worldPos.y);
		IBiomeProvider biomeProvider = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider();
		BiomeDefinition biomeDefinition = biomeProvider?.GetBiomeAt((int)worldPos.x, (int)worldPos.y);
		for (int i = 0; i < poiPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance2 = poiPrefabs[i];
			if (prefabInstance2.prefab.PrefabName.Contains("rwg_tile") || (!prefabInstance2.prefab.GetQuestTag(questTag) && !questTag.IsEmpty))
			{
				continue;
			}
			if (ignoreCurrentPOI)
			{
				pos.y = prefabInstance2.boundingBoxPosition.y;
				if (prefabInstance2.Overlaps(pos))
				{
					continue;
				}
			}
			Vector2 item = new Vector2((float)prefabInstance2.boundingBoxPosition.x + (float)prefabInstance2.boundingBoxSize.x / 2f, (float)prefabInstance2.boundingBoxPosition.z + (float)prefabInstance2.boundingBoxSize.z / 2f);
			if (excludeList != null && excludeList.Contains(new Vector2(prefabInstance2.boundingBoxPosition.x, prefabInstance2.boundingBoxPosition.z)))
			{
				continue;
			}
			if (biomeFilterType != BiomeFilterTypes.AnyBiome)
			{
				BiomeDefinition biomeDefinition2 = biomeProvider?.GetBiomeAt((int)item.x, (int)item.y);
				switch (biomeFilterType)
				{
				case BiomeFilterTypes.OnlyBiome:
					if (biomeDefinition2.m_sBiomeName != biomeFilter)
					{
						continue;
					}
					break;
				case BiomeFilterTypes.ExcludeBiome:
				{
					if (array == null)
					{
						array = biomeFilter.Split(',');
					}
					bool flag = false;
					for (int j = 0; j < array.Length; j++)
					{
						if (biomeDefinition2.m_sBiomeName == array[j])
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
					break;
				}
				case BiomeFilterTypes.SameBiome:
					if (biomeDefinition2 != biomeDefinition)
					{
						continue;
					}
					break;
				}
			}
			list.Add(new Tuple<PrefabInstance, Vector2>(prefabInstance2, item));
		}
		float maxSearchDistanceSquared2 = ((maxSearchDistanceSquared < 0) ? float.MaxValue : ((float)maxSearchDistanceSquared));
		if (string.Compare(questKey, "traderquest", ignoreCase: true) == 0)
		{
			return chooseBestTrader(list, worldPos, maxSearchDistanceSquared2);
		}
		return chooseClosestPrefab(list, worldPos, maxSearchDistanceSquared2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance chooseClosestPrefab(List<Tuple<PrefabInstance, Vector2>> prefabCandidates, Vector2 worldPos, float maxSearchDistanceSquared)
	{
		PrefabInstance result = null;
		foreach (Tuple<PrefabInstance, Vector2> prefabCandidate in prefabCandidates)
		{
			float sqrMagnitude = (worldPos - prefabCandidate.Item2).sqrMagnitude;
			if (sqrMagnitude < maxSearchDistanceSquared)
			{
				maxSearchDistanceSquared = sqrMagnitude;
				result = prefabCandidate.Item1;
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance chooseBestTrader(List<Tuple<PrefabInstance, Vector2>> prefabCandidates, Vector2 worldPos, float maxSearchDistanceSquared)
	{
		PrefabInstance result = null;
		int num = 0;
		foreach (Tuple<PrefabInstance, Vector2> prefabCandidate in prefabCandidates)
		{
			if (!((worldPos - prefabCandidate.Item2).sqrMagnitude <= maxSearchDistanceSquared))
			{
				continue;
			}
			TraderArea traderAtPosition = GetTraderAtPosition(new Vector3i(prefabCandidate.Item2.x, 0f, prefabCandidate.Item2.y), 0);
			if (traderAtPosition != null)
			{
				int traderPoiCount = QuestEventManager.Current.GetTraderPoiCount(traderAtPosition, 1, 0);
				if (num < traderPoiCount)
				{
					num = traderPoiCount;
					result = prefabCandidate.Item1;
				}
			}
		}
		return result;
	}

	public virtual PrefabInstance GetPrefabFromWorldPos(int x, int z)
	{
		for (int i = 0; i < allPrefabs.Count; i++)
		{
			if (allPrefabs[i].boundingBoxPosition.x == x && allPrefabs[i].boundingBoxPosition.z == z && !allPrefabs[i].prefab.PrefabName.Contains("rwg_tile") && !allPrefabs[i].prefab.PrefabName.Contains("part_"))
			{
				return allPrefabs[i];
			}
		}
		return null;
	}

	public virtual PrefabInstance GetPrefabFromWorldPosInside(int _x, int _z)
	{
		for (int i = 0; i < allPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance = allPrefabs[i];
			int x = prefabInstance.boundingBoxPosition.x;
			int z = prefabInstance.boundingBoxPosition.z;
			if (x <= _x && z <= _z && x + prefabInstance.boundingBoxSize.x >= _x && z + prefabInstance.boundingBoxSize.z >= _z)
			{
				return allPrefabs[i];
			}
		}
		return null;
	}

	public virtual PrefabInstance GetPrefabFromWorldPosInsideWithOffset(int _x, int _z, int _offset)
	{
		for (int i = 0; i < allPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance = allPrefabs[i];
			int num = prefabInstance.boundingBoxPosition.x - _offset;
			int num2 = prefabInstance.boundingBoxPosition.z - _offset;
			int num3 = prefabInstance.boundingBoxPosition.x + prefabInstance.boundingBoxSize.x + _offset;
			int num4 = prefabInstance.boundingBoxPosition.z + prefabInstance.boundingBoxSize.z + _offset;
			if (num <= _x && num2 <= _z && num3 >= _x && num4 >= _z)
			{
				return allPrefabs[i];
			}
		}
		return null;
	}

	public virtual PrefabInstance GetPrefabFromWorldPosInside(int _x, int _y, int _z)
	{
		for (int i = 0; i < allPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance = allPrefabs[i];
			int x = prefabInstance.boundingBoxPosition.x;
			int y = prefabInstance.boundingBoxPosition.y;
			int z = prefabInstance.boundingBoxPosition.z;
			if (x <= _x && y <= _y && z <= _z && x + prefabInstance.boundingBoxSize.x >= _x && y + prefabInstance.boundingBoxSize.y >= _y && z + prefabInstance.boundingBoxSize.z >= _z)
			{
				return allPrefabs[i];
			}
		}
		return null;
	}

	public virtual List<PrefabInstance> GetPrefabsFromWorldPosInside(Vector3 _pos, FastTags<TagGroup.Global> _questTags)
	{
		_pos += boundsPad;
		List<PrefabInstance> list = new List<PrefabInstance>();
		Bounds bounds = default(Bounds);
		for (int i = 0; i < allPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance = allPrefabs[i];
			if (prefabInstance.prefab.GetQuestTag(_questTags))
			{
				bounds.SetMinMax(prefabInstance.boundingBoxPosition, prefabInstance.boundingBoxPosition + prefabInstance.boundingBoxSize - boundsPad);
				if (bounds.Contains(_pos))
				{
					list.AddRange(GetPrefabsIntersecting(prefabInstance));
				}
			}
		}
		return list.OrderByDescending([PublicizedFrom(EAccessModifier.Internal)] (PrefabInstance pi) => pi.boundingBoxSize.x * pi.boundingBoxSize.z).ToList();
	}

	public virtual List<PrefabInstance> GetPrefabsIntersecting(PrefabInstance parentPI)
	{
		List<PrefabInstance> list = new List<PrefabInstance>();
		list.Add(parentPI);
		Bounds bounds = default(Bounds);
		bounds.SetMinMax(parentPI.boundingBoxPosition, parentPI.boundingBoxPosition + parentPI.boundingBoxSize - boundsPad);
		float num = bounds.size.x * bounds.size.z;
		Bounds bounds2 = default(Bounds);
		for (int i = 0; i < allPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance = allPrefabs[i];
			if (parentPI != prefabInstance)
			{
				bounds2.SetMinMax(prefabInstance.boundingBoxPosition, prefabInstance.boundingBoxPosition + prefabInstance.boundingBoxSize - boundsPad);
				if (bounds.Intersects(bounds2) && bounds2.size.x * bounds2.size.z < num && !list.Contains(prefabInstance))
				{
					list.Add(prefabInstance);
				}
			}
		}
		return list.OrderByDescending([PublicizedFrom(EAccessModifier.Internal)] (PrefabInstance pi) => pi.boundingBoxSize.x * pi.boundingBoxSize.z).ToList();
	}

	public IEnumerator CopyPrefabHeightsIntoHeightMap(int _heightMapWidth, int _heightMapHeight, IBackedArray<ushort> _heightData, int _heightMapScale = 1, ushort[] _topTextures = null)
	{
		MicroStopwatch yieldMs = new MicroStopwatch(_bStart: true);
		if (blockValueTerrainFiller.isair)
		{
			blockValueTerrainFiller = Block.GetBlockValue(Constants.cTerrainFillerBlockName);
			blockValueTerrainFiller2 = Block.GetBlockValue(Constants.cTerrainFiller2BlockName);
		}
		List<PrefabInstance> allPrefabs = GetDynamicPrefabs();
		for (int i = 0; i < allPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance = allPrefabs[i];
			if (prefabInstance.prefab != null)
			{
				copyPrefabsIntoHeightMap(prefabInstance, _heightMapWidth, _heightMapHeight, _heightData, _heightMapScale, _topTextures);
				if (yieldMs.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					yieldMs.ResetAndRestart();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void copyPrefabsIntoHeightMap(PrefabInstance _pi, int _heightMapWidth, int _heightMapHeight, IBackedArray<ushort> _heightData, int _heightMapScale, ushort[] _topTextures = null)
	{
		using IBackedArrayView<ushort> backedArrayView = BackedArrays.CreateSingleView(_heightData, BackedArrayHandleMode.ReadWrite);
		int rotation = _pi.rotation;
		Prefab prefab = _pi.prefab;
		int yOffset = prefab.yOffset;
		Vector3i size = prefab.size;
		int x = _pi.boundingBoxPosition.x;
		int y = _pi.boundingBoxPosition.y;
		int z = _pi.boundingBoxPosition.z;
		if (_pi.boundingBoxPosition.x < -_heightMapWidth / 2 || _pi.boundingBoxPosition.x + size.x > _heightMapWidth / 2 || _pi.boundingBoxPosition.z < -_heightMapHeight / 2 || _pi.boundingBoxPosition.z + size.z > _heightMapHeight / 2)
		{
			Log.Warning($"Prefab {_pi.name} outside of the world bounds (position {_pi.boundingBoxPosition})");
		}
		bool flag = _pi.name.Contains("rwg_tile");
		for (int i = (size.z + _heightMapScale - 1) % _heightMapScale; i < size.z; i += _heightMapScale)
		{
			int num = i + z;
			int num2 = (num / _heightMapScale + _heightMapHeight / 2) * _heightMapWidth;
			int num3 = (num + _heightMapHeight / 2) * _heightMapScale * _heightMapWidth;
			for (int j = (size.x + _heightMapScale - 1) % _heightMapScale; j < size.x; j += _heightMapScale)
			{
				int num4 = j + x;
				int num5 = (num4 / _heightMapScale + _heightMapWidth / 2 + num2) % _heightData.Length;
				int num6 = (num4 + _heightMapWidth / 2) * _heightMapScale + num3;
				for (int k = 0; k < size.y; k++)
				{
					BlockValue blockNoDamage = prefab.GetBlockNoDamage(rotation, j, k, i);
					WaterValue water = prefab.GetWater(j, k, i);
					Block block = blockNoDamage.Block;
					if (blockNoDamage.isair || block == null || !block.shape.IsTerrain() || water.HasMass())
					{
						if (k > -yOffset)
						{
							break;
						}
						if (!flag || k > 0)
						{
							continue;
						}
					}
					sbyte density = prefab.GetDensity(rotation, j, k, i);
					float num7 = y + k + yOffset;
					num7 += (float)(-density) / 128f - 1f;
					if (flag)
					{
						num7 -= (float)yOffset;
					}
					if (num7 <= 0f)
					{
						continue;
					}
					ushort num8 = (ushort)(num7 / 0.0038910506f);
					if (blockNoDamage.type == blockValueTerrainFiller2.type && num8 > backedArrayView[num5])
					{
						continue;
					}
					if (num5 >= 0 && num5 < _heightData.Length && (flag || num8 > backedArrayView[num5]))
					{
						backedArrayView[num5] = num8;
					}
					if (block != null && _topTextures != null && !blockNoDamage.isair && blockNoDamage.type != blockValueTerrainFiller.type && blockNoDamage.type != blockValueTerrainFiller2.type)
					{
						int sideTextureId = block.GetSideTextureId(blockNoDamage, BlockFace.Top, 0);
						if (num6 >= 0 && num6 < _topTextures.Length)
						{
							_topTextures[num6] = (ushort)sideTextureId;
						}
					}
				}
			}
		}
	}

	public void CalculateStats(out int basePrefabCount, out int rotatedPrefabsCount, out int activePrefabCount, out int basePrefabBytes, out int rotatedPrefabBytes, out int activePrefabBytes)
	{
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		lock (prefabCache)
		{
			basePrefabCount = prefabCache.Count;
			basePrefabBytes = 0;
			foreach (Prefab value in prefabCache.Values)
			{
				basePrefabBytes += value.EstimateOwnedBytes();
			}
			rotatedPrefabsCount = 0;
			rotatedPrefabBytes = 0;
			foreach (Prefab[] value2 in prefabCacheRotations.Values)
			{
				for (int i = 1; i < value2.Length; i++)
				{
					Prefab prefab = value2[i];
					if (prefab != null)
					{
						rotatedPrefabsCount++;
						rotatedPrefabBytes += prefab.EstimateOwnedBytes();
					}
				}
			}
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			activePrefabCount = -1;
			activePrefabBytes = -1;
			return;
		}
		List<EntityPlayer> list = GameManager.Instance.World.Players.list;
		HashSet<Prefab> hashSet = new HashSet<Prefab>();
		List<PrefabInstance> list2 = new List<PrefabInstance>();
		foreach (EntityPlayer item in list)
		{
			foreach (long item2 in item.ChunkObserver.chunksAround.list)
			{
				IChunk chunkSync = chunkCache.GetChunkSync(item2);
				if (chunkSync == null)
				{
					continue;
				}
				Vector3i worldPos = chunkSync.GetWorldPos();
				GetPrefabsAtXZ(worldPos.x, worldPos.x + 15, worldPos.z, worldPos.z + 15, list2);
				foreach (PrefabInstance item3 in list2)
				{
					hashSet.Add(item3.prefab);
				}
			}
		}
		activePrefabCount = hashSet.Count;
		activePrefabBytes = 0;
		foreach (Prefab item4 in hashSet)
		{
			activePrefabBytes += item4.EstimateOwnedBytes();
		}
	}
}
