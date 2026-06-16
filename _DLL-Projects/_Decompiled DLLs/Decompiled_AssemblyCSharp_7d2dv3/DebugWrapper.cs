using System;
using System.Collections;
using System.Collections.Generic;

public abstract class DebugWrapper
{
	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class DummyScope : IDisposable
	{
		public static readonly DummyScope Instance = new DummyScope();

		[PublicizedFrom(EAccessModifier.Private)]
		public DummyScope()
		{
		}

		public void Dispose()
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class ReadScope : IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DebugWrapper m_wrapper;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool m_concurrentModificationNotified;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_disposed;

		public ReadScope(DebugWrapper wrapper)
		{
			m_wrapper = wrapper;
			if (m_wrapper.DebugConcurrentModifications && m_wrapper.m_writeCounter.Value > 0)
			{
				Log.Exception(new DebugWrapperException("A read is being issued while there are active writers."));
				m_concurrentModificationNotified = true;
			}
			m_wrapper.m_readCounter.Increment();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		~ReadScope()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Dispose(bool disposing)
		{
			if (!m_disposed)
			{
				m_disposed = true;
				if (!disposing)
				{
					Log.Error("DebugWrapper.ReadScope was not disposed of correctly.");
				}
				m_wrapper.m_readCounter.Decrement();
				if (!m_concurrentModificationNotified && m_wrapper.DebugConcurrentModifications && m_wrapper.m_writeCounter.Value > 0)
				{
					Log.Exception(new DebugWrapperException("A read was issued while there were active writers."));
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class ReadWriteScope : IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DebugWrapper m_wrapper;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool m_concurrentModificationNotified;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_disposed;

		public ReadWriteScope(DebugWrapper wrapper)
		{
			m_wrapper = wrapper;
			if (m_wrapper.DebugConcurrentModifications && m_wrapper.m_writeCounter.Value > 0)
			{
				if (m_wrapper.m_writeCounter.Value > 0)
				{
					Log.Exception(new DebugWrapperException("A read/write is being issued while there are active writers."));
				}
				else if (m_wrapper.m_readCounter.Value > 0)
				{
					Log.Exception(new DebugWrapperException("A read/write is being issued while there are active readers."));
				}
				m_concurrentModificationNotified = true;
			}
			m_wrapper.m_readCounter.Increment();
			m_wrapper.m_writeCounter.Increment();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		~ReadWriteScope()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Dispose(bool disposing)
		{
			if (m_disposed)
			{
				return;
			}
			m_disposed = true;
			if (!disposing)
			{
				Log.Error("DebugWrapper.ReadWriteScope was not disposed of correctly.");
			}
			m_wrapper.m_writeCounter.Decrement();
			m_wrapper.m_readCounter.Decrement();
			if (!m_concurrentModificationNotified && m_wrapper.DebugConcurrentModifications)
			{
				if (m_wrapper.m_writeCounter.Value > 0)
				{
					Log.Exception(new DebugWrapperException("A read/write was issued while there were active writers."));
				}
				else if (m_wrapper.m_readCounter.Value > 0)
				{
					Log.Exception(new DebugWrapperException("A read/write was issued while there are active readers."));
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class Enumerator : IEnumerator, IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly IDisposable m_scope;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly IEnumerator m_enumerator;

		public object Current => m_enumerator.Current;

		public Enumerator(IDisposable scope, IEnumerator enumerator)
		{
			m_scope = scope;
			m_enumerator = enumerator;
		}

		public void Dispose()
		{
			m_scope.Dispose();
			(m_enumerator as IDisposable)?.Dispose();
		}

		public bool MoveNext()
		{
			return m_enumerator.MoveNext();
		}

		public void Reset()
		{
			m_enumerator.Reset();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class Enumerator<T> : IEnumerator<T>, IEnumerator, IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly IDisposable m_scope;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly IEnumerator<T> m_enumerator;

		public T Current => m_enumerator.Current;

		object IEnumerator.Current
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return ((IEnumerator)m_enumerator).Current;
			}
		}

		public Enumerator(IDisposable scope, IEnumerator<T> enumerator)
		{
			m_scope = scope;
			m_enumerator = enumerator;
		}

		public void Dispose()
		{
			m_scope.Dispose();
			m_enumerator.Dispose();
		}

		public bool MoveNext()
		{
			return m_enumerator.MoveNext();
		}

		public void Reset()
		{
			m_enumerator.Reset();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly AtomicCounter m_readCounter;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly AtomicCounter m_writeCounter;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool DebugNonMainThreadAccess { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool DebugConcurrentModifications { get; set; }

	public bool NeedsScope
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return DebugConcurrentModifications;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public DebugWrapper(DebugWrapper parent)
	{
		m_readCounter = parent?.m_readCounter ?? new AtomicCounter();
		m_writeCounter = parent?.m_writeCounter ?? new AtomicCounter();
		DebugNonMainThreadAccess = parent?.DebugNonMainThreadAccess ?? false;
		DebugConcurrentModifications = parent?.DebugConcurrentModifications ?? false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IDisposable DebugReadScope()
	{
		if (DebugNonMainThreadAccess && !ThreadManager.IsMainThread())
		{
			Log.Exception(new DebugWrapperException("A read is being issued outside of the main thread."));
		}
		if (!NeedsScope)
		{
			return DummyScope.Instance;
		}
		return new ReadScope(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IDisposable DebugReadWriteScope()
	{
		if (DebugNonMainThreadAccess && !ThreadManager.IsMainThread())
		{
			Log.Exception(new DebugWrapperException("A read/write is being issued outside of the main thread."));
		}
		if (!NeedsScope)
		{
			return DummyScope.Instance;
		}
		return new ReadWriteScope(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator<T> DebugEnumerator<T>(IEnumerator<T> enumerator)
	{
		return new Enumerator<T>(DebugReadScope(), enumerator);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator DebugEnumerator(IEnumerator enumerator)
	{
		return new Enumerator(DebugReadScope(), enumerator);
	}
}
