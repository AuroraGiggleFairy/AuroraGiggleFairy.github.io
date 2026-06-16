using System;

public sealed class RefCountedBuffer : IDisposable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class SharedBuffer : IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly object m_lockObj = new object();

		[PublicizedFrom(EAccessModifier.Private)]
		public byte[] m_bufferRaw;

		[PublicizedFrom(EAccessModifier.Private)]
		public int m_offset;

		[PublicizedFrom(EAccessModifier.Private)]
		public int m_length;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool m_pooled;

		[PublicizedFrom(EAccessModifier.Private)]
		public int m_refs;

		public bool IsDisposed => m_bufferRaw == null;

		public byte[] BufferRaw => m_bufferRaw;

		public int Offset => m_offset;

		public int Length => m_length;

		public SharedBuffer(byte[] bufferRaw, int offset, int length, bool pooled)
		{
			m_bufferRaw = bufferRaw;
			m_offset = offset;
			m_length = length;
			m_pooled = pooled;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Dispose(bool disposing)
		{
			if (!disposing)
			{
				Log.Error("RefCountedBuffer.SharedBuffer is being finalized. It should be disposed of properly by making use of RefCountedBuffer.");
				return;
			}
			lock (m_lockObj)
			{
				if (m_bufferRaw != null)
				{
					if (m_refs != 0)
					{
						Log.Error(string.Format("{0} expected {1} to be 0 when disposing but was {2}.", "RefCountedBuffer", "m_refs", m_refs));
					}
					if (m_pooled)
					{
						SaveBufferPool.Instance.Free(m_bufferRaw);
					}
					m_bufferRaw = null;
					m_offset = 0;
					m_length = 0;
				}
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		~SharedBuffer()
		{
			Dispose(disposing: false);
		}

		public void Increment()
		{
			lock (m_lockObj)
			{
				if (m_bufferRaw == null)
				{
					throw new ObjectDisposedException("Shared Buffer has already been disposed.");
				}
				m_refs++;
			}
		}

		public void Decrement()
		{
			lock (m_lockObj)
			{
				if (m_bufferRaw == null)
				{
					throw new ObjectDisposedException("Shared Buffer has already been disposed.");
				}
				m_refs--;
				if (m_refs <= 0)
				{
					Dispose();
				}
			}
		}

		public RefCountedBuffer CreateRef()
		{
			return new RefCountedBuffer(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SharedBuffer m_buffer;

	public bool IsDisposed
	{
		get
		{
			if (m_buffer != null)
			{
				return m_buffer.IsDisposed;
			}
			return true;
		}
	}

	public Span<byte> Span
	{
		get
		{
			if (m_buffer?.BufferRaw != null)
			{
				return m_buffer.BufferRaw.AsSpan(m_buffer.Offset, m_buffer.Length);
			}
			return Span<byte>.Empty;
		}
	}

	public Memory<byte> Memory => m_buffer?.BufferRaw?.AsMemory(m_buffer.Offset, m_buffer.Length) ?? Memory<byte>.Empty;

	public byte[] BufferRaw => m_buffer?.BufferRaw ?? Array.Empty<byte>();

	public int Offset => m_buffer?.Offset ?? 0;

	public int Length => m_buffer?.Length ?? 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public RefCountedBuffer(SharedBuffer buffer)
	{
		if (buffer == null || buffer.IsDisposed)
		{
			Dispose();
			return;
		}
		m_buffer = buffer;
		m_buffer.Increment();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Dispose(bool disposing)
	{
		if (!disposing)
		{
			Log.Error("RefCountedBuffer is being finalized. It should be disposed properly.");
		}
		else if (m_buffer != null)
		{
			m_buffer.Decrement();
			m_buffer = null;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~RefCountedBuffer()
	{
		Dispose(disposing: false);
	}

	public RefCountedBuffer CreateRef()
	{
		return m_buffer?.CreateRef();
	}

	public static RefCountedBuffer CreateFromExisting(byte[] buffer)
	{
		return CreateFromExisting(buffer, 0, buffer.Length);
	}

	public static RefCountedBuffer CreateFromExisting(byte[] buffer, int offset, int length)
	{
		return new SharedBuffer(buffer, offset, length, pooled: false).CreateRef();
	}

	public static RefCountedBuffer CreatePooled(int length)
	{
		return new SharedBuffer(SaveBufferPool.Instance.Alloc(length), 0, length, pooled: true).CreateRef();
	}
}
