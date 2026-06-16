using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class ReferenceEqualityComparer : IEqualityComparer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static ReferenceEqualityComparer instance;

	public static ReferenceEqualityComparer Instance
	{
		get
		{
			if (instance == null)
			{
				ReferenceEqualityComparer value = new ReferenceEqualityComparer();
				Interlocked.CompareExchange(ref instance, value, null);
			}
			return instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ReferenceEqualityComparer()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	bool IEqualityComparer.Equals(object _x, object _y)
	{
		return _x == _y;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	int IEqualityComparer.GetHashCode(object _obj)
	{
		return _obj?.GetHashCode() ?? 0;
	}
}
public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static ReferenceEqualityComparer<T> instance;

	public static ReferenceEqualityComparer<T> Instance
	{
		get
		{
			if (instance == null)
			{
				ReferenceEqualityComparer<T> value = new ReferenceEqualityComparer<T>();
				Interlocked.CompareExchange(ref instance, value, null);
			}
			return instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ReferenceEqualityComparer()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	bool IEqualityComparer<T>.Equals(T _x, T _y)
	{
		return _x == _y;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	int IEqualityComparer<T>.GetHashCode(T _obj)
	{
		return _obj?.GetHashCode() ?? 0;
	}
}
