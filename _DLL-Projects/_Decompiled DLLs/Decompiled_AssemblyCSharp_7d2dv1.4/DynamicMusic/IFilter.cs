using System.Collections.Generic;

namespace DynamicMusic;

public interface IFilter<T>
{
	List<T> Filter(List<T> _list);
}
