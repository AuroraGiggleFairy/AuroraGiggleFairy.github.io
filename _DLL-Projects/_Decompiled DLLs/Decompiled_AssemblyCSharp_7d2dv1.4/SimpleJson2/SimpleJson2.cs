using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using SimpleJson2.Reflection;

namespace SimpleJson2;

[GeneratedCode("simple-json", "1.0.0")]
public static class SimpleJson2
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int TOKEN_NONE = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int TOKEN_CURLY_OPEN = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int TOKEN_CURLY_CLOSE = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int TOKEN_SQUARED_OPEN = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int TOKEN_SQUARED_CLOSE = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int TOKEN_COLON = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int TOKEN_COMMA = 6;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int TOKEN_STRING = 7;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int TOKEN_NUMBER = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int TOKEN_TRUE = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int TOKEN_FALSE = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int TOKEN_NULL = 11;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int BUILDER_CAPACITY = 2000;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] EscapeTable;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] EscapeCharacters;

	[PublicizedFrom(EAccessModifier.Private)]
	public static IJsonSerializerStrategy _currentJsonSerializerStrategy;

	[PublicizedFrom(EAccessModifier.Private)]
	public static PocoJsonSerializerStrategy _pocoJsonSerializerStrategy;

	public static IJsonSerializerStrategy CurrentJsonSerializerStrategy
	{
		get
		{
			return _currentJsonSerializerStrategy ?? (_currentJsonSerializerStrategy = PocoJsonSerializerStrategy);
		}
		set
		{
			_currentJsonSerializerStrategy = value;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static PocoJsonSerializerStrategy PocoJsonSerializerStrategy => _pocoJsonSerializerStrategy ?? (_pocoJsonSerializerStrategy = new PocoJsonSerializerStrategy());

	[PublicizedFrom(EAccessModifier.Private)]
	static SimpleJson2()
	{
		EscapeCharacters = new char[7] { '"', '\\', '\b', '\f', '\n', '\r', '\t' };
		EscapeTable = new char[93];
		EscapeTable[34] = '"';
		EscapeTable[92] = '\\';
		EscapeTable[8] = 'b';
		EscapeTable[12] = 'f';
		EscapeTable[10] = 'n';
		EscapeTable[13] = 'r';
		EscapeTable[9] = 't';
	}

	public static object DeserializeObject(string json)
	{
		if (TryDeserializeObject(json, out var obj))
		{
			return obj;
		}
		throw new SerializationException("Invalid JSON string");
	}

	public static bool TryDeserializeObject(string json, out object obj)
	{
		bool success = true;
		if (json != null)
		{
			char[] json2 = json.ToCharArray();
			int index = 0;
			obj = ParseValue(json2, ref index, ref success);
		}
		else
		{
			obj = null;
		}
		return success;
	}

	public static object DeserializeObject(string json, Type type, IJsonSerializerStrategy jsonSerializerStrategy)
	{
		object obj = DeserializeObject(json);
		if (!(type == null) && (obj == null || !ReflectionUtils.IsAssignableFrom(obj.GetType(), type)))
		{
			return (jsonSerializerStrategy ?? CurrentJsonSerializerStrategy).DeserializeObject(obj, type);
		}
		return obj;
	}

	public static object DeserializeObject(string json, Type type)
	{
		return DeserializeObject(json, type, null);
	}

	public static T DeserializeObject<T>(string json, IJsonSerializerStrategy jsonSerializerStrategy)
	{
		return (T)DeserializeObject(json, typeof(T), jsonSerializerStrategy);
	}

	public static T DeserializeObject<T>(string json)
	{
		return (T)DeserializeObject(json, typeof(T), null);
	}

	public static string SerializeObject(object json, IJsonSerializerStrategy jsonSerializerStrategy)
	{
		StringBuilder stringBuilder = new StringBuilder(2000);
		if (!SerializeValue(jsonSerializerStrategy, json, stringBuilder))
		{
			return null;
		}
		return stringBuilder.ToString();
	}

	public static string SerializeObject(object json)
	{
		return SerializeObject(json, CurrentJsonSerializerStrategy);
	}

	public static string EscapeToJavascriptString(string jsonString)
	{
		if (string.IsNullOrEmpty(jsonString))
		{
			return jsonString;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		while (num < jsonString.Length)
		{
			char c = jsonString[num++];
			if (c == '\\')
			{
				if (jsonString.Length - num >= 2)
				{
					switch (jsonString[num])
					{
					case '\\':
						stringBuilder.Append('\\');
						num++;
						break;
					case '"':
						stringBuilder.Append("\"");
						num++;
						break;
					case 't':
						stringBuilder.Append('\t');
						num++;
						break;
					case 'b':
						stringBuilder.Append('\b');
						num++;
						break;
					case 'n':
						stringBuilder.Append('\n');
						num++;
						break;
					case 'r':
						stringBuilder.Append('\r');
						num++;
						break;
					}
				}
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IDictionary<string, object> ParseObject(char[] json, ref int index, ref bool success)
	{
		IDictionary<string, object> dictionary = new JsonObject();
		NextToken(json, ref index);
		bool flag = false;
		while (!flag)
		{
			switch (LookAhead(json, index))
			{
			case 0:
				success = false;
				return null;
			case 6:
				NextToken(json, ref index);
				continue;
			case 2:
				NextToken(json, ref index);
				return dictionary;
			}
			string key = ParseString(json, ref index, ref success);
			if (!success)
			{
				success = false;
				return null;
			}
			int num = NextToken(json, ref index);
			if (num != 5)
			{
				success = false;
				return null;
			}
			object value = ParseValue(json, ref index, ref success);
			if (!success)
			{
				success = false;
				return null;
			}
			dictionary[key] = value;
		}
		return dictionary;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static JsonArray ParseArray(char[] json, ref int index, ref bool success)
	{
		JsonArray jsonArray = new JsonArray();
		NextToken(json, ref index);
		bool flag = false;
		while (!flag)
		{
			switch (LookAhead(json, index))
			{
			case 0:
				success = false;
				return null;
			case 6:
				NextToken(json, ref index);
				continue;
			case 4:
				break;
			default:
			{
				object item = ParseValue(json, ref index, ref success);
				if (!success)
				{
					return null;
				}
				jsonArray.Add(item);
				continue;
			}
			}
			NextToken(json, ref index);
			break;
		}
		return jsonArray;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static object ParseValue(char[] json, ref int index, ref bool success)
	{
		switch (LookAhead(json, index))
		{
		case 7:
			return ParseString(json, ref index, ref success);
		case 8:
			return ParseNumber(json, ref index, ref success);
		case 1:
			return ParseObject(json, ref index, ref success);
		case 3:
			return ParseArray(json, ref index, ref success);
		case 9:
			NextToken(json, ref index);
			return true;
		case 10:
			NextToken(json, ref index);
			return false;
		case 11:
			NextToken(json, ref index);
			return null;
		default:
			success = false;
			return null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string ParseString(char[] json, ref int index, ref bool success)
	{
		StringBuilder stringBuilder = new StringBuilder(2000);
		EatWhitespace(json, ref index);
		char c = json[index++];
		bool flag = false;
		while (!flag && index != json.Length)
		{
			c = json[index++];
			switch (c)
			{
			case '"':
				flag = true;
				break;
			case '\\':
			{
				if (index == json.Length)
				{
					break;
				}
				switch (json[index++])
				{
				case '"':
					stringBuilder.Append('"');
					continue;
				case '\\':
					stringBuilder.Append('\\');
					continue;
				case '/':
					stringBuilder.Append('/');
					continue;
				case 'b':
					stringBuilder.Append('\b');
					continue;
				case 'f':
					stringBuilder.Append('\f');
					continue;
				case 'n':
					stringBuilder.Append('\n');
					continue;
				case 'r':
					stringBuilder.Append('\r');
					continue;
				case 't':
					stringBuilder.Append('\t');
					continue;
				case 'u':
					break;
				default:
					continue;
				}
				if (json.Length - index < 4)
				{
					break;
				}
				if (!(success = uint.TryParse(new string(json, index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result)))
				{
					return "";
				}
				if (55296 <= result && result <= 56319)
				{
					index += 4;
					if (json.Length - index < 6 || !(new string(json, index, 2) == "\\u") || !uint.TryParse(new string(json, index + 2, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result2) || 56320 > result2 || result2 > 57343)
					{
						success = false;
						return "";
					}
					stringBuilder.Append((char)result);
					stringBuilder.Append((char)result2);
					index += 6;
				}
				else
				{
					stringBuilder.Append(ConvertFromUtf32((int)result));
					index += 4;
				}
				continue;
			}
			default:
				stringBuilder.Append(c);
				continue;
			}
			break;
		}
		if (!flag)
		{
			success = false;
			return null;
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string ConvertFromUtf32(int utf32)
	{
		if (utf32 < 0 || utf32 > 1114111)
		{
			throw new ArgumentOutOfRangeException("utf32", "The argument must be from 0 to 0x10FFFF.");
		}
		if (55296 <= utf32 && utf32 <= 57343)
		{
			throw new ArgumentOutOfRangeException("utf32", "The argument must not be in surrogate pair range.");
		}
		if (utf32 < 65536)
		{
			return new string((char)utf32, 1);
		}
		utf32 -= 65536;
		return new string(new char[2]
		{
			(char)((utf32 >> 10) + 55296),
			(char)(utf32 % 1024 + 56320)
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static object ParseNumber(char[] json, ref int index, ref bool success)
	{
		EatWhitespace(json, ref index);
		int lastIndexOfNumber = GetLastIndexOfNumber(json, index);
		int length = lastIndexOfNumber - index + 1;
		string text = new string(json, index, length);
		object result2;
		if (text.IndexOf(".", StringComparison.OrdinalIgnoreCase) != -1 || text.IndexOf("e", StringComparison.OrdinalIgnoreCase) != -1)
		{
			success = double.TryParse(new string(json, index, length), NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
			result2 = result;
		}
		else
		{
			success = long.TryParse(new string(json, index, length), NumberStyles.Any, CultureInfo.InvariantCulture, out var result3);
			result2 = result3;
		}
		index = lastIndexOfNumber + 1;
		return result2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int GetLastIndexOfNumber(char[] json, int index)
	{
		int i;
		for (i = index; i < json.Length && "0123456789+-.eE".IndexOf(json[i]) != -1; i++)
		{
		}
		return i - 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void EatWhitespace(char[] json, ref int index)
	{
		while (index < json.Length && " \t\n\r\b\f".IndexOf(json[index]) != -1)
		{
			index++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int LookAhead(char[] json, int index)
	{
		int index2 = index;
		return NextToken(json, ref index2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int NextToken(char[] json, ref int index)
	{
		EatWhitespace(json, ref index);
		if (index == json.Length)
		{
			return 0;
		}
		char c = json[index];
		index++;
		switch (c)
		{
		case '{':
			return 1;
		case '}':
			return 2;
		case '[':
			return 3;
		case ']':
			return 4;
		case ',':
			return 6;
		case '"':
			return 7;
		case '-':
		case '0':
		case '1':
		case '2':
		case '3':
		case '4':
		case '5':
		case '6':
		case '7':
		case '8':
		case '9':
			return 8;
		case ':':
			return 5;
		default:
		{
			index--;
			int num = json.Length - index;
			if (num >= 5 && json[index] == 'f' && json[index + 1] == 'a' && json[index + 2] == 'l' && json[index + 3] == 's' && json[index + 4] == 'e')
			{
				index += 5;
				return 10;
			}
			if (num >= 4 && json[index] == 't' && json[index + 1] == 'r' && json[index + 2] == 'u' && json[index + 3] == 'e')
			{
				index += 4;
				return 9;
			}
			if (num >= 4 && json[index] == 'n' && json[index + 1] == 'u' && json[index + 2] == 'l' && json[index + 3] == 'l')
			{
				index += 4;
				return 11;
			}
			return 0;
		}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool SerializeValue(IJsonSerializerStrategy jsonSerializerStrategy, object value, StringBuilder builder)
	{
		bool flag = true;
		if (value is string aString)
		{
			flag = SerializeString(aString, builder);
		}
		else if (value is IDictionary<string, object> dictionary)
		{
			flag = SerializeObject(jsonSerializerStrategy, dictionary.Keys, dictionary.Values, builder);
		}
		else if (value is IDictionary<string, string> dictionary2)
		{
			flag = SerializeObject(jsonSerializerStrategy, dictionary2.Keys, dictionary2.Values, builder);
		}
		else if (value is IEnumerable anArray)
		{
			flag = SerializeArray(jsonSerializerStrategy, anArray, builder);
		}
		else if (IsNumeric(value))
		{
			flag = SerializeNumber(value, builder);
		}
		else if (value is bool)
		{
			builder.Append(((bool)value) ? "true" : "false");
		}
		else if (value == null)
		{
			builder.Append("null");
		}
		else
		{
			flag = jsonSerializerStrategy.TrySerializeNonPrimitiveObject(value, out var output);
			if (flag)
			{
				SerializeValue(jsonSerializerStrategy, output, builder);
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool SerializeObject(IJsonSerializerStrategy jsonSerializerStrategy, IEnumerable keys, IEnumerable values, StringBuilder builder)
	{
		builder.Append("{");
		IEnumerator enumerator = keys.GetEnumerator();
		IEnumerator enumerator2 = values.GetEnumerator();
		bool flag = true;
		while (enumerator.MoveNext() && enumerator2.MoveNext())
		{
			object current = enumerator.Current;
			object current2 = enumerator2.Current;
			if (!flag)
			{
				builder.Append(",");
			}
			if (current is string aString)
			{
				SerializeString(aString, builder);
			}
			else if (!SerializeValue(jsonSerializerStrategy, current2, builder))
			{
				return false;
			}
			builder.Append(":");
			if (!SerializeValue(jsonSerializerStrategy, current2, builder))
			{
				return false;
			}
			flag = false;
		}
		builder.Append("}");
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool SerializeArray(IJsonSerializerStrategy jsonSerializerStrategy, IEnumerable anArray, StringBuilder builder)
	{
		builder.Append("[");
		bool flag = true;
		foreach (object item in anArray)
		{
			if (!flag)
			{
				builder.Append(",");
			}
			if (!SerializeValue(jsonSerializerStrategy, item, builder))
			{
				return false;
			}
			flag = false;
		}
		builder.Append("]");
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool SerializeString(string aString, StringBuilder builder)
	{
		if (aString.IndexOfAny(EscapeCharacters) == -1)
		{
			builder.Append('"');
			builder.Append(aString);
			builder.Append('"');
			return true;
		}
		builder.Append('"');
		int num = 0;
		char[] array = aString.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			char c = array[i];
			if (c >= EscapeTable.Length || EscapeTable[(uint)c] == '\0')
			{
				num++;
				continue;
			}
			if (num > 0)
			{
				builder.Append(array, i - num, num);
				num = 0;
			}
			builder.Append('\\');
			builder.Append(EscapeTable[(uint)c]);
		}
		if (num > 0)
		{
			builder.Append(array, array.Length - num, num);
		}
		builder.Append('"');
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool SerializeNumber(object number, StringBuilder builder)
	{
		if (number is long)
		{
			builder.Append(((long)number).ToString(CultureInfo.InvariantCulture));
		}
		else if (number is ulong)
		{
			builder.Append(((ulong)number).ToString(CultureInfo.InvariantCulture));
		}
		else if (number is int)
		{
			builder.Append(((int)number).ToString(CultureInfo.InvariantCulture));
		}
		else if (number is uint)
		{
			builder.Append(((uint)number).ToString(CultureInfo.InvariantCulture));
		}
		else if (number is decimal)
		{
			builder.Append(((decimal)number).ToString(CultureInfo.InvariantCulture));
		}
		else if (number is float)
		{
			builder.Append(((float)number).ToString(CultureInfo.InvariantCulture));
		}
		else
		{
			builder.Append(Convert.ToDouble(number, CultureInfo.InvariantCulture).ToString("r", CultureInfo.InvariantCulture));
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool IsNumeric(object value)
	{
		if (value is sbyte)
		{
			return true;
		}
		if (value is byte)
		{
			return true;
		}
		if (value is short)
		{
			return true;
		}
		if (value is ushort)
		{
			return true;
		}
		if (value is int)
		{
			return true;
		}
		if (value is uint)
		{
			return true;
		}
		if (value is long)
		{
			return true;
		}
		if (value is ulong)
		{
			return true;
		}
		if (value is float)
		{
			return true;
		}
		if (value is double)
		{
			return true;
		}
		if (value is decimal)
		{
			return true;
		}
		return false;
	}
}
