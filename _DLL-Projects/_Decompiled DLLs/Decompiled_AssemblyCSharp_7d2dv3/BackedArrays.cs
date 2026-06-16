using Platform;

public static class BackedArrays
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const DeviceFlag ENABLE_FILE_BACKED_ARRAYS_PLATFORMS = DeviceFlag.XBoxSeriesS;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly bool ENABLE_FILE_BACKED_ARRAYS;

	[PublicizedFrom(EAccessModifier.Private)]
	static BackedArrays()
	{
		ENABLE_FILE_BACKED_ARRAYS = PlatformOptimizations.FileBackedArrays;
		Log.Out(string.Format("Initial {0} == {1}", "ENABLE_FILE_BACKED_ARRAYS", ENABLE_FILE_BACKED_ARRAYS));
	}

	public static IBackedArray<T> Create<T>(int length) where T : unmanaged
	{
		if (ENABLE_FILE_BACKED_ARRAYS && length > 0)
		{
			return new FileBackedArray<T>(length);
		}
		return new MemoryBackedArray<T>(length);
	}

	public static IBackedArrayView<T> CreateSingleView<T>(IBackedArray<T> array, BackedArrayHandleMode mode, int viewLength = 0) where T : unmanaged
	{
		if (array is MemoryBackedArray<T> array2)
		{
			return new MemoryBackedArray<T>.MemoryBackedArrayView(array2, mode);
		}
		return new BackedArraySingleView<T>(array, mode, viewLength);
	}
}
