using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace WorldGenerationEngineFinal;

public class DataMap<T> where T : struct
{
	public int width;

	[PublicizedFrom(EAccessModifier.Private)]
	public T[] data;

	public DataMap(int tileWidth, T defaultValue)
	{
		width = tileWidth;
		int num = width * width;
		data = new T[num];
		for (int i = 0; i < num; i++)
		{
			data[i] = defaultValue;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Get(int x, int y)
	{
		return data[x + y * width];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Set(int x, int y, T _value)
	{
		data[x + y * width] = _value;
	}

	public void Replace(T _old, T _new)
	{
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		int num = width * width;
		for (int i = 0; i < num; i++)
		{
			if (equalityComparer.Equals(data[i], _old))
			{
				data[i] = _new;
			}
		}
	}
}
