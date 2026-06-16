using System.Collections.Generic;
using UnityEngine.Scripting;
using Utf8Json;

namespace Webserver.WebAPI.APIs;

[Preserve]
public class LogApi : AbsRestApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int maxCount = 1000;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyEntries = JsonWriter.GetEncodedPropertyNameWithBeginObject("entries");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyFirstLine = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("firstLine");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyLastLine = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("lastLine");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonIdKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("id");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonMsgKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("msg");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonTypeKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("type");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonTraceKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("trace");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonIsotimeKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("isotime");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonUptimeKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("uptime");

	public LogApi()
		: base("Log")
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestGet(RequestContext _context)
	{
		if (_context.QueryParameters["count"] == null || !int.TryParse(_context.QueryParameters["count"], out var result))
		{
			result = 50;
		}
		if (result == 0)
		{
			result = 1;
		}
		if (result > 1000)
		{
			result = 1000;
		}
		if (result < -1000)
		{
			result = -1000;
		}
		if (_context.QueryParameters["firstLine"] == null || !int.TryParse(_context.QueryParameters["firstLine"], out var _start))
		{
			_start = ((result > 0) ? LogBuffer.Instance.OldestLine : LogBuffer.Instance.LatestLine);
		}
		AbsRestApi.PrepareEnvelopedResult(out var _writer);
		_writer.WriteRaw(jsonKeyEntries);
		int _end;
		List<LogBuffer.LogEntry> range = LogBuffer.Instance.GetRange(ref _start, result, out _end);
		_writer.WriteBeginArray();
		for (int i = 0; i < range.Count; i++)
		{
			LogBuffer.LogEntry logEntry = range[i];
			if (i > 0)
			{
				_writer.WriteValueSeparator();
			}
			WriteLogMessageObject(ref _writer, logEntry);
		}
		_writer.WriteEndArray();
		_writer.WriteRaw(jsonKeyFirstLine);
		_writer.WriteInt32(_start);
		_writer.WriteRaw(jsonKeyLastLine);
		_writer.WriteInt32(_end);
		_writer.WriteEndObject();
		AbsRestApi.SendEnvelopedResult(_context, ref _writer);
	}

	public static void WriteLogMessageObject(ref JsonWriter _writer, LogBuffer.LogEntry _logEntry)
	{
		_writer.WriteRaw(jsonIdKey);
		_writer.WriteInt32(_logEntry.MessageId);
		_writer.WriteRaw(jsonMsgKey);
		_writer.WriteString(_logEntry.Message);
		_writer.WriteRaw(jsonTypeKey);
		_writer.WriteString(_logEntry.Type.ToStringCached());
		_writer.WriteRaw(jsonTraceKey);
		_writer.WriteString(_logEntry.Trace);
		_writer.WriteRaw(jsonIsotimeKey);
		_writer.WriteString(_logEntry.IsoTime);
		_writer.WriteRaw(jsonUptimeKey);
		long uptime = _logEntry.Uptime;
		_writer.WriteString(uptime.ToString());
		_writer.WriteEndObject();
	}
}
