using System;
using System.Collections.Generic;
using System.IO;

public class NameIdMapping : IMemoryPoolableObject, IDisposable
{
	public delegate int MissingEntryCallbackDelegate(string _entryName, int _sourceId);

	[PublicizedFrom(EAccessModifier.Private)]
	public const int FILE_VERSION = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, int> namesToIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] idsToNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string path;

	[PublicizedFrom(EAccessModifier.Private)]
	public string filename;

	public Dictionary<string, int>.Enumerator NamesToIdsIterator => namesToIds.GetEnumerator();

	public NameIdMapping()
	{
	}

	public NameIdMapping(string _filename, int _maxIds)
	{
		InitMapping(_filename, _maxIds);
	}

	public void InitMapping(string _filename, int _maxIds)
	{
		filename = _filename;
		path = Path.GetDirectoryName(_filename);
		if (namesToIds == null)
		{
			namesToIds = new Dictionary<string, int>(_maxIds);
		}
		if (idsToNames == null || idsToNames.Length < _maxIds)
		{
			idsToNames = new string[_maxIds];
		}
	}

	public void AddMapping(int _id, string _name, bool _force = false)
	{
		lock (this)
		{
			if (idsToNames[_id] == null || _force)
			{
				idsToNames[_id] = _name;
				namesToIds[_name] = _id;
				isDirty = true;
			}
		}
	}

	public int GetIdForName(string _name)
	{
		lock (this)
		{
			if (namesToIds.TryGetValue(_name, out var value))
			{
				return value;
			}
		}
		return -1;
	}

	public string GetNameForId(int _id)
	{
		lock (this)
		{
			return idsToNames[_id];
		}
	}

	public ArrayListMP<int> createIdTranslationTable(Func<string, int> _getDstId, MissingEntryCallbackDelegate _onMissingDestination = null)
	{
		ArrayListMP<int> arrayListMP = new ArrayListMP<int>(MemoryPools.poolInt, Block.MAX_BLOCKS);
		int[] items = arrayListMP.Items;
		for (int i = 0; i < items.Length; i++)
		{
			items[i] = -1;
		}
		for (int j = 0; j < idsToNames.Length; j++)
		{
			string text = idsToNames[j];
			if (text == null)
			{
				continue;
			}
			int num = _getDstId(text);
			if (num < 0)
			{
				if (_onMissingDestination == null)
				{
					Log.Error($"Creating id translation table from \"{filename}\" failed: Entry \"{text}\" ({j}) in source map is unknown.");
					return null;
				}
				num = _onMissingDestination(text, j);
				if (num < 0)
				{
					return null;
				}
			}
			items[j] = num;
		}
		return arrayListMP;
	}

	public int ReplaceNames(IEnumerable<(string oldName, string newName)> _replacementList)
	{
		int num = 0;
		lock (this)
		{
			foreach (var (key, text) in _replacementList)
			{
				if (namesToIds.TryGetValue(key, out var value))
				{
					idsToNames[value] = text;
					namesToIds.Remove(key);
					namesToIds[text] = value;
					num++;
				}
			}
			if (num > 0)
			{
				isDirty = true;
			}
		}
		return num;
	}

	public void SaveIfDirty(bool _async = true)
	{
		if (!isDirty)
		{
			return;
		}
		if (_async)
		{
			ThreadManager.AddSingleTask([PublicizedFrom(EAccessModifier.Private)] (ThreadManager.TaskInfo _info) =>
			{
				WriteToFile();
			});
		}
		else
		{
			WriteToFile();
		}
	}

	public void WriteToFile()
	{
		try
		{
			if (filename == null)
			{
				Log.Error("Can not save mapping, no filename specified");
				return;
			}
			if (!SdDirectory.Exists(path))
			{
				SdDirectory.CreateDirectory(path);
			}
			using Stream baseStream = SdFile.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Read);
			using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
			pooledBinaryWriter.SetBaseStream(baseStream);
			pooledBinaryWriter.BaseStream.Seek(0L, SeekOrigin.Begin);
			SaveToWriter(pooledBinaryWriter);
		}
		catch (Exception ex)
		{
			Log.Error("Could not save file '" + filename + "': " + ex.Message);
			Log.Exception(ex);
		}
	}

	public byte[] SaveToArray()
	{
		using PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
		{
			pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
			SaveToWriter(pooledBinaryWriter);
		}
		return pooledExpandableMemoryStream.ToArray();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveToWriter(BinaryWriter _writer)
	{
		_writer.Write(1);
		lock (this)
		{
			int num = 0;
			long position = _writer.BaseStream.Position;
			_writer.Write(num);
			for (int i = 0; i < idsToNames.Length; i++)
			{
				string text = idsToNames[i];
				if (text != null)
				{
					_writer.Write(i);
					_writer.Write(text);
					num++;
				}
			}
			_writer.BaseStream.Position = position;
			_writer.Write(num);
			_writer.BaseStream.Position = _writer.BaseStream.Length;
			isDirty = false;
		}
	}

	public bool LoadFromFile()
	{
		try
		{
			if (filename == null)
			{
				Log.Error("Can not load mapping, no filename specified");
				return false;
			}
			if (!SdFile.Exists(filename))
			{
				return false;
			}
			using (Stream baseStream = SdFile.OpenRead(filename))
			{
				using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
				pooledBinaryReader.SetBaseStream(baseStream);
				LoadFromReader(pooledBinaryReader);
			}
			return true;
		}
		catch (Exception ex)
		{
			Log.Error("Could not load file '" + filename + "': " + ex.Message);
			Log.Exception(ex);
			return false;
		}
	}

	public bool LoadFromArray(byte[] _data)
	{
		try
		{
			using (MemoryStream baseStream = new MemoryStream(_data))
			{
				using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
				pooledBinaryReader.SetBaseStream(baseStream);
				LoadFromReader(pooledBinaryReader);
			}
			return true;
		}
		catch (Exception ex)
		{
			Log.Error("Could not load mapping from array: " + ex.Message);
			Log.Exception(ex);
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LoadFromReader(BinaryReader _reader)
	{
		_reader.ReadInt32();
		lock (this)
		{
			Array.Clear(idsToNames, 0, idsToNames.Length);
			namesToIds.Clear();
			int num = _reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				int num2 = _reader.ReadInt32();
				string text = _reader.ReadString();
				idsToNames[num2] = text;
				namesToIds[text] = num2;
			}
			isDirty = false;
		}
	}

	public void Reset()
	{
		filename = null;
		path = null;
		if (idsToNames != null)
		{
			Array.Clear(idsToNames, 0, idsToNames.Length);
		}
		if (namesToIds != null)
		{
			namesToIds.Clear();
		}
		isDirty = false;
	}

	public void Cleanup()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	void IDisposable.Dispose()
	{
		MemoryPools.poolNameIdMapping.FreeSync(this);
	}
}
