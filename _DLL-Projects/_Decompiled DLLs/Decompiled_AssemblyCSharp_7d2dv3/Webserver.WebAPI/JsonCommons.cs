using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Utf8Json;

namespace Webserver.WebAPI;

public static class JsonCommons
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyPositionX = JsonWriter.GetEncodedPropertyNameWithBeginObject("x");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyPositionY = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("y");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyPositionZ = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("z");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyDays = JsonWriter.GetEncodedPropertyNameWithBeginObject("days");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyHours = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("hours");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyMinutes = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("minutes");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyCombinedString = JsonWriter.GetEncodedPropertyNameWithBeginObject("combinedString");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyPlatformId = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("platformId");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyUserId = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("userId");

	public static void WriteVector3I(ref JsonWriter _writer, Vector3i _position)
	{
		_writer.WriteRaw(jsonKeyPositionX);
		_writer.WriteInt32(_position.x);
		_writer.WriteRaw(jsonKeyPositionY);
		_writer.WriteInt32(_position.y);
		_writer.WriteRaw(jsonKeyPositionZ);
		_writer.WriteInt32(_position.z);
		_writer.WriteEndObject();
	}

	public static void WriteVector3(ref JsonWriter _writer, Vector3 _position)
	{
		_writer.WriteRaw(jsonKeyPositionX);
		_writer.WriteSingle(_position.x);
		_writer.WriteRaw(jsonKeyPositionY);
		_writer.WriteSingle(_position.y);
		_writer.WriteRaw(jsonKeyPositionZ);
		_writer.WriteSingle(_position.z);
		_writer.WriteEndObject();
	}

	public static void WriteVector2I(ref JsonWriter _writer, Vector2i _position)
	{
		_writer.WriteRaw(jsonKeyPositionX);
		_writer.WriteInt32(_position.x);
		_writer.WriteRaw(jsonKeyPositionY);
		_writer.WriteInt32(_position.y);
		_writer.WriteEndObject();
	}

	public static void WriteVector2(ref JsonWriter _writer, Vector2 _position)
	{
		_writer.WriteRaw(jsonKeyPositionX);
		_writer.WriteSingle(_position.x);
		_writer.WriteRaw(jsonKeyPositionY);
		_writer.WriteSingle(_position.y);
		_writer.WriteEndObject();
	}

	public static void WriteGameTimeObject(ref JsonWriter _writer, int _days, int _hours, int _minutes)
	{
		_writer.WriteRaw(jsonKeyDays);
		_writer.WriteInt32(_days);
		_writer.WriteRaw(jsonKeyHours);
		_writer.WriteInt32(_hours);
		_writer.WriteRaw(jsonKeyMinutes);
		_writer.WriteInt32(_minutes);
		_writer.WriteEndObject();
	}

	public static void WritePlatformUserIdentifier(ref JsonWriter _writer, PlatformUserIdentifierAbs _userIdentifier)
	{
		if (_userIdentifier == null)
		{
			_writer.WriteNull();
			return;
		}
		_writer.WriteRaw(jsonKeyCombinedString);
		_writer.WriteString(_userIdentifier.CombinedString);
		_writer.WriteRaw(jsonKeyPlatformId);
		_writer.WriteString(_userIdentifier.PlatformIdentifierString);
		_writer.WriteRaw(jsonKeyUserId);
		_writer.WriteString(_userIdentifier.ReadablePlatformUserIdentifier);
		_writer.WriteEndObject();
	}

	public static bool TryReadPlatformUserIdentifier(IDictionary<string, object> _jsonInput, out PlatformUserIdentifierAbs _userIdentifier)
	{
		if (TryGetJsonField(_jsonInput, "combinedString", out string _value))
		{
			_userIdentifier = PlatformUserIdentifierAbs.FromCombinedString(_value, _logErrors: false);
			if (_userIdentifier != null)
			{
				return true;
			}
		}
		if (!TryGetJsonField(_jsonInput, "platformId", out string _value2))
		{
			_userIdentifier = null;
			return false;
		}
		if (!TryGetJsonField(_jsonInput, "userId", out string _value3))
		{
			_userIdentifier = null;
			return false;
		}
		_userIdentifier = PlatformUserIdentifierAbs.FromPlatformAndId(_value2, _value3, _logErrors: false);
		return _userIdentifier != null;
	}

	public static void WriteDateTime(ref JsonWriter _writer, DateTime _dateTime)
	{
		_writer.WriteString(_dateTime.ToString("o"));
	}

	public static bool TryReadDateTime(IDictionary<string, object> _jsonInput, string _fieldName, out DateTime _result)
	{
		_result = default(DateTime);
		if (!TryGetJsonField(_jsonInput, _fieldName, out string _value))
		{
			return false;
		}
		return DateTime.TryParse(_value, null, DateTimeStyles.RoundtripKind, out _result);
	}

	public static bool TryGetJsonField(IDictionary<string, object> _jsonObject, string _fieldName, out int _value)
	{
		_value = 0;
		if (!_jsonObject.TryGetValue(_fieldName, out var value))
		{
			return false;
		}
		if (!(value is double num))
		{
			return false;
		}
		try
		{
			_value = (int)num;
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static bool TryGetJsonField(IDictionary<string, object> _jsonObject, string _fieldName, out double _value)
	{
		_value = 0.0;
		if (!_jsonObject.TryGetValue(_fieldName, out var value))
		{
			return false;
		}
		if (!(value is double num))
		{
			return false;
		}
		try
		{
			_value = num;
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static bool TryGetJsonField(IDictionary<string, object> _jsonObject, string _fieldName, out string _value)
	{
		_value = null;
		if (!_jsonObject.TryGetValue(_fieldName, out var value))
		{
			return false;
		}
		if (!(value is string text))
		{
			return false;
		}
		try
		{
			_value = text;
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static bool TryGetJsonField(IDictionary<string, object> _jsonObject, string _fieldName, out IDictionary<string, object> _value)
	{
		_value = null;
		if (!_jsonObject.TryGetValue(_fieldName, out var value))
		{
			return false;
		}
		if (!(value is IDictionary<string, object> dictionary))
		{
			return false;
		}
		try
		{
			_value = dictionary;
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
}
