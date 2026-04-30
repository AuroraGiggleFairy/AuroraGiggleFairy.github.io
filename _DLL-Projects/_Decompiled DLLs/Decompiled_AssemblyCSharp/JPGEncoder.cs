using System;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Profiling;

public class JPGEncoder
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class ByteArray
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public MemoryStream stream;

		[PublicizedFrom(EAccessModifier.Private)]
		public BinaryWriter writer;

		public ByteArray()
		{
			stream = new MemoryStream();
			writer = new BinaryWriter(stream);
		}

		public void WriteByte(byte value)
		{
			writer.Write(value);
		}

		public byte[] GetAllBytes()
		{
			byte[] array = new byte[stream.Length];
			stream.Position = 0L;
			stream.Read(array, 0, array.Length);
			return array;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct BitString
	{
		public int length;

		public int value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class BitmapData
	{
		public int height;

		public int width;

		[PublicizedFrom(EAccessModifier.Private)]
		public Color32[] pixels;

		public BitmapData(Texture2D texture)
		{
			height = texture.height;
			width = texture.width;
			pixels = texture.GetPixels32();
		}

		public Color32 GetPixelColor(int x, int y)
		{
			x = Mathf.Clamp(x, 0, width - 1);
			y = Mathf.Clamp(y, 0, height - 1);
			return pixels[y * width + x];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] ZigZag = new int[64]
	{
		0, 1, 5, 6, 14, 15, 27, 28, 2, 4,
		7, 13, 16, 26, 29, 42, 3, 8, 12, 17,
		25, 30, 41, 43, 9, 11, 18, 24, 31, 40,
		44, 53, 10, 19, 23, 32, 39, 45, 52, 54,
		20, 22, 33, 38, 46, 51, 55, 60, 21, 34,
		37, 47, 50, 56, 59, 61, 35, 36, 48, 49,
		57, 58, 62, 63
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] YTable = new int[64];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] UVTable = new int[64];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] fdtbl_Y = new float[64];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] fdtbl_UV = new float[64];

	[PublicizedFrom(EAccessModifier.Private)]
	public BitString[] YDC_HT;

	[PublicizedFrom(EAccessModifier.Private)]
	public BitString[] UVDC_HT;

	[PublicizedFrom(EAccessModifier.Private)]
	public BitString[] YAC_HT;

	[PublicizedFrom(EAccessModifier.Private)]
	public BitString[] UVAC_HT;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] std_dc_luminance_nrcodes = new byte[17]
	{
		0, 0, 1, 5, 1, 1, 1, 1, 1, 1,
		0, 0, 0, 0, 0, 0, 0
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] std_dc_luminance_values = new byte[12]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
		10, 11
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] std_ac_luminance_nrcodes = new byte[17]
	{
		0, 0, 2, 1, 3, 3, 2, 4, 3, 5,
		5, 4, 4, 0, 0, 1, 125
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] std_ac_luminance_values = new byte[162]
	{
		1, 2, 3, 0, 4, 17, 5, 18, 33, 49,
		65, 6, 19, 81, 97, 7, 34, 113, 20, 50,
		129, 145, 161, 8, 35, 66, 177, 193, 21, 82,
		209, 240, 36, 51, 98, 114, 130, 9, 10, 22,
		23, 24, 25, 26, 37, 38, 39, 40, 41, 42,
		52, 53, 54, 55, 56, 57, 58, 67, 68, 69,
		70, 71, 72, 73, 74, 83, 84, 85, 86, 87,
		88, 89, 90, 99, 100, 101, 102, 103, 104, 105,
		106, 115, 116, 117, 118, 119, 120, 121, 122, 131,
		132, 133, 134, 135, 136, 137, 138, 146, 147, 148,
		149, 150, 151, 152, 153, 154, 162, 163, 164, 165,
		166, 167, 168, 169, 170, 178, 179, 180, 181, 182,
		183, 184, 185, 186, 194, 195, 196, 197, 198, 199,
		200, 201, 202, 210, 211, 212, 213, 214, 215, 216,
		217, 218, 225, 226, 227, 228, 229, 230, 231, 232,
		233, 234, 241, 242, 243, 244, 245, 246, 247, 248,
		249, 250
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] std_dc_chrominance_nrcodes = new byte[17]
	{
		0, 0, 3, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 0, 0, 0, 0, 0
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] std_dc_chrominance_values = new byte[12]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
		10, 11
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] std_ac_chrominance_nrcodes = new byte[17]
	{
		0, 0, 2, 1, 2, 4, 4, 3, 4, 7,
		5, 4, 4, 0, 1, 2, 119
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] std_ac_chrominance_values = new byte[162]
	{
		0, 1, 2, 3, 17, 4, 5, 33, 49, 6,
		18, 65, 81, 7, 97, 113, 19, 34, 50, 129,
		8, 20, 66, 145, 161, 177, 193, 9, 35, 51,
		82, 240, 21, 98, 114, 209, 10, 22, 36, 52,
		225, 37, 241, 23, 24, 25, 26, 38, 39, 40,
		41, 42, 53, 54, 55, 56, 57, 58, 67, 68,
		69, 70, 71, 72, 73, 74, 83, 84, 85, 86,
		87, 88, 89, 90, 99, 100, 101, 102, 103, 104,
		105, 106, 115, 116, 117, 118, 119, 120, 121, 122,
		130, 131, 132, 133, 134, 135, 136, 137, 138, 146,
		147, 148, 149, 150, 151, 152, 153, 154, 162, 163,
		164, 165, 166, 167, 168, 169, 170, 178, 179, 180,
		181, 182, 183, 184, 185, 186, 194, 195, 196, 197,
		198, 199, 200, 201, 202, 210, 211, 212, 213, 214,
		215, 216, 217, 218, 226, 227, 228, 229, 230, 231,
		232, 233, 234, 242, 243, 244, 245, 246, 247, 248,
		249, 250
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public BitString[] bitcode = new BitString[65535];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] category = new int[65535];

	[PublicizedFrom(EAccessModifier.Private)]
	public uint bytenew;

	[PublicizedFrom(EAccessModifier.Private)]
	public int bytepos = 7;

	[PublicizedFrom(EAccessModifier.Private)]
	public ByteArray byteout = new ByteArray();

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] DU = new int[64];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] YDU = new float[64];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] UDU = new float[64];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] VDU = new float[64];

	public bool isDone;

	[PublicizedFrom(EAccessModifier.Private)]
	public BitmapData image;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sf;

	[PublicizedFrom(EAccessModifier.Private)]
	public string path;

	[PublicizedFrom(EAccessModifier.Private)]
	public int cores;

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitQuantTables(int sf)
	{
		int[] array = new int[64]
		{
			16, 11, 10, 16, 24, 40, 51, 61, 12, 12,
			14, 19, 26, 58, 60, 55, 14, 13, 16, 24,
			40, 57, 69, 56, 14, 17, 22, 29, 51, 87,
			80, 62, 18, 22, 37, 56, 68, 109, 103, 77,
			24, 35, 55, 64, 81, 104, 113, 92, 49, 64,
			78, 87, 103, 121, 120, 101, 72, 92, 95, 98,
			112, 100, 103, 99
		};
		int i;
		for (i = 0; i < 64; i++)
		{
			float value = Mathf.Floor((array[i] * sf + 50) / 100);
			value = Mathf.Clamp(value, 1f, 255f);
			YTable[ZigZag[i]] = Mathf.RoundToInt(value);
		}
		int[] array2 = new int[64]
		{
			17, 18, 24, 47, 99, 99, 99, 99, 18, 21,
			26, 66, 99, 99, 99, 99, 24, 26, 56, 99,
			99, 99, 99, 99, 47, 66, 99, 99, 99, 99,
			99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			99, 99, 99, 99
		};
		for (i = 0; i < 64; i++)
		{
			float value = Mathf.Floor((array2[i] * sf + 50) / 100);
			value = Mathf.Clamp(value, 1f, 255f);
			UVTable[ZigZag[i]] = (int)value;
		}
		float[] array3 = new float[8] { 1f, 1.3870399f, 1.306563f, 1.1758755f, 1f, 0.78569496f, 0.5411961f, 0.27589938f };
		i = 0;
		for (int j = 0; j < 8; j++)
		{
			for (int k = 0; k < 8; k++)
			{
				fdtbl_Y[i] = 1f / ((float)YTable[ZigZag[i]] * array3[j] * array3[k] * 8f);
				fdtbl_UV[i] = 1f / ((float)UVTable[ZigZag[i]] * array3[j] * array3[k] * 8f);
				i++;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BitString[] ComputeHuffmanTbl(byte[] nrcodes, byte[] std_table)
	{
		int num = 0;
		int num2 = 0;
		BitString[] array = new BitString[256];
		for (int i = 1; i <= 16; i++)
		{
			for (int j = 1; j <= nrcodes[i]; j++)
			{
				array[std_table[num2]] = default(BitString);
				array[std_table[num2]].value = num;
				array[std_table[num2]].length = i;
				num2++;
				num++;
			}
			num *= 2;
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitHuffmanTbl()
	{
		YDC_HT = ComputeHuffmanTbl(std_dc_luminance_nrcodes, std_dc_luminance_values);
		UVDC_HT = ComputeHuffmanTbl(std_dc_chrominance_nrcodes, std_dc_chrominance_values);
		YAC_HT = ComputeHuffmanTbl(std_ac_luminance_nrcodes, std_ac_luminance_values);
		UVAC_HT = ComputeHuffmanTbl(std_ac_chrominance_nrcodes, std_ac_chrominance_values);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitCategoryfloat()
	{
		int num = 1;
		int num2 = 2;
		for (int i = 1; i <= 15; i++)
		{
			for (int j = num; j < num2; j++)
			{
				category[32767 + j] = i;
				BitString bitString = new BitString
				{
					length = i,
					value = j
				};
				bitcode[32767 + j] = bitString;
			}
			for (int j = -(num2 - 1); j <= -num; j++)
			{
				category[32767 + j] = i;
				BitString bitString = new BitString
				{
					length = i,
					value = num2 - 1 + j
				};
				bitcode[32767 + j] = bitString;
			}
			num <<= 1;
			num2 <<= 1;
		}
	}

	public byte[] GetBytes()
	{
		if (!isDone)
		{
			Log.Error("JPEGEncoder not complete, cannot get bytes!");
			return null;
		}
		return byteout.GetAllBytes();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteBits(BitString bs)
	{
		int value = bs.value;
		int num = bs.length - 1;
		while (num >= 0)
		{
			if ((value & Convert.ToUInt32(1 << num)) != 0L)
			{
				bytenew |= Convert.ToUInt32(1 << bytepos);
			}
			num--;
			bytepos--;
			if (bytepos < 0)
			{
				if (bytenew == 255)
				{
					WriteByte(byte.MaxValue);
					WriteByte(0);
				}
				else
				{
					WriteByte((byte)bytenew);
				}
				bytepos = 7;
				bytenew = 0u;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteByte(byte value)
	{
		byteout.WriteByte(value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteWord(int value)
	{
		WriteByte((byte)((value >> 8) & 0xFF));
		WriteByte((byte)(value & 0xFF));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] FDCTQuant(float[] data, float[] fdtbl)
	{
		int num = 0;
		for (int i = 0; i < 8; i++)
		{
			float num2 = data[num] + data[num + 7];
			float num3 = data[num] - data[num + 7];
			float num4 = data[num + 1] + data[num + 6];
			float num5 = data[num + 1] - data[num + 6];
			float num6 = data[num + 2] + data[num + 5];
			float num7 = data[num + 2] - data[num + 5];
			float num8 = data[num + 3] + data[num + 4];
			float num9 = data[num + 3] - data[num + 4];
			float num10 = num2 + num8;
			float num11 = num2 - num8;
			float num12 = num4 + num6;
			float num13 = num4 - num6;
			data[num] = num10 + num12;
			data[num + 4] = num10 - num12;
			float num14 = (num13 + num11) * 0.70710677f;
			data[num + 2] = num11 + num14;
			data[num + 6] = num11 - num14;
			num10 = num9 + num7;
			num12 = num7 + num5;
			num13 = num5 + num3;
			float num15 = (num10 - num13) * 0.38268343f;
			float num16 = 0.5411961f * num10 + num15;
			float num17 = 1.306563f * num13 + num15;
			float num18 = num12 * 0.70710677f;
			float num19 = num3 + num18;
			float num20 = num3 - num18;
			data[num + 5] = num20 + num16;
			data[num + 3] = num20 - num16;
			data[num + 1] = num19 + num17;
			data[num + 7] = num19 - num17;
			num += 8;
		}
		num = 0;
		for (int i = 0; i < 8; i++)
		{
			float num2 = data[num] + data[num + 56];
			float num3 = data[num] - data[num + 56];
			float num4 = data[num + 8] + data[num + 48];
			float num5 = data[num + 8] - data[num + 48];
			float num6 = data[num + 16] + data[num + 40];
			float num7 = data[num + 16] - data[num + 40];
			float num8 = data[num + 24] + data[num + 32];
			float num21 = data[num + 24] - data[num + 32];
			float num10 = num2 + num8;
			float num11 = num2 - num8;
			float num12 = num4 + num6;
			float num13 = num4 - num6;
			data[num] = num10 + num12;
			data[num + 32] = num10 - num12;
			float num14 = (num13 + num11) * 0.70710677f;
			data[num + 16] = num11 + num14;
			data[num + 48] = num11 - num14;
			num10 = num21 + num7;
			num12 = num7 + num5;
			num13 = num5 + num3;
			float num15 = (num10 - num13) * 0.38268343f;
			float num16 = 0.5411961f * num10 + num15;
			float num17 = 1.306563f * num13 + num15;
			float num18 = num12 * 0.70710677f;
			float num19 = num3 + num18;
			float num20 = num3 - num18;
			data[num + 40] = num20 + num16;
			data[num + 24] = num20 - num16;
			data[num + 8] = num19 + num17;
			data[num + 56] = num19 - num17;
			num++;
		}
		for (int i = 0; i < 64; i++)
		{
			data[i] = Mathf.Round(data[i] * fdtbl[i]);
		}
		return data;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteAPP0()
	{
		WriteWord(65504);
		WriteWord(16);
		WriteByte(74);
		WriteByte(70);
		WriteByte(73);
		WriteByte(70);
		WriteByte(0);
		WriteByte(1);
		WriteByte(1);
		WriteByte(0);
		WriteWord(1);
		WriteWord(1);
		WriteByte(0);
		WriteByte(0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteSOF0(int width, int height)
	{
		WriteWord(65472);
		WriteWord(17);
		WriteByte(8);
		WriteWord(height);
		WriteWord(width);
		WriteByte(3);
		WriteByte(1);
		WriteByte(17);
		WriteByte(0);
		WriteByte(2);
		WriteByte(17);
		WriteByte(1);
		WriteByte(3);
		WriteByte(17);
		WriteByte(1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteDQT()
	{
		WriteWord(65499);
		WriteWord(132);
		WriteByte(0);
		for (int i = 0; i < 64; i++)
		{
			WriteByte((byte)YTable[i]);
		}
		WriteByte(1);
		for (int i = 0; i < 64; i++)
		{
			WriteByte((byte)UVTable[i]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteDHT()
	{
		WriteWord(65476);
		WriteWord(418);
		WriteByte(0);
		for (int i = 0; i < 16; i++)
		{
			WriteByte(std_dc_luminance_nrcodes[i + 1]);
		}
		for (int i = 0; i <= 11; i++)
		{
			WriteByte(std_dc_luminance_values[i]);
		}
		WriteByte(16);
		for (int i = 0; i < 16; i++)
		{
			WriteByte(std_ac_luminance_nrcodes[i + 1]);
		}
		for (int i = 0; i <= 161; i++)
		{
			WriteByte(std_ac_luminance_values[i]);
		}
		WriteByte(1);
		for (int i = 0; i < 16; i++)
		{
			WriteByte(std_dc_chrominance_nrcodes[i + 1]);
		}
		for (int i = 0; i <= 11; i++)
		{
			WriteByte(std_dc_chrominance_values[i]);
		}
		WriteByte(17);
		for (int i = 0; i < 16; i++)
		{
			WriteByte(std_ac_chrominance_nrcodes[i + 1]);
		}
		for (int i = 0; i <= 161; i++)
		{
			WriteByte(std_ac_chrominance_values[i]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeSOS()
	{
		WriteWord(65498);
		WriteWord(12);
		WriteByte(3);
		WriteByte(1);
		WriteByte(0);
		WriteByte(2);
		WriteByte(17);
		WriteByte(3);
		WriteByte(17);
		WriteByte(0);
		WriteByte(63);
		WriteByte(0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float ProcessDU(float[] CDU, float[] fdtbl, float DC, BitString[] HTDC, BitString[] HTAC)
	{
		BitString bs = HTAC[0];
		BitString bs2 = HTAC[240];
		float[] array = FDCTQuant(CDU, fdtbl);
		for (int i = 0; i < 64; i++)
		{
			DU[ZigZag[i]] = (int)array[i];
		}
		int num = (int)((float)DU[0] - DC);
		DC = DU[0];
		if (num == 0)
		{
			WriteBits(HTDC[0]);
		}
		else
		{
			WriteBits(HTDC[category[32767 + num]]);
			WriteBits(bitcode[32767 + num]);
		}
		int num2 = 63;
		while (num2 > 0 && DU[num2] == 0)
		{
			num2--;
		}
		if (num2 == 0)
		{
			WriteBits(bs);
			return DC;
		}
		for (int i = 1; i <= num2; i++)
		{
			int num3 = i;
			for (; DU[i] == 0 && i <= num2; i++)
			{
			}
			int num4 = i - num3;
			if (num4 >= 16)
			{
				for (int j = 1; j <= num4 / 16; j++)
				{
					WriteBits(bs2);
				}
				num4 &= 0xF;
			}
			WriteBits(HTAC[num4 * 16 + category[32767 + DU[i]]]);
			WriteBits(bitcode[32767 + DU[i]]);
		}
		if (num2 != 63)
		{
			WriteBits(bs);
		}
		return DC;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RGB2YUV(BitmapData image, int xpos, int ypos)
	{
		int num = 0;
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				Color32 pixelColor = image.GetPixelColor(xpos + j, image.height - (ypos + i));
				YDU[num] = 0.299f * (float)(int)pixelColor.r + 0.587f * (float)(int)pixelColor.g + 0.114f * (float)(int)pixelColor.b - 128f;
				UDU[num] = -0.16874f * (float)(int)pixelColor.r + -0.33126f * (float)(int)pixelColor.g + 0.5f * (float)(int)pixelColor.b;
				VDU[num] = 0.5f * (float)(int)pixelColor.r + -0.41869f * (float)(int)pixelColor.g + -0.08131f * (float)(int)pixelColor.b;
				num++;
			}
		}
	}

	public JPGEncoder(Texture2D texture, float quality)
		: this(texture, quality, "", blocking: false)
	{
	}

	public JPGEncoder(Texture2D texture, float quality, bool blocking)
		: this(texture, quality, "", blocking)
	{
	}

	public JPGEncoder(Texture2D texture, float quality, string path)
		: this(texture, quality, path, blocking: false)
	{
	}

	public JPGEncoder(Texture2D texture, float quality, string path, bool blocking)
	{
		this.path = path;
		image = new BitmapData(texture);
		quality = Mathf.Clamp(quality, 1f, 100f);
		sf = ((quality < 50f) ? ((int)(5000f / quality)) : ((int)(200f - quality * 2f)));
		cores = SystemInfo.processorCount;
		Thread thread = new Thread(DoEncoding);
		thread.Name = "JPGEncoder";
		thread.Start();
		if (blocking)
		{
			thread.Join();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DoEncoding()
	{
		isDone = false;
		InitHuffmanTbl();
		InitCategoryfloat();
		InitQuantTables(sf);
		Encode();
		if (!string.IsNullOrEmpty(path))
		{
			SdFile.WriteAllBytes(path, GetBytes());
		}
		isDone = true;
		Profiler.EndThreadProfiling();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Encode()
	{
		byteout = new ByteArray();
		bytenew = 0u;
		bytepos = 7;
		WriteWord(65496);
		WriteAPP0();
		WriteDQT();
		WriteSOF0(image.width, image.height);
		WriteDHT();
		writeSOS();
		float dC = 0f;
		float dC2 = 0f;
		float dC3 = 0f;
		bytenew = 0u;
		bytepos = 7;
		for (int i = 0; i < image.height; i += 8)
		{
			for (int j = 0; j < image.width; j += 8)
			{
				RGB2YUV(image, j, i);
				dC = ProcessDU(YDU, fdtbl_Y, dC, YDC_HT, YAC_HT);
				dC2 = ProcessDU(UDU, fdtbl_UV, dC2, UVDC_HT, UVAC_HT);
				dC3 = ProcessDU(VDU, fdtbl_UV, dC3, UVDC_HT, UVAC_HT);
				if (cores == 1)
				{
					Thread.Sleep(0);
				}
			}
		}
		if (bytepos >= 0)
		{
			WriteBits(new BitString
			{
				length = bytepos + 1,
				value = (1 << bytepos + 1) - 1
			});
		}
		WriteWord(65497);
		isDone = true;
	}
}
