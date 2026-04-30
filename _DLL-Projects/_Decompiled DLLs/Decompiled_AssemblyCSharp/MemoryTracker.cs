using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class MemoryTracker
{
	public delegate int EstimateOwnedBytes(object obj);

	public struct Allocation(object _obj, EstimateOwnedBytes _func)
	{
		public WeakReference<object> obj = new WeakReference<object>(_obj);

		public EstimateOwnedBytes estimateBytesFunc = _func;

		public int GetOwnedBytes()
		{
			if (obj.TryGetTarget(out var target))
			{
				return estimateBytesFunc(target);
			}
			return 0;
		}
	}

	public class AllocationsForType
	{
		public struct Summary(long _totalBytes)
		{
			public long totalBytes = _totalBytes;

			public int numInstances = 1;

			public int numGC = 0;
		}

		public LinkedList<Allocation> allocations = new LinkedList<Allocation>();

		public int ClearDeadAllocations()
		{
			int num = 0;
			LinkedListNode<Allocation> linkedListNode = allocations.First;
			while (linkedListNode != null)
			{
				LinkedListNode<Allocation> next = linkedListNode.Next;
				if (!linkedListNode.Value.obj.TryGetTarget(out var target) || target == null)
				{
					allocations.Remove(linkedListNode);
					num++;
				}
				linkedListNode = next;
			}
			return num;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static MemoryTracker m_Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<object, int> refs = new Dictionary<object, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<object, int> last = new Dictionary<object, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Type, AllocationsForType> allocTypeDict = new Dictionary<Type, AllocationsForType>();

	public static MemoryTracker Instance
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = new MemoryTracker();
			}
			return m_Instance;
		}
	}

	public void New(object _o)
	{
		lock (refs)
		{
			Type type = _o.GetType();
			refs[type] = ((!refs.ContainsKey(type)) ? 1 : (refs[type] + 1));
		}
	}

	public void Delete(object _o)
	{
		lock (refs)
		{
			Type type = _o.GetType();
			refs[type] -= 1;
		}
	}

	public void SetEstimationFunction(object _o, EstimateOwnedBytes _func)
	{
		if (_o == null)
		{
			return;
		}
		lock (allocTypeDict)
		{
			Type type = _o.GetType();
			if (!allocTypeDict.TryGetValue(type, out var value))
			{
				value = new AllocationsForType();
				allocTypeDict.Add(type, value);
			}
			value.allocations.AddLast(new Allocation(_o, _func));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int EstimateSelfBytes()
	{
		int num = 0;
		lock (refs)
		{
			num += GetUsedSize(refs);
			num += GetUsedSize(last);
		}
		lock (allocTypeDict)
		{
			num += GetUsedSize(allocTypeDict);
			foreach (AllocationsForType value in allocTypeDict.Values)
			{
				num += value.allocations.Count * GetSize<Allocation>();
			}
			return num;
		}
	}

	public void Dump()
	{
		Dictionary<string, AllocationsForType.Summary> dictionary = new Dictionary<string, AllocationsForType.Summary>();
		long num = 0L;
		lock (refs)
		{
			Log.Out("---Classes----------------------------------------");
			foreach (KeyValuePair<object, int> @ref in refs)
			{
				Log.Out(@ref.Key.ToString() + " = " + @ref.Value + " last = " + (last.ContainsKey(@ref.Key) ? last[@ref.Key] : 0));
				last[@ref.Key] = @ref.Value;
			}
		}
		long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
		lock (allocTypeDict)
		{
			foreach (KeyValuePair<Type, AllocationsForType> item in allocTypeDict)
			{
				Type key = item.Key;
				AllocationsForType value = item.Value;
				AllocationsForType.Summary value2 = new AllocationsForType.Summary
				{
					numGC = value.ClearDeadAllocations()
				};
				foreach (Allocation allocation in value.allocations)
				{
					value2.totalBytes += allocation.GetOwnedBytes();
					value2.numInstances++;
				}
				dictionary.Add(key.ToString(), value2);
				num += value2.totalBytes;
			}
		}
		int num2 = EstimateSelfBytes();
		dictionary.Add(typeof(MemoryTracker).ToString(), new AllocationsForType.Summary(num2));
		num += num2;
		double num3 = (double)(totalMemory - num) * 9.5367431640625E-07;
		Log.Out("GC.GetTotalMemory (MB): {0:F2}", (double)totalMemory * 9.5367431640625E-07);
		Log.Out("Total Tracked (MB): {0:F2}", (double)num * 9.5367431640625E-07);
		Log.Out("Untracked (MB): {0:F2}", num3);
		if (num <= 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder.Append("Untracked,");
		stringBuilder2.AppendFormat("{0:F2},", num3);
		Log.Out("---Tracked----------------------------------------");
		foreach (KeyValuePair<string, AllocationsForType.Summary> item2 in dictionary)
		{
			string key2 = item2.Key;
			AllocationsForType.Summary value3 = item2.Value;
			double num4 = (double)value3.totalBytes * 9.5367431640625E-07;
			Log.Out("{0}: {1:F2} MB, Count = {2}, GC Count = {3}", key2, num4, value3.numInstances, value3.numGC);
			stringBuilder.AppendFormat("{0},", key2);
			stringBuilder2.AppendFormat("{0:F2},", num4);
		}
		if (stringBuilder.Length > 0)
		{
			Log.Out("---CSV----------------------------------------");
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
			stringBuilder2.Remove(stringBuilder2.Length - 1, 1);
			Log.Out(stringBuilder.ToString());
			Log.Out(stringBuilder2.ToString());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawLabel(float _left, float _top, string _text)
	{
		Rect rect = new Rect(_left, _top, _text.Length * 20, 30f);
		Rect position = new Rect(rect);
		position.x += 1f;
		position.y += 1f;
		GUI.color = Color.black;
		GUI.Label(position, _text);
		GUI.color = Color.white;
		GUI.Label(rect, _text);
	}

	public void DebugOnGui()
	{
		DrawLabel(800f, 30f, "Type");
		DrawLabel(1000f, 30f, "Count");
		int num = 0;
		lock (refs)
		{
			foreach (KeyValuePair<object, int> @ref in refs)
			{
				DrawLabel(800f, 80 + num * 35, @ref.Key.ToString());
				DrawLabel(1000f, 80 + num * 35, @ref.Value.ToString());
				num++;
			}
		}
	}

	public static int GetSize<T>()
	{
		return GetSize(typeof(T));
	}

	public static int GetSize(Type _type)
	{
		if (_type.IsEnum)
		{
			return Marshal.SizeOf(Enum.GetUnderlyingType(_type));
		}
		if (_type.IsValueType)
		{
			return UnsafeUtility.SizeOf(_type);
		}
		return IntPtr.Size;
	}

	public static int GetSize<T>(T[] _array)
	{
		int num = IntPtr.Size;
		if (_array != null)
		{
			num += GetSize<T>() * _array.Length;
		}
		return num;
	}

	public static int GetSize<T>(T[][] _doubleArray)
	{
		int num = IntPtr.Size;
		if (_doubleArray != null)
		{
			foreach (T[] array in _doubleArray)
			{
				num += GetSize(array);
			}
		}
		return num;
	}

	public static int GetSize<T>(T[,] _array)
	{
		int num = IntPtr.Size;
		if (_array != null)
		{
			num += GetSize<T>() * _array.GetLength(0) * _array.GetLength(1);
		}
		return num;
	}

	public static int GetSize<T>(List<T> _list)
	{
		int num = IntPtr.Size;
		if (_list != null)
		{
			num += _list.Capacity * GetSize<T>();
		}
		return num;
	}

	public static int GetUsedSize<TKey, TValue>(IDictionary<TKey, TValue> _dictionary)
	{
		int num = IntPtr.Size;
		if (_dictionary != null)
		{
			num += (GetSize<TKey>() + GetSize<TValue>()) * _dictionary.Count;
		}
		return num;
	}

	public static int GetSize(string stringVal)
	{
		if (stringVal == null)
		{
			return IntPtr.Size;
		}
		return stringVal.Length * 2 + IntPtr.Size;
	}

	public static int GetSize(Dictionary<string, string> stringDict)
	{
		int num = 0;
		foreach (KeyValuePair<string, string> item in stringDict)
		{
			num += GetSize(item.Key) + GetSize(item.Value);
		}
		return num;
	}

	public static int GetSizeAuto(object _obj)
	{
		if (_obj == null)
		{
			return IntPtr.Size;
		}
		Type type = _obj.GetType();
		if (type.IsEnum)
		{
			return Marshal.SizeOf(Enum.GetUnderlyingType(type));
		}
		if (type.IsValueType)
		{
			return UnsafeUtility.SizeOf(type);
		}
		if (type.IsArray)
		{
			Type elementType = type.GetElementType();
			Array array = _obj as Array;
			int num = IntPtr.Size;
			if (array != null)
			{
				for (int i = 0; i < array.Rank; i++)
				{
					num += array.GetLength(i) * GetSize(elementType);
				}
			}
			return num;
		}
		if (typeof(string).IsAssignableFrom(type))
		{
			string text = (string)_obj;
			int num2 = IntPtr.Size;
			if (text != null)
			{
				num2 += GetSize(text);
			}
			return num2;
		}
		return IntPtr.Size;
	}
}
