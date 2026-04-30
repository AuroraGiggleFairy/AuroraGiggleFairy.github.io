using System;
using System.IO;
using UnityEngine;

public static class NetPackageLogger
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static int opened;

	[PublicizedFrom(EAccessModifier.Private)]
	public static StreamWriter logFileStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool logEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string logFilePathPrefix;

	public static void Init()
	{
		if (logFilePathPrefix == null)
		{
			if (string.IsNullOrEmpty(Application.consoleLogPath))
			{
				logFilePathPrefix = "";
				logEnabled = false;
			}
			else
			{
				logFilePathPrefix = Path.GetDirectoryName(Application.consoleLogPath) + "/netpackages_";
				logEnabled = GameUtils.GetLaunchArgument("debugpackages") != null;
			}
		}
	}

	public static void BeginLog(bool _asServer)
	{
		if (logEnabled)
		{
			opened++;
			if (logFileStream == null)
			{
				logFileStream = SdFile.CreateText(logFilePathPrefix + (_asServer ? "S_" : "C_") + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv");
				logFileStream.WriteLine("Time,Dir,Src/Tgt,PackageType,Chn,Len,Encrypted,Compressed,Pkg# in Msg,Pkgs in Msg");
			}
		}
	}

	public static void LogPackage(bool _dirIsOut, ClientInfo _clientInfo, NetPackage _packageType, int _channel, int _length, bool _encrypted, bool _compressed, int _pkgNumInMsg, int _pkgsInMsg)
	{
		if (logFileStream == null)
		{
			return;
		}
		string text = ((_clientInfo == null) ? "Server" : _clientInfo.InternalId.CombinedString);
		lock (logFileStream)
		{
			logFileStream.WriteLine(string.Format("{0:O},{1},{2},{3},{4},{5},{6},{7},{8},{9}", DateTime.Now, _dirIsOut ? "Out" : "In", text, _packageType.GetType().Name, _channel, _length, _encrypted, _compressed, _pkgNumInMsg, _pkgsInMsg));
		}
	}

	public static void EndLog()
	{
		if (logFileStream != null && --opened <= 0)
		{
			logFileStream.Close();
			logFileStream = null;
		}
	}
}
