using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Discord.Commands;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal struct TypeReaderValue
{
	public object Value
	{
		[_003Cc51ba3f1_002D43fc_002D4bc9_002D811d_002D97306e7eb83f_003EIsReadOnly]
		get;
	}

	public float Score
	{
		[_003Cc51ba3f1_002D43fc_002D4bc9_002D811d_002D97306e7eb83f_003EIsReadOnly]
		get;
	}

	private string DebuggerDisplay => $"[{Value}, {Math.Round(Score, 2)}]";

	public TypeReaderValue(object value, float score)
	{
		Value = value;
		Score = score;
	}

	public override string ToString()
	{
		return Value?.ToString();
	}
}
