using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System;

[DebuggerDisplay("{ToString(),raw}")]
[DebuggerTypeProxy(typeof(SpanDebugView<>))]
[_003Ca49c99bc_002D5074_002D4086_002Dac07_002Dfb6cba902e04_003EIsReadOnly]
[DebuggerTypeProxy(typeof(SpanDebugView<>))]
internal ref struct Span<T>
{
	public ref struct Enumerator(Span<T> span)
	{
		private readonly Span<T> _span = span;

		private int _index = -1;

		public ref T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return ref _span[_index];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			int num = _index + 1;
			if (num < _span.Length)
			{
				_index = num;
				return true;
			}
			return false;
		}
	}

	private readonly Pinnable<T> _pinnable;

	private readonly IntPtr _byteOffset;

	private readonly int _length;

	public int Length => _length;

	public bool IsEmpty => _length == 0;

	public static Span<T> Empty => default(Span<T>);

	public unsafe ref T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if ((uint)index >= (uint)_length)
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowIndexOutOfRangeException();
			}
			if (_pinnable == null)
			{
				return ref Unsafe.Add(ref Unsafe.AsRef<T>(_byteOffset.ToPointer()), index);
			}
			return ref Unsafe.Add(ref Unsafe.AddByteOffset(ref _pinnable.Data, _byteOffset), index);
		}
	}

	internal Pinnable<T> Pinnable => _pinnable;

	internal IntPtr ByteOffset => _byteOffset;

	public static bool operator !=(Span<T> left, Span<T> right)
	{
		return !(left == right);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Equals() on Span will always throw an exception. Use == instead.")]
	public override bool Equals(object obj)
	{
		throw new NotSupportedException(_003Ce08d9188_002D0e65_002D4efd_002Da7ab_002D1ba7e9ec9e51_003ESR.NotSupported_CannotCallEqualsOnSpan);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("GetHashCode() on Span will always throw an exception.")]
	public override int GetHashCode()
	{
		throw new NotSupportedException(_003Ce08d9188_002D0e65_002D4efd_002Da7ab_002D1ba7e9ec9e51_003ESR.NotSupported_CannotCallGetHashCodeOnSpan);
	}

	public static implicit operator Span<T>(T[] array)
	{
		return new Span<T>(array);
	}

	public static implicit operator Span<T>(ArraySegment<T> segment)
	{
		return new Span<T>(segment.Array, segment.Offset, segment.Count);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span(T[] array)
	{
		if (array == null)
		{
			this = default(Span<T>);
			return;
		}
		if (default(T) == null && array.GetType() != typeof(T[]))
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArrayTypeMismatchException();
		}
		_length = array.Length;
		_pinnable = Unsafe.As<Pinnable<T>>(array);
		_byteOffset = SpanHelpers.PerTypeValues<T>.ArrayAdjustment;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Span<T> Create(T[] array, int start)
	{
		if (array == null)
		{
			if (start != 0)
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.start);
			}
			return default(Span<T>);
		}
		if (default(T) == null && array.GetType() != typeof(T[]))
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArrayTypeMismatchException();
		}
		if ((uint)start > (uint)array.Length)
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.start);
		}
		IntPtr byteOffset = SpanHelpers.PerTypeValues<T>.ArrayAdjustment.Add<T>(start);
		int length = array.Length - start;
		return new Span<T>(Unsafe.As<Pinnable<T>>(array), byteOffset, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span(T[] array, int start, int length)
	{
		if (array == null)
		{
			if (start != 0 || length != 0)
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.start);
			}
			this = default(Span<T>);
			return;
		}
		if (default(T) == null && array.GetType() != typeof(T[]))
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArrayTypeMismatchException();
		}
		if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.start);
		}
		_length = length;
		_pinnable = Unsafe.As<Pinnable<T>>(array);
		_byteOffset = SpanHelpers.PerTypeValues<T>.ArrayAdjustment.Add<T>(start);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CLSCompliant(false)]
	public unsafe Span(void* pointer, int length)
	{
		if (SpanHelpers.IsReferenceOrContainsReferences<T>())
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));
		}
		if (length < 0)
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.start);
		}
		_length = length;
		_pinnable = null;
		_byteOffset = new IntPtr(pointer);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal Span(Pinnable<T> pinnable, IntPtr byteOffset, int length)
	{
		_length = length;
		_pinnable = pinnable;
		_byteOffset = byteOffset;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public unsafe ref T GetPinnableReference()
	{
		if (_length != 0)
		{
			if (_pinnable == null)
			{
				return ref Unsafe.AsRef<T>(_byteOffset.ToPointer());
			}
			return ref Unsafe.AddByteOffset(ref _pinnable.Data, _byteOffset);
		}
		return ref Unsafe.AsRef<T>(null);
	}

	public unsafe void Clear()
	{
		int length = _length;
		if (length == 0)
		{
			return;
		}
		UIntPtr byteLength = (UIntPtr)(ulong)((uint)length * Unsafe.SizeOf<T>());
		if ((Unsafe.SizeOf<T>() & (sizeof(IntPtr) - 1)) != 0)
		{
			if (_pinnable == null)
			{
				byte* ptr = (byte*)_byteOffset.ToPointer();
				SpanHelpers.ClearLessThanPointerSized(ptr, byteLength);
			}
			else
			{
				SpanHelpers.ClearLessThanPointerSized(ref Unsafe.As<T, byte>(ref Unsafe.AddByteOffset(ref _pinnable.Data, _byteOffset)), byteLength);
			}
		}
		else if (SpanHelpers.IsReferenceOrContainsReferences<T>())
		{
			UIntPtr pointerSizeLength = (UIntPtr)(ulong)(length * Unsafe.SizeOf<T>() / sizeof(IntPtr));
			SpanHelpers.ClearPointerSizedWithReferences(ref Unsafe.As<T, IntPtr>(ref DangerousGetPinnableReference()), pointerSizeLength);
		}
		else
		{
			SpanHelpers.ClearPointerSizedWithoutReferences(ref Unsafe.As<T, byte>(ref DangerousGetPinnableReference()), byteLength);
		}
	}

	public unsafe void Fill(T value)
	{
		int length = _length;
		if (length == 0)
		{
			return;
		}
		if (Unsafe.SizeOf<T>() == 1)
		{
			byte value2 = Unsafe.As<T, byte>(ref value);
			if (_pinnable == null)
			{
				Unsafe.InitBlockUnaligned(_byteOffset.ToPointer(), value2, (uint)length);
			}
			else
			{
				Unsafe.InitBlockUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.AddByteOffset(ref _pinnable.Data, _byteOffset)), value2, (uint)length);
			}
			return;
		}
		ref T source = ref DangerousGetPinnableReference();
		int i;
		for (i = 0; i < (length & -8); i += 8)
		{
			Unsafe.Add(ref source, i) = value;
			Unsafe.Add(ref source, i + 1) = value;
			Unsafe.Add(ref source, i + 2) = value;
			Unsafe.Add(ref source, i + 3) = value;
			Unsafe.Add(ref source, i + 4) = value;
			Unsafe.Add(ref source, i + 5) = value;
			Unsafe.Add(ref source, i + 6) = value;
			Unsafe.Add(ref source, i + 7) = value;
		}
		if (i < (length & -4))
		{
			Unsafe.Add(ref source, i) = value;
			Unsafe.Add(ref source, i + 1) = value;
			Unsafe.Add(ref source, i + 2) = value;
			Unsafe.Add(ref source, i + 3) = value;
			i += 4;
		}
		for (; i < length; i++)
		{
			Unsafe.Add(ref source, i) = value;
		}
	}

	public void CopyTo(Span<T> destination)
	{
		if (!TryCopyTo(destination))
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentException_DestinationTooShort();
		}
	}

	public bool TryCopyTo(Span<T> destination)
	{
		int length = _length;
		int length2 = destination._length;
		if (length == 0)
		{
			return true;
		}
		if ((uint)length > (uint)length2)
		{
			return false;
		}
		ref T src = ref DangerousGetPinnableReference();
		SpanHelpers.CopyTo(ref destination.DangerousGetPinnableReference(), length2, ref src, length);
		return true;
	}

	public static bool operator ==(Span<T> left, Span<T> right)
	{
		if (left._length == right._length)
		{
			return Unsafe.AreSame(ref left.DangerousGetPinnableReference(), ref right.DangerousGetPinnableReference());
		}
		return false;
	}

	public static implicit operator ReadOnlySpan<T>(Span<T> span)
	{
		return new ReadOnlySpan<T>(span._pinnable, span._byteOffset, span._length);
	}

	public unsafe override string ToString()
	{
		if (typeof(T) == typeof(char))
		{
			fixed (char* value = &Unsafe.As<T, char>(ref DangerousGetPinnableReference()))
			{
				return new string(value, 0, _length);
			}
		}
		return $"System.Span<{typeof(T).Name}>[{_length}]";
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<T> Slice(int start)
	{
		if ((uint)start > (uint)_length)
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.start);
		}
		IntPtr byteOffset = _byteOffset.Add<T>(start);
		int length = _length - start;
		return new Span<T>(_pinnable, byteOffset, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<T> Slice(int start, int length)
	{
		if ((uint)start > (uint)_length || (uint)length > (uint)(_length - start))
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.start);
		}
		IntPtr byteOffset = _byteOffset.Add<T>(start);
		return new Span<T>(_pinnable, byteOffset, length);
	}

	public T[] ToArray()
	{
		if (_length == 0)
		{
			return SpanHelpers.PerTypeValues<T>.EmptyArray;
		}
		T[] array = new T[_length];
		CopyTo(array);
		return array;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal unsafe ref T DangerousGetPinnableReference()
	{
		if (_pinnable == null)
		{
			return ref Unsafe.AsRef<T>(_byteOffset.ToPointer());
		}
		return ref Unsafe.AddByteOffset(ref _pinnable.Data, _byteOffset);
	}
}
