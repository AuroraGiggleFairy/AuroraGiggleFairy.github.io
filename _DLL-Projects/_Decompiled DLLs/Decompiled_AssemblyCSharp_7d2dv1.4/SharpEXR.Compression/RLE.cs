namespace SharpEXR.Compression;

public static class RLE
{
	public static int Uncompress(IEXRReader reader, int count, byte[] uncompressed)
	{
		int num = reader.Position + count;
		int num2 = uncompressed.Length;
		int num3 = 0;
		while (reader.Position < num)
		{
			int num4 = (sbyte)reader.ReadByte();
			if (num4 < 0)
			{
				num4 = -num4;
				if (num3 + num4 >= num2)
				{
					reader.Position--;
					return 0;
				}
				reader.CopyBytes(uncompressed, num3, num4);
				num3 += num4;
			}
			else
			{
				if (num3 + num4 + 1 >= num2)
				{
					reader.Position--;
					return 0;
				}
				byte value = reader.ReadByte();
				MemSet(uncompressed, num3, value, num4 + 1);
				num3 += num4 + 1;
			}
		}
		return num3;
	}

	public static void MemSet(byte[] array, int offset, byte value, int count)
	{
		int num = offset + count;
		for (int i = offset; i < num; i++)
		{
			array[i] = value;
		}
	}
}
