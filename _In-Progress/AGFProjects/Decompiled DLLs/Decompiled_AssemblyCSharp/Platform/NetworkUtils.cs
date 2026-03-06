using System;
using System.Net;

namespace Platform;

public static class NetworkUtils
{
	public static uint ToInt(string _addr)
	{
		return (uint)IPAddress.HostToNetworkOrder(BitConverter.ToInt32(IPAddress.Parse(_addr).GetAddressBytes(), 0));
	}

	public static string ToAddr(uint _address)
	{
		string[] array = new IPAddress(_address).ToString().Split('.');
		return array[3] + "." + array[2] + "." + array[1] + "." + array[0];
	}

	public static string BuildGameTags(GameServerInfo _game)
	{
		using PooledMemoryStream pooledMemoryStream = MemoryPools.poolMS.AllocSync(_bReset: true);
		using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: true);
		pooledBinaryWriter.SetBaseStream(pooledMemoryStream);
		GameInfoInt[] intInfosInGameTags = GameServerInfo.IntInfosInGameTags;
		foreach (GameInfoInt key in intInfosInGameTags)
		{
			pooledBinaryWriter.Write7BitEncodedSignedInt(_game.GetValue(key));
		}
		byte b = 0;
		int j;
		for (j = 0; j < GameServerInfo.BoolInfosInGameTags.Length; j++)
		{
			b |= (byte)((_game.GetValue(GameServerInfo.BoolInfosInGameTags[j]) ? 1 : 0) << j % 8);
			if (j % 8 == 7)
			{
				pooledBinaryWriter.Write(b);
				b = 0;
			}
		}
		if (j % 8 != 0)
		{
			pooledBinaryWriter.Write(b);
		}
		return Convert.ToBase64String(pooledMemoryStream.GetBuffer(), 0, (int)pooledMemoryStream.Length);
	}

	public static bool ParseGameTags(string _tags, GameServerInfo _gameInfo)
	{
		if (_tags.IndexOf(';') < 0)
		{
			return ParseGameTags2(_tags, _gameInfo);
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool ParseGameTags2(string _tags, GameServerInfo _gameInfo)
	{
		byte[] array;
		try
		{
			array = Convert.FromBase64String(_tags);
		}
		catch (Exception)
		{
			Log.Warning("Parsing gametags for server " + _gameInfo.GetValue(GameInfoString.IP) + ":" + _gameInfo.GetValue(GameInfoInt.Port) + " failed: \"" + _tags + "\"");
			return false;
		}
		using PooledMemoryStream pooledMemoryStream = MemoryPools.poolMS.AllocSync(_bReset: true);
		pooledMemoryStream.Write(array, 0, array.Length);
		pooledMemoryStream.Position = 0L;
		using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: true);
		pooledBinaryReader.SetBaseStream(pooledMemoryStream);
		try
		{
			GameInfoInt[] intInfosInGameTags = GameServerInfo.IntInfosInGameTags;
			foreach (GameInfoInt key in intInfosInGameTags)
			{
				int value = pooledBinaryReader.Read7BitEncodedSignedInt();
				_gameInfo.SetValue(key, value);
			}
			for (int j = 0; j < GameServerInfo.BoolInfosInGameTags.Length; j += 8)
			{
				byte b = pooledBinaryReader.ReadByte();
				for (int k = 0; j + k < GameServerInfo.BoolInfosInGameTags.Length && k < 8; k++)
				{
					GameInfoBool key2 = GameServerInfo.BoolInfosInGameTags[j + k];
					bool value2 = (b & (1 << k)) != 0;
					_gameInfo.SetValue(key2, value2);
				}
			}
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}
}
