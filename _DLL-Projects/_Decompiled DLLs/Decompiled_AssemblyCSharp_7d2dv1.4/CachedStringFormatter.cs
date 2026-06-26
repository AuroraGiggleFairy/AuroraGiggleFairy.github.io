using System;
using System.Collections.Generic;

public class CachedStringFormatter<T1>
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public Func<T1, string> formatter;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedResult;

	[PublicizedFrom(EAccessModifier.Private)]
	public T1 oldValue1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T1> comparer1 = EqualityComparer<T1>.Default;

	public CachedStringFormatter(Func<T1, string> _formatterFunc)
	{
		formatter = _formatterFunc;
	}

	public string Format(T1 _v1)
	{
		bool _valueChanged;
		return Format(_v1, out _valueChanged);
	}

	public string Format(T1 _v1, out bool _valueChanged)
	{
		_valueChanged = cachedResult == null;
		if (!comparer1.Equals(oldValue1, _v1))
		{
			oldValue1 = _v1;
			_valueChanged = true;
		}
		if (_valueChanged)
		{
			cachedResult = formatter(_v1);
		}
		return cachedResult;
	}
}
public class CachedStringFormatter<T1, T2>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Func<T1, T2, string> formatter;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedResult;

	[PublicizedFrom(EAccessModifier.Private)]
	public T1 oldValue1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T1> comparer1 = EqualityComparer<T1>.Default;

	[PublicizedFrom(EAccessModifier.Private)]
	public T2 oldValue2;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T2> comparer2 = EqualityComparer<T2>.Default;

	public CachedStringFormatter(Func<T1, T2, string> _formatterFunc)
	{
		formatter = _formatterFunc;
	}

	public string Format(T1 _v1, T2 _v2)
	{
		bool _valueChanged;
		return Format(_v1, _v2, out _valueChanged);
	}

	public string Format(T1 _v1, T2 _v2, out bool _valueChanged)
	{
		_valueChanged = cachedResult == null;
		if (!comparer1.Equals(oldValue1, _v1))
		{
			oldValue1 = _v1;
			_valueChanged = true;
		}
		if (!comparer2.Equals(oldValue2, _v2))
		{
			oldValue2 = _v2;
			_valueChanged = true;
		}
		if (_valueChanged)
		{
			cachedResult = formatter(_v1, _v2);
		}
		return cachedResult;
	}
}
public class CachedStringFormatter<T1, T2, T3>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Func<T1, T2, T3, string> formatter;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedResult;

	[PublicizedFrom(EAccessModifier.Private)]
	public T1 oldValue1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T1> comparer1 = EqualityComparer<T1>.Default;

	[PublicizedFrom(EAccessModifier.Private)]
	public T2 oldValue2;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T2> comparer2 = EqualityComparer<T2>.Default;

	[PublicizedFrom(EAccessModifier.Private)]
	public T3 oldValue3;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T3> comparer3 = EqualityComparer<T3>.Default;

	public CachedStringFormatter(Func<T1, T2, T3, string> _formatterFunc)
	{
		formatter = _formatterFunc;
	}

	public string Format(T1 _v1, T2 _v2, T3 _v3)
	{
		bool _valueChanged;
		return Format(_v1, _v2, _v3, out _valueChanged);
	}

	public string Format(T1 _v1, T2 _v2, T3 _v3, out bool _valueChanged)
	{
		_valueChanged = cachedResult == null;
		if (!comparer1.Equals(oldValue1, _v1))
		{
			oldValue1 = _v1;
			_valueChanged = true;
		}
		if (!comparer2.Equals(oldValue2, _v2))
		{
			oldValue2 = _v2;
			_valueChanged = true;
		}
		if (!comparer3.Equals(oldValue3, _v3))
		{
			oldValue3 = _v3;
			_valueChanged = true;
		}
		if (_valueChanged)
		{
			cachedResult = formatter(_v1, _v2, _v3);
		}
		return cachedResult;
	}
}
public class CachedStringFormatter<T1, T2, T3, T4>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Func<T1, T2, T3, T4, string> formatter;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedResult;

	[PublicizedFrom(EAccessModifier.Private)]
	public T1 oldValue1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T1> comparer1 = EqualityComparer<T1>.Default;

	[PublicizedFrom(EAccessModifier.Private)]
	public T2 oldValue2;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T2> comparer2 = EqualityComparer<T2>.Default;

	[PublicizedFrom(EAccessModifier.Private)]
	public T3 oldValue3;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T3> comparer3 = EqualityComparer<T3>.Default;

	[PublicizedFrom(EAccessModifier.Private)]
	public T4 oldValue4;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T4> comparer4 = EqualityComparer<T4>.Default;

	public CachedStringFormatter(Func<T1, T2, T3, T4, string> _formatterFunc)
	{
		formatter = _formatterFunc;
	}

	public string Format(T1 _v1, T2 _v2, T3 _v3, T4 _v4)
	{
		bool _valueChanged;
		return Format(_v1, _v2, _v3, _v4, out _valueChanged);
	}

	public string Format(T1 _v1, T2 _v2, T3 _v3, T4 _v4, out bool _valueChanged)
	{
		_valueChanged = cachedResult == null;
		if (!comparer1.Equals(oldValue1, _v1))
		{
			oldValue1 = _v1;
			_valueChanged = true;
		}
		if (!comparer2.Equals(oldValue2, _v2))
		{
			oldValue2 = _v2;
			_valueChanged = true;
		}
		if (!comparer3.Equals(oldValue3, _v3))
		{
			oldValue3 = _v3;
			_valueChanged = true;
		}
		if (!comparer4.Equals(oldValue4, _v4))
		{
			oldValue4 = _v4;
			_valueChanged = true;
		}
		if (_valueChanged)
		{
			cachedResult = formatter(_v1, _v2, _v3, _v4);
		}
		return cachedResult;
	}
}
public class CachedStringFormatter<T1, T2, T3, T4, T5>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Func<T1, T2, T3, T4, T5, string> formatter;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedResult;

	[PublicizedFrom(EAccessModifier.Private)]
	public T1 oldValue1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T1> comparer1 = EqualityComparer<T1>.Default;

	[PublicizedFrom(EAccessModifier.Private)]
	public T2 oldValue2;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T2> comparer2 = EqualityComparer<T2>.Default;

	[PublicizedFrom(EAccessModifier.Private)]
	public T3 oldValue3;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T3> comparer3 = EqualityComparer<T3>.Default;

	[PublicizedFrom(EAccessModifier.Private)]
	public T4 oldValue4;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T4> comparer4 = EqualityComparer<T4>.Default;

	[PublicizedFrom(EAccessModifier.Private)]
	public T5 oldValue5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T5> comparer5 = EqualityComparer<T5>.Default;

	public CachedStringFormatter(Func<T1, T2, T3, T4, T5, string> _formatterFunc)
	{
		formatter = _formatterFunc;
	}

	public string Format(T1 _v1, T2 _v2, T3 _v3, T4 _v4, T5 _v5)
	{
		bool _valueChanged;
		return Format(_v1, _v2, _v3, _v4, _v5, out _valueChanged);
	}

	public string Format(T1 _v1, T2 _v2, T3 _v3, T4 _v4, T5 _v5, out bool _valueChanged)
	{
		_valueChanged = cachedResult == null;
		if (!comparer1.Equals(oldValue1, _v1))
		{
			oldValue1 = _v1;
			_valueChanged = true;
		}
		if (!comparer2.Equals(oldValue2, _v2))
		{
			oldValue2 = _v2;
			_valueChanged = true;
		}
		if (!comparer3.Equals(oldValue3, _v3))
		{
			oldValue3 = _v3;
			_valueChanged = true;
		}
		if (!comparer4.Equals(oldValue4, _v4))
		{
			oldValue4 = _v4;
			_valueChanged = true;
		}
		if (!comparer5.Equals(oldValue5, _v5))
		{
			oldValue5 = _v5;
			_valueChanged = true;
		}
		if (_valueChanged)
		{
			cachedResult = formatter(_v1, _v2, _v3, _v4, _v5);
		}
		return cachedResult;
	}
}
