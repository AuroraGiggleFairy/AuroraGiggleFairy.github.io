using System.Collections.Generic;
using MemoryPack;

public class DataItem<T> : IDataItem
{
	public delegate void OnChangeDelegate(T _oldValue, T _newValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public T internalValue;

	public IDataItemFormatter Formatter;

	public string Name => name;

	public T Value
	{
		get
		{
			return internalValue;
		}
		set
		{
			T oldValue = internalValue;
			internalValue = value;
			if (this.OnChangeDelegates != null)
			{
				this.OnChangeDelegates(oldValue, value);
			}
		}
	}

	public event OnChangeDelegate OnChangeDelegates;

	[MemoryPackConstructor]
	public DataItem()
		: this(default(T))
	{
	}

	public DataItem(T _startValue)
		: this((string)null, _startValue)
	{
	}

	public DataItem(string _name, T _startValue)
	{
		name = _name;
		internalValue = _startValue;
	}

	public override string ToString()
	{
		if (Formatter != null)
		{
			return Formatter.ToString(internalValue);
		}
		if (internalValue == null)
		{
			return "null";
		}
		return internalValue.ToString();
	}

	public static bool operator ==(DataItem<T> v1, T v2)
	{
		if (v1 != null)
		{
			return EqualityComparer<T>.Default.Equals(v1.internalValue, v2);
		}
		return v2 == null;
	}

	public static bool operator !=(DataItem<T> v1, T v2)
	{
		if (v1 != null)
		{
			return !EqualityComparer<T>.Default.Equals(v1.internalValue, v2);
		}
		return v2 != null;
	}

	public override bool Equals(object obj)
	{
		if (obj != null)
		{
			return internalValue.Equals(obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return internalValue.GetHashCode();
	}
}
