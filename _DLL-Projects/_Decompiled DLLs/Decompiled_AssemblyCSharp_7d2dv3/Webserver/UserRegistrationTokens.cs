using System;
using System.Collections.Generic;

namespace Webserver;

public static class UserRegistrationTokens
{
	public class RegistrationData
	{
		public readonly string PlayerName;

		public readonly DateTime ExpiryTime;

		public readonly PlatformUserIdentifierAbs PlatformUserId;

		public readonly PlatformUserIdentifierAbs CrossPlatformUserId;

		public RegistrationData(string _playerName, PlatformUserIdentifierAbs _platformUserId, PlatformUserIdentifierAbs _crossPlatformUserId)
		{
			PlayerName = _playerName;
			ExpiryTime = DateTime.Now + TimeSpan.FromMinutes(3.0);
			PlatformUserId = _platformUserId;
			CrossPlatformUserId = _crossPlatformUserId;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float tokenExpirationMinutes = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, RegistrationData> activeTokens = new Dictionary<string, RegistrationData>();

	public static bool TryValidate(string _token, out RegistrationData _data)
	{
		if (activeTokens.TryGetValue(_token, out _data))
		{
			return _data.ExpiryTime > DateTime.Now;
		}
		return false;
	}

	public static string CreateToken(string _playerName, PlatformUserIdentifierAbs _platformUserId, PlatformUserIdentifierAbs _crossPlatformUserId)
	{
		string text = Utils.GenerateGuid();
		DateTime currentTime = DateTime.Now;
		activeTokens.RemoveAll([PublicizedFrom(EAccessModifier.Internal)] (RegistrationData _data) => _data.ExpiryTime < currentTime || _platformUserId.Equals(_data.PlatformUserId) || (_crossPlatformUserId?.Equals(_data.CrossPlatformUserId) ?? false));
		RegistrationData value = new RegistrationData(_playerName, _platformUserId, _crossPlatformUserId);
		activeTokens[text] = value;
		return text;
	}
}
