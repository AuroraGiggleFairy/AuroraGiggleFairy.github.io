using System.Collections.Generic;
using PrefabVolumes;
using UnityEngine;

public static class SleeperVolumeToolManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class BlockData
	{
		public BlockSleeper block;

		public Transform prefabT;

		public Vector3i position;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject GroupGameObject;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<Vector3i, List<BlockData>> sleepers = new Dictionary<Vector3i, List<BlockData>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<SelectionBox> registeredSleeperVolumes = new List<SelectionBox>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool xRayOn = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cActiveIndex = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cPriorityIndex = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cNoVolumeIndex = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDarkIndex = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cBanditIndex = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cInfestedIndex = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color[] typeColors = new Color[6]
	{
		new Color(1f, 0.6f, 0.1f),
		new Color(0.7f, 0.7f, 0.7f),
		new Color(1f, 0.1f, 1f),
		new Color(0.02f, 0.02f, 0.02f),
		new Color(0.1f, 1f, 0.1f),
		new Color(1f, 0.1f, 0.1f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Material> typeMats = new List<Material>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color groupSelectedColor = new Color(0.9f, 0.9f, 1f, 0.4f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color[] groupColors = new Color[8]
	{
		new Color(1f, 0.2f, 0.2f, 0.4f),
		new Color(1f, 0.6f, 0.2f, 0.4f),
		new Color(1f, 1f, 0.2f, 0.4f),
		new Color(0.6f, 1f, 0.2f, 0.4f),
		new Color(0.2f, 1f, 0.2f, 0.4f),
		new Color(0.2f, 1f, 0.6f, 0.4f),
		new Color(0.2f, 1f, 1f, 0.4f),
		new Color(0.2f, 0.6f, 1f, 0.4f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static SelectionBox currentSelectionBox;

	[PublicizedFrom(EAccessModifier.Private)]
	public static SelectionBox previousSelectionBox;

	public static SelectionBox SelectedSleeperBox
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			SelectionBox selection = SelectionBoxManager.Instance.Selection;
			if (selection == null)
			{
				return null;
			}
			if (selection.Category != SelectionBoxManager.Instance.CategorySleeperVolume)
			{
				return null;
			}
			return selection;
		}
	}

	public static bool XRayMode
	{
		get
		{
			return xRayOn;
		}
		set
		{
			if (xRayOn == value)
			{
				return;
			}
			xRayOn = value;
			int value2 = (xRayOn ? (-200000000) : (-200000));
			foreach (Material typeMat in typeMats)
			{
				typeMat.SetInt("_Offset", value2);
			}
			UpdateAllSleeperBlockVisuals();
		}
	}

	public static void RegisterSleeperBlock(BlockValue _bv, Transform _prefabTrans, Vector3i _position)
	{
		if (!(_bv.Block is BlockSleeper block))
		{
			Log.Warning("SleeperVolumeToolManager RegisterSleeperBlock not sleeper {0}", _bv);
			return;
		}
		if (typeMats.Count == 0)
		{
			Shader shader = Shader.Find("Game/UI/Sleeper");
			Color[] array = typeColors;
			foreach (Color color in array)
			{
				Material material = new Material(shader);
				material.renderQueue = 4001;
				material.color = color;
				typeMats.Add(material);
			}
		}
		Vector3i worldPos = GameManager.Instance.World.ChunkCache.GetChunkFromWorldPos(_position).GetWorldPos();
		BlockData blockData = new BlockData
		{
			block = block,
			prefabT = _prefabTrans,
			position = _position
		};
		_prefabTrans.position = _prefabTrans.position + _position.ToVector3() + Vector3.one * 0.5f + Vector3.up * 0.01f;
		if (GroupGameObject == null)
		{
			GroupGameObject = new GameObject();
			GroupGameObject.name = "SleeperVolumeToolManagerPrefabs";
		}
		_prefabTrans.parent = GroupGameObject.transform;
		if (!sleepers.TryGetValue(worldPos, out var value))
		{
			value = new List<BlockData>();
			sleepers.Add(worldPos, value);
		}
		value.Add(blockData);
		UpdateSleeperBlockVisuals(blockData, SelectedSleeperBox);
	}

	public static void UnRegisterSleeperBlock(Vector3i _position)
	{
		if (sleepers == null)
		{
			return;
		}
		Vector3i worldPos = GameManager.Instance.World.ChunkCache.GetChunkFromWorldPos(_position).GetWorldPos();
		if (!sleepers.TryGetValue(worldPos, out var value))
		{
			return;
		}
		for (int i = 0; i < value.Count; i++)
		{
			if (value[i].position == _position)
			{
				Object.Destroy(value[i].prefabT.gameObject);
				value.RemoveAt(i);
				break;
			}
		}
	}

	public static void CleanUp()
	{
		if (sleepers != null)
		{
			foreach (KeyValuePair<Vector3i, List<BlockData>> sleeper in sleepers)
			{
				sleeper.Deconstruct(out var _, out var value);
				foreach (BlockData item in value)
				{
					Transform prefabT = item.prefabT;
					if ((bool)prefabT)
					{
						Object.Destroy(prefabT.gameObject);
					}
				}
			}
			sleepers.Clear();
		}
		ClearSleeperVolumes();
	}

	public static void RegisterSleeperVolume(SelectionBox _selBox)
	{
		if (!(_selBox == null) && _selBox.UserData is PrefabSleeperVolume && !registeredSleeperVolumes.Contains(_selBox))
		{
			registeredSleeperVolumes.Add(_selBox);
		}
	}

	public static void UnRegisterSleeperVolume(SelectionBox _selBox)
	{
		registeredSleeperVolumes.Remove(_selBox);
	}

	public static void ClearSleeperVolumes()
	{
		registeredSleeperVolumes.Clear();
	}

	public static void CheckKeys()
	{
		if (!Input.GetKeyDown(KeyCode.RightBracket) || !SelectedSleeperBox || !(SelectedSleeperBox.UserData is PrefabSleeperVolume prefabSleeperVolume) || !PrefabVolumeManager.GetPrefabIdAndVolumeId(SelectedSleeperBox.name, out var _volumeId, out var _prefabInstance))
		{
			return;
		}
		PrefabSleeperVolume prefabSleeperVolume2 = prefabSleeperVolume.CloneGeneric();
		if (InputUtils.ShiftKeyPressed)
		{
			prefabSleeperVolume2.groupId = 0;
			PrefabVolumeManager.Instance.UpdatePropertiesServer(_prefabInstance.id, _volumeId, prefabSleeperVolume2);
			Log.Out($"Set sleeper volume {prefabSleeperVolume.startPos} to group ID {0}");
			return;
		}
		short num = -1;
		if (previousSelectionBox != null && previousSelectionBox.UserData is PrefabSleeperVolume prefabSleeperVolume3)
		{
			num = prefabSleeperVolume3.groupId;
			if (num == 0 && PrefabVolumeManager.GetPrefabIdAndVolumeId(previousSelectionBox.name, out var _volumeId2, out var _prefabInstance2))
			{
				num = _prefabInstance2.prefab.FindSleeperVolumeFreeGroupId();
				PrefabSleeperVolume prefabSleeperVolume4 = prefabSleeperVolume3.CloneGeneric();
				prefabSleeperVolume4.groupId = num;
				PrefabVolumeManager.Instance.UpdatePropertiesServer(_prefabInstance2.id, _volumeId2, prefabSleeperVolume4);
				Log.Out($"Set sleeper volume {prefabSleeperVolume3.startPos} to new group ID {num}");
			}
		}
		if (num >= 0)
		{
			prefabSleeperVolume2.groupId = num;
			PrefabVolumeManager.Instance.UpdatePropertiesServer(_prefabInstance.id, _volumeId, prefabSleeperVolume2);
			Log.Out($"Set sleeper volume {prefabSleeperVolume.startPos} to group ID {num}");
		}
	}

	public static void SetVisible(bool _visible)
	{
		if (_visible)
		{
			SelectionChanged(null);
		}
		else
		{
			hideSleeperBlocks();
		}
	}

	public static void SelectionChanged(SelectionBox _selBox)
	{
		if ((bool)_selBox)
		{
			if ((bool)currentSelectionBox && _selBox != currentSelectionBox)
			{
				previousSelectionBox = currentSelectionBox;
			}
			currentSelectionBox = _selBox;
		}
		UpdateAllVisuals();
	}

	public static void UpdateAllVisuals()
	{
		UpdateAllSleeperBlockVisuals();
		UpdateSleeperVolumeColors();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateSleeperVolumeColors()
	{
		int num = 0;
		if ((bool)SelectedSleeperBox)
		{
			num = ((PrefabSleeperVolume)SelectedSleeperBox.UserData).groupId;
		}
		SelectionCategory categorySleeperVolume = SelectionBoxManager.Instance.CategorySleeperVolume;
		foreach (SelectionBox registeredSleeperVolume in registeredSleeperVolumes)
		{
			PrefabSleeperVolume prefabSleeperVolume = (PrefabSleeperVolume)registeredSleeperVolume.UserData;
			if (prefabSleeperVolume.groupId != 0)
			{
				if (prefabSleeperVolume.groupId == num)
				{
					registeredSleeperVolume.SetAllFacesColor(groupSelectedColor);
				}
				else
				{
					registeredSleeperVolume.SetAllFacesColor(groupColors[prefabSleeperVolume.groupId % groupColors.Length]);
				}
			}
			else
			{
				registeredSleeperVolume.SetAllFacesColor(categorySleeperVolume.colInactive);
			}
		}
		if ((bool)SelectedSleeperBox)
		{
			SelectedSleeperBox.SetAllFacesColor(categorySleeperVolume.colActive);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void hideSleeperBlocks()
	{
		if (sleepers == null)
		{
			return;
		}
		foreach (KeyValuePair<Vector3i, List<BlockData>> sleeper in sleepers)
		{
			sleeper.Deconstruct(out var _, out var value);
			foreach (BlockData item in value)
			{
				Transform prefabT = item.prefabT;
				if ((bool)prefabT)
				{
					prefabT.gameObject.SetActive(value: false);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateAllSleeperBlockVisuals()
	{
		if (sleepers == null)
		{
			return;
		}
		SelectionBox selectedSleeperBox = SelectedSleeperBox;
		foreach (KeyValuePair<Vector3i, List<BlockData>> sleeper in sleepers)
		{
			sleeper.Deconstruct(out var _, out var value);
			foreach (BlockData item in value)
			{
				UpdateSleeperBlockVisuals(item, selectedSleeperBox);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateSleeperBlockVisuals(BlockData _data, SelectionBox _selectedBox)
	{
		Transform prefabT = _data.prefabT;
		if (!SelectionBoxManager.Instance.CategorySleeperVolume.IsVisible() || (_selectedBox == null && !xRayOn))
		{
			prefabT.gameObject.SetActive(value: false);
			return;
		}
		Vector3i vector3i = Vector3i.min;
		Vector3i vector3i2 = Vector3i.min;
		if (_selectedBox != null)
		{
			vector3i = Vector3i.FromVector3Rounded(_selectedBox.bounds.min);
			vector3i2 = Vector3i.FromVector3Rounded(_selectedBox.bounds.max);
		}
		Vector3i position = _data.position;
		if (position.x >= vector3i.x && position.x < vector3i2.x && position.y >= vector3i.y && position.y < vector3i2.y && position.z >= vector3i.z && position.z < vector3i2.z)
		{
			int index = 0;
			DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
			if (dynamicPrefabDecorator == null)
			{
				return;
			}
			PrefabInstance prefabFromWorldPosInside = dynamicPrefabDecorator.GetPrefabFromWorldPosInside(position.x, position.z);
			if (prefabFromWorldPosInside == null)
			{
				Log.Error("No prefab found for SleeperBlock position!\n" + StackTraceUtility.ExtractStackTrace());
				return;
			}
			Vector3i pos = position - prefabFromWorldPosInside.boundingBoxPosition;
			PrefabSleeperVolume prefabSleeperVolume = prefabFromWorldPosInside.prefab.FindSleeperVolume(pos);
			if (prefabSleeperVolume != null && prefabSleeperVolume.isPriority)
			{
				index = 1;
			}
			if (_data.block.spawnMode == BlockSleeper.eMode.Bandit)
			{
				index = 4;
			}
			if (_data.block.spawnMode == BlockSleeper.eMode.Infested)
			{
				index = 5;
			}
			prefabT.gameObject.SetActive(value: true);
			SetMats(prefabT, typeMats[index]);
		}
		else if (!InAnyVolume(position))
		{
			prefabT.gameObject.SetActive(value: true);
			SetMats(prefabT, typeMats[2]);
		}
		else if (xRayOn && _selectedBox == null)
		{
			prefabT.gameObject.SetActive(value: true);
			SetMats(prefabT, typeMats[3]);
		}
		else
		{
			prefabT.gameObject.SetActive(value: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool InAnyVolume(Vector3i _pos)
	{
		Vector3i zero = Vector3i.zero;
		Vector3i zero2 = Vector3i.zero;
		foreach (SelectionBox registeredSleeperVolume in registeredSleeperVolumes)
		{
			zero.RoundToInt(registeredSleeperVolume.bounds.min);
			zero2.RoundToInt(registeredSleeperVolume.bounds.max);
			if (_pos.x >= zero.x && _pos.x < zero2.x && _pos.y >= zero.y && _pos.y < zero2.y && _pos.z >= zero.z && _pos.z < zero2.z)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetMats(Transform _t, Material _mat)
	{
		int value = (xRayOn ? (-200000000) : (-200000));
		_mat.SetInt("_Offset", value);
		MeshRenderer[] componentsInChildren = _t.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sharedMaterial = _mat;
		}
	}
}
