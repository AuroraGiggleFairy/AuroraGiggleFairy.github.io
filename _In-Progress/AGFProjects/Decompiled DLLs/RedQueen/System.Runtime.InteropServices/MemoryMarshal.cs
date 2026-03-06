using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices;

internal static class MemoryMarshal
{
	public static bool TryGetArray<T>(ReadOnlyMemory<T> memory, out ArraySegment<T> segment)
	{
		int start;
		int length;
		object objectStartLength = memory.GetObjectStartLength(out start, out length);
		if (start < 0)
		{
			if (((MemoryManager<T>)objectStartLength).TryGetArray(out var segment2))
			{
				segment = new ArraySegment<T>(segment2.Array, segment2.Offset + (start & 0x7FFFFFFF), length);
				return true;
			}
		}
		else if (objectStartLength is T[] array)
		{
			segment = new ArraySegment<T>(array, start, length & 0x7FFFFFFF);
			return true;
		}
		if ((length & 0x7FFFFFFF) == 0)
		{
			segment = new ArraySegment<T>(SpanHelpers.PerTypeValues<T>.EmptyArray);
			return true;
		}
		segment = default(ArraySegment<T>);
		return false;
	}

	public static bool TryGetMemoryManager<T, TManager>(ReadOnlyMemory<T> memory, out TManager manager) where TManager : MemoryManager<T>
	{
		int start;
		int length;
		TManager val = (manager = memory.GetObjectStartLength(out start, out length) as TManager);
		return manager != null;
	}

	public static bool TryGetMemoryManager<T, TManager>(ReadOnlyMemory<T> memory, out TManager manager, out int start, out int length) where TManager : MemoryManager<T>
	{
		TManager val = (manager = memory.GetObjectStartLength(out start, out length) as TManager);
		start &= int.MaxValue;
		if (manager == null)
		{
			start = 0;
			length = 0;
			return false;
		}
		return true;
	}

	public static IEnumerable<T> ToEnumerable<T>(ReadOnlyMemory<T> memory)
	{
		for (int i = 0; i < memory.Length; i++)
		{
			yield return memory.Span[i];
		}
	}

	public static bool TryGetString(ReadOnlyMemory<char> memory, out string text, out int start, out int length)
	{
		if (memory.GetObjectStartLength(out var start2, out var length2) is string text2)
		{
			text = text2;
			start = start2;
			length = length2;
			return true;
		}
		text = null;
		start = 0;
		length = 0;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T Read<T>(ReadOnlySpan<byte> source) where T : struct
	{
		if (SpanHelpers.IsReferenceOrContainsReferences<T>())
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));
		}
		if (Unsafe.SizeOf<T>() > source.Length)
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.length);
		}
		return Unsafe.ReadUnaligned<T>(ref GetReference(source));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryRead<T>(ReadOnlySpan<byte> source, out T value) where T : struct
	{
		if (SpanHelpers.IsReferenceOrContainsReferences<T>())
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));
		}
		if (Unsafe.SizeOf<T>() > (uint)source.Length)
		{
			value = default(T);
			return false;
		}
		value = Unsafe.ReadUnaligned<T>(ref GetReference(source));
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Write<T>(Span<byte> destination, ref T value) where T : struct
	{
		if (SpanHelpers.IsReferenceOrContainsReferences<T>())
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));
		}
		if ((uint)Unsafe.SizeOf<T>() > (uint)destination.Length)
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.length);
		}
		Unsafe.WriteUnaligned(ref GetReference(destination), value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryWrite<T>(Span<byte> destination, ref T value) where T : struct
	{
		if (SpanHelpers.IsReferenceOrContainsReferences<T>())
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));
		}
		if (Unsafe.SizeOf<T>() > (uint)destination.Length)
		{
			return false;
		}
		Unsafe.WriteUnaligned(ref GetReference(destination), value);
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Memory<T> CreateFromPinnedArray<T>(T[] array, int start, int length)
	{
		if (array == null)
		{
			if (start != 0 || length != 0)
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException();
			}
			return default(Memory<T>);
		}
		if (default(T) == null && array.GetType() != typeof(T[]))
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArrayTypeMismatchException();
		}
		if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException();
		}
		return new Memory<T>((object)array, start, length | int.MinValue);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<byte> AsBytes<T>(Span<T> span) where T : struct
	{
		if (SpanHelpers.IsReferenceOrContainsReferences<T>())
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));
		}
		int length = checked(span.Length * Unsafe.SizeOf<T>());
		return new Span<byte>(Unsafe.As<Pinnable<byte>>(span.Pinnable), span.ByteOffset, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<byte> AsBytes<T>(ReadOnlySpan<T> span) where T : struct
	{
		if (SpanHelpers.IsReferenceOrContainsReferences<T>())
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));
		}
		int length = checked(span.Length * Unsafe.SizeOf<T>());
		return new ReadOnlySpan<byte>(Unsafe.As<Pinnable<byte>>(span.Pinnable), span.ByteOffset, length);
	}

	public static Memory<T> AsMemory<T>(ReadOnlyMemory<T> memory)
	{
		return Unsafe.As<ReadOnlyMemory<T>, Memory<T>>(ref memory);
	}

	public unsafe static ref T GetReference<T>(Span<T> span)
	{
		if (span.Pinnable == null)
		{
			return ref Unsafe.AsRef<T>(span.ByteOffset.ToPointer());
		}
		return ref Unsafe.AddByteOffset(ref span.Pinnable.Data, span.ByteOffset);
	}

	public unsafe static ref T GetReference<T>(ReadOnlySpan<T> span)
	{
		if (span.Pinnable == null)
		{
			return ref Unsafe.AsRef<T>(span.ByteOffset.ToPointer());
		}
		return ref Unsafe.AddByteOffset(ref span.Pinnable.Data, span.ByteOffset);
	}

	public static Span<TTo> Cast<TFrom, TTo>(Span<TFrom> span) where TFrom : struct where TTo : struct
	{
		if (SpanHelpers.IsReferenceOrContainsReferences<TFrom>())
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(TFrom));
		}
		if (SpanHelpers.IsReferenceOrContainsReferences<TTo>())
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(TTo));
		}
		checked
		{
			int length = (int)unchecked(checked(unchecked((long)span.Length) * unchecked((long)Unsafe.SizeOf<TFrom>())) / Unsafe.SizeOf<TTo>());
			return new Span<TTo>(Unsafe.As<Pinnable<TTo>>(span.Pinnable), span.ByteOffset, length);
		}
	}

	public static ReadOnlySpan<TTo> Cast<TFrom, TTo>(ReadOnlySpan<TFrom> span) where TFrom : struct where TTo : struct
	{
		if (SpanHelpers.IsReferenceOrContainsReferences<TFrom>())
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(TFrom));
		}
		if (SpanHelpers.IsReferenceOrContainsReferences<TTo>())
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(TTo));
		}
		checked
		{
			int length = (int)unchecked(checked(unchecked((long)span.Length) * unchecked((long)Unsafe.SizeOf<TFrom>())) / Unsafe.SizeOf<TTo>());
			return new ReadOnlySpan<TTo>(Unsafe.As<Pinnable<TTo>>(span.Pinnable), span.ByteOffset, length);
		}
	}
}
