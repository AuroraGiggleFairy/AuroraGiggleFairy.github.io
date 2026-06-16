using UnityEngine.Profiling;
using Utf8Json;

namespace Webserver.WebAPI.APIs.ServerState;

public abstract class KeyValueListAbs : AbsRestApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CustomSampler buildSampler;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] keyName = JsonWriter.GetEncodedPropertyNameWithBeginObject("name");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] keyType = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("type");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] keyValue = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("value");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] keyDefault = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("default");

	[PublicizedFrom(EAccessModifier.Private)]
	public int largestBuffer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public KeyValueListAbs(string _listName)
	{
		buildSampler = CustomSampler.Create("JSON_" + _listName + "_BuildSampler");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestGet(RequestContext _context)
	{
		AbsRestApi.PrepareEnvelopedResult(out var _writer);
		_writer.EnsureCapacity(largestBuffer);
		_writer.WriteBeginArray();
		bool _first = true;
		iterateList(ref _writer, ref _first);
		_writer.WriteEndArray();
		int num = _writer.CurrentOffset + 128;
		if (num > largestBuffer)
		{
			largestBuffer = num;
		}
		AbsRestApi.SendEnvelopedResult(_context, ref _writer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeKeyType(ref JsonWriter _writer, ref bool _first, string _key, string _type)
	{
		if (!_first)
		{
			_writer.WriteValueSeparator();
		}
		_first = false;
		_writer.WriteRaw(keyName);
		_writer.WriteString(_key);
		_writer.WriteRaw(keyType);
		_writer.WriteString(_type);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeInt(ref JsonWriter _writer, ref bool _first, string _key, int _value)
	{
		writeKeyType(ref _writer, ref _first, _key, "int");
		_writer.WriteRaw(keyValue);
		_writer.WriteInt32(_value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void addItem(ref JsonWriter _writer, ref bool _first, string _key, int _value)
	{
		writeInt(ref _writer, ref _first, _key, _value);
		_writer.WriteEndObject();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void addItem(ref JsonWriter _writer, ref bool _first, string _key, int _value, int? _default)
	{
		writeInt(ref _writer, ref _first, _key, _value);
		_writer.WriteRaw(keyDefault);
		if (_default.HasValue)
		{
			_writer.WriteInt32(_default.Value);
		}
		else
		{
			_writer.WriteNull();
		}
		_writer.WriteEndObject();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeFloat(ref JsonWriter _writer, ref bool _first, string _key, float _value)
	{
		writeKeyType(ref _writer, ref _first, _key, "float");
		_writer.WriteRaw(keyValue);
		_writer.WriteSingle(_value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void addItem(ref JsonWriter _writer, ref bool _first, string _key, float _value)
	{
		writeFloat(ref _writer, ref _first, _key, _value);
		_writer.WriteEndObject();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void addItem(ref JsonWriter _writer, ref bool _first, string _key, float _value, float? _default)
	{
		writeFloat(ref _writer, ref _first, _key, _value);
		_writer.WriteRaw(keyDefault);
		if (_default.HasValue)
		{
			_writer.WriteSingle(_default.Value);
		}
		else
		{
			_writer.WriteNull();
		}
		_writer.WriteEndObject();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeBool(ref JsonWriter _writer, ref bool _first, string _key, bool _value)
	{
		writeKeyType(ref _writer, ref _first, _key, "bool");
		_writer.WriteRaw(keyValue);
		_writer.WriteBoolean(_value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void addItem(ref JsonWriter _writer, ref bool _first, string _key, bool _value)
	{
		writeBool(ref _writer, ref _first, _key, _value);
		_writer.WriteEndObject();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void addItem(ref JsonWriter _writer, ref bool _first, string _key, bool _value, bool? _default)
	{
		writeBool(ref _writer, ref _first, _key, _value);
		_writer.WriteRaw(keyDefault);
		if (_default.HasValue)
		{
			_writer.WriteBoolean(_default.Value);
		}
		else
		{
			_writer.WriteNull();
		}
		_writer.WriteEndObject();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeString(ref JsonWriter _writer, ref bool _first, string _key, string _value)
	{
		writeKeyType(ref _writer, ref _first, _key, "string");
		_writer.WriteRaw(keyValue);
		_writer.WriteString(_value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void addItem(ref JsonWriter _writer, ref bool _first, string _key, string _value)
	{
		writeString(ref _writer, ref _first, _key, _value);
		_writer.WriteEndObject();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void addItem(ref JsonWriter _writer, ref bool _first, string _key, string _value, string _default)
	{
		writeString(ref _writer, ref _first, _key, _value);
		_writer.WriteRaw(keyDefault);
		_writer.WriteString(_default);
		_writer.WriteEndObject();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void iterateList(ref JsonWriter _writer, ref bool _first);
}
