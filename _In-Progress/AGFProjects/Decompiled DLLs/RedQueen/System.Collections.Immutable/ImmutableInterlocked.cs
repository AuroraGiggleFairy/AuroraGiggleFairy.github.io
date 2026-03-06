using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Collections.Immutable;

[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(1)]
[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(0)]
internal static class ImmutableInterlocked
{
	public static bool Update<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(ref T location, Func<T, T> transformer) where T : class
	{
		Requires.NotNull(transformer, "transformer");
		T val = Volatile.Read(ref location);
		bool flag;
		do
		{
			T val2 = transformer(val);
			if (val == val2)
			{
				return false;
			}
			T val3 = Interlocked.CompareExchange(ref location, val2, val);
			flag = val == val3;
			val = val3;
		}
		while (!flag);
		return true;
	}

	public static bool Update<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TArg>(ref T location, Func<T, TArg, T> transformer, TArg transformerArgument) where T : class
	{
		Requires.NotNull(transformer, "transformer");
		T val = Volatile.Read(ref location);
		bool flag;
		do
		{
			T val2 = transformer(val, transformerArgument);
			if (val == val2)
			{
				return false;
			}
			T val3 = Interlocked.CompareExchange(ref location, val2, val);
			flag = val == val3;
			val = val3;
		}
		while (!flag);
		return true;
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	public static bool Update<T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] ref System.Collections.Immutable.ImmutableArray<T> location, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 0, 1 })] Func<System.Collections.Immutable.ImmutableArray<T>, System.Collections.Immutable.ImmutableArray<T>> transformer)
	{
		Requires.NotNull(transformer, "transformer");
		T[] array = Volatile.Read(ref location.array);
		bool flag;
		do
		{
			System.Collections.Immutable.ImmutableArray<T> immutableArray = transformer(new System.Collections.Immutable.ImmutableArray<T>(array));
			if (array == immutableArray.array)
			{
				return false;
			}
			T[] array2 = Interlocked.CompareExchange(ref location.array, immutableArray.array, array);
			flag = array == array2;
			array = array2;
		}
		while (!flag);
		return true;
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	public static bool Update<T, TArg>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] ref System.Collections.Immutable.ImmutableArray<T> location, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 1, 0, 1, 1, 0, 1 })] Func<System.Collections.Immutable.ImmutableArray<T>, TArg, System.Collections.Immutable.ImmutableArray<T>> transformer, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(1)] TArg transformerArgument)
	{
		Requires.NotNull(transformer, "transformer");
		T[] array = Volatile.Read(ref location.array);
		bool flag;
		do
		{
			System.Collections.Immutable.ImmutableArray<T> immutableArray = transformer(new System.Collections.Immutable.ImmutableArray<T>(array), transformerArgument);
			if (array == immutableArray.array)
			{
				return false;
			}
			T[] array2 = Interlocked.CompareExchange(ref location.array, immutableArray.array, array);
			flag = array == array2;
			array = array2;
		}
		while (!flag);
		return true;
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public static System.Collections.Immutable.ImmutableArray<T> InterlockedExchange<T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] ref System.Collections.Immutable.ImmutableArray<T> location, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T> value)
	{
		return new System.Collections.Immutable.ImmutableArray<T>(Interlocked.Exchange(ref location.array, value.array));
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	[return: _003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })]
	public static System.Collections.Immutable.ImmutableArray<T> InterlockedCompareExchange<T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] ref System.Collections.Immutable.ImmutableArray<T> location, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T> value, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T> comparand)
	{
		return new System.Collections.Immutable.ImmutableArray<T>(Interlocked.CompareExchange(ref location.array, value.array, comparand.array));
	}

	[_003Ccef8cac1_002Da3f8_002D48fb_002D901d_002Deab912a32728_003ENullableContext(2)]
	public static bool InterlockedInitialize<T>([_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] ref System.Collections.Immutable.ImmutableArray<T> location, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(new byte[] { 0, 1 })] System.Collections.Immutable.ImmutableArray<T> value)
	{
		return InterlockedCompareExchange(ref location, value, default(System.Collections.Immutable.ImmutableArray<T>)).IsDefault;
	}

	public static TValue GetOrAdd<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TArg>(ref ImmutableDictionary<TKey, TValue> location, TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument)
	{
		Requires.NotNull(valueFactory, "valueFactory");
		ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
		Requires.NotNull(immutableDictionary, "location");
		if (immutableDictionary.TryGetValue(key, out var value))
		{
			return value;
		}
		value = valueFactory(key, factoryArgument);
		return GetOrAdd(ref location, key, value);
	}

	public static TValue GetOrAdd<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, Func<TKey, TValue> valueFactory)
	{
		Requires.NotNull(valueFactory, "valueFactory");
		ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
		Requires.NotNull(immutableDictionary, "location");
		if (immutableDictionary.TryGetValue(key, out var value))
		{
			return value;
		}
		value = valueFactory(key);
		return GetOrAdd(ref location, key, value);
	}

	public static TValue GetOrAdd<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue value)
	{
		ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
		bool flag;
		do
		{
			Requires.NotNull(immutableDictionary, "location");
			if (immutableDictionary.TryGetValue(key, out var value2))
			{
				return value2;
			}
			ImmutableDictionary<TKey, TValue> value3 = immutableDictionary.Add(key, value);
			ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange(ref location, value3, immutableDictionary);
			flag = immutableDictionary == immutableDictionary2;
			immutableDictionary = immutableDictionary2;
		}
		while (!flag);
		return value;
	}

	public static TValue AddOrUpdate<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
	{
		Requires.NotNull(addValueFactory, "addValueFactory");
		Requires.NotNull(updateValueFactory, "updateValueFactory");
		ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
		TValue val;
		bool flag;
		do
		{
			Requires.NotNull(immutableDictionary, "location");
			val = ((!immutableDictionary.TryGetValue(key, out var value)) ? addValueFactory(key) : updateValueFactory(key, value));
			ImmutableDictionary<TKey, TValue> value2 = immutableDictionary.SetItem(key, val);
			ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange(ref location, value2, immutableDictionary);
			flag = immutableDictionary == immutableDictionary2;
			immutableDictionary = immutableDictionary2;
		}
		while (!flag);
		return val;
	}

	public static TValue AddOrUpdate<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
	{
		Requires.NotNull(updateValueFactory, "updateValueFactory");
		ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
		TValue val;
		bool flag;
		do
		{
			Requires.NotNull(immutableDictionary, "location");
			val = ((!immutableDictionary.TryGetValue(key, out var value)) ? addValue : updateValueFactory(key, value));
			ImmutableDictionary<TKey, TValue> value2 = immutableDictionary.SetItem(key, val);
			ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange(ref location, value2, immutableDictionary);
			flag = immutableDictionary == immutableDictionary2;
			immutableDictionary = immutableDictionary2;
		}
		while (!flag);
		return val;
	}

	public static bool TryAdd<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue value)
	{
		ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
		bool flag;
		do
		{
			Requires.NotNull(immutableDictionary, "location");
			if (immutableDictionary.ContainsKey(key))
			{
				return false;
			}
			ImmutableDictionary<TKey, TValue> value2 = immutableDictionary.Add(key, value);
			ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange(ref location, value2, immutableDictionary);
			flag = immutableDictionary == immutableDictionary2;
			immutableDictionary = immutableDictionary2;
		}
		while (!flag);
		return true;
	}

	public static bool TryUpdate<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue newValue, TValue comparisonValue)
	{
		EqualityComparer<TValue> equalityComparer = EqualityComparer<TValue>.Default;
		ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
		bool flag;
		do
		{
			Requires.NotNull(immutableDictionary, "location");
			if (!immutableDictionary.TryGetValue(key, out var value) || !equalityComparer.Equals(value, comparisonValue))
			{
				return false;
			}
			ImmutableDictionary<TKey, TValue> value2 = immutableDictionary.SetItem(key, newValue);
			ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange(ref location, value2, immutableDictionary);
			flag = immutableDictionary == immutableDictionary2;
			immutableDictionary = immutableDictionary2;
		}
		while (!flag);
		return true;
	}

	public static bool TryRemove<TKey, [_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, [_003C6723b510_002D2ae0_002D4796_002Dbe1b_002D098bdaf7a574_003EMaybeNullWhen(false)] out TValue value)
	{
		ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
		bool flag;
		do
		{
			Requires.NotNull(immutableDictionary, "location");
			if (!immutableDictionary.TryGetValue(key, out value))
			{
				return false;
			}
			ImmutableDictionary<TKey, TValue> value2 = immutableDictionary.Remove(key);
			ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange(ref location, value2, immutableDictionary);
			flag = immutableDictionary == immutableDictionary2;
			immutableDictionary = immutableDictionary2;
		}
		while (!flag);
		return true;
	}

	public static bool TryPop<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(ref ImmutableStack<T> location, [_003C6723b510_002D2ae0_002D4796_002Dbe1b_002D098bdaf7a574_003EMaybeNullWhen(false)] out T value)
	{
		ImmutableStack<T> immutableStack = Volatile.Read(ref location);
		bool flag;
		do
		{
			Requires.NotNull(immutableStack, "location");
			if (immutableStack.IsEmpty)
			{
				value = default(T);
				return false;
			}
			ImmutableStack<T> value2 = immutableStack.Pop(out value);
			ImmutableStack<T> immutableStack2 = Interlocked.CompareExchange(ref location, value2, immutableStack);
			flag = immutableStack == immutableStack2;
			immutableStack = immutableStack2;
		}
		while (!flag);
		return true;
	}

	public static void Push<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(ref ImmutableStack<T> location, T value)
	{
		ImmutableStack<T> immutableStack = Volatile.Read(ref location);
		bool flag;
		do
		{
			Requires.NotNull(immutableStack, "location");
			ImmutableStack<T> value2 = immutableStack.Push(value);
			ImmutableStack<T> immutableStack2 = Interlocked.CompareExchange(ref location, value2, immutableStack);
			flag = immutableStack == immutableStack2;
			immutableStack = immutableStack2;
		}
		while (!flag);
	}

	public static bool TryDequeue<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(ref ImmutableQueue<T> location, [_003C6723b510_002D2ae0_002D4796_002Dbe1b_002D098bdaf7a574_003EMaybeNullWhen(false)] out T value)
	{
		ImmutableQueue<T> immutableQueue = Volatile.Read(ref location);
		bool flag;
		do
		{
			Requires.NotNull(immutableQueue, "location");
			if (immutableQueue.IsEmpty)
			{
				value = default(T);
				return false;
			}
			ImmutableQueue<T> value2 = immutableQueue.Dequeue(out value);
			ImmutableQueue<T> immutableQueue2 = Interlocked.CompareExchange(ref location, value2, immutableQueue);
			flag = immutableQueue == immutableQueue2;
			immutableQueue = immutableQueue2;
		}
		while (!flag);
		return true;
	}

	public static void Enqueue<[_003C6b98f2e6_002Dd0ee_002D434d_002D9b89_002D930bc51b5d5c_003ENullable(2)] T>(ref ImmutableQueue<T> location, T value)
	{
		ImmutableQueue<T> immutableQueue = Volatile.Read(ref location);
		bool flag;
		do
		{
			Requires.NotNull(immutableQueue, "location");
			ImmutableQueue<T> value2 = immutableQueue.Enqueue(value);
			ImmutableQueue<T> immutableQueue2 = Interlocked.CompareExchange(ref location, value2, immutableQueue);
			flag = immutableQueue == immutableQueue2;
			immutableQueue = immutableQueue2;
		}
		while (!flag);
	}
}
