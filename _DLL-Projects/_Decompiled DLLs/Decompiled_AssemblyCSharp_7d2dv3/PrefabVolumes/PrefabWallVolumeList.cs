using System.Collections.Generic;
using System.Text;

namespace PrefabVolumes;

public class PrefabWallVolumeList : PrefabVolumeListAbs<PrefabWallVolumeList, PrefabWallVolume>
{
	public override PrefabVolumeAbs.EVolumeType VolumeType => PrefabVolumeAbs.EVolumeType.Wall;

	public override SelectionCategory SelectionCategory
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return SelectionBoxManager.Instance.CategoryWallVolume;
		}
	}

	public PrefabWallVolumeList(Prefab _owner)
		: base(_owner)
	{
	}

	public override void ReadFromProperties(DynamicProperties _properties)
	{
		List.Clear();
		Dictionary<string, string> values = _properties.Values;
		if (values.ContainsKey("WallVolumeSize") && values.ContainsKey("WallVolumeStart"))
		{
			List<Vector3i> list = StringParsers.ParseList(values["WallVolumeSize"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			List<Vector3i> list2 = StringParsers.ParseList(values["WallVolumeStart"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			for (int num = 0; num < list2.Count; num++)
			{
				Vector3i startPos = list2[num];
				Vector3i size = ((num < list.Count) ? list[num] : Vector3i.one);
				PrefabWallVolume prefabWallVolume = new PrefabWallVolume();
				prefabWallVolume.Use(startPos, size);
				List.Add(prefabWallVolume);
			}
		}
	}

	public override void WriteToProperties(DynamicProperties _properties)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		foreach (PrefabWallVolume item in List)
		{
			if (item.Used)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append('#');
					stringBuilder2.Append('#');
				}
				stringBuilder.Append(item.size.ToString());
				stringBuilder2.Append(item.startPos.ToString());
			}
		}
		if (stringBuilder.Length > 0)
		{
			_properties.Values["WallVolumeSize"] = stringBuilder.ToString();
			_properties.Values["WallVolumeStart"] = stringBuilder2.ToString();
		}
		else
		{
			_properties.Values.Remove("WallVolumeSize");
			_properties.Values.Remove("WallVolumeStart");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override int FindWorldVolume(World _world, Vector3i _min, Vector3i _max)
	{
		return _world.FindWallVolume(_min, _max);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override int CreateWorldVolume(int _prefabVolumeIndex, PrefabWallVolume _prefabVolume, Vector3i _offset, Vector3i _volumeMin, Vector3i _volumeMax, World _world, Vector3i _volumeWorldMin, Vector3i _volumeWorldMax)
	{
		WallVolume volume = WallVolume.Create(_prefabVolume, _volumeWorldMin, _volumeWorldMax);
		return _world.AddWallVolume(volume);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AddWorldVolume(Chunk _chunk, int _volumeIndex)
	{
		_chunk.AddWallVolumeId(_volumeIndex);
	}

	public override void CopyVolumesIntoWorld(World _world, Chunk _chunk, Vector3i _offset)
	{
		CopyVolumesIntoWorldCommon(_world, _chunk, _offset, Vector3i.zero);
	}
}
