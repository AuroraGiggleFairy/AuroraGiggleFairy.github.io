using System.Collections.Generic;
using System.Collections.Immutable;

namespace Discord;

internal class AsyncEvent<T> where T : class
{
	private readonly object _subLock = new object();

	internal System.Collections.Immutable.ImmutableArray<T> _subscriptions;

	public bool HasSubscribers => _subscriptions.Length != 0;

	public IReadOnlyList<T> Subscriptions => _subscriptions;

	public AsyncEvent()
	{
		_subscriptions = System.Collections.Immutable.ImmutableArray.Create<T>();
	}

	public void Add(T subscriber)
	{
		Preconditions.NotNull(subscriber, "subscriber");
		lock (_subLock)
		{
			_subscriptions = _subscriptions.Add(subscriber);
		}
	}

	public void Remove(T subscriber)
	{
		Preconditions.NotNull(subscriber, "subscriber");
		lock (_subLock)
		{
			_subscriptions = _subscriptions.Remove(subscriber);
		}
	}
}
