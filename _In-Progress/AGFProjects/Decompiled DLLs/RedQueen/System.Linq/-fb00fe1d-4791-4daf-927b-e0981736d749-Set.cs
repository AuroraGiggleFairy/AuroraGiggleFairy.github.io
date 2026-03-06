using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Linq;

[ExcludeFromCodeCoverage]
[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(0)]
[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(1)]
internal sealed class _003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet<[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(2)] TElement>
{
	[_003C9d6b1468_002D8822_002D4fb0_002Db953_002D36817db5a9a6_003ENullableContext(0)]
	private struct Slot
	{
		internal int _hashCode;

		internal int _next;

		[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(1)]
		internal TElement _value;
	}

	private readonly IEqualityComparer<TElement> _comparer;

	private int[] _buckets;

	[_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 1, 0, 0 })]
	private Slot[] _slots;

	internal int Count { get; private set; }

	public _003Cfb00fe1d_002D4791_002D4daf_002D927b_002De0981736d749_003ESet([_003Cf4729669_002D5e81_002D4853_002D9c3c_002D98f81f38ea17_003ENullable(new byte[] { 2, 1 })] IEqualityComparer<TElement> comparer)
	{
		_comparer = comparer ?? EqualityComparer<TElement>.Default;
		_buckets = new int[7];
		_slots = new Slot[7];
	}

	public bool Add(TElement value)
	{
		int num = InternalGetHashCode(value);
		for (int num2 = _buckets[num % _buckets.Length] - 1; num2 >= 0; num2 = _slots[num2]._next)
		{
			if (_slots[num2]._hashCode == num && _comparer.Equals(_slots[num2]._value, value))
			{
				return false;
			}
		}
		if (Count == _slots.Length)
		{
			Resize();
		}
		int count = Count;
		Count++;
		int num3 = num % _buckets.Length;
		_slots[count]._hashCode = num;
		_slots[count]._value = value;
		_slots[count]._next = _buckets[num3] - 1;
		_buckets[num3] = count + 1;
		return true;
	}

	public bool Remove(TElement value)
	{
		int num = InternalGetHashCode(value);
		int num2 = num % _buckets.Length;
		int num3 = -1;
		for (int num4 = _buckets[num2] - 1; num4 >= 0; num4 = _slots[num4]._next)
		{
			if (_slots[num4]._hashCode == num && _comparer.Equals(_slots[num4]._value, value))
			{
				if (num3 < 0)
				{
					_buckets[num2] = _slots[num4]._next + 1;
				}
				else
				{
					_slots[num3]._next = _slots[num4]._next;
				}
				_slots[num4]._hashCode = -1;
				_slots[num4]._value = default(TElement);
				_slots[num4]._next = -1;
				return true;
			}
			num3 = num4;
		}
		return false;
	}

	internal int InternalGetHashCode(TElement value)
	{
		if (value != null)
		{
			return _comparer.GetHashCode(value) & 0x7FFFFFFF;
		}
		return 0;
	}

	internal TElement[] ToArray()
	{
		TElement[] array = new TElement[Count];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = _slots[i]._value;
		}
		return array;
	}

	internal List<TElement> ToList()
	{
		int count = Count;
		List<TElement> list = new List<TElement>(count);
		for (int i = 0; i != count; i++)
		{
			list.Add(_slots[i]._value);
		}
		return list;
	}

	private void Resize()
	{
		int num = checked(Count * 2 + 1);
		int[] array = new int[num];
		Slot[] array2 = new Slot[num];
		Array.Copy(_slots, 0, array2, 0, Count);
		for (int i = 0; i < Count; i++)
		{
			int num2 = array2[i]._hashCode % num;
			array2[i]._next = array[num2] - 1;
			array[num2] = i + 1;
		}
		_buckets = array;
		_slots = array2;
	}
}
