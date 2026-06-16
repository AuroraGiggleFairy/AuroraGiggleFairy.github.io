using System.Collections.Generic;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;
using Webserver.Permissions;

namespace Webserver.WebAPI.APIs.Permissions;

[Preserve]
public class WebUsers : AbsRestApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyName = "name";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyPassword = "password";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyPlatformUserId = "platformUserId";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyCrossplatformUserId = "crossplatformUserId";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyName = JsonWriter.GetEncodedPropertyNameWithBeginObject("name");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyPlatformUserId = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("platformUserId");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyCrossplatformUserId = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("crossplatformUserId");

	public static AdminWebUsers WebUsersInstance
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return AdminWebUsers.Instance;
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
			foreach (var (_, user) in WebUsersInstance.GetUsers())
			{
				if (!flag)
				{
					_writer.WriteValueSeparator();
				}
				flag = false;
				writeUserJson(ref _writer, user);
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
	public void writeUserJson(ref JsonWriter _writer, AdminWebUsers.WebUser _user)
	{
		_writer.WriteRaw(jsonKeyName);
		_writer.WriteString(_user.Name ?? "");
		_writer.WriteRaw(jsonKeyPlatformUserId);
		JsonCommons.WritePlatformUserIdentifier(ref _writer, _user.PlatformUser);
		_writer.WriteRaw(jsonKeyCrossplatformUserId);
		JsonCommons.WritePlatformUserIdentifier(ref _writer, _user.CrossPlatformUser);
		_writer.WriteEndObject();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestPost(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
	{
		if (!TryParseName(_context, _jsonInputData, out var _userName))
		{
			return;
		}
		if (!JsonCommons.TryGetJsonField(_jsonInput, "password", out string _value))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_OR_INVALID_PASSWORD);
			return;
		}
		if (!JsonCommons.TryGetJsonField(_jsonInput, "platformUserId", out IDictionary<string, object> _value2))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_PLATFORM_USER_ID);
			return;
		}
		if (!JsonCommons.TryReadPlatformUserIdentifier(_value2, out var _userIdentifier))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_PLATFORM_USER_ID);
			return;
		}
		PlatformUserIdentifierAbs _userIdentifier2 = null;
		if (JsonCommons.TryGetJsonField(_jsonInput, "crossplatformUserId", out _value2) && !JsonCommons.TryReadPlatformUserIdentifier(_value2, out _userIdentifier2))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_CROSSPLATFORM_USER_ID);
			return;
		}
		WebUsersInstance.AddUser(_userName, _value, _userIdentifier, _userIdentifier2);
		AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.Created);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestDelete(RequestContext _context)
	{
		if (TryParseName(_context, null, out var _userName))
		{
			bool flag = WebUsersInstance.RemoveUser(_userName);
			AbsRestApi.SendEmptyResponse(_context, flag ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryParseName(RequestContext _context, byte[] _jsonInputData, out string _userName)
	{
		string requestPath = _context.RequestPath;
		_userName = null;
		if (string.IsNullOrEmpty(requestPath))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_USERNAME);
			return false;
		}
		_userName = requestPath;
		return true;
	}

	public override int[] DefaultMethodPermissionLevels()
	{
		return new int[5] { -2147483647, -2147483648, -2147483648, -2147483647, -2147483648 };
	}
}
