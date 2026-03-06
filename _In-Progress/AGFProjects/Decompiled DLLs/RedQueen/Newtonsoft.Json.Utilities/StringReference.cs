using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Utilities;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C7efad6e0_002D6dbc_002D40f5_002Dac7f_002De8a284fe164b_003EIsReadOnly]
internal struct StringReference
{
	private readonly char[] _chars;

	private readonly int _startIndex;

	private readonly int _length;

	public char this[int i] => _chars[i];

	public char[] Chars => _chars;

	public int StartIndex => _startIndex;

	public int Length => _length;

	public StringReference(char[] chars, int startIndex, int length)
	{
		_chars = chars;
		_startIndex = startIndex;
		_length = length;
	}

	public override string ToString()
	{
		return new string(_chars, _startIndex, _length);
	}
}
