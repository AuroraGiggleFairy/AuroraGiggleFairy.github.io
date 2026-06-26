using System.Collections.Generic;
using MusicUtils.Enums;

namespace DynamicMusic;

public abstract class AbstractFilter : IFilter<SectionType>
{
	public abstract List<SectionType> Filter(List<SectionType> _list);

	[PublicizedFrom(EAccessModifier.Protected)]
	public AbstractFilter()
	{
	}
}
