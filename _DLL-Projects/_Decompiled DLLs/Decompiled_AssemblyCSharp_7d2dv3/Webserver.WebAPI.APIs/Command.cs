using System.Collections.Generic;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;

namespace Webserver.WebAPI.APIs;

[Preserve]
public class Command : AbsRestApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonCommandsKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("commands");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonOverloadsKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("overloads");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonCommandKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("command");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonDescriptionKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("description");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonHelpKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("help");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonAllowedKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("allowed");

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestGet(RequestContext _context)
	{
		string requestPath = _context.RequestPath;
		int permissionLevel = _context.PermissionLevel;
		AbsRestApi.PrepareEnvelopedResult(out var _writer);
		_writer.WriteRaw(jsonCommandsKey);
		_writer.WriteBeginArray();
		if (string.IsNullOrEmpty(requestPath))
		{
			IList<IConsoleCommand> commands = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommands();
			for (int i = 0; i < commands.Count; i++)
			{
				IConsoleCommand command = commands[i];
				if (i > 0)
				{
					_writer.WriteValueSeparator();
				}
				writeCommandJson(ref _writer, command, permissionLevel);
			}
		}
		else
		{
			IConsoleCommand command2 = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommand(requestPath);
			if (command2 == null)
			{
				_writer.WriteEndArray();
				_writer.WriteEndObject();
				AbsRestApi.SendEnvelopedResult(_context, ref _writer, HttpStatusCode.NotFound);
				return;
			}
			writeCommandJson(ref _writer, command2, permissionLevel);
		}
		_writer.WriteEndArray();
		_writer.WriteEndObject();
		AbsRestApi.SendEnvelopedResult(_context, ref _writer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeCommandJson(ref JsonWriter _writer, IConsoleCommand _command, int _userPermissionLevel)
	{
		_writer.WriteRaw(jsonOverloadsKey);
		_writer.WriteBeginArray();
		string text = string.Empty;
		string[] commands = _command.GetCommands();
		for (int i = 0; i < commands.Length; i++)
		{
			string text2 = commands[i];
			if (i > 0)
			{
				_writer.WriteValueSeparator();
			}
			_writer.WriteString(text2);
			if (text2.Length > text.Length)
			{
				text = text2;
			}
		}
		_writer.WriteEndArray();
		_writer.WriteRaw(jsonCommandKey);
		_writer.WriteString(text);
		_writer.WriteRaw(jsonDescriptionKey);
		_writer.WriteString(_command.GetDescription());
		_writer.WriteRaw(jsonHelpKey);
		_writer.WriteString(_command.GetHelp());
		int commandPermissionLevel = GameManager.Instance.adminTools.Commands.GetCommandPermissionLevel(_command.GetCommands());
		_writer.WriteRaw(jsonAllowedKey);
		_writer.WriteBoolean(_userPermissionLevel <= commandPermissionLevel);
		_writer.WriteEndObject();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestPost(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
	{
		if (!JsonCommons.TryGetJsonField(_jsonInput, "command", out string _value))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_COMMAND);
			return;
		}
		WebCommandResult.ResultType responseType = WebCommandResult.ResultType.Full;
		if (JsonCommons.TryGetJsonField(_jsonInput, "format", out string _value2))
		{
			if (_value2.EqualsCaseInsensitive("raw"))
			{
				responseType = WebCommandResult.ResultType.Raw;
			}
			else if (_value2.EqualsCaseInsensitive("simple"))
			{
				responseType = WebCommandResult.ResultType.ResultOnly;
			}
		}
		int num = _value.IndexOf(' ');
		string text = ((num > 0) ? _value.Substring(0, num) : _value);
		string parameters = ((num > 0) ? _value.Substring(text.Length + 1) : "");
		IConsoleCommand command = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommand(text, _alreadyTokenized: true);
		if (command == null)
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.NotFound, _jsonInputData, EApiErrorCode.UNKNOWN_COMMAND);
			return;
		}
		int commandPermissionLevel = GameManager.Instance.adminTools.Commands.GetCommandPermissionLevel(command.GetCommands());
		if (_context.PermissionLevel > commandPermissionLevel)
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.Forbidden, _jsonInputData, EApiErrorCode.NO_PERMISSION);
			return;
		}
		_context.Response.SendChunked = true;
		WebCommandResult sender = new WebCommandResult(text, parameters, responseType, _context);
		SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteAsync(_value, sender);
	}

	public override int DefaultPermissionLevel()
	{
		return 1000;
	}

	public override int[] DefaultMethodPermissionLevels()
	{
		return new int[5] { -2147483647, 2000, -2147483648, -2147483647, -2147483647 };
	}
}
