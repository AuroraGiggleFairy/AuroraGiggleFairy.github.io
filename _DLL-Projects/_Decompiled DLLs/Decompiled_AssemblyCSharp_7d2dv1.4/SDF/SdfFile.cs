using System;
using System.IO;

namespace SDF;

public class SdfFile
{
	[PublicizedFrom(EAccessModifier.Private)]
	public SdfData data;

	[PublicizedFrom(EAccessModifier.Private)]
	public string filePath;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool valuesChanged;

	public void Open(string path)
	{
		try
		{
			data = new SdfData();
			filePath = path;
			valuesChanged = false;
			if (!SdDirectory.Exists(Path.GetDirectoryName(path)))
			{
				SdDirectory.CreateDirectory(Path.GetDirectoryName(path));
			}
			using Stream fs = SdFile.Open(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
			data.Nodes = SdfReader.Read(fs);
		}
		catch (Exception ex)
		{
			Log.Error("Error opening SDF file: " + ex.Message);
		}
	}

	public void Close()
	{
		try
		{
			if (!valuesChanged)
			{
				return;
			}
			using Stream fs = SdFile.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
			SdfWriter.Write(fs, data.Nodes);
		}
		catch (Exception e)
		{
			Log.Error("Error opening SDF file:");
			Log.Exception(e);
		}
	}

	public void SaveAndKeepOpen()
	{
		Close();
		Open(filePath);
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
}
