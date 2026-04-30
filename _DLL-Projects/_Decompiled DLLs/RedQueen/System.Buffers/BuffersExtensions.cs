using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Buffers;

internal static class BuffersExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SequencePosition? PositionOf<T>([In][_003Ca49c99bc_002D5074_002D4086_002Dac07_002Dfb6cba902e04_003EIsReadOnly] this ref ReadOnlySequence<T> source, T value) where T : IEquatable<T>
	{
		if (source.IsSingleSegment)
		{
			int num = source.First.Span.IndexOf(value);
			if (num != -1)
			{
				return source.GetPosition(num);
			}
			return null;
		}
		return PositionOfMultiSegment(ref source, value);
	}

	private static SequencePosition? PositionOfMultiSegment<T>([In][_003Ca49c99bc_002D5074_002D4086_002Dac07_002Dfb6cba902e04_003EIsReadOnly] ref ReadOnlySequence<T> source, T value) where T : IEquatable<T>
	{
		SequencePosition position = source.Start;
		SequencePosition origin = position;
		ReadOnlyMemory<T> memory;
		while (source.TryGet(ref position, out memory))
		{
			int num = memory.Span.IndexOf(value);
			if (num != -1)
			{
				return source.GetPosition(num, origin);
			}
			if (position.GetObject() == null)
			{
				break;
			}
			origin = position;
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void CopyTo<T>([In][_003Ca49c99bc_002D5074_002D4086_002Dac07_002Dfb6cba902e04_003EIsReadOnly] this ref ReadOnlySequence<T> source, Span<T> destination)
	{
		if (source.Length > destination.Length)
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.destination);
		}
		if (source.IsSingleSegment)
		{
			source.First.Span.CopyTo(destination);
		}
		else
		{
			CopyToMultiSegment(ref source, destination);
		}
	}

	private static void CopyToMultiSegment<T>([In][_003Ca49c99bc_002D5074_002D4086_002Dac07_002Dfb6cba902e04_003EIsReadOnly] ref ReadOnlySequence<T> sequence, Span<T> destination)
	{
		SequencePosition position = sequence.Start;
		ReadOnlyMemory<T> memory;
		while (sequence.TryGet(ref position, out memory))
		{
			ReadOnlySpan<T> span = memory.Span;
			span.CopyTo(destination);
			if (position.GetObject() != null)
			{
				destination = destination.Slice(span.Length);
				continue;
			}
			break;
		}
	}

	public static T[] ToArray<T>([In][_003Ca49c99bc_002D5074_002D4086_002Dac07_002Dfb6cba902e04_003EIsReadOnly] this ref ReadOnlySequence<T> sequence)
	{
		T[] array = new T[sequence.Length];
		sequence.CopyTo(array);
		return array;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Write<T>(this IBufferWriter<T> writer, ReadOnlySpan<T> value)
	{
		Span<T> span = writer.GetSpan();
		if (value.Length <= span.Length)
		{
			value.CopyTo(span);
			writer.Advance(value.Length);
		}
		else
		{
			WriteMultiSegment(writer, ref value, span);
		}
	}

	private static void WriteMultiSegment<T>(IBufferWriter<T> writer, [In][_003Ca49c99bc_002D5074_002D4086_002Dac07_002Dfb6cba902e04_003EIsReadOnly] ref ReadOnlySpan<T> source, Span<T> destination)
	{
		ReadOnlySpan<T> readOnlySpan = source;
		while (true)
		{
			int num = Math.Min(destination.Length, readOnlySpan.Length);
			readOnlySpan.Slice(0, num).CopyTo(destination);
			writer.Advance(num);
			readOnlySpan = readOnlySpan.Slice(num);
			if (readOnlySpan.Length > 0)
			{
				destination = writer.GetSpan(readOnlySpan.Length);
				continue;
			}
			break;
		}
	}
}
