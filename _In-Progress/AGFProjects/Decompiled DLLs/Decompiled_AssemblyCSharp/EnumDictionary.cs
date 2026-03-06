using System;
using System.Collections.Generic;

public class EnumDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : struct, IConvertible
{
	public EnumDictionary()
		: base((IEqualityComparer<TKey>)new FastEnumIntEqualityComparer<TKey>())
	{
	}

	public EnumDictionary(int capacity)
		: base(capacity, (IEqualityComparer<TKey>)new FastEnumIntEqualityComparer<TKey>())
	{
	}

	public EnumDictionary(IDictionary<TKey, TValue> dictionary)
		: base(dictionary, (IEqualityComparer<TKey>)new FastEnumIntEqualityComparer<TKey>())
	{
	}

	[Obsolete("EnumDictionary constructors with explicit comparer are deprecated in favor of the variants without, as these automatically set the appropriate comparer for the enum key type.", true)]
	public EnumDictionary(IEqualityComparer<TKey> comparer)
		: base(comparer)
	{
	}

	[Obsolete("EnumDictionary constructors with explicit comparer are deprecated in favor of the variants without, as these automatically set the appropriate comparer for the enum key type.", true)]
	public EnumDictionary(int capacity, IEqualityComparer<TKey> comparer)
		: base(capacity, comparer)
	{
	}

	[Obsolete("EnumDictionary constructors with explicit comparer are deprecated in favor of the variants without, as these automatically set the appropriate comparer for the enum key type.", true)]
	public EnumDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
		: base(dictionary, comparer)
	{
	}
}
