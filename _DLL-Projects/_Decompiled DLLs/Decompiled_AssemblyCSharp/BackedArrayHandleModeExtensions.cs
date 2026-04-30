using System;

public static class BackedArrayHandleModeExtensions
{
	public static bool CanRead(this BackedArrayHandleMode mode)
	{
		return mode switch
		{
			BackedArrayHandleMode.ReadOnly => true, 
			BackedArrayHandleMode.ReadWrite => true, 
			_ => throw new ArgumentOutOfRangeException("mode", mode, $"Unknown mode: {mode}"), 
		};
	}

	public static bool CanWrite(this BackedArrayHandleMode mode)
	{
		return mode switch
		{
			BackedArrayHandleMode.ReadOnly => false, 
			BackedArrayHandleMode.ReadWrite => true, 
			_ => throw new ArgumentOutOfRangeException("mode", mode, $"Unknown mode: {mode}"), 
		};
	}
}
