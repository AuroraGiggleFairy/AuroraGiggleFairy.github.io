using UnityEngine.Scripting;

namespace ModSync
{
	/// <summary>
	/// Placeholder only. Never sent. Registered under spare names so the client's
	/// KnownPackageCount leaves room for the higher package ids a modded server hands out.
	/// </summary>
	[Preserve]
	public class NetPackageModSyncReserved : NetPackage
	{
		public override void read(PooledBinaryReader _reader)
		{
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
		}

		public override int GetLength()
		{
			return 4;
		}
	}

	/// <summary>
	/// Client -> server: "I speak ModSync, send me what you have."
	/// Sent right after the package id mappings arrive, before login.
	/// </summary>
	[Preserve]
	public class NetPackageModSyncHello : NetPackage
	{
		private int protocol;

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

		public override bool AllowedBeforeAuth => true;

		public override bool FlushQueue => true;

		public NetPackageModSyncHello Setup()
		{
			protocol = 1;
			return this;
		}

		public override void read(PooledBinaryReader _reader)
		{
			protocol = _reader.ReadInt32();
		}

		public override void write(PooledBinaryWriter _writer)
		{
			base.write(_writer);
			_writer.Write(protocol);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			ModSyncServer.OnHello(Sender, protocol);
		}

		public override int GetLength()
		{
			return 8;
		}
	}

	/// <summary>Server -> client: one gzipped slice of the file manifest.</summary>
	[Preserve]
	public class NetPackageModSyncManifest : NetPackage
	{
		private int version;
		private int chunkIndex;
		private int chunkCount;
		private byte[] data;

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

		public override bool AllowedBeforeAuth => true;

		public override bool FlushQueue => true;

		public NetPackageModSyncManifest Setup(int _version, int _chunkIndex, int _chunkCount, byte[] _data)
		{
			version = _version;
			chunkIndex = _chunkIndex;
			chunkCount = _chunkCount;
			data = _data;
			return this;
		}

		public override void read(PooledBinaryReader _reader)
		{
			version = _reader.ReadInt32();
			chunkIndex = _reader.ReadInt32();
			chunkCount = _reader.ReadInt32();
			int len = _reader.ReadInt32();
			data = _reader.ReadBytes(len);
		}

		public override void write(PooledBinaryWriter _writer)
		{
			base.write(_writer);
			_writer.Write(version);
			_writer.Write(chunkIndex);
			_writer.Write(chunkCount);
			_writer.Write(data.Length);
			_writer.Write(data);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			ModSyncClient.OnManifestChunk(version, chunkIndex, chunkCount, data);
		}

		public override int GetLength()
		{
			return 20 + (data != null ? data.Length : 0);
		}
	}

	/// <summary>Client -> server: the manifest indices this client is missing or has out of date.</summary>
	[Preserve]
	public class NetPackageModSyncRequest : NetPackage
	{
		private int version;
		private int[] indices;

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

		public override bool AllowedBeforeAuth => true;

		public override bool FlushQueue => true;

		public NetPackageModSyncRequest Setup(int _version, int[] _indices)
		{
			version = _version;
			indices = _indices;
			return this;
		}

		public override void read(PooledBinaryReader _reader)
		{
			version = _reader.ReadInt32();
			int count = _reader.ReadInt32();
			indices = new int[count];
			for (int i = 0; i < count; i++)
			{
				indices[i] = _reader.ReadInt32();
			}
		}

		public override void write(PooledBinaryWriter _writer)
		{
			base.write(_writer);
			_writer.Write(version);
			_writer.Write(indices.Length);
			for (int i = 0; i < indices.Length; i++)
			{
				_writer.Write(indices[i]);
			}
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			ModSyncServer.OnRequest(Sender, version, indices);
		}

		public override int GetLength()
		{
			return 12 + (indices != null ? indices.Length * 4 : 0);
		}
	}

	/// <summary>Server -> client: one slice of one file.</summary>
	[Preserve]
	public class NetPackageModSyncData : NetPackage
	{
		private int fileIndex;
		private int chunkIndex;
		private int chunkCount;
		private byte[] data;

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

		public override bool AllowedBeforeAuth => true;

		public NetPackageModSyncData Setup(int _fileIndex, int _chunkIndex, int _chunkCount, byte[] _data)
		{
			fileIndex = _fileIndex;
			chunkIndex = _chunkIndex;
			chunkCount = _chunkCount;
			data = _data;
			return this;
		}

		public override void read(PooledBinaryReader _reader)
		{
			fileIndex = _reader.ReadInt32();
			chunkIndex = _reader.ReadInt32();
			chunkCount = _reader.ReadInt32();
			int len = _reader.ReadInt32();
			data = _reader.ReadBytes(len);
		}

		public override void write(PooledBinaryWriter _writer)
		{
			base.write(_writer);
			_writer.Write(fileIndex);
			_writer.Write(chunkIndex);
			_writer.Write(chunkCount);
			_writer.Write(data.Length);
			_writer.Write(data);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			ModSyncClient.OnFileChunk(fileIndex, chunkIndex, chunkCount, data);
		}

		public override int GetLength()
		{
			return 20 + (data != null ? data.Length : 0);
		}
	}

	/// <summary>Client -> server: flow control. Tells the server how many chunks have landed.</summary>
	[Preserve]
	public class NetPackageModSyncAck : NetPackage
	{
		private int received;

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

		public override bool AllowedBeforeAuth => true;

		public override bool FlushQueue => true;

		public NetPackageModSyncAck Setup(int _received)
		{
			received = _received;
			return this;
		}

		public override void read(PooledBinaryReader _reader)
		{
			received = _reader.ReadInt32();
		}

		public override void write(PooledBinaryWriter _writer)
		{
			base.write(_writer);
			_writer.Write(received);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			ModSyncServer.OnAck(Sender, received);
		}

		public override int GetLength()
		{
			return 8;
		}
	}

	/// <summary>
	/// Server -> client: you are queued behind other downloaders. Sent while waiting so a
	/// player on a busy server sees a place in line instead of a frozen progress bar.
	/// </summary>
	[Preserve]
	public class NetPackageModSyncWait : NetPackage
	{
		private int place;
		private int total;

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

		public override bool AllowedBeforeAuth => true;

		public override bool FlushQueue => true;

		public NetPackageModSyncWait Setup(int _place, int _total)
		{
			place = _place;
			total = _total;
			return this;
		}

		public override void read(PooledBinaryReader _reader)
		{
			place = _reader.ReadInt32();
			total = _reader.ReadInt32();
		}

		public override void write(PooledBinaryWriter _writer)
		{
			base.write(_writer);
			_writer.Write(place);
			_writer.Write(total);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			ModSyncClient.OnQueued(place, total);
		}

		public override int GetLength()
		{
			return 12;
		}
	}

	/// <summary>Server -> client: the transfer finished (or failed).</summary>
	[Preserve]
	public class NetPackageModSyncDone : NetPackage
	{
		private bool ok;
		private string message;

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

		public override bool AllowedBeforeAuth => true;

		public override bool FlushQueue => true;

		public NetPackageModSyncDone Setup(bool _ok, string _message)
		{
			ok = _ok;
			message = _message ?? "";
			return this;
		}

		public override void read(PooledBinaryReader _reader)
		{
			ok = _reader.ReadBoolean();
			message = _reader.ReadString();
		}

		public override void write(PooledBinaryWriter _writer)
		{
			base.write(_writer);
			_writer.Write(ok);
			_writer.Write(message ?? "");
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			ModSyncClient.OnTransferDone(ok, message);
		}

		public override int GetLength()
		{
			return 8 + (message != null ? message.Length * 2 : 0);
		}
	}
}
