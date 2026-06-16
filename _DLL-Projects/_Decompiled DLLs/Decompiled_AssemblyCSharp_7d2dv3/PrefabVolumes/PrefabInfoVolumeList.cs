using System.Collections.Generic;
using System.Text;

namespace PrefabVolumes;

public class PrefabInfoVolumeList : PrefabVolumeListAbs<PrefabInfoVolumeList, PrefabInfoVolume>
{
	public override PrefabVolumeAbs.EVolumeType VolumeType => PrefabVolumeAbs.EVolumeType.Info;

	public override SelectionCategory SelectionCategory
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return SelectionBoxManager.Instance.CategoryInfoVolume;
		}
	}

	public PrefabInfoVolumeList(Prefab _owner)
		: base(_owner)
	{
	}

	public override void ReadFromProperties(DynamicProperties _properties)
	{
		List.Clear();
		Dictionary<string, string> values = _properties.Values;
		if (values.ContainsKey("InfoVolumeSize") && values.ContainsKey("InfoVolumeStart"))
		{
			List<Vector3i> list = StringParsers.ParseList(values["InfoVolumeSize"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			List<Vector3i> list2 = StringParsers.ParseList(values["InfoVolumeStart"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			for (int num = 0; num < list2.Count; num++)
			{
				Vector3i startPos = list2[num];
				Vector3i size = ((num < list.Count) ? list[num] : Vector3i.one);
				PrefabInfoVolume prefabInfoVolume = new PrefabInfoVolume();
				prefabInfoVolume.Use(startPos, size);
				List.Add(prefabInfoVolume);
			}
		}
	}

	public override void WriteToProperties(DynamicProperties _properties)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		foreach (PrefabInfoVolume item in List)
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
			_properties.Values["InfoVolumeSize"] = stringBuilder.ToString();
			_properties.Values["InfoVolumeStart"] = stringBuilder2.ToString();
		}
		else
		{
			_properties.Values.Remove("InfoVolumeSize");
			_properties.Values.Remove("InfoVolumeStart");
		}
	}
}
