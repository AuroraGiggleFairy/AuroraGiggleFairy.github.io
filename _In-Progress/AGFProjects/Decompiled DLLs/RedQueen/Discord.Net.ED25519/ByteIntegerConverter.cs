namespace Discord.Net.ED25519;

internal static class ByteIntegerConverter
{
	public static ulong LoadBigEndian64(byte[] buf, int offset)
	{
		return buf[offset + 7] | ((ulong)buf[offset + 6] << 8) | ((ulong)buf[offset + 5] << 16) | ((ulong)buf[offset + 4] << 24) | ((ulong)buf[offset + 3] << 32) | ((ulong)buf[offset + 2] << 40) | ((ulong)buf[offset + 1] << 48) | ((ulong)buf[offset] << 56);
	}

	public static void StoreBigEndian64(byte[] buf, int offset, ulong value)
	{
		buf[offset + 7] = (byte)value;
		buf[offset + 6] = (byte)(value >> 8);
		buf[offset + 5] = (byte)(value >> 16);
		buf[offset + 4] = (byte)(value >> 24);
		buf[offset + 3] = (byte)(value >> 32);
		buf[offset + 2] = (byte)(value >> 40);
		buf[offset + 1] = (byte)(value >> 48);
		buf[offset] = (byte)(value >> 56);
	}

	public static void Array16LoadBigEndian64(out Array16<ulong> output, byte[] input, int inputOffset)
	{
		output.x0 = LoadBigEndian64(input, inputOffset);
		output.x1 = LoadBigEndian64(input, inputOffset + 8);
		output.x2 = LoadBigEndian64(input, inputOffset + 16);
		output.x3 = LoadBigEndian64(input, inputOffset + 24);
		output.x4 = LoadBigEndian64(input, inputOffset + 32);
		output.x5 = LoadBigEndian64(input, inputOffset + 40);
		output.x6 = LoadBigEndian64(input, inputOffset + 48);
		output.x7 = LoadBigEndian64(input, inputOffset + 56);
		output.x8 = LoadBigEndian64(input, inputOffset + 64);
		output.x9 = LoadBigEndian64(input, inputOffset + 72);
		output.x10 = LoadBigEndian64(input, inputOffset + 80);
		output.x11 = LoadBigEndian64(input, inputOffset + 88);
		output.x12 = LoadBigEndian64(input, inputOffset + 96);
		output.x13 = LoadBigEndian64(input, inputOffset + 104);
		output.x14 = LoadBigEndian64(input, inputOffset + 112);
		output.x15 = LoadBigEndian64(input, inputOffset + 120);
	}
}
