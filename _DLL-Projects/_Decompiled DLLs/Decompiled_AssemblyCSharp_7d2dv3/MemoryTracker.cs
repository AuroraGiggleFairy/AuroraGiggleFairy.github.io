using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UniLinq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class MemoryTracker
{
	public struct EstimatedMemoryContext(int lame)
	{
		public Dictionary<string, int> InteredStrings = new Dictionary<string, int>();

		public Dictionary<string, int> NonInteredStrings = new Dictionary<string, int>();

		public long ActualBytes = 0L;

		public long BytesInterned = 0L;
	}

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
			if (allocTypeDict.TryGetValue(typeof(DynamicProperties), out var value3))
			{
				int num2 = 0;
				EstimatedMemoryContext estimatedMemoryContext = new EstimatedMemoryContext(0);
				foreach (Allocation allocation2 in value3.allocations)
				{
					if (allocation2.obj.TryGetTarget(out var target))
					{
						num2++;
						EstimateMemoryUsage(target, ref estimatedMemoryContext);
					}
				}
				double num3 = (double)estimatedMemoryContext.ActualBytes * 9.5367431640625E-07;
				double num4 = (double)estimatedMemoryContext.BytesInterned * 9.5367431640625E-07;
				long num5 = 0L;
				long num6 = 0L;
				foreach (KeyValuePair<string, int> interedString in estimatedMemoryContext.InteredStrings)
				{
					num5 += GetSize(interedString.Key);
				}
				foreach (KeyValuePair<string, int> nonInteredString in estimatedMemoryContext.NonInteredStrings)
				{
					num6 += GetSize(nonInteredString.Key) * nonInteredString.Value;
				}
				double num7 = (double)num5 * 9.5367431640625E-07;
				double num8 = (double)num6 * 9.5367431640625E-07;
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("---CSV DynamicProperties---------------------");
				stringBuilder.AppendLine("Type,Count,ActualBytes(MB),BytesInterned(MB),InternedStrings,InternedStrings(MB),NonInternedStrings,NonInternedStrings(MB)");
				stringBuilder.AppendLine($"DynamicProperties,{num2},{num3:F2},{num4:F2},{estimatedMemoryContext.InteredStrings.Count},{num7:F2},{estimatedMemoryContext.NonInteredStrings.Count},{num8:F2}");
				Log.Out(stringBuilder.ToString());
				StringBuilder stringBuilder2 = new StringBuilder();
				stringBuilder2.AppendLine("---CSV InteredStrings---------------------");
				stringBuilder2.AppendLine("InteredStrings,Count");
				foreach (KeyValuePair<string, int> item2 in estimatedMemoryContext.InteredStrings.OrderByDescending([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<string, int> x) => x.Value))
				{
					stringBuilder2.AppendLine($"{item2.Key},{item2.Value}");
				}
				Log.Out(stringBuilder2.ToString());
				StringBuilder stringBuilder3 = new StringBuilder();
				stringBuilder3.AppendLine("---CSV NonInteredStrings---------------------");
				stringBuilder3.AppendLine("NonInteredStrings,Count");
				foreach (KeyValuePair<string, int> item3 in estimatedMemoryContext.NonInteredStrings.OrderByDescending([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<string, int> x) => x.Value))
				{
					stringBuilder3.AppendLine($"{item3.Key},{item3.Value}");
				}
				Log.Out(stringBuilder3.ToString());
			}
		}
		int num9 = EstimateSelfBytes();
		dictionary.Add(typeof(MemoryTracker).ToString(), new AllocationsForType.Summary(num9));
		num += num9;
		double num10 = (double)(totalMemory - num) * 9.5367431640625E-07;
		Log.Out("GC.GetTotalMemory (MB): {0:F2}", (double)totalMemory * 9.5367431640625E-07);
		Log.Out("Total Tracked (MB): {0:F2}", (double)num * 9.5367431640625E-07);
		Log.Out("Untracked (MB): {0:F2}", num10);
		if (num <= 0)
		{
			return;
		}
		StringBuilder stringBuilder4 = new StringBuilder();
		stringBuilder4.AppendLine("---CSV----------------------------------------");
		stringBuilder4.AppendLine("Type,Bytes(MB),Count,GC Count,");
		stringBuilder4.AppendLine($"Untracked,{num10:F2},0,0");
		Log.Out("---Tracked----------------------------------------");
		foreach (KeyValuePair<string, AllocationsForType.Summary> item4 in dictionary)
		{
			string key2 = item4.Key;
			AllocationsForType.Summary value4 = item4.Value;
			double num11 = (double)value4.totalBytes * 9.5367431640625E-07;
			stringBuilder4.AppendLine($"{key2},{num11:F2},{value4.numInstances},{value4.numGC}");
		}
		Log.Out(stringBuilder4.ToString());
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

	public static int GetSize<T>(IBackedArray<T> _array) where T : unmanaged
	{
		int num = IntPtr.Size;
		if (_array != null)
		{
			num += GetSize<T>() * _array.Length;
		}
		return num;
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

	public static void EstimateMemoryUsage(object _obj, ref EstimatedMemoryContext estimatedMemoryContext)
	{
		if (_obj == null)
		{
			estimatedMemoryContext.ActualBytes += IntPtr.Size;
			return;
		}
		Type type = _obj.GetType();
		if (type.IsEnum)
		{
			estimatedMemoryContext.ActualBytes += Marshal.SizeOf(Enum.GetUnderlyingType(type));
		}
		else if (type.IsValueType)
		{
			estimatedMemoryContext.ActualBytes += UnsafeUtility.SizeOf(type);
		}
		else if (type.IsArray)
		{
			Type elementType = type.GetElementType();
			Array array = _obj as Array;
			_ = IntPtr.Size;
			if (array != null)
			{
				for (int i = 0; i < array.Rank; i++)
				{
					estimatedMemoryContext.ActualBytes += array.GetLength(i) * GetSize(elementType);
				}
			}
		}
		else if (typeof(string).IsAssignableFrom(type))
		{
			string text = (string)_obj;
			estimatedMemoryContext.ActualBytes += IntPtr.Size;
			string text2 = string.IsInterned(text);
			if (text2 != null)
			{
				int value;
				bool num = estimatedMemoryContext.InteredStrings.TryGetValue(text2, out value);
				estimatedMemoryContext.InteredStrings[text2] = value + 1;
				if (!num)
				{
					estimatedMemoryContext.ActualBytes += GetSize(text);
					estimatedMemoryContext.BytesInterned += GetSize(text);
				}
			}
			else
			{
				int value2;
				bool num2 = estimatedMemoryContext.NonInteredStrings.TryGetValue(text, out value2);
				estimatedMemoryContext.NonInteredStrings[text] = value2 + 1;
				estimatedMemoryContext.ActualBytes += GetSize(text);
				if (!num2)
				{
					estimatedMemoryContext.BytesInterned += GetSize(text);
				}
			}
		}
		else if (!(_obj is DynamicProperties))
		{
			estimatedMemoryContext.ActualBytes += IntPtr.Size;
		}
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
