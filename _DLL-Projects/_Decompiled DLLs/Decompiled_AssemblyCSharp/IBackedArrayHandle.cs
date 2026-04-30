using System;

public interface IBackedArrayHandle : IDisposable
{
	BackedArrayHandleMode Mode { get; }

	void Flush();
}
