using System.Collections.Generic;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;

namespace Webserver.WebAPI.APIs.Permissions;

[Preserve]
public class Whitelist : AbsRestApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyName = "name";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyUserId = "userId";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyGroupId = "groupId";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyUsers = JsonWriter.GetEncodedPropertyNameWithBeginObject("users");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyGroups = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("groups");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyName = JsonWriter.GetEncodedPropertyNameWithBeginObject("name");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyUserId = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("userId");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyGroupId = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("groupId");

	public static AdminWhitelist WhitelistInstance
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameManager.Instance.adminTools.Whitelist;
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
			_writer.WriteRaw(jsonKeyUsers);
			_writer.WriteBeginArray();
			bool flag = true;
			foreach (var (_, userPermission) in WhitelistInstance.GetUsers())
			{
				if (!flag)
				{
					_writer.WriteValueSeparator();
				}
				flag = false;
				writeUserJson(ref _writer, userPermission);
			}
			_writer.WriteEndArray();
			_writer.WriteRaw(jsonKeyGroups);
			_writer.WriteBeginArray();
			flag = true;
			foreach (var (_, groupPermission) in WhitelistInstance.GetGroups())
			{
				if (!flag)
				{
					_writer.WriteValueSeparator();
				}
				flag = false;
				writeGroupJson(ref _writer, groupPermission);
			}
			_writer.WriteEndArray();
			_writer.WriteEndObject();
			AbsRestApi.SendEnvelopedResult(_context, ref _writer);
		}
		else
		{
			_writer.WriteRaw(WebUtils.JsonEmptyData);
			AbsRestApi.SendEnvelopedResult(_context, ref _writer, HttpStatusCode.BadRequest);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeUserJson(ref JsonWriter _writer, AdminWhitelist.WhitelistUser _userPermission)
	{
		_writer.WriteRaw(jsonKeyName);
		_writer.WriteString(_userPermission.Name ?? "");
		_writer.WriteRaw(jsonKeyUserId);
		JsonCommons.WritePlatformUserIdentifier(ref _writer, _userPermission.UserIdentifier);
		_writer.WriteEndObject();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeGroupJson(ref JsonWriter _writer, AdminWhitelist.WhitelistGroup _groupPermission)
	{
		_writer.WriteRaw(jsonKeyName);
		_writer.WriteString(_groupPermission.Name ?? "");
		_writer.WriteRaw(jsonKeyGroupId);
		_writer.WriteString(_groupPermission.SteamIdGroup);
		_writer.WriteEndObject();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestPost(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
	{
		if (PermissionsApiHelpers.TryParseId(_context, _jsonInputData, out var _userId, out var _groupId))
		{
			JsonCommons.TryGetJsonField(_jsonInput, "name", out string _value);
			if (_userId != null)
			{
				WhitelistInstance.AddUser(_value, _userId);
			}
			else
			{
				WhitelistInstance.AddGroup(_value, _groupId);
			}
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.Created);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestDelete(RequestContext _context)
	{
		if (PermissionsApiHelpers.TryParseId(_context, null, out var _userId, out var _groupId))
		{
			bool flag = ((_userId != null) ? WhitelistInstance.RemoveUser(_userId) : WhitelistInstance.RemoveGroup(_groupId));
			AbsRestApi.SendEmptyResponse(_context, flag ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
		}
	}

	public override int[] DefaultMethodPermissionLevels()
	{
		return new int[5] { -2147483647, -2147483648, -2147483648, -2147483647, -2147483648 };
	}
}
