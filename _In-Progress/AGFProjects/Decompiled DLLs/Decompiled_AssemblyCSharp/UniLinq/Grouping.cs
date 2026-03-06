using System.Collections;
using System.Collections.Generic;

namespace UniLinq;

[PublicizedFrom(EAccessModifier.Internal)]
public class Grouping<K, T> : IGrouping<K, T>, IEnumerable<T>, IEnumerable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public K key;

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerable<T> group;

	public K Key
	{
		get
		{
			return key;
		}
		set
		{
			key = value;
		}
	}

	public Grouping(K key, IEnumerable<T> group)
	{
		this.group = group;
		this.key = key;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return group.GetEnumerator();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return group.GetEnumerator();
	}
}
