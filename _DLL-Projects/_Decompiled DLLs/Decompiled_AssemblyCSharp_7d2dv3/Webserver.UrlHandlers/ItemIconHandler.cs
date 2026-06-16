using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Profiling;
using UnityEngine;

namespace Webserver.UrlHandlers;

public class ItemIconHandler : AbsHandler
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class LoadingStats
	{
		public int Files;

		public int Tints;

		public readonly MicroStopwatch MswTotal = new MicroStopwatch(_bStart: false);

		public readonly MicroStopwatch MswLoading = new MicroStopwatch(_bStart: false);

		public readonly MicroStopwatch MswEncoding = new MicroStopwatch(_bStart: false);

		public readonly MicroStopwatch MswTinting = new MicroStopwatch(_bStart: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, byte[]> icons = new Dictionary<string, byte[]>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool logMissingFiles;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool loaded;

	[PublicizedFrom(EAccessModifier.Private)]
	public int loadingMaxMsPerFrame = 100;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool loading;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmSourceFile = new ProfilerMarker(".SourceFile");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmAddIcon = new ProfilerMarker(".AddIcon");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmTint = new ProfilerMarker(".Tint");

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static ItemIconHandler Instance
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static ItemIconHandler()
	{
		Instance = null;
	}

	public ItemIconHandler(bool _logMissingFiles, string _moduleName = null)
		: base(_moduleName)
	{
		logMissingFiles = _logMissingFiles;
		Instance = this;
	}

	public override void HandleRequest(RequestContext _context)
	{
		if (!loaded)
		{
			_context.Response.StatusCode = 500;
			Log.Out("[Web] IconHandler: Icons not loaded");
			return;
		}
		if (!_context.RequestPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
		{
			_context.Response.StatusCode = 400;
			return;
		}
		string text = _context.RequestPath.Remove(0, urlBasePath.Length);
		int num = text.LastIndexOf('.');
		if (num < 0)
		{
			_context.Response.StatusCode = 400;
			return;
		}
		text = text.Remove(num);
		if (!icons.TryGetValue(text, out var value))
		{
			_context.Response.StatusCode = 404;
			if (logMissingFiles)
			{
				Log.Out("[Web] IconHandler: FileNotFound: \"" + _context.RequestPath + "\" ");
			}
		}
		else
		{
			_context.Response.ContentType = MimeType.GetMimeType(".png");
			_context.Response.ContentLength64 = value.Length;
			_context.Response.OutputStream.Write(value, 0, value.Length);
		}
	}

	public IEnumerator LoadIcons()
	{
		lock (icons)
		{
			if (loading || loaded)
			{
				yield break;
			}
			loading = true;
			loadingMaxMsPerFrame = (GameManager.IsDedicatedServer ? 100 : 10);
			MicroStopwatch mswPerFrame = new MicroStopwatch(_bStart: true);
			LoadingStats stats = new LoadingStats();
			stats?.MswTotal.Start();
			Dictionary<string, List<Color>> tintedIcons = new Dictionary<string, List<Color>>();
			ItemClass[] list = ItemClass.list;
			foreach (ItemClass itemClass in list)
			{
				if (itemClass == null)
				{
					continue;
				}
				Color iconTint = itemClass.GetIconTint();
				if (!(iconTint == Color.white))
				{
					string iconName = itemClass.GetIconName();
					if (!tintedIcons.TryGetValue(iconName, out var value))
					{
						value = new List<Color>();
						tintedIcons.Add(iconName, value);
					}
					value.Add(iconTint);
				}
			}
			yield return loadIconsFromFolder(GameIO.GetGameDir("Data/ItemIcons"), tintedIcons, stats, mswPerFrame);
			foreach (Mod loadedMod in ModManager.GetLoadedMods())
			{
				string path = loadedMod.Path + "/ItemIcons";
				yield return loadIconsFromFolder(path, tintedIcons, stats, mswPerFrame);
			}
			loaded = true;
			if (stats == null)
			{
				Log.Out($"[Web] IconHandler: Loaded {icons.Count} icons");
				yield break;
			}
			stats.MswTotal.Stop();
			Log.Out($"[Web] IconHandler: Loaded {icons.Count} icons ({stats.Files} source images with {stats.Tints} tints applied)");
			Log.Out($"[Web] IconHandler: Total time {stats.MswTotal.ElapsedMilliseconds} ms, loading files {stats.MswLoading.ElapsedMilliseconds} ms, tinting files {stats.MswTinting.ElapsedMilliseconds} ms, encoding files {stats.MswEncoding.ElapsedMilliseconds} ms");
			int num = 0;
			foreach (KeyValuePair<string, byte[]> icon in icons)
			{
				icon.Deconstruct(out var _, out var value2);
				byte[] array = value2;
				num += array.Length;
			}
			Log.Out($"[Web] IconHandler: Cached {num / 1024} KiB");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator loadIconsFromFolder(string _path, Dictionary<string, List<Color>> _tintedIcons, LoadingStats _stats, MicroStopwatch _mswPerFrame)
	{
		if (!Directory.Exists(_path))
		{
			yield break;
		}
		_mswPerFrame.ResetAndRestart();
		string[] files = Directory.GetFiles(_path);
		foreach (string text in files)
		{
			byte[] array = null;
			Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			try
			{
				using (pmSourceFile.Auto())
				{
					if (text.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
					{
						_stats?.MswLoading.Start();
						array = File.ReadAllBytes(text);
						if (tex.LoadImage(array))
						{
							_stats?.MswLoading.Stop();
							goto IL_012d;
						}
						_stats?.MswLoading.Stop();
					}
				}
			}
			catch (Exception e)
			{
				Log.Error("[Web] Failed loading icon from " + _path);
				Log.Exception(e);
				goto IL_012d;
			}
			continue;
			IL_012d:
			if (tex != null)
			{
				if (tex.width > 1)
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
					yield return AddIcon(fileNameWithoutExtension, array, tex, _tintedIcons, _stats, _mswPerFrame);
				}
				UnityEngine.Object.Destroy(tex);
			}
			if (_mswPerFrame.ElapsedMilliseconds >= loadingMaxMsPerFrame)
			{
				yield return null;
				_mswPerFrame.ResetAndRestart();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator AddIcon(string _name, byte[] _sourceBytes, Texture2D _tex, Dictionary<string, List<Color>> _tintedIcons, LoadingStats _stats, MicroStopwatch _mswPerFrame)
	{
		using (pmAddIcon.Auto())
		{
			_stats?.MswEncoding.Start();
			icons[_name + "__FFFFFF"] = _sourceBytes;
			_stats?.MswEncoding.Stop();
			if (_stats != null)
			{
				_stats.Files++;
			}
			if (!_tintedIcons.TryGetValue(_name, out var value))
			{
				yield break;
			}
			foreach (Color item in value)
			{
				string key = _name + "__" + item.ToHexCode();
				if (icons.ContainsKey(key))
				{
					continue;
				}
				using (pmTint.Auto())
				{
					Texture2D texture2D = new Texture2D(_tex.width, _tex.height, TextureFormat.ARGB32, mipChain: false);
					_stats?.MswTinting.Start();
					TextureUtils.ApplyTint(_tex, texture2D, item);
					_stats?.MswTinting.Stop();
					_stats?.MswEncoding.Start();
					icons[key] = texture2D.EncodeToPNG();
					_stats?.MswEncoding.Stop();
					UnityEngine.Object.Destroy(texture2D);
					if (_stats != null)
					{
						_stats.Tints++;
					}
					if (_mswPerFrame.ElapsedMilliseconds >= loadingMaxMsPerFrame)
					{
						yield return null;
						_mswPerFrame.ResetAndRestart();
					}
				}
			}
		}
	}
}
