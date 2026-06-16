using UnityEngine.Scripting;
using Utf8Json;

namespace Webserver.WebAPI.APIs.GameData;

[Preserve]
public class Mods : AbsRestApi
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly byte[] loadedWebMods;

	public Mods(Web _parent)
	{
		JsonWriter _writer = default(JsonWriter);
		_writer.WriteBeginArray();
		for (int i = 0; i < _parent.WebMods.Count; i++)
		{
			WebMod webMod = _parent.WebMods[i];
			if (i > 0)
			{
				_writer.WriteValueSeparator();
			}
			writeModJson(ref _writer, webMod);
		}
		_writer.WriteEndArray();
		loadedWebMods = _writer.ToUtf8ByteArray();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void writeModJson(ref JsonWriter _writer, WebMod _webMod)
	{
		_writer.WriteBeginObject();
		_writer.WritePropertyName("name");
		_writer.WriteString(_webMod.ParentMod.Name);
		_writer.WriteValueSeparator();
		_writer.WritePropertyName("displayName");
		_writer.WriteString(_webMod.ParentMod.DisplayName);
		_writer.WriteValueSeparator();
		_writer.WritePropertyName("description");
		_writer.WriteString(_webMod.ParentMod.Description);
		_writer.WriteValueSeparator();
		_writer.WritePropertyName("author");
		_writer.WriteString(_webMod.ParentMod.Author);
		_writer.WriteValueSeparator();
		_writer.WritePropertyName("version");
		_writer.WriteString(_webMod.ParentMod.VersionString);
		_writer.WriteValueSeparator();
		_writer.WritePropertyName("website");
		_writer.WriteString(_webMod.ParentMod.Website);
		writeWebModJson(ref _writer, _webMod);
		_writer.WriteEndObject();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void writeWebModJson(ref JsonWriter _writer, WebMod _webMod)
	{
		if (_webMod.ModUrl != null)
		{
			_writer.WriteValueSeparator();
			_writer.WritePropertyName("web");
			_writer.WriteBeginObject();
			_writer.WritePropertyName("baseUrl");
			_writer.WriteString(_webMod.ModUrl);
			string reactBundle = _webMod.ReactBundle;
			if (reactBundle != null)
			{
				_writer.WriteValueSeparator();
				_writer.WritePropertyName("bundle");
				_writer.WriteString(reactBundle);
			}
			string cssPath = _webMod.CssPath;
			if (cssPath != null)
			{
				_writer.WriteValueSeparator();
				_writer.WritePropertyName("css");
				_writer.WriteString(cssPath);
			}
			_writer.WriteEndObject();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestGet(RequestContext _context)
	{
		AbsRestApi.PrepareEnvelopedResult(out var _writer);
		_writer.WriteRaw(loadedWebMods);
		AbsRestApi.SendEnvelopedResult(_context, ref _writer);
	}

	public override int DefaultPermissionLevel()
	{
		return 2000;
	}
}
