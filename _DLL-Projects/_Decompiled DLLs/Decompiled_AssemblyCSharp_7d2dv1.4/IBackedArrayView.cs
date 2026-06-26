using System;

public interface IBackedArrayView<T> : IDisposable where T : unmanaged
{
	int Length { get; }

	BackedArrayHandleMode Mode { get; }

	T this[int i] { get; set; }

	void Flush();
}
