using System;
using System.IO;
using UnityEngine;

public static class TGALoader
{
	public static Texture2D LoadTGA(string fileName, bool mipMaps = true)
	{
		using Stream tGAStream = SdFile.OpenRead(fileName);
		return LoadTGA(tGAStream, mipMaps);
	}

	public static Texture2D LoadTGA(Stream TGAStream, bool mipMaps = true)
	{
		using BinaryReader binaryReader = new BinaryReader(TGAStream);
		binaryReader.BaseStream.Seek(0L, SeekOrigin.Begin);
		byte[] array = binaryReader.ReadBytes(12);
		if (array[0] != 0)
		{
			throw new Exception("TGA ID length !0");
		}
		if (array[1] != 0)
		{
			throw new Exception("TGA has color map");
		}
		byte b = array[2];
		if (b != 2 && b != 3)
		{
			throw new Exception("TGA unsupported image type " + b);
		}
		short num = binaryReader.ReadInt16();
		short num2 = binaryReader.ReadInt16();
		int num3 = binaryReader.ReadByte();
		binaryReader.BaseStream.Seek(1L, SeekOrigin.Current);
		Texture2D texture2D = new Texture2D(num, num2, TextureFormat.RGBA32, mipMaps);
		Color32[] array2 = new Color32[num * num2];
		switch (num3)
		{
		case 8:
		{
			Color32 color = new Color32(0, 0, 0, byte.MaxValue);
			for (int j = 0; j < num * num2; j++)
			{
				color.b = (color.g = (color.r = binaryReader.ReadByte()));
				array2[j] = color;
			}
			break;
		}
		case 32:
		{
			for (int k = 0; k < num * num2; k++)
			{
				byte b3 = binaryReader.ReadByte();
				byte g2 = binaryReader.ReadByte();
				byte r2 = binaryReader.ReadByte();
				byte a = binaryReader.ReadByte();
				array2[k] = new Color32(r2, g2, b3, a);
			}
			break;
		}
		case 24:
		{
			for (int i = 0; i < num * num2; i++)
			{
				byte b2 = binaryReader.ReadByte();
				byte g = binaryReader.ReadByte();
				byte r = binaryReader.ReadByte();
				array2[i] = new Color32(r, g, b2, byte.MaxValue);
			}
			break;
		}
		default:
			throw new Exception("TGA texture had non 8/32/24 bit depth.");
		}
		texture2D.SetPixels32(array2);
		texture2D.Apply();
		return texture2D;
	}

	public static Color32[] LoadTGAAsArray(string fileName, out int width, out int height, byte[] tempBuf = null)
	{
		using Stream input = SdFile.OpenRead(fileName);
		using BinaryReader binaryReader = new BinaryReader(input);
		binaryReader.BaseStream.Seek(12L, SeekOrigin.Begin);
		width = binaryReader.ReadInt16();
		height = binaryReader.ReadInt16();
		int num = binaryReader.ReadByte();
		binaryReader.BaseStream.Seek(1L, SeekOrigin.Current);
		Color32[] array = new Color32[width * height];
		if (tempBuf == null)
		{
			tempBuf = new byte[1024];
		}
		int num2 = 0;
		if (num == 32)
		{
			while (true)
			{
				int num3 = binaryReader.Read(tempBuf, 0, tempBuf.Length);
				if (num3 == 0)
				{
					break;
				}
				int num4 = 0;
				while (num4 < num3 && num2 < array.Length)
				{
					byte b = tempBuf[num4++];
					byte g = tempBuf[num4++];
					byte r = tempBuf[num4++];
					byte a = tempBuf[num4++];
					array[num2++] = new Color32(r, g, b, a);
				}
			}
		}
		else
		{
			if (num != 24)
			{
				throw new Exception("TGA texture had non 32/24 bit depth.");
			}
			while (true)
			{
				int num5 = binaryReader.Read(tempBuf, 0, tempBuf.Length);
				if (num5 == 0)
				{
					break;
				}
				int num6 = 0;
				while (num6 < num5 && num2 < array.Length)
				{
					byte b2 = tempBuf[num6++];
					byte g2 = tempBuf[num6++];
					byte r2 = tempBuf[num6++];
					array[num2++] = new Color32(r2, g2, b2, byte.MaxValue);
				}
			}
		}
		return array;
	}

	public static Color32[] LoadTGAAsArrayThreaded(string fileName, out int w, out int h)
	{
		using Stream input = SdFile.OpenRead(fileName);
		using BinaryReader binaryReader = new BinaryReader(input);
		binaryReader.BaseStream.Seek(12L, SeekOrigin.Begin);
		int width = binaryReader.ReadInt16();
		int height = binaryReader.ReadInt16();
		int bitDepth = binaryReader.ReadByte();
		binaryReader.BaseStream.Seek(1L, SeekOrigin.Current);
		Color32[] pulledColors = new Color32[width * height];
		ThreadManager.TaskInfo taskInfo = ThreadManager.AddSingleTask([PublicizedFrom(EAccessModifier.Internal)] (ThreadManager.TaskInfo _taskInfo) =>
		{
			using Stream input3 = SdFile.OpenRead(fileName);
			using BinaryReader binaryReader3 = new BinaryReader(input3);
			_ = width;
			_ = height;
			int offs2 = 0 / 4;
			binaryReader3.BaseStream.Seek(18L, SeekOrigin.Begin);
			loadPart(bitDepth, binaryReader3, pulledColors, offs2, width * height / 4);
		});
		ThreadManager.TaskInfo taskInfo2 = ThreadManager.AddSingleTask([PublicizedFrom(EAccessModifier.Internal)] (ThreadManager.TaskInfo _taskInfo) =>
		{
			using Stream input3 = SdFile.OpenRead(fileName);
			using BinaryReader binaryReader3 = new BinaryReader(input3);
			int offs2 = width * height / 4;
			binaryReader3.BaseStream.Seek(18L, SeekOrigin.Begin);
			loadPart(bitDepth, binaryReader3, pulledColors, offs2, width * height / 4);
		});
		ThreadManager.TaskInfo taskInfo3 = ThreadManager.AddSingleTask([PublicizedFrom(EAccessModifier.Internal)] (ThreadManager.TaskInfo _taskInfo) =>
		{
			using Stream input3 = SdFile.OpenRead(fileName);
			using BinaryReader binaryReader3 = new BinaryReader(input3);
			int offs2 = 2 * (width * height) / 4;
			binaryReader3.BaseStream.Seek(18L, SeekOrigin.Begin);
			loadPart(bitDepth, binaryReader3, pulledColors, offs2, width * height / 4);
		});
		using (Stream input2 = SdFile.OpenRead(fileName))
		{
			using BinaryReader binaryReader2 = new BinaryReader(input2);
			int offs = 3 * (width * height) / 4;
			binaryReader2.BaseStream.Seek(18L, SeekOrigin.Begin);
			loadPart(bitDepth, binaryReader2, pulledColors, offs, width * height / 4);
		}
		taskInfo.WaitForEnd();
		taskInfo2.WaitForEnd();
		taskInfo3.WaitForEnd();
		w = width;
		h = height;
		return pulledColors;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void loadPart(int bitDepth, BinaryReader r, Color32[] pulledColors, int offs, int length)
	{
		byte[] array = new byte[1024];
		int num = 0;
		switch (bitDepth)
		{
		case 32:
			r.BaseStream.Seek(offs * 4, SeekOrigin.Current);
			while (true)
			{
				int num4 = r.Read(array, 0, array.Length);
				if (num4 != 0)
				{
					int num5 = 0;
					while (num5 < num4 && num < length)
					{
						byte b2 = array[num5++];
						byte g2 = array[num5++];
						byte r3 = array[num5++];
						byte a = array[num5++];
						pulledColors[offs + num++] = new Color32(r3, g2, b2, a);
					}
					continue;
				}
				break;
			}
			break;
		case 24:
			r.BaseStream.Seek(offs * 3, SeekOrigin.Current);
			while (true)
			{
				int num2 = r.Read(array, 0, array.Length);
				if (num2 != 0)
				{
					int num3 = 0;
					while (num3 < num2 && num < length)
					{
						byte b = array[num3++];
						byte g = array[num3++];
						byte r2 = array[num3++];
						pulledColors[offs + num++] = new Color32(r2, g, b, byte.MaxValue);
					}
					continue;
				}
				break;
			}
			break;
		default:
			throw new Exception("TGA texture had non 32/24 bit depth.");
		}
	}
}
