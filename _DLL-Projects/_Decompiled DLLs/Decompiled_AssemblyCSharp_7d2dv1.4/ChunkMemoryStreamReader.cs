using System.IO;

[PublicizedFrom(EAccessModifier.Internal)]
public class ChunkMemoryStreamReader : MemoryStream
{
	public ChunkMemoryStreamReader()
		: this(new byte[512000])
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkMemoryStreamReader(byte[] _buffer)
		: base(_buffer)
	{
	}

	public override void Close()
	{
		Seek(0L, SeekOrigin.Begin);
	}
}
