using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class DatabaseWithFixedDS<KEY, DATA> where KEY : struct where DATA : class
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cHeaderSize = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int VERSION = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public int magicBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sizeofKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxCountOfDataSets;

	[PublicizedFrom(EAccessModifier.Private)]
	public int oldMaxCountOfDataSets;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sizeOfDataSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public KEY invalidKeyValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionaryKeyList<KEY, int> catalog = new DictionaryKeyList<KEY, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionarySave<KEY, DATA> database = new DictionarySave<KEY, DATA>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<KEY, bool> dirty = new Dictionary<KEY, bool>();

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] arrayReadBuf;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] arrayWriteBuf;

	public DatabaseWithFixedDS(int _magicBytes, int _sizeofKey, int _maxCountOfDataSets, int _sizeOfDataSet, KEY _invalidKeyValue, int _oldMaxCountOfDataSets = -1)
	{
		magicBytes = _magicBytes;
		sizeofKey = _sizeofKey;
		maxCountOfDataSets = _maxCountOfDataSets;
		oldMaxCountOfDataSets = _oldMaxCountOfDataSets;
		sizeOfDataSet = _sizeOfDataSet;
		invalidKeyValue = _invalidKeyValue;
	}

	public List<KEY> GetAllKeys()
	{
		lock (catalog)
		{
			return catalog.list;
		}
	}

	public DATA GetDS(KEY _key)
	{
		lock (catalog)
		{
			if (!catalog.dict.ContainsKey(_key))
			{
				return null;
			}
			return database[_key];
		}
	}

	public void SetDS(KEY _key, DATA _data)
	{
		lock (catalog)
		{
			if (catalog.dict.Count >= maxCountOfDataSets)
			{
				int num = maxCountOfDataSets / 10;
				int num2 = 0;
				while (catalog.list.Count > 0 && num2 < num)
				{
					KEY key = catalog.list[0];
					catalog.Remove(key);
					database.Remove(key);
					num2++;
				}
				dirty.Clear();
				for (int i = 0; i < catalog.list.Count; i++)
				{
					dirty[catalog.list[i]] = true;
				}
			}
			if (!catalog.dict.ContainsKey(_key))
			{
				catalog.Add(_key, catalog.list.Count);
			}
			database[_key] = _data;
			dirty[_key] = true;
		}
	}

	public bool ContainsDS(KEY _key)
	{
		lock (catalog)
		{
			return catalog.dict.ContainsKey(_key);
		}
	}

	public int CountDS()
	{
		lock (catalog)
		{
			return catalog.list.Count;
		}
	}

	public virtual void Clear()
	{
		lock (catalog)
		{
			catalog.Clear();
			database.Clear();
			dirty.Clear();
		}
	}

	public void Load(string _dir, string _filename)
	{
		try
		{
			if (!SdFile.Exists(_dir + "/" + _filename))
			{
				return;
			}
			using Stream baseStream = SdFile.OpenRead(_dir + "/" + _filename);
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader.SetBaseStream(baseStream);
			int num = pooledBinaryReader.ReadInt32();
			if (num != magicBytes)
			{
				Log.Error($"Map file has invalid magic bytes: 0x{num:X8}");
				return;
			}
			uint version = pooledBinaryReader.ReadByte();
			if (arrayReadBuf == null)
			{
				arrayReadBuf = new byte[sizeOfDataSet];
			}
			pooledBinaryReader.Read(arrayReadBuf, 0, 3);
			read(pooledBinaryReader, version);
		}
		catch (Exception ex)
		{
			Log.Error("Could not load file '" + _dir + "/" + _filename + "': " + ex.Message);
			Log.Exception(ex);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void read(BinaryReader _br, uint _version)
	{
		lock (catalog)
		{
			int num = oldMaxCountOfDataSets;
			if (_version <= 2 && oldMaxCountOfDataSets < 0)
			{
				throw new Exception("Can not load old dataset file, unknown size of key list");
			}
			if (_version > 2)
			{
				num = _br.ReadInt32();
			}
			catalog.Clear();
			dirty.Clear();
			uint num2 = _br.ReadUInt32();
			for (uint num3 = 0u; num3 < num2; num3++)
			{
				KEY key = readKey(_br);
				catalog.Add(key, (int)num3);
			}
			for (uint num4 = num2; num4 < num; num4++)
			{
				readKey(_br);
			}
			database.Clear();
			for (int i = 0; i < catalog.list.Count; i++)
			{
				_br.Read(arrayReadBuf, 0, arrayReadBuf.Length);
				DATA val = allocateDataStorage();
				copyFromRead(arrayReadBuf, val);
				database[catalog.list[i]] = val;
			}
			if (num == maxCountOfDataSets)
			{
				return;
			}
			if (num > maxCountOfDataSets)
			{
				int num5 = num - maxCountOfDataSets;
				int num6 = 0;
				while (catalog.list.Count > 0 && num6 < num5)
				{
					KEY key2 = catalog.list[0];
					catalog.Remove(key2);
					database.Remove(key2);
					num6++;
				}
				dirty.Clear();
				for (int j = 0; j < catalog.list.Count; j++)
				{
					dirty[catalog.list[j]] = true;
				}
			}
			else
			{
				dirty.Clear();
				for (int k = 0; k < catalog.list.Count; k++)
				{
					dirty[catalog.list[k]] = true;
				}
			}
		}
	}

	public void Save(string _dir, string _filename)
	{
		try
		{
			if (!SdDirectory.Exists(_dir))
			{
				SdDirectory.CreateDirectory(_dir);
			}
			lock (this)
			{
				using Stream baseStream = SdFile.Open(_dir + "/" + _filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
				using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
				pooledBinaryWriter.SetBaseStream(baseStream);
				pooledBinaryWriter.BaseStream.Seek(0L, SeekOrigin.Begin);
				pooledBinaryWriter.Write(magicBytes);
				pooledBinaryWriter.Write((byte)3);
				pooledBinaryWriter.Write((byte)0);
				pooledBinaryWriter.Write((byte)0);
				pooledBinaryWriter.Write((byte)0);
				write(pooledBinaryWriter);
			}
		}
		catch (Exception ex)
		{
			Log.Error("Could not save file '" + _dir + "/" + _filename + "': " + ex.Message);
			Log.Exception(ex);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void write(BinaryWriter _bw)
	{
		lock (catalog)
		{
			_bw.Write((uint)maxCountOfDataSets);
			_bw.Write((uint)catalog.list.Count);
			for (int i = 0; i < catalog.list.Count; i++)
			{
				writeKey(_bw, catalog.list[i]);
			}
			for (int j = catalog.list.Count; j < maxCountOfDataSets; j++)
			{
				writeKey(_bw, invalidKeyValue);
			}
			int num = 0;
			if (arrayWriteBuf == null)
			{
				arrayWriteBuf = new byte[sizeOfDataSet];
			}
			int count = catalog.list.Count;
			for (int k = 0; k < count; k++)
			{
				if (!dirty.ContainsKey(catalog.list[k]) || !dirty[catalog.list[k]])
				{
					_bw.Seek(sizeOfDataSet, SeekOrigin.Current);
					continue;
				}
				DATA data = database[catalog.list[k]];
				copyToWrite(data, arrayWriteBuf);
				_bw.Write(arrayWriteBuf, 0, arrayWriteBuf.Length);
				num++;
			}
			dirty.Clear();
			Array.Clear(arrayWriteBuf, 0, arrayWriteBuf.Length);
			int num2 = (int)Mathf.Pow(2f, Mathf.Ceil(Mathf.Log(count) / Mathf.Log(2f)));
			int num3 = 16 + maxCountOfDataSets * sizeofKey + num2 * sizeOfDataSet;
			if (_bw.BaseStream.Length < num3)
			{
				for (int l = count; l < num2; l++)
				{
					_bw.Write(arrayWriteBuf, 0, arrayWriteBuf.Length);
				}
			}
			else if (_bw.BaseStream.Length > num3)
			{
				_bw.BaseStream.SetLength(num3);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract KEY readKey(BinaryReader _br);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void writeKey(BinaryWriter _bw, KEY _key);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void copyFromRead(byte[] _dataRead, DATA _data);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void copyToWrite(DATA _data, byte[] _dataWrite);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract DATA allocateDataStorage();
}
