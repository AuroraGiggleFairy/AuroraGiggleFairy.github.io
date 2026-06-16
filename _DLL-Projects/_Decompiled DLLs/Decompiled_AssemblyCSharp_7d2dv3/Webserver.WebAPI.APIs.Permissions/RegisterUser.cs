using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.Scripting;
using Utf8Json;
using Webserver.Permissions;
using Webserver.UrlHandlers;

namespace Webserver.WebAPI.APIs.Permissions;

[Preserve]
public class RegisterUser : AbsRestApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonPlayerNameKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("playerName");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonExpirationKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("expirationSeconds");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex userValidationRegex = new Regex("^\\w{4,16}$", RegexOptions.Compiled | RegexOptions.ECMAScript);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex passValidationRegex = new Regex("^\\w{4,16}$", RegexOptions.Compiled | RegexOptions.ECMAScript);

	public RegisterUser(Web _parentWeb)
		: base(_parentWeb)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestGet(RequestContext _context)
	{
		string requestPath = _context.RequestPath;
		if (string.IsNullOrEmpty(requestPath))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, null, EApiErrorCode.MISSING_TOKEN);
			return;
		}
		if (!UserRegistrationTokens.TryValidate(requestPath, out var _data))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.NotFound, null, EApiErrorCode.INVALID_OR_EXPIRED_TOKEN);
			return;
		}
		AbsRestApi.PrepareEnvelopedResult(out var _writer);
		_writer.WriteRaw(jsonPlayerNameKey);
		_writer.WriteString(_data.PlayerName);
		_writer.WriteRaw(jsonExpirationKey);
		_writer.WriteDouble((_data.ExpiryTime - DateTime.Now).TotalSeconds);
		_writer.WriteEndObject();
		AbsRestApi.SendEnvelopedResult(_context, ref _writer);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestPost(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
	{
		if (!JsonCommons.TryGetJsonField(_jsonInput, "token", out string _value))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.MISSING_TOKEN);
			return;
		}
		if (!JsonCommons.TryGetJsonField(_jsonInput, "username", out string _value2))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.MISSING_USERNAME);
			return;
		}
		if (!JsonCommons.TryGetJsonField(_jsonInput, "password", out string _value3))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.MISSING_PASSWORD);
			return;
		}
		if (!UserRegistrationTokens.TryValidate(_value, out var _data))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.Unauthorized, null, EApiErrorCode.INVALID_OR_EXPIRED_TOKEN);
			return;
		}
		if (!userValidationRegex.IsMatch(_value2))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.Unauthorized, _jsonInputData, EApiErrorCode.INVALID_USERNAME);
			return;
		}
		if (!passValidationRegex.IsMatch(_value3))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.Unauthorized, _jsonInputData, EApiErrorCode.INVALID_PASSWORD);
			return;
		}
		if (AdminWebUsers.Instance.GetUsers().TryGetValue(_value2, out var value) && (!object.Equals(value.PlatformUser, _data.PlatformUserId) || !object.Equals(value.CrossPlatformUser, _data.CrossPlatformUserId)))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.Unauthorized, _jsonInputData, EApiErrorCode.DUPLICATE_USERNAME);
			return;
		}
		string text = ((_data.CrossPlatformUserId == null) ? "" : (", crossplatform ID " + _data.CrossPlatformUserId.CombinedString));
		Log.Out("[Web] User registered: Username '" + _value2 + "' for platform ID " + _data.PlatformUserId.CombinedString + text);
		if (AdminWebUsers.Instance.HasUser(_data.PlatformUserId, _data.CrossPlatformUserId, out var _result))
		{
			Log.Out("[Web] Re-registration, replacing existing username '" + _result.Name + "'");
			AdminWebUsers.Instance.RemoveUser(_result.Name);
		}
		AdminWebUsers.Instance.AddUser(_value2, _value3, _data.PlatformUserId, _data.CrossPlatformUserId);
		string remoteEndpointString = _context.Request.RemoteEndPoint.ToString();
		SessionHandler.HandleUserIdLogin(ParentWeb.ConnectionHandler, _context, remoteEndpointString, "User/pass", _value2, _data.PlatformUserId, _data.CrossPlatformUserId);
		_context.Response.StatusCode = 201;
		_context.Response.ContentType = "text/plain";
		_context.Response.ContentEncoding = Encoding.UTF8;
		_context.Response.ContentLength64 = 0L;
	}

	public override int DefaultPermissionLevel()
	{
		return 2000;
	}
}
