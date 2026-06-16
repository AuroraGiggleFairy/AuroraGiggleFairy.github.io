using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;
using Webserver.Permissions;

namespace Webserver.WebAPI.APIs.Permissions;

[Preserve]
public class WebModules : AbsRestApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyModule = "module";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyPermissionLevelGlobal = "permissionLevelGlobal";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyPermissionLevelPerMethod = "permissionLevelPerMethod";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyIsDefault = "isDefault";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyModule;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyPermissionLevelGlobal;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyPermissionLevelPerMethod;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyIsDefault;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[][] jsonMethodNameKeys;

	public static AdminWebModules ModulesInstance
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return AdminWebModules.Instance;
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

	[PublicizedFrom(EAccessModifier.Private)]
	static WebModules()
	{
		jsonKeyModule = JsonWriter.GetEncodedPropertyNameWithBeginObject("module");
		jsonKeyPermissionLevelGlobal = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("permissionLevelGlobal");
		jsonKeyPermissionLevelPerMethod = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("permissionLevelPerMethod");
		jsonKeyIsDefault = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("isDefault");
		jsonMethodNameKeys = new byte[7][];
		for (int i = 0; i < jsonMethodNameKeys.Length; i++)
		{
			ERequestMethod enumValue = (ERequestMethod)i;
			jsonMethodNameKeys[i] = JsonWriter.GetEncodedPropertyName(enumValue.ToStringCached());
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
			foreach (AdminWebModules.WebModule module in ModulesInstance.GetModules())
			{
				if (!flag)
				{
					_writer.WriteValueSeparator();
				}
				flag = false;
				writeModuleJson(ref _writer, module);
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
	public void writeModuleJson(ref JsonWriter _writer, AdminWebModules.WebModule _module)
	{
		_writer.WriteRaw(jsonKeyModule);
		_writer.WriteString(_module.Name);
		_writer.WriteRaw(jsonKeyPermissionLevelGlobal);
		_writer.WriteInt32(_module.LevelGlobal);
		_writer.WriteRaw(jsonKeyPermissionLevelPerMethod);
		_writer.WriteBeginObject();
		if (_module.LevelPerMethod != null)
		{
			bool flag = true;
			for (int i = 0; i < _module.LevelPerMethod.Length; i++)
			{
				int num = _module.LevelPerMethod[i];
				if (num != -2147483647)
				{
					if (!flag)
					{
						_writer.WriteValueSeparator();
					}
					flag = false;
					_writer.WriteRaw(jsonMethodNameKeys[i]);
					if (num == int.MinValue)
					{
						_writer.WriteString("inherit");
					}
					else
					{
						_writer.WriteInt32(num);
					}
				}
			}
		}
		_writer.WriteEndObject();
		_writer.WriteRaw(jsonKeyIsDefault);
		_writer.WriteBoolean(_module.IsDefault);
		_writer.WriteEndObject();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestPost(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
	{
		string requestPath = _context.RequestPath;
		if (string.IsNullOrEmpty(requestPath))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_MODULE);
			return;
		}
		if (!AdminWebModules.Instance.IsKnownModule(requestPath))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_MODULE);
			return;
		}
		AdminWebModules.WebModule module = AdminWebModules.Instance.GetModule(requestPath);
		if (_jsonInput.ContainsKey("permissionLevelGlobal"))
		{
			if (!JsonCommons.TryGetJsonField(_jsonInput, "permissionLevelGlobal", out int _value))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_PERMISSION_LEVEL_GLOBAL);
				return;
			}
			module = module.SetLevelGlobal(_value);
		}
		if (_jsonInput.TryGetValue("permissionLevelPerMethod", out var value))
		{
			if (!(value is IDictionary<string, object> dictionary))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_PERMISSION_LEVEL_PER_METHOD_PROPERTY);
				return;
			}
			foreach (var (name, obj2) in dictionary)
			{
				if (!EnumUtils.TryParse<ERequestMethod>(name, out var _result, _ignoreCase: true))
				{
					AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_METHOD_NAME);
					return;
				}
				if (module.LevelPerMethod == null || module.LevelPerMethod[(int)_result] == -2147483647)
				{
					AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.UNSUPPORTED_METHOD);
					return;
				}
				int level;
				if (obj2 is string a)
				{
					if (!a.EqualsCaseInsensitive("inherit"))
					{
						AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_PERMISSION_STRING);
						return;
					}
					level = int.MinValue;
				}
				else
				{
					if (!(obj2 is double num))
					{
						AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_PERMISSION_VALUE_TYPE);
						return;
					}
					try
					{
						level = (int)num;
					}
					catch (Exception)
					{
						AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_PERMISSION_VALUE);
						return;
					}
				}
				module = module.SetLevelForMethod(_result, level);
			}
		}
		ModulesInstance.AddModule(module);
		AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.Created);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestDelete(RequestContext _context)
	{
		string requestPath = _context.RequestPath;
		bool flag = ModulesInstance.RemoveModule(requestPath);
		AbsRestApi.SendEmptyResponse(_context, flag ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
	}

	public override int[] DefaultMethodPermissionLevels()
	{
		return new int[5] { -2147483647, -2147483648, -2147483648, -2147483647, -2147483648 };
	}
}
