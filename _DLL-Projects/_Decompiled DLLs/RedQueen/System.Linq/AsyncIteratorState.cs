namespace System.Linq;

internal enum AsyncIteratorState
{
	New = 0,
	Allocated = 1,
	Iterating = 2,
	Disposed = -1
}
