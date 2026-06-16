using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PrefabVolumes;

public class PrefabSleeperVolumeList : PrefabVolumeListAbs<PrefabSleeperVolumeList, PrefabSleeperVolume>
{
	public override PrefabVolumeAbs.EVolumeType VolumeType => PrefabVolumeAbs.EVolumeType.Sleeper;

	public override SelectionCategory SelectionCategory
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return SelectionBoxManager.Instance.CategorySleeperVolume;
		}
	}

	public PrefabSleeperVolumeList(Prefab _owner)
		: base(_owner)
	{
	}

	public override void ReadFromProperties(DynamicProperties _properties)
	{
		List.Clear();
		Dictionary<string, string> values = _properties.Values;
		if (!values.ContainsKey("SleeperVolumeSize") || !values.ContainsKey("SleeperVolumeStart"))
		{
			return;
		}
		List<Vector3i> list = StringParsers.ParseList(values["SleeperVolumeSize"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
		List<Vector3i> list2 = StringParsers.ParseList(values["SleeperVolumeStart"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
		List<string> list3 = null;
		if (values.TryGetValue("SleeperVolumeGroupId", out var value))
		{
			list3 = new List<string>(value.Split(','));
		}
		List<string> list4 = null;
		if (values.TryGetValue("SleeperVolumeGroup", out value))
		{
			list4 = new List<string>(value.Split(','));
		}
		List<bool> list5 = (values.ContainsKey("SleeperIsLootVolume") ? StringParsers.ParseList(values["SleeperIsLootVolume"], ',', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseBool(_s, _start, _end)) : new List<bool>());
		List<bool> list6 = (values.ContainsKey("SleeperIsQuestExclude") ? StringParsers.ParseList(values["SleeperIsQuestExclude"], ',', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseBool(_s, _start, _end)) : new List<bool>());
		List<int> list7 = null;
		if (values.TryGetValue("SleeperVolumeFlags", out value))
		{
			list7 = StringParsers.ParseList(value, ',', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseSInt32(_s, _start, _end, NumberStyles.HexNumber));
		}
		List<string> list8 = null;
		if (values.TryGetValue("SleeperVolumeTriggeredBy", out value))
		{
			list8 = StringParsers.ParseList(value, '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => _s.Substring(_start, (_end == -1) ? (_s.Length - _start) : (_end + 1 - _start)));
		}
		for (int num = 0; num < list2.Count; num++)
		{
			Vector3i startPos = list2[num];
			Vector3i size = ((num < list.Count) ? list[num] : Vector3i.one);
			short groupId = 0;
			string text = "???";
			short spawnCountMin = 5;
			short spawnCountMax = 5;
			if (list3 != null)
			{
				groupId = StringParsers.ParseSInt16(list3[num]);
			}
			if (list4 != null)
			{
				if (list4.Count == list2.Count)
				{
					text = list4[num];
				}
				else if (list4.Count == list2.Count * 3)
				{
					int num2 = num * 3;
					text = list4[num2];
					spawnCountMin = StringParsers.ParseSInt16(list4[num2 + 1]);
					spawnCountMax = StringParsers.ParseSInt16(list4[num2 + 2]);
				}
				text = GameStageGroup.CleanName(text);
			}
			bool isPriority = num < list5.Count && list5[num];
			bool isQuestExclude = num < list6.Count && list6[num];
			int flags = 0;
			if (list7 != null && num < list7.Count)
			{
				flags = list7[num];
			}
			PrefabSleeperVolume prefabSleeperVolume = new PrefabSleeperVolume();
			prefabSleeperVolume.Use(startPos, size);
			prefabSleeperVolume.groupId = groupId;
			prefabSleeperVolume.groupName = text;
			prefabSleeperVolume.isPriority = isPriority;
			prefabSleeperVolume.isQuestExclude = isQuestExclude;
			prefabSleeperVolume.spawnCountMin = spawnCountMin;
			prefabSleeperVolume.spawnCountMax = spawnCountMax;
			prefabSleeperVolume.flags = flags;
			string text2 = _properties.GetString("SVS" + num);
			if (text2.Length > 0)
			{
				prefabSleeperVolume.minScript = text2;
			}
			if (list8 != null && list8[num].Trim() != "")
			{
				prefabSleeperVolume.triggeredByIndices.AddRange(StringParsers.ParseList(list8[num], ',', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseUInt8(_s, _start, _end)));
			}
			List.Add(prefabSleeperVolume);
		}
	}

	public override void WriteToProperties(DynamicProperties _properties)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, string> value in _properties.Values)
		{
			if (value.Key.StartsWith("SVS"))
			{
				list.Add(value.Key);
			}
		}
		foreach (string item in list)
		{
			_properties.Values.Remove(item);
		}
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		StringBuilder stringBuilder3 = new StringBuilder();
		StringBuilder stringBuilder4 = new StringBuilder();
		StringBuilder stringBuilder5 = new StringBuilder();
		StringBuilder stringBuilder6 = new StringBuilder();
		StringBuilder stringBuilder7 = new StringBuilder();
		StringBuilder stringBuilder8 = new StringBuilder();
		foreach (PrefabSleeperVolume item2 in List)
		{
			if (!item2.Used)
			{
				continue;
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append('#');
				stringBuilder2.Append('#');
				stringBuilder3.Append(',');
				stringBuilder4.Append(',');
				stringBuilder5.Append(',');
				stringBuilder6.Append(',');
				stringBuilder7.Append(',');
				stringBuilder8.Append('#');
			}
			stringBuilder.Append(item2.size.ToString());
			stringBuilder2.Append(item2.startPos.ToString());
			stringBuilder3.Append(item2.groupId);
			stringBuilder4.Append(item2.groupName);
			stringBuilder4.Append(',');
			stringBuilder4.Append(item2.spawnCountMin.ToString());
			stringBuilder4.Append(',');
			stringBuilder4.Append(item2.spawnCountMax.ToString());
			stringBuilder5.Append(item2.isPriority.ToString());
			stringBuilder6.Append(item2.isQuestExclude.ToString());
			stringBuilder7.Append(item2.flags.ToString("x"));
			for (int i = 0; i < item2.triggeredByIndices.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder8.Append(',');
				}
				stringBuilder8.Append(item2.triggeredByIndices[i].ToString());
			}
			if (item2.triggeredByIndices.Count == 0)
			{
				stringBuilder8.Append(" ");
			}
		}
		if (stringBuilder.Length > 0)
		{
			_properties.Values["SleeperVolumeSize"] = stringBuilder.ToString();
			_properties.Values["SleeperVolumeStart"] = stringBuilder2.ToString();
			_properties.Values["SleeperVolumeGroupId"] = stringBuilder3.ToString();
			_properties.Values["SleeperVolumeGroup"] = stringBuilder4.ToString();
			_properties.Values["SleeperIsLootVolume"] = stringBuilder5.ToString();
			_properties.Values["SleeperIsQuestExclude"] = stringBuilder6.ToString();
			_properties.Values["SleeperVolumeFlags"] = stringBuilder7.ToString();
			_properties.Values["SleeperVolumeTriggeredBy"] = stringBuilder8.ToString();
			int num = 0;
			{
				foreach (PrefabSleeperVolume item3 in List)
				{
					if (item3.Used)
					{
						if (item3.minScript != null)
						{
							_properties.Values["SVS" + num] = item3.minScript;
						}
						num++;
					}
				}
				return;
			}
		}
		_properties.Values.Remove("SleeperVolumeSize");
		_properties.Values.Remove("SleeperVolumeStart");
		_properties.Values.Remove("SleeperVolumeGroupId");
		_properties.Values.Remove("SleeperVolumeGroup");
		_properties.Values.Remove("SleeperIsLootVolume");
		_properties.Values.Remove("SleeperIsQuestExclude");
		_properties.Values.Remove("SleeperVolumeFlags");
		_properties.Values.Remove("SleeperVolumeTriggeredBy");
	}

	public override (int volumeIndex, PrefabVolumeAbs volume, SelectionBox box) AddNewVolume(string _prefabInstanceName, Vector3i _bbPos, Vector3i _startPos, Vector3i _size)
	{
		(PrefabSleeperVolume volume, int index, string name) tuple = PrepareNewEntry(_prefabInstanceName, _startPos, _size);
		PrefabSleeperVolume item = tuple.volume;
		int item2 = tuple.index;
		string item3 = tuple.name;
		SelectionBox selectionBox = AddSelectionBox(item, item3, _bbPos + _startPos);
		SelectionBoxManager.Instance.SetActive(selectionBox, _bActive: true);
		return (volumeIndex: item2, volume: item, box: selectionBox);
	}

	public override void SetVolume(PrefabInstance _prefabInstance, int _index, PrefabSleeperVolume _volumeSettings)
	{
		base.SetVolume(_prefabInstance, _index, _volumeSettings);
		XUiC_WoPropsSleeperVolume.Instance?.SleeperVolumeChanged(_prefabInstance.id, _index);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override int FindWorldVolume(World _world, Vector3i _min, Vector3i _max)
	{
		return _world.FindSleeperVolume(_min, _max);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override int CreateWorldVolume(int _prefabVolumeIndex, PrefabSleeperVolume _prefabVolume, Vector3i _offset, Vector3i _volumeMin, Vector3i _volumeMax, World _world, Vector3i _volumeWorldMin, Vector3i _volumeWorldMax)
	{
		SleeperVolume volume = SleeperVolume.Create(_prefabVolume, _volumeWorldMin, _volumeWorldMax);
		int result = _world.AddSleeperVolume(volume);
		Owner.CopySleeperBlocksContainedInVolume(_prefabVolumeIndex, _offset, volume, _volumeMin, _volumeMax);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AddWorldVolume(Chunk _chunk, int _volumeIndex)
	{
		_chunk.AddSleeperVolumeId(_volumeIndex);
	}

	public override void CopyVolumesIntoWorld(World _world, Chunk _chunk, Vector3i _offset)
	{
		CopyVolumesIntoWorldCommon(_world, _chunk, _offset, SleeperVolume.chunkPadding);
	}
}
