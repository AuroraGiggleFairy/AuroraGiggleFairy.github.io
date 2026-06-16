using System;
using System.IO;

public class PooledExpandableMemoryStream : MemoryStream, IMemoryPoolableObject, IDisposable
{
	public override void Close()
	{
	}

	public void Reset()
	{
		SetLength(0L);
	}

	public void Cleanup()
	{
		Reset();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Dispose(bool _disposing)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	void IDisposable.Dispose()
	{
		MemoryPools.poolMemoryStream.FreeSync(this);
	}
}
