using System;
using System.Collections.Generic;

namespace Platform.LAN;

public class LANServerCacheControl
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct ServerKey(string _ipAddress, int _port) : IEquatable<ServerKey>
	{
		public readonly string ipAddress = _ipAddress;

		public readonly int port = _port;

		public override int GetHashCode()
		{
			return ipAddress.GetHashCode() ^ port;
		}

		public bool Equals(ServerKey _other)
		{
			if (ipAddress.Equals(_other.ipAddress))
			{
				return port == _other.port;
			}
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct UpdateTimes
	{
		public DateTime lastServerCheckedTime;

		public DateTime lastRulesUpdateTime;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TimeSpan timeout;

	[PublicizedFrom(EAccessModifier.Private)]
	public TimeSpan updateInterval;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<ServerKey, UpdateTimes> lastServerUpdateTimes = new Dictionary<ServerKey, UpdateTimes>();

	public LANServerCacheControl(TimeSpan updateInterval, TimeSpan timeout)
	{
		this.timeout = timeout;
		this.updateInterval = updateInterval;
	}

	public bool IsUpdateRequired(string addressString, int port)
	{
		ServerKey key = new ServerKey(addressString, port);
		if (lastServerUpdateTimes.TryGetValue(key, out var value))
		{
			TimeSpan timeSpan = DateTime.Now - value.lastServerCheckedTime;
			value.lastServerCheckedTime = DateTime.Now;
			lastServerUpdateTimes[key] = value;
			if (timeSpan > timeout)
			{
				Log.Out(string.Format("[{0}] server timed out, update needed. Last checked {1:F2} seconds ago", "LANServerCacheControl", timeSpan.TotalSeconds));
				return true;
			}
			TimeSpan timeSpan2 = DateTime.Now - value.lastRulesUpdateTime;
			if (timeSpan2 > updateInterval)
			{
				Log.Out(string.Format("[{0}] found known server, last updated {1:F2} seconds ago", "LANServerCacheControl", timeSpan2.TotalSeconds));
				return true;
			}
			return false;
		}
		lastServerUpdateTimes[key] = new UpdateTimes
		{
			lastServerCheckedTime = DateTime.Now
		};
		return true;
	}

	public void SetUpdated(string addressString, int port)
	{
		ServerKey key = new ServerKey(addressString, port);
		if (!lastServerUpdateTimes.TryGetValue(key, out var value))
		{
			lastServerUpdateTimes[key] = new UpdateTimes
			{
				lastServerCheckedTime = DateTime.Now,
				lastRulesUpdateTime = DateTime.Now
			};
		}
		else
		{
			value.lastRulesUpdateTime = DateTime.Now;
			lastServerUpdateTimes[key] = value;
		}
	}

	public void Clear()
	{
		lastServerUpdateTimes.Clear();
	}
}
