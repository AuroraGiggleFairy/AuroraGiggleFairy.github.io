using System.Collections.Generic;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;

namespace Webserver.WebAPI.APIs.Permissions;

[Preserve]
public class Blacklist : AbsRestApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyName = "name";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyUserId = "userId";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyBannedUntil = "bannedUntil";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyBanReason = "banReason";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyName = JsonWriter.GetEncodedPropertyNameWithBeginObject("name");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyUserId = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("userId");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyBannedUntil = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("bannedUntil");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyBanReason = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("banReason");

	public static AdminBlacklist BlacklistInstance
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameManager.Instance.adminTools.Blacklist;
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
			foreach (AdminBlacklist.BannedUser item in BlacklistInstance.GetBanned())
			{
				if (!flag)
				{
					_writer.WriteValueSeparator();
				}
				flag = false;
				writeBan(ref _writer, item);
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
	public void writeBan(ref JsonWriter _writer, AdminBlacklist.BannedUser _ban)
	{
		_writer.WriteRaw(jsonKeyName);
		_writer.WriteString(_ban.Name ?? "");
		_writer.WriteRaw(jsonKeyUserId);
		JsonCommons.WritePlatformUserIdentifier(ref _writer, _ban.UserIdentifier);
		_writer.WriteRaw(jsonKeyBannedUntil);
		JsonCommons.WriteDateTime(ref _writer, _ban.BannedUntil);
		_writer.WriteRaw(jsonKeyBanReason);
		_writer.WriteString(_ban.BanReason);
		_writer.WriteEndObject();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestPost(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
	{
		if (TryParseId(_context, _jsonInputData, out var _userId))
		{
			if (!JsonCommons.TryReadDateTime(_jsonInput, "bannedUntil", out var _result))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_OR_INVALID_BANNED_UNTIL);
				return;
			}
			JsonCommons.TryGetJsonField(_jsonInput, "banReason", out string _value);
			JsonCommons.TryGetJsonField(_jsonInput, "name", out string _value2);
			BlacklistInstance.AddBan(_value2, _userId, _result, _value);
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.Created);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestDelete(RequestContext _context)
	{
		if (TryParseId(_context, null, out var _userId))
		{
			bool flag = BlacklistInstance.RemoveBan(_userId);
			AbsRestApi.SendEmptyResponse(_context, flag ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryParseId(RequestContext _context, byte[] _jsonInputData, out PlatformUserIdentifierAbs _userId)
	{
		string requestPath = _context.RequestPath;
		_userId = null;
		if (string.IsNullOrEmpty(requestPath))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_USER);
			return false;
		}
		if (!PlatformUserIdentifierAbs.TryFromCombinedString(requestPath, out _userId))
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.INVALID_USER);
			return false;
		}
		return true;
	}

	public override int[] DefaultMethodPermissionLevels()
	{
		return new int[5] { -2147483647, -2147483648, -2147483648, -2147483647, -2147483648 };
	}
}
