using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Utilities;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class BidirectionalDictionary<TFirst, TSecond>
{
	private readonly IDictionary<TFirst, TSecond> _firstToSecond;

	private readonly IDictionary<TSecond, TFirst> _secondToFirst;

	private readonly string _duplicateFirstErrorMessage;

	private readonly string _duplicateSecondErrorMessage;

	public BidirectionalDictionary()
		: this((IEqualityComparer<TFirst>)EqualityComparer<TFirst>.Default, (IEqualityComparer<TSecond>)EqualityComparer<TSecond>.Default)
	{
	}

	public BidirectionalDictionary(IEqualityComparer<TFirst> firstEqualityComparer, IEqualityComparer<TSecond> secondEqualityComparer)
		: this(firstEqualityComparer, secondEqualityComparer, "Duplicate item already exists for '{0}'.", "Duplicate item already exists for '{0}'.")
	{
	}

	public BidirectionalDictionary(IEqualityComparer<TFirst> firstEqualityComparer, IEqualityComparer<TSecond> secondEqualityComparer, string duplicateFirstErrorMessage, string duplicateSecondErrorMessage)
	{
		_firstToSecond = new Dictionary<TFirst, TSecond>(firstEqualityComparer);
		_secondToFirst = new Dictionary<TSecond, TFirst>(secondEqualityComparer);
		_duplicateFirstErrorMessage = duplicateFirstErrorMessage;
		_duplicateSecondErrorMessage = duplicateSecondErrorMessage;
	}

	public void Set(TFirst first, TSecond second)
	{
		if (_firstToSecond.TryGetValue(first, out var value) && !value.Equals(second))
		{
			throw new ArgumentException(_duplicateFirstErrorMessage.FormatWith(CultureInfo.InvariantCulture, first));
		}
		if (_secondToFirst.TryGetValue(second, out var value2) && !value2.Equals(first))
		{
			throw new ArgumentException(_duplicateSecondErrorMessage.FormatWith(CultureInfo.InvariantCulture, second));
		}
		_firstToSecond.Add(first, second);
		_secondToFirst.Add(second, first);
	}

	public bool TryGetByFirst(TFirst first, [_003C49f72aa1_002Dca2e_002D4970_002D89f5_002D98556253c04f_003ENotNullWhen(true)][_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] out TSecond second)
	{
		return _firstToSecond.TryGetValue(first, out second);
	}

	public bool TryGetBySecond(TSecond second, [_003C49f72aa1_002Dca2e_002D4970_002D89f5_002D98556253c04f_003ENotNullWhen(true)][_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] out TFirst first)
	{
		return _secondToFirst.TryGetValue(second, out first);
	}
}
