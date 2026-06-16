using System.Collections.Generic;
using MusicUtils.Enums;

namespace DynamicMusic;

public interface IConfigurable
{
	void SetConfiguration(IList<PlacementType> _placements);
}
