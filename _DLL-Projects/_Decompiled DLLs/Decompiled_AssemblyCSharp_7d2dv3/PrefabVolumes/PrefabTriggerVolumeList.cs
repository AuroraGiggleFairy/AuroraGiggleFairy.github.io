using System.Collections.Generic;
using System.Text;

namespace PrefabVolumes;

public class PrefabTriggerVolumeList : PrefabVolumeListAbs<PrefabTriggerVolumeList, PrefabTriggerVolume>
{
	public override PrefabVolumeAbs.EVolumeType VolumeType => PrefabVolumeAbs.EVolumeType.Trigger;

	public override SelectionCategory SelectionCategory
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return SelectionBoxManager.Instance.CategoryTriggerVolume;
		}
	}

	public PrefabTriggerVolumeList(Prefab _owner)
		: base(_owner)
	{
	}

	public override void ReadFromProperties(DynamicProperties _properties)
	{
		List.Clear();
		Dictionary<string, string> values = _properties.Values;
		if (!values.ContainsKey("TriggerVolumeSize") || !values.ContainsKey("TriggerVolumeStart"))
		{
			return;
		}
		List<Vector3i> list = StringParsers.ParseList(values["TriggerVolumeSize"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
		List<Vector3i> list2 = StringParsers.ParseList(values["TriggerVolumeStart"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
		List<string> list3 = StringParsers.ParseList(values["TriggerVolumeTriggers"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => _s.Substring(_start, (_end == -1) ? (_s.Length - _start) : (_end + 1 - _start)));
		for (int num = 0; num < list2.Count; num++)
		{
			Vector3i startPos = list2[num];
			Vector3i size = ((num < list.Count) ? list[num] : Vector3i.one);
			PrefabTriggerVolume prefabTriggerVolume = new PrefabTriggerVolume();
			prefabTriggerVolume.Use(startPos, size);
			if (list3[num].Trim() != "")
			{
				prefabTriggerVolume.TriggersIndices.AddRange(StringParsers.ParseList(list3[num], ',', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseUInt8(_s, _start, _end)));
			}
			List.Add(prefabTriggerVolume);
			Owner.HandleAddingTriggerLayers(prefabTriggerVolume);
		}
	}

	public override void WriteToProperties(DynamicProperties _properties)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		StringBuilder stringBuilder3 = new StringBuilder();
		foreach (PrefabTriggerVolume item in List)
		{
			if (!item.Used)
			{
				continue;
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append('#');
				stringBuilder2.Append('#');
				stringBuilder3.Append('#');
			}
			for (int i = 0; i < item.TriggersIndices.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder3.Append(',');
				}
				stringBuilder3.Append(item.TriggersIndices[i].ToString());
			}
			if (item.TriggersIndices.Count == 0)
			{
				stringBuilder3.Append(" ");
			}
			stringBuilder.Append(item.size.ToString());
			stringBuilder2.Append(item.startPos.ToString());
		}
		if (stringBuilder.Length > 0)
		{
			_properties.Values["TriggerVolumeSize"] = stringBuilder.ToString();
			_properties.Values["TriggerVolumeStart"] = stringBuilder2.ToString();
			_properties.Values["TriggerVolumeTriggers"] = stringBuilder3.ToString();
		}
		else
		{
			_properties.Values.Remove("TriggerVolumeSize");
			_properties.Values.Remove("TriggerVolumeStart");
			_properties.Values.Remove("TriggerVolumeTriggers");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override int FindWorldVolume(World _world, Vector3i _min, Vector3i _max)
	{
		return _world.FindTriggerVolume(_min, _max);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override int CreateWorldVolume(int _prefabVolumeIndex, PrefabTriggerVolume _prefabVolume, Vector3i _offset, Vector3i _volumeMin, Vector3i _volumeMax, World _world, Vector3i _volumeWorldMin, Vector3i _volumeWorldMax)
	{
		TriggerVolume volume = TriggerVolume.Create(_prefabVolume, _volumeWorldMin, _volumeWorldMax);
		return _world.AddTriggerVolume(volume);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AddWorldVolume(Chunk _chunk, int _volumeIndex)
	{
		_chunk.AddTriggerVolumeId(_volumeIndex);
	}

	public override void CopyVolumesIntoWorld(World _world, Chunk _chunk, Vector3i _offset)
	{
		CopyVolumesIntoWorldCommon(_world, _chunk, _offset, SleeperVolume.chunkPadding);
	}
}
