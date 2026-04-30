using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WoPropsSleeperVolume : XUiController, ISelectionBoxCallback
{
	public struct VolumeStats
	{
		public int index;

		public Vector3i pos;

		public Vector3i size;

		public string groupName;

		public int sleeperCount;

		public int spawnCountMin;

		public int spawnCountMax;

		public bool isPriority;

		public bool isQuestExclude;
	}

	public struct CountPreset(short _min, short _max, string _name)
	{
		public readonly short min = _min;

		public readonly short max = _max;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string name = _name;

		public override string ToString()
		{
			return name;
		}
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView triggersBox;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelSleeperCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtGroupId;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool cbxPriority;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool cbxQuestExclude;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<CountPreset> cbxCountPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSpawnMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSpawnMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<SleeperVolume.ETriggerType> cbxTrigger;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtMinScript;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SpawnersList spawnersList;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance m_selectedPrefabInstance;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bSleeperVolumeChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabTriggerEditorList triggeredByList;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showTriggeredBy;

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_WoPropsSleeperVolume instance;

	public static int selectedVolumeIndex
	{
		get
		{
			if (instance != null && instance.m_selectedPrefabInstance != null)
			{
				return instance.selIdx;
			}
			return -1;
		}
	}

	public static PrefabInstance selectedPrefabInstance
	{
		get
		{
			if (instance != null)
			{
				return instance.m_selectedPrefabInstance;
			}
			return null;
		}
	}

	public List<byte> TriggeredByIndices
	{
		get
		{
			if (m_selectedPrefabInstance != null)
			{
				return m_selectedPrefabInstance.prefab.SleeperVolumes[selIdx].triggeredByIndices;
			}
			return null;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		instance = this;
		labelIndex = GetChildById("labelIndex").ViewComponent as XUiV_Label;
		labelPosition = GetChildById("labelPosition").ViewComponent as XUiV_Label;
		labelSize = GetChildById("labelSize").ViewComponent as XUiV_Label;
		labelSleeperCount = GetChildById("labelSleeperCount").ViewComponent as XUiV_Label;
		labelGroup = GetChildById("labelGroup").ViewComponent as XUiV_Label;
		txtGroupId = (XUiC_TextInput)GetChildById("groupId");
		txtGroupId.OnChangeHandler += TxtGroupId_OnChangeHandler;
		cbxPriority = (XUiC_ComboBoxBool)GetChildById("cbxPriority");
		cbxPriority.OnValueChanged += CbxPriority_OnValueChanged;
		cbxQuestExclude = (XUiC_ComboBoxBool)GetChildById("cbxQuestExclude");
		cbxQuestExclude.OnValueChanged += CbxQuestExclude_OnValueChanged;
		cbxCountPreset = (XUiC_ComboBoxList<CountPreset>)GetChildById("cbxCountPreset");
		cbxCountPreset.OnValueChanged += CbxCountPreset_OnValueChanged;
		cbxCountPreset.Elements.Add(new CountPreset(-1, -1, "Custom"));
		cbxCountPreset.Elements.Add(new CountPreset(1, 2, "12"));
		cbxCountPreset.Elements.Add(new CountPreset(2, 3, "23"));
		cbxCountPreset.Elements.Add(new CountPreset(3, 4, "34"));
		cbxCountPreset.Elements.Add(new CountPreset(4, 5, "45"));
		cbxCountPreset.Elements.Add(new CountPreset(5, 6, "56"));
		cbxCountPreset.Elements.Add(new CountPreset(6, 7, "67"));
		cbxCountPreset.Elements.Add(new CountPreset(7, 8, "78"));
		cbxCountPreset.Elements.Add(new CountPreset(8, 9, "89"));
		cbxCountPreset.Elements.Add(new CountPreset(9, 10, "910"));
		cbxCountPreset.MinIndex = 1;
		txtSpawnMin = (XUiC_TextInput)GetChildById("spawnMin");
		txtSpawnMin.OnChangeHandler += TxtSpawnMin_OnChangeHandler;
		txtSpawnMax = (XUiC_TextInput)GetChildById("spawnMax");
		txtSpawnMax.OnChangeHandler += TxtSpawnMax_OnChangeHandler;
		cbxTrigger = (XUiC_ComboBoxEnum<SleeperVolume.ETriggerType>)GetChildById("cbxTrigger");
		cbxTrigger.OnValueChanged += CbxTrigger_OnValueChanged;
		txtMinScript = (XUiC_TextInput)GetChildById("script");
		txtMinScript.OnChangeHandler += TxtMinScript_OnChangeHandler;
		spawnersList = (XUiC_SpawnersList)GetChildById("spawners");
		spawnersList.SelectionChanged += SpawnersList_SelectionChanged;
		spawnersList.SelectableEntries = false;
		triggersBox = GetChildById("triggersBox").ViewComponent;
		triggeredByList = GetChildById("triggeredBy") as XUiC_PrefabTriggerEditorList;
		if (triggeredByList != null)
		{
			triggeredByList.SelectionChanged += TriggeredByList_SelectionChanged;
		}
		XUiController childById = GetChildById("addTriggeredByButton");
		if (childById != null)
		{
			childById.OnPress += HandleAddTriggeredByEntry;
		}
		if (SelectionBoxManager.Instance != null)
		{
			SelectionBoxManager.Instance.GetCategory("SleeperVolume").SetCallback(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleAddTriggeredByEntry(XUiController _sender, int _mouseButton)
	{
		TriggerOnAddTriggersPressed();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtGroupId_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!_changeFromCode && _text.Length > 0 && m_selectedPrefabInstance != null)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(m_selectedPrefabInstance.prefab.SleeperVolumes[selIdx]);
			short groupId = StringParsers.ParseSInt16(_text);
			prefabSleeperVolume.groupId = groupId;
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(m_selectedPrefabInstance.id, selIdx, prefabSleeperVolume);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxPriority_OnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		if (m_selectedPrefabInstance != null)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(m_selectedPrefabInstance.prefab.SleeperVolumes[selIdx]);
			prefabSleeperVolume.isPriority = _newValue;
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(m_selectedPrefabInstance.id, selIdx, prefabSleeperVolume);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxQuestExclude_OnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		if (m_selectedPrefabInstance != null)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(m_selectedPrefabInstance.prefab.SleeperVolumes[selIdx]);
			prefabSleeperVolume.isQuestExclude = _newValue;
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(m_selectedPrefabInstance.id, selIdx, prefabSleeperVolume);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int FindCountPresetIndex(int _min, int _max)
	{
		for (int i = 0; i < cbxCountPreset.Elements.Count; i++)
		{
			if (cbxCountPreset.Elements[i].min == _min && cbxCountPreset.Elements[i].max == _max)
			{
				return i;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateCountPresetLabel()
	{
		if (m_selectedPrefabInstance != null)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = m_selectedPrefabInstance.prefab.SleeperVolumes[selIdx];
			int num = FindCountPresetIndex(prefabSleeperVolume.spawnCountMin, prefabSleeperVolume.spawnCountMax);
			if (num < 0)
			{
				cbxCountPreset.MinIndex = 0;
				cbxCountPreset.SelectedIndex = 0;
			}
			else
			{
				cbxCountPreset.MinIndex = 1;
				cbxCountPreset.SelectedIndex = num;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxCountPreset_OnValueChanged(XUiController _sender, CountPreset _oldvalue, CountPreset _newvalue)
	{
		cbxCountPreset.MinIndex = 1;
		if (m_selectedPrefabInstance != null)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(m_selectedPrefabInstance.prefab.SleeperVolumes[selIdx]);
			prefabSleeperVolume.spawnCountMin = _newvalue.min;
			prefabSleeperVolume.spawnCountMax = _newvalue.max;
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(m_selectedPrefabInstance.id, selIdx, prefabSleeperVolume);
		}
		UpdateCountPresetLabel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtSpawnMin_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!_changeFromCode && _text.Length > 0)
		{
			short spawnCountMin = StringParsers.ParseSInt16(_text);
			if (m_selectedPrefabInstance != null)
			{
				Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(m_selectedPrefabInstance.prefab.SleeperVolumes[selIdx]);
				prefabSleeperVolume.spawnCountMin = spawnCountMin;
				PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(m_selectedPrefabInstance.id, selIdx, prefabSleeperVolume);
				UpdateCountPresetLabel();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtSpawnMax_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!_changeFromCode && _text.Length > 0)
		{
			short spawnCountMax = StringParsers.ParseSInt16(_text);
			if (m_selectedPrefabInstance != null)
			{
				Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(m_selectedPrefabInstance.prefab.SleeperVolumes[selIdx]);
				prefabSleeperVolume.spawnCountMax = spawnCountMax;
				PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(m_selectedPrefabInstance.id, selIdx, prefabSleeperVolume);
				UpdateCountPresetLabel();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxTrigger_OnValueChanged(XUiController _sender, SleeperVolume.ETriggerType _oldValue, SleeperVolume.ETriggerType _newValue)
	{
		if (m_selectedPrefabInstance != null)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(m_selectedPrefabInstance.prefab.SleeperVolumes[selIdx]);
			prefabSleeperVolume.SetTrigger(_newValue);
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(m_selectedPrefabInstance.id, selIdx, prefabSleeperVolume);
			triggersBox.IsVisible = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtMinScript_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!_changeFromCode && m_selectedPrefabInstance != null)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(m_selectedPrefabInstance.prefab.SleeperVolumes[selIdx]);
			prefabSleeperVolume.minScript = MinScript.ConvertFromUIText(_text);
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(m_selectedPrefabInstance.id, selIdx, prefabSleeperVolume);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnersList_SelectionChanged(XUiC_ListEntry<XUiC_SpawnersList.SpawnerEntry> _previousEntry, XUiC_ListEntry<XUiC_SpawnersList.SpawnerEntry> _newEntry)
	{
		string groupName = null;
		if (_newEntry != null)
		{
			groupName = _newEntry.GetEntry().name;
		}
		if (m_selectedPrefabInstance != null)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(m_selectedPrefabInstance.prefab.SleeperVolumes[selIdx]);
			prefabSleeperVolume.groupName = groupName;
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(m_selectedPrefabInstance.id, selIdx, prefabSleeperVolume);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggeredByList_SelectionChanged(XUiC_ListEntry<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry> _previousEntry, XUiC_ListEntry<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry> _newEntry)
	{
		if (_newEntry == null)
		{
			return;
		}
		byte _result = 0;
		if (StringParsers.TryParseUInt8(_newEntry.GetEntry().name, out _result))
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(m_selectedPrefabInstance.prefab.SleeperVolumes[selIdx]);
			if (prefabSleeperVolume != null)
			{
				HandleTriggersSetting(prefabSleeperVolume, _result, isTriggers: false, GameManager.Instance.World);
			}
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(m_selectedPrefabInstance.id, selIdx, prefabSleeperVolume);
		}
		_newEntry.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleTriggersSetting(Prefab.PrefabSleeperVolume psv, byte triggerLayer, bool isTriggers, World _world)
	{
		if (_world.IsEditor() && !isTriggers)
		{
			if (psv.HasTriggeredBy(triggerLayer))
			{
				psv.RemoveTriggeredByFlag(triggerLayer);
			}
			else
			{
				psv.SetTriggeredByFlag(triggerLayer);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggerOnAddTriggersPressed()
	{
		if (m_selectedPrefabInstance != null)
		{
			m_selectedPrefabInstance.prefab.AddNewTriggerLayer();
			triggeredByList.RebuildList();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		UpdateCountPresetLabel();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (m_selectedPrefabInstance != null)
		{
			if (bSleeperVolumeChanged)
			{
				bSleeperVolumeChanged = false;
				m_selectedPrefabInstance.prefab.CountSleeperSpawnsInVolume(GameManager.Instance.World, m_selectedPrefabInstance.boundingBoxPosition, selIdx);
				UpdateCountPresetLabel();
			}
			Prefab.PrefabSleeperVolume prefabSleeperVolume = m_selectedPrefabInstance.prefab.SleeperVolumes[selIdx];
			txtGroupId.Text = prefabSleeperVolume.groupId.ToString();
			cbxPriority.Value = prefabSleeperVolume.isPriority;
			cbxQuestExclude.Value = prefabSleeperVolume.isQuestExclude;
			labelIndex.Text = selIdx.ToString();
			labelPosition.Text = prefabSleeperVolume.startPos.ToString();
			labelSize.Text = prefabSleeperVolume.size.ToString();
			labelSleeperCount.Text = m_selectedPrefabInstance.prefab.Transient_NumSleeperSpawns.ToString();
			labelGroup.Text = GameStageGroup.MakeDisplayName(prefabSleeperVolume.groupName);
			txtSpawnMin.Text = prefabSleeperVolume.spawnCountMin.ToString();
			txtSpawnMax.Text = prefabSleeperVolume.spawnCountMax.ToString();
			cbxTrigger.Value = (SleeperVolume.ETriggerType)(prefabSleeperVolume.flags & 7);
			txtMinScript.Text = MinScript.ConvertToUIText(prefabSleeperVolume.minScript);
		}
		else
		{
			txtGroupId.Text = string.Empty;
			cbxPriority.Value = false;
			cbxQuestExclude.Value = false;
			labelIndex.Text = string.Empty;
			labelPosition.Text = string.Empty;
			labelSize.Text = string.Empty;
			labelSleeperCount.Text = string.Empty;
			labelGroup.Text = string.Empty;
			txtSpawnMin.Text = string.Empty;
			txtSpawnMax.Text = string.Empty;
			cbxTrigger.Value = SleeperVolume.ETriggerType.Active;
			txtMinScript.Text = string.Empty;
		}
		triggersBox.IsVisible = true;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (SelectionBoxManager.Instance != null)
		{
			SelectionBoxManager.Instance.GetCategory("SleeperVolume").SetCallback(null);
		}
		instance = null;
	}

	public bool OnSelectionBoxActivated(string _category, string _name, bool _bActivated)
	{
		if (_bActivated)
		{
			if (getPrefabIdAndVolumeId(_name, out var _, out var _volumeId))
			{
				selIdx = _volumeId;
			}
		}
		else
		{
			m_selectedPrefabInstance = null;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool getPrefabIdAndVolumeId(string _name, out int _prefabInstanceId, out int _volumeId)
	{
		_prefabInstanceId = (_volumeId = 0);
		string[] array = _name.Split('.');
		if (array.Length > 1)
		{
			string[] array2 = array[1].Split('_');
			if (array2.Length > 1 && int.TryParse(array2[1], out _volumeId) && int.TryParse(array2[0], out _prefabInstanceId))
			{
				m_selectedPrefabInstance = PrefabSleeperVolumeManager.Instance.GetPrefabInstance(_prefabInstanceId);
				bSleeperVolumeChanged = true;
				Prefab prefab = m_selectedPrefabInstance.prefab;
				triggeredByList.EditPrefab = prefab;
				triggeredByList.SleeperOwner = this;
				triggeredByList.IsTriggers = false;
				if (prefab.TriggerLayers.Count == 0)
				{
					prefab.AddInitialTriggerLayers();
				}
				return true;
			}
		}
		return false;
	}

	public static void SleeperVolumeChanged(int _prefabInstanceId, int _volumeId)
	{
		if (selectedPrefabInstance != null && selectedPrefabInstance.id == _prefabInstanceId && selectedVolumeIndex == _volumeId)
		{
			instance.bSleeperVolumeChanged = true;
		}
	}

	public void OnSelectionBoxMoved(string _category, string _name, Vector3 _moveVector)
	{
		if (m_selectedPrefabInstance != null)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(m_selectedPrefabInstance.prefab.SleeperVolumes[selIdx]);
			prefabSleeperVolume.startPos += new Vector3i(_moveVector);
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(m_selectedPrefabInstance.id, selIdx, prefabSleeperVolume);
		}
	}

	public void OnSelectionBoxSized(string _category, string _name, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		if (m_selectedPrefabInstance != null)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(m_selectedPrefabInstance.prefab.SleeperVolumes[selIdx]);
			prefabSleeperVolume.size += new Vector3i(_dEast + _dWest, _dTop + _dBottom, _dNorth + _dSouth);
			prefabSleeperVolume.startPos += new Vector3i(-_dWest, -_dBottom, -_dSouth);
			Vector3i size = prefabSleeperVolume.size;
			if (size.x < 2)
			{
				size = new Vector3i(1, size.y, size.z);
			}
			if (size.y < 2)
			{
				size = new Vector3i(size.x, 1, size.z);
			}
			if (size.z < 2)
			{
				size = new Vector3i(size.x, size.y, 1);
			}
			prefabSleeperVolume.size = size;
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(m_selectedPrefabInstance.id, selIdx, prefabSleeperVolume);
		}
	}

	public void OnSelectionBoxMirrored(Vector3i _axis)
	{
	}

	public bool OnSelectionBoxDelete(string _category, string _name)
	{
		foreach (LocalPlayerUI playerUI in LocalPlayerUI.PlayerUIs)
		{
			if (playerUI.windowManager.IsModalWindowOpen())
			{
				SelectionBoxManager.Instance.SetActive(_category, _name, _bActive: true);
				return false;
			}
		}
		if (getPrefabIdAndVolumeId(_name, out var _, out var _volumeId))
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(m_selectedPrefabInstance.prefab.SleeperVolumes[_volumeId]);
			prefabSleeperVolume.used = false;
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(m_selectedPrefabInstance.id, _volumeId, prefabSleeperVolume);
			return true;
		}
		return false;
	}

	public bool OnSelectionBoxIsAvailable(string _category, EnumSelectionBoxAvailabilities _criteria)
	{
		if (_criteria != EnumSelectionBoxAvailabilities.CanShowProperties)
		{
			return _criteria == EnumSelectionBoxAvailabilities.CanResize;
		}
		return true;
	}

	public void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager)
	{
		if (SelectionBoxManager.Instance.GetSelected(out var _selectedCategory, out var _) && _selectedCategory.Equals("SleeperVolume"))
		{
			_windowManager.SwitchVisible(ID);
		}
	}

	public void OnSelectionBoxRotated(string _category, string _name)
	{
	}

	public static bool GetSelectedVolumeStats(out VolumeStats _stats)
	{
		_stats = default(VolumeStats);
		int num = selectedVolumeIndex;
		if (num >= 0)
		{
			if (instance.bSleeperVolumeChanged)
			{
				instance.bSleeperVolumeChanged = false;
				selectedPrefabInstance.prefab.CountSleeperSpawnsInVolume(GameManager.Instance.World, selectedPrefabInstance.boundingBoxPosition, num);
				instance.UpdateCountPresetLabel();
			}
			Prefab.PrefabSleeperVolume prefabSleeperVolume = selectedPrefabInstance.prefab.SleeperVolumes[num];
			_stats.index = num;
			_stats.pos = selectedPrefabInstance.boundingBoxPosition + prefabSleeperVolume.startPos;
			_stats.size = prefabSleeperVolume.size;
			_stats.groupName = GameStageGroup.MakeDisplayName(prefabSleeperVolume.groupName);
			_stats.isPriority = prefabSleeperVolume.isPriority;
			_stats.isQuestExclude = prefabSleeperVolume.isQuestExclude;
			_stats.sleeperCount = selectedPrefabInstance.prefab.Transient_NumSleeperSpawns;
			_stats.spawnCountMin = prefabSleeperVolume.spawnCountMin;
			_stats.spawnCountMax = prefabSleeperVolume.spawnCountMax;
			return true;
		}
		return false;
	}
}
