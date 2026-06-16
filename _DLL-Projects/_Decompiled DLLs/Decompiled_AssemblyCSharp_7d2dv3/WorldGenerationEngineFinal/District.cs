using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public class District
{
	public enum Type
	{
		None,
		Commercial,
		Downtown,
		Gateway,
		Industrial,
		Residential,
		Rural
	}

	public string name;

	public string prefabName;

	public Type type;

	public FastTags<TagGroup.Poi> tag;

	public FastTags<TagGroup.Poi> townships;

	public float weight = 0.5f;

	public float spawnOrder;

	public Color preview_color;

	public bool spawnCustomSizePrefabs;

	public List<string> avoidedNeighborDistricts = new List<string>();

	public District()
	{
	}

	public District(District _other)
	{
		name = _other.name;
		prefabName = _other.prefabName;
		tag = _other.tag;
		townships = _other.townships;
		weight = _other.weight;
		preview_color = _other.preview_color;
		avoidedNeighborDistricts = _other.avoidedNeighborDistricts;
		Init();
	}

	public void Init()
	{
		type = Type.None;
		if (name.EndsWith("commercial"))
		{
			type = Type.Commercial;
			spawnOrder = 1f;
		}
		else if (name.EndsWith("downtown"))
		{
			type = Type.Downtown;
			spawnOrder = 99f;
		}
		else if (name.EndsWith("gateway"))
		{
			type = Type.Gateway;
		}
		else if (name.EndsWith("industrial"))
		{
			type = Type.Industrial;
		}
		else if (name.EndsWith("residential"))
		{
			type = Type.Residential;
			spawnOrder = -1f;
		}
		else if (name.EndsWith("rural"))
		{
			type = Type.Rural;
		}
	}
}
