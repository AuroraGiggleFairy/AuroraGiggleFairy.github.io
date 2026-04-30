using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Utilities;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class ThreadSafeStore<TKey, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] TValue>
{
	private readonly ConcurrentDictionary<TKey, TValue> _concurrentStore;

	private readonly Func<TKey, TValue> _creator;

	public ThreadSafeStore(Func<TKey, TValue> creator)
	{
		ValidationUtils.ArgumentNotNull(creator, "creator");
		_creator = creator;
		_concurrentStore = new ConcurrentDictionary<TKey, TValue>();
	}

	public TValue Get(TKey key)
	{
		return _concurrentStore.GetOrAdd(key, _creator);
	}
}
