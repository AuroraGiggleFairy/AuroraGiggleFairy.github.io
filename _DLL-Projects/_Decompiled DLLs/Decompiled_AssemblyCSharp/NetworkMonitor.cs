using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;

public class NetworkMonitor
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct SIdCnt
	{
		public int Id;

		public int Cnt;

		public int Bytes;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class SIdCntSorter : IComparer<SIdCnt>
	{
		public int Compare(SIdCnt _obj1, SIdCnt _obj2)
		{
			return _obj2.Cnt - _obj1.Cnt;
		}
	}

	public bool Enabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool initialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly UILabel uiLblOverview;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly UILabel uiLblRecvPkgCnt;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly UILabel uiLblSentPkgCnt;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly UILabel uiLblRecvPkgSeq;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly UILabel uiLblSentPkgSeq;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly UITexture uiTxtRecv;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly UITexture uiTxtSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public SimpleGraph graphReceived;

	[PublicizedFrom(EAccessModifier.Private)]
	public SimpleGraph graphSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] packagesPerTypeReceived = new int[NetPackageManager.KnownPackageCount];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] packagesPerTypeSent = new int[NetPackageManager.KnownPackageCount];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] bytesPerTypeReceived = new int[NetPackageManager.KnownPackageCount];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] bytesPerTypeSent = new int[NetPackageManager.KnownPackageCount];

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bResetNext;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalBytesSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalBytesReceived;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalPackagesSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalPackagesReceived;

	[PublicizedFrom(EAccessModifier.Private)]
	public RingBuffer<SNetPackageInfo> recSequence;

	[PublicizedFrom(EAccessModifier.Private)]
	public RingBuffer<SNetPackageInfo> sentSequence;

	[PublicizedFrom(EAccessModifier.Private)]
	public float timePassed;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<SIdCnt> sortList = new List<SIdCnt>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SIdCntSorter sorter = new SIdCntSorter();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Transform parent;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int channel;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sumBRecv;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sumBSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sumPRecv;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sumPSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalBRecv;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalBSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalPRecv;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalPSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public float bpsRecv;

	[PublicizedFrom(EAccessModifier.Private)]
	public float bpsSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public float ppsRecv;

	[PublicizedFrom(EAccessModifier.Private)]
	public float ppsSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int maxDisplayedPackageTypes = 16;

	public NetworkMonitor(int _channel, Transform _parent)
	{
		channel = _channel;
		parent = _parent;
		uiLblOverview = _parent.Find("lblOverview").GetComponent<UILabel>();
		Transform transform;
		uiLblRecvPkgCnt = (((transform = _parent.Find("lblRecvPkgCnt")) != null) ? transform.GetComponent<UILabel>() : null);
		uiLblSentPkgCnt = (((transform = _parent.Find("lblSentPkgCnt")) != null) ? transform.GetComponent<UILabel>() : null);
		uiLblRecvPkgSeq = (((transform = _parent.Find("lblRecvPkgSeq")) != null) ? transform.GetComponent<UILabel>() : null);
		uiLblSentPkgSeq = (((transform = _parent.Find("lblSentPkgSeq")) != null) ? transform.GetComponent<UILabel>() : null);
		uiTxtRecv = (((transform = _parent.Find("texRecv")) != null) ? transform.GetComponent<UITexture>() : null);
		uiTxtSent = (((transform = _parent.Find("texSent")) != null) ? transform.GetComponent<UITexture>() : null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		initialized = true;
		if (uiTxtRecv != null)
		{
			graphReceived = new SimpleGraph();
			graphReceived.Init(1024, 128, 1f, new float[3] { 0.01f, 0.5f, 1f });
			uiTxtRecv.mainTexture = graphReceived.texture;
		}
		if (uiTxtSent != null)
		{
			graphSent = new SimpleGraph();
			graphSent.Init(1024, 128, 1f, new float[3] { 0.01f, 0.5f, 1f });
			uiTxtSent.mainTexture = graphSent.texture;
		}
	}

	public void Cleanup()
	{
		graphReceived?.Cleanup();
		graphSent?.Cleanup();
	}

	public void ResetAllNumbers()
	{
		bResetNext = true;
		sumBRecv = 0;
		totalBRecv = 0;
		sumBSent = 0;
		totalBSent = 0;
		sumPRecv = 0;
		totalPRecv = 0;
		sumPSent = 0;
		totalPSent = 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDataForConnection(INetConnection[] _nc)
	{
		if (channel < _nc.Length)
		{
			if (_nc[channel] != null)
			{
				_nc[channel].GetStats().GetStats(0f, out var _bytesPerSecondSent, out var _packagesPerSecondSent, out var _bytesPerSecondReceived, out var _packagesPerSecondReceived);
				totalBytesSent += _bytesPerSecondSent;
				totalBytesReceived += _bytesPerSecondReceived;
				totalPackagesSent += _packagesPerSecondSent;
				totalPackagesReceived += _packagesPerSecondReceived;
				_nc[channel].GetStats().GetPackageTypes(packagesPerTypeReceived, bytesPerTypeReceived, packagesPerTypeSent, bytesPerTypeSent, bResetNext);
				recSequence = _nc[channel].GetStats().GetLastPackagesReceived();
				sentSequence = _nc[channel].GetStats().GetLastPackagesSent();
			}
			bResetNext = false;
		}
	}

	public void Update()
	{
		if (Enabled != parent.gameObject.activeSelf)
		{
			parent.gameObject.SetActive(Enabled);
		}
		if (!Enabled)
		{
			bpsRecv = (bpsSent = 0f);
			sumPRecv = (sumPSent = 0);
			return;
		}
		timePassed += Time.deltaTime;
		if (!initialized)
		{
			Init();
		}
		totalBytesSent = 0;
		totalBytesReceived = 0;
		totalPackagesSent = 0;
		totalPackagesReceived = 0;
		Array.Clear(packagesPerTypeReceived, 0, packagesPerTypeReceived.Length);
		Array.Clear(bytesPerTypeReceived, 0, bytesPerTypeReceived.Length);
		Array.Clear(packagesPerTypeSent, 0, packagesPerTypeSent.Length);
		Array.Clear(bytesPerTypeSent, 0, bytesPerTypeSent.Length);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			ReadOnlyCollection<ClientInfo> list = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List;
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					ClientInfo clientInfo = list[i];
					updateDataForConnection(clientInfo.netConnection);
				}
			}
		}
		else
		{
			updateDataForConnection(SingletonMonoBehaviour<ConnectionManager>.Instance.GetConnectionToServer());
		}
		graphReceived?.Update((float)totalBytesReceived / 1024f, Color.green);
		graphSent?.Update((float)totalBytesSent / 1024f, Color.green);
		sumBRecv += totalBytesReceived;
		totalBRecv += totalBytesReceived;
		sumBSent += totalBytesSent;
		totalBSent += totalBytesSent;
		sumPRecv += totalPackagesReceived;
		totalPRecv += totalPackagesReceived;
		sumPSent += totalPackagesSent;
		totalPSent += totalPackagesSent;
		if (timePassed > 1f)
		{
			bpsRecv = (float)sumBRecv / timePassed;
			bpsSent = (float)sumBSent / timePassed;
			sumBSent = (sumBRecv = 0);
			ppsRecv = (float)sumPRecv / timePassed;
			ppsSent = (float)sumPSent / timePassed;
			sumPRecv = (sumPSent = 0);
			timePassed = 0f;
		}
		uiLblOverview.text = $"Overview Channel {channel}:\n Recv: {bpsRecv / 1024f:0.00} kB/s {(float)totalBRecv / 1024f:0} kB\n Sent: {bpsSent / 1024f:0.00} kB/s {(float)totalBSent / 1024f:0} kB\n Recv: {ppsRecv:0.0} p/s {totalPRecv:0} p\n Sent: {ppsSent:0.0} p/s {totalPSent:0} p\n";
		StringBuilder sb = new StringBuilder();
		updatePackageListText(sb, uiLblRecvPkgCnt, packagesPerTypeReceived, bytesPerTypeReceived, "Rec");
		updatePackageListText(sb, uiLblSentPkgCnt, packagesPerTypeSent, bytesPerTypeSent, "Sent");
		if (uiLblRecvPkgSeq != null)
		{
			uiLblRecvPkgSeq.text = printPackageSequence(recSequence, "Rec last 30:");
		}
		if (uiLblSentPkgSeq != null)
		{
			uiLblSentPkgSeq.text = printPackageSequence(sentSequence, "Sent last 30:");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePackageListText(StringBuilder _sb, UILabel _label, int[] _packagesPerType, int[] _bytesPerType, string _caption)
	{
		if (_label != null)
		{
			sortList.Clear();
			for (int i = 0; i < _packagesPerType.Length; i++)
			{
				SIdCnt item = new SIdCnt
				{
					Id = i,
					Cnt = _packagesPerType[i],
					Bytes = _bytesPerType[i]
				};
				sortList.Add(item);
			}
			sortList.Sort(sorter);
			_sb.Length = 0;
			_sb.Append(_caption);
			_sb.Append(":\n");
			for (int j = 0; j < 16 && j < sortList.Count && sortList[j].Cnt != 0; j++)
			{
				_sb.Append($"{j + 1}. {NetPackageManager.GetPackageName(sortList[j].Id)}: {sortList[j].Cnt} - {sortList[j].Bytes} B\n");
			}
			_label.text = _sb.ToString();
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
		SNetPackageInfo? sNetPackageInfo = null;
		int num = 0;
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
					num = 0;
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
		return string.Format(" {0:000} {1}{2} {3} B\n", _baseTick - _lastPackage.Tick, (_lastPackageCnt > 0) ? (_lastPackageCnt + 1 + "x ") : string.Empty, NetPackageManager.GetPackageName(_lastPackage.Id), _size);
	}
}
