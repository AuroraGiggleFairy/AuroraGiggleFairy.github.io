using System.IO;
using System.Xml;

public static class SdXmlDocumentExtensions
{
	public static void SdLoad(this XmlDocument xmlDoc, string filename)
	{
		using Stream inStream = SdFile.OpenRead(filename);
		xmlDoc.Load(inStream);
	}

	public static void SdSave(this XmlDocument xmlDoc, string filename)
	{
		using Stream outStream = SdFile.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Read);
		xmlDoc.Save(outStream);
	}
}
