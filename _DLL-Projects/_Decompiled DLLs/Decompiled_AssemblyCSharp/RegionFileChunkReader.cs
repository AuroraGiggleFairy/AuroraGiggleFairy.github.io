using System;
using System.IO;
using Noemax.GZip;

public class RegionFileChunkReader
{
	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileAccessAbstract regionFileAccess;

	[PublicizedFrom(EAccessModifier.Private)]
	public Stream innerLoadStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public DeflateInputStream zipLoadStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream loadChunkMemoryStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledBinaryReader loadChunkReader;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] loadBuffer = new byte[4096];

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] magicBytes = new byte[4];

	public RegionFileChunkReader(RegionFileAccessAbstract regionFileAccess)
	{
		this.regionFileAccess = regionFileAccess;
		loadChunkMemoryStream = new MemoryStream(65536);
		loadChunkReader = new PooledBinaryReader();
		loadChunkReader.SetBaseStream(loadChunkMemoryStream);
	}

	public PooledBinaryReader readIntoLoadStream(string _dir, int chunkX, int chunkZ, string ext, out uint version)
	{
		Stream inputStream = regionFileAccess.GetInputStream(_dir, chunkX, chunkZ, ext);
		if (inputStream == null)
		{
			version = 0u;
			return null;
		}
		using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false))
		{
			pooledBinaryReader.SetBaseStream(inputStream);
			for (int i = 0; i < magicBytes.Length; i++)
			{
				magicBytes[i] = pooledBinaryReader.ReadByte();
			}
			if (magicBytes[0] != 116 || magicBytes[1] != 116 || magicBytes[2] != 99 || magicBytes[3] != 0)
			{
				throw new Exception("Wrong chunk header!");
			}
			version = pooledBinaryReader.ReadUInt32();
		}
		if (zipLoadStream == null || innerLoadStream != inputStream)
		{
			if (zipLoadStream != null)
			{
				Log.Warning("RFM.Load: Creating new DeflateStream, underlying Stream changed");
			}
			zipLoadStream = new DeflateInputStream(inputStream, leaveOpen: true);
			innerLoadStream = inputStream;
		}
		zipLoadStream.Restart();
		DeflateInputStream source = zipLoadStream;
		loadChunkMemoryStream.SetLength(0L);
		StreamUtils.StreamCopy(source, loadChunkMemoryStream, loadBuffer);
		loadChunkMemoryStream.Position = 0L;
		inputStream.Close();
		return loadChunkReader;
	}

	public void WriteBackup(string dir, int chunkX, int chunkZ)
	{
		using (Stream destination = SdFile.OpenWrite(dir + "/error_backup_" + chunkX + "_" + chunkZ + ".comp.bak"))
		{
			innerLoadStream.Position = 0L;
			StreamUtils.StreamCopy(innerLoadStream, destination);
		}
		using Stream destination2 = SdFile.OpenWrite(dir + "/error_backup_" + chunkX + "_" + chunkZ + ".uncomp.bak");
		loadChunkMemoryStream.Position = 0L;
		StreamUtils.StreamCopy(loadChunkMemoryStream, destination2);
	}
}
