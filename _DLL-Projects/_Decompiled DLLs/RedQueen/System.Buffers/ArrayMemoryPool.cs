using System.Runtime.CompilerServices;

namespace System.Buffers;

internal sealed class ArrayMemoryPool<T> : MemoryPool<T>
{
	private sealed class ArrayMemoryPoolBuffer : IMemoryOwner<T>, IDisposable
	{
		private T[] _array;

		public Memory<T> Memory
		{
			get
			{
				T[] array = _array;
				if (array == null)
				{
					_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowObjectDisposedException_ArrayMemoryPoolBuffer();
				}
				return new Memory<T>(array);
			}
		}

		public ArrayMemoryPoolBuffer(int size)
		{
			_array = ArrayPool<T>.Shared.Rent(size);
		}

		public void Dispose()
		{
			T[] array = _array;
			if (array != null)
			{
				_array = null;
				ArrayPool<T>.Shared.Return(array);
			}
		}
	}

	private const int s_maxBufferSize = int.MaxValue;

	public sealed override int MaxBufferSize => int.MaxValue;

	public sealed override IMemoryOwner<T> Rent(int minimumBufferSize = -1)
	{
		if (minimumBufferSize == -1)
		{
			minimumBufferSize = 1 + 4095 / Unsafe.SizeOf<T>();
		}
		else if ((uint)minimumBufferSize > 2147483647u)
		{
			_003C61ca5adb_002De494_002D47ea_002D8b14_002D064e034f5f0d_003EThrowHelper.ThrowArgumentOutOfRangeException(_003C654e2d23_002D2563_002D49b3_002D9aca_002D67e4d79feeb7_003EExceptionArgument.minimumBufferSize);
		}
		return new ArrayMemoryPoolBuffer(minimumBufferSize);
	}

	protected sealed override void Dispose(bool disposing)
	{
	}
}
