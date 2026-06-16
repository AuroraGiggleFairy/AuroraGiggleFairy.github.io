using System.Collections.Generic;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;

namespace Webserver.WebAPI.APIs.Permissions;

[Preserve]
public class CommandPermissions : AbsRestApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyCommand = "command";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyPermissionLevel = "permissionLevel";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyIsDefault = "default";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyCommand = JsonWriter.GetEncodedPropertyNameWithBeginObject("command");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyPermissionLevel = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("permissionLevel");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyIsDefault = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("default");

	public static AdminCommands CommandsInstance
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameManager.Instance.adminTools.Commands;
		}
	}

	public override bool AllowPostWithId
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestGet(RequestContext _context)
	{
		string requestPath = _context.RequestPath;
		AbsRestApi.PrepareEnvelopedResult(out var _writer);
		if (string.IsNullOrEmpty(requestPath))
		{
			_writer.WriteBeginArray();
			bool flag = true;
			foreach (IConsoleCommand command in SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommands())
			{
				if (!flag)
				{
					_writer.WriteValueSeparator();
				}
				flag = false;
				AdminCommands.CommandPermission commandPermission = CommandsInstance.GetAdminToolsCommandPermission(command.GetCommands());
				bool isDefault = commandPermission.PermissionLevel == command.DefaultPermissionLevel;
				if (commandPermission.Command == "")
				{
					commandPermission = new AdminCommands.CommandPermission(command.GetCommands()[0], commandPermission.PermissionLevel);
				}
				writeCommandJson(ref _writer, commandPermission, isDefault);
			}
			_writer.WriteEndArray();
			AbsRestApi.SendEnvelopedResult(_context, ref _writer);
		}
		else
		{
			_writer.WriteRaw(WebUtils.JsonEmptyData);
			AbsRestApi.SendEnvelopedResult(_context, ref _writer, HttpStatusCode.BadRequest);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeCommandJson(ref JsonWriter _writer, AdminCommands.CommandPermission _commandPermission, bool _isDefault)
	{
		_writer.WriteRaw(jsonKeyCommand);
		_writer.WriteString(_commandPermission.Command);
		_writer.WriteRaw(jsonKeyPermissionLevel);
		_writer.WriteInt32(_commandPermission.PermissionLevel);
		_writer.WriteRaw(jsonKeyIsDefault);
		_writer.WriteBoolean(_isDefault);
		_writer.WriteEndObject();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestPost(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
	{
		string requestPath = _context.RequestPath;
		if (string.IsNullOrEmpty(requestPath))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_COMMAND);
			return;
		}
		IConsoleCommand command = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommand(requestPath);
		if (command == null)
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.NotFound, _jsonInputData, EApiErrorCode.UNKNOWN_COMMAND);
			return;
		}
		if (!JsonCommons.TryGetJsonField(_jsonInput, "permissionLevel", out int _value))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_OR_INVALID_PERMISSION_LEVEL);
			return;
		}
		CommandsInstance.AddCommand(command.GetCommands()[0], _value);
		AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.Created);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestDelete(RequestContext _context)
	{
		string requestPath = _context.RequestPath;
		IConsoleCommand command = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommand(requestPath);
		if (command == null || !CommandsInstance.IsPermissionDefined(command.GetCommands()))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.NotFound);
			return;
		}
		bool flag = CommandsInstance.RemoveCommand(command.GetCommands());
		AbsRestApi.SendEmptyResponse(_context, flag ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
	}

	public override int[] DefaultMethodPermissionLevels()
	{
		return new int[5] { -2147483647, -2147483648, -2147483648, -2147483647, -2147483648 };
	}
}
