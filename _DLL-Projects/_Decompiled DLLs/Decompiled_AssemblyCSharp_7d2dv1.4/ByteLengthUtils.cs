using System.Text;

public static class ByteLengthUtils
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static int GetBinaryWriter7BitEncodedIntLength(int value)
	{
		int num = 0;
		for (uint num2 = (uint)value; num2 >= 128; num2 >>= 7)
		{
			num++;
		}
		return num + 1;
	}

	public static int GetBinaryWriterLength(this string text, Encoding encoding)
	{
		int byteCount = encoding.GetByteCount(text);
		return GetBinaryWriter7BitEncodedIntLength(byteCount) + byteCount;
	}
}
