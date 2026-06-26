using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

[MemoryPackable(GenerateType.Object)]
public class DictionarySave<T1, T2> : IMemoryPackable<DictionarySave<T1, T2>>, IMemoryPackFormatterRegister where T2 : class
{
	public delegate void DictionaryRemoveCallback(T1 _o);

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class DictionarySaveFormatter : MemoryPackFormatter<DictionarySave<T1, T2>>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref DictionarySave<T1, T2> value)
		{
			DictionarySave<T1, T2>.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DictionarySave<T1, T2> value)
		{
			DictionarySave<T1, T2>.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackInclude]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<T1, T2> dic = new Dictionary<T1, T2>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<T1> toRemove = new List<T1>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly bool KeyIsValuetype;

	public virtual T2 this[T1 _v]
	{
		get
		{
			if (!KeyIsValuetype && _v == null)
			{
				return null;
			}
			if (dic.TryGetValue(_v, out var value))
			{
				return value;
			}
			return null;
		}
		set
		{
			dic[_v] = value;
		}
	}

	public Dictionary<T1, T2> Dict => dic;

	public int Count => dic.Count;

	public bool ContainsKey(T1 _key)
	{
		return dic.ContainsKey(_key);
	}

	public bool TryGetValue(T1 _key, out T2 _value)
	{
		return dic.TryGetValue(_key, out _value);
	}

	public void Add(T1 _key, T2 _value)
	{
		dic.Add(_key, _value);
	}

	public void Remove(T1 _key)
	{
		dic.Remove(_key);
	}

	public void Clear()
	{
		dic.Clear();
	}

	public void MarkToRemove(T1 _v)
	{
		toRemove.Add(_v);
	}

	public void RemoveAllMarked(DictionaryRemoveCallback _callback)
	{
		foreach (T1 item in toRemove)
		{
			_callback(item);
		}
		toRemove.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static DictionarySave()
	{
		KeyIsValuetype = typeof(T1).IsValueType;
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DictionarySave<T1, T2>>())
		{
			MemoryPackFormatterProvider.Register(new DictionarySaveFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DictionarySave<T1, T2>[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<DictionarySave<T1, T2>>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<T1, T2>>())
		{
			MemoryPackFormatterProvider.Register(new DictionaryFormatter<T1, T2>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref DictionarySave<T1, T2>? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WriteValue(in value.dic);
		writer.WriteValue<Dictionary<T1, T2>>(value.Dict);
		writer.WriteUnmanaged<int>(value.Count);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref DictionarySave<T1, T2>? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		Dictionary<T1, T2> value2;
		if (memberCount == 3)
		{
			Dictionary<T1, T2> value3;
			int value4;
			if (value != null)
			{
				value2 = value.dic;
				value3 = value.Dict;
				value4 = value.Count;
				reader.ReadValue(ref value2);
				reader.ReadValue(ref value3);
				reader.ReadUnmanaged<int>(out value4);
				goto IL_00c8;
			}
			value2 = reader.ReadValue<Dictionary<T1, T2>>();
			value3 = reader.ReadValue<Dictionary<T1, T2>>();
			reader.ReadUnmanaged<int>(out value4);
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DictionarySave<T1, T2>), 3, memberCount);
				return;
			}
			Dictionary<T1, T2> value3;
			int value4;
			if (value == null)
			{
				value2 = null;
				value3 = null;
				value4 = 0;
			}
			else
			{
				value2 = value.dic;
				value3 = value.Dict;
				value4 = value.Count;
			}
			if (memberCount != 0)
			{
				reader.ReadValue(ref value2);
				if (memberCount != 1)
				{
					reader.ReadValue(ref value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<int>(out value4);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00c8;
			}
		}
		value = new DictionarySave<T1, T2>
		{
			dic = value2
		};
		return;
		IL_00c8:
		value.dic = value2;
	}
}
