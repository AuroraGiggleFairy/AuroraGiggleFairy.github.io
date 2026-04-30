using System;
using System.Collections.Generic;
using System.Text;

public class ServerInfoCache
{
	public class FavoritesHistoryKey : IEquatable<FavoritesHistoryKey>
	{
		public readonly string Address;

		public readonly int Port;

		public FavoritesHistoryKey(string _address, int _port)
		{
			if (string.IsNullOrEmpty(_address))
			{
				throw new ArgumentException("Parameter must contain a valid IP", "_address");
			}
			if (_port < 1 || _port > 65535)
			{
				throw new ArgumentException($"Parameter needs to be a valid port (is: {_port})", "_port");
			}
			Address = _address;
			Port = _port;
		}

		public override string ToString()
		{
			return $"{Address}${Port}";
		}

		public static FavoritesHistoryKey FromString(string _input)
		{
			int num = _input.IndexOf('$');
			string address = _input.Substring(0, num);
			int port = int.Parse(_input.Substring(num + 1));
			return new FavoritesHistoryKey(address, port);
		}

		public bool Equals(FavoritesHistoryKey _other)
		{
			if (_other == null)
			{
				return false;
			}
			if (this == _other)
			{
				return true;
			}
			if (string.Equals(Address, _other.Address, StringComparison.OrdinalIgnoreCase))
			{
				return Port == _other.Port;
			}
			return false;
		}

		public override bool Equals(object _obj)
		{
			if (_obj == null)
			{
				return false;
			}
			if (this == _obj)
			{
				return true;
			}
			if (_obj.GetType() == GetType())
			{
				return Equals((FavoritesHistoryKey)_obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (StringComparer.OrdinalIgnoreCase.GetHashCode(Address) * 397) ^ Port;
		}
	}

	public class FavoritesHistoryValue
	{
		public uint LastPlayedTime;

		public bool IsFavorite;

		public FavoritesHistoryValue(uint _lastPlayedTime, bool _isFavorite)
		{
			LastPlayedTime = _lastPlayedTime;
			IsFavorite = _isFavorite;
		}

		public override string ToString()
		{
			return $"{LastPlayedTime}${IsFavorite}";
		}

		public static FavoritesHistoryValue FromString(string _input)
		{
			int num = _input.IndexOf('$');
			string s = _input.Substring(0, num);
			string value = _input.Substring(num + 1);
			uint lastPlayedTime = uint.Parse(s);
			bool isFavorite = bool.Parse(value);
			return new FavoritesHistoryValue(lastPlayedTime, isFavorite);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static ServerInfoCache instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<FavoritesHistoryKey, FavoritesHistoryValue> favoritesHistoryCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public const char elementSeparator = ';';

	[PublicizedFrom(EAccessModifier.Private)]
	public const char fieldSeparator = ':';

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string elementSeparatorString = ';'.ToString();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string fieldSeparatorString = ':'.ToString();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string elementSeparatorEscaped = new string(';', 2);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string fieldSeparatorEscaped = new string(':', 2);

	public static ServerInfoCache Instance => instance ?? (instance = new ServerInfoCache());

	[PublicizedFrom(EAccessModifier.Private)]
	public int serverInfoToPersistentKey(GameServerInfo _gsi)
	{
		return (_gsi.GetValue(GameInfoString.IP) + _gsi.GetValue(GameInfoInt.Port)).GetHashCode();
	}

	public void SavePassword(GameServerInfo _gsi, string _password)
	{
		Dictionary<int, string> passwordCacheList = GetPasswordCacheList();
		int key = serverInfoToPersistentKey(_gsi);
		passwordCacheList[key] = _password;
		GamePrefs.Set(EnumGamePrefs.ServerPasswordCache, DictToString(passwordCacheList));
	}

	public string GetPassword(GameServerInfo _gsi)
	{
		Dictionary<int, string> passwordCacheList = GetPasswordCacheList();
		int key = serverInfoToPersistentKey(_gsi);
		if (!passwordCacheList.TryGetValue(key, out var value))
		{
			return "";
		}
		return value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, string> GetPasswordCacheList()
	{
		try
		{
			return StringToDict(GamePrefs.GetString(EnumGamePrefs.ServerPasswordCache), Convert.ToInt32, [PublicizedFrom(EAccessModifier.Internal)] (string _valueString) => _valueString);
		}
		catch (Exception)
		{
			GamePrefs.Set(EnumGamePrefs.ServerPasswordCache, "");
			return new Dictionary<int, string>();
		}
	}

	public void AddHistory(GameServerInfo _info)
	{
		try
		{
			getFavHistoryCacheList();
			FavoritesHistoryKey keyFromInfo = getKeyFromInfo(_info);
			if (!favoritesHistoryCache.TryGetValue(keyFromInfo, out var value))
			{
				value = new FavoritesHistoryValue(0u, _isFavorite: false);
				favoritesHistoryCache[keyFromInfo] = value;
			}
			value.LastPlayedTime = Utils.CurrentUnixTime;
			saveFavHistoryCacheList();
			Log.Out("[NET] Added server to history: " + keyFromInfo.Address);
		}
		catch (Exception e)
		{
			Log.Error("Could not add server " + _info.GetValue(GameInfoString.IP) + " to history:");
			Log.Exception(e);
		}
	}

	public uint IsHistory(GameServerInfo _info)
	{
		try
		{
			getFavHistoryCacheList();
			FavoritesHistoryKey keyFromInfo = getKeyFromInfo(_info);
			if (keyFromInfo == null)
			{
				Log.Warning("Could not check if server " + _info.GetValue(GameInfoString.IP) + " is in history: Invalid IP/port");
				return 0u;
			}
			FavoritesHistoryValue value;
			return favoritesHistoryCache.TryGetValue(keyFromInfo, out value) ? value.LastPlayedTime : 0u;
		}
		catch (Exception e)
		{
			Log.Error("Could not check if server " + _info.GetValue(GameInfoString.IP) + " is in history:");
			Log.Exception(e);
			return 0u;
		}
	}

	public void ToggleFavorite(GameServerInfo _info)
	{
		try
		{
			getFavHistoryCacheList();
			FavoritesHistoryKey keyFromInfo = getKeyFromInfo(_info);
			if (!favoritesHistoryCache.TryGetValue(keyFromInfo, out var value))
			{
				value = new FavoritesHistoryValue(0u, _isFavorite: false);
				favoritesHistoryCache[keyFromInfo] = value;
			}
			_info.IsFavorite = (value.IsFavorite = !value.IsFavorite);
			if (!value.IsFavorite && value.LastPlayedTime == 0)
			{
				favoritesHistoryCache.Remove(keyFromInfo);
			}
			saveFavHistoryCacheList();
			Log.Out($"[NET] Toggled server favorite: {keyFromInfo.Address} - {value.IsFavorite}");
		}
		catch (Exception e)
		{
			Log.Error("Could not toggle server " + _info.GetValue(GameInfoString.IP) + " favorite:");
			Log.Exception(e);
		}
	}

	public bool IsFavorite(GameServerInfo _info)
	{
		try
		{
			getFavHistoryCacheList();
			FavoritesHistoryKey keyFromInfo = getKeyFromInfo(_info);
			if (keyFromInfo == null)
			{
				Log.Warning("Could not check if server " + _info.GetValue(GameInfoString.IP) + " is favorite: Invalid IP/port");
				return false;
			}
			FavoritesHistoryValue value;
			return favoritesHistoryCache.TryGetValue(keyFromInfo, out value) && value.IsFavorite;
		}
		catch (Exception e)
		{
			Log.Error("Could not check if server " + _info.GetValue(GameInfoString.IP) + " is favorite:");
			Log.Exception(e);
			return false;
		}
	}

	public Dictionary<FavoritesHistoryKey, FavoritesHistoryValue>.Enumerator GetFavoriteServersEnumerator()
	{
		getFavHistoryCacheList();
		return favoritesHistoryCache.GetEnumerator();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FavoritesHistoryKey getKeyFromInfo(GameServerInfo _info)
	{
		string value = _info.GetValue(GameInfoString.IP);
		int value2 = _info.GetValue(GameInfoInt.Port);
		if (string.IsNullOrEmpty(value) || value2 < 1 || value2 > 65535)
		{
			return null;
		}
		return new FavoritesHistoryKey(value, value2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void getFavHistoryCacheList()
	{
		if (favoritesHistoryCache != null)
		{
			return;
		}
		try
		{
			favoritesHistoryCache = StringToDict(GamePrefs.GetString(EnumGamePrefs.ServerHistoryCache), FavoritesHistoryKey.FromString, FavoritesHistoryValue.FromString);
		}
		catch (Exception)
		{
			GamePrefs.Set(EnumGamePrefs.ServerHistoryCache, "");
			favoritesHistoryCache = new Dictionary<FavoritesHistoryKey, FavoritesHistoryValue>();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void saveFavHistoryCacheList()
	{
		GamePrefs.Set(EnumGamePrefs.ServerHistoryCache, DictToString(favoritesHistoryCache));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string escapeString(string _input)
	{
		return _input.Replace(elementSeparatorString, elementSeparatorEscaped).Replace(fieldSeparatorString, fieldSeparatorEscaped);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string unescapeString(string _input)
	{
		return _input.Replace(elementSeparatorEscaped, elementSeparatorString).Replace(fieldSeparatorEscaped, fieldSeparatorString);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string DictToString<TKey, TValue>(Dictionary<TKey, TValue> _dictionary)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<TKey, TValue> item in _dictionary)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(';');
			}
			stringBuilder.Append(escapeString(item.Key.ToString()));
			stringBuilder.Append(':');
			stringBuilder.Append(escapeString(item.Value.ToString()));
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<TKey, TValue> StringToDict<TKey, TValue>(string _input, Func<string, TKey> _keyParser, Func<string, TValue> _valueParser)
	{
		if (string.IsNullOrEmpty(_input))
		{
			return new Dictionary<TKey, TValue>();
		}
		Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(Math.Max(_input.Length >> 5, 16));
		int num = 0;
		while (num < _input.Length)
		{
			int num2 = findNextSeparator(_input, ';', num);
			int num3 = findNextSeparator(_input, ':', num);
			if (num3 >= num2)
			{
				Log.Warning("Invalid cache elementy: No field separator found in '" + _input.Substring(num, num2 - num) + "'");
				num = num2 + 1;
				continue;
			}
			string arg = unescapeString(_input.Substring(num, num3 - num));
			string arg2 = unescapeString(_input.Substring(num3 + 1, num2 - num3 - 1));
			num = num2 + 1;
			TKey val = _keyParser(arg);
			TValue value = _valueParser(arg2);
			if (dictionary.ContainsKey(val))
			{
				Log.Warning($"Cache contains multiple elements for '{val}'");
			}
			dictionary[val] = value;
		}
		return dictionary;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int findNextSeparator(string _input, char _separator, int _startInclusive)
	{
		int num = _input.IndexOf(_separator, _startInclusive);
		while (num >= 0 && num < _input.Length - 1 && _input[num + 1] == _separator)
		{
			num = _input.IndexOf(_separator, num + 2);
		}
		if (num < 0)
		{
			return _input.Length;
		}
		return num;
	}
}
