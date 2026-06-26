using System;
using System.Collections.Generic;
using System.IO;
using SharpEXR.AttributeTypes;

namespace SharpEXR;

public class EXRFile
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public EXRVersion Version
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public List<EXRHeader> Headers
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public List<OffsetTable> OffsetTables
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public List<EXRPart> Parts
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public void Read(IEXRReader reader)
	{
		if (reader.ReadInt32() != 20000630)
		{
			throw new EXRFormatException("Invalid or corrupt EXR layout: First four bytes were not 20000630.");
		}
		int value = reader.ReadInt32();
		Version = new EXRVersion(value);
		Headers = new List<EXRHeader>();
		if (Version.IsMultiPart)
		{
			while (true)
			{
				EXRHeader eXRHeader = new EXRHeader();
				eXRHeader.Read(this, reader);
				if (eXRHeader.IsEmpty)
				{
					break;
				}
				Headers.Add(eXRHeader);
			}
			throw new NotImplementedException("Multi part EXR files are not currently supported");
		}
		if (Version.IsSinglePartTiled)
		{
			throw new NotImplementedException("Tiled EXR files are not currently supported");
		}
		EXRHeader eXRHeader2 = new EXRHeader();
		eXRHeader2.Read(this, reader);
		Headers.Add(eXRHeader2);
		OffsetTables = new List<OffsetTable>();
		foreach (EXRHeader header in Headers)
		{
			int num;
			if (Version.IsMultiPart)
			{
				num = header.ChunkCount;
			}
			else if (Version.IsSinglePartTiled)
			{
				num = 0;
			}
			else
			{
				EXRCompression compression = header.Compression;
				Box2I dataWindow = header.DataWindow;
				int scanLinesPerBlock = GetScanLinesPerBlock(compression);
				num = (int)Math.Ceiling((double)dataWindow.Height / (double)scanLinesPerBlock);
			}
			OffsetTable offsetTable = new OffsetTable(num);
			offsetTable.Read(reader, num);
			OffsetTables.Add(offsetTable);
		}
	}

	public static int GetScanLinesPerBlock(EXRCompression compression)
	{
		switch (compression)
		{
		default:
			return 1;
		case EXRCompression.ZIP:
		case EXRCompression.PXR24:
			return 16;
		case EXRCompression.PIZ:
		case EXRCompression.B44:
		case EXRCompression.B44A:
			return 32;
		}
	}

	public static int GetBytesPerPixel(ImageDestFormat format)
	{
		switch (format)
		{
		case ImageDestFormat.RGB16:
		case ImageDestFormat.BGR16:
			return 6;
		case ImageDestFormat.RGB32:
		case ImageDestFormat.BGR32:
			return 12;
		case ImageDestFormat.RGB8:
		case ImageDestFormat.BGR8:
			return 3;
		case ImageDestFormat.RGBA16:
		case ImageDestFormat.PremultipliedRGBA16:
		case ImageDestFormat.BGRA16:
		case ImageDestFormat.PremultipliedBGRA16:
			return 8;
		case ImageDestFormat.RGBA32:
		case ImageDestFormat.PremultipliedRGBA32:
		case ImageDestFormat.BGRA32:
		case ImageDestFormat.PremultipliedBGRA32:
			return 16;
		case ImageDestFormat.RGBA8:
		case ImageDestFormat.PremultipliedRGBA8:
		case ImageDestFormat.BGRA8:
		case ImageDestFormat.PremultipliedBGRA8:
			return 4;
		default:
			throw new ArgumentException("Unrecognized destination format", "format");
		}
	}

	public static int GetBitsPerPixel(ImageDestFormat format)
	{
		switch (format)
		{
		case ImageDestFormat.RGB32:
		case ImageDestFormat.RGBA32:
		case ImageDestFormat.PremultipliedRGBA32:
		case ImageDestFormat.BGR32:
		case ImageDestFormat.BGRA32:
		case ImageDestFormat.PremultipliedBGRA32:
			return 32;
		case ImageDestFormat.RGB8:
		case ImageDestFormat.RGBA8:
		case ImageDestFormat.PremultipliedRGBA8:
		case ImageDestFormat.BGR8:
		case ImageDestFormat.BGRA8:
		case ImageDestFormat.PremultipliedBGRA8:
			return 8;
		case ImageDestFormat.RGB16:
		case ImageDestFormat.RGBA16:
		case ImageDestFormat.PremultipliedRGBA16:
		case ImageDestFormat.BGR16:
		case ImageDestFormat.BGRA16:
		case ImageDestFormat.PremultipliedBGRA16:
			return 16;
		default:
			throw new ArgumentException("Unrecognized destination format", "format");
		}
	}

	public static EXRFile FromFile(string file)
	{
		EXRReader eXRReader = new EXRReader(new FileStream(file, FileMode.Open, FileAccess.Read));
		EXRFile result = FromReader(eXRReader);
		eXRReader.Dispose();
		return result;
	}

	public static EXRFile FromStream(Stream stream)
	{
		EXRReader eXRReader = new EXRReader(new BinaryReader(stream));
		EXRFile result = FromReader(eXRReader);
		eXRReader.Dispose();
		return result;
	}

	public static EXRFile FromReader(IEXRReader reader)
	{
		EXRFile eXRFile = new EXRFile();
		eXRFile.Read(reader);
		eXRFile.Parts = new List<EXRPart>();
		for (int i = 0; i < eXRFile.Headers.Count; i++)
		{
			EXRPart item = new EXRPart(eXRFile.Version, eXRFile.Headers[i], eXRFile.OffsetTables[i]);
			eXRFile.Parts.Add(item);
		}
		return eXRFile;
	}
}
