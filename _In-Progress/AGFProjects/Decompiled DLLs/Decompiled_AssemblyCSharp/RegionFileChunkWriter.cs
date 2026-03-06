using System.IO;
using Noemax.GZip;

public class RegionFileChunkWriter
{
	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileAccessAbstract regionFileAccess;

	[PublicizedFrom(EAccessModifier.Private)]
	public Stream innerSaveStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public DeflateOutputStream zipSaveStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] saveBuffer = new byte[4096];

	public RegionFileChunkWriter(RegionFileAccessAbstract regionFileAccess)
	{
		this.regionFileAccess = regionFileAccess;
	}

	public void WriteStreamCompressed(string dir, int chunkX, int chunkZ, string ext, MemoryStream memoryStream)
	{
		Stream outputStream = regionFileAccess.GetOutputStream(dir, chunkX, chunkZ, ext);
		long v = StreamUtils.ReadInt64(memoryStream);
		StreamUtils.Write(outputStream, v);
		if (zipSaveStream == null || innerSaveStream != outputStream)
		{
			if (zipSaveStream != null)
			{
				Log.Warning("RFM.Save: Creating new DeflateStream, underlying Stream changed");
			}
			zipSaveStream = new DeflateOutputStream(outputStream, 3, leaveOpen: true);
			innerSaveStream = outputStream;
		}
		StreamUtils.StreamCopy(memoryStream, zipSaveStream, saveBuffer);
		zipSaveStream.Restart();
		outputStream.Close();
	}
}
