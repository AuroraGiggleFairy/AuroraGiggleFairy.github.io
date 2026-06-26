using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using UnityEngine;

public class XmlFile
{
	public readonly string Directory;

	public readonly string Filename;

	public XDocument XmlDoc;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XObject> tempXpathMatchList;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool Loaded
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public XmlFile(XmlFile _orig)
	{
		Directory = _orig.Directory;
		Filename = _orig.Filename;
		Loaded = _orig.Loaded;
		XmlDoc = new XDocument(_orig.XmlDoc);
	}

	public XmlFile(string _name)
	{
		Directory = GameIO.GetGameDir("Data/Config");
		Filename = ((!_name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)) ? (_name + ".xml") : _name);
		load(Directory, Filename);
	}

	public XmlFile(string _text, string _directory, string _filename, bool _throwExc = false)
	{
		Directory = _directory;
		Filename = _filename;
		toXml(_text, _filename, _throwExc);
	}

	public XmlFile(TextAsset _ta)
	{
		using MemoryStream stream = new MemoryStream(_ta.bytes);
		load(stream, _ta.name);
	}

	public XmlFile(byte[] _data, bool _throwExc = false)
	{
		using MemoryStream stream = new MemoryStream(_data);
		load(stream, null, _throwExc);
	}

	public XmlFile(string _directory, string _file, bool _loadAsync = false, bool _throwExc = false)
	{
		XmlFile xmlFile = this;
		Directory = _directory;
		Filename = ((!_file.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)) ? (_file + ".xml") : _file);
		if (!_loadAsync)
		{
			load(_directory, Filename);
			return;
		}
		ThreadManager.AddSingleTask([PublicizedFrom(EAccessModifier.Internal)] (ThreadManager.TaskInfo _) =>
		{
			xmlFile.load(_directory, xmlFile.Filename);
		});
	}

	public XmlFile(string _directory, string _file, Action<Exception> _doneCallback)
	{
		XmlFile xmlFile = this;
		Directory = _directory;
		Filename = ((!_file.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)) ? (_file + ".xml") : _file);
		ThreadManager.AddSingleTask([PublicizedFrom(EAccessModifier.Internal)] (ThreadManager.TaskInfo _) =>
		{
			try
			{
				xmlFile.load(_directory, xmlFile.Filename);
				_doneCallback(null);
			}
			catch (Exception obj)
			{
				_doneCallback(obj);
			}
		});
	}

	public XmlFile(Stream _stream)
	{
		load(_stream);
	}

	public string SerializeToString(bool _minified = false)
	{
		using StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		using (XmlWriter writer = XmlWriter.Create(stringWriter, GetWriterSettings(!_minified, Encoding.UTF8)))
		{
			XmlDoc.WriteTo(writer);
		}
		return stringWriter.ToString();
	}

	public byte[] SerializeToBytes(bool _minified = false, Encoding _encoding = null)
	{
		PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
		SerializeToStream(pooledExpandableMemoryStream, _minified, _encoding);
		byte[] result = pooledExpandableMemoryStream.ToArray();
		MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
		return result;
	}

	public void SerializeToFile(string _path, bool _minified = false, Encoding _encoding = null)
	{
		using Stream stream = SdFile.Create(_path);
		SerializeToStream(stream, _minified, _encoding);
	}

	public void SerializeToStream(Stream _stream, bool _minified = false, Encoding _encoding = null)
	{
		if (_encoding == null)
		{
			_encoding = Encoding.UTF8;
		}
		using XmlWriter writer = XmlWriter.Create(_stream, GetWriterSettings(!_minified, _encoding));
		XmlDoc.WriteTo(writer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XmlWriterSettings GetWriterSettings(bool _indent, Encoding _encoding)
	{
		return new XmlWriterSettings
		{
			Encoding = _encoding,
			Indent = _indent,
			OmitXmlDeclaration = true
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toXml(string _data, string _filename = null, bool _throwExc = false)
	{
		try
		{
			XmlDoc = XDocument.Parse(_data, LoadOptions.SetLineInfo);
			Loaded = true;
		}
		catch (Exception e)
		{
			if (_throwExc)
			{
				throw;
			}
			Log.Error("Failed parsing XML" + ((!string.IsNullOrEmpty(_filename)) ? (" (" + _filename + ")") : "") + ":");
			Log.Exception(e);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void load(byte[] _bytes, bool _throwExc = false)
	{
		using MemoryStream stream = new MemoryStream(_bytes);
		load(stream, null, _throwExc);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void load(string _directory, string _file, bool _throwExc = false)
	{
		if (_file == null)
		{
			SdFileInfo[] directory = GameIO.GetDirectory(_directory, "*.xml");
			if (directory.Length == 0)
			{
				return;
			}
			_file = directory[0].Name;
		}
		string text = _directory + "/" + _file;
		using Stream stream = SdFile.OpenRead(text);
		load(stream, text, _throwExc);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void load(Stream _stream, string _name = null, bool _throwExc = false)
	{
		try
		{
			using StreamReader textReader = new StreamReader(_stream, Encoding.UTF8);
			XmlDoc = XDocument.Load(textReader, LoadOptions.SetLineInfo);
			Loaded = true;
		}
		catch (Exception e)
		{
			if (_throwExc)
			{
				throw;
			}
			Log.Error("Failed parsing XML" + ((!string.IsNullOrEmpty(_name)) ? (" (" + _name + ")") : "") + ":");
			Log.Exception(e);
		}
	}

	public void RemoveComments()
	{
		XmlDoc.DescendantNodes().OfType<XComment>().Remove();
	}

	public bool GetXpathResults(string _xpath, out List<XObject> _matchList)
	{
		if (tempXpathMatchList == null)
		{
			tempXpathMatchList = new List<XObject>();
		}
		if (GetXpathResultsInList(_xpath, tempXpathMatchList))
		{
			_matchList = tempXpathMatchList;
			return true;
		}
		_matchList = null;
		return false;
	}

	public int ClearXpathResults()
	{
		int count = tempXpathMatchList.Count;
		tempXpathMatchList.Clear();
		return count;
	}

	public bool GetXpathResultsInList(string _xpath, List<XObject> _matchList)
	{
		if (_matchList == null)
		{
			throw new ArgumentNullException("_matchList", "GetXpathResultsInList can not be called with a null _matchList argument");
		}
		_matchList.Clear();
		if (!(XmlDoc.XPathEvaluate(_xpath) is IEnumerable source))
		{
			return false;
		}
		_matchList.AddRange(source.Cast<XObject>());
		if (_matchList.Count == 0)
		{
			return false;
		}
		return true;
	}
}
