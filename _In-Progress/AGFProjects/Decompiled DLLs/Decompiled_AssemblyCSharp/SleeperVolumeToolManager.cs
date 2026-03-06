using System.Collections.Generic;
using UnityEngine;

public class SleeperVolumeToolManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class BlockData
	{
		public BlockSleeper block;

		public Transform prefabT;

		public Vector3i position;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject GroupGameObject = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<Vector3i, List<BlockData>> sleepers;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<SelectionBox> registeredSleeperVolumes = new List<SelectionBox>();

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
	public static Color groupSelectedColor = new Color(0.9f, 0.9f, 1f, 0.4f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color[] groupColors = new Color[8]
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

	public static void RegisterSleeperBlock(BlockValue _bv, Transform prefabTrans, Vector3i position)
	{
		if (!(_bv.Block is BlockSleeper block))
		{
			Log.Warning("SleeperVolumeToolManager RegisterSleeperBlock not sleeper {0}", _bv);
			return;
		}
		if (sleepers == null)
		{
			sleepers = new Dictionary<Vector3i, List<BlockData>>();
			Shader shader = Shader.Find("Game/UI/Sleeper");
			for (int i = 0; i < typeColors.Length; i++)
			{
				Material material = new Material(shader);
				material.renderQueue = 4001;
				material.color = typeColors[i];
				typeMats.Add(material);
			}
		}
		Vector3i worldPos = GameManager.Instance.World.ChunkClusters[0].GetChunkFromWorldPos(position).GetWorldPos();
		BlockData blockData = new BlockData();
		blockData.block = block;
		blockData.prefabT = prefabTrans;
		blockData.position = position;
		prefabTrans.position = prefabTrans.position + position.ToVector3() + Vector3.one * 0.5f + Vector3.up * 0.01f;
		if (GroupGameObject == null)
		{
			GroupGameObject = new GameObject();
			GroupGameObject.name = "SleeperVolumeToolManagerPrefabs";
		}
		prefabTrans.parent = GroupGameObject.transform;
		if (!sleepers.TryGetValue(worldPos, out var value))
		{
			value = new List<BlockData>();
			sleepers.Add(worldPos, value);
		}
		value.Add(blockData);
		UpdateSleeperVisuals(blockData);
	}

	public static void UnRegisterSleeperBlock(Vector3i position)
	{
		if (sleepers == null)
		{
			return;
		}
		Vector3i worldPos = GameManager.Instance.World.ChunkClusters[0].GetChunkFromWorldPos(position).GetWorldPos();
		if (!sleepers.TryGetValue(worldPos, out var value))
		{
			return;
		}
		for (int i = 0; i < value.Count; i++)
		{
			if (value[i].position == position)
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
				for (int i = 0; i < sleeper.Value.Count; i++)
				{
					Transform prefabT = sleeper.Value[i].prefabT;
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
		if (!registeredSleeperVolumes.Contains(_selBox))
		{
			registeredSleeperVolumes.Add(_selBox);
		}
	}

	public static void UnRegisterSleeperVolume(SelectionBox _selBox)
	{
		if (registeredSleeperVolumes.Contains(_selBox))
		{
			registeredSleeperVolumes.Remove(_selBox);
		}
	}

	public static void ClearSleeperVolumes()
	{
		registeredSleeperVolumes.Clear();
	}

	public static void CheckKeys()
	{
		if (!Input.GetKeyDown(KeyCode.RightBracket) || !currentSelectionBox)
		{
			return;
		}
		Prefab.PrefabSleeperVolume prefabSleeperVolume = (Prefab.PrefabSleeperVolume)currentSelectionBox.UserData;
		if (prefabSleeperVolume == null)
		{
			return;
		}
		short num = -1;
		if (InputUtils.ShiftKeyPressed)
		{
			num = 0;
		}
		else if ((bool)previousSelectionBox)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume2 = (Prefab.PrefabSleeperVolume)previousSelectionBox.UserData;
			if (prefabSleeperVolume2 != null)
			{
				num = prefabSleeperVolume2.groupId;
				if (num == 0)
				{
					PrefabInstance selectedPrefabInstance = XUiC_WoPropsSleeperVolume.selectedPrefabInstance;
					if (selectedPrefabInstance != null)
					{
						num = (prefabSleeperVolume2.groupId = selectedPrefabInstance.prefab.FindSleeperVolumeFreeGroupId());
						Log.Out("Set sleeper volume {0} to new group ID {1}", prefabSleeperVolume2.startPos, num);
					}
				}
			}
		}
		if (num >= 0)
		{
			prefabSleeperVolume.groupId = num;
			SelectionChanged(currentSelectionBox);
			Log.Out("Set sleeper volume {0} to group ID {1}", prefabSleeperVolume.startPos, num);
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
			ShowSleepers(bShow: false);
		}
	}

	public static void SelectionChanged(SelectionBox selBox)
	{
		if ((bool)selBox && selBox != currentSelectionBox)
		{
			previousSelectionBox = currentSelectionBox;
		}
		currentSelectionBox = selBox;
		UpdateSleeperVisuals();
		UpdateVolumeColors();
	}

	public static void UpdateVolumeColors()
	{
		int num = 0;
		if ((bool)currentSelectionBox)
		{
			num = ((Prefab.PrefabSleeperVolume)currentSelectionBox.UserData).groupId;
		}
		for (int i = 0; i < registeredSleeperVolumes.Count; i++)
		{
			SelectionBox selectionBox = registeredSleeperVolumes[i];
			Prefab.PrefabSleeperVolume prefabSleeperVolume = (Prefab.PrefabSleeperVolume)selectionBox.UserData;
			if (prefabSleeperVolume.groupId != 0)
			{
				if (prefabSleeperVolume.groupId == num)
				{
					selectionBox.SetAllFacesColor(groupSelectedColor);
				}
				else
				{
					selectionBox.SetAllFacesColor(groupColors[prefabSleeperVolume.groupId % groupColors.Length]);
				}
			}
			else
			{
				selectionBox.SetAllFacesColor(SelectionBoxManager.ColSleeperVolumeInactive);
			}
		}
		if ((bool)currentSelectionBox)
		{
			currentSelectionBox.SetAllFacesColor(SelectionBoxManager.ColSleeperVolume);
		}
	}

	public static void ShowSleepers(bool bShow = true)
	{
		if (sleepers == null)
		{
			return;
		}
		foreach (KeyValuePair<Vector3i, List<BlockData>> sleeper in sleepers)
		{
			for (int i = 0; i < sleeper.Value.Count; i++)
			{
				Transform prefabT = sleeper.Value[i].prefabT;
				if ((bool)prefabT)
				{
					prefabT.gameObject.SetActive(bShow);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateSleeperVisuals()
	{
		if (sleepers == null)
		{
			return;
		}
		foreach (KeyValuePair<Vector3i, List<BlockData>> sleeper in sleepers)
		{
			List<BlockData> value = sleeper.Value;
			for (int i = 0; i < value.Count; i++)
			{
				UpdateSleeperVisuals(value[i]);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateSleeperVisuals(BlockData data)
	{
		Transform prefabT = data.prefabT;
		if (!SelectionBoxManager.Instance.GetCategory("SleeperVolume").IsVisible() || (currentSelectionBox == null && !xRayOn))
		{
			prefabT.gameObject.SetActive(value: false);
			return;
		}
		Vector3i vector3i = Vector3i.min;
		Vector3i vector3i2 = Vector3i.min;
		SelectionBox selectionBox = currentSelectionBox;
		if (selectionBox != null)
		{
			vector3i = Vector3i.FromVector3Rounded(selectionBox.bounds.min);
			vector3i2 = Vector3i.FromVector3Rounded(selectionBox.bounds.max);
		}
		Vector3i position = data.position;
		if (position.x >= vector3i.x && position.x < vector3i2.x && position.y >= vector3i.y && position.y < vector3i2.y && position.z >= vector3i.z && position.z < vector3i2.z)
		{
			int index = 0;
			PrefabInstance selectedPrefabInstance = XUiC_WoPropsSleeperVolume.selectedPrefabInstance;
			Vector3i pos = position - selectedPrefabInstance.boundingBoxPosition;
			Prefab.PrefabSleeperVolume prefabSleeperVolume = selectedPrefabInstance.prefab.FindSleeperVolume(pos);
			if (prefabSleeperVolume != null && prefabSleeperVolume.isPriority)
			{
				index = 1;
			}
			if (data.block.spawnMode == BlockSleeper.eMode.Bandit)
			{
				index = 4;
			}
			if (data.block.spawnMode == BlockSleeper.eMode.Infested)
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
		else if (xRayOn && currentSelectionBox == null)
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
	public static bool InAnyVolume(Vector3i pos)
	{
		Vector3i zero = Vector3i.zero;
		Vector3i zero2 = Vector3i.zero;
		for (int i = 0; i < registeredSleeperVolumes.Count; i++)
		{
			SelectionBox selectionBox = registeredSleeperVolumes[i];
			zero.RoundToInt(selectionBox.bounds.min);
			zero2.RoundToInt(selectionBox.bounds.max);
			if (pos.x >= zero.x && pos.x < zero2.x && pos.y >= zero.y && pos.y < zero2.y && pos.z >= zero.z && pos.z < zero2.z)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetMats(Transform t, Material _mat)
	{
		int value = (xRayOn ? (-200000000) : (-200000));
		_mat.SetInt("_Offset", value);
		MeshRenderer[] componentsInChildren = t.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sharedMaterial = _mat;
		}
	}

	public static bool GetXRay()
	{
		return xRayOn;
	}

	public static void SetXRay(bool _on)
	{
		if (xRayOn != _on)
		{
			xRayOn = _on;
			int value = (xRayOn ? (-200000000) : (-200000));
			for (int i = 0; i < typeMats.Count; i++)
			{
				typeMats[i].SetInt("_Offset", value);
			}
			UpdateSleeperVisuals();
		}
	}
}
