using System.IO;
using UnityEngine;

public interface IBinaryReaderOrWriter
{
	Stream BaseStream { get; }

	bool ReadWrite(bool _value);

	byte ReadWrite(byte _value);

	sbyte ReadWrite(sbyte _value);

	char ReadWrite(char _value);

	short ReadWrite(short _value);

	ushort ReadWrite(ushort _value);

	int ReadWrite(int _value);

	uint ReadWrite(uint _value);

	long ReadWrite(long _value);

	ulong ReadWrite(ulong _value);

	float ReadWrite(float _value);

	double ReadWrite(double _value);

	decimal ReadWrite(decimal _value);

	string ReadWrite(string _value);

	void ReadWrite(byte[] _buffer, int _index, int _count);

	Vector3 ReadWrite(Vector3 _value);
}
