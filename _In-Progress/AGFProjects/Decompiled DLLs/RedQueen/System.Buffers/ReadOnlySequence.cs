using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Buffers;

[DebuggerDisplay("{ToString(),raw}")]
[_003Ca49c99bc_002D5074_002D4086_002Dac07_002Dfb6cba902e04_003EIsReadOnly]
[DebuggerTypeProxy(typeof(ReadOnlySequenceDebugView<>))]
internal struct ReadOnlySequence<T>
{
	public struct Enumerator([In][_003Ca49c99bc_002D5074_002D4086_002Dac07_002Dfb6cba902e04_003EIsReadOnly] ref ReadOnlySequence<T> sequence)
	{
		private readonly ReadOnlySequence<T> _sequence = sequence;

		private SequencePosition _next = sequence.Start;

		private ReadOnlyMemory<T> _currentMemory = default(ReadOnlyMemory<T>);

		public ReadOnlyMemory<T> Current => _currentMemory;

		public bool MoveNext()
		{
			if (_next.GetObject() == null)
			{
				return false;
			}
			return _sequence.TryGet(ref _next, out _currentMemory);
		}
	}

	private enum SequenceType
	{
		MultiSegment,
		Array,
		MemoryManager,
		String,
		Empty
	}

	private readonly SequencePosition _sequenceStart;

	private readonly SequencePosition _sequenceEnd;

	public static readonly ReadOnlySequence<T> Empty;

	public long Length => GetLength();

	public bool IsEmpty => Length == 0;

	public bool IsSingleSegment
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _sequenceStart.GetObject() == _sequenceEnd.GetObject();
		}
	}

	public ReadOnlyMemory<T> First => GetFirstBuffer();

	public SequencePosition Start => _sequenceStart;

	public SequencePosition End => _sequenceEnd;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private ReadOnlySequence(object startSegment, int startIndexAndFlags, object endSegment, int endIndexAndFlags)
	{
		_sequenceStart = new SequencePosition(startSegment, startIndexAndFlags);
		_sequenceEnd = new SequencePosition(endSegment, endIndexAndFlags);
	}

	public ReadOnlySequence(ReadOnlySequenceSegment<T> startSegment, int startIndex, ReadOnlySequenceSegment<T> endSegment, int endIndex)
	{
		if (startSegment == null || endSegment == null || (startSegment != endSegment && startSegment.RunningIndex > endSegment.RunningIndex) || (uint)startSegment.Memory.Length < (uint)startIndex || (uint)endSegment.Memory.Length < (uint)endIndex || (startSegment == endSegment && endIndex < startIndex))
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentValidationException(startSegment, startIndex, endSegment);
		}
		_sequenceStart = new SequencePosition(startSegment, ReadOnlySequence.SegmentToSequenceStart(startIndex));
		_sequenceEnd = new SequencePosition(endSegment, ReadOnlySequence.SegmentToSequenceEnd(endIndex));
	}

	public ReadOnlySequence(T[] array)
	{
		if (array == null)
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentNullException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.array);
		}
		_sequenceStart = new SequencePosition(array, ReadOnlySequence.ArrayToSequenceStart(0));
		_sequenceEnd = new SequencePosition(array, ReadOnlySequence.ArrayToSequenceEnd(array.Length));
	}

	public ReadOnlySequence(T[] array, int start, int length)
	{
		if (array == null || (uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentValidationException(array, start);
		}
		_sequenceStart = new SequencePosition(array, ReadOnlySequence.ArrayToSequenceStart(start));
		_sequenceEnd = new SequencePosition(array, ReadOnlySequence.ArrayToSequenceEnd(start + length));
	}

	public ReadOnlySequence(ReadOnlyMemory<T> memory)
	{
		ArraySegment<T> segment;
		if (MemoryMarshal.TryGetMemoryManager<T, MemoryManager<T>>(memory, out var manager, out var start, out var length))
		{
			_sequenceStart = new SequencePosition(manager, ReadOnlySequence.MemoryManagerToSequenceStart(start));
			_sequenceEnd = new SequencePosition(manager, ReadOnlySequence.MemoryManagerToSequenceEnd(length));
		}
		else if (MemoryMarshal.TryGetArray(memory, out segment))
		{
			T[] array = segment.Array;
			int offset = segment.Offset;
			_sequenceStart = new SequencePosition(array, ReadOnlySequence.ArrayToSequenceStart(offset));
			_sequenceEnd = new SequencePosition(array, ReadOnlySequence.ArrayToSequenceEnd(offset + segment.Count));
		}
		else if (typeof(T) == typeof(char))
		{
			if (!MemoryMarshal.TryGetString((ReadOnlyMemory<char>)(object)memory, out var text, out var start2, out length))
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowInvalidOperationException();
			}
			_sequenceStart = new SequencePosition(text, ReadOnlySequence.StringToSequenceStart(start2));
			_sequenceEnd = new SequencePosition(text, ReadOnlySequence.StringToSequenceEnd(start2 + length));
		}
		else
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowInvalidOperationException();
			_sequenceStart = default(SequencePosition);
			_sequenceEnd = default(SequencePosition);
		}
	}

	public ReadOnlySequence<T> Slice(long start, long length)
	{
		if (start < 0 || length < 0)
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowStartOrEndArgumentValidationException(start);
		}
		int index = GetIndex(ref _sequenceStart);
		int index2 = GetIndex(ref _sequenceEnd);
		object obj = _sequenceStart.GetObject();
		object obj2 = _sequenceEnd.GetObject();
		SequencePosition position;
		SequencePosition end;
		if (obj != obj2)
		{
			ReadOnlySequenceSegment<T> readOnlySequenceSegment = (ReadOnlySequenceSegment<T>)obj;
			int num = readOnlySequenceSegment.Memory.Length - index;
			if (num > start)
			{
				index += (int)start;
				position = new SequencePosition(obj, index);
				end = GetEndPosition(readOnlySequenceSegment, obj, index, obj2, index2, length);
			}
			else
			{
				if (num < 0)
				{
					_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
				}
				position = SeekMultiSegment(readOnlySequenceSegment.Next, obj2, index2, start - num, _003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.start);
				int index3 = GetIndex(ref position);
				object obj3 = position.GetObject();
				if (obj3 != obj2)
				{
					end = GetEndPosition((ReadOnlySequenceSegment<T>)obj3, obj3, index3, obj2, index2, length);
				}
				else
				{
					if (index2 - index3 < length)
					{
						_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowStartOrEndArgumentValidationException(0L);
					}
					end = new SequencePosition(obj3, index3 + (int)length);
				}
			}
		}
		else
		{
			if (index2 - index < start)
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowStartOrEndArgumentValidationException(-1L);
			}
			index += (int)start;
			position = new SequencePosition(obj, index);
			if (index2 - index < length)
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowStartOrEndArgumentValidationException(0L);
			}
			end = new SequencePosition(obj, index + (int)length);
		}
		return SliceImpl(ref position, ref end);
	}

	public ReadOnlySequence<T> Slice(long start, SequencePosition end)
	{
		if (start < 0)
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowStartOrEndArgumentValidationException(start);
		}
		uint index = (uint)GetIndex(ref end);
		object obj = end.GetObject();
		uint index2 = (uint)GetIndex(ref _sequenceStart);
		object obj2 = _sequenceStart.GetObject();
		uint index3 = (uint)GetIndex(ref _sequenceEnd);
		object obj3 = _sequenceEnd.GetObject();
		if (obj2 == obj3)
		{
			if (!InRange(index, index2, index3))
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
			}
			if (index - index2 < start)
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowStartOrEndArgumentValidationException(-1L);
			}
		}
		else
		{
			ReadOnlySequenceSegment<T> readOnlySequenceSegment = (ReadOnlySequenceSegment<T>)obj2;
			ulong num = (ulong)(readOnlySequenceSegment.RunningIndex + index2);
			ulong num2 = (ulong)(((ReadOnlySequenceSegment<T>)obj).RunningIndex + index);
			if (!InRange(num2, num, (ulong)(((ReadOnlySequenceSegment<T>)obj3).RunningIndex + index3)))
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
			}
			if ((ulong)((long)num + start) > num2)
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.start);
			}
			int num3 = readOnlySequenceSegment.Memory.Length - (int)index2;
			if (num3 <= start)
			{
				if (num3 < 0)
				{
					_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
				}
				SequencePosition start2 = SeekMultiSegment(readOnlySequenceSegment.Next, obj, (int)index, start - num3, _003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.start);
				return SliceImpl(ref start2, ref end);
			}
		}
		SequencePosition start3 = new SequencePosition(obj2, (int)index2 + (int)start);
		return SliceImpl(ref start3, ref end);
	}

	public ReadOnlySequence<T> Slice(SequencePosition start, long length)
	{
		uint index = (uint)GetIndex(ref start);
		object obj = start.GetObject();
		uint index2 = (uint)GetIndex(ref _sequenceStart);
		object obj2 = _sequenceStart.GetObject();
		uint index3 = (uint)GetIndex(ref _sequenceEnd);
		object obj3 = _sequenceEnd.GetObject();
		if (obj2 == obj3)
		{
			if (!InRange(index, index2, index3))
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
			}
			if (length < 0)
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowStartOrEndArgumentValidationException(0L);
			}
			if (index3 - index < length)
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowStartOrEndArgumentValidationException(0L);
			}
		}
		else
		{
			ReadOnlySequenceSegment<T> readOnlySequenceSegment = (ReadOnlySequenceSegment<T>)obj;
			ulong num = (ulong)(readOnlySequenceSegment.RunningIndex + index);
			ulong start2 = (ulong)(((ReadOnlySequenceSegment<T>)obj2).RunningIndex + index2);
			ulong num2 = (ulong)(((ReadOnlySequenceSegment<T>)obj3).RunningIndex + index3);
			if (!InRange(num, start2, num2))
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
			}
			if (length < 0)
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowStartOrEndArgumentValidationException(0L);
			}
			if ((ulong)((long)num + length) > num2)
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.length);
			}
			int num3 = readOnlySequenceSegment.Memory.Length - (int)index;
			if (num3 < length)
			{
				if (num3 < 0)
				{
					_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
				}
				SequencePosition end = SeekMultiSegment(readOnlySequenceSegment.Next, obj3, (int)index3, length - num3, _003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.length);
				return SliceImpl(ref start, ref end);
			}
		}
		SequencePosition end2 = new SequencePosition(obj, (int)index + (int)length);
		return SliceImpl(ref start, ref end2);
	}

	public ReadOnlySequence<T> Slice(int start, int length)
	{
		return Slice((long)start, (long)length);
	}

	public ReadOnlySequence<T> Slice(int start, SequencePosition end)
	{
		return Slice((long)start, end);
	}

	public ReadOnlySequence<T> Slice(SequencePosition start, int length)
	{
		return Slice(start, (long)length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySequence<T> Slice(SequencePosition start, SequencePosition end)
	{
		BoundsCheck((uint)GetIndex(ref start), start.GetObject(), (uint)GetIndex(ref end), end.GetObject());
		return SliceImpl(ref start, ref end);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySequence<T> Slice(SequencePosition start)
	{
		BoundsCheck(ref start);
		return SliceImpl(ref start, ref _sequenceEnd);
	}

	public ReadOnlySequence<T> Slice(long start)
	{
		if (start < 0)
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowStartOrEndArgumentValidationException(start);
		}
		if (start == 0L)
		{
			return this;
		}
		SequencePosition start2 = Seek(ref _sequenceStart, ref _sequenceEnd, start, _003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.start);
		return SliceImpl(ref start2, ref _sequenceEnd);
	}

	public override string ToString()
	{
		if (typeof(T) == typeof(char))
		{
			ReadOnlySequence<T> source = this;
			ReadOnlySequence<char> sequence = Unsafe.As<ReadOnlySequence<T>, ReadOnlySequence<char>>(ref source);
			if (SequenceMarshal.TryGetString(sequence, out var text, out var start, out var length))
			{
				return text.Substring(start, length);
			}
			if (Length < int.MaxValue)
			{
				return new string(BuffersExtensions.ToArray(ref sequence));
			}
		}
		return $"System.Buffers.ReadOnlySequence<{typeof(T).Name}>[{Length}]";
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(ref this);
	}

	public SequencePosition GetPosition(long offset)
	{
		return GetPosition(offset, _sequenceStart);
	}

	public SequencePosition GetPosition(long offset, SequencePosition origin)
	{
		if (offset < 0)
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException_OffsetOutOfRange();
		}
		return Seek(ref origin, ref _sequenceEnd, offset, _003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.offset);
	}

	public bool TryGet(ref SequencePosition position, out ReadOnlyMemory<T> memory, bool advance = true)
	{
		SequencePosition next;
		bool result = TryGetBuffer(ref position, out memory, out next);
		if (advance)
		{
			position = next;
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool TryGetBuffer([In][_003Ca49c99bc_002D5074_002D4086_002Dac07_002Dfb6cba902e04_003EIsReadOnly] ref SequencePosition position, out ReadOnlyMemory<T> memory, out SequencePosition next)
	{
		object obj = position.GetObject();
		next = default(SequencePosition);
		if (obj == null)
		{
			memory = default(ReadOnlyMemory<T>);
			return false;
		}
		SequenceType sequenceType = GetSequenceType();
		object obj2 = _sequenceEnd.GetObject();
		int index = GetIndex(ref position);
		int index2 = GetIndex(ref _sequenceEnd);
		if (sequenceType == SequenceType.MultiSegment)
		{
			ReadOnlySequenceSegment<T> readOnlySequenceSegment = (ReadOnlySequenceSegment<T>)obj;
			if (readOnlySequenceSegment != obj2)
			{
				ReadOnlySequenceSegment<T> next2 = readOnlySequenceSegment.Next;
				if (next2 == null)
				{
					_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowInvalidOperationException_EndPositionNotReached();
				}
				next = new SequencePosition(next2, 0);
				memory = readOnlySequenceSegment.Memory.Slice(index);
			}
			else
			{
				memory = readOnlySequenceSegment.Memory.Slice(index, index2 - index);
			}
		}
		else
		{
			if (obj != obj2)
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowInvalidOperationException_EndPositionNotReached();
			}
			if (sequenceType == SequenceType.Array)
			{
				memory = new ReadOnlyMemory<T>((T[])obj, index, index2 - index);
			}
			else if (typeof(T) == typeof(char) && sequenceType == SequenceType.String)
			{
				memory = (ReadOnlyMemory<T>)(object)((string)obj).AsMemory(index, index2 - index);
			}
			else
			{
				memory = ((MemoryManager<T>)obj).Memory.Slice(index, index2 - index);
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private ReadOnlyMemory<T> GetFirstBuffer()
	{
		object obj = _sequenceStart.GetObject();
		if (obj == null)
		{
			return default(ReadOnlyMemory<T>);
		}
		int integer = _sequenceStart.GetInteger();
		int integer2 = _sequenceEnd.GetInteger();
		bool flag = obj != _sequenceEnd.GetObject();
		if (integer >= 0)
		{
			if (integer2 >= 0)
			{
				ReadOnlyMemory<T> memory = ((ReadOnlySequenceSegment<T>)obj).Memory;
				if (flag)
				{
					return memory.Slice(integer);
				}
				return memory.Slice(integer, integer2 - integer);
			}
			if (flag)
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowInvalidOperationException_EndPositionNotReached();
			}
			return new ReadOnlyMemory<T>((T[])obj, integer, (integer2 & 0x7FFFFFFF) - integer);
		}
		if (flag)
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowInvalidOperationException_EndPositionNotReached();
		}
		if (typeof(T) == typeof(char) && integer2 < 0)
		{
			return (ReadOnlyMemory<T>)(object)((string)obj).AsMemory(integer & 0x7FFFFFFF, integer2 - integer);
		}
		integer &= 0x7FFFFFFF;
		return ((MemoryManager<T>)obj).Memory.Slice(integer, integer2 - integer);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private SequencePosition Seek([In][_003Ca49c99bc_002D5074_002D4086_002Dac07_002Dfb6cba902e04_003EIsReadOnly] ref SequencePosition start, [In][_003Ca49c99bc_002D5074_002D4086_002Dac07_002Dfb6cba902e04_003EIsReadOnly] ref SequencePosition end, long offset, _003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument argument)
	{
		int index = GetIndex(ref start);
		int index2 = GetIndex(ref end);
		object obj = start.GetObject();
		object obj2 = end.GetObject();
		if (obj != obj2)
		{
			ReadOnlySequenceSegment<T> readOnlySequenceSegment = (ReadOnlySequenceSegment<T>)obj;
			int num = readOnlySequenceSegment.Memory.Length - index;
			if (num <= offset)
			{
				if (num < 0)
				{
					_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
				}
				return SeekMultiSegment(readOnlySequenceSegment.Next, obj2, index2, offset - num, argument);
			}
		}
		else if (index2 - index < offset)
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException(argument);
		}
		return new SequencePosition(obj, index + (int)offset);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static SequencePosition SeekMultiSegment(ReadOnlySequenceSegment<T> currentSegment, object endObject, int endIndex, long offset, _003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument argument)
	{
		while (true)
		{
			if (currentSegment != null && currentSegment != endObject)
			{
				int length = currentSegment.Memory.Length;
				if (length > offset)
				{
					break;
				}
				offset -= length;
				currentSegment = currentSegment.Next;
				continue;
			}
			if (currentSegment == null || endIndex < offset)
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException(argument);
			}
			break;
		}
		return new SequencePosition(currentSegment, (int)offset);
	}

	private void BoundsCheck([In][_003Ca49c99bc_002D5074_002D4086_002Dac07_002Dfb6cba902e04_003EIsReadOnly] ref SequencePosition position)
	{
		uint index = (uint)GetIndex(ref position);
		uint index2 = (uint)GetIndex(ref _sequenceStart);
		uint index3 = (uint)GetIndex(ref _sequenceEnd);
		object obj = _sequenceStart.GetObject();
		object obj2 = _sequenceEnd.GetObject();
		if (obj == obj2)
		{
			if (!InRange(index, index2, index3))
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
			}
			return;
		}
		ulong start = (ulong)(((ReadOnlySequenceSegment<T>)obj).RunningIndex + index2);
		if (!InRange((ulong)(((ReadOnlySequenceSegment<T>)position.GetObject()).RunningIndex + index), start, (ulong)(((ReadOnlySequenceSegment<T>)obj2).RunningIndex + index3)))
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
		}
	}

	private void BoundsCheck(uint sliceStartIndex, object sliceStartObject, uint sliceEndIndex, object sliceEndObject)
	{
		uint index = (uint)GetIndex(ref _sequenceStart);
		uint index2 = (uint)GetIndex(ref _sequenceEnd);
		object obj = _sequenceStart.GetObject();
		object obj2 = _sequenceEnd.GetObject();
		if (obj == obj2)
		{
			if (sliceStartObject != sliceEndObject || sliceStartObject != obj || sliceStartIndex > sliceEndIndex || sliceStartIndex < index || sliceEndIndex > index2)
			{
				_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
			}
			return;
		}
		ulong num = (ulong)(((ReadOnlySequenceSegment<T>)sliceStartObject).RunningIndex + sliceStartIndex);
		ulong num2 = (ulong)(((ReadOnlySequenceSegment<T>)sliceEndObject).RunningIndex + sliceEndIndex);
		if (num > num2)
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
		}
		if (num < (ulong)(((ReadOnlySequenceSegment<T>)obj).RunningIndex + index) || num2 > (ulong)(((ReadOnlySequenceSegment<T>)obj2).RunningIndex + index2))
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
		}
	}

	private static SequencePosition GetEndPosition(ReadOnlySequenceSegment<T> startSegment, object startObject, int startIndex, object endObject, int endIndex, long length)
	{
		int num = startSegment.Memory.Length - startIndex;
		if (num > length)
		{
			return new SequencePosition(startObject, startIndex + (int)length);
		}
		if (num < 0)
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
		}
		return SeekMultiSegment(startSegment.Next, endObject, endIndex, length - num, _003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private SequenceType GetSequenceType()
	{
		return (SequenceType)(-(2 * (_sequenceStart.GetInteger() >> 31) + (_sequenceEnd.GetInteger() >> 31)));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int GetIndex([In][_003Ca49c99bc_002D5074_002D4086_002Dac07_002Dfb6cba902e04_003EIsReadOnly] ref SequencePosition position)
	{
		return position.GetInteger() & 0x7FFFFFFF;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private ReadOnlySequence<T> SliceImpl([In][_003Ca49c99bc_002D5074_002D4086_002Dac07_002Dfb6cba902e04_003EIsReadOnly] ref SequencePosition start, [In][_003Ca49c99bc_002D5074_002D4086_002Dac07_002Dfb6cba902e04_003EIsReadOnly] ref SequencePosition end)
	{
		return new ReadOnlySequence<T>(start.GetObject(), GetIndex(ref start) | (_sequenceStart.GetInteger() & int.MinValue), end.GetObject(), GetIndex(ref end) | (_sequenceEnd.GetInteger() & int.MinValue));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private long GetLength()
	{
		int index = GetIndex(ref _sequenceStart);
		int index2 = GetIndex(ref _sequenceEnd);
		object obj = _sequenceStart.GetObject();
		object obj2 = _sequenceEnd.GetObject();
		if (obj != obj2)
		{
			ReadOnlySequenceSegment<T> readOnlySequenceSegment = (ReadOnlySequenceSegment<T>)obj;
			ReadOnlySequenceSegment<T> readOnlySequenceSegment2 = (ReadOnlySequenceSegment<T>)obj2;
			return readOnlySequenceSegment2.RunningIndex + index2 - (readOnlySequenceSegment.RunningIndex + index);
		}
		return index2 - index;
	}

	internal bool TryGetReadOnlySequenceSegment(out ReadOnlySequenceSegment<T> startSegment, out int startIndex, out ReadOnlySequenceSegment<T> endSegment, out int endIndex)
	{
		object obj = _sequenceStart.GetObject();
		if (obj == null || GetSequenceType() != SequenceType.MultiSegment)
		{
			startSegment = null;
			startIndex = 0;
			endSegment = null;
			endIndex = 0;
			return false;
		}
		startSegment = (ReadOnlySequenceSegment<T>)obj;
		startIndex = GetIndex(ref _sequenceStart);
		endSegment = (ReadOnlySequenceSegment<T>)_sequenceEnd.GetObject();
		endIndex = GetIndex(ref _sequenceEnd);
		return true;
	}

	internal bool TryGetArray(out ArraySegment<T> segment)
	{
		if (GetSequenceType() != SequenceType.Array)
		{
			segment = default(ArraySegment<T>);
			return false;
		}
		int index = GetIndex(ref _sequenceStart);
		segment = new ArraySegment<T>((T[])_sequenceStart.GetObject(), index, GetIndex(ref _sequenceEnd) - index);
		return true;
	}

	internal bool TryGetString(out string text, out int start, out int length)
	{
		if (typeof(T) != typeof(char) || GetSequenceType() != SequenceType.String)
		{
			start = 0;
			length = 0;
			text = null;
			return false;
		}
		start = GetIndex(ref _sequenceStart);
		length = GetIndex(ref _sequenceEnd) - start;
		text = (string)_sequenceStart.GetObject();
		return true;
	}

	private static bool InRange(uint value, uint start, uint end)
	{
		return value - start <= end - start;
	}

	private static bool InRange(ulong value, ulong start, ulong end)
	{
		return value - start <= end - start;
	}

	static ReadOnlySequence()
	{
		Empty = new ReadOnlySequence<T>(SpanHelpers.PerTypeValues<T>.EmptyArray);
	}
}
internal static class ReadOnlySequence
{
	public const int FlagBitMask = int.MinValue;

	public const int IndexBitMask = int.MaxValue;

	public const int SegmentStartMask = 0;

	public const int SegmentEndMask = 0;

	public const int ArrayStartMask = 0;

	public const int ArrayEndMask = int.MinValue;

	public const int MemoryManagerStartMask = int.MinValue;

	public const int MemoryManagerEndMask = 0;

	public const int StringStartMask = int.MinValue;

	public const int StringEndMask = int.MinValue;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int SegmentToSequenceStart(int startIndex)
	{
		return startIndex | 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int SegmentToSequenceEnd(int endIndex)
	{
		return endIndex | 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int ArrayToSequenceStart(int startIndex)
	{
		return startIndex | 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int ArrayToSequenceEnd(int endIndex)
	{
		return endIndex | int.MinValue;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int MemoryManagerToSequenceStart(int startIndex)
	{
		return startIndex | int.MinValue;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int MemoryManagerToSequenceEnd(int endIndex)
	{
		return endIndex | 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int StringToSequenceStart(int startIndex)
	{
		return startIndex | int.MinValue;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int StringToSequenceEnd(int endIndex)
	{
		return endIndex | int.MinValue;
	}
}
