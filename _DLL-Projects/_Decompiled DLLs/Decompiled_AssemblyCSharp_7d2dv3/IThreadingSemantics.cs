using System;

public interface IThreadingSemantics
{
	void Synchronize(AtomicActionDelegate _delegate);

	T Synchronize<T>(Func<T> _delegate);

	int InterlockedAdd(ref int _number, int _add);
}
