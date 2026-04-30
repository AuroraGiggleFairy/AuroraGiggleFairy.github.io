using System;
using System.IO;

public static class BinaryReaderExtensions
{
	public static bool TryReadAllBytes(this BinaryReader reader, Span<byte> dest)
	{
		int totalBytesRead;
		return reader.TryReadAllBytes(dest, out totalBytesRead);
	}

	public static bool TryReadAllBytes(this BinaryReader reader, Span<byte> dest, out int totalBytesRead)
	{
		totalBytesRead = 0;
		while (totalBytesRead < dest.Length)
		{
			Span<byte> span = dest;
			int num = totalBytesRead;
			int num2 = reader.Read(span.Slice(num, span.Length - num));
			if (num2 <= 0)
			{
				return false;
			}
			totalBytesRead += num2;
		}
		return true;
	}
}
