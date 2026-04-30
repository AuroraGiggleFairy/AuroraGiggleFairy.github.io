using System.Buffers;
using System.Runtime.CompilerServices;

namespace System;

internal static class _003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper
{
	internal static void ThrowArgumentNullException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument argument)
	{
		throw CreateArgumentNullException(argument);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception CreateArgumentNullException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument argument)
	{
		return new ArgumentNullException(argument.ToString());
	}

	internal static void ThrowArrayTypeMismatchException()
	{
		throw CreateArrayTypeMismatchException();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception CreateArrayTypeMismatchException()
	{
		return new ArrayTypeMismatchException();
	}

	internal static void ThrowArgumentException_InvalidTypeWithPointersNotSupported(Type type)
	{
		throw CreateArgumentException_InvalidTypeWithPointersNotSupported(type);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception CreateArgumentException_InvalidTypeWithPointersNotSupported(Type type)
	{
		return new ArgumentException(_003Ce08d9188_002D0e65_002D4efd_002Da7ab_002D1ba7e9ec9e51_003ESR.Format(_003Ce08d9188_002D0e65_002D4efd_002Da7ab_002D1ba7e9ec9e51_003ESR.Argument_InvalidTypeWithPointersNotSupported, type));
	}

	internal static void ThrowArgumentException_DestinationTooShort()
	{
		throw CreateArgumentException_DestinationTooShort();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception CreateArgumentException_DestinationTooShort()
	{
		return new ArgumentException(_003Ce08d9188_002D0e65_002D4efd_002Da7ab_002D1ba7e9ec9e51_003ESR.Argument_DestinationTooShort);
	}

	internal static void ThrowIndexOutOfRangeException()
	{
		throw CreateIndexOutOfRangeException();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception CreateIndexOutOfRangeException()
	{
		return new IndexOutOfRangeException();
	}

	internal static void ThrowArgumentOutOfRangeException()
	{
		throw CreateArgumentOutOfRangeException();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception CreateArgumentOutOfRangeException()
	{
		return new ArgumentOutOfRangeException();
	}

	internal static void ThrowArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument argument)
	{
		throw CreateArgumentOutOfRangeException(argument);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception CreateArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument argument)
	{
		return new ArgumentOutOfRangeException(argument.ToString());
	}

	internal static void ThrowArgumentOutOfRangeException_PrecisionTooLarge()
	{
		throw CreateArgumentOutOfRangeException_PrecisionTooLarge();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception CreateArgumentOutOfRangeException_PrecisionTooLarge()
	{
		return new ArgumentOutOfRangeException("precision", _003Ce08d9188_002D0e65_002D4efd_002Da7ab_002D1ba7e9ec9e51_003ESR.Format(_003Ce08d9188_002D0e65_002D4efd_002Da7ab_002D1ba7e9ec9e51_003ESR.Argument_PrecisionTooLarge, (byte)99));
	}

	internal static void ThrowArgumentOutOfRangeException_SymbolDoesNotFit()
	{
		throw CreateArgumentOutOfRangeException_SymbolDoesNotFit();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception CreateArgumentOutOfRangeException_SymbolDoesNotFit()
	{
		return new ArgumentOutOfRangeException("symbol", _003Ce08d9188_002D0e65_002D4efd_002Da7ab_002D1ba7e9ec9e51_003ESR.Argument_BadFormatSpecifier);
	}

	internal static void ThrowInvalidOperationException()
	{
		throw CreateInvalidOperationException();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception CreateInvalidOperationException()
	{
		return new InvalidOperationException();
	}

	internal static void ThrowInvalidOperationException_OutstandingReferences()
	{
		throw CreateInvalidOperationException_OutstandingReferences();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception CreateInvalidOperationException_OutstandingReferences()
	{
		return new InvalidOperationException(_003Ce08d9188_002D0e65_002D4efd_002Da7ab_002D1ba7e9ec9e51_003ESR.OutstandingReferences);
	}

	internal static void ThrowInvalidOperationException_UnexpectedSegmentType()
	{
		throw CreateInvalidOperationException_UnexpectedSegmentType();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception CreateInvalidOperationException_UnexpectedSegmentType()
	{
		return new InvalidOperationException(_003Ce08d9188_002D0e65_002D4efd_002Da7ab_002D1ba7e9ec9e51_003ESR.UnexpectedSegmentType);
	}

	internal static void ThrowInvalidOperationException_EndPositionNotReached()
	{
		throw CreateInvalidOperationException_EndPositionNotReached();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception CreateInvalidOperationException_EndPositionNotReached()
	{
		return new InvalidOperationException(_003Ce08d9188_002D0e65_002D4efd_002Da7ab_002D1ba7e9ec9e51_003ESR.EndPositionNotReached);
	}

	internal static void ThrowArgumentOutOfRangeException_PositionOutOfRange()
	{
		throw CreateArgumentOutOfRangeException_PositionOutOfRange();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception CreateArgumentOutOfRangeException_PositionOutOfRange()
	{
		return new ArgumentOutOfRangeException("position");
	}

	internal static void ThrowArgumentOutOfRangeException_OffsetOutOfRange()
	{
		throw CreateArgumentOutOfRangeException_OffsetOutOfRange();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception CreateArgumentOutOfRangeException_OffsetOutOfRange()
	{
		return new ArgumentOutOfRangeException("offset");
	}

	internal static void ThrowObjectDisposedException_ArrayMemoryPoolBuffer()
	{
		throw CreateObjectDisposedException_ArrayMemoryPoolBuffer();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception CreateObjectDisposedException_ArrayMemoryPoolBuffer()
	{
		return new ObjectDisposedException("ArrayMemoryPoolBuffer");
	}

	internal static void ThrowFormatException_BadFormatSpecifier()
	{
		throw CreateFormatException_BadFormatSpecifier();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception CreateFormatException_BadFormatSpecifier()
	{
		return new FormatException(_003Ce08d9188_002D0e65_002D4efd_002Da7ab_002D1ba7e9ec9e51_003ESR.Argument_BadFormatSpecifier);
	}

	internal static void ThrowArgumentException_OverlapAlignmentMismatch()
	{
		throw CreateArgumentException_OverlapAlignmentMismatch();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception CreateArgumentException_OverlapAlignmentMismatch()
	{
		return new ArgumentException(_003Ce08d9188_002D0e65_002D4efd_002Da7ab_002D1ba7e9ec9e51_003ESR.Argument_OverlapAlignmentMismatch);
	}

	internal static void ThrowNotSupportedException()
	{
		throw CreateThrowNotSupportedException();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception CreateThrowNotSupportedException()
	{
		return new NotSupportedException();
	}

	public static bool TryFormatThrowFormatException(out int bytesWritten)
	{
		bytesWritten = 0;
		ThrowFormatException_BadFormatSpecifier();
		return false;
	}

	public static bool TryParseThrowFormatException<T>(out T value, out int bytesConsumed)
	{
		value = default(T);
		bytesConsumed = 0;
		ThrowFormatException_BadFormatSpecifier();
		return false;
	}

	public static void ThrowArgumentValidationException<T>(ReadOnlySequenceSegment<T> startSegment, int startIndex, ReadOnlySequenceSegment<T> endSegment)
	{
		throw CreateArgumentValidationException(startSegment, startIndex, endSegment);
	}

	private static Exception CreateArgumentValidationException<T>(ReadOnlySequenceSegment<T> startSegment, int startIndex, ReadOnlySequenceSegment<T> endSegment)
	{
		if (startSegment == null)
		{
			return CreateArgumentNullException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.startSegment);
		}
		if (endSegment == null)
		{
			return CreateArgumentNullException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.endSegment);
		}
		if (startSegment != endSegment && startSegment.RunningIndex > endSegment.RunningIndex)
		{
			return CreateArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.endSegment);
		}
		if ((uint)startSegment.Memory.Length < (uint)startIndex)
		{
			return CreateArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.startIndex);
		}
		return CreateArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.endIndex);
	}

	public static void ThrowArgumentValidationException(Array array, int start)
	{
		throw CreateArgumentValidationException(array, start);
	}

	private static Exception CreateArgumentValidationException(Array array, int start)
	{
		if (array == null)
		{
			return CreateArgumentNullException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.array);
		}
		if ((uint)start > (uint)array.Length)
		{
			return CreateArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.start);
		}
		return CreateArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.length);
	}

	public static void ThrowStartOrEndArgumentValidationException(long start)
	{
		throw CreateStartOrEndArgumentValidationException(start);
	}

	private static Exception CreateStartOrEndArgumentValidationException(long start)
	{
		if (start < 0)
		{
			return CreateArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.start);
		}
		return CreateArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.length);
	}
}
