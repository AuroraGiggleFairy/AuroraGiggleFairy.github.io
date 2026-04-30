using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Utilities;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal static class BufferUtils
{
	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public static char[] RentBuffer([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] IArrayPool<char> bufferPool, int minSize)
	{
		if (bufferPool == null)
		{
			return new char[minSize];
		}
		return bufferPool.Rent(minSize);
	}

	public static void ReturnBuffer(IArrayPool<char> bufferPool, char[] buffer)
	{
		bufferPool?.Return(buffer);
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	public static char[] EnsureBufferSize(IArrayPool<char> bufferPool, int size, char[] buffer)
	{
		if (bufferPool == null)
		{
			return new char[size];
		}
		if (buffer != null)
		{
			bufferPool.Return(buffer);
		}
		return bufferPool.Rent(size);
	}
}
