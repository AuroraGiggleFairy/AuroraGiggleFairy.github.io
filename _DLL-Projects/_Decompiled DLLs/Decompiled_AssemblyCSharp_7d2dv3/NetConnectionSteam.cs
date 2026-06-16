using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Noemax.GZip;

public class NetConnectionSteam : NetConnectionAbs
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Queue<NetPackage> packetsToSend = new Queue<NetPackage>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ManualResetEvent writerTriggerEvent = new ManualResetEvent(initialState: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ThreadManager.ThreadInfo writerThreadInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream sendStreamUncompressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream sendStreamCompressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledBinaryWriter sendStreamWriter;

	[PublicizedFrom(EAccessModifier.Private)]
	public DeflateOutputStream sendZipStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly byte[] copyBufferWriter;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BlockingQueue<ArrayListMP<byte>> bufsToRead = new BlockingQueue<ArrayListMP<byte>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ManualResetEvent readerTriggerEvent = new ManualResetEvent(initialState: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ThreadManager.ThreadInfo readerThreadInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly byte[] copyBufferReader;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream receiveStreamUncompressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream receiveStreamCompressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public DeflateInputStream receiveZipStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly PooledBinaryReader receiveStreamReader = new PooledBinaryReader();

	public NetConnectionSteam(int _channel, ClientInfo _clientInfo, INetworkClient _netClient, string _uniqueId)
		: base(_channel, _clientInfo, _netClient, _uniqueId)
	{
		copyBufferReader = new byte[4096];
		copyBufferWriter = new byte[4096];
		if (_clientInfo != null)
		{
			if (_channel == 0)
			{
				InitStreams(_full: false);
			}
		}
		else
		{
			InitStreams(_full: true);
		}
		readerThreadInfo = ThreadManager.StartThread("NCSteam_Reader_" + connectionIdentifier, Task_CommReader, null, null, true, false);
		writerThreadInfo = ThreadManager.StartThread("NCSteam_Writer_" + connectionIdentifier, Task_CommWriter, null, null, true, false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitStreams(bool _full)
	{
		if (fullConnection)
		{
			return;
		}
		if (_full)
		{
			byte[] array = new byte[2097152];
			receiveStreamCompressed = new MemoryStream(array, 0, array.Length, writable: true, publiclyVisible: true);
			receiveStreamCompressed.SetLength(0L);
			byte[] array2 = new byte[2097152];
			receiveStreamUncompressed = new MemoryStream(array2, 0, array2.Length, writable: true, publiclyVisible: true);
			receiveZipStream = new DeflateInputStream(receiveStreamCompressed, leaveOpen: true);
			byte[] array3 = new byte[2097152];
			sendStreamUncompressed = new MemoryStream(array3, 0, array3.Length, writable: true, publiclyVisible: true);
			if (sendStreamWriter == null)
			{
				sendStreamWriter = new PooledBinaryWriter();
			}
			sendStreamWriter.SetBaseStream(sendStreamUncompressed);
			byte[] array4 = new byte[2097152];
			sendStreamCompressed = new MemoryStream(array4, 0, array4.Length, writable: true, publiclyVisible: true);
			sendZipStream = new DeflateOutputStream(sendStreamCompressed, 3, leaveOpen: true);
			fullConnection = true;
		}
		else
		{
			byte[] array5 = new byte[32768];
			receiveStreamCompressed = new MemoryStream(array5, 0, array5.Length, writable: true, publiclyVisible: true);
			receiveStreamCompressed.SetLength(0L);
			byte[] array6 = new byte[32768];
			sendStreamUncompressed = new MemoryStream(array6, 0, array6.Length, writable: true, publiclyVisible: true);
			sendStreamWriter = new PooledBinaryWriter();
			sendStreamWriter.SetBaseStream(sendStreamUncompressed);
		}
	}

	public override void Disconnect(bool _kick)
	{
		base.Disconnect(_kick);
		readerTriggerEvent.Set();
		writerTriggerEvent.Set();
	}

	public override void AddToSendQueue(NetPackage _package)
	{
		_package.RegisterSendQueue();
		lock (packetsToSend)
		{
			packetsToSend.Enqueue(_package);
		}
		writerTriggerEvent.Set();
	}

	public override void AppendToReaderStream(byte[] _data, int _size)
	{
		if (!bDisconnected)
		{
			ArrayListMP<byte> arrayListMP = new ArrayListMP<byte>(MemoryPools.poolByte, _size);
			Array.Copy(_data, arrayListMP.Items, _size);
			arrayListMP.Count = _size;
			MemoryPools.poolByte.Free(_data);
			bufsToRead.Enqueue(arrayListMP);
			readerTriggerEvent.Set();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Task_CommReader(ThreadManager.ThreadInfo _threadInfo)
	{
		try
		{
			while (!bDisconnected)
			{
				readerTriggerEvent.WaitOne();
				if (bDisconnected)
				{
					break;
				}
				if (!bufsToRead.HasData())
				{
					readerTriggerEvent.Reset();
					continue;
				}
				ArrayListMP<byte> arrayListMP = bufsToRead.Dequeue();
				stats.RegisterReceivedData(1, arrayListMP.Count);
				bool flag = arrayListMP.Items[0] == 1;
				bool bEncrypted = arrayListMP.Items[1] == 1;
				MemoryStream memoryStream = new MemoryStream(arrayListMP.Items, 2, arrayListMP.Count - 2);
				receiveStreamCompressed.SetLength(0L);
				StreamUtils.StreamCopy(memoryStream, receiveStreamCompressed, copyBufferReader);
				receiveStreamCompressed.Position = 0L;
				if (!Decrypt(bEncrypted, receiveStreamCompressed))
				{
					continue;
				}
				receiveStreamReader.SetBaseStream(Decompress(flag, receiveStreamUncompressed, receiveZipStream, copyBufferReader) ? receiveStreamUncompressed : receiveStreamCompressed);
				NetPackage netPackage;
				try
				{
					netPackage = NetPackageManager.ParsePackage(receiveStreamReader, cInfo);
				}
				catch (KeyNotFoundException)
				{
					receiveStreamReader.BaseStream.Position = 0L;
					Log.Error("[NET] Trying to parse package failed, package id {0} unknown!", receiveStreamReader.ReadByte());
					Log.Error("Length: {0}, Compressed: {1}", arrayListMP.Count - 1, flag);
					Log.Error("Orig data: {0}", string.Join(",", Array.ConvertAll(arrayListMP.Items, [PublicizedFrom(EAccessModifier.Internal)] (byte _val) => _val.ToString("X2"))));
					throw;
				}
				stats.RegisterReceivedPackage(netPackage.PackageId, (int)memoryStream.Length);
				lock (receivedPackages)
				{
					receivedPackages.Add(netPackage);
				}
			}
		}
		catch (Exception e)
		{
			if (cInfo != null)
			{
				Log.Error($"Task_CommReaderSteam (cl={cInfo.PlatformId.CombinedString}, ch={channel}):");
			}
			else
			{
				Log.Error($"Task_CommReaderSteam (ch={channel}):");
			}
			Log.Exception(e);
			Disconnect(_kick: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Task_CommWriter(ThreadManager.ThreadInfo _threadInfo)
	{
		try
		{
			while (!bDisconnected)
			{
				NetPackage netPackage = null;
				writerTriggerEvent.WaitOne();
				if (bDisconnected)
				{
					break;
				}
				lock (packetsToSend)
				{
					if (packetsToSend.Count == 0)
					{
						writerTriggerEvent.Reset();
						continue;
					}
					netPackage = packetsToSend.Dequeue();
				}
				try
				{
					sendStreamUncompressed.Position = 0L;
					sendStreamUncompressed.SetLength(0L);
					netPackage.write(sendStreamWriter);
					stats.RegisterSentPackage(netPackage.PackageId, (int)sendStreamUncompressed.Length);
					sendStreamUncompressed.Position = 0L;
					MemoryStream memoryStream = sendStreamUncompressed;
					bool flag = allowCompression && netPackage.Compress;
					if (Compress(flag, sendStreamUncompressed, sendZipStream, sendStreamCompressed, copyBufferWriter, 1))
					{
						memoryStream = sendStreamCompressed;
					}
					bool flag2 = Encrypt(memoryStream);
					stats.RegisterSentData(1, (int)memoryStream.Length);
					int num = (int)(memoryStream.Length + 3);
					ArrayListMP<byte> arrayListMP = new ArrayListMP<byte>(MemoryPools.poolByte, num);
					arrayListMP.Items[0] = (byte)(flag ? 1 : 0);
					arrayListMP.Items[1] = (byte)(flag2 ? 1 : 0);
					try
					{
						memoryStream.Read(arrayListMP.Items, 2, (int)memoryStream.Length);
					}
					catch (ArgumentException)
					{
						Log.Out("Buffer size: " + arrayListMP.Items.Length + " - Stream length: " + memoryStream.Length + " - Package: " + netPackage);
						throw;
					}
					arrayListMP.Count = num;
					if (isServer)
					{
						cInfo.network.SendData(cInfo, channel, arrayListMP, netPackage.ReliableDelivery);
					}
					else
					{
						netClient.SendData(channel, arrayListMP);
					}
				}
				finally
				{
					netPackage.SendQueueHandled();
				}
			}
		}
		catch (Exception e)
		{
			if (cInfo != null)
			{
				Log.Error($"Task_CommWriterSteam (cl={cInfo.PlatformId.CombinedString}, ch={channel}):");
			}
			else
			{
				Log.Error($"Task_CommWriterSteam (ch={channel}):");
			}
			Log.Exception(e);
			Disconnect(_kick: true);
		}
	}
}
