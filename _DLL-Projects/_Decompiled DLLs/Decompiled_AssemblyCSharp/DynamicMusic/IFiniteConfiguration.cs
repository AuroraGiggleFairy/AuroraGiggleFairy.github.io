using System.Collections.Generic;
using MusicUtils.Enums;

namespace DynamicMusic;

public interface IFiniteConfiguration : IConfiguration<IList<PlacementType>>, IConfiguration
{
}
