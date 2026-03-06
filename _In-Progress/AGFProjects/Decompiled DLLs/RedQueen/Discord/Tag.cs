using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class Tag<T> : ITag
{
	public TagType Type { get; }

	public int Index { get; }

	public int Length { get; }

	public ulong Key { get; }

	public T Value { get; }

	private string DebuggerDisplay
	{
		get
		{
			T value = Value;
			return string.Format("{0} ({1})", ((value != null) ? value.ToString() : null) ?? "null", Type);
		}
	}

	object ITag.Value => Value;

	internal Tag(TagType type, int index, int length, ulong key, T value)
	{
		Type = type;
		Index = index;
		Length = length;
		Key = key;
		Value = value;
	}

	public override string ToString()
	{
		T value = Value;
		return string.Format("{0} ({1})", ((value != null) ? value.ToString() : null) ?? "null", Type);
	}
}
