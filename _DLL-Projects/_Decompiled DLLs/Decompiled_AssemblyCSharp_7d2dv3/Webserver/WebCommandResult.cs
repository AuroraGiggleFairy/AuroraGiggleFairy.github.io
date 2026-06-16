using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Utf8Json;

namespace Webserver;

public class WebCommandResult : IConsoleConnection
{
	public enum ResultType
	{
		Full,
		ResultOnly,
		Raw
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string command;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string parameters;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string sourceName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly RequestContext context;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ResultType responseType;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonRawKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("resultRaw");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonCommandKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("command");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonParametersKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("parameters");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonResultKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("result");

	public WebCommandResult(string _command, string _parameters, ResultType _responseType, RequestContext _context)
	{
		context = _context;
		command = _command;
		parameters = _parameters;
		responseType = _responseType;
		object obj = _context.Connection?.Username;
		if (obj == null)
		{
			int permissionLevel = _context.PermissionLevel;
			obj = "Unauth-PermLevel-" + permissionLevel;
		}
		sourceName = (string)obj;
	}

	public void SendLines(List<string> _output)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string item in _output)
		{
			stringBuilder.AppendLine(item);
		}
		string text = stringBuilder.ToString();
		try
		{
			if (responseType == ResultType.Raw)
			{
				WebUtils.WriteText(context.Response, text);
				return;
			}
			WebUtils.PrepareEnvelopedResult(out var _writer);
			if (responseType == ResultType.ResultOnly)
			{
				_writer.WriteRaw(jsonRawKey);
				_writer.WriteString(text);
				_writer.WriteEndObject();
			}
			else
			{
				_writer.WriteRaw(jsonCommandKey);
				_writer.WriteString(command);
				_writer.WriteRaw(jsonParametersKey);
				_writer.WriteString(parameters);
				_writer.WriteRaw(jsonResultKey);
				_writer.WriteString(text);
				_writer.WriteEndObject();
			}
			WebUtils.SendEnvelopedResult(context, ref _writer);
		}
		catch (IOException ex)
		{
			if (ex.InnerException is SocketException)
			{
				Log.Warning("[Web] Error in WebCommandResult.SendLines(): Remote host closed connection: " + ex.InnerException.Message);
			}
			else
			{
				Log.Warning($"[Web] Error (IO) in WebCommandResult.SendLines(): {ex}");
			}
		}
		catch (Exception arg)
		{
			Log.Warning($"[Web] Error in WebCommandResult.SendLines(): {arg}");
		}
		finally
		{
			context?.Response?.Close();
		}
	}

	public void SendLine(string _text)
	{
	}

	public void SendLog(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime)
	{
	}

	public void EnableLogLevel(LogType _type, bool _enable)
	{
	}

	public string GetDescription()
	{
		return "WebCommandResult_for_" + command + "_by_" + sourceName;
	}
}
