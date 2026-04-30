using System;
using System.Collections.Generic;

public class CaseInsensitiveStringDictionary<TValue> : Dictionary<string, TValue>
{
	public CaseInsensitiveStringDictionary()
		: base((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase)
	{
	}

	public CaseInsensitiveStringDictionary(int _capacity)
		: base(_capacity, (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase)
	{
	}

	public CaseInsensitiveStringDictionary(IDictionary<string, TValue> _dictionary)
		: base(_dictionary, (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase)
	{
	}

	[Obsolete("CaseInsensitiveStringDictionary constructors with explicit comparer are deprecated in favor of the variants without, as these automatically set the appropriate comparer for the enum key type.", true)]
	public CaseInsensitiveStringDictionary(IEqualityComparer<string> _comparer)
		: base(_comparer)
	{
	}

	[Obsolete("CaseInsensitiveStringDictionary constructors with explicit comparer are deprecated in favor of the variants without, as these automatically set the appropriate comparer for the enum key type.", true)]
	public CaseInsensitiveStringDictionary(int _capacity, IEqualityComparer<string> _comparer)
		: base(_capacity, _comparer)
	{
	}

	[Obsolete("CaseInsensitiveStringDictionary constructors with explicit comparer are deprecated in favor of the variants without, as these automatically set the appropriate comparer for the enum key type.", true)]
	public CaseInsensitiveStringDictionary(IDictionary<string, TValue> _dictionary, IEqualityComparer<string> _comparer)
		: base(_dictionary, _comparer)
	{
	}
}
