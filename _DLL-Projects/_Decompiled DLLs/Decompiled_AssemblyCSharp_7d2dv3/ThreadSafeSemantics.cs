using System;
using System.Threading;

public class ThreadSafeSemantics : IThreadingSemantics
{
	[PublicizedFrom(EAccessModifier.Private)]
	public object monitor;

	public ThreadSafeSemantics()
	{
		monitor = this;
	}

	public ThreadSafeSemantics(object _monitor)
	{
		monitor = _monitor;
	}

	public void Synchronize(AtomicActionDelegate _delegate)
	{
		lock (monitor)
		{
			_delegate();
		}
	}

	public int InterlockedAdd(ref int _number, int _add)
	{
		return Interlocked.Add(ref _number, _add);
	}

	public T Synchronize<T>(Func<T> _delegate)
	{
		lock (monitor)
		{
			return _delegate();
		}
	}
}
