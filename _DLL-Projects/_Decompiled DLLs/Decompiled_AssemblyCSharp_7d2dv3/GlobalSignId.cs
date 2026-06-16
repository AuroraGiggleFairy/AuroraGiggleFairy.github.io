using System;
using System.IO;

public readonly struct GlobalSignId(string libraryId, Guid signGuid) : IEquatable<GlobalSignId>
{
	public readonly string libraryId = libraryId;

	public readonly Guid signGuid = signGuid;

	public static GlobalSignId InvalidId => new GlobalSignId(string.Empty, Guid.Empty);

	public bool IsValid => !string.IsNullOrEmpty(libraryId);

	public bool Equals(GlobalSignId other)
	{
		if (libraryId == other.libraryId)
		{
			return signGuid == other.signGuid;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is GlobalSignId other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(libraryId, signGuid);
	}

	public override string ToString()
	{
		return $"{libraryId}:{signGuid}";
	}

	public static bool operator ==(GlobalSignId a, GlobalSignId b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(GlobalSignId a, GlobalSignId b)
	{
		return !a.Equals(b);
	}

	public static GlobalSignId FromStream(BinaryReader _br)
	{
		string text = _br.ReadString();
		Span<byte> span = stackalloc byte[16];
		if (_br.Read(span) != 16)
		{
			Log.Error("Invalid Guid data.");
		}
		Guid guid = new Guid(span);
		return new GlobalSignId(text, guid);
	}

	public void ToStream(BinaryWriter _bw)
	{
		_bw.Write(libraryId);
		Span<byte> span = stackalloc byte[16];
		signGuid.TryWriteBytes(span);
		_bw.Write(span);
	}
}
