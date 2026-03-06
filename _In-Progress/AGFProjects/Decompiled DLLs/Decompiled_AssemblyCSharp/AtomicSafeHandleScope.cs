using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct AtomicSafeHandleScope : IDisposable
{
	public void Dispose()
	{
	}
}
