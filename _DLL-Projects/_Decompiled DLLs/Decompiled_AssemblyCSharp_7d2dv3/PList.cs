using System;
using System.Collections.Generic;
using System.IO;

public class PList<T> : List<T>
{
	public Action<BinaryWriter, T> writeElement;

	public Func<BinaryReader, uint, T> readElement;

	[PublicizedFrom(EAccessModifier.Protected)]
	public uint saveVersion;

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<T> toRemove = new List<T>();

	public PList()
		: this(1u)
	{
	}

	public PList(uint _saveVersion)
	{
		saveVersion = _saveVersion;
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((ushort)base.Count);
		_bw.Write((byte)saveVersion);
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			writeElement(_bw, current);
		}
	}

	public void Read(BinaryReader _br)
	{
		Clear();
		int num = _br.ReadUInt16();
		uint arg = _br.ReadByte();
		for (int i = 0; i < num; i++)
		{
			T item = readElement(_br, arg);
			Add(item);
		}
	}

	public void MarkToRemove(T _v)
	{
		toRemove.Add(_v);
	}

	public void RemoveAllMarked()
	{
		foreach (T item in toRemove)
		{
			Remove(item);
		}
		toRemove.Clear();
	}
}
