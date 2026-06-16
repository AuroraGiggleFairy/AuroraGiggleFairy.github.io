using System.Collections.Generic;
using Utf8Json;
using Webserver.Permissions;

namespace Webserver.UrlHandlers;

public class UserStatusHandler : AbsHandler
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonLoggedInKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonUsernameKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonPermissionLevelKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonPermissionsKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonModuleKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonAllowedKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[][] jsonMethodNameKeys;

	public UserStatusHandler(string _moduleName = null)
		: base(_moduleName)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static UserStatusHandler()
	{
		jsonLoggedInKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("loggedIn");
		jsonUsernameKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("username");
		jsonPermissionLevelKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("permissionLevel");
		jsonPermissionsKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("permissions");
		jsonModuleKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("module");
		jsonAllowedKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("allowed");
		jsonMethodNameKeys = new byte[7][];
		for (int i = 0; i < jsonMethodNameKeys.Length; i++)
		{
			ERequestMethod enumValue = (ERequestMethod)i;
			jsonMethodNameKeys[i] = JsonWriter.GetEncodedPropertyName(enumValue.ToStringCached());
		}
	}

	public override void HandleRequest(RequestContext _context)
	{
		WebUtils.PrepareEnvelopedResult(out var _writer);
		_writer.WriteRaw(jsonLoggedInKey);
		_writer.WriteBoolean(_context.Connection != null);
		_writer.WriteRaw(jsonUsernameKey);
		_writer.WriteString((_context.Connection != null) ? _context.Connection.Username : string.Empty);
		_writer.WriteRaw(jsonPermissionLevelKey);
		_writer.WriteInt32(_context.PermissionLevel);
		_writer.WriteRaw(jsonPermissionsKey);
		_writer.WriteBeginArray();
		List<AdminWebModules.WebModule> modules = AdminWebModules.Instance.GetModules();
		for (int i = 0; i < modules.Count; i++)
		{
			AdminWebModules.WebModule webModule = modules[i];
			if (i > 0)
			{
				_writer.WriteValueSeparator();
			}
			_writer.WriteRaw(jsonModuleKey);
			_writer.WriteString(webModule.Name);
			_writer.WriteRaw(jsonAllowedKey);
			_writer.WriteBeginObject();
			if (webModule.LevelPerMethod == null)
			{
				_writer.WriteRaw(jsonMethodNameKeys[1]);
				_writer.WriteBoolean(webModule.LevelGlobal >= _context.PermissionLevel);
			}
			else
			{
				bool flag = true;
				for (int j = 0; j < webModule.LevelPerMethod.Length; j++)
				{
					int num = webModule.LevelPerMethod[j];
					if (num != -2147483647)
					{
						if (num == int.MinValue)
						{
							num = webModule.LevelGlobal;
						}
						if (!flag)
						{
							_writer.WriteValueSeparator();
						}
						flag = false;
						_writer.WriteRaw(jsonMethodNameKeys[j]);
						_writer.WriteBoolean(num >= _context.PermissionLevel);
					}
				}
			}
			_writer.WriteEndObject();
			_writer.WriteEndObject();
		}
		_writer.WriteEndArray();
		_writer.WriteEndObject();
		WebUtils.SendEnvelopedResult(_context, ref _writer);
	}
}
