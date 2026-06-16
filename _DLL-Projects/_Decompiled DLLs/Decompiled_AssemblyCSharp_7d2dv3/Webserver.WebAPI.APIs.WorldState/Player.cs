using System;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;
using Webserver.Permissions;

namespace Webserver.WebAPI.APIs.WorldState;

[Preserve]
public class Player : AbsRestApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonPlayersKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("players");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonEntityIdKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("entityId");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonNameKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("name");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonPlatformIdKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("platformId");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonCrossplatformIdKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("crossplatformId");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonTotalPlayTimeKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("totalPlayTimeSeconds");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonLastOnlineKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("lastOnline");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonOnlineKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("online");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonIpKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("ip");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonPingKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("ping");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonPositionKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("position");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonLevelKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("level");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonHealthKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("health");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonStaminaKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("stamina");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonScoreKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("score");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonDeathsKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("deaths");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKillsKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("kills");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKillsZombiesKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("zombies");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonKillsPlayersKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("players");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonBannedKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("banned");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonBanActiveKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("banActive");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonBanReasonKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("reason");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] jsonBanUntilKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("until");

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestGet(RequestContext _context)
	{
		string requestPath = _context.RequestPath;
		bool allowViewAll = PermissionUtils.CanViewAllPlayers(_context.PermissionLevel);
		PlatformUserIdentifierAbs requesterNativeUserId = _context.Connection?.UserId;
		AbsRestApi.PrepareEnvelopedResult(out var _writer);
		_writer.WriteRaw(jsonPlayersKey);
		_writer.WriteBeginArray();
		int _written = 0;
		if (string.IsNullOrEmpty(requestPath))
		{
			for (int i = 0; i < SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List.Count; i++)
			{
				ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List[i];
				writePlayerJson(ref _writer, ref _written, clientInfo.PlatformId, allowViewAll, requesterNativeUserId);
			}
		}
		else
		{
			ClientInfo clientInfo2;
			if (!int.TryParse(requestPath, out var result) || (clientInfo2 = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(result)) == null)
			{
				_writer.WriteEndArray();
				_writer.WriteEndObject();
				AbsRestApi.SendEnvelopedResult(_context, ref _writer, HttpStatusCode.NotFound);
				return;
			}
			writePlayerJson(ref _writer, ref _written, clientInfo2.PlatformId, allowViewAll, requesterNativeUserId);
		}
		_writer.WriteEndArray();
		_writer.WriteEndObject();
		AbsRestApi.SendEnvelopedResult(_context, ref _writer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writePlayerJson(ref JsonWriter _writer, ref int _written, PlatformUserIdentifierAbs _nativeUserId, bool _allowViewAll, PlatformUserIdentifierAbs _requesterNativeUserId)
	{
		if (!_allowViewAll && (_requesterNativeUserId == null || !_requesterNativeUserId.Equals(_nativeUserId)))
		{
			return;
		}
		ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForUserId(_nativeUserId);
		if (clientInfo == null)
		{
			Log.Warning("[Web] Player.GET: ClientInfo null");
			return;
		}
		int entityId = clientInfo.entityId;
		GameManager.Instance.World.Players.dict.TryGetValue(entityId, out var value);
		if (value == null)
		{
			Log.Warning("[Web] Player.GET: EntityPlayer null");
			return;
		}
		if (_written > 0)
		{
			_writer.WriteValueSeparator();
		}
		_written++;
		bool flag = true;
		_writer.WriteRaw(jsonEntityIdKey);
		_writer.WriteInt32(entityId);
		_writer.WriteRaw(jsonNameKey);
		_writer.WriteString(clientInfo.playerName);
		_writer.WriteRaw(jsonPlatformIdKey);
		JsonCommons.WritePlatformUserIdentifier(ref _writer, _nativeUserId);
		_writer.WriteRaw(jsonCrossplatformIdKey);
		JsonCommons.WritePlatformUserIdentifier(ref _writer, clientInfo.CrossplatformId);
		_writer.WriteRaw(jsonTotalPlayTimeKey);
		_writer.WriteNull();
		_writer.WriteRaw(jsonLastOnlineKey);
		_writer.WriteNull();
		_writer.WriteRaw(jsonOnlineKey);
		_writer.WriteBoolean(flag);
		_writer.WriteRaw(jsonIpKey);
		if (flag)
		{
			_writer.WriteString(clientInfo.ip);
		}
		else
		{
			_writer.WriteNull();
		}
		_writer.WriteRaw(jsonPingKey);
		if (flag)
		{
			_writer.WriteInt32(clientInfo.ping);
		}
		else
		{
			_writer.WriteNull();
		}
		_writer.WriteRaw(jsonPositionKey);
		if (flag)
		{
			JsonCommons.WriteVector3(ref _writer, value.GetPosition());
		}
		else
		{
			_writer.WriteNull();
		}
		_writer.WriteRaw(jsonLevelKey);
		_writer.WriteInt32(value.Progression.Level);
		_writer.WriteRaw(jsonHealthKey);
		_writer.WriteInt32(value.Health);
		_writer.WriteRaw(jsonStaminaKey);
		_writer.WriteSingle(value.Stamina);
		_writer.WriteRaw(jsonScoreKey);
		_writer.WriteInt32(value.Score);
		_writer.WriteRaw(jsonDeathsKey);
		_writer.WriteInt32(value.Died);
		_writer.WriteRaw(jsonKillsKey);
		_writer.WriteRaw(jsonKillsZombiesKey);
		_writer.WriteInt32(value.KilledZombies);
		_writer.WriteRaw(jsonKillsPlayersKey);
		_writer.WriteInt32(value.KilledPlayers);
		_writer.WriteEndObject();
		_writer.WriteRaw(jsonBannedKey);
		DateTime _bannedUntil;
		string _reason;
		bool flag2 = GameManager.Instance.adminTools.Blacklist.IsBanned(_nativeUserId, out _bannedUntil, out _reason);
		if (!flag2 && clientInfo.CrossplatformId != null)
		{
			flag2 = GameManager.Instance.adminTools.Blacklist.IsBanned(clientInfo.CrossplatformId, out _bannedUntil, out _reason);
		}
		_writer.WriteRaw(jsonBanActiveKey);
		_writer.WriteBoolean(flag2);
		_writer.WriteRaw(jsonBanReasonKey);
		if (flag2)
		{
			_writer.WriteString(_reason);
		}
		else
		{
			_writer.WriteNull();
		}
		_writer.WriteRaw(jsonBanUntilKey);
		if (flag2)
		{
			JsonCommons.WriteDateTime(ref _writer, _bannedUntil);
		}
		else
		{
			_writer.WriteNull();
		}
		_writer.WriteEndObject();
		_writer.WriteEndObject();
	}

	public override int DefaultPermissionLevel()
	{
		return 2000;
	}
}
