using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpEXR.AttributeTypes;
using SharpEXR.ColorSpace;

namespace SharpEXR;

public class EXRPart
{
	public readonly EXRVersion Version;

	public readonly EXRHeader Header;

	public readonly OffsetTable Offsets;

	public readonly PartType Type;

	public readonly Box2I DataWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasData;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, float[]> floatChannels;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, Half[]> halfChannels;

	public Dictionary<string, float[]> FloatChannels
	{
		get
		{
			return floatChannels;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			floatChannels = value;
		}
	}

	public Dictionary<string, Half[]> HalfChannels
	{
		get
		{
			return halfChannels;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			halfChannels = value;
		}
	}

	public bool IsRGB
	{
		get
		{
			if ((HalfChannels.ContainsKey("R") || FloatChannels.ContainsKey("R")) && (HalfChannels.ContainsKey("G") || FloatChannels.ContainsKey("G")))
			{
				if (!HalfChannels.ContainsKey("B"))
				{
					return FloatChannels.ContainsKey("B");
				}
				return true;
			}
			return false;
		}
	}

	public bool HasAlpha
	{
		get
		{
			if (!HalfChannels.ContainsKey("A"))
			{
				return FloatChannels.ContainsKey("A");
			}
			return true;
		}
	}

	public EXRPart(EXRVersion version, EXRHeader header, OffsetTable offsets)
	{
		Version = version;
		Header = header;
		Offsets = offsets;
		if (Version.IsMultiPart)
		{
			Type = header.Type;
		}
		else
		{
			Type = (version.IsSinglePartTiled ? PartType.Tiled : PartType.ScanLine);
		}
		DataWindow = Header.DataWindow;
		FloatChannels = new Dictionary<string, float[]>();
		HalfChannels = new Dictionary<string, Half[]>();
		foreach (Channel channel in header.Channels)
		{
			if (channel.Type == PixelType.Float)
			{
				FloatChannels[channel.Name] = new float[DataWindow.Width * DataWindow.Height];
				continue;
			}
			if (channel.Type == PixelType.Half)
			{
				HalfChannels[channel.Name] = new Half[DataWindow.Width * DataWindow.Height];
				continue;
			}
			throw new NotImplementedException("Only 16 and 32 bit floating point EXR images are supported.");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void CheckHasData()
	{
		if (!hasData)
		{
			throw new InvalidOperationException("Call EXRPart.Open before performing image operations.");
		}
	}

	public Half[] GetHalfs(ChannelConfiguration channels, bool premultiplied, GammaEncoding gamma)
	{
		return GetHalfs(channels, premultiplied, gamma, HasAlpha);
	}

	public Half[] GetHalfs(ChannelConfiguration channels, bool premultiplied, GammaEncoding gamma, bool includeAlpha)
	{
		ImageSourceFormat srcFormat;
		if (HalfChannels.ContainsKey("R") && HalfChannels.ContainsKey("G") && HalfChannels.ContainsKey("B"))
		{
			srcFormat = (includeAlpha ? ImageSourceFormat.HalfRGBA : ImageSourceFormat.HalfRGB);
		}
		else
		{
			if (!FloatChannels.ContainsKey("R") || !FloatChannels.ContainsKey("G") || !FloatChannels.ContainsKey("B"))
			{
				throw new EXRFormatException("Unrecognized EXR image format, did not contain half/single RGB color channels");
			}
			srcFormat = ((!includeAlpha) ? ImageSourceFormat.SingleRGB : ImageSourceFormat.SingleRGBA);
		}
		return GetHalfs(srcFormat, channels, premultiplied, gamma);
	}

	public Half[] GetHalfs(ImageSourceFormat srcFormat, ChannelConfiguration channels, bool premultiplied, GammaEncoding gamma)
	{
		ImageDestFormat imageDestFormat = ((srcFormat != ImageSourceFormat.HalfRGBA && srcFormat != ImageSourceFormat.SingleRGBA) ? ((channels == ChannelConfiguration.BGR) ? ImageDestFormat.BGR16 : ImageDestFormat.RGB16) : ((!premultiplied) ? ((channels == ChannelConfiguration.BGR) ? ImageDestFormat.BGRA16 : ImageDestFormat.RGBA16) : ((channels == ChannelConfiguration.BGR) ? ImageDestFormat.PremultipliedBGRA16 : ImageDestFormat.PremultipliedRGBA16)));
		int bytesPerPixel = EXRFile.GetBytesPerPixel(imageDestFormat);
		if (srcFormat != ImageSourceFormat.SingleRGB)
		{
			_ = 3;
		}
		byte[] bytes = GetBytes(srcFormat, imageDestFormat, gamma, DataWindow.Width * bytesPerPixel);
		Half[] array = new Half[bytes.Length / 2];
		Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
		return array;
	}

	public float[] GetFloats(ChannelConfiguration channels, bool premultiplied, GammaEncoding gamma)
	{
		return GetFloats(channels, premultiplied, gamma, HasAlpha);
	}

	public float[] GetFloats(ChannelConfiguration channels, bool premultiplied, GammaEncoding gamma, bool includeAlpha)
	{
		ImageSourceFormat srcFormat;
		if (HalfChannels.ContainsKey("R") && HalfChannels.ContainsKey("G") && HalfChannels.ContainsKey("B"))
		{
			srcFormat = (includeAlpha ? ImageSourceFormat.HalfRGBA : ImageSourceFormat.HalfRGB);
		}
		else
		{
			if (!FloatChannels.ContainsKey("R") || !FloatChannels.ContainsKey("G") || !FloatChannels.ContainsKey("B"))
			{
				throw new EXRFormatException("Unrecognized EXR image format, did not contain half/single RGB color channels");
			}
			srcFormat = ((!includeAlpha) ? ImageSourceFormat.SingleRGB : ImageSourceFormat.SingleRGBA);
		}
		return GetFloats(srcFormat, channels, premultiplied, gamma);
	}

	public float[] GetFloats(ImageSourceFormat srcFormat, ChannelConfiguration channels, bool premultiplied, GammaEncoding gamma)
	{
		ImageDestFormat imageDestFormat = ((srcFormat != ImageSourceFormat.HalfRGBA && srcFormat != ImageSourceFormat.SingleRGBA) ? ((channels == ChannelConfiguration.BGR) ? ImageDestFormat.BGR32 : ImageDestFormat.RGB32) : ((!premultiplied) ? ((channels == ChannelConfiguration.BGR) ? ImageDestFormat.BGRA32 : ImageDestFormat.RGBA32) : ((channels == ChannelConfiguration.BGR) ? ImageDestFormat.PremultipliedBGRA32 : ImageDestFormat.PremultipliedRGBA32)));
		int bytesPerPixel = EXRFile.GetBytesPerPixel(imageDestFormat);
		if (srcFormat != ImageSourceFormat.SingleRGB)
		{
			_ = 3;
		}
		byte[] bytes = GetBytes(srcFormat, imageDestFormat, gamma, DataWindow.Width * bytesPerPixel);
		float[] array = new float[bytes.Length / 4];
		Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
		return array;
	}

	public byte[] GetBytes(ImageDestFormat destFormat, GammaEncoding gamma)
	{
		return GetBytes(destFormat, gamma, DataWindow.Width * EXRFile.GetBytesPerPixel(destFormat));
	}

	public byte[] GetBytes(ImageDestFormat destFormat, GammaEncoding gamma, int stride)
	{
		ImageSourceFormat srcFormat;
		if (HalfChannels.ContainsKey("R") && HalfChannels.ContainsKey("G") && HalfChannels.ContainsKey("B"))
		{
			srcFormat = (HalfChannels.ContainsKey("A") ? ImageSourceFormat.HalfRGBA : ImageSourceFormat.HalfRGB);
		}
		else
		{
			if (!FloatChannels.ContainsKey("R") || !FloatChannels.ContainsKey("G") || !FloatChannels.ContainsKey("B"))
			{
				throw new EXRFormatException("Unrecognized EXR image format, did not contain half/single RGB color channels");
			}
			srcFormat = ((!FloatChannels.ContainsKey("A")) ? ImageSourceFormat.SingleRGB : ImageSourceFormat.SingleRGBA);
		}
		return GetBytes(srcFormat, destFormat, gamma, stride);
	}

	public byte[] GetBytes(ImageSourceFormat srcFormat, ImageDestFormat destFormat, GammaEncoding gamma)
	{
		return GetBytes(srcFormat, destFormat, gamma, DataWindow.Width * EXRFile.GetBytesPerPixel(destFormat));
	}

	public byte[] GetBytes(ImageSourceFormat srcFormat, ImageDestFormat destFormat, GammaEncoding gamma, int stride)
	{
		CheckHasData();
		int bytesPerPixel = EXRFile.GetBytesPerPixel(destFormat);
		int bitsPerPixel = EXRFile.GetBitsPerPixel(destFormat);
		if (stride < bytesPerPixel * DataWindow.Width)
		{
			throw new ArgumentException("Stride was lower than minimum", "stride");
		}
		byte[] array = new byte[stride * DataWindow.Height];
		int num = stride - bytesPerPixel * DataWindow.Width;
		bool flag = srcFormat == ImageSourceFormat.HalfRGB || srcFormat == ImageSourceFormat.HalfRGBA;
		bool sourceAlpha = false;
		bool destinationAlpha = destFormat == ImageDestFormat.BGRA16 || destFormat == ImageDestFormat.BGRA32 || destFormat == ImageDestFormat.BGRA8 || destFormat == ImageDestFormat.PremultipliedBGRA16 || destFormat == ImageDestFormat.PremultipliedBGRA32 || destFormat == ImageDestFormat.PremultipliedBGRA8 || destFormat == ImageDestFormat.PremultipliedRGBA16 || destFormat == ImageDestFormat.PremultipliedRGBA32 || destFormat == ImageDestFormat.PremultipliedRGBA8 || destFormat == ImageDestFormat.RGBA16 || destFormat == ImageDestFormat.RGBA32 || destFormat == ImageDestFormat.RGBA8;
		bool premultiplied = destFormat == ImageDestFormat.PremultipliedBGRA16 || destFormat == ImageDestFormat.PremultipliedBGRA32 || destFormat == ImageDestFormat.PremultipliedBGRA8 || destFormat == ImageDestFormat.PremultipliedRGBA16 || destFormat == ImageDestFormat.PremultipliedRGBA32 || destFormat == ImageDestFormat.PremultipliedRGBA8;
		bool bgra = destFormat == ImageDestFormat.BGR16 || destFormat == ImageDestFormat.BGR32 || destFormat == ImageDestFormat.BGR8 || destFormat == ImageDestFormat.BGRA16 || destFormat == ImageDestFormat.BGRA32 || destFormat == ImageDestFormat.BGRA8 || destFormat == ImageDestFormat.PremultipliedBGRA16 || destFormat == ImageDestFormat.PremultipliedBGRA32 || destFormat == ImageDestFormat.PremultipliedBGRA8;
		Half[] hg;
		Half[] hb;
		Half[] ha;
		Half[] hr = (hg = (hb = (ha = null)));
		float[] fg;
		float[] fb;
		float[] fa;
		float[] fr = (fg = (fb = (fa = null)));
		if (flag)
		{
			if (!HalfChannels.ContainsKey("R"))
			{
				throw new ArgumentException("Half type channel R not found", "srcFormat");
			}
			if (!HalfChannels.ContainsKey("G"))
			{
				throw new ArgumentException("Half type channel G not found", "srcFormat");
			}
			if (!HalfChannels.ContainsKey("B"))
			{
				throw new ArgumentException("Half type channel B not found", "srcFormat");
			}
			hr = HalfChannels["R"];
			hg = HalfChannels["G"];
			hb = HalfChannels["B"];
			if (srcFormat == ImageSourceFormat.HalfRGBA)
			{
				if (!HalfChannels.ContainsKey("A"))
				{
					throw new ArgumentException("Half type channel A not found", "srcFormat");
				}
				ha = HalfChannels["A"];
				sourceAlpha = true;
			}
		}
		else
		{
			if (!FloatChannels.ContainsKey("R"))
			{
				throw new ArgumentException("Single type channel R not found", "srcFormat");
			}
			if (!FloatChannels.ContainsKey("G"))
			{
				throw new ArgumentException("Single type channel G not found", "srcFormat");
			}
			if (!FloatChannels.ContainsKey("B"))
			{
				throw new ArgumentException("Single type channel B not found", "srcFormat");
			}
			fr = FloatChannels["R"];
			fg = FloatChannels["G"];
			fb = FloatChannels["B"];
			if (srcFormat == ImageSourceFormat.HalfRGBA)
			{
				if (!FloatChannels.ContainsKey("A"))
				{
					throw new ArgumentException("Single type channel A not found", "srcFormat");
				}
				fa = FloatChannels["A"];
				sourceAlpha = true;
			}
		}
		int num2 = 0;
		int num3 = 0;
		BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(array));
		int num4 = 0;
		while (num4 < DataWindow.Height)
		{
			GetScanlineBytes(bytesPerPixel, num3, num2, flag, destinationAlpha, sourceAlpha, hr, hg, hb, ha, fr, fg, fb, fa, bitsPerPixel, gamma, premultiplied, bgra, array, binaryWriter);
			num3 += DataWindow.Width * bytesPerPixel;
			num2 += DataWindow.Width;
			num4++;
			num3 += num;
		}
		binaryWriter.Dispose();
		binaryWriter.BaseStream.Dispose();
		return array;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetScanlineBytes(int bytesPerPixel, int destIndex, int srcIndex, bool isHalf, bool destinationAlpha, bool sourceAlpha, Half[] hr, Half[] hg, Half[] hb, Half[] ha, float[] fr, float[] fg, float[] fb, float[] fa, int bitsPerPixel, GammaEncoding gamma, bool premultiplied, bool bgra, byte[] buffer, BinaryWriter writer)
	{
		writer.Seek(destIndex, SeekOrigin.Begin);
		int num = 0;
		while (num < DataWindow.Width)
		{
			float num2;
			float num3;
			float num4;
			float num5;
			if (isHalf)
			{
				num2 = hr[srcIndex];
				num3 = hg[srcIndex];
				num4 = hb[srcIndex];
				num5 = ((!destinationAlpha) ? 1f : (sourceAlpha ? ((float)ha[srcIndex]) : 1f));
			}
			else
			{
				num2 = fr[srcIndex];
				num3 = fg[srcIndex];
				num4 = fb[srcIndex];
				num5 = ((!destinationAlpha) ? 1f : (sourceAlpha ? fa[srcIndex] : 1f));
			}
			switch (bitsPerPixel)
			{
			case 8:
			{
				byte b = byte.MaxValue;
				byte b2;
				byte b3;
				byte b4;
				switch (gamma)
				{
				case GammaEncoding.Linear:
					if (premultiplied)
					{
						b2 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num2 * num5 * 255f) + 0.5)));
						b3 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num3 * num5 * 255f) + 0.5)));
						b4 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num4 * num5 * 255f) + 0.5)));
						b = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num5 * 255f) + 0.5)));
						break;
					}
					b2 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num2 * 255f) + 0.5)));
					b3 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num3 * 255f) + 0.5)));
					b4 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num4 * 255f) + 0.5)));
					if (destinationAlpha)
					{
						b = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num5 * 255f) + 0.5)));
					}
					break;
				case GammaEncoding.Gamma:
					if (premultiplied)
					{
						b2 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress(num2) * num5 * 255f) + 0.5)));
						b3 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress(num3) * num5 * 255f) + 0.5)));
						b4 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress(num4) * num5 * 255f) + 0.5)));
						b = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num5 * 255f) + 0.5)));
						break;
					}
					b2 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress(num2) * 255f) + 0.5)));
					b3 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress(num3) * 255f) + 0.5)));
					b4 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress(num4) * 255f) + 0.5)));
					if (destinationAlpha)
					{
						b = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num5 * 255f) + 0.5)));
					}
					break;
				default:
					if (premultiplied)
					{
						b2 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress_sRGB(num2) * num5 * 255f) + 0.5)));
						b3 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress_sRGB(num3) * num5 * 255f) + 0.5)));
						b4 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress_sRGB(num4) * num5 * 255f) + 0.5)));
						b = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num5 * 255f) + 0.5)));
						break;
					}
					b2 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress_sRGB(num2) * 255f) + 0.5)));
					b3 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress_sRGB(num3) * 255f) + 0.5)));
					b4 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress_sRGB(num4) * 255f) + 0.5)));
					if (destinationAlpha)
					{
						b = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num5 * 255f) + 0.5)));
					}
					break;
				}
				if (bgra)
				{
					buffer[destIndex] = b4;
					buffer[destIndex + 1] = b3;
					buffer[destIndex + 2] = b2;
				}
				else
				{
					buffer[destIndex] = b2;
					buffer[destIndex + 1] = b3;
					buffer[destIndex + 2] = b4;
				}
				if (destinationAlpha)
				{
					buffer[destIndex + 3] = b;
				}
				break;
			}
			case 32:
			{
				float value = 1f;
				float value2;
				float value3;
				float value4;
				switch (gamma)
				{
				case GammaEncoding.Linear:
					if (premultiplied)
					{
						value2 = num2 * num5;
						value3 = num3 * num5;
						value4 = num4 * num5;
						value = num5;
						break;
					}
					value2 = num2;
					value3 = num3;
					value4 = num4;
					if (destinationAlpha)
					{
						value = num5;
					}
					break;
				case GammaEncoding.Gamma:
					if (premultiplied)
					{
						value2 = Gamma.Compress(num2) * num5;
						value3 = Gamma.Compress(num3) * num5;
						value4 = Gamma.Compress(num4) * num5;
						value = num5;
						break;
					}
					value2 = Gamma.Compress(num2);
					value3 = Gamma.Compress(num3);
					value4 = Gamma.Compress(num4);
					if (destinationAlpha)
					{
						value = num5;
					}
					break;
				default:
					if (premultiplied)
					{
						value2 = Gamma.Compress_sRGB(num2) * num5;
						value3 = Gamma.Compress_sRGB(num3) * num5;
						value4 = Gamma.Compress_sRGB(num4) * num5;
						value = num5;
						break;
					}
					value2 = Gamma.Compress_sRGB(num2);
					value3 = Gamma.Compress_sRGB(num3);
					value4 = Gamma.Compress_sRGB(num4);
					if (destinationAlpha)
					{
						value = num5;
					}
					break;
				}
				if (bgra)
				{
					writer.Write(value4);
					writer.Write(value3);
					writer.Write(value2);
				}
				else
				{
					writer.Write(value2);
					writer.Write(value3);
					writer.Write(value4);
				}
				if (destinationAlpha)
				{
					writer.Write(value);
				}
				break;
			}
			default:
			{
				Half half = new Half(1f);
				Half half2;
				Half half3;
				Half half4;
				switch (gamma)
				{
				case GammaEncoding.Linear:
					if (premultiplied)
					{
						half2 = (Half)(num2 * num5);
						half3 = (Half)(num3 * num5);
						half4 = (Half)(num4 * num5);
						half = (Half)num5;
						break;
					}
					half2 = (Half)num2;
					half3 = (Half)num3;
					half4 = (Half)num4;
					if (destinationAlpha)
					{
						half = (Half)num5;
					}
					break;
				case GammaEncoding.Gamma:
					if (premultiplied)
					{
						half2 = (Half)(Gamma.Compress(num2) * num5);
						half3 = (Half)(Gamma.Compress(num3) * num5);
						half4 = (Half)(Gamma.Compress(num4) * num5);
						half = (Half)num5;
						break;
					}
					half2 = (Half)Gamma.Compress(num2);
					half3 = (Half)Gamma.Compress(num3);
					half4 = (Half)Gamma.Compress(num4);
					if (destinationAlpha)
					{
						half = (Half)num5;
					}
					break;
				default:
					if (premultiplied)
					{
						half2 = (Half)(Gamma.Compress_sRGB(num2) * num5);
						half3 = (Half)(Gamma.Compress_sRGB(num3) * num5);
						half4 = (Half)(Gamma.Compress_sRGB(num4) * num5);
						half = (Half)num5;
						break;
					}
					half2 = (Half)Gamma.Compress_sRGB(num2);
					half3 = (Half)Gamma.Compress_sRGB(num3);
					half4 = (Half)Gamma.Compress_sRGB(num4);
					if (destinationAlpha)
					{
						half = (Half)num5;
					}
					break;
				}
				if (bgra)
				{
					writer.Write(half4.value);
					writer.Write(half3.value);
					writer.Write(half2.value);
				}
				else
				{
					writer.Write(half2.value);
					writer.Write(half3.value);
					writer.Write(half4.value);
				}
				if (destinationAlpha)
				{
					writer.Write(half.value);
				}
				break;
			}
			}
			num++;
			destIndex += bytesPerPixel;
			srcIndex++;
		}
	}

	public void Open(string file)
	{
		EXRReader eXRReader = new EXRReader(new FileStream(file, FileMode.Open, FileAccess.Read));
		Open(eXRReader);
		eXRReader.Dispose();
	}

	public void Open(Stream stream)
	{
		EXRReader eXRReader = new EXRReader(new BinaryReader(stream));
		Open(eXRReader);
		eXRReader.Dispose();
	}

	public void Close()
	{
		hasData = false;
		HalfChannels.Clear();
		FloatChannels.Clear();
	}

	public void Open(IEXRReader reader)
	{
		hasData = true;
		ReadPixelData(reader);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReadPixelBlock(IEXRReader reader, uint offset, int linesPerBlock, List<Channel> sortedChannels)
	{
		reader.Position = (int)offset;
		if (Version.IsMultiPart)
		{
			reader.ReadUInt32();
			reader.ReadUInt32();
		}
		int num = reader.ReadInt32();
		int num2 = Math.Min(DataWindow.Height, num + linesPerBlock);
		int num3 = num * DataWindow.Width;
		reader.ReadInt32();
		if (Header.Compression != EXRCompression.None)
		{
			throw new NotImplementedException("Compressed images are currently not supported");
		}
		foreach (Channel sortedChannel in sortedChannels)
		{
			float[] array = null;
			Half[] array2 = null;
			if (sortedChannel.Type == PixelType.Float)
			{
				array = FloatChannels[sortedChannel.Name];
			}
			else
			{
				if (sortedChannel.Type != PixelType.Half)
				{
					throw new NotImplementedException();
				}
				array2 = HalfChannels[sortedChannel.Name];
			}
			int num4 = num3;
			for (int i = num; i < num2; i++)
			{
				int num5 = 0;
				while (num5 < DataWindow.Width)
				{
					if (sortedChannel.Type == PixelType.Float)
					{
						array[num4] = reader.ReadSingle();
					}
					else
					{
						if (sortedChannel.Type != PixelType.Half)
						{
							throw new NotImplementedException();
						}
						array2[num4] = reader.ReadHalf();
					}
					num5++;
					num4++;
				}
			}
		}
	}

	public void OpenParallel(string file)
	{
		Open(file);
	}

	public void OpenParallel(ParallelReaderCreationDelegate createReader)
	{
		IEXRReader iEXRReader = createReader();
		Open(iEXRReader);
		iEXRReader.Dispose();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ReadPixelData(IEXRReader reader)
	{
		int scanLinesPerBlock = EXRFile.GetScanLinesPerBlock(Header.Compression);
		List<Channel> sortedChannels = Header.Channels.OrderBy([PublicizedFrom(EAccessModifier.Internal)] (Channel c) => c.Name).ToList();
		foreach (uint offset in Offsets)
		{
			ReadPixelBlock(reader, offset, scanLinesPerBlock, sortedChannels);
		}
	}
}
