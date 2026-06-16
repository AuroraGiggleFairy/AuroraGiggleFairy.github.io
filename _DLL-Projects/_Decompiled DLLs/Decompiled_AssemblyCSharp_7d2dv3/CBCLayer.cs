using System;

public class CBCLayer : IMemoryPoolableObject
{
	public readonly byte[] data = new byte[1024];

	public static int InstanceCount;

	[PublicizedFrom(EAccessModifier.Protected)]
	~CBCLayer()
	{
	}

	public void Reset()
	{
	}

	public void Cleanup()
	{
	}

	public void CopyFrom(CBCLayer _other)
	{
		Array.Copy(_other.data, data, data.Length);
	}
}
