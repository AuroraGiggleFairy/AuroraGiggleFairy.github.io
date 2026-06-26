using System;

public class CBCLayer : IMemoryPoolableObject
{
	public byte[] data;

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

	public void InitData(int size)
	{
		if (data == null)
		{
			data = new byte[size];
		}
	}

	public void CopyFrom(CBCLayer _other)
	{
		Array.Copy(_other.data, data, data.Length);
	}
}
