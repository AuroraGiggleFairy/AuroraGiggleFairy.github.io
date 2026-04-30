using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Newtonsoft.Json.Utilities;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal static class MiscellaneousUtils
{
	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	[Conditional("DEBUG")]
	public static void Assert([_003C32c2a3cd_002D2527_002D43b3_002D9fd6_002D785536118797_003EDoesNotReturnIf(false)] bool condition, string message = null)
	{
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	public static bool ValueEquals(object objA, object objB)
	{
		if (objA == objB)
		{
			return true;
		}
		if (objA == null || objB == null)
		{
			return false;
		}
		if (objA.GetType() != objB.GetType())
		{
			if (ConvertUtils.IsInteger(objA) && ConvertUtils.IsInteger(objB))
			{
				return Convert.ToDecimal(objA, CultureInfo.CurrentCulture).Equals(Convert.ToDecimal(objB, CultureInfo.CurrentCulture));
			}
			if ((objA is double || objA is float || objA is decimal) && (objB is double || objB is float || objB is decimal))
			{
				return MathUtils.ApproxEquals(Convert.ToDouble(objA, CultureInfo.CurrentCulture), Convert.ToDouble(objB, CultureInfo.CurrentCulture));
			}
			return false;
		}
		return objA.Equals(objB);
	}

	public static ArgumentOutOfRangeException CreateArgumentOutOfRangeException(string paramName, object actualValue, string message)
	{
		string message2 = message + Environment.NewLine + "Actual value was {0}.".FormatWith(CultureInfo.InvariantCulture, actualValue);
		return new ArgumentOutOfRangeException(paramName, message2);
	}

	public static string ToString([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value)
	{
		if (value == null)
		{
			return "{null}";
		}
		if (!(value is string text))
		{
			return value.ToString();
		}
		return "\"" + text + "\"";
	}

	public static int ByteArrayCompare(byte[] a1, byte[] a2)
	{
		int num = a1.Length.CompareTo(a2.Length);
		if (num != 0)
		{
			return num;
		}
		for (int i = 0; i < a1.Length; i++)
		{
			int num2 = a1[i].CompareTo(a2[i]);
			if (num2 != 0)
			{
				return num2;
			}
		}
		return 0;
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public static string GetPrefix(string qualifiedName)
	{
		GetQualifiedNameParts(qualifiedName, out var prefix, out var _);
		return prefix;
	}

	public static string GetLocalName(string qualifiedName)
	{
		GetQualifiedNameParts(qualifiedName, out var _, out var localName);
		return localName;
	}

	public static void GetQualifiedNameParts(string qualifiedName, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] out string prefix, out string localName)
	{
		int num = StringUtils.IndexOf(qualifiedName, ':');
		if (num == -1 || num == 0 || qualifiedName.Length - 1 == num)
		{
			prefix = null;
			localName = qualifiedName;
		}
		else
		{
			prefix = qualifiedName.Substring(0, num);
			localName = qualifiedName.Substring(num + 1);
		}
	}

	internal static RegexOptions GetRegexOptions(string optionsText)
	{
		RegexOptions regexOptions = RegexOptions.None;
		for (int i = 0; i < optionsText.Length; i++)
		{
			switch (optionsText[i])
			{
			case 'i':
				regexOptions |= RegexOptions.IgnoreCase;
				break;
			case 'm':
				regexOptions |= RegexOptions.Multiline;
				break;
			case 's':
				regexOptions |= RegexOptions.Singleline;
				break;
			case 'x':
				regexOptions |= RegexOptions.ExplicitCapture;
				break;
			}
		}
		return regexOptions;
	}
}
