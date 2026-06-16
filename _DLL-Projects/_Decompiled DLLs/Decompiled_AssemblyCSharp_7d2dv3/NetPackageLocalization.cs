using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Noemax.GZip;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageLocalization : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] data;

	[PublicizedFrom(EAccessModifier.Private)]
	public int seqNr;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalParts;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DateTime downloadStartTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public static MemoryStream receiveStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public static WaitForSeconds PACKET_SEND_DELAY = new WaitForSeconds(0.25f);

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CHUNK_SIZE = 131072;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<byte[]> dataChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int cachedDataSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object lockObj = new object();

	public override bool Compress => false;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackageLocalization Setup(byte[] _data, int _seqNr, int _totalParts)
	{
		data = _data;
		seqNr = _seqNr;
		totalParts = _totalParts;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		seqNr = _reader.ReadInt32();
		totalParts = _reader.ReadInt32();
		int num = _reader.ReadInt32();
		if (num >= 0)
		{
			data = _reader.ReadBytes(num);
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(seqNr);
		_writer.Write(totalParts);
		byte[] array = data;
		_writer.Write((array != null) ? array.Length : (-1));
		if (data != null)
		{
			_writer.Write(data);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		DateTime now = DateTime.Now;
		if (seqNr == 0)
		{
			receiveStream = new MemoryStream();
			downloadStartTime = now;
		}
		float num = (float)(seqNr + 1) / (float)totalParts;
		TimeSpan timeSpan = TimeSpan.FromSeconds((now - downloadStartTime).TotalSeconds / (double)num * (double)(1f - num));
		if ((double)num > 0.05)
		{
			XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, string.Format(Localization.Get("uiLoadDownloadingLocalizationEstimate"), num, (int)timeSpan.TotalMinutes, timeSpan.Seconds));
		}
		else
		{
			XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, string.Format(Localization.Get("uiLoadDownloadingLocalization"), num));
		}
		receiveStream.Write(data, 0, data.Length);
		if (seqNr != totalParts - 1)
		{
			return;
		}
		receiveStream.Position = 0L;
		using DeflateInputStream source = new DeflateInputStream(receiveStream);
		using MemoryStream memoryStream = new MemoryStream();
		StreamUtils.StreamCopy(source, memoryStream);
		Localization.LoadServerPatchDictionary(memoryStream.ToArray());
		receiveStream = null;
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadWaitingForConfigs"));
	}

	public override int GetLength()
	{
		return 16 + data.Length;
	}

	public static IEnumerator StartSendingPacketsToClient(ClientInfo _cInfo)
	{
		byte[] patchedData = Localization.PatchedData;
		if (dataChunks == null || cachedDataSize != patchedData.Length)
		{
			lock (lockObj)
			{
				if (dataChunks == null || cachedDataSize != patchedData.Length)
				{
					prepareDataPackets(patchedData);
				}
			}
		}
		yield return sendPacketsToClient(_cInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void prepareDataPackets(byte[] _patchedData)
	{
		int num = GamePrefs.GetInt(EnumGamePrefs.ServerMaxWorldTransferSpeedKiBs);
		if (num > 0)
		{
			PACKET_SEND_DELAY = new WaitForSeconds(131072f / (float)(num * 1024));
		}
		Log.Out("Preparing Localization chunks for clients");
		long num2 = _patchedData.Length / 131072;
		long num3 = _patchedData.Length % 131072;
		if (num3 == 0L)
		{
			num3 = 131072L;
		}
		else
		{
			num2++;
		}
		List<byte[]> list = new List<byte[]>();
		for (int i = 0; i < num2; i++)
		{
			long num4 = 131072L;
			if (i == num2 - 1)
			{
				num4 = num3;
			}
			byte[] array = new byte[num4];
			Array.Copy(_patchedData, i * 131072, array, 0L, num4);
			list.Add(array);
		}
		Log.Out("Localization size: {0} B, chunk count: {1}", _patchedData.Length, list.Count);
		cachedDataSize = _patchedData.Length;
		dataChunks = list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator sendPacketsToClient(ClientInfo _cInfo)
	{
		while (dataChunks == null)
		{
			yield return null;
		}
		string cInfoString = _cInfo.ToString();
		Log.Out("Starting to send Localization to " + cInfoString + "...");
		for (int i = 0; i < dataChunks.Count; i++)
		{
			_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageLocalization>().Setup(dataChunks[i], i, dataChunks.Count));
			yield return PACKET_SEND_DELAY;
		}
		Log.Out("Sending Localization to " + cInfoString + " done");
	}
}
