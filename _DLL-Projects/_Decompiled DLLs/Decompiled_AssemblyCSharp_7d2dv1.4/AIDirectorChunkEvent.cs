using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorChunkEvent
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cVersion = 2;

	public EnumAIDirectorChunkEvent EventType;

	public Vector3i Position;

	public float Value;

	public float Duration;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorChunkEvent()
	{
	}

	public AIDirectorChunkEvent(EnumAIDirectorChunkEvent _type, Vector3i _position, float _value, float _duration)
	{
		EventType = _type;
		Position = _position;
		Value = _value;
		Duration = _duration;
	}

	public void Write(BinaryWriter _stream)
	{
		_stream.Write(2);
		_stream.Write(Position.x);
		_stream.Write(Position.y);
		_stream.Write(Position.z);
		_stream.Write(Value);
		_stream.Write((byte)EventType);
		_stream.Write(Duration);
	}

	public static AIDirectorChunkEvent Read(BinaryReader _stream)
	{
		int num = _stream.ReadInt32();
		AIDirectorChunkEvent aIDirectorChunkEvent = new AIDirectorChunkEvent
		{
			Position = 
			{
				x = _stream.ReadInt32(),
				y = _stream.ReadInt32(),
				z = _stream.ReadInt32()
			},
			Value = _stream.ReadSingle(),
			EventType = (EnumAIDirectorChunkEvent)_stream.ReadByte()
		};
		if (num >= 2)
		{
			aIDirectorChunkEvent.Duration = _stream.ReadSingle();
		}
		else
		{
			_stream.ReadUInt64();
		}
		return aIDirectorChunkEvent;
	}
}
