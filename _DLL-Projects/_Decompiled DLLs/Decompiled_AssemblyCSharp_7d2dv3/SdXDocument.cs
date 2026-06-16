using System.IO;
using System.Xml.Linq;

public static class SdXDocument
{
	public static XDocument Load(string filename)
	{
		using Stream stream = SdFile.OpenRead(filename);
		return XDocument.Load(stream);
	}
}
