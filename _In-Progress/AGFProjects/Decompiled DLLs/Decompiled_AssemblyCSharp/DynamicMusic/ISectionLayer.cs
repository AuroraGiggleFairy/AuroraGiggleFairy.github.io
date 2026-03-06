using MusicUtils.Enums;

namespace DynamicMusic;

public interface ISectionLayer : IPlayable, ILayerable, IConfigurable, ICleanable
{
	void SetParentSection(SectionType _sectionType);
}
