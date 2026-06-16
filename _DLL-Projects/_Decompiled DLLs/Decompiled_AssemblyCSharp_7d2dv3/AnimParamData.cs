using System.IO;

public struct AnimParamData
{
	public enum ValueTypes : byte
	{
		Bool,
		Trigger,
		Float,
		Int,
		DataFloat
	}

	public readonly int NameHash;

	public readonly ValueTypes ValueType;

	public readonly float FloatValue;

	public readonly int IntValue;

	public AnimParamData(int _nameHash, ValueTypes _valueType, bool _value)
	{
		NameHash = _nameHash;
		ValueType = _valueType;
		FloatValue = 0f;
		IntValue = (_value ? 1 : 0);
	}

	public AnimParamData(int _nameHash, ValueTypes _valueType, float _value)
	{
		NameHash = _nameHash;
		ValueType = _valueType;
		FloatValue = _value;
		IntValue = 0;
	}

	public AnimParamData(int _nameHash, ValueTypes _valueType, int _value)
	{
		NameHash = _nameHash;
		ValueType = _valueType;
		FloatValue = 0f;
		IntValue = _value;
	}

	public static AnimParamData CreateFromBinary(BinaryReader _br)
	{
		int nameHash = _br.ReadInt32();
		ValueTypes valueTypes = (ValueTypes)_br.ReadByte();
		switch (valueTypes)
		{
		case ValueTypes.Bool:
		case ValueTypes.Trigger:
			return new AnimParamData(nameHash, valueTypes, _br.ReadBoolean());
		case ValueTypes.Float:
		case ValueTypes.DataFloat:
			return new AnimParamData(nameHash, valueTypes, _br.ReadSingle());
		case ValueTypes.Int:
			return new AnimParamData(nameHash, valueTypes, _br.ReadInt32());
		default:
		{
			byte b = (byte)valueTypes;
			throw new InvalidDataException("Invalid Value Type: " + b);
		}
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(NameHash);
		_bw.Write((byte)ValueType);
		switch (ValueType)
		{
		case ValueTypes.Bool:
		case ValueTypes.Trigger:
			_bw.Write(IntValue != 0);
			break;
		case ValueTypes.Float:
		case ValueTypes.DataFloat:
			_bw.Write(FloatValue);
			break;
		case ValueTypes.Int:
			_bw.Write(IntValue);
			break;
		}
	}

	public string ToString(AvatarController _controller)
	{
		string parameterName = _controller.GetParameterName(NameHash);
		return $"{parameterName} {NameHash}, {ValueType}, f{FloatValue}, i{IntValue}";
	}
}
