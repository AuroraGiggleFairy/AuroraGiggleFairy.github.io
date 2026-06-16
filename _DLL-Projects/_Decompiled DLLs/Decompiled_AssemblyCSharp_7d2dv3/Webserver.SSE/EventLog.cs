using UnityEngine.Scripting;
using Utf8Json;
using Webserver.UrlHandlers;
using Webserver.WebAPI.APIs;

namespace Webserver.SSE;

[Preserve]
public class EventLog : AbsEvent
{
	public EventLog(SseHandler _parent)
		: base(_parent, _reuseEncodingBuffer: true, "log")
	{
		LogBuffer.EntryAdded += LogCallback;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogCallback(LogBuffer.LogEntry _logEntry)
	{
		JsonWriter _writer = default(JsonWriter);
		LogApi.WriteLogMessageObject(ref _writer, _logEntry);
		SendData("logLine", _writer.ToString());
	}
}
