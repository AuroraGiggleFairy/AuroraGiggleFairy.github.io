using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Noemax.GZip;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWorldFolder : NetPackage
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
	public static MemoryStream ReceiveStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool WorldReceivedAndUncompressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public static WaitForSeconds PACKET_SEND_DELAY = new WaitForSeconds(0.25f);

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CHUNK_SIZE = 65536;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int DTM_BITMASK = 65520;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<byte[]> CompressedWorldDataChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string CachedWorldName;

	[PublicizedFrom(EAccessModifier.Private)]
	public static object LockObj = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator PreparationCoroutine;

	public override bool Compress => false;

	public override int Channel => 1;

	public override NetPackageDirection PackageDirection => NetPackageDirection.Both;

	public static int MaxTimePerFrame
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!GameManager.IsDedicatedServer)
			{
				return 5;
			}
			return 40;
		}
	}

	public NetPackageWorldFolder Setup(byte[] _data, int _seqNr, int _totalParts)
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
		_writer.Write((data != null) ? data.Length : (-1));
		if (data != null)
		{
			_writer.Write(data);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			StartSendingPacketsToClient(base.Sender);
			return;
		}
		DateTime now = DateTime.Now;
		if (downloadStartTime > now)
		{
			downloadStartTime = now;
		}
		float num = (float)(seqNr + 1) / (float)totalParts;
		TimeSpan timeSpan = TimeSpan.FromSeconds((now - downloadStartTime).TotalSeconds / (double)num * (double)(1f - num));
		if ((double)num > 0.05)
		{
			XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, string.Format(Localization.Get("uiLoadDownloadingWorldEstimate"), num, (int)timeSpan.TotalMinutes, timeSpan.Seconds));
		}
		else
		{
			XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, string.Format(Localization.Get("uiLoadDownloadingWorld"), num));
		}
		ReceiveStream.Write(data, 0, data.Length);
		if (seqNr == totalParts - 1)
		{
			ThreadManager.StartCoroutine(uncompressWorld());
		}
	}

	public override int GetLength()
	{
		return 16 + ((data != null) ? data.Length : 0);
	}

	public static IEnumerator TestWorldValid(string _locationPath, Dictionary<string, uint> _worldFileHashes, Action<bool> _resultCallback)
	{
		byte[] buffer = new byte[8192];
		foreach (KeyValuePair<string, uint> hashEntry in _worldFileHashes)
		{
			string text = _locationPath + "/" + hashEntry.Key;
			if (!SdFile.Exists(text))
			{
				Log.Out("World file {0} does not exist", hashEntry.Key);
				_resultCallback(obj: false);
				yield break;
			}
			bool validHash = true;
			yield return IOUtils.CalcCrcCoroutine(text, [PublicizedFrom(EAccessModifier.Internal)] (uint _crc) =>
			{
				if (_crc != hashEntry.Value)
				{
					validHash = false;
					Log.Out("World file {0} is different than server's version: received {1:X8}, calculated {2:X8}", hashEntry.Key, hashEntry.Value, _crc);
				}
				else
				{
					validHash = true;
				}
			}, 15, buffer);
			if (!validHash)
			{
				_resultCallback(obj: false);
				yield break;
			}
		}
		_resultCallback(obj: true);
	}

	public static IEnumerator RequestWorld()
	{
		ReceiveStream = new MemoryStream();
		WorldReceivedAndUncompressed = false;
		downloadStartTime = DateTime.MaxValue;
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWorldFolder>());
		while (!WorldReceivedAndUncompressed && SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			yield return null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator uncompressWorld()
	{
		string worldFolder = GameIO.GetSaveGameLocalDir() + "/World";
		ReceiveStream.Position = 0L;
		DeflateInputStream zipStream = new DeflateInputStream(ReceiveStream);
		BinaryReader reader = new BinaryReader(zipStream);
		SdDirectory.CreateDirectory(worldFolder);
		int fileCount = reader.ReadInt32();
		MicroStopwatch mswCompressing = new MicroStopwatch();
		byte[] buffer = MemoryPools.poolByte.Alloc(4096);
		yield return null;
		for (int i = 0; i < fileCount; i++)
		{
			string text = reader.ReadString();
			long fileSize = reader.ReadInt64();
			if (text.StartsWith('.') || text.IndexOfAny(GameIO.ResourcePathSeparators) >= 0)
			{
				Log.Warning("Received world files contains file with parent path specifier or path separator: " + text);
				bool bWhileCopying = true;
				while (bWhileCopying && fileSize > 0)
				{
					int num = zipStream.Read(buffer, 0, (int)Math.Min(fileSize, buffer.Length));
					if (num > 0)
					{
						fileSize -= num;
					}
					else
					{
						bWhileCopying = false;
					}
					if (bWhileCopying && mswCompressing.ElapsedMilliseconds > MaxTimePerFrame)
					{
						yield return null;
						mswCompressing.ResetAndRestart();
					}
				}
				continue;
			}
			Stream fs = SdFile.Create(worldFolder + "/" + text);
			mswCompressing.ResetAndRestart();
			if (text.StartsWith("dtm", StringComparison.OrdinalIgnoreCase) && text.EndsWith(".raw", StringComparison.OrdinalIgnoreCase))
			{
				yield return ThreadManager.StartCoroutine(readDtmDelta(fs, zipStream, mswCompressing, fileSize));
			}
			else
			{
				bool bWhileCopying = true;
				while (bWhileCopying && fileSize > 0)
				{
					int num2 = zipStream.Read(buffer, 0, (int)Math.Min(fileSize, buffer.Length));
					if (num2 > 0)
					{
						fs.Write(buffer, 0, num2);
						fileSize -= num2;
					}
					else
					{
						bWhileCopying = false;
					}
					if (bWhileCopying && mswCompressing.ElapsedMilliseconds > MaxTimePerFrame)
					{
						yield return null;
						mswCompressing.ResetAndRestart();
					}
				}
			}
			fs.Dispose();
			yield return null;
		}
		MemoryPools.poolByte.Free(buffer);
		zipStream.Dispose();
		yield return null;
		SdFile.WriteAllBytes(worldFolder + "/completed", new byte[0]);
		WorldReceivedAndUncompressed = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator readDtmDelta(Stream _fs, DeflateInputStream _zipStream, MicroStopwatch _mswCompressing, long _fileSize)
	{
		int w = (int)Math.Sqrt(_fileSize / 2);
		int h = w;
		int lineBytes = w * 2;
		byte[] readLineData = new byte[lineBytes];
		byte[] writeLineData = new byte[lineBytes];
		MemoryStream readStream = new MemoryStream(readLineData);
		MemoryStream writeStream = new MemoryStream(writeLineData);
		for (int z = 0; z < h; z++)
		{
			_zipStream.Read(readLineData, 0, lineBytes);
			readStream.Position = 0L;
			writeStream.Position = 0L;
			int num = StreamUtils.ReadUInt16(readStream);
			StreamUtils.Write(writeStream, (ushort)num);
			for (int i = 1; i < w; i++)
			{
				int num2 = StreamUtils.ReadInt16(readStream);
				int num3 = num + num2;
				num = num3;
				if (num3 < 0 || num3 > 65535)
				{
					Log.Out("Current out of range: " + num3);
				}
				StreamUtils.Write(writeStream, (ushort)num3);
			}
			_fs.Write(writeLineData, 0, lineBytes);
			if (_mswCompressing.ElapsedMilliseconds > MaxTimePerFrame)
			{
				yield return null;
				_mswCompressing.ResetAndRestart();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void StartSendingPacketsToClient(ClientInfo _cInfo)
	{
		string text = GamePrefs.GetString(EnumGamePrefs.GameWorld);
		if (CompressedWorldDataChunks == null || text != CachedWorldName)
		{
			CompressedWorldDataChunks = null;
			CachedWorldName = null;
			lock (LockObj)
			{
				if (PreparationCoroutine == null)
				{
					PreparationCoroutine = prepareWorldFolderData();
					ThreadManager.StartCoroutine(PreparationCoroutine);
				}
			}
		}
		ThreadManager.StartCoroutine(sendPacketsToClient(_cInfo));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator prepareWorldFolderData()
	{
		int num = GamePrefs.GetInt(EnumGamePrefs.ServerMaxWorldTransferSpeedKiBs);
		if (num > 0)
		{
			PACKET_SEND_DELAY = new WaitForSeconds(65536f / (float)(num * 1024));
		}
		Log.Out("Preparing World chunks for clients");
		MemoryStream memStream = new MemoryStream();
		DeflateOutputStream zipStream = new DeflateOutputStream(memStream, 3);
		BinaryWriter writer = new BinaryWriter(zipStream);
		string worldFolder = PathAbstractions.WorldsSearchPaths.GetLocation(GamePrefs.GetString(EnumGamePrefs.GameWorld)).FullPath;
		List<string> worldFiles = GameUtils.GetWorldFilesToTransmitToClient(worldFolder);
		yield return null;
		byte[] buffer = MemoryPools.poolByte.Alloc(4096);
		MicroStopwatch mswCompressing = new MicroStopwatch();
		writer.Write(worldFiles.Count);
		for (int i = 0; i < worldFiles.Count; i++)
		{
			string text = worldFiles[i];
			string path = worldFolder + "/" + text;
			Stream fs = SdFile.OpenRead(path);
			long length = fs.Length;
			writer.Write(text);
			writer.Write(length);
			mswCompressing.ResetAndRestart();
			if (text.StartsWith("dtm", StringComparison.OrdinalIgnoreCase) && text.EndsWith(".raw", StringComparison.OrdinalIgnoreCase))
			{
				yield return ThreadManager.StartCoroutine(writeDtmDelta(fs, zipStream, mswCompressing));
			}
			else
			{
				bool bWhileCopying = true;
				while (bWhileCopying)
				{
					int num2 = fs.Read(buffer, 0, buffer.Length);
					if (num2 > 0)
					{
						zipStream.Write(buffer, 0, num2);
					}
					else
					{
						bWhileCopying = false;
					}
					if (bWhileCopying && mswCompressing.ElapsedMilliseconds > MaxTimePerFrame)
					{
						yield return null;
						mswCompressing.ResetAndRestart();
					}
				}
			}
			fs.Dispose();
			yield return null;
		}
		MemoryPools.poolByte.Free(buffer);
		zipStream.Flush();
		memStream.Position = 0L;
		yield return null;
		long num3 = memStream.Length / 65536;
		long num4 = memStream.Length % 65536;
		if (num4 == 0L)
		{
			num4 = 65536L;
		}
		else
		{
			num3++;
		}
		List<byte[]> list = new List<byte[]>();
		for (int j = 0; j < num3; j++)
		{
			long num5 = 65536L;
			if (j == num3 - 1)
			{
				num5 = num4;
			}
			byte[] array = new byte[num5];
			memStream.Read(array, 0, (int)num5);
			list.Add(array);
		}
		if (memStream.Position != memStream.Length)
		{
			Log.Out("Wrong memStream Position after creating arrays: pos={0}, len={1}", memStream.Position, memStream.Length);
		}
		Log.Out("World chunks size: {0} B, chunk count: {1}", memStream.Length, list.Count);
		zipStream.Dispose();
		lock (LockObj)
		{
			CachedWorldName = GamePrefs.GetString(EnumGamePrefs.GameWorld);
			CompressedWorldDataChunks = list;
			PreparationCoroutine = null;
		}
	}

	public static IEnumerator writeDtmDelta(Stream _sourceStream, Stream _targetStream, MicroStopwatch _mswCompressing)
	{
		int w = (int)Math.Sqrt(_sourceStream.Length / 2);
		int h = w;
		int lineBytes = w * 2;
		byte[] readLineData = new byte[lineBytes];
		byte[] writeLineData = new byte[lineBytes];
		MemoryStream readStream = new MemoryStream(readLineData);
		MemoryStream writeStream = new MemoryStream(writeLineData);
		for (int z = 0; z < h; z++)
		{
			readStream.Position = 0L;
			writeStream.Position = 0L;
			_sourceStream.Read(readLineData, 0, lineBytes);
			int num = StreamUtils.ReadUInt16(readStream);
			StreamUtils.Write(writeStream, (ushort)num);
			for (int i = 1; i < w; i++)
			{
				ushort num2 = StreamUtils.ReadUInt16(readStream);
				int num3 = num2 - num;
				num = num2;
				if (num3 < -32768 || num3 > 32767)
				{
					Log.Out("Delta out of range: " + num3);
				}
				StreamUtils.Write(writeStream, (short)num3);
			}
			_targetStream.Write(writeLineData, 0, lineBytes);
			if (_mswCompressing != null && _mswCompressing.ElapsedMilliseconds > MaxTimePerFrame)
			{
				yield return null;
				_mswCompressing.ResetAndRestart();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator sendPacketsToClient(ClientInfo _cInfo)
	{
		while (CompressedWorldDataChunks == null)
		{
			yield return null;
		}
		string cInfoString = _cInfo.ToString();
		Log.Out("Starting to send world to " + cInfoString + "...");
		for (int i = 0; i < CompressedWorldDataChunks.Count; i++)
		{
			_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageWorldFolder>().Setup(CompressedWorldDataChunks[i], i, CompressedWorldDataChunks.Count));
			yield return PACKET_SEND_DELAY;
		}
		Log.Out("Sending world to " + cInfoString + " done");
	}
}
