using System;
using System.Collections.Generic;

public class PrefabData
{
	public const int ThemeRepeatDistanceDefault = 300;

	public const int DuplicateRepeatDistanceDefault = 1000;

	public Vector3i size;

	public readonly string Name;

	public FastTags<TagGroup.Poi> Tags;

	public FastTags<TagGroup.Poi> ThemeTags;

	public readonly int ThemeRepeatDistance = 300;

	public readonly int DuplicateRepeatDistance = 1000;

	public byte RotationsToNorth;

	public float DensityScore;

	public int DifficultyTier;

	public int yOffset;

	public PathAbstractions.AbstractedLocation location;

	public List<Prefab.Marker> POIMarkers = new List<Prefab.Marker>();

	public PrefabData(PathAbstractions.AbstractedLocation _location, DynamicProperties properties)
	{
		location = _location;
		Name = _location.Name.ToLower();
		DictionarySave<string, string> values = properties.Values;
		properties.ParseVec("PrefabSize", ref size);
		if (values.ContainsKey("POIMarkerSize") && values.ContainsKey("POIMarkerStart"))
		{
			POIMarkers.Clear();
			List<Vector3i> list = StringParsers.ParseList(values["POIMarkerSize"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			List<Vector3i> list2 = StringParsers.ParseList(values["POIMarkerStart"], '#', [PublicizedFrom(EAccessModifier.Internal)] (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end));
			List<Prefab.Marker.MarkerTypes> list3 = new List<Prefab.Marker.MarkerTypes>();
			if (values.ContainsKey("POIMarkerType"))
			{
				string[] array = values["POIMarkerType"].Split(',');
				for (int num = 0; num < array.Length; num++)
				{
					if (Enum.TryParse<Prefab.Marker.MarkerTypes>(array[num], ignoreCase: true, out var result))
					{
						list3.Add(result);
					}
				}
			}
			List<FastTags<TagGroup.Poi>> list4 = new List<FastTags<TagGroup.Poi>>();
			if (values.ContainsKey("POIMarkerTags"))
			{
				string[] array = values["POIMarkerTags"].Split('#');
				for (int num2 = 0; num2 < array.Length; num2++)
				{
					if (array[num2].Length > 0)
					{
						list4.Add(FastTags<TagGroup.Poi>.Parse(array[num2]));
					}
					else
					{
						list4.Add(FastTags<TagGroup.Poi>.none);
					}
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
				string[] array2 = array;
				for (int num3 = 0; num3 < array2.Length; num3++)
				{
					if (StringParsers.TryParseSInt32(array2[num3], out var _result))
					{
						list7.Add(_result);
					}
					else
					{
						list7.Add(0);
					}
				}
			}
			List<float> list8 = new List<float>();
			if (values.ContainsKey("POIMarkerPartSpawnChance"))
			{
				string[] array = values["POIMarkerPartSpawnChance"].Split(',');
				string[] array2 = array;
				for (int num3 = 0; num3 < array2.Length; num3++)
				{
					if (StringParsers.TryParseFloat(array2[num3], out var _result2))
					{
						list8.Add(_result2);
					}
					else
					{
						list8.Add(0f);
					}
				}
			}
			for (int num4 = 0; num4 < list2.Count; num4++)
			{
				Prefab.Marker marker = new Prefab.Marker
				{
					Start = list2[num4]
				};
				if (num4 < list.Count)
				{
					marker.Size = list[num4];
				}
				if (num4 < list3.Count)
				{
					marker.MarkerType = list3[num4];
				}
				if (num4 < list5.Count)
				{
					marker.GroupName = list5[num4];
				}
				if (num4 < list4.Count)
				{
					marker.Tags = list4[num4];
				}
				if (num4 < list6.Count)
				{
					marker.PartToSpawn = list6[num4];
				}
				if (num4 < list7.Count)
				{
					marker.Rotations = (byte)list7[num4];
				}
				if (num4 < list8.Count)
				{
					marker.PartChanceToSpawn = list8[num4];
				}
				POIMarkers.Add(marker);
			}
		}
		RotationsToNorth = (byte)properties.GetInt("RotationToFaceNorth");
		if (properties.Values.ContainsKey("Tags"))
		{
			Tags = FastTags<TagGroup.Poi>.Parse(properties.Values["Tags"].Replace(" ", ""));
		}
		if (properties.Values.ContainsKey("ThemeTags"))
		{
			ThemeTags = FastTags<TagGroup.Poi>.Parse(properties.Values["ThemeTags"].Replace(" ", ""));
		}
		properties.ParseInt("ThemeRepeatDistance", ref ThemeRepeatDistance);
		properties.ParseInt("DuplicateRepeatDistance", ref DuplicateRepeatDistance);
		if (properties.Classes.ContainsKey("Stats"))
		{
			WorldStats worldStats = WorldStats.FromProperties(properties.Classes["Stats"]);
			if (worldStats != null)
			{
				DensityScore = (worldStats.TotalVertices + 50000) / 100000;
			}
		}
		yOffset = properties.GetInt("YOffset");
		properties.ParseInt("DifficultyTier", ref DifficultyTier);
	}

	public List<Prefab.Marker> RotatePOIMarkers(bool _bLeft, int _rotCount)
	{
		Vector3i vector3i = size;
		List<Prefab.Marker> list = new List<Prefab.Marker>(POIMarkers.Count);
		for (int i = 0; i < POIMarkers.Count; i++)
		{
			list.Add(new Prefab.Marker(POIMarkers[i]));
		}
		for (int j = 0; j < _rotCount; j++)
		{
			for (int k = 0; k < list.Count; k++)
			{
				Prefab.Marker marker = list[k];
				Vector3i vector3i2 = marker.Size;
				Vector3i start = marker.Start;
				Vector3i vector3i3 = start + vector3i2;
				if (_bLeft)
				{
					start = new Vector3i(vector3i.z - start.z, start.y, start.x);
					vector3i3 = new Vector3i(vector3i.z - vector3i3.z, vector3i3.y, vector3i3.x);
				}
				else
				{
					start = new Vector3i(start.z, start.y, vector3i.x - start.x);
					vector3i3 = new Vector3i(vector3i3.z, vector3i3.y, vector3i.x - vector3i3.x);
				}
				if (start.x > vector3i3.x)
				{
					MathUtils.Swap(ref start.x, ref vector3i3.x);
				}
				if (start.z > vector3i3.z)
				{
					MathUtils.Swap(ref start.z, ref vector3i3.z);
				}
				marker.Start = start;
				MathUtils.Swap(ref vector3i2.x, ref vector3i2.z);
				marker.Size = vector3i2;
			}
			MathUtils.Swap(ref vector3i.x, ref vector3i.z);
		}
		return list;
	}

	public static PrefabData LoadPrefabData(PathAbstractions.AbstractedLocation _location)
	{
		if (!SdFile.Exists(_location.FullPathNoExtension + ".xml"))
		{
			return null;
		}
		DynamicProperties dynamicProperties = new DynamicProperties();
		if (!dynamicProperties.Load(_location.Folder, _location.Name, _addClassesToMain: false))
		{
			return null;
		}
		return new PrefabData(_location, dynamicProperties);
	}
}
