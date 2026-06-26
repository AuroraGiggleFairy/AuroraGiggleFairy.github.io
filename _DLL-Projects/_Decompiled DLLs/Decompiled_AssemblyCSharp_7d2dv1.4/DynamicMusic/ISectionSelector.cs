using MusicUtils.Enums;

namespace DynamicMusic;

public interface ISectionSelector : INotifiable<MusicActionType>, ISelector<SectionType>
{
}
