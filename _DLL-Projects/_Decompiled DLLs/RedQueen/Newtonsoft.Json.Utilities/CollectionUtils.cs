using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Utilities;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal static class CollectionUtils
{
	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(0)]
	private static class EmptyArrayContainer<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T>
	{
		[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
		public static readonly T[] Empty = new T[0];
	}

	public static bool IsNullOrEmpty<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T>(ICollection<T> collection)
	{
		if (collection != null)
		{
			return collection.Count == 0;
		}
		return true;
	}

	public static void AddRange<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T>(this IList<T> initial, IEnumerable<T> collection)
	{
		if (initial == null)
		{
			throw new ArgumentNullException("initial");
		}
		if (collection == null)
		{
			return;
		}
		foreach (T item in collection)
		{
			initial.Add(item);
		}
	}

	public static bool IsDictionaryType(Type type)
	{
		ValidationUtils.ArgumentNotNull(type, "type");
		if (typeof(IDictionary).IsAssignableFrom(type))
		{
			return true;
		}
		if (ReflectionUtils.ImplementsGenericDefinition(type, typeof(IDictionary<, >)))
		{
			return true;
		}
		if (ReflectionUtils.ImplementsGenericDefinition(type, typeof(IReadOnlyDictionary<, >)))
		{
			return true;
		}
		return false;
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public static ConstructorInfo ResolveEnumerableCollectionConstructor(Type collectionType, Type collectionItemType)
	{
		Type constructorArgumentType = typeof(IList<>).MakeGenericType(collectionItemType);
		return ResolveEnumerableCollectionConstructor(collectionType, collectionItemType, constructorArgumentType);
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public static ConstructorInfo ResolveEnumerableCollectionConstructor(Type collectionType, Type collectionItemType, Type constructorArgumentType)
	{
		Type type = typeof(IEnumerable<>).MakeGenericType(collectionItemType);
		ConstructorInfo constructorInfo = null;
		ConstructorInfo[] constructors = collectionType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
		foreach (ConstructorInfo constructorInfo2 in constructors)
		{
			IList<ParameterInfo> parameters = constructorInfo2.GetParameters();
			if (parameters.Count == 1)
			{
				Type parameterType = parameters[0].ParameterType;
				if (type == parameterType)
				{
					constructorInfo = constructorInfo2;
					break;
				}
				if (constructorInfo == null && parameterType.IsAssignableFrom(constructorArgumentType))
				{
					constructorInfo = constructorInfo2;
				}
			}
		}
		return constructorInfo;
	}

	public static bool AddDistinct<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T>(this IList<T> list, T value)
	{
		return list.AddDistinct(value, EqualityComparer<T>.Default);
	}

	public static bool AddDistinct<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T>(this IList<T> list, T value, IEqualityComparer<T> comparer)
	{
		if (list.ContainsValue(value, comparer))
		{
			return false;
		}
		list.Add(value);
		return true;
	}

	public static bool ContainsValue<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
	{
		if (comparer == null)
		{
			comparer = EqualityComparer<TSource>.Default;
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		foreach (TSource item in source)
		{
			if (comparer.Equals(item, value))
			{
				return true;
			}
		}
		return false;
	}

	public static bool AddRangeDistinct<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T>(this IList<T> list, IEnumerable<T> values, IEqualityComparer<T> comparer)
	{
		bool result = true;
		foreach (T value in values)
		{
			if (!list.AddDistinct(value, comparer))
			{
				result = false;
			}
		}
		return result;
	}

	public static int IndexOf<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T>(this IEnumerable<T> collection, Func<T, bool> predicate)
	{
		int num = 0;
		foreach (T item in collection)
		{
			if (predicate(item))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public static bool Contains<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T>(this List<T> list, T value, IEqualityComparer comparer)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (comparer.Equals(value, list[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static int IndexOfReference<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T>(this List<T> list, T item)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if ((object)item == (object)list[i])
			{
				return i;
			}
		}
		return -1;
	}

	public static void FastReverse<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T>(this List<T> list)
	{
		int num = 0;
		int num2 = list.Count - 1;
		while (num < num2)
		{
			T value = list[num];
			list[num] = list[num2];
			list[num2] = value;
			num++;
			num2--;
		}
	}

	private static IList<int> GetDimensions(IList values, int dimensionsCount)
	{
		IList<int> list = new List<int>();
		IList list2 = values;
		while (true)
		{
			list.Add(list2.Count);
			if (list.Count == dimensionsCount || list2.Count == 0 || !(list2[0] is IList list3))
			{
				break;
			}
			list2 = list3;
		}
		return list;
	}

	private static void CopyFromJaggedToMultidimensionalArray(IList values, Array multidimensionalArray, int[] indices)
	{
		int num = indices.Length;
		if (num == multidimensionalArray.Rank)
		{
			multidimensionalArray.SetValue(JaggedArrayGetValue(values, indices), indices);
			return;
		}
		int length = multidimensionalArray.GetLength(num);
		if (((IList)JaggedArrayGetValue(values, indices)).Count != length)
		{
			throw new Exception("Cannot deserialize non-cubical array as multidimensional array.");
		}
		int[] array = new int[num + 1];
		for (int i = 0; i < num; i++)
		{
			array[i] = indices[i];
		}
		for (int j = 0; j < multidimensionalArray.GetLength(num); j++)
		{
			array[num] = j;
			CopyFromJaggedToMultidimensionalArray(values, multidimensionalArray, array);
		}
	}

	private static object JaggedArrayGetValue(IList values, int[] indices)
	{
		IList list = values;
		for (int i = 0; i < indices.Length; i++)
		{
			int index = indices[i];
			if (i == indices.Length - 1)
			{
				return list[index];
			}
			list = (IList)list[index];
		}
		return list;
	}

	public static Array ToMultidimensionalArray(IList values, Type type, int rank)
	{
		IList<int> dimensions = GetDimensions(values, rank);
		while (dimensions.Count < rank)
		{
			dimensions.Add(0);
		}
		Array array = Array.CreateInstance(type, dimensions.ToArray());
		CopyFromJaggedToMultidimensionalArray(values, array, ArrayEmpty<int>());
		return array;
	}

	public static T[] ArrayEmpty<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T>()
	{
		return EmptyArrayContainer<T>.Empty;
	}
}
