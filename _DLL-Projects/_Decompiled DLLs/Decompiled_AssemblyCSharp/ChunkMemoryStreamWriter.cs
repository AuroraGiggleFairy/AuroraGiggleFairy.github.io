using System.IO;

[PublicizedFrom(EAccessModifier.Internal)]
public class ChunkMemoryStreamWriter : MemoryStream
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string dir;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ext;

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileAccessMultipleChunks regionFileAccess;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] buffer;

	public ChunkMemoryStreamWriter()
		: this(new byte[512000])
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkMemoryStreamWriter(byte[] _buffer)
		: base(_buffer)
	{
		buffer = _buffer;
	}

	public void Init(RegionFileAccessMultipleChunks _regionFileAccess, string _dir, int _x, int _z, string _ext)
	{
		dir = _dir;
		chunkX = _x;
		chunkZ = _z;
		ext = _ext;
		regionFileAccess = _regionFileAccess;
		Seek(0L, SeekOrigin.Begin);
	}

	public override void Close()
	{
		regionFileAccess.Write(dir, chunkX, chunkZ, ext, buffer, (int)Position);
	}
}
