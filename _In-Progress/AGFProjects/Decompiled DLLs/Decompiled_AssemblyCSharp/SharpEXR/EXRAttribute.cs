using System;
using System.Collections.Generic;
using SharpEXR.AttributeTypes;

namespace SharpEXR;

public class EXRAttribute
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Name
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Type
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int Size
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public object Value
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public static bool Read(EXRFile file, IEXRReader reader, out EXRAttribute attribute)
	{
		attribute = new EXRAttribute();
		return attribute.Read(file, reader);
	}

	public override string ToString()
	{
		return Value.ToString();
	}

	public bool Read(EXRFile file, IEXRReader reader)
	{
		int maxNameLength = file.Version.MaxNameLength;
		try
		{
			Name = reader.ReadNullTerminatedString(maxNameLength);
		}
		catch (Exception ex)
		{
			throw new EXRFormatException("Invalid or corrupt EXR header attribute name: " + ex.Message, ex);
		}
		if (Name == "")
		{
			return false;
		}
		try
		{
			Type = reader.ReadNullTerminatedString(maxNameLength);
		}
		catch (Exception ex2)
		{
			throw new EXRFormatException("Invalid or corrupt EXR header attribute type for '" + Name + "': " + ex2.Message, ex2);
		}
		if (Type == "")
		{
			throw new EXRFormatException("Invalid or corrupt EXR header attribute type for '" + Name + "': Cannot be an empty string.");
		}
		Size = reader.ReadInt32();
		switch (Type)
		{
		case "box2i":
			if (Size != 16)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type box2i: Size must be 16 bytes, was " + Size + ".");
			}
			Value = new Box2I(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
			break;
		case "box2f":
			if (Size != 16)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type box2f: Size must be 16 bytes, was " + Size + ".");
			}
			Value = new Box2F(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
			break;
		case "chromaticities":
			if (Size != 32)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type chromaticities: Size must be 32 bytes, was " + Size + ".");
			}
			Value = new Chromaticities(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
			break;
		case "compression":
			if (Size != 1)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type compression: Size must be 1 byte, was " + Size + ".");
			}
			Value = (EXRCompression)reader.ReadByte();
			break;
		case "double":
			if (Size != 8)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type double: Size must be 8 bytes, was " + Size + ".");
			}
			Value = reader.ReadDouble();
			break;
		case "envmap":
			if (Size != 1)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type envmap: Size must be 1 byte, was " + Size + ".");
			}
			Value = (EnvMap)reader.ReadByte();
			break;
		case "float":
			if (Size != 4)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type float: Size must be 4 bytes, was " + Size + ".");
			}
			Value = reader.ReadSingle();
			break;
		case "int":
			if (Size != 4)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type int: Size must be 4 bytes, was " + Size + ".");
			}
			Value = reader.ReadInt32();
			break;
		case "keycode":
			if (Size != 28)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type keycode: Size must be 28 bytes, was " + Size + ".");
			}
			Value = new KeyCode(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
			break;
		case "lineOrder":
			if (Size != 1)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type lineOrder: Size must be 1 byte, was " + Size + ".");
			}
			Value = (LineOrder)reader.ReadByte();
			break;
		case "m33f":
			if (Size != 36)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type m33f: Size must be 36 bytes, was " + Size + ".");
			}
			Value = new M33F(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
			break;
		case "m44f":
			if (Size != 64)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type m44f: Size must be 64 bytes, was " + Size + ".");
			}
			Value = new M44F(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
			break;
		case "rational":
			if (Size != 8)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type rational: Size must be 8 bytes, was " + Size + ".");
			}
			Value = new Rational(reader.ReadInt32(), reader.ReadUInt32());
			break;
		case "string":
			if (Size < 0)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type string: Invalid Size, was " + Size + ".");
			}
			Value = reader.ReadString(Size);
			break;
		case "stringvector":
		{
			if (Size == 0)
			{
				Value = new List<string>();
				break;
			}
			if (Size < 4)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type stringvector: Size must be at least 4 bytes or 0 bytes, was " + Size + ".");
			}
			List<string> list = (List<string>)(Value = new List<string>());
			int i;
			int position;
			for (i = 0; i < Size; i += reader.Position - position)
			{
				position = reader.Position;
				string item = reader.ReadString();
				list.Add(item);
			}
			if (i == Size)
			{
				break;
			}
			throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type stringvector: Read " + i + " bytes but Size was " + Size + ".");
		}
		case "tiledesc":
			if (Size != 9)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type tiledesc: Size must be 9 bytes, was " + Size + ".");
			}
			Value = new TileDesc(reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadByte());
			break;
		case "timecode":
			if (Size != 8)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type timecode: Size must be 8 bytes, was " + Size + ".");
			}
			Value = new TimeCode(reader.ReadUInt32(), reader.ReadUInt32());
			break;
		case "v2i":
			if (Size != 8)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type v2i: Size must be 8 bytes, was " + Size + ".");
			}
			Value = new V2I(reader.ReadInt32(), reader.ReadInt32());
			break;
		case "v2f":
			if (Size != 8)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type v2f: Size must be 8 bytes, was " + Size + ".");
			}
			Value = new V2F(reader.ReadSingle(), reader.ReadSingle());
			break;
		case "v3i":
			if (Size != 12)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type v3i: Size must be 12 bytes, was " + Size + ".");
			}
			Value = new V3I(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
			break;
		case "v3f":
			if (Size != 12)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type v3f: Size must be 12 bytes, was " + Size + ".");
			}
			Value = new V3F(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
			break;
		case "chlist":
		{
			ChannelList channelList = new ChannelList();
			try
			{
				channelList.Read(file, reader, Size);
			}
			catch (Exception ex3)
			{
				throw new EXRFormatException("Invalid or corrupt EXR header attribute '" + Name + "' of type chlist: " + ex3.Message, ex3);
			}
			Value = channelList;
			break;
		}
		default:
			Value = reader.ReadBytes(Size);
			break;
		}
		return true;
	}
}
