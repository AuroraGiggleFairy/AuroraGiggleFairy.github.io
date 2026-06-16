using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdProfileNetwork : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static float lastTimeWritten;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ColWidthType = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ColWidthCount = 6;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ColWidthSize = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ColWidthRate = 10;

	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "profilenetwork" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		doProfileNetwork();
	}

	public void resetData()
	{
		lastTimeWritten = Time.time;
		ReadOnlyCollection<ClientInfo> list = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			for (int i = 0; i < list.Count; i++)
			{
				ClientInfo clientInfo = list[i];
				resetDataOnConnection(clientInfo.netConnection);
			}
		}
		else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			for (int j = 0; j < 2; j++)
			{
				resetDataOnConnection(SingletonMonoBehaviour<ConnectionManager>.Instance.GetConnectionToServer());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetDataOnConnection(INetConnection[] _con)
	{
		int[] packagesPerTypeReceived = new int[NetPackageManager.KnownPackageCount];
		int[] packagesPerTypeSent = new int[NetPackageManager.KnownPackageCount];
		int[] bytesPerTypeReceived = new int[NetPackageManager.KnownPackageCount];
		int[] bytesPerTypeSent = new int[NetPackageManager.KnownPackageCount];
		for (int i = 0; i < 2; i++)
		{
			NetConnectionStatistics stats = _con[i].GetStats();
			stats.GetStats(0f, out var _, out var _, out var _, out var _);
			stats.GetPackageTypes(packagesPerTypeReceived, bytesPerTypeReceived, packagesPerTypeSent, bytesPerTypeSent, _reset: true);
			stats.GetLastPackagesReceived().Clear();
			stats.GetLastPackagesSent().Clear();
		}
	}

	public void doProfileNetwork()
	{
		string arg = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? "server" : "client");
		string text = $"{GameIO.GetApplicationPath()}/network_profiling_{arg}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
		float num = Time.time - lastTimeWritten;
		lastTimeWritten = Time.time;
		using StreamWriter streamWriter = new StreamWriter(text);
		streamWriter.WriteLine("Interval: " + num.ToCultureInvariantString("0.0") + "s\n");
		int[] array = new int[2];
		int[] array2 = new int[2];
		int[] array3 = new int[2];
		int[] array4 = new int[2];
		int[] array5 = new int[NetPackageManager.KnownPackageCount];
		int[] array6 = new int[NetPackageManager.KnownPackageCount];
		int[] array7 = new int[NetPackageManager.KnownPackageCount];
		int[] array8 = new int[NetPackageManager.KnownPackageCount];
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			ReadOnlyCollection<ClientInfo> list = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List;
			for (int i = 0; i < list.Count; i++)
			{
				ClientInfo clientInfo = list[i];
				streamWriter.WriteLine($"*** Client {clientInfo}:\n");
				doResultsForConnection(clientInfo.netConnection, streamWriter, array, array2, array3, array4, array6, array8, array5, array7, num);
			}
			streamWriter.WriteLine("\n\nTotal:");
			for (int j = 0; j < 2; j++)
			{
				printChannelStats(streamWriter, j, array4[j], array2[j], array3[j], array[j], num);
			}
		}
		else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			streamWriter.WriteLine("*** ToServer\n");
			doResultsForConnection(SingletonMonoBehaviour<ConnectionManager>.Instance.GetConnectionToServer(), streamWriter, array, array2, array3, array4, array6, array8, array5, array7, num);
		}
		streamWriter.WriteLine("\nPackages:");
		streamWriter.WriteLine(string.Format(" {0,3}\t{1,6}\t{2,8}\t{3,6}\t{4,8}\t{5,6}\t{6,8}\t{7,8}\tPackage type name", "ID", "CntRX", "SizeRX", "CntTX", "SizeTX", "CntSum", "SizeSum", "SizeAvg"));
		for (int k = 0; k < array5.Length; k++)
		{
			int num2 = array5[k];
			int num3 = array7[k];
			int num4 = array6[k];
			int num5 = array8[k];
			int num6 = num2 + num4;
			int num7 = num3 + num5;
			int num8 = ((num6 > 0) ? (num7 / num6) : 0);
			streamWriter.WriteLine($" {k,3}\t{num2,6}\t{num3,8}\t{num4,6}\t{num5,8}\t{num6,6}\t{num7,8}\t{num8,8}\t{NetPackageManager.GetPackageName(k)}");
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Network profiling for {num:0.0}s writing to file {text}");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void printChannelStats(StreamWriter _file, int _channel, int _pkgsRx, int _bytesRx, int _pkgsTx, int _bytesTx, float _interval)
	{
		if (_channel == 0)
		{
			_file.WriteLine("Stats:");
			_file.WriteLine(string.Format(" {0,3}\t{1,6}\t{2,8}\t{3,6}\t{4,8}\t{5,10}\t{6,10}", "Ch", "CntRX", "SizeRX", "CntTX", "SizeTX", "RateRX", "RateTX"));
		}
		_file.WriteLine(string.Format(" {0,3}\t{1,6}\t{2,8}\t{3,6}\t{4,8}\t{5,10}kb/s\t{6,10}kb/s", _channel, _pkgsRx, _bytesRx, _pkgsTx, _bytesTx, ((float)_bytesRx / (_interval * 1024f)).ToCultureInvariantString("0.0"), ((float)_bytesTx / (_interval * 1024f)).ToCultureInvariantString("0.0")));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doResultsForConnection(INetConnection[] _con, StreamWriter _outFile, int[] _tBytesSent, int[] _tBytesReceived, int[] _tPackagesSent, int[] _tPackagesReceived, int[] _tPackagesPerTypeSent, int[] _tBytesPerTypeSent, int[] _tPackagesPerTypeReceived, int[] _tBytesPerTypeReceived, float _interval)
	{
		for (int i = 0; i < 2; i++)
		{
			_con[i].GetStats().GetStats(0f, out var _bytesPerSecondSent, out var _packagesPerSecondSent, out var _bytesPerSecondReceived, out var _packagesPerSecondReceived);
			printChannelStats(_outFile, i, _packagesPerSecondReceived, _bytesPerSecondReceived, _packagesPerSecondSent, _bytesPerSecondSent, _interval);
			_tBytesSent[i] += _bytesPerSecondSent;
			_tBytesReceived[i] += _bytesPerSecondReceived;
			_tPackagesSent[i] += _packagesPerSecondSent;
			_tPackagesReceived[i] += _packagesPerSecondReceived;
			_con[i].GetStats().GetPackageTypes(_tPackagesPerTypeReceived, _tBytesPerTypeReceived, _tPackagesPerTypeSent, _tBytesPerTypeSent, _reset: true);
		}
		_outFile.WriteLine("");
		for (int j = 0; j < 2; j++)
		{
			string value = printPackageSequence(_con[j].GetStats().GetLastPackagesReceived(), "Recv Ch" + j);
			_outFile.WriteLine(value);
		}
		_outFile.WriteLine("");
		for (int k = 0; k < 2; k++)
		{
			string value2 = printPackageSequence(_con[k].GetStats().GetLastPackagesSent(), "Sent on Ch" + k);
			_outFile.WriteLine(value2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string printPackageSequence(RingBuffer<SNetPackageInfo> _sequence, string _addInfo)
	{
		if (_sequence == null)
		{
			return _addInfo;
		}
		StringBuilder stringBuilder = new StringBuilder();
		_sequence.SetToLast();
		ulong tick = _sequence.Peek().Tick;
		stringBuilder.Append(_addInfo);
		stringBuilder.Append(" ");
		stringBuilder.Append(tick.ToString());
		stringBuilder.Append("\n");
		stringBuilder.Append("Past\tCnt\t Size\tID\n");
		SNetPackageInfo? sNetPackageInfo = null;
		int num = 1;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		while (num3 < 30 && num4 < _sequence.Count - 1)
		{
			SNetPackageInfo prev = _sequence.GetPrev();
			if (sNetPackageInfo.HasValue)
			{
				if (sNetPackageInfo.Value.Tick == prev.Tick && sNetPackageInfo.Value.Id == prev.Id)
				{
					num++;
				}
				else
				{
					stringBuilder.Append(formatPackage(tick, sNetPackageInfo.Value, num, num2));
					num = 1;
					num2 = 0;
					num3++;
				}
			}
			sNetPackageInfo = prev;
			num2 += prev.Size;
			num4++;
		}
		if (sNetPackageInfo.HasValue)
		{
			stringBuilder.Append(formatPackage(tick, sNetPackageInfo.Value, num, num2));
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string formatPackage(ulong _baseTick, SNetPackageInfo _lastPackage, int _lastPackageCnt, int _size)
	{
		return $" {_baseTick - _lastPackage.Tick,3}\t{_lastPackageCnt,2}\t{_size,5}\t{NetPackageManager.GetPackageName(_lastPackage.Id)}\n";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Writes network profiling information";
	}
}
