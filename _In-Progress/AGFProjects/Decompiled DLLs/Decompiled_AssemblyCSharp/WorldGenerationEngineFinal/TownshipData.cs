using System.Collections.Generic;

namespace WorldGenerationEngineFinal;

public class TownshipData
{
	public enum eCategory
	{
		Normal,
		Roadside,
		Rural,
		Wilderness
	}

	public string Name;

	public int Id;

	public List<string> SpawnableTerrain = new List<string>();

	public bool SpawnCustomSizes;

	public bool SpawnTrader = true;

	public bool SpawnGateway = true;

	public string OutskirtDistrict;

	public float OutskirtDistrictPercent;

	public FastTags<TagGroup.Poi> Biomes;

	public readonly eCategory Category;

	public TownshipData(string _name, int _id)
	{
		Name = _name;
		Id = _id;
		if (_name.EndsWith("roadside"))
		{
			Category = eCategory.Roadside;
		}
		else if (_name.EndsWith("rural"))
		{
			Category = eCategory.Rural;
		}
		else if (_name.EndsWith("wilderness"))
		{
			Category = eCategory.Wilderness;
		}
		WorldBuilderStatic.idToTownshipData[Id] = this;
	}
}
