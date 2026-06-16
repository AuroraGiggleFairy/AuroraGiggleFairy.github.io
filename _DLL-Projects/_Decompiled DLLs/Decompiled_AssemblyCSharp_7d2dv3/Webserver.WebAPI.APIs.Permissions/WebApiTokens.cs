using System.Collections.Generic;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;
using Webserver.Permissions;

namespace Webserver.WebAPI.APIs.Permissions;

[Preserve]
public class WebApiTokens : AbsRestApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyName = "name";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertySecret = "secret";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyPermissionLevel = "permissionLevel";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyName = JsonWriter.GetEncodedPropertyNameWithBeginObject("name");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeySecret = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("secret");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyPermissionLevel = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("permissionLevel");

	public static AdminApiTokens ApiTokensInstance
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return AdminApiTokens.Instance;
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
			foreach (var (_, token) in ApiTokensInstance.GetTokens())
			{
				if (!flag)
				{
					_writer.WriteValueSeparator();
				}
				flag = false;
				writeTokenJson(ref _writer, token);
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
	public void writeTokenJson(ref JsonWriter _writer, AdminApiTokens.ApiToken _token)
	{
		_writer.WriteRaw(jsonKeyName);
		_writer.WriteString(_token.Name);
		_writer.WriteRaw(jsonKeySecret);
		_writer.WriteString(_token.Secret);
		_writer.WriteRaw(jsonKeyPermissionLevel);
		_writer.WriteInt32(_token.PermissionLevel);
		_writer.WriteEndObject();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestPost(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
	{
		string requestPath = _context.RequestPath;
		if (string.IsNullOrEmpty(requestPath))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_NAME);
			return;
		}
		if (!JsonCommons.TryGetJsonField(_jsonInput, "secret", out string _value))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_OR_INVALID_SECRET);
			return;
		}
		if (!JsonCommons.TryGetJsonField(_jsonInput, "permissionLevel", out int _value2))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_OR_INVALID_PERMISSION_LEVEL);
			return;
		}
		ApiTokensInstance.AddToken(requestPath, _value, _value2);
		AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.Created);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestDelete(RequestContext _context)
	{
		string requestPath = _context.RequestPath;
		bool flag = ApiTokensInstance.RemoveToken(requestPath);
		AbsRestApi.SendEmptyResponse(_context, flag ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
	}

	public override int[] DefaultMethodPermissionLevels()
	{
		return new int[5] { -2147483647, -2147483648, -2147483648, -2147483647, -2147483648 };
	}
}
