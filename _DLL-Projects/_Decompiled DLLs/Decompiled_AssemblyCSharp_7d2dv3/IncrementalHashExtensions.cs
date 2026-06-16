using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

public static class IncrementalHashExtensions
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] scratch = new byte[8];

	public static void AppendDataNoAlloc(this IncrementalHash hash, short value)
	{
		Unsafe.As<byte, short>(ref scratch[0]) = value;
		if (!BitConverter.IsLittleEndian)
		{
			Array.Reverse(scratch, 0, 2);
		}
		hash.AppendData(scratch, 0, 2);
	}

	public static void AppendDataNoAlloc(this IncrementalHash hash, ushort value)
	{
		Unsafe.As<byte, ushort>(ref scratch[0]) = value;
		if (!BitConverter.IsLittleEndian)
		{
			Array.Reverse(scratch, 0, 2);
		}
		hash.AppendData(scratch, 0, 2);
	}

	public static void AppendDataNoAlloc(this IncrementalHash hash, int value)
	{
		Unsafe.As<byte, int>(ref scratch[0]) = value;
		if (!BitConverter.IsLittleEndian)
		{
			Array.Reverse(scratch, 0, 4);
		}
		hash.AppendData(scratch, 0, 4);
	}

	public static void AppendDataNoAlloc(this IncrementalHash hash, uint value)
	{
		Unsafe.As<byte, uint>(ref scratch[0]) = value;
		if (!BitConverter.IsLittleEndian)
		{
			Array.Reverse(scratch, 0, 4);
		}
		hash.AppendData(scratch, 0, 4);
	}

	public static void AppendDataNoAlloc(this IncrementalHash hash, long value)
	{
		Unsafe.As<byte, long>(ref scratch[0]) = value;
		if (!BitConverter.IsLittleEndian)
		{
			Array.Reverse(scratch, 0, 8);
		}
		hash.AppendData(scratch, 0, 8);
	}

	public static void AppendDataNoAlloc(this IncrementalHash hash, ulong value)
	{
		Unsafe.As<byte, ulong>(ref scratch[0]) = value;
		if (!BitConverter.IsLittleEndian)
		{
			Array.Reverse(scratch, 0, 8);
		}
		hash.AppendData(scratch, 0, 8);
	}

	public static void AppendDataNoAlloc(this IncrementalHash hash, float value)
	{
		Unsafe.As<byte, float>(ref scratch[0]) = value;
		if (!BitConverter.IsLittleEndian)
		{
			Array.Reverse(scratch, 0, 4);
		}
		hash.AppendData(scratch, 0, 4);
	}

	public static void AppendDataNoAlloc(this IncrementalHash hash, bool value)
	{
		scratch[0] = (byte)(value ? 1 : 0);
		hash.AppendData(scratch, 0, 1);
	}
}
