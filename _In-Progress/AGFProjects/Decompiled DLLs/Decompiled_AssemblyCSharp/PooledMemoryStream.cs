using System.IO;

public class PooledMemoryStream : MemoryStream, IMemoryPoolableObject
{
	public static int InstanceCount;

	[PublicizedFrom(EAccessModifier.Protected)]
	~PooledMemoryStream()
	{
	}

	public void Reset()
	{
		SetLength(0L);
	}

	public void Cleanup()
	{
	}
}
