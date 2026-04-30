using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Discord.Net.ED25519;

internal class CryptoBytes
{
	private const string strDigits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

	public static bool ConstantTimeEquals(byte[] x, byte[] y)
	{
		if (x.Length != y.Length)
		{
			return false;
		}
		return InternalConstantTimeEquals(x, 0, y, 0, x.Length) != 0;
	}

	public static bool ConstantTimeEquals(ArraySegment<byte> x, ArraySegment<byte> y)
	{
		if (x.Count != y.Count)
		{
			return false;
		}
		return InternalConstantTimeEquals(x.Array, x.Offset, y.Array, y.Offset, x.Count) != 0;
	}

	public static bool ConstantTimeEquals(byte[] x, int xOffset, byte[] y, int yOffset, int length)
	{
		return InternalConstantTimeEquals(x, xOffset, y, yOffset, length) != 0;
	}

	private static uint InternalConstantTimeEquals(byte[] x, int xOffset, byte[] y, int yOffset, int length)
	{
		int num = 0;
		for (int i = 0; i < length; i++)
		{
			num |= x[xOffset + i] ^ y[yOffset + i];
		}
		return (uint)(1 & (num - 1 >>> 8));
	}

	public static void Wipe(byte[] data)
	{
		InternalWipe(data, 0, data.Length);
	}

	public static void Wipe(byte[] data, int offset, int length)
	{
		InternalWipe(data, offset, length);
	}

	public static void Wipe(ArraySegment<byte> data)
	{
		InternalWipe(data.Array, data.Offset, data.Count);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void InternalWipe(byte[] data, int offset, int count)
	{
		Array.Clear(data, offset, count);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static void InternalWipe<T>(ref T data) where T : struct
	{
		data = default(T);
	}

	public static string ToHexStringUpper(byte[] data)
	{
		if (data == null)
		{
			return null;
		}
		char[] array = new char[data.Length * 2];
		for (int i = 0; i < data.Length; i++)
		{
			int num = data[i] >> 4;
			array[i * 2] = (char)(55 + num + ((num - 10 >> 31) & -7));
			num = data[i] & 0xF;
			array[i * 2 + 1] = (char)(55 + num + ((num - 10 >> 31) & -7));
		}
		return new string(array);
	}

	public static string ToHexStringLower(byte[] data)
	{
		if (data == null)
		{
			return null;
		}
		char[] array = new char[data.Length * 2];
		for (int i = 0; i < data.Length; i++)
		{
			int num = data[i] >> 4;
			array[i * 2] = (char)(87 + num + ((num - 10 >> 31) & -39));
			num = data[i] & 0xF;
			array[i * 2 + 1] = (char)(87 + num + ((num - 10 >> 31) & -39));
		}
		return new string(array);
	}

	public static byte[] FromHexString(string hexString)
	{
		if (hexString == null)
		{
			return null;
		}
		if (hexString.Length % 2 != 0)
		{
			throw new FormatException("The hex string is invalid because it has an odd length");
		}
		byte[] array = new byte[hexString.Length / 2];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
		}
		return array;
	}

	public static string ToBase64String(byte[] data)
	{
		if (data == null)
		{
			return null;
		}
		return Convert.ToBase64String(data);
	}

	public static byte[] FromBase64String(string base64String)
	{
		if (base64String == null)
		{
			return null;
		}
		return Convert.FromBase64String(base64String);
	}

	public static string Base58Encode(byte[] input)
	{
		BigInteger bigInteger = 0;
		for (int i = 0; i < input.Length; i++)
		{
			bigInteger = bigInteger * 256 + input[i];
		}
		string text = "";
		while (bigInteger > 0L)
		{
			int index = (int)(bigInteger % 58);
			bigInteger /= (BigInteger)58;
			text = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz"[index] + text;
		}
		for (int j = 0; j < input.Length && input[j] == 0; j++)
		{
			text = "1" + text;
		}
		return text;
	}

	public static byte[] Base58Decode(string input)
	{
		BigInteger bigInteger = 0;
		for (int i = 0; i < input.Length; i++)
		{
			int num = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz".IndexOf(input[i]);
			if (num < 0)
			{
				throw new FormatException($"Invalid Base58 character `{input[i]}` at position {i}");
			}
			bigInteger = bigInteger * 58 + num;
		}
		int count = input.TakeWhile((char c) => c == '1').Count();
		IEnumerable<byte> first = Enumerable.Repeat((byte)0, count);
		IEnumerable<byte> second = Enumerable.Reverse(bigInteger.ToByteArray()).SkipWhile((byte b) => b == 0);
		return first.Concat(second).ToArray();
	}
}
