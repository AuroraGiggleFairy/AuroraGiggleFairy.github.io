using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ProfilerScope : IDisposable
{
	public ProfilerScope(string name)
	{
	}

	public void Dispose()
	{
	}
}
