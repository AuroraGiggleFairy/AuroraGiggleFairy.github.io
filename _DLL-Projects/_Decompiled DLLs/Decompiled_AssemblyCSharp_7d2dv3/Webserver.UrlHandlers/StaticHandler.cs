using System.IO;
using Webserver.FileCache;

namespace Webserver.UrlHandlers;

public class StaticHandler : AbsHandler
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly AbstractCache cache;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string datapath;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool logMissingFiles;

	public StaticHandler(string _filePath, AbstractCache _cache, bool _logMissingFiles, string _moduleName = null)
		: base(_moduleName)
	{
		datapath = _filePath + ((_filePath[_filePath.Length - 1] == '/') ? "" : "/");
		cache = _cache;
		logMissingFiles = _logMissingFiles;
	}

	public override void HandleRequest(RequestContext _context)
	{
		string text = _context.RequestPath.Remove(0, urlBasePath.Length);
		byte[] fileContent = cache.GetFileContent(datapath + text);
		if (fileContent != null)
		{
			_context.Response.ContentType = MimeType.GetMimeType(Path.GetExtension(text));
			_context.Response.ContentLength64 = fileContent.Length;
			_context.Response.OutputStream.Write(fileContent, 0, fileContent.Length);
			return;
		}
		_context.Response.StatusCode = 404;
		if (logMissingFiles)
		{
			Log.Warning("[Web] Static: FileNotFound: \"" + _context.RequestPath + "\" @ \"" + datapath + text + "\"");
		}
	}
}
