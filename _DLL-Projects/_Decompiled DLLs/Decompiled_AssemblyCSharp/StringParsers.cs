using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class StringParsers
{
	public struct SeparatorPositions(int _tmp)
	{
		public int TotalFound = 0;

		public int Sep1 = -1;

		public int Sep2 = -1;

		public int Sep3 = -1;

		public int Sep4 = -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EFloatParseState
	{
		SignOrIntegralDigit,
		IntegralDigit,
		DecimalDigit,
		ExponentialTest,
		ExponentialSignOrDigit,
		ExponentialDigit,
		TrailingWs
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const char NEGATIVE_SIGN = '-';

	[PublicizedFrom(EAccessModifier.Private)]
	public const char POSITIVE_SIGN = '+';

	[PublicizedFrom(EAccessModifier.Private)]
	public const char CURRENCY_SYMBOL = '¤';

	[PublicizedFrom(EAccessModifier.Private)]
	public const char DECIMAL_SEP = '.';

	[PublicizedFrom(EAccessModifier.Private)]
	public const char THOUSANDS_SEP = ',';

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string NAN_SYMBOL = "NaN";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string POSITIVE_INFINITY_SYMBOL = "Infinity";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string NEGATIVE_INFINITY_SYMBOL = "-Infinity";

	public static sbyte ParseSInt8(string _input, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Integer)
	{
		if (!internalParseInt64(_input, _style, _tryParse: false, out var _resultSigned, out var _, out var _exc, _signedResult: true, _startIndex, _endIndex))
		{
			throw _exc;
		}
		if (_resultSigned < -128 || _resultSigned > 127)
		{
			throw new OverflowException("Value too large or too small (input='" + _input + "')");
		}
		return (sbyte)_resultSigned;
	}

	public static byte ParseUInt8(string _input, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Integer)
	{
		if (!internalParseInt64(_input, _style, _tryParse: false, out var _, out var _resultUnsigned, out var _exc, _signedResult: false, _startIndex, _endIndex))
		{
			throw _exc;
		}
		if (_resultUnsigned > 255)
		{
			throw new OverflowException("Value too large or too small (input='" + _input + "')");
		}
		return (byte)_resultUnsigned;
	}

	public static bool TryParseSInt8(string _input, out sbyte _result, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Integer)
	{
		if (!internalParseInt64(_input, _style, _tryParse: true, out var _resultSigned, out var _, out var _, _signedResult: true, _startIndex, _endIndex))
		{
			_result = 0;
			return false;
		}
		if (_resultSigned < -128 || _resultSigned > 127)
		{
			_result = 0;
			return false;
		}
		_result = (sbyte)_resultSigned;
		return true;
	}

	public static bool TryParseUInt8(string _input, out byte _result, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Integer)
	{
		if (!internalParseInt64(_input, _style, _tryParse: true, out var _, out var _resultUnsigned, out var _, _signedResult: false, _startIndex, _endIndex))
		{
			_result = 0;
			return false;
		}
		if (_resultUnsigned > 255)
		{
			_result = 0;
			return false;
		}
		_result = (byte)_resultUnsigned;
		return true;
	}

	public static short ParseSInt16(string _input, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Integer)
	{
		if (!internalParseInt64(_input, _style, _tryParse: false, out var _resultSigned, out var _, out var _exc, _signedResult: true, _startIndex, _endIndex))
		{
			throw _exc;
		}
		if (_resultSigned < -32768 || _resultSigned > 32767)
		{
			throw new OverflowException("Value too large or too small (input='" + _input + "')");
		}
		return (short)_resultSigned;
	}

	public static ushort ParseUInt16(string _input, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Integer)
	{
		if (!internalParseInt64(_input, _style, _tryParse: false, out var _, out var _resultUnsigned, out var _exc, _signedResult: false, _startIndex, _endIndex))
		{
			throw _exc;
		}
		if (_resultUnsigned > 65535)
		{
			throw new OverflowException("Value too large or too small (input='" + _input + "')");
		}
		return (ushort)_resultUnsigned;
	}

	public static bool TryParseSInt16(string _input, out short _result, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Integer)
	{
		if (!internalParseInt64(_input, _style, _tryParse: true, out var _resultSigned, out var _, out var _, _signedResult: true, _startIndex, _endIndex))
		{
			_result = 0;
			return false;
		}
		if (_resultSigned < -32768 || _resultSigned > 32767)
		{
			_result = 0;
			return false;
		}
		_result = (short)_resultSigned;
		return true;
	}

	public static bool TryParseUInt16(string _input, out ushort _result, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Integer)
	{
		if (!internalParseInt64(_input, _style, _tryParse: true, out var _, out var _resultUnsigned, out var _, _signedResult: false, _startIndex, _endIndex))
		{
			_result = 0;
			return false;
		}
		if (_resultUnsigned > 65535)
		{
			_result = 0;
			return false;
		}
		_result = (ushort)_resultUnsigned;
		return true;
	}

	public static int ParseSInt32(string _input, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Integer)
	{
		if (!internalParseInt64(_input, _style, _tryParse: false, out var _resultSigned, out var _, out var _exc, _signedResult: true, _startIndex, _endIndex))
		{
			throw _exc;
		}
		if (_resultSigned < int.MinValue || _resultSigned > int.MaxValue)
		{
			throw new OverflowException("Value too large or too small (input='" + _input + "')");
		}
		return (int)_resultSigned;
	}

	public static uint ParseUInt32(string _input, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Integer)
	{
		if (!internalParseInt64(_input, _style, _tryParse: false, out var _, out var _resultUnsigned, out var _exc, _signedResult: false, _startIndex, _endIndex))
		{
			throw _exc;
		}
		if (_resultUnsigned > uint.MaxValue)
		{
			throw new OverflowException("Value too large or too small (input='" + _input + "')");
		}
		return (uint)_resultUnsigned;
	}

	public static bool TryParseSInt32(string _input, out int _result, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Integer)
	{
		if (!internalParseInt64(_input, _style, _tryParse: true, out var _resultSigned, out var _, out var _, _signedResult: true, _startIndex, _endIndex))
		{
			_result = 0;
			return false;
		}
		if (_resultSigned < int.MinValue || _resultSigned > int.MaxValue)
		{
			_result = 0;
			return false;
		}
		_result = (int)_resultSigned;
		return true;
	}

	public static bool TryParseUInt32(string _input, out uint _result, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Integer)
	{
		if (!internalParseInt64(_input, _style, _tryParse: true, out var _, out var _resultUnsigned, out var _, _signedResult: false, _startIndex, _endIndex))
		{
			_result = 0u;
			return false;
		}
		if (_resultUnsigned > uint.MaxValue)
		{
			_result = 0u;
			return false;
		}
		_result = (uint)_resultUnsigned;
		return true;
	}

	public static long ParseSInt64(string _input, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Integer)
	{
		if (!internalParseInt64(_input, _style, _tryParse: false, out var _resultSigned, out var _, out var _exc, _signedResult: true, _startIndex, _endIndex))
		{
			throw _exc;
		}
		return _resultSigned;
	}

	public static ulong ParseUInt64(string _input, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Integer)
	{
		if (!internalParseInt64(_input, _style, _tryParse: false, out var _, out var _resultUnsigned, out var _exc, _signedResult: false, _startIndex, _endIndex))
		{
			throw _exc;
		}
		return _resultUnsigned;
	}

	public static bool TryParseSInt64(string _input, out long _result, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Integer)
	{
		if (!internalParseInt64(_input, _style, _tryParse: true, out var _resultSigned, out var _, out var _, _signedResult: true, _startIndex, _endIndex))
		{
			_result = 0L;
			return false;
		}
		_result = _resultSigned;
		return true;
	}

	public static bool TryParseUInt64(string _input, out ulong _result, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Integer)
	{
		if (!internalParseInt64(_input, _style, _tryParse: true, out var _, out var _resultUnsigned, out var _, _signedResult: false, _startIndex, _endIndex))
		{
			_result = 0uL;
			return false;
		}
		_result = _resultUnsigned;
		return true;
	}

	public static float ParseFloat(string _input, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Any)
	{
		double num = ParseDouble(_input, _startIndex, _endIndex, _style);
		if (num - 3.4028234663852886E+38 > 3.6147112457961776E+29 && !double.IsPositiveInfinity(num))
		{
			throw new OverflowException();
		}
		return (float)num;
	}

	public static bool TryParseFloat(string _input, out float _result, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Any)
	{
		if (!internalParseDouble(_input, _style, _tryParse: true, out var _result2, out var _, _startIndex, _endIndex))
		{
			_result = 0f;
			return false;
		}
		if (_result2 - 3.4028234663852886E+38 > 3.6147112457961776E+29 && !double.IsPositiveInfinity(_result2))
		{
			_result = 0f;
			return false;
		}
		_result = (float)_result2;
		return true;
	}

	public static double ParseDouble(string _input, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Any)
	{
		if (!internalParseDouble(_input, _style, _tryParse: false, out var _result, out var _exception, _startIndex, _endIndex))
		{
			throw _exception;
		}
		return _result;
	}

	public static bool TryParseDouble(string _input, out double _result, int _startIndex = 0, int _endIndex = -1, NumberStyles _style = NumberStyles.Any)
	{
		if (!internalParseDouble(_input, _style, _tryParse: true, out _result, out var _, _startIndex, _endIndex))
		{
			_result = 0.0;
			return false;
		}
		return true;
	}

	public static DateTime ParseDateTime(string _s)
	{
		return DateTime.Parse(_s, Utils.StandardCulture);
	}

	public static bool TryParseDateTime(string _s, out DateTime _result)
	{
		return DateTime.TryParse(_s, Utils.StandardCulture, DateTimeStyles.None, out _result);
	}

	public static bool ParseBool(string _input, int _startIndex = 0, int _endIndex = -1, bool _ignoreCase = true)
	{
		if (!internalParseBool(_input, _tryParse: false, out var _result, out var _exception, _ignoreCase, _startIndex, _endIndex))
		{
			throw _exception;
		}
		return _result;
	}

	public static bool TryParseBool(string _input, out bool _result, int _startIndex = 0, int _endIndex = -1, bool _ignoreCase = true)
	{
		if (!internalParseBool(_input, _tryParse: true, out _result, out var _, _ignoreCase, _startIndex, _endIndex))
		{
			_result = false;
			return false;
		}
		return true;
	}

	public static bool TryParseRange(string _input, out FloatRange _result, float? _defaultMax = null)
	{
		_result = default(FloatRange);
		SeparatorPositions separatorPositions = GetSeparatorPositions(_input, ',', 1);
		if (separatorPositions.TotalFound == 0)
		{
			if (TryParseFloat(_input, out var _result2))
			{
				_result = new FloatRange(_result2, _defaultMax ?? _result2);
				return true;
			}
			return false;
		}
		if (TryParseFloat(_input, out var _result3, 0, separatorPositions.Sep1 - 1) && TryParseFloat(_input, out var _result4, separatorPositions.Sep1 + 1))
		{
			_result = new FloatRange(_result3, _result4);
			return true;
		}
		return false;
	}

	public static bool TryParseRange(string _input, out IntRange _result, int? _defaultMax = null)
	{
		_result = default(IntRange);
		SeparatorPositions separatorPositions = GetSeparatorPositions(_input, ',', 1);
		if (separatorPositions.TotalFound == 0)
		{
			if (TryParseSInt32(_input, out var _result2))
			{
				_result = new IntRange(_result2, _defaultMax ?? _result2);
				return true;
			}
			return false;
		}
		if (TryParseSInt32(_input, out var _result3, 0, separatorPositions.Sep1 - 1) && TryParseSInt32(_input, out var _result4, separatorPositions.Sep1 + 1))
		{
			_result = new IntRange(_result3, _result4);
			return true;
		}
		return false;
	}

	public static Vector2 ParseVector2(string _input)
	{
		SeparatorPositions separatorPositions = GetSeparatorPositions(_input, ',', 1);
		if (separatorPositions.TotalFound != 1)
		{
			return Vector2.zero;
		}
		return new Vector2(ParseFloat(_input, 0, separatorPositions.Sep1 - 1), ParseFloat(_input, separatorPositions.Sep1 + 1));
	}

	public static Vector2 ParseVector2(string _input, float _defaultValue)
	{
		SeparatorPositions separatorPositions = GetSeparatorPositions(_input, ',', 1);
		if (separatorPositions.TotalFound == 0)
		{
			return new Vector2(ParseFloat(_input), _defaultValue);
		}
		return new Vector2(ParseFloat(_input, 0, separatorPositions.Sep1 - 1), ParseFloat(_input, separatorPositions.Sep1 + 1));
	}

	public static Vector3 ParseVector3(string _input, int _startIndex = 0, int _endIndex = -1)
	{
		if (_startIndex == 0 && _endIndex < 0 && _input.Length > 0 && _input[0] == '(' && _input[_input.Length - 1] == ')')
		{
			_startIndex = 1;
			_endIndex = _input.Length - 2;
		}
		if (_endIndex < 0)
		{
			_endIndex = _input.Length - 1;
		}
		if (_endIndex < _startIndex || _endIndex >= _input.Length)
		{
			throw new ArgumentException("_endIndex out of range (input='" + _input + "')", "_endIndex");
		}
		SeparatorPositions separatorPositions = GetSeparatorPositions(_input, ',', 2, _startIndex, _endIndex);
		if (separatorPositions.TotalFound != 2)
		{
			return Vector3.zero;
		}
		return new Vector3(ParseFloat(_input, _startIndex, separatorPositions.Sep1 - 1), ParseFloat(_input, separatorPositions.Sep1 + 1, separatorPositions.Sep2 - 1), ParseFloat(_input, separatorPositions.Sep2 + 1, _endIndex));
	}

	public static Vector3 ParseVector3(string _input, float _defaultValue)
	{
		SeparatorPositions separatorPositions = GetSeparatorPositions(_input, ',', 1);
		if (separatorPositions.TotalFound == 0)
		{
			return new Vector3(ParseFloat(_input), _defaultValue, _defaultValue);
		}
		if (separatorPositions.TotalFound == 1)
		{
			return new Vector3(ParseFloat(_input, 0, separatorPositions.Sep1 - 1), ParseFloat(_input, separatorPositions.Sep1 + 1), _defaultValue);
		}
		return new Vector3(ParseFloat(_input, 0, separatorPositions.Sep1 - 1), ParseFloat(_input, separatorPositions.Sep1 + 1, separatorPositions.Sep2 - 1), ParseFloat(_input, separatorPositions.Sep2 + 1));
	}

	public static Vector4 ParseVector4(string _input)
	{
		SeparatorPositions separatorPositions = GetSeparatorPositions(_input, ',', 3);
		if (separatorPositions.TotalFound != 3)
		{
			return Vector4.zero;
		}
		return new Vector4(ParseFloat(_input, 0, separatorPositions.Sep1 - 1), ParseFloat(_input, separatorPositions.Sep1 + 1, separatorPositions.Sep2 - 1), ParseFloat(_input, separatorPositions.Sep2 + 1, separatorPositions.Sep3 - 1), ParseFloat(_input, separatorPositions.Sep3 + 1));
	}

	public static BlockFaceFlag ParseWaterFlowMask(string _input)
	{
		if (_input.EqualsCaseInsensitive("permitted"))
		{
			return BlockFaceFlag.None;
		}
		if (_input.Contains(','))
		{
			string[] array = _input.Split(',');
			BlockFaceFlag blockFaceFlag = BlockFaceFlag.All;
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				if (char.TryParse(array2[i], out var result))
				{
					blockFaceFlag &= ~BlockFaceFlags.FromBlockFace(BlockFaces.CharToFace(result));
				}
			}
			return blockFaceFlag;
		}
		return BlockFaceFlag.All;
	}

	public static Plane ParsePlane(string _input)
	{
		SeparatorPositions separatorPositions = GetSeparatorPositions(_input, ',', 3);
		if (separatorPositions.TotalFound != 3)
		{
			return default(Plane);
		}
		Vector3 inNormal = new Vector3(ParseFloat(_input, 0, separatorPositions.Sep1 - 1), ParseFloat(_input, separatorPositions.Sep1 + 1, separatorPositions.Sep2 - 1), ParseFloat(_input, separatorPositions.Sep2 + 1, separatorPositions.Sep3 - 1));
		float d = ParseFloat(_input, separatorPositions.Sep3 + 1);
		return new Plane(inNormal, d);
	}

	public static Quaternion ParseQuaternion(string _input)
	{
		SeparatorPositions separatorPositions = GetSeparatorPositions(_input, ',', 3);
		if (separatorPositions.TotalFound != 3)
		{
			return Quaternion.identity;
		}
		return new Quaternion(ParseFloat(_input, 0, separatorPositions.Sep1 - 1), ParseFloat(_input, separatorPositions.Sep1 + 1, separatorPositions.Sep2 - 1), ParseFloat(_input, separatorPositions.Sep2 + 1, separatorPositions.Sep3 - 1), ParseFloat(_input, separatorPositions.Sep3 + 1));
	}

	public static Color ParseColor(string _input)
	{
		SeparatorPositions separatorPositions = GetSeparatorPositions(_input, ',', 3);
		if (separatorPositions.TotalFound < 2 || separatorPositions.TotalFound > 3)
		{
			return Color.white;
		}
		float r = ParseFloat(_input, 0, separatorPositions.Sep1 - 1);
		float g = ParseFloat(_input, separatorPositions.Sep1 + 1, separatorPositions.Sep2 - 1);
		float b = ParseFloat(_input, separatorPositions.Sep2 + 1, separatorPositions.Sep3 - 1);
		if (separatorPositions.TotalFound == 2)
		{
			return new Color(r, g, b);
		}
		float a = ParseFloat(_input, separatorPositions.Sep3 + 1);
		return new Color(r, g, b, a);
	}

	public static Color ParseHexColor(string _input)
	{
		if (_input == null)
		{
			return Color.clear;
		}
		if (_input.IndexOf(',') >= 0)
		{
			return ParseColor32(_input);
		}
		if (_input.Length < 6)
		{
			return Color.clear;
		}
		int num = 0;
		if (_input[0] == '#')
		{
			num = 1;
		}
		byte r = ParseUInt8(_input, num, num + 1, NumberStyles.HexNumber);
		byte g = ParseUInt8(_input, num + 2, num + 3, NumberStyles.HexNumber);
		byte b = ParseUInt8(_input, num + 4, num + 5, NumberStyles.HexNumber);
		return new Color32(r, g, b, byte.MaxValue);
	}

	public static Color ParseColor32(string _input)
	{
		SeparatorPositions separatorPositions = GetSeparatorPositions(_input, ',', 3);
		if (separatorPositions.TotalFound < 2)
		{
			return Color.white;
		}
		float num = ParseSInt32(_input, 0, separatorPositions.Sep1 - 1);
		float num2 = ParseSInt32(_input, separatorPositions.Sep1 + 1, separatorPositions.Sep2 - 1);
		float num3 = ParseSInt32(_input, separatorPositions.Sep2 + 1, separatorPositions.Sep3 - 1);
		float num4 = 255f;
		if (separatorPositions.TotalFound > 2)
		{
			num4 = ParseSInt32(_input, separatorPositions.Sep3 + 1);
		}
		return new Color(num / 255f, num2 / 255f, num3 / 255f, num4 / 255f);
	}

	public static Bounds ParseBounds(string _input)
	{
		int num = _input.IndexOf('(');
		if (num < 0)
		{
			throw new FormatException("Bounds input string is not in the correct format. Expected \"([Center X], [Center Y], [Center Z]), ([Size X], [Size Y], [Size Z])\" - e.g. \"(-0.5,0.5,-0.5),(2,2,6)\" (input='" + _input + "')");
		}
		int num2 = _input.IndexOf(')', num + 1);
		if (num2 < 0)
		{
			throw new FormatException("Bounds input string is not in the correct format. Expected \"([Center X], [Center Y], [Center Z]), ([Size X], [Size Y], [Size Z])\" - e.g. \"(-0.5,0.5,-0.5),(2,2,6)\" (input='" + _input + "')");
		}
		int num3 = _input.IndexOf('(', num2 + 1);
		if (num3 < 0)
		{
			throw new FormatException("Bounds input string is not in the correct format. Expected \"([Center X], [Center Y], [Center Z]), ([Size X], [Size Y], [Size Z])\" - e.g. \"(-0.5,0.5,-0.5),(2,2,6)\" (input='" + _input + "')");
		}
		int num4 = _input.IndexOf(')', num3 + 1);
		if (num4 < 0)
		{
			throw new FormatException("Bounds input string is not in the correct format. Expected \"([Center X], [Center Y], [Center Z]), ([Size X], [Size Y], [Size Z])\" - e.g. \"(-0.5,0.5,-0.5),(2,2,6)\" (input='" + _input + "')");
		}
		Vector3 center = ParseVector3(_input, num + 1, num2 - 1);
		Vector3 size = ParseVector3(_input, num3 + 1, num4 - 1);
		return new Bounds(center, size);
	}

	public static Vector2d ParseVector2d(string _input)
	{
		SeparatorPositions separatorPositions = GetSeparatorPositions(_input, ',', 1);
		if (separatorPositions.TotalFound != 1)
		{
			return Vector2d.Zero;
		}
		return new Vector2d(ParseDouble(_input, 0, separatorPositions.Sep1 - 1), ParseDouble(_input, separatorPositions.Sep1 + 1));
	}

	public static Vector3d ParseVector3d(string _input)
	{
		SeparatorPositions separatorPositions = GetSeparatorPositions(_input, ',', 2);
		if (separatorPositions.TotalFound != 2)
		{
			return Vector3d.Zero;
		}
		return new Vector3d(ParseDouble(_input, 0, separatorPositions.Sep1 - 1), ParseDouble(_input, separatorPositions.Sep1 + 1, separatorPositions.Sep2 - 1), ParseDouble(_input, separatorPositions.Sep2 + 1));
	}

	public static Vector2i ParseVector2i(string _input, char _customSep = ',')
	{
		SeparatorPositions separatorPositions = GetSeparatorPositions(_input, _customSep, 1);
		if (separatorPositions.TotalFound != 1)
		{
			return Vector2i.zero;
		}
		return new Vector2i(ParseSInt32(_input, 0, separatorPositions.Sep1 - 1), ParseSInt32(_input, separatorPositions.Sep1 + 1));
	}

	public static Vector3i ParseVector3i(string _input, int _startIndex = 0, int _endIndex = -1, bool _errorOnFailure = false)
	{
		if (_endIndex < 0)
		{
			_endIndex = _input.Length - 1;
		}
		if (_endIndex < _startIndex || _endIndex >= _input.Length)
		{
			throw new ArgumentException("_endIndex out of range (input='" + _input + "')", "_endIndex");
		}
		SeparatorPositions separatorPositions = GetSeparatorPositions(_input, ',', 2, _startIndex, _endIndex);
		if (separatorPositions.TotalFound != 2)
		{
			if (_errorOnFailure)
			{
				throw new FormatException("_input in invalid format (input='" + _input + "')");
			}
			return Vector3i.zero;
		}
		return new Vector3i(ParseSInt32(_input, _startIndex, separatorPositions.Sep1 - 1), ParseSInt32(_input, separatorPositions.Sep1 + 1, separatorPositions.Sep2 - 1), ParseSInt32(_input, separatorPositions.Sep2 + 1, _endIndex));
	}

	public static List<T> ParseList<T>(string _input, char _separator, Func<string, int, int, T> _parserFunc)
	{
		List<T> list = new List<T>();
		int num = -1;
		for (int num2 = _input.IndexOf(_separator, 0); num2 >= 0; num2 = _input.IndexOf(_separator, num + 1))
		{
			list.Add(_parserFunc(_input, num + 1, num2 - 1));
			num = num2;
		}
		if (num + 1 < _input.Length)
		{
			list.Add(_parserFunc(_input, num + 1, -1));
		}
		return list;
	}

	public static void ParseMinMaxCount(string _input, out int _minCount, out int _maxCount)
	{
		SeparatorPositions separatorPositions = GetSeparatorPositions(_input, ',', 1);
		if (separatorPositions.TotalFound > 1)
		{
			throw new Exception("Parsing error count (input='" + _input + "')");
		}
		if (separatorPositions.TotalFound == 0)
		{
			if (!TryParseSInt32(_input, out _minCount))
			{
				throw new Exception("Parsing error count (input='" + _input + "')");
			}
			_maxCount = _minCount;
		}
		if (!TryParseSInt32(_input, out _minCount, 0, separatorPositions.Sep1 - 1) || !TryParseSInt32(_input, out _maxCount, separatorPositions.Sep1 + 1))
		{
			throw new Exception("Parsing error count (input='" + _input + "')");
		}
	}

	public static void ParseMinMaxCount(string _input, out float _minCount, out float _maxCount)
	{
		SeparatorPositions separatorPositions = GetSeparatorPositions(_input, ',', 1);
		if (separatorPositions.TotalFound > 1)
		{
			throw new Exception("Parsing error count (input='" + _input + "')");
		}
		if (separatorPositions.TotalFound == 0)
		{
			if (!TryParseFloat(_input, out _minCount))
			{
				throw new Exception("Parsing error count (input='" + _input + "')");
			}
			_maxCount = _minCount;
		}
		if (!TryParseFloat(_input, out _minCount, 0, separatorPositions.Sep1 - 1) || !TryParseFloat(_input, out _maxCount, separatorPositions.Sep1 + 1))
		{
			throw new Exception("Parsing error count (input='" + _input + "')");
		}
	}

	public static Vector2 ParseMinMaxCount(string _input)
	{
		SeparatorPositions separatorPositions = GetSeparatorPositions(_input, ',', 1);
		if (separatorPositions.TotalFound != 1)
		{
			return Vector2.zero;
		}
		float num = ParseFloat(_input, 0, separatorPositions.Sep1 - 1);
		float num2 = ParseFloat(_input, separatorPositions.Sep1 + 1);
		if (num != num2)
		{
			return new Vector2(Mathf.Min(num, num2), Mathf.Max(num, num2));
		}
		return new Vector2(num, num2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool findOther(ref int _pos, string _input, char _other)
	{
		if (_input[_pos] == _other)
		{
			_pos++;
			return true;
		}
		return false;
	}

	public static SeparatorPositions GetSeparatorPositions(string _input, char _separator, int _expected, int _startIndex = 0, int _endIndex = -1)
	{
		SeparatorPositions result = new SeparatorPositions(0);
		if (_expected <= 0)
		{
			throw new ArgumentException("_expected has to be greater than 0");
		}
		if (_input == null)
		{
			throw new ArgumentNullException("_input");
		}
		if (_input.Length == 0)
		{
			return result;
		}
		if (_endIndex < 0)
		{
			_endIndex = _input.Length - 1;
		}
		if (_endIndex < _startIndex || _endIndex >= _input.Length)
		{
			throw new ArgumentException("_endIndex out of range (input='" + _input + "')", "_endIndex");
		}
		result.Sep1 = _input.IndexOf(_separator, _startIndex, _endIndex - _startIndex + 1);
		if (result.Sep1 < 0)
		{
			return result;
		}
		result.TotalFound++;
		result.Sep2 = _input.IndexOf(_separator, result.Sep1 + 1, _endIndex - result.Sep1);
		if (result.Sep2 < 0)
		{
			return result;
		}
		result.TotalFound++;
		if (_expected == 1)
		{
			return result;
		}
		result.Sep3 = _input.IndexOf(_separator, result.Sep2 + 1, _endIndex - result.Sep2);
		if (result.Sep3 < 0)
		{
			return result;
		}
		result.TotalFound++;
		if (_expected == 2)
		{
			return result;
		}
		result.Sep4 = _input.IndexOf(_separator, result.Sep3 + 1, _endIndex - result.Sep3);
		if (result.Sep4 < 0)
		{
			return result;
		}
		result.TotalFound++;
		if (_expected == 3)
		{
			return result;
		}
		if (_input.IndexOf(_separator, result.Sep4 + 1, _endIndex - result.Sep4) < 0)
		{
			return result;
		}
		result.TotalFound++;
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool internalParseDouble(string _input, NumberStyles _numberStyle, bool _tryParse, out double _result, out Exception _exception, int _startIndex, int _endIndex)
	{
		_result = 0.0;
		_exception = null;
		if (_input == null)
		{
			if (!_tryParse)
			{
				_exception = new ArgumentNullException("_input");
			}
			return false;
		}
		if (_input.Length == 0)
		{
			if (!_tryParse)
			{
				_exception = new FormatException("Empty input string");
			}
			return false;
		}
		if (_startIndex < 0 || _startIndex >= _input.Length)
		{
			if (!_tryParse)
			{
				_exception = new ArgumentException($"_startIndex ({_startIndex}) out of range (input='{_input}')", "_startIndex");
			}
			return false;
		}
		if (_endIndex < 0)
		{
			_endIndex = _input.Length - 1;
		}
		if (_endIndex >= _input.Length)
		{
			if (!_tryParse)
			{
				_exception = new ArgumentException($"_endIndex ({_endIndex}) out of range (input='{_input}')", "_endIndex");
			}
			return false;
		}
		if ((_numberStyle & NumberStyles.AllowHexSpecifier) != NumberStyles.None)
		{
			throw new ArgumentException("Double doesn't support parsing with 'AllowHexSpecifier' (input='" + _input + "')");
		}
		if (_numberStyle > NumberStyles.Any)
		{
			if (!_tryParse)
			{
				_exception = new ArgumentException();
			}
			return false;
		}
		bool num = (_numberStyle & NumberStyles.AllowLeadingWhite) != 0;
		bool flag = (_numberStyle & NumberStyles.AllowTrailingWhite) != 0;
		bool flag2 = (_numberStyle & NumberStyles.AllowLeadingSign) != 0;
		bool flag3 = (_numberStyle & NumberStyles.AllowExponent) != 0;
		bool flag4 = (_numberStyle & NumberStyles.AllowDecimalPoint) != 0;
		bool flag5 = (_numberStyle & NumberStyles.AllowThousands) != 0;
		bool flag6 = (_numberStyle & NumberStyles.AllowCurrencySymbol) != 0;
		int i = _startIndex;
		if (num)
		{
			for (; i <= _endIndex && char.IsWhiteSpace(_input[i]); i++)
			{
			}
			if (i > _endIndex)
			{
				if (!_tryParse)
				{
					_exception = new FormatException("Input string was not in the correct format (input='" + _input + "')");
				}
				return false;
			}
		}
		if (flag)
		{
			while (_endIndex >= 0 && char.IsWhiteSpace(_input[_endIndex]))
			{
				_endIndex--;
			}
		}
		if (i > _endIndex || _endIndex < 0)
		{
			if (!_tryParse)
			{
				_exception = new FormatException("Input string was not in the correct format (input='" + _input + "')");
			}
			return false;
		}
		if (_endIndex - _startIndex + 1 == NAN_SYMBOL.Length && string.Compare(NAN_SYMBOL, 0, _input, _startIndex, NAN_SYMBOL.Length, StringComparison.Ordinal) == 0)
		{
			_result = double.NaN;
			return true;
		}
		if (_endIndex - _startIndex + 1 == POSITIVE_INFINITY_SYMBOL.Length && string.Compare(POSITIVE_INFINITY_SYMBOL, 0, _input, _startIndex, POSITIVE_INFINITY_SYMBOL.Length, StringComparison.Ordinal) == 0)
		{
			_result = double.PositiveInfinity;
			return true;
		}
		if (_endIndex - _startIndex + 1 == NEGATIVE_INFINITY_SYMBOL.Length && string.Compare(NEGATIVE_INFINITY_SYMBOL, 0, _input, _startIndex, NEGATIVE_INFINITY_SYMBOL.Length, StringComparison.Ordinal) == 0)
		{
			_result = double.NegativeInfinity;
			return true;
		}
		double num2 = 0.0;
		bool flag7 = false;
		bool flag8 = false;
		int num3 = 0;
		int num4 = 0;
		EFloatParseState eFloatParseState = EFloatParseState.SignOrIntegralDigit;
		for (; i <= _endIndex; i++)
		{
			char c = _input[i];
			int num5 = c ^ 0x30;
			bool flag9 = num5 <= 9;
			if (c == '\0')
			{
				break;
			}
			switch (eFloatParseState)
			{
			case EFloatParseState.SignOrIntegralDigit:
				if (flag2 && c == '+')
				{
					eFloatParseState = EFloatParseState.IntegralDigit;
				}
				else if (flag2 && c == '-')
				{
					eFloatParseState = EFloatParseState.IntegralDigit;
					flag7 = true;
				}
				else
				{
					eFloatParseState = EFloatParseState.IntegralDigit;
					i--;
				}
				break;
			case EFloatParseState.IntegralDigit:
				if (flag9)
				{
					num2 = num2 * 10.0 + (double)num5;
				}
				else if (c == 'e' || c == 'E')
				{
					eFloatParseState = EFloatParseState.ExponentialTest;
					i--;
				}
				else if (flag4 && c == '.')
				{
					eFloatParseState = EFloatParseState.DecimalDigit;
				}
				else
				{
					if ((flag5 && c == ',') || (flag6 && c == '¤'))
					{
						break;
					}
					if (!char.IsWhiteSpace(c))
					{
						if (!_tryParse)
						{
							_exception = new FormatException($"Unknown char: {c} (input: '{_input}')");
						}
						return false;
					}
					eFloatParseState = EFloatParseState.TrailingWs;
					i--;
				}
				break;
			case EFloatParseState.DecimalDigit:
				if (flag9)
				{
					num2 = num2 * 10.0 + (double)num5;
					num3++;
					break;
				}
				if (c == 'e' || c == 'E')
				{
					eFloatParseState = EFloatParseState.ExponentialTest;
					i--;
					break;
				}
				if (char.IsWhiteSpace(c))
				{
					eFloatParseState = EFloatParseState.TrailingWs;
					i--;
					break;
				}
				if (!_tryParse)
				{
					_exception = new FormatException($"Unknown char: {c} (input: '{_input}')");
				}
				return false;
			case EFloatParseState.ExponentialTest:
				if (!flag3)
				{
					if (!_tryParse)
					{
						_exception = new FormatException($"Unknown char: {c} (input: '{_input}')");
					}
					return false;
				}
				eFloatParseState = EFloatParseState.ExponentialSignOrDigit;
				break;
			case EFloatParseState.ExponentialSignOrDigit:
				if (flag9)
				{
					eFloatParseState = EFloatParseState.ExponentialDigit;
					i--;
					break;
				}
				switch (c)
				{
				case '+':
					eFloatParseState = EFloatParseState.ExponentialDigit;
					break;
				case '-':
					flag8 = true;
					eFloatParseState = EFloatParseState.ExponentialDigit;
					break;
				default:
					if (char.IsWhiteSpace(c))
					{
						eFloatParseState = EFloatParseState.TrailingWs;
						i--;
						break;
					}
					if (!_tryParse)
					{
						_exception = new FormatException($"Unknown char: {c} (input: '{_input}')");
					}
					return false;
				}
				break;
			case EFloatParseState.ExponentialDigit:
				if (flag9)
				{
					num4 = num4 * 10 + num5;
					break;
				}
				if (char.IsWhiteSpace(c))
				{
					eFloatParseState = EFloatParseState.TrailingWs;
					i--;
					break;
				}
				if (!_tryParse)
				{
					_exception = new FormatException($"Unknown char: {c} (input: '{_input}')");
				}
				return false;
			case EFloatParseState.TrailingWs:
				if (!flag || !char.IsWhiteSpace(c))
				{
					if (!_tryParse)
					{
						_exception = new FormatException($"Unknown char: {c} (input: '{_input}')");
					}
					return false;
				}
				break;
			}
			if (eFloatParseState > EFloatParseState.TrailingWs)
			{
				break;
			}
		}
		if (num2 != 0.0)
		{
			if (flag7)
			{
				num2 *= -1.0;
			}
			if (flag8)
			{
				num4 *= -1;
			}
			num4 -= num3;
			if (num4 < 0)
			{
				flag8 = true;
				num4 *= -1;
			}
			double num6 = 1.0;
			double num7 = 10.0;
			while (num4 > 0)
			{
				if (num4 % 2 == 1)
				{
					num6 *= num7;
				}
				num4 >>= 1;
				num7 *= num7;
			}
			num2 = ((!flag8) ? (num2 * num6) : (num2 / num6));
		}
		if (double.IsPositiveInfinity(num2) || double.IsNegativeInfinity(num2))
		{
			if (!_tryParse)
			{
				_exception = new OverflowException();
			}
			return false;
		}
		_result = num2;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool internalParseBool(string _input, bool _tryParse, out bool _result, out Exception _exception, bool _ignoreCase, int _startIndex, int _endIndex)
	{
		_result = false;
		_exception = null;
		if (_input == null)
		{
			if (!_tryParse)
			{
				_exception = new ArgumentNullException("_input");
			}
			return false;
		}
		if (_input.Length == 0)
		{
			if (!_tryParse)
			{
				_exception = new FormatException("Value is not equivalent to either TrueString or FalseString (input='" + _input + "')");
			}
			return false;
		}
		if (_startIndex < 0 || _startIndex >= _input.Length)
		{
			if (!_tryParse)
			{
				_exception = new ArgumentException("_startIndex out of range (input='" + _input + "')", "_startIndex");
			}
			return false;
		}
		if (_endIndex >= _input.Length)
		{
			if (!_tryParse)
			{
				_exception = new ArgumentException("_endIndex out of range (input='" + _input + "')", "_endIndex");
			}
			return false;
		}
		if (_endIndex < 0)
		{
			_endIndex = _input.Length - 1;
		}
		while (_startIndex <= _endIndex && char.IsWhiteSpace(_input[_startIndex]))
		{
			_startIndex++;
		}
		if (_startIndex > _endIndex)
		{
			if (!_tryParse)
			{
				_exception = new FormatException("Input string was not in the correct format (input='" + _input + "')");
			}
			return false;
		}
		while (_endIndex >= 0 && char.IsWhiteSpace(_input[_endIndex]))
		{
			_endIndex--;
		}
		if (_startIndex > _endIndex || _endIndex < 0)
		{
			if (!_tryParse)
			{
				_exception = new FormatException("Input string was not in the correct format (input='" + _input + "')");
			}
			return false;
		}
		StringComparison comparisonType = (_ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
		if (string.Compare(_input, _startIndex, bool.TrueString, 0, _endIndex - _startIndex + 1, comparisonType) == 0)
		{
			_result = true;
			return true;
		}
		if (string.Compare(_input, _startIndex, bool.FalseString, 0, _endIndex - _startIndex + 1, comparisonType) == 0)
		{
			_result = false;
			return true;
		}
		if (!_tryParse)
		{
			_exception = new FormatException("Value is not equivalent to either TrueString or FalseString (input='" + _input + "')");
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool internalParseInt64(string _input, NumberStyles _numberStyle, bool _tryParse, out long _resultSigned, out ulong _resultUnsigned, out Exception _exc, bool _signedResult, int _startIndex, int _endIndex)
	{
		if (_numberStyle != NumberStyles.Integer)
		{
			return internalParseInt64Advanced(_input, _numberStyle, _tryParse, out _resultSigned, out _resultUnsigned, out _exc, _signedResult, _startIndex, _endIndex);
		}
		_resultSigned = 0L;
		_resultUnsigned = 0uL;
		_exc = null;
		if (_input == null)
		{
			if (!_tryParse)
			{
				_exc = new ArgumentNullException("_input");
			}
			return false;
		}
		if (_input.Length == 0)
		{
			if (!_tryParse)
			{
				_exc = new FormatException("Input string was not in the correct format: s.Length==0.");
			}
			return false;
		}
		if (_startIndex < 0 || _startIndex >= _input.Length)
		{
			if (!_tryParse)
			{
				_exc = new ArgumentException("_startIndex out of range (input='" + _input + "')", "_startIndex");
			}
			return false;
		}
		if (_endIndex < 0)
		{
			_endIndex = _input.Length - 1;
		}
		if (_endIndex >= _input.Length)
		{
			if (!_tryParse)
			{
				_exc = new ArgumentException("_endIndex out of range (input='" + _input + "')", "_endIndex");
			}
			return false;
		}
		int i;
		for (i = _startIndex; i <= _endIndex && char.IsWhiteSpace(_input[i]); i++)
		{
		}
		if (i > _endIndex)
		{
			if (!_tryParse)
			{
				_exc = new FormatException("Input string was not in the correct format (input='" + _input + "')");
			}
			return false;
		}
		bool flag = false;
		if (_input[i] == '-')
		{
			flag = true;
			i++;
		}
		else if (_input[i] == '+')
		{
			i++;
		}
		if (i > _endIndex)
		{
			if (!_tryParse)
			{
				_exc = new FormatException("Input string only has a sign (input='" + _input + "')");
			}
			return false;
		}
		ulong num = 0uL;
		int num2 = 0;
		do
		{
			char c = _input[i];
			if ((c ^ 0x30) > 9)
			{
				break;
			}
			num2++;
			checked
			{
				try
				{
					num = num * 10 + unchecked((ulong)(c ^ 0x30));
				}
				catch (OverflowException)
				{
					if (!_tryParse)
					{
						_exc = new OverflowException("Value too large or too small (input='" + _input + "')");
					}
					return false;
				}
			}
			i++;
		}
		while (i <= _endIndex);
		if (num2 == 0)
		{
			if (!_tryParse)
			{
				_exc = new FormatException("Input string was not in the correct format: nDigits == 0 (input='" + _input + "')");
			}
			return false;
		}
		if (i <= _endIndex)
		{
			for (; i <= _endIndex && char.IsWhiteSpace(_input[i]); i++)
			{
			}
			if (i <= _endIndex && _input[i] != 0)
			{
				if (!_tryParse)
				{
					_exc = new FormatException($"Input string was not in the correct format: Did not parse entire string. pos = {i} endIndex = {_endIndex} (input='{_input}')");
				}
				return false;
			}
		}
		if (_signedResult)
		{
			if (flag)
			{
				if (num > 9223372036854775808uL)
				{
					if (!_tryParse)
					{
						_exc = new OverflowException("Value too large or too small (input='" + _input + "')");
					}
					return false;
				}
				_resultSigned = -1L * (long)(num - 1) - 1;
				return true;
			}
			if (num > long.MaxValue)
			{
				if (!_tryParse)
				{
					_exc = new OverflowException("Value too large or too small (input='" + _input + "')");
				}
				return false;
			}
			_resultSigned = (long)num;
			return true;
		}
		if (flag && num != 0)
		{
			if (!_tryParse)
			{
				_exc = new OverflowException("Negative number (input='" + _input + "')");
			}
			return false;
		}
		_resultUnsigned = num;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool internalParseInt64Advanced(string _input, NumberStyles _numberStyle, bool _tryParse, out long _resultSigned, out ulong _resultUnsigned, out Exception _exc, bool _signedResult, int _startIndex, int _endIndex)
	{
		_resultSigned = 0L;
		_resultUnsigned = 0uL;
		_exc = null;
		if (_input == null)
		{
			if (!_tryParse)
			{
				_exc = new ArgumentNullException("_input");
			}
			return false;
		}
		if (_input.Length == 0)
		{
			if (!_tryParse)
			{
				_exc = new FormatException("Input string was not in the correct format: s.Length==0.");
			}
			return false;
		}
		if (_startIndex < 0 || _startIndex >= _input.Length)
		{
			if (!_tryParse)
			{
				_exc = new ArgumentException("_startIndex out of range (input='" + _input + "')", "_startIndex");
			}
			return false;
		}
		if (_endIndex < 0)
		{
			_endIndex = _input.Length - 1;
		}
		if (_endIndex >= _input.Length)
		{
			if (!_tryParse)
			{
				_exc = new ArgumentException("_endIndex out of range (input='" + _input + "')", "_endIndex");
			}
			return false;
		}
		if ((_numberStyle & NumberStyles.AllowHexSpecifier) != NumberStyles.None)
		{
			NumberStyles numberStyles = _numberStyle ^ NumberStyles.AllowHexSpecifier;
			if ((numberStyles & NumberStyles.AllowLeadingWhite) != NumberStyles.None)
			{
				numberStyles ^= NumberStyles.AllowLeadingWhite;
			}
			if ((numberStyles & NumberStyles.AllowTrailingWhite) != NumberStyles.None)
			{
				numberStyles ^= NumberStyles.AllowTrailingWhite;
			}
			if (numberStyles != NumberStyles.None)
			{
				if (!_tryParse)
				{
					_exc = new ArgumentException("With AllowHexSpecifier only AllowLeadingWhite and AllowTrailingWhite are permitted (input='" + _input + "')");
				}
				return false;
			}
		}
		else if (_numberStyle > NumberStyles.Any)
		{
			if (!_tryParse)
			{
				_exc = new ArgumentException("Not a valid number style (input='" + _input + "')");
			}
			return false;
		}
		bool flag = (_numberStyle & NumberStyles.AllowCurrencySymbol) != 0;
		bool flag2 = (_numberStyle & NumberStyles.AllowHexSpecifier) != 0;
		bool flag3 = (_numberStyle & NumberStyles.AllowThousands) != 0;
		bool flag4 = (_numberStyle & NumberStyles.AllowDecimalPoint) != 0;
		bool flag5 = (_numberStyle & NumberStyles.AllowParentheses) != 0;
		bool flag6 = (_numberStyle & NumberStyles.AllowTrailingSign) != 0;
		bool flag7 = (_numberStyle & NumberStyles.AllowLeadingSign) != 0;
		bool flag8 = (_numberStyle & NumberStyles.AllowTrailingWhite) != 0;
		bool flag9 = (_numberStyle & NumberStyles.AllowLeadingWhite) != 0;
		int i = _startIndex;
		if (flag9)
		{
			for (; i <= _endIndex && char.IsWhiteSpace(_input[i]); i++)
			{
			}
			if (i > _endIndex)
			{
				if (!_tryParse)
				{
					_exc = new FormatException("Input string was not in the correct format (input='" + _input + "')");
				}
				return false;
			}
		}
		bool flag10 = false;
		bool flag11 = false;
		bool flag12 = false;
		bool flag13 = false;
		if (flag5 && _input[i] == '(')
		{
			flag10 = true;
			flag12 = true;
			flag11 = true;
			i++;
			if (flag9)
			{
				for (; i <= _endIndex && char.IsWhiteSpace(_input[i]); i++)
				{
				}
				if (i > _endIndex)
				{
					if (!_tryParse)
					{
						_exc = new FormatException("Input string was not in the correct format (input='" + _input + "')");
					}
					return false;
				}
			}
			if (_input[i] == '-')
			{
				if (!_tryParse)
				{
					_exc = new FormatException("Input string was not in the correct format: Has Negative Sign (input='" + _input + "')");
				}
				return false;
			}
			if (_input[i] == '+')
			{
				if (!_tryParse)
				{
					_exc = new FormatException("Input string was not in the correct format: Has Positive Sign (input='" + _input + "')");
				}
				return false;
			}
		}
		if (flag7 && !flag12)
		{
			if (_input[i] == '-')
			{
				flag11 = true;
				flag12 = true;
				i++;
			}
			else if (_input[i] == '+')
			{
				flag12 = true;
				i++;
			}
			if (flag12 && flag9)
			{
				for (; i <= _endIndex && char.IsWhiteSpace(_input[i]); i++)
				{
				}
				if (i > _endIndex)
				{
					if (!_tryParse)
					{
						_exc = new FormatException("Input string was not in the correct format (input='" + _input + "')");
					}
					return false;
				}
			}
		}
		if (flag)
		{
			if (_input[i] == '¤')
			{
				flag13 = true;
				i++;
			}
			if (flag13)
			{
				if (flag9)
				{
					for (; i <= _endIndex && char.IsWhiteSpace(_input[i]); i++)
					{
					}
					if (i > _endIndex)
					{
						if (!_tryParse)
						{
							_exc = new FormatException("Input string was not in the correct format (input='" + _input + "')");
						}
						return false;
					}
				}
				if (!flag12 && flag7)
				{
					if (_input[i] == '-')
					{
						flag11 = true;
						flag12 = true;
						i++;
					}
					else if (_input[i] == '+')
					{
						flag11 = false;
						flag12 = true;
						i++;
					}
					if (flag12 && flag9)
					{
						for (; i <= _endIndex && char.IsWhiteSpace(_input[i]); i++)
						{
						}
						if (i > _endIndex)
						{
							if (!_tryParse)
							{
								_exc = new FormatException("Input string was not in the correct format (input='" + _input + "')");
							}
							return false;
						}
					}
				}
			}
		}
		ulong num = 0uL;
		int num2 = 0;
		bool flag14 = false;
		do
		{
			char c = _input[i];
			if ((c ^ 0x30) > 9 && (!flag2 || ((c < 'A' || c > 'F') && (c < 'a' || c > 'f'))))
			{
				if (!flag3 || !findOther(ref i, _input, ','))
				{
					if (flag14 || !flag4 || !findOther(ref i, _input, '.'))
					{
						break;
					}
					flag14 = true;
				}
				continue;
			}
			if (flag2)
			{
				num2++;
				int num3 = (((c ^ 0x30) <= 9) ? (c - 48) : ((c >= 'a') ? (c - 97 + 10) : (c - 65 + 10)));
				try
				{
					num = checked(num * 16 + (ulong)num3);
				}
				catch (OverflowException ex)
				{
					if (!_tryParse)
					{
						_exc = ex;
					}
					return false;
				}
				i++;
				continue;
			}
			if (flag14)
			{
				num2++;
				if (c != '0')
				{
					if (!_tryParse)
					{
						_exc = new OverflowException("Value too large or too small (input='" + _input + "')");
					}
					return false;
				}
				i++;
				continue;
			}
			num2++;
			checked
			{
				try
				{
					num = num * 10 + unchecked((ulong)checked(c - 48));
				}
				catch (OverflowException)
				{
					if (!_tryParse)
					{
						_exc = new OverflowException("Value too large or too small (input='" + _input + "')");
					}
					return false;
				}
			}
			i++;
		}
		while (i <= _endIndex);
		if (num2 == 0)
		{
			if (!_tryParse)
			{
				_exc = new FormatException("Input string was not in the correct format: nDigits == 0 (input='" + _input + "')");
			}
			return false;
		}
		if (flag6 && !flag12)
		{
			if (_input[i] == '-')
			{
				flag11 = true;
				flag12 = true;
				i++;
			}
			else if (_input[i] == '+')
			{
				flag11 = false;
				flag12 = true;
				i++;
			}
			if (flag12)
			{
				if (flag8)
				{
					for (; i <= _endIndex && char.IsWhiteSpace(_input[i]); i++)
					{
					}
					if (i > _endIndex)
					{
						if (!_tryParse)
						{
							_exc = new FormatException("Input string was not in the correct format (input='" + _input + "')");
						}
						return false;
					}
				}
				if (flag && _input[i] == '¤')
				{
					flag13 = true;
					i++;
				}
			}
		}
		if (flag && !flag13)
		{
			if (_input[i] == '¤')
			{
				flag13 = true;
				i++;
			}
			if (flag13 && i < _input.Length)
			{
				if (flag8)
				{
					for (; i <= _endIndex && char.IsWhiteSpace(_input[i]); i++)
					{
					}
					if (i > _endIndex)
					{
						if (!_tryParse)
						{
							_exc = new FormatException("Input string was not in the correct format (input='" + _input + "')");
						}
						return false;
					}
				}
				if (!flag12 && flag6)
				{
					if (_input[i] == '-')
					{
						flag11 = true;
						flag12 = true;
						i++;
					}
					else if (_input[i] == '+')
					{
						flag11 = false;
						flag12 = true;
						i++;
					}
				}
			}
		}
		if (flag8 && i <= _endIndex)
		{
			for (; i <= _endIndex && char.IsWhiteSpace(_input[i]); i++)
			{
			}
		}
		if (flag10)
		{
			if (i > _endIndex || _input[i++] != ')')
			{
				if (!_tryParse)
				{
					_exc = new FormatException("Input string was not in the correct format: No room for close parens (input='" + _input + "')");
				}
				return false;
			}
			if (flag8 && i <= _endIndex)
			{
				for (; i <= _endIndex && char.IsWhiteSpace(_input[i]); i++)
				{
				}
			}
		}
		if (i <= _endIndex && _input[i] != 0)
		{
			if (!_tryParse)
			{
				_exc = new FormatException($"Input string was not in the correct format: Did not parse entire string. pos = {i} endIndex = {_endIndex} (input='{_input}')");
			}
			return false;
		}
		if (_signedResult)
		{
			if (flag11)
			{
				ulong num4 = 9223372036854775808uL;
				if (num > num4)
				{
					if (!_tryParse)
					{
						_exc = new OverflowException("Value too large or too small (input='" + _input + "')");
					}
					return false;
				}
				_resultSigned = -1L * (long)(num - 1) - 1;
				return true;
			}
			ulong num5 = 9223372036854775807uL;
			if (num > num5)
			{
				if (!_tryParse)
				{
					_exc = new OverflowException("Value too large or too small (input='" + _input + "')");
				}
				return false;
			}
			_resultSigned = (long)num;
			return true;
		}
		if (flag11)
		{
			if (!_tryParse)
			{
				_exc = new OverflowException("Negative number (input='" + _input + "')");
			}
			return false;
		}
		_resultUnsigned = num;
		return true;
	}
}
