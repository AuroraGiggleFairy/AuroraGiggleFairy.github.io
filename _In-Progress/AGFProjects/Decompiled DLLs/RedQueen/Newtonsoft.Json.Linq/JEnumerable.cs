using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq;

[_003C7efad6e0_002D6dbc_002D40f5_002Dac7f_002De8a284fe164b_003EIsReadOnly]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal struct JEnumerable<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)] T> : IJEnumerable<T>, IEnumerable<T>, IEnumerable, IEquatable<JEnumerable<T>> where T : JToken
{
	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 0, 1 })]
	public static readonly JEnumerable<T> Empty = new JEnumerable<T>(Enumerable.Empty<T>());

	private readonly IEnumerable<T> _enumerable;

	public IJEnumerable<JToken> this[object key]
	{
		get
		{
			if (_enumerable == null)
			{
				return JEnumerable<JToken>.Empty;
			}
			return new JEnumerable<JToken>(_enumerable.Values<T, JToken>(key));
		}
	}

	public JEnumerable(IEnumerable<T> enumerable)
	{
		ValidationUtils.ArgumentNotNull(enumerable, "enumerable");
		_enumerable = enumerable;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return ((IEnumerable<T>)(_enumerable ?? ((object)Empty))).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool Equals([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 0, 1 })] JEnumerable<T> other)
	{
		return object.Equals(_enumerable, other._enumerable);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	public override bool Equals(object obj)
	{
		if (obj is JEnumerable<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (_enumerable == null)
		{
			return 0;
		}
		return _enumerable.GetHashCode();
	}
}
