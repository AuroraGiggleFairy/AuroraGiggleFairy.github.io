using System.Collections.Generic;
using MusicUtils.Enums;
using UnityEngine.Scripting;

namespace DynamicMusic;

[Preserve]
public class FixedConfigurationLayerData : ICountable
{
	public List<List<PlacementType>> LayerInstances;

	public int Count => LayerInstances.Count;

	public FixedConfigurationLayerData()
	{
		LayerInstances = new List<List<PlacementType>>();
	}

	public void Add(List<PlacementType> _list)
	{
		LayerInstances.Add(_list);
	}
}
