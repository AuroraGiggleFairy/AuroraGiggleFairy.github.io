using System;
using System.Threading;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageDynamicMesh : DynamicMeshServerData, IMemoryPoolableObject
{
	public static byte[] DelayedMessageBytes = new byte[1];

	public static int MaxMessageSize = 2097152;

	public static int MaxLength = 0;

	public static int LastLength;

	public static int LastZ;

	public static int LastX;

	public static int Count = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicMeshItem Item;

	public int Attempts;

	public int PresumedLength;

	public int UpdateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] bytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsValidUpdate;

	public override bool FlushQueue => true;

	public override NetPackageDirection PackageDirection => NetPackageDirection.Both;

	public override int Channel => 1;

	public override bool Compress => true;

	public void Setup(DynamicMeshItem item, byte[] byteArray)
	{
		Item = item;
		bytes = byteArray;
		X = item.WorldPosition.x;
		Z = item.WorldPosition.z;
		UpdateTime = item.UpdateTime;
		PresumedLength = Item?.PackageLength ?? 0;
	}

	public override bool Prechecks()
	{
		return true;
	}

	public override void read(PooledBinaryReader reader)
	{
		X = reader.ReadInt32();
		Z = reader.ReadInt32();
		UpdateTime = reader.ReadInt32();
		int num = reader.ReadInt32();
		Count++;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		string text = DynamicMeshFile.MeshLocation + X + "," + Z + ".mesh";
		if (DynamicMeshManager.DoLog)
		{
			Log.Out($"Reading {X},{Z} len {num}");
		}
		if (num == 0)
		{
			if (DynamicMeshManager.Instance != null)
			{
				DynamicMeshManager.Instance.ArrangeChunkRemoval(X, Z);
			}
			bytes = null;
			return;
		}
		int num2 = 0;
		bytes = DynamicMeshThread.ChunkDataQueue.GetFromPool(num);
		reader.Read(bytes, 0, num);
		if (string.IsNullOrWhiteSpace(DynamicMeshFile.MeshLocation))
		{
			IsValidUpdate = false;
			return;
		}
		while (num2++ < 10)
		{
			try
			{
				IsValidUpdate = DynamicMeshThread.ChunkDataQueue.SaveNetPackageData(X, Z, bytes, UpdateTime);
				break;
			}
			catch (Exception)
			{
				Log.Out("Failed attempt " + num2 + " to write mesh " + text + ". Retrying...");
				Thread.Sleep(1000);
			}
		}
	}

	public override void write(PooledBinaryWriter writer)
	{
		base.write(writer);
		writer.Write(X);
		writer.Write(Z);
		writer.Write(UpdateTime);
		int num = PresumedLength;
		long position = writer.BaseStream.Position;
		string text = "start";
		try
		{
			text = "len";
			if (DynamicMeshManager.DoLog)
			{
				Log.Out($"Sending {X},{Z} len {num}");
			}
			text = "lencheck";
			if (MaxLength < num)
			{
				MaxLength = num;
				if (DynamicMeshManager.DoLog)
				{
					Log.Out("New dyMesh maxLen: " + MaxLength);
				}
			}
			LastLength = num;
			LastX = X;
			LastZ = Z;
			text = "writelen";
			writer.Write(num);
			if (bytes != null)
			{
				text = "writebytes";
				if (bytes.Length < num)
				{
					text = "writebytecheck";
					Log.Warning("Dymesh byte length was lower than expected. Len was " + num + " and bytes were " + bytes.Length);
					num = (PresumedLength = bytes.Length);
				}
				text = "writenow";
				writer.Write(bytes, 0, num);
			}
		}
		catch (Exception ex)
		{
			Log.Error(string.Format(" ERROR MESH EXCEPTION\r\ndyMesh netWrite error: {0}\r\n({1},{2})\r\nLength: {3}\r\nLen: {4}\r\nbytes: {5}\r\nMaxLength: {6}\r\nwriterStartPosition: {7}\r\nwriterLength: {8}\r\nwriterPos: {9}\r\nstep: {10}\r\n", ex.Message, X, Z, num, num, bytes?.Length.ToString() ?? "null", MaxLength, position, writer.BaseStream.Length, writer.BaseStream.Position, text));
			throw ex;
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!DynamicMeshManager.CONTENT_ENABLED)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			DynamicMeshServer.ClientReadyForNextMesh(this);
			return;
		}
		if (IsValidUpdate)
		{
			DynamicMeshManager.AddDataFromServer(X, Z);
		}
		NetPackageDynamicMesh package = NetPackageManager.GetPackage<NetPackageDynamicMesh>();
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
	}

	public override int GetLength()
	{
		if (PresumedLength > 0)
		{
			return Math.Min(PresumedLength, MaxMessageSize);
		}
		if (bytes != null)
		{
			return 16 + bytes.Length;
		}
		return 16;
	}

	public void Reset()
	{
		DynamicMeshServer.SyncRelease(Item);
		Item = null;
		Attempts = 0;
		bytes = null;
		PresumedLength = 0;
	}

	public void Cleanup()
	{
		Reset();
	}
}
