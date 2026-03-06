using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal struct JsonPosition(JsonContainerType type)
{
	private static readonly char[] SpecialCharacters = new char[18]
	{
		'.', ' ', '\'', '/', '"', '[', ']', '(', ')', '\t',
		'\n', '\r', '\f', '\b', '\\', '\u0085', '\u2028', '\u2029'
	};

	internal JsonContainerType Type = type;

	internal int Position = -1;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	internal string PropertyName = null;

	internal bool HasIndex = TypeHasIndex(type);

	internal int CalculateLength()
	{
		switch (Type)
		{
		case JsonContainerType.Object:
			return PropertyName.Length + 5;
		case JsonContainerType.Array:
		case JsonContainerType.Constructor:
			return Newtonsoft.Json.Utilities.MathUtils.IntLength((ulong)Position) + 2;
		default:
			throw new ArgumentOutOfRangeException("Type");
		}
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	internal void WriteTo([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] StringBuilder sb, ref StringWriter writer, ref char[] buffer)
	{
		switch (Type)
		{
		case JsonContainerType.Object:
		{
			string propertyName = PropertyName;
			if (propertyName.IndexOfAny(SpecialCharacters) != -1)
			{
				sb.Append("['");
				if (writer == null)
				{
					writer = new StringWriter(sb);
				}
				JavaScriptUtils.WriteEscapedJavaScriptString(writer, propertyName, '\'', appendDelimiters: false, JavaScriptUtils.SingleQuoteCharEscapeFlags, StringEscapeHandling.Default, null, ref buffer);
				sb.Append("']");
			}
			else
			{
				if (sb.Length > 0)
				{
					sb.Append('.');
				}
				sb.Append(propertyName);
			}
			break;
		}
		case JsonContainerType.Array:
		case JsonContainerType.Constructor:
			sb.Append('[');
			sb.Append(Position);
			sb.Append(']');
			break;
		}
	}

	internal static bool TypeHasIndex(JsonContainerType type)
	{
		if (type != JsonContainerType.Array)
		{
			return type == JsonContainerType.Constructor;
		}
		return true;
	}

	internal static string BuildPath(List<JsonPosition> positions, JsonPosition? currentPosition)
	{
		int num = 0;
		if (positions != null)
		{
			for (int i = 0; i < positions.Count; i++)
			{
				num += positions[i].CalculateLength();
			}
		}
		if (currentPosition.HasValue)
		{
			num += currentPosition.GetValueOrDefault().CalculateLength();
		}
		StringBuilder stringBuilder = new StringBuilder(num);
		StringWriter writer = null;
		char[] buffer = null;
		if (positions != null)
		{
			foreach (JsonPosition position in positions)
			{
				position.WriteTo(stringBuilder, ref writer, ref buffer);
			}
		}
		currentPosition?.WriteTo(stringBuilder, ref writer, ref buffer);
		return stringBuilder.ToString();
	}

	internal static string FormatMessage([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] IJsonLineInfo lineInfo, string path, string message)
	{
		if (!message.EndsWith(Environment.NewLine, StringComparison.Ordinal))
		{
			message = message.Trim();
			if (!message.EndsWith('.'))
			{
				message += ".";
			}
			message += " ";
		}
		message += "Path '{0}'".FormatWith(CultureInfo.InvariantCulture, path);
		if (lineInfo != null && lineInfo.HasLineInfo())
		{
			message += ", line {0}, position {1}".FormatWith(CultureInfo.InvariantCulture, lineInfo.LineNumber, lineInfo.LinePosition);
		}
		message += ".";
		return message;
	}
}
