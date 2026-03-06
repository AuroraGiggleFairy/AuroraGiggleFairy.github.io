using MusicUtils.Enums;

namespace DynamicMusic;

public interface IThreatLevel
{
	ThreatLevelType Category { get; }

	float Numeric { get; set; }
}
