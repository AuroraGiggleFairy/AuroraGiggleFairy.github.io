using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Webserver.UrlHandlers;

namespace Webserver.WebAPI;

public class OpenApiHelpers
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct OpenApiSpec(string _spec, Dictionary<string, string> _exportedPaths = null)
	{
		public readonly Dictionary<string, string> ExportedPaths = _exportedPaths;

		public readonly string Spec = _spec;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string apiSpecResourcesFolder = "Data/Webserver/OpenApiSpecs";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string masterResourceName = "openapi.master.yaml";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string masterDocName = "openapi.yaml";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, OpenApiSpec> specs = new CaseInsensitiveStringDictionary<OpenApiSpec>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex pathMatcher = new Regex("^\\s{1,2}(/\\S+):.*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

	public OpenApiHelpers()
	{
		loadMainSpec();
		Web.ServerInitialized += [PublicizedFrom(EAccessModifier.Private)] (Web _) =>
		{
			buildMainSpecRefs();
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void loadMainSpec()
	{
		Assembly assembly = GetType().Assembly;
		string text = loadSpecFileForAssembly(assembly, "openapi.master.yaml");
		if (text == null)
		{
			Log.Warning($"[Web] Failed loading main OpenAPI spec from assembly '{assembly}'");
		}
		else
		{
			specs.Add("openapi.yaml", new OpenApiSpec(text));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void buildMainSpecRefs()
	{
		if (!TryGetOpenApiSpec(null, out var _specText))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder(_specText);
		foreach (KeyValuePair<string, OpenApiSpec> spec in specs)
		{
			spec.Deconstruct(out var key, out var value);
			string text = key;
			OpenApiSpec openApiSpec = value;
			if (text.Equals("openapi.yaml") || openApiSpec.ExportedPaths == null || openApiSpec.ExportedPaths.Count < 1)
			{
				continue;
			}
			foreach (KeyValuePair<string, string> exportedPath2 in openApiSpec.ExportedPaths)
			{
				exportedPath2.Deconstruct(out key, out var value2);
				string exportedPath = key;
				string rebasedPath = value2;
				writePath(stringBuilder, text, exportedPath, rebasedPath);
			}
		}
		specs["openapi.yaml"] = new OpenApiSpec(stringBuilder.ToString());
		Log.Out("[Web] OpenAPI preparation done");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writePath(StringBuilder _sb, string _apiSpecName, string _exportedPath, string _rebasedPath)
	{
		_sb.AppendLine("  " + (_rebasedPath ?? _exportedPath) + ":");
		_sb.Append("    $ref: './" + _apiSpecName + "#/paths/");
		writeJsonPointerEncodedPath(_sb, _exportedPath);
		_sb.AppendLine("'");
	}

	public void LoadOpenApiSpec(AbsWebAPI _api)
	{
		loadOpenApiSpec(_api.GetType().Assembly, _api.Name, null);
	}

	public void LoadOpenApiSpec(AbsHandler _pathHandler)
	{
		Type type = _pathHandler.GetType();
		loadOpenApiSpec(type.Assembly, type.Name, _pathHandler.UrlBasePath);
	}

	public void RegisterCustomSpec(Assembly _assembly, string _apiSpecName, string _replaceBasePath = null)
	{
		loadOpenApiSpec(_assembly, _apiSpecName, _replaceBasePath);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void loadOpenApiSpec(Assembly _containingAssembly, string _apiName, string _basePath)
	{
		string text = _apiName + ".openapi.yaml";
		string text2 = loadSpecFileForAssembly(_containingAssembly, text);
		if (text2 != null)
		{
			OpenApiSpec value = new OpenApiSpec(text2, findExportedPaths(text2, _basePath));
			specs.Add(text, value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string loadSpecFileForAssembly(Assembly _containingAssembly, string _specFileName)
	{
		Assembly assembly = typeof(GameManager).Assembly;
		if (_containingAssembly != assembly)
		{
			return ResourceHelpers.GetManifestResourceText(_containingAssembly, _specFileName, _ignoreCase: true);
		}
		return ((TextAsset)Resources.Load("Data/Webserver/OpenApiSpecs/" + Path.GetFileNameWithoutExtension(_specFileName)))?.text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, string> findExportedPaths(string _spec, string _replaceBasePath = null)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		using TextReader textReader = new StringReader(_spec);
		bool flag = false;
		string text;
		while ((text = textReader.ReadLine()) != null)
		{
			if (!flag)
			{
				if (text.StartsWith("paths:"))
				{
					flag = true;
				}
				continue;
			}
			Match match = pathMatcher.Match(text);
			if (match.Success)
			{
				string value = match.Groups[1].Value;
				string value2 = null;
				if (_replaceBasePath != null)
				{
					value2 = value.Replace("/BASEPATH/", _replaceBasePath);
				}
				dictionary[value] = value2;
			}
		}
		return dictionary;
	}

	public bool TryGetOpenApiSpec(string _name, out string _specText)
	{
		if (string.IsNullOrEmpty(_name))
		{
			_name = "openapi.yaml";
		}
		if (!specs.TryGetValue(_name, out var value))
		{
			_specText = null;
			return false;
		}
		_specText = value.Spec;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeJsonPointerEncodedPath(StringBuilder _targetSb, string _path)
	{
		foreach (char c in _path)
		{
			switch (c)
			{
			case '"':
				_targetSb.Append("\\\"");
				break;
			case '\\':
				_targetSb.Append("\\\\");
				break;
			case '\b':
				_targetSb.Append("\\b");
				break;
			case '\f':
				_targetSb.Append("\\f");
				break;
			case '\n':
				_targetSb.Append("\\n");
				break;
			case '\r':
				_targetSb.Append("\\r");
				break;
			case '\t':
				_targetSb.Append("\\t");
				break;
			case '\0':
				_targetSb.Append("\\u0000");
				break;
			case '\u0001':
				_targetSb.Append("\\u0001");
				break;
			case '\u0002':
				_targetSb.Append("\\u0002");
				break;
			case '\u0003':
				_targetSb.Append("\\u0003");
				break;
			case '\u0004':
				_targetSb.Append("\\u0004");
				break;
			case '\u0005':
				_targetSb.Append("\\u0005");
				break;
			case '\u0006':
				_targetSb.Append("\\u0006");
				break;
			case '\a':
				_targetSb.Append("\\u0007");
				break;
			case '\v':
				_targetSb.Append("\\u000b");
				break;
			case '\u000e':
				_targetSb.Append("\\u000e");
				break;
			case '\u000f':
				_targetSb.Append("\\u000f");
				break;
			case '\u0010':
				_targetSb.Append("\\u0010");
				break;
			case '\u0011':
				_targetSb.Append("\\u0011");
				break;
			case '\u0012':
				_targetSb.Append("\\u0012");
				break;
			case '\u0013':
				_targetSb.Append("\\u0013");
				break;
			case '\u0014':
				_targetSb.Append("\\u0014");
				break;
			case '\u0015':
				_targetSb.Append("\\u0015");
				break;
			case '\u0016':
				_targetSb.Append("\\u0016");
				break;
			case '\u0017':
				_targetSb.Append("\\u0017");
				break;
			case '\u0018':
				_targetSb.Append("\\u0018");
				break;
			case '\u0019':
				_targetSb.Append("\\u0019");
				break;
			case '\u001a':
				_targetSb.Append("\\u001a");
				break;
			case '\u001b':
				_targetSb.Append("\\u001b");
				break;
			case '\u001c':
				_targetSb.Append("\\u001c");
				break;
			case '\u001d':
				_targetSb.Append("\\u001d");
				break;
			case '\u001e':
				_targetSb.Append("\\u001e");
				break;
			case '\u001f':
				_targetSb.Append("\\u001f");
				break;
			case '/':
				_targetSb.Append("~1");
				break;
			case '~':
				_targetSb.Append("~0");
				break;
			default:
				_targetSb.Append(c);
				break;
			}
		}
	}
}
