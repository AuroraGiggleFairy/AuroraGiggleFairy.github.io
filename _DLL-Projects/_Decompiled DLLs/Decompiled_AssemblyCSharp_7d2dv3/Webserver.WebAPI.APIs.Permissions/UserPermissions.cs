using System.Collections.Generic;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;

namespace Webserver.WebAPI.APIs.Permissions;

[Preserve]
public class UserPermissions : AbsRestApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyName = "name";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyUserId = "userId";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyPermissionLevel = "permissionLevel";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyGroupId = "groupId";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyPermissionLevelMods = "permissionLevelMods";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string propertyPermissionLevelNormal = "permissionLevelNormal";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyUsers = JsonWriter.GetEncodedPropertyNameWithBeginObject("users");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyGroups = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("groups");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyName = JsonWriter.GetEncodedPropertyNameWithBeginObject("name");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyUserId = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("userId");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyPermissionLevel = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("permissionLevel");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyGroupId = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("groupId");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyPermissionLevelMods = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("permissionLevelMods");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKeyPermissionLevelNormal = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("permissionLevelNormal");

	public static AdminUsers UsersInstance
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameManager.Instance.adminTools.Users;
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
			foreach (var (_, userPermission2) in UsersInstance.GetUsers())
			{
				if (!flag)
				{
					_writer.WriteValueSeparator();
				}
				flag = false;
				writeUserJson(ref _writer, userPermission2);
			}
			_writer.WriteEndArray();
			_writer.WriteRaw(jsonKeyGroups);
			_writer.WriteBeginArray();
			flag = true;
			foreach (var (_, groupPermission2) in UsersInstance.GetGroups())
			{
				if (!flag)
				{
					_writer.WriteValueSeparator();
				}
				flag = false;
				writeGroupJson(ref _writer, groupPermission2);
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
	public void writeUserJson(ref JsonWriter _writer, AdminUsers.UserPermission _userPermission)
	{
		_writer.WriteRaw(jsonKeyName);
		_writer.WriteString(_userPermission.Name ?? "");
		_writer.WriteRaw(jsonKeyUserId);
		JsonCommons.WritePlatformUserIdentifier(ref _writer, _userPermission.UserIdentifier);
		_writer.WriteRaw(jsonKeyPermissionLevel);
		_writer.WriteInt32(_userPermission.PermissionLevel);
		_writer.WriteEndObject();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeGroupJson(ref JsonWriter _writer, AdminUsers.GroupPermission _groupPermission)
	{
		_writer.WriteRaw(jsonKeyName);
		_writer.WriteString(_groupPermission.Name ?? "");
		_writer.WriteRaw(jsonKeyGroupId);
		_writer.WriteString(_groupPermission.SteamIdGroup);
		_writer.WriteRaw(jsonKeyPermissionLevelMods);
		_writer.WriteInt32(_groupPermission.PermissionLevelMods);
		_writer.WriteRaw(jsonKeyPermissionLevelNormal);
		_writer.WriteInt32(_groupPermission.PermissionLevelNormal);
		_writer.WriteEndObject();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestPost(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
	{
		if (!PermissionsApiHelpers.TryParseId(_context, _jsonInputData, out var _userId, out var _groupId))
		{
			return;
		}
		if (_userId != null)
		{
			if (!JsonCommons.TryGetJsonField(_jsonInput, "permissionLevel", out int _value))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_OR_INVALID_PERMISSION_LEVEL);
				return;
			}
			JsonCommons.TryGetJsonField(_jsonInput, "name", out string _value2);
			UsersInstance.AddUser(_value2, _userId, _value);
		}
		else
		{
			if (!JsonCommons.TryGetJsonField(_jsonInput, "permissionLevelMods", out int _value3))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_OR_INVALID_PERMISSION_LEVEL_MODS);
				return;
			}
			if (!JsonCommons.TryGetJsonField(_jsonInput, "permissionLevelNormal", out int _value4))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_OR_INVALID_PERMISSION_LEVEL_NORMAL);
				return;
			}
			JsonCommons.TryGetJsonField(_jsonInput, "name", out string _value5);
			UsersInstance.AddGroup(_value5, _groupId, _value4, _value3);
		}
		AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.Created);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestDelete(RequestContext _context)
	{
		if (PermissionsApiHelpers.TryParseId(_context, null, out var _userId, out var _groupId))
		{
			bool flag = ((_userId != null) ? UsersInstance.RemoveUser(_userId) : UsersInstance.RemoveGroup(_groupId));
			AbsRestApi.SendEmptyResponse(_context, flag ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
		}
	}

	public override int[] DefaultMethodPermissionLevels()
	{
		return new int[5] { -2147483647, -2147483648, -2147483648, -2147483647, -2147483648 };
	}
}
