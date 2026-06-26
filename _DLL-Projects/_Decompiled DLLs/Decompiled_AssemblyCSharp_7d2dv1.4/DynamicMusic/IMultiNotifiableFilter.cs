using MusicUtils.Enums;

namespace DynamicMusic;

public interface IMultiNotifiableFilter : INotifiable, INotifiableFilter<MusicActionType, SectionType>, INotifiable<MusicActionType>, IFilter<SectionType>
{
}
