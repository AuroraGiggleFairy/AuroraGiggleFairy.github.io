using MusicUtils.Enums;

namespace DynamicMusic;

public interface ISection : IPlayable, IFadeable, ICleanable
{
	bool IsInitialized { get; }

	SectionType Sect { get; set; }
}
