using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static class SpanUtils
{
	public static int GetHashCode<T>(ReadOnlySpan<T> span) where T : unmanaged
	{
		return GetHashCodeInternal(MemoryMarshal.Cast<T, byte>(span));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int GetHashCodeInternal(ReadOnlySpan<byte> span)
	{
		int num = span.Length / 4 * 4;
		ReadOnlySpan<int> readOnlySpan = MemoryMarshal.Cast<byte, int>(span.Slice(0, num));
		int num2 = 1009;
		ReadOnlySpan<int> readOnlySpan2 = readOnlySpan;
		for (int i = 0; i < readOnlySpan2.Length; i++)
		{
			int num3 = readOnlySpan2[i];
			num2 = num2 * 9176 + num3;
		}
		if (num == span.Length)
		{
			return num2;
		}
		int num4 = span.Length - num;
		return num2 * 9176 + num4 switch
		{
			1 => span[num], 
			2 => span[num] | (span[num + 1] << 8), 
			3 => span[num] | (span[num + 1] << 8) | (span[num + 2] << 16), 
			_ => throw new InvalidOperationException($"Remainder should be 1, 2 or 3, but was: {num4}"), 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int ConcatAccLength(int length, ref int totalLength)
	{
		totalLength += length;
		return length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe static void ConcatCopyThenSlice(ref Span<char> dest, (IntPtr ptr, int len) src)
	{
		new ReadOnlySpan<char>(src.ptr.ToPointer(), src.len).CopyTo(dest);
		Span<char> span = dest;
		int item = src.len;
		dest = span.Slice(item, span.Length - item);
	}

	public unsafe static string Concat(StringSpan s0, StringSpan s1)
	{
		ReadOnlySpan<char> readOnlySpan = s0.AsSpan();
		fixed (char* ptr = readOnlySpan)
		{
			void* ptr2 = ptr;
			readOnlySpan = s1.AsSpan();
			fixed (char* ptr3 = readOnlySpan)
			{
				void* ptr4 = ptr3;
				int totalLength = 0;
				return string.Create(state: (((IntPtr)ptr2, ConcatAccLength(s0.Length, ref totalLength)), ((IntPtr)ptr4, ConcatAccLength(s1.Length, ref totalLength))), length: totalLength, action: [PublicizedFrom(EAccessModifier.Internal)] (Span<char> span, ((IntPtr, int) d0, (IntPtr, int) d1) data) =>
				{
					ConcatCopyThenSlice(ref span, data.d0);
					ConcatCopyThenSlice(ref span, data.d1);
				});
			}
		}
	}

	public unsafe static string Concat(StringSpan s0, StringSpan s1, StringSpan s2)
	{
		ReadOnlySpan<char> readOnlySpan = s0.AsSpan();
		fixed (char* ptr = readOnlySpan)
		{
			void* ptr2 = ptr;
			readOnlySpan = s1.AsSpan();
			fixed (char* ptr3 = readOnlySpan)
			{
				void* ptr4 = ptr3;
				readOnlySpan = s2.AsSpan();
				fixed (char* ptr5 = readOnlySpan)
				{
					void* ptr6 = ptr5;
					int totalLength = 0;
					return string.Create(state: (((IntPtr)ptr2, ConcatAccLength(s0.Length, ref totalLength)), ((IntPtr)ptr4, ConcatAccLength(s1.Length, ref totalLength)), ((IntPtr)ptr6, ConcatAccLength(s2.Length, ref totalLength))), length: totalLength, action: [PublicizedFrom(EAccessModifier.Internal)] (Span<char> span, ((IntPtr, int) d0, (IntPtr, int) d1, (IntPtr, int) d2) data) =>
					{
						ConcatCopyThenSlice(ref span, data.d0);
						ConcatCopyThenSlice(ref span, data.d1);
						ConcatCopyThenSlice(ref span, data.d2);
					});
				}
			}
		}
	}

	public unsafe static string Concat(StringSpan s0, StringSpan s1, StringSpan s2, StringSpan s3)
	{
		ReadOnlySpan<char> readOnlySpan = s0.AsSpan();
		fixed (char* ptr = readOnlySpan)
		{
			void* ptr2 = ptr;
			readOnlySpan = s1.AsSpan();
			fixed (char* ptr3 = readOnlySpan)
			{
				void* ptr4 = ptr3;
				readOnlySpan = s2.AsSpan();
				fixed (char* ptr5 = readOnlySpan)
				{
					void* ptr6 = ptr5;
					readOnlySpan = s3.AsSpan();
					fixed (char* ptr7 = readOnlySpan)
					{
						void* ptr8 = ptr7;
						int totalLength = 0;
						return string.Create(state: (((IntPtr)ptr2, ConcatAccLength(s0.Length, ref totalLength)), ((IntPtr)ptr4, ConcatAccLength(s1.Length, ref totalLength)), ((IntPtr)ptr6, ConcatAccLength(s2.Length, ref totalLength)), ((IntPtr)ptr8, ConcatAccLength(s3.Length, ref totalLength))), length: totalLength, action: [PublicizedFrom(EAccessModifier.Internal)] (Span<char> span, ((IntPtr, int) d0, (IntPtr, int) d1, (IntPtr, int) d2, (IntPtr, int) d3) data) =>
						{
							ConcatCopyThenSlice(ref span, data.d0);
							ConcatCopyThenSlice(ref span, data.d1);
							ConcatCopyThenSlice(ref span, data.d2);
							ConcatCopyThenSlice(ref span, data.d3);
						});
					}
				}
			}
		}
	}

	public unsafe static string Concat(StringSpan s0, StringSpan s1, StringSpan s2, StringSpan s3, StringSpan s4)
	{
		ReadOnlySpan<char> readOnlySpan = s0.AsSpan();
		fixed (char* ptr = readOnlySpan)
		{
			void* ptr2 = ptr;
			readOnlySpan = s1.AsSpan();
			fixed (char* ptr3 = readOnlySpan)
			{
				void* ptr4 = ptr3;
				readOnlySpan = s2.AsSpan();
				fixed (char* ptr5 = readOnlySpan)
				{
					void* ptr6 = ptr5;
					readOnlySpan = s3.AsSpan();
					fixed (char* ptr7 = readOnlySpan)
					{
						void* ptr8 = ptr7;
						readOnlySpan = s4.AsSpan();
						fixed (char* ptr9 = readOnlySpan)
						{
							void* ptr10 = ptr9;
							int totalLength = 0;
							return string.Create(state: (((IntPtr)ptr2, ConcatAccLength(s0.Length, ref totalLength)), ((IntPtr)ptr4, ConcatAccLength(s1.Length, ref totalLength)), ((IntPtr)ptr6, ConcatAccLength(s2.Length, ref totalLength)), ((IntPtr)ptr8, ConcatAccLength(s3.Length, ref totalLength)), ((IntPtr)ptr10, ConcatAccLength(s4.Length, ref totalLength))), length: totalLength, action: [PublicizedFrom(EAccessModifier.Internal)] (Span<char> span, ((IntPtr, int) d0, (IntPtr, int) d1, (IntPtr, int) d2, (IntPtr, int) d3, (IntPtr, int) d4) data) =>
							{
								ConcatCopyThenSlice(ref span, data.d0);
								ConcatCopyThenSlice(ref span, data.d1);
								ConcatCopyThenSlice(ref span, data.d2);
								ConcatCopyThenSlice(ref span, data.d3);
								ConcatCopyThenSlice(ref span, data.d4);
							});
						}
					}
				}
			}
		}
	}
}
