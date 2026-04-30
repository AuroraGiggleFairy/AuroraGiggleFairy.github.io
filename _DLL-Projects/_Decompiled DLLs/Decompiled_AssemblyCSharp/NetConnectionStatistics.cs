using System;
using UnityEngine;

public class NetConnectionStatistics
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTimeStatsRequested;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile int statsBytesSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile int statsPackagesSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] statsPackagePerTypeSent = new int[NetPackageManager.KnownPackageCount];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] statsBytesPerTypeSent = new int[NetPackageManager.KnownPackageCount];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly RingBuffer<SNetPackageInfo> statsLastPackagesSent = new RingBuffer<SNetPackageInfo>(30);

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile int statsBytesReceived;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile int statsPackagesReceived;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] statsPackagePerTypeReceived = new int[NetPackageManager.KnownPackageCount];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] statsBytesPerTypeReceived = new int[NetPackageManager.KnownPackageCount];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly RingBuffer<SNetPackageInfo> statsLastPackagesRec = new RingBuffer<SNetPackageInfo>(30);

	[PublicizedFrom(EAccessModifier.Private)]
	public int bytesPerSecondSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public int packagesPerSecondSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public int bytesPerSecondReceived;

	[PublicizedFrom(EAccessModifier.Private)]
	public int packagesPerSecondReceived;

	public void RegisterReceivedPackage(int _packageType, int _length)
	{
		statsPackagePerTypeReceived[_packageType]++;
		statsBytesPerTypeReceived[_packageType] += _length;
		statsLastPackagesRec.Add(new SNetPackageInfo(_packageType, _length));
	}

	public void RegisterReceivedData(int _packageCount, int _netDataSize)
	{
		statsBytesReceived += _netDataSize;
		statsPackagesReceived += _packageCount;
	}

	public void RegisterSentData(int _packageCount, int _netDataSize)
	{
		statsBytesSent += _netDataSize;
		statsPackagesSent += _packageCount;
	}

	public void RegisterSentPackage(int _packageType, int _length)
	{
		statsPackagePerTypeSent[_packageType]++;
		statsBytesPerTypeSent[_packageType] += _length;
		statsLastPackagesSent.Add(new SNetPackageInfo(_packageType, _length));
	}

	public void GetPackageTypes(int[] _packagesPerTypeReceived, int[] _bytesPerTypeReceived, int[] _packagesPerTypeSent, int[] _bytesPerTypeSent, bool _reset)
	{
		for (int i = 0; i < _packagesPerTypeReceived.Length; i++)
		{
			_packagesPerTypeReceived[i] += statsPackagePerTypeReceived[i];
			_packagesPerTypeSent[i] += statsPackagePerTypeSent[i];
			_bytesPerTypeReceived[i] += statsBytesPerTypeReceived[i];
			_bytesPerTypeSent[i] += statsBytesPerTypeSent[i];
		}
		if (_reset)
		{
			Array.Clear(statsPackagePerTypeReceived, 0, statsPackagePerTypeReceived.Length);
			Array.Clear(statsPackagePerTypeSent, 0, statsPackagePerTypeSent.Length);
			Array.Clear(statsBytesPerTypeReceived, 0, statsBytesPerTypeReceived.Length);
			Array.Clear(statsBytesPerTypeSent, 0, statsBytesPerTypeSent.Length);
		}
	}

	public RingBuffer<SNetPackageInfo> GetLastPackagesSent()
	{
		return statsLastPackagesSent;
	}

	public RingBuffer<SNetPackageInfo> GetLastPackagesReceived()
	{
		return statsLastPackagesRec;
	}

	public void GetStats(float _interval, out int _bytesPerSecondSent, out int _packagesPerSecondSent, out int _bytesPerSecondReceived, out int _packagesPerSecondReceived)
	{
		_bytesPerSecondSent = statsBytesSent;
		_packagesPerSecondSent = statsPackagesSent;
		_bytesPerSecondReceived = statsBytesReceived;
		_packagesPerSecondReceived = statsPackagesReceived;
		resetStats();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetStats()
	{
		statsBytesReceived = 0;
		statsPackagesReceived = 0;
		statsBytesSent = 0;
		statsPackagesSent = 0;
		lastTimeStatsRequested = Time.time;
	}
}
