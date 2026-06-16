using System;
using System.Collections.Generic;
using UnityEngine;

namespace Webserver;

public class LogBuffer
{
	public class LogEntry
	{
		public readonly int MessageId;

		public readonly DateTime Timestamp;

		public readonly string IsoTime;

		public readonly string Message;

		public readonly string Trace;

		public readonly LogType Type;

		public readonly long Uptime;

		public LogEntry(int _messageId, DateTime _timestamp, string _message, string _trace, LogType _type, long _uptime)
		{
			MessageId = _messageId;
			Timestamp = _timestamp;
			IsoTime = _timestamp.ToString("o");
			Message = _message;
			Trace = _trace;
			Type = _type;
			Uptime = _uptime;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int maxEntries = 3000;

	[PublicizedFrom(EAccessModifier.Private)]
	public static LogBuffer instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<LogEntry> logEntries = new List<LogEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int listOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<LogEntry> emptyList = new List<LogEntry>();

	public static LogBuffer Instance => instance ?? (instance = new LogBuffer());

	public int OldestLine
	{
		get
		{
			lock (logEntries)
			{
				return listOffset;
			}
		}
	}

	public int LatestLine
	{
		get
		{
			lock (logEntries)
			{
				return listOffset + logEntries.Count - 1;
			}
		}
	}

	public int StoredLines
	{
		get
		{
			lock (logEntries)
			{
				return logEntries.Count;
			}
		}
	}

	public LogEntry this[int _index]
	{
		get
		{
			lock (logEntries)
			{
				if (_index >= listOffset && _index < listOffset + logEntries.Count)
				{
					return logEntries[_index];
				}
			}
			return null;
		}
	}

	public static event Action<LogEntry> EntryAdded;

	public static void Init()
	{
		if (instance == null)
		{
			instance = new LogBuffer();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public LogBuffer()
	{
		Log.LogCallbacksExtended += LogCallback;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogCallback(string _formattedMsg, string _plainMsg, string _trace, LogType _type, DateTime _timestamp, long _uptime)
	{
		lock (logEntries)
		{
			LogEntry logEntry = new LogEntry(listOffset + logEntries.Count, _timestamp, _plainMsg, _trace, _type, _uptime);
			logEntries.Add(logEntry);
			LogBuffer.EntryAdded?.Invoke(logEntry);
			if (logEntries.Count > 3000)
			{
				listOffset += logEntries.Count - 3000;
				logEntries.RemoveRange(0, logEntries.Count - 3000);
			}
		}
	}

	public List<LogEntry> GetRange(ref int _start, int _count, out int _end)
	{
		lock (logEntries)
		{
			int num;
			if (_count < 0)
			{
				_count = -_count;
				if (_start >= listOffset + logEntries.Count)
				{
					_start = listOffset + logEntries.Count - 1;
				}
				_end = _start;
				if (_start < listOffset)
				{
					return emptyList;
				}
				_start -= _count - 1;
				if (_start < listOffset)
				{
					_start = listOffset;
				}
				num = _start - listOffset;
				_end++;
				_count = _end - _start;
			}
			else
			{
				if (_start < listOffset)
				{
					_start = listOffset;
				}
				if (_start >= listOffset + logEntries.Count)
				{
					_end = _start;
					return emptyList;
				}
				num = _start - listOffset;
				if (num + _count > logEntries.Count)
				{
					_count = logEntries.Count - num;
				}
				_end = _start + _count;
			}
			return logEntries.GetRange(num, _count);
		}
	}
}
