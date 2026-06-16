using System;
using System.Net.Sockets;
using System.Threading.Tasks;

[PublicizedFrom(EAccessModifier.Internal)]
public class ServerDateTimeRequest
{
	public static void GetNtpTimeAsync(Action<ServerDateTimeResult> _onComplete, string _ntpServer = "pool.ntp.org", int _timeoutMilliseconds = 5000)
	{
		Task.Run([PublicizedFrom(EAccessModifier.Internal)] async () =>
		{
			try
			{
				ServerDateTimeResult obj = await FetchNtpTimeAsync(_ntpServer, _timeoutMilliseconds);
				_onComplete?.Invoke(obj);
			}
			catch
			{
				_onComplete?.Invoke(new ServerDateTimeResult(_requestComplete: true, _hasError: true, 0));
			}
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static async Task<ServerDateTimeResult> FetchNtpTimeAsync(string _ntpServer, int _timeoutMilliseconds)
	{
		byte[] array = new byte[48];
		int num = 0;
		int num2 = 3;
		int num3 = 3;
		array[0] = (byte)((num << 6) | (num2 << 3) | num3);
		using (UdpClient client = new UdpClient())
		{
			_ = 1;
			try
			{
				client.Client.ReceiveTimeout = _timeoutMilliseconds;
				await client.SendAsync(array, array.Length, _ntpServer, 123);
				array = (await client.ReceiveAsync()).Buffer;
			}
			catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
			{
				Log.Warning($"Request to NTP server '{_ntpServer}' timed out after {_timeoutMilliseconds} ms.", ex);
				return new ServerDateTimeResult(_requestComplete: true, _hasError: true, 0);
			}
			catch (Exception ex2)
			{
				Log.Error("Network error while communicating with NTP server '" + _ntpServer + "': " + ex2.Message, ex2);
				return new ServerDateTimeResult(_requestComplete: true, _hasError: true, 0);
			}
		}
		try
		{
			ulong x = BitConverter.ToUInt32(array, 40);
			long x2 = BitConverter.ToUInt32(array, 44);
			x = SwapEndianness(x);
			long num4 = SwapEndianness((ulong)x2);
			ulong num5 = x - 2208988800u;
			double value = (double)(ulong)num4 * 1000.0 / 4294967296.0;
			return new ServerDateTimeResult(_requestComplete: true, _hasError: false, (int)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(num5).AddMilliseconds(value).ToLocalTime()).TotalSeconds);
		}
		catch
		{
			Log.Error("Error parsing the NTP server response.");
			return new ServerDateTimeResult(_requestComplete: true, _hasError: true, 0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static uint SwapEndianness(ulong _x)
	{
		return (uint)(((_x & 0xFF) << 24) + ((_x & 0xFF00) << 8) + ((_x & 0xFF0000) >> 8) + ((_x & 0xFF000000u) >> 24));
	}
}
