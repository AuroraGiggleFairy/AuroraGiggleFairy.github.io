using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;

public unsafe struct UnsafeBitArraySetIndicesEnumerator(UnsafeBitArray bitArray) : IEnumerator<int>, IEnumerator, IDisposable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public UnsafeBitArray bitArray = bitArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe ulong currentSlice = *bitArray.Ptr;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sliceIndex = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public int numSlices = bitArray.Length / 64;

	[PublicizedFrom(EAccessModifier.Private)]
	public int leadingZeroCount = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int numSetBits = bitArray.CountBits(0, bitArray.Length);

	[PublicizedFrom(EAccessModifier.Private)]
	public int foundBits = 0;

	public int Current => currentIndex;

	object IEnumerator.Current
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return Current;
		}
	}

	public unsafe bool MoveNext()
	{
		while (foundBits < numSetBits && sliceIndex < numSlices)
		{
			if (currentSlice == 0L)
			{
				sliceIndex++;
				if (sliceIndex < numSlices)
				{
					currentSlice = bitArray.Ptr[sliceIndex];
					leadingZeroCount = 0;
				}
				continue;
			}
			if ((currentSlice & 1) != 0L)
			{
				currentIndex = leadingZeroCount + sliceIndex * 64;
				leadingZeroCount++;
				currentSlice >>= 1;
				foundBits++;
				return true;
			}
			if ((currentSlice & 0xFFFFFFFFu) == 0L)
			{
				leadingZeroCount += 32;
				currentSlice >>= 32;
			}
			if ((currentSlice & 0xFFFF) == 0L)
			{
				leadingZeroCount += 16;
				currentSlice >>= 16;
			}
			if ((currentSlice & 0xFF) == 0L)
			{
				leadingZeroCount += 8;
				currentSlice >>= 8;
			}
			if ((currentSlice & 0xF) == 0L)
			{
				leadingZeroCount += 4;
				currentSlice >>= 4;
			}
			if ((currentSlice & 3) == 0L)
			{
				leadingZeroCount += 2;
				currentSlice >>= 2;
			}
			if ((currentSlice & 1) == 0L)
			{
				leadingZeroCount++;
				currentSlice >>= 1;
			}
		}
		return false;
	}

	public unsafe void Reset()
	{
		sliceIndex = 0;
		currentSlice = *bitArray.Ptr;
		currentIndex = -1;
		leadingZeroCount = 0;
		foundBits = 0;
	}

	public void Dispose()
	{
	}
}
