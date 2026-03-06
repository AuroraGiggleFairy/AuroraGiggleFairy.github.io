using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EditorStat : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastDirtyTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasLootStat;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lootContainers;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fetchLootContainers;

	[PublicizedFrom(EAccessModifier.Private)]
	public int restorePowerNodes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasBlockEntitiesStat;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalBlockEntities;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasSelection;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i selectionSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int DC_AVERAGE_FRAMES = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] drawcallsBuf = new int[20];

	[PublicizedFrom(EAccessModifier.Private)]
	public int drawcallsBufIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int drawcallsSum;

	[PublicizedFrom(EAccessModifier.Private)]
	public static WorldStats manualStats;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DateTime ManualStatsUpdateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<Vector3i> prefabSizeFormatter = new CachedStringFormatter<Vector3i>([PublicizedFrom(EAccessModifier.Internal)] (Vector3i _i) => _i.ToString());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<Vector3i> selectionSizeFormatter = new CachedStringFormatter<Vector3i>([PublicizedFrom(EAccessModifier.Internal)] (Vector3i _i) => _i.ToString());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt lootFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt fetchlootFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt restorepowerFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt blockentitiesFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> vertsFormatter = new CachedStringFormatter<int>([PublicizedFrom(EAccessModifier.Internal)] (int _i) => ValueDisplayFormatters.FormatNumberWithMetricPrefix(_i));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> trisFormatter = new CachedStringFormatter<int>([PublicizedFrom(EAccessModifier.Internal)] (int _i) => ValueDisplayFormatters.FormatNumberWithMetricPrefix(_i));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt batchesFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt statsVertsFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt statsTrisFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<DateTime> statsManualUpdateTimeFormatter = new CachedStringFormatter<DateTime>([PublicizedFrom(EAccessModifier.Internal)] (DateTime _dt) => _dt.ToString(Utils.StandardCulture));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt statsManualVertsFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt statsManualTrisFormatter = new CachedStringFormatterInt();

	public bool hasPrefabLoaded
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (PrefabEditModeManager.Instance.IsActive() && PrefabEditModeManager.Instance.VoxelPrefab != null)
			{
				return PrefabEditModeManager.Instance.VoxelPrefab.location.Type != PathAbstractions.EAbstractedLocationType.None;
			}
			return false;
		}
	}

	public Prefab selectedPrefab
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameManager.Instance.GetDynamicPrefabDecorator()?.ActivePrefab?.prefab;
		}
	}

	public bool hasPrefabSelected
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return selectedPrefab != null;
		}
	}

	public static WorldStats ManualStats
	{
		get
		{
			return manualStats;
		}
		set
		{
			ManualStatsUpdateTime = DateTime.Now;
			manualStats = value;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!IsDirty && !(Time.time - lastDirtyTime >= 1f))
		{
			return;
		}
		lootContainers = 0;
		fetchLootContainers = 0;
		restorePowerNodes = 0;
		totalBlockEntities = 0;
		hasSelection = false;
		selectionSize = default(Vector3i);
		if (hasPrefabLoaded)
		{
			SelectionBox selectionBox = SelectionBoxManager.Instance.Selection?.box;
			if (selectionBox != null)
			{
				selectionSize = selectionBox.GetScale();
				hasSelection = true;
			}
			if (hasLootStat)
			{
				PrefabEditModeManager.Instance.GetLootAndFetchLootContainerCount(out lootContainers, out fetchLootContainers, out restorePowerNodes);
			}
			if (hasBlockEntitiesStat)
			{
				foreach (ChunkGameObject usedChunkGameObject in GameManager.Instance.World.m_ChunkManager.GetUsedChunkGameObjects())
				{
					totalBlockEntities += countBlockEntities(usedChunkGameObject.transform);
				}
			}
		}
		RefreshBindings();
		IsDirty = false;
		lastDirtyTime = Time.time;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int countBlockEntities(Transform _t)
	{
		int num = 0;
		for (int i = 0; i < _t.childCount; i++)
		{
			Transform child = _t.GetChild(i);
			num = ((!(child.name == "_BlockEntities")) ? (num + countBlockEntities(child)) : (num + child.childCount));
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		Prefab prefab = (hasPrefabLoaded ? PrefabEditModeManager.Instance.VoxelPrefab : selectedPrefab);
		bool flag = prefab != null;
		bool flag2 = prefab?.RenderingCostStats != null;
		bool flag3 = ManualStats != null;
		switch (_bindingName)
		{
		case "has_loaded_prefab":
			_value = hasPrefabLoaded.ToString();
			return true;
		case "has_selected_prefab":
			_value = hasPrefabSelected.ToString();
			return true;
		case "loaded_prefab_name":
			_value = prefab?.PrefabName ?? "";
			return true;
		case "loaded_prefab_changed":
			_value = ((hasPrefabLoaded && PrefabEditModeManager.Instance.NeedsSaving) ? "*" : "");
			return true;
		case "prefab_size":
			_value = (flag ? prefabSizeFormatter.Format(prefab.size) : "");
			return true;
		case "prefab_volume":
			_value = (flag ? prefab.size.Volume().ToString() : "");
			return true;
		case "has_selection":
			_value = hasSelection.ToString();
			return true;
		case "selection_size":
			_value = (hasSelection ? selectionSizeFormatter.Format(selectionSize) : "-");
			return true;
		case "loot_containers":
			hasLootStat = true;
			_value = lootFormatter.Format(lootContainers);
			return true;
		case "fetchloot_containers":
			hasLootStat = true;
			_value = fetchlootFormatter.Format(fetchLootContainers);
			return true;
		case "restorepower_nodes":
			hasLootStat = true;
			_value = restorepowerFormatter.Format(restorePowerNodes);
			return true;
		case "block_entities":
			hasBlockEntitiesStat = true;
			_value = blockentitiesFormatter.Format(totalBlockEntities);
			return true;
		case "show_quest_clear_count":
			_value = prefab?.ShowQuestClearCount.ToString() ?? "";
			return true;
		case "sleeper_info":
			_value = prefab?.CalcSleeperInfo() ?? "";
			return true;
		case "difficulty_tier":
			_value = prefab?.DifficultyTier.ToString() ?? "";
			return true;
		case "verts":
			_value = "";
			return true;
		case "tris":
			_value = "";
			return true;
		case "drawcalls":
			_value = batchesFormatter.Format(drawcallsSum / 20);
			return true;
		case "statsVertices":
			_value = (flag2 ? statsVertsFormatter.Format(prefab.RenderingCostStats.TotalVertices) : "-");
			return true;
		case "statsTriangles":
			_value = (flag2 ? statsTrisFormatter.Format(prefab.RenderingCostStats.TotalTriangles) : "-");
			return true;
		case "statsLightsVolume":
		{
			if (!flag2)
			{
				_value = "-";
				return true;
			}
			float lightsVolume2 = prefab.RenderingCostStats.LightsVolume;
			int num2 = prefab.size.Volume();
			_value = $"{lightsVolume2:F0} ({lightsVolume2 / (float)num2:P1})";
			return true;
		}
		case "statsManualUpdateTime":
			_value = (flag3 ? statsManualUpdateTimeFormatter.Format(ManualStatsUpdateTime.ToLocalTime()) : "<not captured>");
			return true;
		case "statsManualVertices":
			_value = (flag3 ? statsManualVertsFormatter.Format(ManualStats.TotalVertices) : "-");
			return true;
		case "statsManualTriangles":
			_value = (flag3 ? statsManualTrisFormatter.Format(ManualStats.TotalTriangles) : "-");
			return true;
		case "statsManualLightsVolume":
		{
			if (!flag3)
			{
				_value = "-";
				return true;
			}
			float lightsVolume = ManualStats.LightsVolume;
			int num = prefab?.size.Volume() ?? 0;
			_value = $"{lightsVolume:F0} ({lightsVolume / (float)num:P1})";
			return true;
		}
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}
}
