using System.IO;
using System.Xml.Linq;

public static class SdXDocumentExtensions
{
	public static void SdSave(this XDocument xmlDoc, string filename)
	{
		using Stream stream = SdFile.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Read);
		xmlDoc.Save(stream);
	}
}
