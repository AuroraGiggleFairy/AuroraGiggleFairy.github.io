using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Noemax.GZip;
using UnityEngine.Networking;

public class NetConnectionSimple : NetConnectionAbs
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct RecvBuffer(byte[] _data, int _size)
	{
		public readonly byte[] Data = _data;

		public readonly int Size = _size;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int reservedHeaderBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int maxPacketSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int maxPayloadPerPacket;

	public static bool doCrash;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object writerListLockObj = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<NetPackage> writerListFilling = new List<NetPackage>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<NetPackage> writerListProcessing = new List<NetPackage>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ManualResetEvent writerTriggerEvent = new ManualResetEvent(initialState: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly byte[] writerBuffer;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo writerThreadInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledBinaryWriter reliableSendStreamWriter;

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledBinaryWriter unreliableSendStreamWriter;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream reliableSendStreamUncompressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream unreliableSendStreamUncompressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream sendStreamCompressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public DeflateOutputStream sendZipStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream writerStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly LinkedList<ArrayListMP<byte>> reliableBufsToSend = new LinkedList<ArrayListMP<byte>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly LinkedList<ArrayListMP<byte>> unreliableBufsToSend = new LinkedList<ArrayListMP<byte>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ManualResetEvent readerTriggerEvent = new ManualResetEvent(initialState: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo readerThreadInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly byte[] readerBuffer;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream receiveStreamCompressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream receiveStreamUncompressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public DeflateInputStream receiveZipStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly PooledBinaryReader receiveStreamReader = new PooledBinaryReader();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Queue<RecvBuffer> receivedBuffers = new Queue<RecvBuffer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int networkErrorCooldownMs = 500;

	public int preCompressMaxBufferSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!allowCompression)
			{
				return 32256;
			}
			return 2097152;
		}
	}

	public NetConnectionSimple(int _channel, ClientInfo _clientInfo, INetworkClient _netClient, string _uniqueId, int _reservedHeaderBytes = 0, int _maxPacketSize = 0)
		: base(_channel, _clientInfo, _netClient, _uniqueId)
	{
		reservedHeaderBytes = _reservedHeaderBytes;
		maxPacketSize = _maxPacketSize;
		if (maxPacketSize > 0)
		{
			maxPayloadPerPacket = maxPacketSize - reservedHeaderBytes;
		}
		readerBuffer = new byte[4096];
		writerBuffer = new byte[4096];
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
		readerThreadInfo = ThreadManager.StartThread("NCS_Reader_" + connectionIdentifier, taskDeserialize, null, null, true, false);
		writerThreadInfo = ThreadManager.StartThread("NCS_Writer_" + connectionIdentifier, taskSerialize, null, null, true, false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitStreams(bool _full)
	{
		if (!fullConnection)
		{
			if (_full)
			{
				byte[] array = new byte[2097152];
				receiveStreamCompressed = new MemoryStream(array, 0, array.Length, writable: true, publiclyVisible: true);
				receiveStreamCompressed.SetLength(0L);
				byte[] array2 = new byte[2097152];
				receiveStreamUncompressed = new MemoryStream(array2, 0, array2.Length, writable: true, publiclyVisible: true);
				receiveZipStream = new DeflateInputStream(receiveStreamCompressed, leaveOpen: true);
				byte[] array3 = new byte[2097152];
				reliableSendStreamUncompressed = new MemoryStream(array3, 0, array3.Length, writable: true, publiclyVisible: true);
				reliableSendStreamWriter = new PooledBinaryWriter();
				reliableSendStreamWriter.SetBaseStream(reliableSendStreamUncompressed);
				byte[] array4 = new byte[2097152];
				unreliableSendStreamUncompressed = new MemoryStream(array4, 0, array4.Length, writable: true, publiclyVisible: true);
				unreliableSendStreamWriter = new PooledBinaryWriter();
				unreliableSendStreamWriter.SetBaseStream(unreliableSendStreamUncompressed);
				byte[] array5 = new byte[2097152];
				sendStreamCompressed = new MemoryStream(array5, 0, array5.Length, writable: true, publiclyVisible: true);
				sendZipStream = new DeflateOutputStream(sendStreamCompressed, 3, leaveOpen: true);
				writerStream = new MemoryStream(new byte[2097152]);
				writerStream.SetLength(0L);
				fullConnection = true;
			}
			else
			{
				byte[] array6 = new byte[32768];
				receiveStreamCompressed = new MemoryStream(array6, 0, array6.Length, writable: true, publiclyVisible: true);
				receiveStreamCompressed.SetLength(0L);
				byte[] array7 = new byte[32768];
				reliableSendStreamUncompressed = new MemoryStream(array7, 0, array7.Length, writable: true, publiclyVisible: true);
				reliableSendStreamWriter = new PooledBinaryWriter();
				reliableSendStreamWriter.SetBaseStream(reliableSendStreamUncompressed);
				byte[] array8 = new byte[32768];
				unreliableSendStreamUncompressed = new MemoryStream(array8, 0, array8.Length, writable: true, publiclyVisible: true);
				unreliableSendStreamWriter = new PooledBinaryWriter();
				unreliableSendStreamWriter.SetBaseStream(unreliableSendStreamUncompressed);
				writerStream = new MemoryStream(new byte[32768]);
				writerStream.SetLength(0L);
			}
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
		lock (writerListLockObj)
		{
			writerListFilling.Add(_package);
		}
	}

	public override void FlushSendQueue()
	{
		lock (writerListLockObj)
		{
			writerTriggerEvent.Set();
		}
	}

	public override void AppendToReaderStream(byte[] _data, int _dataSize)
	{
		if (!bDisconnected)
		{
			lock (receivedBuffers)
			{
				receivedBuffers.Enqueue(new RecvBuffer(_data, _dataSize));
			}
			readerTriggerEvent.Set();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void taskDeserialize(ThreadManager.ThreadInfo _threadInfo)
	{
		bool flag = false;
		int num = 0;
		bool compressed = false;
		bool flag2 = false;
		int num2 = 0;
		int num3 = 0;
		try
		{
			while (!bDisconnected && !_threadInfo.TerminationRequested())
			{
				readerTriggerEvent.WaitOne(8);
				if (bDisconnected)
				{
					break;
				}
				RecvBuffer recvBuffer;
				lock (receivedBuffers)
				{
					if (receivedBuffers.Count == 0)
					{
						readerTriggerEvent.Reset();
						continue;
					}
					recvBuffer = receivedBuffers.Dequeue();
				}
				int offset = reservedHeaderBytes;
				if (!flag)
				{
					num3 = 0;
					num = StreamUtils.ReadInt32(recvBuffer.Data, ref offset);
					receiveStreamCompressed.Position = 0L;
					if (num > receiveStreamCompressed.Capacity)
					{
						if (cInfo != null)
						{
							Log.Error($"NCSimple_Deserializer (cl={cInfo.InternalId.CombinedString}, ch={channel}) Received message with size {num} > capacity {receiveStreamCompressed.Capacity}");
						}
						else
						{
							Log.Error($"NCSimple_Deserializer (ch={channel}) Received message with size {num} > capacity {receiveStreamCompressed.Capacity}");
						}
					}
					receiveStreamCompressed.SetLength(num);
					compressed = StreamUtils.ReadByte(recvBuffer.Data, ref offset) == 1;
					flag2 = StreamUtils.ReadByte(recvBuffer.Data, ref offset) == 1;
					num2 = StreamUtils.ReadUInt16(recvBuffer.Data, ref offset);
					if (num2 == 0)
					{
						continue;
					}
					flag = true;
				}
				while (num3 < num && offset < recvBuffer.Size)
				{
					int num4 = recvBuffer.Size - offset;
					receiveStreamCompressed.Write(recvBuffer.Data, offset, num4);
					num3 += num4;
					offset += num4;
				}
				MemoryPools.poolByte.Free(recvBuffer.Data);
				if (num3 < num)
				{
					continue;
				}
				receiveStreamCompressed.Position = 0L;
				flag = false;
				num3 = 0;
				stats.RegisterReceivedData(num2, num);
				if (!Decrypt(flag2, receiveStreamCompressed))
				{
					continue;
				}
				receiveStreamReader.SetBaseStream(Decompress(compressed, receiveStreamUncompressed, receiveZipStream, readerBuffer) ? receiveStreamUncompressed : receiveStreamCompressed);
				for (int i = 0; i < num2; i++)
				{
					int num5 = receiveStreamReader.ReadInt32();
					if (doCrash)
					{
						num5++;
						doCrash = false;
					}
					long position = receiveStreamReader.BaseStream.Position;
					NetPackage netPackage = NetPackageManager.ParsePackage(receiveStreamReader, cInfo);
					int num6 = (int)(receiveStreamReader.BaseStream.Position - position);
					if (num6 != num5)
					{
						throw new InvalidDataException("Parsed data size (" + num6 + ") does not match expected size (" + num5 + ") in " + netPackage);
					}
					lock (receivedPackages)
					{
						receivedPackages.Add(netPackage);
					}
					int packageId = netPackage.PackageId;
					stats.RegisterReceivedPackage(packageId, num6);
					NetPackageLogger.LogPackage(_dirIsOut: false, cInfo, netPackage, channel, num6, flag2, compressed, i + 1, num2);
					if (ConnectionManager.VerboseNetLogging)
					{
						if (cInfo != null)
						{
							Log.Out("NCSimple deserialized (cl={3}, ch={0}): {1}, size={2}", channel, NetPackageManager.GetPackageName(packageId), num6, cInfo.InternalId.CombinedString);
						}
						else
						{
							Log.Out("NCSimple deserialized (ch={0}): {1}, size={2}", channel, NetPackageManager.GetPackageName(packageId), num6);
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			if (cInfo != null)
			{
				Log.Error($"NCSimple_Deserializer (cl={cInfo.InternalId.CombinedString}, ch={channel}) Message: {ex.Message}");
			}
			else
			{
				Log.Error($"NCSimple_Deserializer (ch={channel}) Message: {ex.Message}");
			}
			Log.Exception(ex);
			if (isServer)
			{
				Disconnect(_kick: true);
			}
			else
			{
				GameUtils.ForceDisconnect();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void taskSerialize(ThreadManager.ThreadInfo _threadInfo)
	{
		writerListProcessing.Clear();
		bool flag = false;
		MicroStopwatch microStopwatch = new MicroStopwatch();
		try
		{
			while (!bDisconnected && !_threadInfo.TerminationRequested())
			{
				if ((!flag || microStopwatch.ElapsedMilliseconds > 500) && (reliableBufsToSend.Count > 0 || unreliableBufsToSend.Count > 0))
				{
					try
					{
						NetworkError networkError = SendBuffers();
						flag = networkError != NetworkError.Ok;
						switch (networkError)
						{
						case NetworkError.WrongConnection:
						case NetworkError.WrongOperation:
							Disconnect(_kick: true);
							break;
						case NetworkError.NoResources:
							if (isServer)
							{
								Log.Warning("NET No resources to send data to client ({0}) on channel {2}, backing off for {1} ms", cInfo.ToString(), 500, channel);
							}
							else
							{
								Log.Warning("NET No resources to send data to server on channel {1}, backing off for {0} ms", 500, channel);
							}
							microStopwatch.ResetAndRestart();
							break;
						default:
							if (isServer)
							{
								Log.Warning("NET Unexpected result '{2}' trying to send data to client ({0}) on channel {1}", cInfo.ToString(), channel, networkError.ToStringCached());
							}
							else
							{
								Log.Warning("NET Unexpected result '{1}' trying to send data to server on channel {0}", channel, networkError.ToStringCached());
							}
							break;
						case NetworkError.Ok:
							break;
						}
					}
					catch (Exception e)
					{
						Log.Exception(e);
						Disconnect(_kick: true);
					}
				}
				writerTriggerEvent.WaitOne(4);
				if (bDisconnected)
				{
					break;
				}
				int count;
				lock (writerListLockObj)
				{
					count = writerListFilling.Count;
				}
				if (count == 0)
				{
					writerTriggerEvent.Reset();
					continue;
				}
				lock (writerListLockObj)
				{
					List<NetPackage> list = writerListFilling;
					List<NetPackage> list2 = writerListProcessing;
					writerListProcessing = list;
					writerListFilling = list2;
					writerListFilling.Clear();
				}
				if (writerListProcessing.Count == 0)
				{
					continue;
				}
				reliableSendStreamUncompressed.Position = 0L;
				reliableSendStreamUncompressed.SetLength(0L);
				unreliableSendStreamUncompressed.Position = 0L;
				unreliableSendStreamUncompressed.SetLength(0L);
				int packagesToSend = 0;
				int packagesToSend2 = 0;
				int num = 0;
				int num2 = 0;
				for (int i = 0; i < writerListProcessing.Count; i++)
				{
					NetPackage package = writerListProcessing[i];
					bool flag2;
					if (package.ReliableDelivery || !GameManager.unreliableNetPackets)
					{
						flag2 = WriteToStream(num, i, ref packagesToSend, ref package, ref reliableSendStreamWriter, ref reliableSendStreamUncompressed);
						num++;
					}
					else
					{
						long position = unreliableSendStreamWriter.BaseStream.Position;
						if (cInfo != null && position + package.GetLength() >= cInfo.network.GetMaximumPacketSize(cInfo))
						{
							flag2 = WriteToStream(num, i, ref packagesToSend, ref package, ref reliableSendStreamWriter, ref reliableSendStreamUncompressed);
							num++;
						}
						else
						{
							flag2 = WriteToStream(num2, i, ref packagesToSend2, ref package, ref unreliableSendStreamWriter, ref unreliableSendStreamUncompressed);
							num2++;
						}
					}
					if (!flag2)
					{
						break;
					}
				}
				if (reliableSendStreamUncompressed.Length > 0)
				{
					StreamToBuffer(ref reliableSendStreamUncompressed, sendReliable: true, ref packagesToSend, microStopwatch.ElapsedMilliseconds);
				}
				if (unreliableSendStreamUncompressed.Length > 0)
				{
					StreamToBuffer(ref unreliableSendStreamUncompressed, sendReliable: false, ref packagesToSend2, microStopwatch.ElapsedMilliseconds);
				}
			}
		}
		catch (Exception ex)
		{
			if (cInfo != null)
			{
				Log.Error($"NCSimple_Serializer (cl={cInfo.InternalId.CombinedString}, ch={channel}) Message: {ex.Message}");
			}
			else
			{
				Log.Error($"NCSimple_Serializer (ch={channel}) Message: {ex.Message}");
			}
			Log.Exception(ex);
			if (isServer)
			{
				Disconnect(_kick: true);
			}
			else
			{
				GameUtils.ForceDisconnect();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool WriteToStream(int streamIndex, int packageIndex, ref int packagesToSend, ref NetPackage package, ref PooledBinaryWriter writer, ref MemoryStream stream)
	{
		long position = writer.BaseStream.Position;
		if (streamIndex > 0 && position + package.GetLength() >= preCompressMaxBufferSize)
		{
			lock (writerListLockObj)
			{
				for (int num = writerListProcessing.Count - 1; num >= packageIndex; num--)
				{
					writerListFilling.Insert(0, writerListProcessing[num]);
				}
			}
			return false;
		}
		try
		{
			writer.Write(-1);
			package.write(writer);
			int num2 = (int)(writer.BaseStream.Position - position - 4);
			writer.BaseStream.Position = position;
			writer.Write(num2);
			writer.BaseStream.Position = writer.BaseStream.Length;
			packagesToSend++;
			int packageId = package.PackageId;
			stats.RegisterSentPackage(packageId, num2);
			NetPackageLogger.LogPackage(_dirIsOut: true, cInfo, package, channel, num2, EnableEncryptData(), _compressed: false, packagesToSend, -1);
			if (ConnectionManager.VerboseNetLogging)
			{
				if (cInfo != null)
				{
					Log.Out("NCSimple serialized (cl={3}, ch={0}): {1}, size={2}", channel, NetPackageManager.GetPackageName(packageId), num2, cInfo.InternalId.CombinedString);
				}
				else
				{
					Log.Out("NCSimple serialized (ch={0}): {1}, size={2}", channel, NetPackageManager.GetPackageName(packageId), num2);
				}
			}
			package.SendQueueHandled();
			return true;
		}
		catch (Exception e)
		{
			if (packagesToSend > 0)
			{
				string text = ((cInfo == null) ? $"(ch={channel})" : $"(cl={cInfo.InternalId.CombinedString}, ch={channel})");
				Log.Exception(e);
				Log.Warning("Failed writing " + package?.ToString() + " to client " + text + ", requeueing " + (writerListProcessing.Count - packageIndex) + " packages. Stream index: " + (streamIndex + 1) + " - stream size before: " + position);
				Log.Warning("Packages in stream:");
				int[] array = new int[NetPackageManager.KnownPackageCount];
				for (int i = 0; i < packageIndex; i++)
				{
					array[writerListProcessing[i].PackageId]++;
				}
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j] > 0)
					{
						Log.Warning("   " + NetPackageManager.GetPackageName(j) + ": " + array[j]);
					}
				}
				lock (writerListLockObj)
				{
					for (int num3 = writerListProcessing.Count - 1; num3 >= packageIndex; num3--)
					{
						writerListFilling.Insert(0, writerListProcessing[num3]);
					}
				}
				stream.SetLength(position);
				writerListProcessing.Clear();
				return false;
			}
			Log.Exception(e);
			Log.Warning("Failed writing first package: " + package?.ToString() + " of size " + package.GetLength() + ". " + (writerListProcessing.Count - packageIndex) + " remaining packages in queue.");
			package.SendQueueHandled();
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StreamToBuffer(ref MemoryStream uncompressedStream, bool sendReliable, ref int packagesToSend, long milliseconds)
	{
		if (uncompressedStream.Length != 0L)
		{
			writerListProcessing.Clear();
			uncompressedStream.Position = 0L;
			_ = uncompressedStream.Length;
			bool flag = allowCompression && uncompressedStream.Length > 500;
			MemoryStream memoryStream = uncompressedStream;
			if (Compress(flag, uncompressedStream, sendZipStream, sendStreamCompressed, writerBuffer, packagesToSend))
			{
				memoryStream = sendStreamCompressed;
				_ = sendStreamCompressed.Length;
			}
			bool flag2 = Encrypt(memoryStream);
			stats.RegisterSentData(packagesToSend, (int)memoryStream.Length);
			StreamUtils.Write(writerStream, (int)memoryStream.Length);
			StreamUtils.Write(writerStream, (byte)(flag ? 1u : 0u));
			StreamUtils.Write(writerStream, (byte)(flag2 ? 1u : 0u));
			StreamUtils.Write(writerStream, (ushort)packagesToSend);
			if (memoryStream.Length > writerStream.Capacity)
			{
				Log.Error($"Source stream size ({memoryStream.Length}) > writer stream capacity ({writerStream.Capacity}), packages: {packagesToSend}, compressed: {flag}");
			}
			StreamUtils.StreamCopy(memoryStream, writerStream, writerBuffer);
			writerStream.Position = 0L;
			ArrayListMP<byte> arrayListMP = new ArrayListMP<byte>(MemoryPools.poolByte, (int)writerStream.Length + reservedHeaderBytes);
			writerStream.Read(arrayListMP.Items, reservedHeaderBytes, (int)writerStream.Length);
			arrayListMP.Count = (int)(writerStream.Length + reservedHeaderBytes);
			if (sendReliable)
			{
				reliableBufsToSend.AddLast(arrayListMP);
			}
			else
			{
				unreliableBufsToSend.AddLast(arrayListMP);
			}
			writerStream.SetLength(0L);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public NetworkError SendBuffers()
	{
		NetworkError result;
		if ((result = sendBuffersFromQueue(reliableBufsToSend, _reliableDelivery: true)) != NetworkError.Ok)
		{
			return result;
		}
		if ((result = sendBuffersFromQueue(unreliableBufsToSend, _reliableDelivery: false)) != NetworkError.Ok)
		{
			return result;
		}
		return NetworkError.Ok;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public NetworkError sendBuffersFromQueue(LinkedList<ArrayListMP<byte>> _sendQueue, bool _reliableDelivery)
	{
		while (_sendQueue.Count > 0)
		{
			ArrayListMP<byte> arrayListMP = _sendQueue.First.Value;
			_sendQueue.RemoveFirst();
			NetworkError networkError = NetworkError.Ok;
			if (!bDisconnected)
			{
				if (maxPacketSize > 0 && arrayListMP.Count > maxPacketSize)
				{
					arrayListMP = splitSendBuffer(arrayListMP, _sendQueue);
				}
				networkError = ((!isServer) ? netClient.SendData(channel, arrayListMP) : cInfo.network.SendData(cInfo, channel, arrayListMP, _reliableDelivery));
			}
			if (networkError != NetworkError.Ok)
			{
				_sendQueue.AddFirst(arrayListMP);
				return networkError;
			}
		}
		return NetworkError.Ok;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ArrayListMP<byte> splitSendBuffer(ArrayListMP<byte> _inBuf, LinkedList<ArrayListMP<byte>> _sendQueue)
	{
		int num = _inBuf.Count - reservedHeaderBytes;
		int num2 = num / maxPayloadPerPacket;
		if (num2 * maxPayloadPerPacket < num)
		{
			num2++;
		}
		int num3 = num2 - 1;
		while (num3 >= 0)
		{
			int num4 = num3 * maxPayloadPerPacket;
			int num5 = ((num3 != num2 - 1) ? maxPayloadPerPacket : (num - num4));
			ArrayListMP<byte> arrayListMP = new ArrayListMP<byte>(MemoryPools.poolByte, num5 + reservedHeaderBytes);
			Array.Copy(_inBuf.Items, num4 + reservedHeaderBytes, arrayListMP.Items, 1, num5);
			arrayListMP.Count = num5 + reservedHeaderBytes;
			if (num3 > 0)
			{
				_sendQueue.AddFirst(arrayListMP);
				num3--;
				continue;
			}
			return arrayListMP;
		}
		return null;
	}
}
