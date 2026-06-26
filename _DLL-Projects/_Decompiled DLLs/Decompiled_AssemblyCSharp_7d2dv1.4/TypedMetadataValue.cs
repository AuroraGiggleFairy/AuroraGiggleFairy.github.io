using System.IO;

public class TypedMetadataValue
{
	public enum TypeTag
	{
		None,
		Float,
		Integer,
		String
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TypeTag typeTag;

	[PublicizedFrom(EAccessModifier.Private)]
	public object value;

	public static TypeTag StringToTag(string str)
	{
		return str switch
		{
			"float" => TypeTag.Float, 
			"int" => TypeTag.Integer, 
			"string" => TypeTag.String, 
			_ => TypeTag.None, 
		};
	}

	public TypedMetadataValue(object val, TypeTag tag)
	{
		typeTag = tag;
		if (ValueMatchesTag(val, tag))
		{
			value = val;
		}
	}

	public object GetValue()
	{
		return value;
	}

	public bool SetValue(object val)
	{
		if (ValueMatchesTag(val, typeTag))
		{
			value = val;
			return true;
		}
		return false;
	}

	public bool ValueMatchesTag(object val, TypeTag tag)
	{
		if (val == null)
		{
			return false;
		}
		return tag switch
		{
			TypeTag.Float => val is float, 
			TypeTag.Integer => val is int, 
			TypeTag.String => val is string, 
			_ => false, 
		};
	}

	public static void Write(TypedMetadataValue tmv, BinaryWriter writer)
	{
		if (tmv != null)
		{
			writer.Write((int)tmv.typeTag);
			switch (tmv.typeTag)
			{
			case TypeTag.Float:
				writer.Write((float)tmv.value);
				break;
			case TypeTag.Integer:
				writer.Write((int)tmv.value);
				break;
			case TypeTag.String:
				writer.Write((string)tmv.value);
				break;
			}
		}
	}

	public static TypedMetadataValue Read(BinaryReader reader)
	{
		object val = null;
		TypeTag typeTag = (TypeTag)reader.ReadInt32();
		switch (typeTag)
		{
		case TypeTag.Float:
			val = reader.ReadSingle();
			break;
		case TypeTag.Integer:
			val = reader.ReadInt32();
			break;
		case TypeTag.String:
			val = reader.ReadString();
			break;
		}
		return new TypedMetadataValue(val, typeTag);
	}

	public override bool Equals(object other)
	{
		if (other is TypedMetadataValue typedMetadataValue && value.Equals(typedMetadataValue.value))
		{
			return typeTag.Equals(typedMetadataValue.typeTag);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public TypedMetadataValue Clone()
	{
		object val = null;
		switch (typeTag)
		{
		case TypeTag.Float:
			val = value as float?;
			break;
		case TypeTag.Integer:
			val = value as int?;
			break;
		case TypeTag.String:
			val = value as string;
			break;
		}
		return new TypedMetadataValue(val, typeTag);
	}
}
