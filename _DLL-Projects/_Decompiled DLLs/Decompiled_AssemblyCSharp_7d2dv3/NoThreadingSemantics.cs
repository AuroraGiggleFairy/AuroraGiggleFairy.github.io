using System;

public class NoThreadingSemantics : IThreadingSemantics
{
	public void Synchronize(AtomicActionDelegate _delegate)
	{
		_delegate();
	}

	public T Synchronize<T>(Func<T> _delegate)
	{
		return _delegate();
	}

	public int InterlockedAdd(ref int _number, int _add)
	{
		_number += _add;
		return _number;
	}
}
