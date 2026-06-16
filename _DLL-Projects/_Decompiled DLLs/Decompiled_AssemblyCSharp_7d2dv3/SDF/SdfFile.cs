using System;
using System.Collections.Generic;
using System.IO;

namespace SDF;

public class SdfFile
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SdfData data;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string filePath;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool valuesChanged;

	public SdfFile(string path)
	{
		data = new SdfData();
		filePath = path;
	}

	public void Load()
	{
		try
		{
			valuesChanged = false;
			if (!SdFile.Exists(filePath))
			{
				return;
			}
			using Stream fs = SdFile.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			data.Nodes = SdfReader.Read(fs);
		}
		catch (Exception ex)
		{
			Log.Error("Error opening SDF file: " + ex.Message);
		}
	}

	public void Save()
	{
		try
		{
			if (!valuesChanged)
			{
				return;
			}
			if (!SdDirectory.Exists(Path.GetDirectoryName(filePath)))
			{
				SdDirectory.CreateDirectory(Path.GetDirectoryName(filePath));
			}
			using Stream fs = SdFile.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
			SdfWriter.Write(fs, data.Nodes);
		}
		catch (Exception e)
		{
			Log.Error("Error opening SDF file:");
			Log.Exception(e);
		}
	}

	public void Set(string name, int val)
	{
		data.Add(new SdfInt(name, val));
		valuesChanged = true;
	}

	public void Set(string name, float val)
	{
		data.Add(new SdfFloat(name, val));
		valuesChanged = true;
	}

	public void Set(string name, string val)
	{
		Set(name, val, isBinary: false);
	}

	public void Set(string name, string val, bool isBinary)
	{
		if (!isBinary)
		{
			data.Add(new SdfString(name, val));
		}
		else
		{
			data.Add(new SdfBinary(name, val));
		}
		valuesChanged = true;
	}

	public void Set(string name, byte[] byteArray)
	{
		data.Add(new SdfByteArray(name, byteArray));
	}

	public void Set(string name, bool val)
	{
		data.Add(new SdfBool(name, val));
		valuesChanged = true;
	}

	public float? GetFloat(string name)
	{
		return data.GetFloat(name);
	}

	public int? GetInt(string name)
	{
		return data.GetInt(name);
	}

	public string GetString(string name)
	{
		return GetString(name, isBinary: false);
	}

	public string GetString(string name, bool isBinary)
	{
		if (!isBinary)
		{
			return data.GetString(name);
		}
		return Utils.FromBase64(data.GetString(name));
	}

	public byte[] GetByteArray(string name)
	{
		return data.GetByteArray(name);
	}

	public bool? GetBool(string name)
	{
		return data.GetBool(name);
	}

	public void Remove(string name)
	{
		data.Remove(name);
		valuesChanged = true;
	}

	public string[] GetKeys()
	{
		string[] array = new string[data.Nodes.Count];
		data.Nodes.CopyKeysTo(array);
		return array;
	}

	public string[] GetStoredGamePrefs()
	{
		string[] array = new string[data.Nodes.Count];
		data.Nodes.CopyKeysTo(array);
		return array;
	}

	public void CopyFrom(SdfFile other)
	{
		foreach (var (key, value) in other.data.Nodes)
		{
			data.Nodes[key] = value;
		}
		valuesChanged = true;
	}
}
