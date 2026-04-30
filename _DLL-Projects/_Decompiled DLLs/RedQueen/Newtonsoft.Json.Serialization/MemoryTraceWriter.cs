using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Newtonsoft.Json.Serialization;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class MemoryTraceWriter : ITraceWriter
{
	private readonly Queue<string> _traceMessages;

	private readonly object _lock;

	public TraceLevel LevelFilter { get; set; }

	public MemoryTraceWriter()
	{
		LevelFilter = TraceLevel.Verbose;
		_traceMessages = new Queue<string>();
		_lock = new object();
	}

	public void Trace(TraceLevel level, string message, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] Exception ex)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff", CultureInfo.InvariantCulture));
		stringBuilder.Append(" ");
		stringBuilder.Append(level.ToString("g"));
		stringBuilder.Append(" ");
		stringBuilder.Append(message);
		string item = stringBuilder.ToString();
		lock (_lock)
		{
			if (_traceMessages.Count >= 1000)
			{
				_traceMessages.Dequeue();
			}
			_traceMessages.Enqueue(item);
		}
	}

	public IEnumerable<string> GetTraceMessages()
	{
		return _traceMessages;
	}

	public override string ToString()
	{
		lock (_lock)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string traceMessage in _traceMessages)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(traceMessage);
			}
			return stringBuilder.ToString();
		}
	}
}
