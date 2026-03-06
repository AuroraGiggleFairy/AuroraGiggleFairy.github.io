using System;
using System.IO;
using System.Xml;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Serializable]
[DoNotTouchSerializableFlags]
[Preserve]
public abstract class PlatformUserIdentifierAbs : IEquatable<PlatformUserIdentifierAbs>
{
	public const byte UserIdentifierVersion = 1;

	public abstract EPlatformIdentifier PlatformIdentifier { get; }

	public abstract string PlatformIdentifierString { get; }

	public abstract string ReadablePlatformUserIdentifier { get; }

	public abstract string CombinedString { get; }

	public abstract bool DecodeTicket(string _ticket);

	[PublicizedFrom(EAccessModifier.ProtectedInternal)]
	public virtual int GetCustomDataLengthEstimate()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.ProtectedInternal)]
	public virtual void WriteCustomData(BinaryWriter _writer)
	{
	}

	[PublicizedFrom(EAccessModifier.ProtectedInternal)]
	public virtual void ReadCustomData(BinaryReader _reader)
	{
	}

	public static PlatformUserIdentifierAbs FromStream(Stream _sourceStream, bool _errorOnEmpty = false, bool _inclCustomData = false)
	{
		using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: true);
		pooledBinaryReader.SetBaseStream(_sourceStream);
		return FromStream(pooledBinaryReader, _errorOnEmpty);
	}

	public static PlatformUserIdentifierAbs FromStream(BinaryReader _sourceReader, bool _errorOnEmpty = false, bool _inclCustomData = false)
	{
		if (!_sourceReader.ReadBoolean())
		{
			if (_errorOnEmpty)
			{
				Log.Error("Empty user identifier string\nFrom: " + StackTraceUtility.ExtractStackTrace());
			}
			return null;
		}
		_sourceReader.ReadByte();
		string platformName = _sourceReader.ReadString();
		string userId = _sourceReader.ReadString();
		bool logErrors = GameManager.Instance;
		PlatformUserIdentifierAbs platformUserIdentifierAbs = FromPlatformAndId(platformName, userId, logErrors);
		if (_inclCustomData)
		{
			platformUserIdentifierAbs.ReadCustomData(_sourceReader);
		}
		return platformUserIdentifierAbs;
	}

	public static (string platformName, string userId)? FieldsFromStream(BinaryReader _sourceReader, bool _errorOnEmpty = false)
	{
		if (!_sourceReader.ReadBoolean())
		{
			if (_errorOnEmpty)
			{
				Log.Error("Empty user identifier string\nFrom: " + StackTraceUtility.ExtractStackTrace());
			}
			return null;
		}
		_sourceReader.ReadByte();
		string item = _sourceReader.ReadString();
		string item2 = _sourceReader.ReadString();
		return (item, item2);
	}

	public void ToXml(XmlElement _xmlElement, string _attributePrefix = "")
	{
		_xmlElement.SetAttrib(_attributePrefix + "platform", PlatformIdentifierString);
		_xmlElement.SetAttrib(_attributePrefix + "userid", ReadablePlatformUserIdentifier);
	}

	public static PlatformUserIdentifierAbs FromXml(XmlElement _xmlElement, bool _warnings = true, string _attributePrefix = null)
	{
		string text = "platform";
		string text2 = "userid";
		if (!string.IsNullOrEmpty(_attributePrefix))
		{
			text = _attributePrefix + text;
			text2 = _attributePrefix + text2;
		}
		if (_xmlElement.HasAttribute(text) && _xmlElement.HasAttribute(text2))
		{
			string attribute = _xmlElement.GetAttribute(text);
			string attribute2 = _xmlElement.GetAttribute(text2);
			return FromPlatformAndId(attribute, attribute2);
		}
		if (_warnings)
		{
			Log.Warning("Entry missing '" + text + "' or '" + text2 + "' attribute: " + _xmlElement.OuterXml);
		}
		return null;
	}

	public static bool TryFromCombinedString(string _combinedString, out PlatformUserIdentifierAbs _userIdentifier)
	{
		_userIdentifier = FromCombinedString(_combinedString, _logErrors: false);
		return _userIdentifier != null;
	}

	public static PlatformUserIdentifierAbs FromCombinedString(string _combinedString, bool _logErrors = true)
	{
		if (_combinedString == null)
		{
			if (_logErrors)
			{
				Log.Error("Empty user identifier string\nFrom: " + StackTraceUtility.ExtractStackTrace());
			}
			return null;
		}
		int num = _combinedString.IndexOf('_');
		if (num < 0)
		{
			if (_logErrors)
			{
				Log.Error("Missing separator '_' in string: " + _combinedString + "\nFrom: " + StackTraceUtility.ExtractStackTrace());
			}
			return null;
		}
		if (num == 0)
		{
			if (_logErrors)
			{
				Log.Error("Missing platform (before the separator '_') in string: " + _combinedString + "\nFrom: " + StackTraceUtility.ExtractStackTrace());
			}
			return null;
		}
		if (num + 1 >= _combinedString.Length)
		{
			if (_logErrors)
			{
				Log.Error("Missing user identifier (after the separator '_') in string: " + _combinedString + "\nFrom: " + StackTraceUtility.ExtractStackTrace());
			}
			return null;
		}
		string platformName = _combinedString.Substring(0, num);
		string userId = _combinedString.Substring(num + 1, _combinedString.Length - num - 1);
		return FromPlatformAndId(platformName, userId, _logErrors);
	}

	public static PlatformUserIdentifierAbs FromPlatformAndId(string _platformName, string _userId, bool _logErrors = true)
	{
		if (!PlatformManager.TryPlatformIdentifierFromString(_platformName, out var _platformIdentifier))
		{
			if (_logErrors)
			{
				Log.Error("Invalid platform name in user identifier: " + _platformName + "\nFrom: " + StackTraceUtility.ExtractStackTrace());
			}
			return null;
		}
		if (!PlatformManager.UserIdentifierFactories.TryGetValue(_platformIdentifier, out var value))
		{
			if (_logErrors)
			{
				throw new ArgumentOutOfRangeException("platformIdentifier", "Invalid platform " + _platformIdentifier);
			}
			return null;
		}
		PlatformUserIdentifierAbs result = null;
		try
		{
			result = value.FromId(_userId);
		}
		catch (Exception)
		{
			if (_logErrors)
			{
				throw;
			}
		}
		return result;
	}

	public abstract bool Equals(PlatformUserIdentifierAbs _other);

	public override bool Equals(object _obj)
	{
		if (_obj == null)
		{
			return false;
		}
		if (this == _obj)
		{
			return true;
		}
		if (!(_obj is PlatformUserIdentifierAbs other))
		{
			return false;
		}
		return Equals(other);
	}

	public static bool Equals(PlatformUserIdentifierAbs _a, PlatformUserIdentifierAbs _b)
	{
		if (_a == _b)
		{
			return true;
		}
		return _a?.Equals(_b) ?? false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return CombinedString;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public PlatformUserIdentifierAbs()
	{
	}
}
