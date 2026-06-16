using System.Collections.Generic;
using PrefabVolumes;

public class PrefabData
{
	public const int ThemeRepeatDistanceDefault = 300;

	public const int DuplicateRepeatDistanceDefault = 1000;

	public Vector3i size;

	public readonly string Name;

	public FastTags<TagGroup.Poi> Tags;

	public FastTags<TagGroup.Poi> ThemeTags;

	public int ThemeRepeatDistance = 300;

	public int DuplicateRepeatDistance = 1000;

	public byte rotationToFaceNorth;

	public float DensityScore;

	public int DifficultyTier;

	public int yOffset;

	public PathAbstractions.AbstractedLocation location;

	public readonly PrefabMarkerVolumeList POIMarkers;

	public PrefabData(PathAbstractions.AbstractedLocation _location)
	{
		POIMarkers = new PrefabMarkerVolumeList(null);
		location = _location;
		Name = _location.Name.ToLower();
	}

	public bool Init(DynamicProperties properties)
	{
		size = new Vector3i(-1, -1, -1);
		properties.ParseVec("PrefabSize", ref size);
		if (size.x < 0 || size.y < 0 || size.z < 0)
		{
			Log.Error("PrefabData {0} PrefabSize error", Name);
			return false;
		}
		POIMarkers.ReadFromProperties(properties);
		rotationToFaceNorth = (byte)properties.GetInt("RotationToFaceNorth");
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
		return true;
	}

	public List<Marker> RotatePOIMarkers(bool _bLeft, int _rotCount)
	{
		Vector3i prefabSize = size;
		List<Marker> list = new List<Marker>(POIMarkers.Count);
		for (int i = 0; i < POIMarkers.Count; i++)
		{
			list.Add(POIMarkers[i].CloneGeneric());
		}
		for (int j = 0; j < _rotCount; j++)
		{
			for (int k = 0; k < list.Count; k++)
			{
				list[k].RotateY(_bLeft, prefabSize);
			}
			MathUtils.Swap(ref prefabSize.x, ref prefabSize.z);
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
		if (!dynamicProperties.Load(_location.Folder, _location.Name))
		{
			return null;
		}
		PrefabData prefabData = new PrefabData(_location);
		if (!prefabData.Init(dynamicProperties))
		{
			return null;
		}
		return prefabData;
	}
}
