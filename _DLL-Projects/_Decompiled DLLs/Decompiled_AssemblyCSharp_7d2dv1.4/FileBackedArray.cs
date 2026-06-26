using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Platform;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public sealed class FileBackedArray<T> : IBackedArray<T>, IDisposable where T : unmanaged
{
	[PublicizedFrom(EAccessModifier.Private)]
	public delegate void OnWrittenHandler(int start, ReadOnlySpan<T> span);

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class FileBackedArrayMemoryManager : MemoryManager<T>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe T* m_ptr;

		[PublicizedFrom(EAccessModifier.Private)]
		public int m_length;

		[PublicizedFrom(EAccessModifier.Private)]
		public int m_valueSize;

		public unsafe FileBackedArrayMemoryManager(T* ptr, int length, int valueSize)
		{
			m_ptr = ptr;
			m_length = length;
			m_valueSize = valueSize;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public unsafe override void Dispose(bool disposing)
		{
			m_ptr = null;
			m_length = 0;
			m_valueSize = 0;
		}

		public unsafe override Span<T> GetSpan()
		{
			return new Span<T>(m_ptr, m_length);
		}

		public unsafe override MemoryHandle Pin(int elementIndex = 0)
		{
			return new MemoryHandle(m_ptr + elementIndex * m_valueSize);
		}

		public override void Unpin()
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class FileBackedArrayHandle : IBackedArrayHandle, IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public FileBackedArray<T> m_array;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int m_valueSize;

		[PublicizedFrom(EAccessModifier.Private)]
		public int m_start;

		[PublicizedFrom(EAccessModifier.Private)]
		public int m_length;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BackedArrayHandleMode m_mode;

		[PublicizedFrom(EAccessModifier.Private)]
		public NativeArray<T> m_buffer;

		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe T* m_ptr;

		[PublicizedFrom(EAccessModifier.Private)]
		public IMemoryOwner<T> m_memoryOwner;

		public BackedArrayHandleMode Mode => m_mode;

		public unsafe FileBackedArrayHandle(FileBackedArray<T> array, int valueSize, int start, int length, BackedArrayHandleMode mode)
		{
			m_array = array;
			m_valueSize = valueSize;
			m_start = start;
			m_length = length;
			m_mode = mode;
			m_buffer = new NativeArray<T>(m_length, Allocator.Persistent);
			m_ptr = m_mode switch
			{
				BackedArrayHandleMode.ReadOnly => (T*)m_buffer.GetUnsafeReadOnlyPtr(), 
				BackedArrayHandleMode.ReadWrite => (T*)m_buffer.GetUnsafePtr(), 
				_ => throw new ArgumentOutOfRangeException("mode", mode, $"Unknown mode: {mode}"), 
			};
			byte* ptr = (byte*)m_ptr;
			int num = m_length * m_valueSize;
			int i = 0;
			int num2 = m_start * m_valueSize;
			FileStream fileStream = m_array.GetFileStream();
			fileStream.Seek(num2, SeekOrigin.Begin);
			int num3;
			for (; i < num; i += num3)
			{
				num3 = fileStream.Read(new Span<byte>(ptr + i, num - i));
				if (num3 <= 0)
				{
					m_buffer.Dispose();
					m_buffer = default(NativeArray<T>);
					throw new IOException($"Unexpected end of file (read {i} but expected {num} after offset {num2}).");
				}
			}
			FileBackedArray<T> array2 = m_array;
			array2.OnWritten = (OnWrittenHandler)Delegate.Combine(array2.OnWritten, new OnWrittenHandler(OnWritten));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe void Dispose(bool disposing)
		{
			if (!disposing)
			{
				Log.Error("FileBackedArrayHandle is being finalized, it should be disposed properly.");
				return;
			}
			if (m_array != null)
			{
				FileBackedArray<T> array = m_array;
				array.OnWritten = (OnWrittenHandler)Delegate.Remove(array.OnWritten, new OnWrittenHandler(OnWritten));
				if (m_mode.CanWrite())
				{
					try
					{
						FlushInternal();
					}
					catch (Exception e)
					{
						Log.Error("Failed to write potential changes back to the FileBackedArray file stream.");
						Log.Exception(e);
					}
				}
			}
			if (m_memoryOwner != null)
			{
				m_memoryOwner.Dispose();
				m_memoryOwner = null;
			}
			if (m_buffer != default(NativeArray<T>))
			{
				m_buffer.Dispose();
				m_buffer = default(NativeArray<T>);
			}
			m_start = 0;
			m_length = 0;
			m_ptr = null;
			m_array = null;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		~FileBackedArrayHandle()
		{
			Dispose(disposing: false);
		}

		[Conditional("DEVELOPMENT_BUILD")]
		[Conditional("UNITY_EDITOR")]
		[PublicizedFrom(EAccessModifier.Private)]
		public void ThrowIfDisposed()
		{
			if (IsDisposed())
			{
				throw new ObjectDisposedException("FileBackedArrayHandle has already been disposed.");
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool IsDisposed()
		{
			return m_array == null;
		}

		[Conditional("DEVELOPMENT_BUILD")]
		[Conditional("UNITY_EDITOR")]
		[PublicizedFrom(EAccessModifier.Private)]
		public void ThrowIfCannotWrite()
		{
			if (!m_mode.CanWrite())
			{
				throw new NotSupportedException("This FileBackedArrayHandle is not writable.");
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnWritten(int start, ReadOnlySpan<T> span)
		{
			int num = Math.Max(start, m_start);
			int num2 = Math.Min(start + span.Length, m_start + m_length) - num;
			if (num2 > 0)
			{
				ReadOnlySpan<T> readOnlySpan = span.Slice(num - start, num2);
				Span<T> destination = GetSpan().Slice(num - m_start, num2);
				readOnlySpan.CopyTo(destination);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe void FlushInternal()
		{
			FileStream fileStream = m_array.GetFileStream();
			ReadOnlySpan<T> span = new ReadOnlySpan<T>(m_ptr, m_length);
			fileStream.Seek(m_start * m_valueSize, SeekOrigin.Begin);
			fileStream.Write(MemoryMarshal.Cast<T, byte>(span));
			fileStream.Flush();
			m_array.OnWritten?.Invoke(m_start, span);
		}

		public void Flush()
		{
			FlushInternal();
		}

		public unsafe Memory<T> GetMemory()
		{
			if (m_memoryOwner == null)
			{
				m_memoryOwner = new FileBackedArrayMemoryManager(m_ptr, m_length, m_valueSize);
			}
			return m_memoryOwner.Memory;
		}

		public unsafe ReadOnlyMemory<T> GetReadOnlyMemory()
		{
			if (m_memoryOwner == null)
			{
				m_memoryOwner = new FileBackedArrayMemoryManager(m_ptr, m_length, m_valueSize);
			}
			return m_memoryOwner.Memory;
		}

		public unsafe T* GetPtr()
		{
			return m_ptr;
		}

		public unsafe Span<T> GetSpan()
		{
			return new Span<T>(m_ptr, m_length);
		}

		public unsafe ReadOnlySpan<T> GetReadOnlySpan()
		{
			return new ReadOnlySpan<T>(m_ptr, m_length);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int FILE_STREAM_BUFFER_SIZE = 4096;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int m_length;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int m_valueSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string m_filePath;

	[PublicizedFrom(EAccessModifier.Private)]
	public FileStream m_fileStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_fileStreamsLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadLocal<FileStream> m_fileStreams;

	public int Length => m_length;

	public OnWrittenHandler OnWritten
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public FileBackedArray(int length)
	{
		if (length <= 0)
		{
			throw new ArgumentOutOfRangeException("length", length, "Length should be positive.");
		}
		m_length = length;
		m_valueSize = UnsafeUtility.SizeOf(typeof(T));
		m_filePath = PlatformManager.NativePlatform.Utils.GetTempFileName("fba", ".fba");
		m_fileStream = new FileStream(m_filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.DeleteOnClose);
		m_fileStream.Seek(length * m_valueSize - 1, SeekOrigin.Begin);
		m_fileStream.WriteByte(0);
		m_fileStream.Flush();
		m_fileStreams = new ThreadLocal<FileStream>(CreateFileStream, trackAllValues: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Dispose(bool disposing)
	{
		if (!disposing)
		{
			Log.Error("FileBackedArray<T> is being finalized, it should be disposed properly.");
			return;
		}
		lock (m_fileStreamsLock)
		{
			if (m_fileStreams != null)
			{
				foreach (FileStream value in m_fileStreams.Values)
				{
					value.Dispose();
				}
				m_fileStreams.Dispose();
				m_fileStreams = null;
			}
			if (m_fileStream != null)
			{
				m_fileStream.Dispose();
				m_fileStream = null;
			}
		}
		try
		{
			File.Delete(m_filePath);
		}
		catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
		{
			Log.Warning("FileBackedArray<T> Failed to delete: " + m_filePath);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~FileBackedArray()
	{
		Dispose(disposing: false);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ThrowIfDisposed()
	{
		if (m_fileStream == null)
		{
			throw new ObjectDisposedException("FileBackedArray has already been disposed.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckBounds(int start, int length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException($"Expected length to be non-negative but was {length}.");
		}
		if (start < 0 || start + length > m_length)
		{
			throw new ArgumentOutOfRangeException($"Expected requested range [{start}, {start + length}) to be a subset of [0, {m_length}).");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FileStream CreateFileStream()
	{
		lock (m_fileStreamsLock)
		{
			return new FileStream(m_filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete, 4096);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FileStream GetFileStream()
	{
		lock (m_fileStreamsLock)
		{
			return m_fileStreams.Value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FileBackedArrayHandle GetHandle(int start, int length, BackedArrayHandleMode mode)
	{
		CheckBounds(start, length);
		return new FileBackedArrayHandle(this, m_valueSize, start, length, mode);
	}

	public IBackedArrayHandle GetMemory(int start, int length, out Memory<T> memory)
	{
		FileBackedArrayHandle handle = GetHandle(start, length, BackedArrayHandleMode.ReadWrite);
		memory = handle.GetMemory();
		return handle;
	}

	public IBackedArrayHandle GetReadOnlyMemory(int start, int length, out ReadOnlyMemory<T> memory)
	{
		FileBackedArrayHandle handle = GetHandle(start, length, BackedArrayHandleMode.ReadOnly);
		memory = handle.GetReadOnlyMemory();
		return handle;
	}

	public unsafe IBackedArrayHandle GetMemoryUnsafe(int start, int length, out T* arrayPtr)
	{
		FileBackedArrayHandle handle = GetHandle(start, length, BackedArrayHandleMode.ReadWrite);
		arrayPtr = handle.GetPtr();
		return handle;
	}

	public unsafe IBackedArrayHandle GetReadOnlyMemoryUnsafe(int start, int length, out T* arrayPtr)
	{
		FileBackedArrayHandle handle = GetHandle(start, length, BackedArrayHandleMode.ReadOnly);
		arrayPtr = handle.GetPtr();
		return handle;
	}

	public IBackedArrayHandle GetSpan(int start, int length, out Span<T> span)
	{
		FileBackedArrayHandle handle = GetHandle(start, length, BackedArrayHandleMode.ReadWrite);
		span = handle.GetSpan();
		return handle;
	}

	public IBackedArrayHandle GetReadOnlySpan(int start, int length, out ReadOnlySpan<T> span)
	{
		FileBackedArrayHandle handle = GetHandle(start, length, BackedArrayHandleMode.ReadOnly);
		span = handle.GetReadOnlySpan();
		return handle;
	}
}
