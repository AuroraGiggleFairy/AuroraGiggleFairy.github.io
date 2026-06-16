using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace Platform.XBL.Save.MasterFileTable.Latest;

public sealed class BlobRef : IEquatable<BlobRef>, IMigratable
{
	public const ushort VERSION = 6;

	public const int HashSizeBytes = 16;

	public const int SizeBytes = 28;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ThreadLocal<MD5> s_md5 = new ThreadLocal<MD5>(MD5.Create);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] s_emptyHash = new byte[16];

	public ulong Id;

	public uint Length;

	[PublicizedFrom(EAccessModifier.Private)]
	public ReadOnlyMemory<byte> m_hash = s_emptyHash;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ushort Version
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = 6;

	public ReadOnlyMemory<byte> Hash
	{
		get
		{
			return m_hash;
		}
		set
		{
			if (16 != value.Length)
			{
				throw new InvalidOperationException($"Expected hash to be of length {16}");
			}
			m_hash = value;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte[] FutureData
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = Array.Empty<byte>();

	public void Write(PooledBinaryWriter writer)
	{
		writer.Write(Version);
		int value = 0;
		long position = writer.BaseStream.Position;
		writer.Write(value);
		long position2 = writer.BaseStream.Position;
		writer.Write(Id);
		writer.Write(Length);
		ReadOnlyMemory<byte> hash = Hash;
		if (16 != hash.Length)
		{
			Log.Error(string.Format("[{0}] Expected a hash size of exactly {1}, padding or truncating as needed.", "BlobRef", 16));
			hash = Hash;
			if (hash.Length < 16)
			{
				hash = Hash;
				writer.Write(hash.Span);
				hash = Hash;
				for (int i = hash.Length; i < 16; i++)
				{
					writer.Write((byte)0);
				}
			}
			else
			{
				hash = Hash;
				writer.Write(hash.Span.Slice(0, 16));
			}
		}
		else
		{
			hash = Hash;
			writer.Write(hash.Span);
		}
		writer.Write(FutureData);
		long position3 = writer.BaseStream.Position;
		value = (int)(position3 - position2);
		writer.BaseStream.Position = position;
		writer.Write(value);
		writer.BaseStream.Position = position3;
	}

	public void Read(PooledBinaryReader reader)
	{
		if (Version != 6 || Id != 0L || Length != 0 || !m_hash.Span.SequenceEqual(s_emptyHash) || FutureData.Length != 0)
		{
			throw new InvalidOperationException("Read should only be called on a new BlobRef.");
		}
		Version = reader.ReadUInt16();
		int num = reader.ReadInt32();
		long position = reader.BaseStream.Position;
		Id = reader.ReadUInt64();
		Length = reader.ReadUInt32();
		byte[] array = new byte[16];
		if (!reader.TryReadAllBytes(array, out var totalBytesRead))
		{
			Log.Error(string.Format("[{0}] Expected {1} for the hash bytes, but reached the end of stream after reading {2} bytes?", "BlobRef", 16, totalBytesRead));
		}
		Hash = array;
		long position2 = reader.BaseStream.Position;
		FutureData = new byte[(int)(num - (position2 - position))];
		if (!reader.TryReadAllBytes(FutureData, out var totalBytesRead2))
		{
			throw new IOException($"Expected {FutureData.Length} bytes to be read for future data but only got {totalBytesRead2} bytes.");
		}
	}

	public override string ToString()
	{
		return "BlobRef[Id=" + SaveContainer.IdToString(Id) + ", Length=" + Length.FormatSize() + ", Hash=" + Hash.ToHexString() + "]";
	}

	public static HashAlgorithm GetHashAlgorithm()
	{
		return s_md5.Value;
	}

	public static ReadOnlyMemory<byte> CalculateHash(RefCountedBuffer buffer)
	{
		HashAlgorithm hashAlgorithm = GetHashAlgorithm();
		if (128 != hashAlgorithm.HashSize)
		{
			throw new InvalidOperationException("Unexpected hash algorithm hash size.");
		}
		byte[] array = new byte[16];
		if (!hashAlgorithm.TryComputeHash(buffer.Span, array, out var _))
		{
			throw new InvalidOperationException("Expected to be able to compute the hash.");
		}
		return array;
	}

	public bool Equals(BlobRef other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (Id == other.Id && Length == other.Length)
		{
			ReadOnlyMemory<byte> hash = Hash;
			if (!hash.Equals(other.Hash))
			{
				hash = Hash;
				ReadOnlySpan<byte> span = hash.Span;
				hash = other.Hash;
				return span.SequenceEqual(hash.Span);
			}
			return true;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (this != obj)
		{
			if (obj is BlobRef other)
			{
				return Equals(other);
			}
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Id, Length, SpanUtils.GetHashCode(Hash.Span));
	}

	public static bool operator ==(BlobRef left, BlobRef right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(BlobRef left, BlobRef right)
	{
		return !object.Equals(left, right);
	}
}
