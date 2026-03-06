using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Serialization;

internal class DiagnosticsTraceWriter : ITraceWriter
{
	public TraceLevel LevelFilter { get; set; }

	private TraceEventType GetTraceEventType(TraceLevel level)
	{
		return level switch
		{
			TraceLevel.Error => TraceEventType.Error, 
			TraceLevel.Warning => TraceEventType.Warning, 
			TraceLevel.Info => TraceEventType.Information, 
			TraceLevel.Verbose => TraceEventType.Verbose, 
			_ => throw new ArgumentOutOfRangeException("level"), 
		};
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public void Trace(TraceLevel level, string message, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] Exception ex)
	{
		if (level == TraceLevel.Off)
		{
			return;
		}
		TraceEventCache eventCache = new TraceEventCache();
		TraceEventType traceEventType = GetTraceEventType(level);
		foreach (TraceListener listener in System.Diagnostics.Trace.Listeners)
		{
			if (!listener.IsThreadSafe)
			{
				lock (listener)
				{
					listener.TraceEvent(eventCache, "Newtonsoft.Json", traceEventType, 0, message);
				}
			}
			else
			{
				listener.TraceEvent(eventCache, "Newtonsoft.Json", traceEventType, 0, message);
			}
			if (System.Diagnostics.Trace.AutoFlush)
			{
				listener.Flush();
			}
		}
	}
}
