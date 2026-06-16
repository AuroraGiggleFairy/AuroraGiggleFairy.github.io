using System.Collections.Generic;

public class PackagesSentInfoEntry
{
	public List<NetPackageInfo> packages;

	public int count;

	public long uncompressedSize;

	public long compressedSize;

	public bool bCompressed;

	public float timestamp;

	public string client;
}
