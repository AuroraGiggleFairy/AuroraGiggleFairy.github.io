using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public readonly ref struct StringSpan(ReadOnlySpan<char> span)
{
	public ref struct CharSplitEnumerator(ReadOnlySpan<char> span, char separator, StringSplitOptions options = StringSplitOptions.None)
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public ReadOnlySpan<char> m_remainder = span;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly char m_separator = separator;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool m_removeEmptyEntries = options.HasFlag(StringSplitOptions.RemoveEmptyEntries);

		[PublicizedFrom(EAccessModifier.Private)]
		public StringSpan m_current = default(StringSpan);

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_done = false;

		public StringSpan Current => m_current;

		public CharSplitEnumerator GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			while (MoveToNextInternal())
			{
				if (!m_removeEmptyEntries || m_current.Length != 0)
				{
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool MoveToNextInternal()
		{
			if (m_done)
			{
				return false;
			}
			for (int i = 0; i < m_remainder.Length; i++)
			{
				if (m_remainder[i] == m_separator)
				{
					ReadOnlySpan<char> remainder = m_remainder;
					m_current = remainder.Slice(0, i);
					remainder = m_remainder;
					int num = i + 1;
					m_remainder = remainder.Slice(num, remainder.Length - num);
					return true;
				}
			}
			m_current = m_remainder;
			m_remainder = default(ReadOnlySpan<char>);
			m_done = true;
			return true;
		}
	}

	public ref struct StringSplitEnumerator(ReadOnlySpan<char> span, ReadOnlySpan<char> separator, StringSplitOptions options = StringSplitOptions.None)
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public ReadOnlySpan<char> m_remainder = span;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ReadOnlySpan<char> m_separator = separator;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool m_removeEmptyEntries = options.HasFlag(StringSplitOptions.RemoveEmptyEntries);

		[PublicizedFrom(EAccessModifier.Private)]
		public StringSpan m_current = default(StringSpan);

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_done = false;

		public StringSpan Current => m_current;

		public StringSplitEnumerator GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			while (MoveToNextInternal())
			{
				if (!m_removeEmptyEntries || m_current.Length != 0)
				{
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool MoveToNextInternal()
		{
			if (m_done)
			{
				return false;
			}
			if (m_separator.Length <= 0)
			{
				m_current = m_remainder;
				m_remainder = default(ReadOnlySpan<char>);
				m_done = true;
				return true;
			}
			int num = m_remainder.Length + 1 - m_separator.Length;
			for (int i = 0; i < num; i++)
			{
				if (m_remainder.Slice(i, m_separator.Length).CompareTo(m_separator, StringComparison.Ordinal) == 0)
				{
					ReadOnlySpan<char> remainder = m_remainder;
					m_current = remainder.Slice(0, i);
					remainder = m_remainder;
					int num2 = i + m_separator.Length;
					m_remainder = remainder.Slice(num2, remainder.Length - num2);
					return true;
				}
			}
			m_current = m_remainder;
			m_remainder = default(ReadOnlySpan<char>);
			m_done = true;
			return true;
		}
	}

	public ref struct WhitespaceSplitEnumerator(ReadOnlySpan<char> text, StringSplitOptions options = StringSplitOptions.None)
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public StringSpan m_remainder = text;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool m_removeEmptyEntries = options.HasFlag(StringSplitOptions.RemoveEmptyEntries);

		[PublicizedFrom(EAccessModifier.Private)]
		public StringSpan m_current = default(StringSpan);

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_done = false;

		public StringSpan Current => m_current;

		public WhitespaceSplitEnumerator GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			while (MoveToNextInternal())
			{
				if (!m_removeEmptyEntries || m_current.Length != 0)
				{
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool MoveToNextInternal()
		{
			if (m_done)
			{
				return false;
			}
			for (int i = 0; i < m_remainder.Length; i++)
			{
				if (char.IsWhiteSpace(m_remainder[i]))
				{
					StringSpan remainder = m_remainder;
					m_current = remainder.Slice(0, i);
					remainder = m_remainder;
					int num = i + 1;
					m_remainder = remainder.Slice(num, remainder.Length - num);
					return true;
				}
			}
			m_current = m_remainder;
			m_remainder = default(StringSpan);
			m_done = true;
			return true;
		}
	}

	public ref struct SeparatorSplitAnyEnumerator
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public StringSpan m_remainder;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool m_removeEmptyEntries;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string[] m_separators;

		[PublicizedFrom(EAccessModifier.Private)]
		public StringSpan m_current;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_done;

		public StringSpan Current => m_current;

		public SeparatorSplitAnyEnumerator(ReadOnlySpan<char> text, StringSplitOptions options = StringSplitOptions.None, params string[] separators)
		{
			m_remainder = text;
			m_removeEmptyEntries = options.HasFlag(StringSplitOptions.RemoveEmptyEntries);
			bool flag = false;
			if (separators != null)
			{
				for (int i = 0; i < separators.Length; i++)
				{
					if (!string.IsNullOrEmpty(separators[i]))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				throw new ArgumentException("StringSplitEnumerator requires at least one non-empty separator");
			}
			m_separators = separators;
			m_current = default(StringSpan);
			m_done = false;
		}

		public SeparatorSplitAnyEnumerator GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			while (MoveToNextInternal())
			{
				if (!m_removeEmptyEntries || m_current.Length != 0)
				{
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool MoveToNextInternal()
		{
			if (m_done)
			{
				return false;
			}
			for (int i = 0; i < m_remainder.Length; i++)
			{
				string[] separators = m_separators;
				foreach (string text in separators)
				{
					if (!string.IsNullOrEmpty(text) && m_remainder[i] == text[0] && i + text.Length <= m_remainder.Length && m_remainder.Slice(i, text.Length).CompareTo(text.AsSpan()) == 0)
					{
						StringSpan remainder = m_remainder;
						m_current = remainder.Slice(0, i);
						remainder = m_remainder;
						int num = i + text.Length;
						m_remainder = remainder.Slice(num, remainder.Length - num);
						return true;
					}
				}
			}
			m_current = m_remainder;
			m_remainder = default(StringSpan);
			m_done = true;
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ReadOnlySpan<char> m_span = span;

	public int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_span.Length;
		}
	}

	public ref readonly char this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return ref m_span[index];
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySpan<char> AsSpan()
	{
		return m_span;
	}

	public override string ToString()
	{
		return new string(m_span);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public StringSpan Slice(int start)
	{
		return m_span.Slice(start);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public StringSpan Slice(int start, int length)
	{
		return m_span.Slice(start, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(StringSpan other, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (!(m_span == other.m_span))
		{
			return m_span.Equals(other.AsSpan(), comparisonType);
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CompareTo(StringSpan other, StringComparison comparisonType = StringComparison.Ordinal)
	{
		if (!(m_span == other.m_span))
		{
			return m_span.CompareTo(other.AsSpan(), comparisonType);
		}
		return 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(char value)
	{
		return IndexOf(value) >= 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(char value, StringComparison comparisonType)
	{
		return IndexOf(value, comparisonType) >= 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(string value)
	{
		return IndexOf(value, StringComparison.Ordinal) >= 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(string value, StringComparison comparisonType)
	{
		return IndexOf(value, comparisonType) >= 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int IndexOf(char value)
	{
		return m_span.IndexOf(value);
	}

	public int IndexOf(char value, StringComparison comparisonType)
	{
		return m_span.IndexOf(MemoryMarshal.CreateReadOnlySpan(ref value, 1), comparisonType);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int IndexOf(StringSpan value)
	{
		return m_span.IndexOf(value.AsSpan());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int IndexOf(StringSpan value, StringComparison comparisonType)
	{
		return m_span.IndexOf(value.AsSpan(), comparisonType);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int LastIndexOf(char value)
	{
		return m_span.LastIndexOf(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int LastIndexOf(StringSpan value)
	{
		return m_span.LastIndexOf(value.AsSpan());
	}

	public int IndexOfAny(StringSpan value)
	{
		return m_span.IndexOfAny(value.AsSpan());
	}

	public int LastIndexOfAny(StringSpan value)
	{
		return m_span.LastIndexOfAny(value.AsSpan());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public WhitespaceSplitEnumerator GetSplitEnumerator(StringSplitOptions options = StringSplitOptions.None)
	{
		return new WhitespaceSplitEnumerator(m_span, options);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public CharSplitEnumerator GetSplitEnumerator(char separator, StringSplitOptions options = StringSplitOptions.None)
	{
		return new CharSplitEnumerator(m_span, separator, options);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public StringSplitEnumerator GetSplitEnumerator(ReadOnlySpan<char> separator, StringSplitOptions options = StringSplitOptions.None)
	{
		return new StringSplitEnumerator(m_span, separator, options);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SeparatorSplitAnyEnumerator GetSplitAnyEnumerator(string[] separator, StringSplitOptions options = StringSplitOptions.None)
	{
		return new SeparatorSplitAnyEnumerator(m_span, options, separator);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public StringSpan Substring(int startIndex)
	{
		StringSpan stringSpan = this;
		return stringSpan.Slice(startIndex, stringSpan.Length - startIndex);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public StringSpan Substring(int startIndex, int length)
	{
		StringSpan stringSpan = this;
		return stringSpan.Slice(startIndex, startIndex + length - startIndex);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public StringSpan Trim()
	{
		int i;
		for (i = 0; i < Length && char.IsWhiteSpace(this[i]); i++)
		{
		}
		int num = Length - 1;
		while (num >= i && char.IsWhiteSpace(this[num]))
		{
			num--;
		}
		if (i > num)
		{
			return default(StringSpan);
		}
		return Slice(i, num - i + 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator StringSpan(string str)
	{
		return new StringSpan(str);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator StringSpan(ReadOnlySpan<char> span)
	{
		return new StringSpan(span);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator string(StringSpan span)
	{
		return new string(span.m_span);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator ReadOnlySpan<char>(StringSpan span)
	{
		return span.m_span;
	}

	public override bool Equals(object obj)
	{
		throw new NotSupportedException("StringSpan.Equals(object) is not supported. Use another method or the operator == instead.");
	}

	public override int GetHashCode()
	{
		return SpanUtils.GetHashCode(m_span);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(StringSpan left, StringSpan right)
	{
		return left.Equals(right);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(StringSpan left, StringSpan right)
	{
		return !(left == right);
	}
}
