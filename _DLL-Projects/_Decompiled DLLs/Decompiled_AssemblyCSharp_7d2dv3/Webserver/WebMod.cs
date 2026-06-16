using System.IO;
using Webserver.FileCache;
using Webserver.UrlHandlers;

namespace Webserver;

public class WebMod
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string modsBaseUrl = "/webmods/";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string reactBundleName = "bundle.js";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string stylingFileName = "styling.css";

	public readonly Mod ParentMod;

	public readonly string ModUrl;

	public readonly string ReactBundle;

	public readonly string CssPath;

	public readonly bool IsWebMod;

	public WebMod(Web _parentWeb, Mod _parentMod, bool _useStaticCache)
	{
		ParentMod = _parentMod;
		string text = _parentMod.Path + "/WebMod";
		IsWebMod = Directory.Exists(text);
		if (IsWebMod)
		{
			ModUrl = "/webmods/" + _parentMod.Name + "/";
			ReactBundle = text + "/bundle.js";
			ReactBundle = (File.Exists(ReactBundle) ? (ModUrl + "bundle.js") : null);
			CssPath = text + "/styling.css";
			CssPath = (File.Exists(CssPath) ? (ModUrl + "styling.css") : null);
			_parentWeb.RegisterPathHandler(ModUrl, new StaticHandler(text, _useStaticCache ? ((AbstractCache)new SimpleCache()) : ((AbstractCache)new DirectAccess()), _logMissingFiles: false));
		}
	}
}
