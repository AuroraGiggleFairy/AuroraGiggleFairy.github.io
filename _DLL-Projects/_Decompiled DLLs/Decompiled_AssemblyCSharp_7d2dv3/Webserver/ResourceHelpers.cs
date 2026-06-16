using System;
using System.IO;
using System.Reflection;

namespace Webserver;

public static class ResourceHelpers
{
	public static Stream OpenManifestResource(Assembly _assembly, string _name, bool _ignoreCase = false)
	{
		string[] manifestResourceNames = _assembly.GetManifestResourceNames();
		foreach (string text in manifestResourceNames)
		{
			if (text.EndsWith(_name, _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
			{
				return _assembly.GetManifestResourceStream(text);
			}
		}
		return null;
	}

	public static string GetManifestResourceText(Assembly _assembly, string _name, bool _ignoreCase = false)
	{
		using Stream stream = OpenManifestResource(_assembly, _name, _ignoreCase);
		if (stream == null)
		{
			return null;
		}
		using TextReader textReader = new StreamReader(stream);
		return textReader.ReadToEnd();
	}
}
