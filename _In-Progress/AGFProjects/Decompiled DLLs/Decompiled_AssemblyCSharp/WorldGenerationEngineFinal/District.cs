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
		Rural
	}

	public string name;

	public string prefabName;

	public Type type;

	public FastTags<TagGroup.Poi> tag;

	public FastTags<TagGroup.Poi> townships;

	public float weight = 0.5f;

	public Color preview_color;

	public int counter;

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
		counter = _other.counter;
		avoidedNeighborDistricts = _other.avoidedNeighborDistricts;
		Init();
	}

	public void Init()
	{
		type = Type.None;
		if (name.EndsWith("commercial"))
		{
			type = Type.Commercial;
		}
		else if (name.EndsWith("downtown"))
		{
			type = Type.Downtown;
		}
		else if (name.EndsWith("gateway"))
		{
			type = Type.Gateway;
		}
		else if (name.EndsWith("rural"))
		{
			type = Type.Rural;
		}
	}
}
