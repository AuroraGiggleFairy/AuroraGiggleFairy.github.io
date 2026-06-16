using System;
using System.Security.Cryptography;
using System.Threading;

namespace Platform.XBL.Save.MasterFileTable.V04;

public sealed class BlobRef : IEquatable<BlobRef>
{
	public const int HashSizeBytes = 16;

	public const int SizeBytes = 28;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ThreadLocal<MD5> s_md5 = new ThreadLocal<MD5>(MD5.Create);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] s_emptyHash = new byte[16];

	public readonly ulong Id;

	public readonly uint Length;

	public readonly ReadOnlyMemory<byte> Hash;

	public BlobRef(ulong id, uint length)
		: this(id, length, s_emptyHash.AsMemory())
	{
	}

	public BlobRef(ulong id, uint length, ReadOnlySpan<byte> hash)
	{
		Id = id;
		Length = length;
		if (16 != hash.Length)
		{
			throw new InvalidOperationException("Expected hash to be of length " + 16);
		}
		Hash = hash.ToArray();
	}

	public BlobRef(ulong id, uint length, ReadOnlyMemory<byte> hash)
	{
		Id = id;
		Length = length;
		if (16 != hash.Length)
		{
			throw new InvalidOperationException("Expected hash to be of length " + 16);
		}
		Hash = hash;
	}

	public BlobRef(ulong id, RefCountedBuffer buffer)
	{
		Id = id;
		Length = (uint)buffer.Length;
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
		Hash = array;
	}

	public void Write(PooledBinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Length);
		if (16 != Hash.Length)
		{
			Log.Error("[BlobRef] Expected a hash size of exactly " + 16 + ", padding or truncating as needed.");
			if (Hash.Length < 16)
			{
				writer.Write(Hash.Span);
				for (int i = Hash.Length; i < 16; i++)
				{
					writer.Write((byte)0);
				}
			}
			else
			{
				writer.Write(Hash.Span.Slice(0, 16));
			}
		}
		else
		{
			writer.Write(Hash.Span);
		}
	}

	public static BlobRef Read(PooledBinaryReader reader)
	{
		ulong id = reader.ReadUInt64();
		uint length = reader.ReadUInt32();
		byte[] array = new byte[16];
		if (!reader.TryReadAllBytes(array, out var totalBytesRead))
		{
			Log.Error($"[BlobRef] Expected {16} for the hash bytes, but reached the end of stream after reading {totalBytesRead} bytes?");
		}
		return new BlobRef(id, length, array.AsMemory());
	}

	public override string ToString()
	{
		return "BlobRef[Id=" + SaveContainer.IdToString(Id) + ", Length=" + Length.FormatSize() + ", Hash=" + Hash.ToHexString() + "]";
	}

	public static HashAlgorithm GetHashAlgorithm()
	{
		return s_md5.Value;
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
			if (!Hash.Equals(other.Hash))
			{
				return Hash.Span.SequenceEqual(other.Hash.Span);
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
