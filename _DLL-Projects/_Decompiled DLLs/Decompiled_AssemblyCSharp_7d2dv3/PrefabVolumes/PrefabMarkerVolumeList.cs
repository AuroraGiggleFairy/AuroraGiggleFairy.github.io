using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PrefabVolumes;

public class PrefabMarkerVolumeList : PrefabVolumeListAbs<PrefabMarkerVolumeList, Marker>
{
	public override PrefabVolumeAbs.EVolumeType VolumeType => PrefabVolumeAbs.EVolumeType.Marker;

	public override SelectionCategory SelectionCategory
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return SelectionBoxManager.Instance.CategoryPOIMarker;
		}
	}

	public PrefabMarkerVolumeList(Prefab _owner)
		: base(_owner)
	{
	}

	public override void ReadFromProperties(DynamicProperties _properties)
	{
		List.Clear();
		Dictionary<string, string> values = _properties.Values;
		if (!values.ContainsKey("POIMarkerSize") || !values.ContainsKey("POIMarkerStart"))
		{
			return;
		}
		List<Vector3i> list = StringParsers.ParseList(values["POIMarkerSize"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
		List<Vector3i> list2 = StringParsers.ParseList(values["POIMarkerStart"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
		List<Marker.MarkerTypes> list3 = new List<Marker.MarkerTypes>();
		if (values.ContainsKey("POIMarkerType"))
		{
			string[] array = values["POIMarkerType"].Split(',');
			for (int num = 0; num < array.Length; num++)
			{
				if (EnumUtils.TryParse<Marker.MarkerTypes>(array[num], out var _result, _ignoreCase: true))
				{
					list3.Add(_result);
				}
			}
		}
		List<FastTags<TagGroup.Poi>> list4 = new List<FastTags<TagGroup.Poi>>();
		if (values.ContainsKey("POIMarkerTags"))
		{
			string[] array = values["POIMarkerTags"].Split('#');
			foreach (string text in array)
			{
				list4.Add((text.Length > 0) ? FastTags<TagGroup.Poi>.Parse(text) : FastTags<TagGroup.Poi>.none);
			}
		}
		List<string> list5 = new List<string>();
		if (values.ContainsKey("POIMarkerGroup"))
		{
			list5.AddRange(values["POIMarkerGroup"].Split(','));
		}
		List<string> list6 = new List<string>();
		if (values.ContainsKey("POIMarkerPartToSpawn"))
		{
			list6.AddRange(values["POIMarkerPartToSpawn"].Split(','));
		}
		List<int> list7 = new List<int>();
		if (values.ContainsKey("POIMarkerPartRotations"))
		{
			string[] array = values["POIMarkerPartRotations"].Split(',');
			for (int num = 0; num < array.Length; num++)
			{
				StringParsers.TryParseSInt32(array[num], out var _result2);
				list7.Add(_result2);
			}
		}
		List<float> list8 = new List<float>();
		if (values.ContainsKey("POIMarkerPartSpawnChance"))
		{
			string[] array = values["POIMarkerPartSpawnChance"].Split(',');
			for (int num = 0; num < array.Length; num++)
			{
				if (StringParsers.TryParseFloat(array[num], out var _result3))
				{
					list8.Add(_result3);
				}
				else
				{
					list8.Add(0f);
				}
			}
		}
		for (int num2 = 0; num2 < list2.Count; num2++)
		{
			Marker marker = new Marker();
			marker.Use(list2[num2], (num2 < list.Count) ? list[num2] : default(Vector3i));
			if (num2 < list3.Count)
			{
				marker.MarkerType = list3[num2];
			}
			if (num2 < list5.Count)
			{
				marker.GroupName = list5[num2];
			}
			if (num2 < list4.Count)
			{
				marker.Tags = list4[num2];
			}
			if (num2 < list6.Count)
			{
				marker.PartToSpawn = list6[num2];
			}
			if (num2 < list7.Count)
			{
				marker.Rotations = (byte)list7[num2];
			}
			if (num2 < list8.Count)
			{
				marker.PartChanceToSpawn = list8[num2];
			}
			List.Add(marker);
		}
	}

	public override void WriteToProperties(DynamicProperties _properties)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		StringBuilder stringBuilder3 = new StringBuilder();
		StringBuilder stringBuilder4 = new StringBuilder();
		StringBuilder stringBuilder5 = new StringBuilder();
		StringBuilder stringBuilder6 = new StringBuilder();
		StringBuilder stringBuilder7 = new StringBuilder();
		StringBuilder stringBuilder8 = new StringBuilder();
		foreach (Marker item in List)
		{
			if (item.Used)
			{
				if (stringBuilder2.Length > 0)
				{
					stringBuilder.Append('#');
					stringBuilder2.Append('#');
					stringBuilder3.Append(',');
					stringBuilder4.Append('#');
					stringBuilder5.Append(',');
					stringBuilder6.Append(',');
					stringBuilder7.Append(',');
					stringBuilder8.Append(',');
				}
				stringBuilder.Append(item.size.ToString());
				stringBuilder2.Append(item.startPos.ToString());
				stringBuilder3.Append(item.GroupName);
				stringBuilder4.Append(item.Tags.ToString());
				stringBuilder5.Append(item.MarkerType.ToString());
				stringBuilder6.Append(item.PartToSpawn);
				stringBuilder7.Append(item.Rotations.ToString());
				stringBuilder8.Append(item.PartChanceToSpawn.ToString(CultureInfo.InvariantCulture));
			}
		}
		if (stringBuilder.Length > 0)
		{
			_properties.Values["POIMarkerSize"] = stringBuilder.ToString();
			_properties.Values["POIMarkerStart"] = stringBuilder2.ToString();
			_properties.Values["POIMarkerGroup"] = stringBuilder3.ToString();
			_properties.Values["POIMarkerTags"] = stringBuilder4.ToString();
			_properties.Values["POIMarkerType"] = stringBuilder5.ToString();
			_properties.Values["POIMarkerPartToSpawn"] = stringBuilder6.ToString();
			_properties.Values["POIMarkerPartRotations"] = stringBuilder7.ToString();
			_properties.Values["POIMarkerPartSpawnChance"] = stringBuilder8.ToString();
		}
		else
		{
			_properties.Values.Remove("POIMarkerSize");
			_properties.Values.Remove("POIMarkerStart");
			_properties.Values.Remove("POIMarkerGroup");
			_properties.Values.Remove("POIMarkerTags");
			_properties.Values.Remove("POIMarkerType");
			_properties.Values.Remove("POIMarkerPartToSpawn");
			_properties.Values.Remove("POIMarkerPartRotations");
			_properties.Values.Remove("POIMarkerPartSpawnChance");
		}
	}

	public override (int volumeIndex, PrefabVolumeAbs volume, SelectionBox box) AddNewVolume(string _prefabInstanceName, Vector3i _bbPos, Vector3i _startPos, Vector3i _size)
	{
		(Marker volume, int index, string name) tuple = PrepareNewEntry(_prefabInstanceName, _startPos, _size);
		Marker item = tuple.volume;
		int item2 = tuple.index;
		string item3 = tuple.name;
		SelectionBox selectionBox = AddSelectionBox(item, item3, _bbPos + _startPos);
		SelectionBoxManager.Instance.SetActive(selectionBox, _bActive: true);
		return (volumeIndex: item2, volume: item, box: selectionBox);
	}

	public override void SetVolume(PrefabInstance _prefabInstance, int _index, Marker _volumeSettings)
	{
		base.SetVolume(_prefabInstance, _index, _volumeSettings);
		if (_volumeSettings.Used)
		{
			string name = _prefabInstance.name + "_" + _index;
			SelectionCategory.TryGetBox(name, out var _box);
			SelectionBox selectionBox = _box;
			selectionBox.FacingDirection = _volumeSettings.Rotations switch
			{
				1 => (_volumeSettings.MarkerType == Marker.MarkerTypes.PartSpawn) ? 90 : 270, 
				2 => 180, 
				3 => (_volumeSettings.MarkerType == Marker.MarkerTypes.PartSpawn) ? 270 : 90, 
				_ => 0, 
			};
		}
	}

	public override SelectionBox AddSelectionBox(Marker _volume, string _name, Vector3i _pos)
	{
		SelectionBox selectionBox = base.AddSelectionBox(_volume, _name, _pos);
		selectionBox.bDrawDirection = true;
		selectionBox.bAlwaysDrawDirection = true;
		SelectionBox selectionBox2 = selectionBox;
		selectionBox2.FacingDirection = _volume.Rotations switch
		{
			1 => (_volume.MarkerType == Marker.MarkerTypes.PartSpawn) ? 90 : 270, 
			2 => 180, 
			3 => (_volume.MarkerType == Marker.MarkerTypes.PartSpawn) ? 270 : 90, 
			_ => 0, 
		};
		selectionBox.Category.SetVisible(_bVisible: true);
		return selectionBox;
	}
}
