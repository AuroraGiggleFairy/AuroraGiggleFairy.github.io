using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class UIAtlasFromFolder
{
	public static IEnumerator CreateUiAtlasFromFolder(string _folder, Shader _shader, Action<UIAtlas> _resultHandler)
	{
		MicroStopwatch coRoutineSw = new MicroStopwatch(_bStart: true);
		string[] files = null;
		try
		{
			files = SdDirectory.GetFiles(_folder);
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		if (files == null)
		{
			yield break;
		}
		Dictionary<string, Texture2D> icons = new Dictionary<string, Texture2D>();
		foreach (string text in files)
		{
			try
			{
				if (text.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || text.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
				{
					Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
					texture2D.name = "ModMgrAtlas";
					if (texture2D.LoadImage(SdFile.ReadAllBytes(text)))
					{
						string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
						icons[fileNameWithoutExtension] = texture2D;
					}
					else
					{
						UnityEngine.Object.Destroy(texture2D);
					}
				}
				else if (text.EndsWith(".tga", StringComparison.OrdinalIgnoreCase))
				{
					Texture2D texture2D2 = TGALoader.LoadTGA(text, mipMaps: false);
					texture2D2.name = "ModMgrAtlas";
					string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(text);
					icons[fileNameWithoutExtension2] = texture2D2;
				}
			}
			catch (Exception e2)
			{
				Log.Error("Adding file " + text + " failed:");
				Log.Exception(e2);
			}
			if (coRoutineSw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				coRoutineSw.ResetAndRestart();
			}
		}
		if (icons.Count == 0)
		{
			yield break;
		}
		Dictionary<string, UISpriteData> customSpriteSettings = loadSpriteSettings(_folder + "/settings.xml");
		UIAtlas obj = createUiAtlasFromTextures(Path.GetFileName(_folder), icons, customSpriteSettings, _shader, _folder + "/..");
		try
		{
			foreach (KeyValuePair<string, Texture2D> item in icons)
			{
				UnityEngine.Object.Destroy(item.Value);
			}
		}
		catch (Exception e3)
		{
			Log.Exception(e3);
		}
		_resultHandler(obj);
	}

	public static UIAtlas createUiAtlasFromTextures(string _name, Dictionary<string, Texture2D> _textures, Dictionary<string, UISpriteData> _customSpriteSettings, Shader _shader, string _dumpFolder = null, bool _applyAndUnreadable = true)
	{
		string[] array = new string[_textures.Count];
		Texture2D[] array2 = new Texture2D[_textures.Count];
		int num = 0;
		foreach (KeyValuePair<string, Texture2D> _texture in _textures)
		{
			array[num] = _texture.Key;
			array2[num] = _texture.Value;
			num++;
		}
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
		Rect[] array3 = texture2D.PackTextures(array2, 2, 8192);
		if (!string.IsNullOrEmpty(_dumpFolder) && GameUtils.GetLaunchArgument("exportcustomatlases") != null)
		{
			SdFile.WriteAllBytes(_dumpFolder + "/" + _name + "_atlas.png", texture2D.EncodeToPNG());
		}
		if (_applyAndUnreadable)
		{
			texture2D.Compress(highQuality: true);
			texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		}
		Log.Out($"UIAtlas {_name}: Pack took {microStopwatch.ElapsedMicroseconds} us");
		int width = texture2D.width;
		int height = texture2D.height;
		List<UISpriteData> list = new List<UISpriteData>(_textures.Count);
		for (int i = 0; i < _textures.Count; i++)
		{
			UISpriteData uISpriteData = new UISpriteData
			{
				name = array[i]
			};
			Rect rect = NGUIMath.ConvertToPixels(array3[i], width, height, round: true);
			uISpriteData.SetRect((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
			if (_customSpriteSettings != null && _customSpriteSettings.TryGetValue(array[i], out var value))
			{
				uISpriteData.borderTop = value.borderTop;
				uISpriteData.borderBottom = value.borderBottom;
				uISpriteData.borderLeft = value.borderLeft;
				uISpriteData.borderRight = value.borderRight;
			}
			list.Add(uISpriteData);
		}
		UIAtlas uIAtlas = new GameObject(_name).AddComponent<UIAtlas>();
		uIAtlas.spriteList = list;
		uIAtlas.spriteMaterial = new Material(_shader);
		uIAtlas.pixelSize = 1f;
		uIAtlas.spriteMaterial.mainTexture = texture2D;
		return uIAtlas;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, UISpriteData> loadSpriteSettings(string _filename)
	{
		if (!SdFile.Exists(_filename))
		{
			return null;
		}
		Dictionary<string, UISpriteData> dictionary = new CaseInsensitiveStringDictionary<UISpriteData>();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.SdLoad(_filename);
		foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
		{
			if (childNode.NodeType != XmlNodeType.Element || !childNode.Name.EqualsCaseInsensitive("sprite"))
			{
				continue;
			}
			XmlElement xmlElement = (XmlElement)childNode;
			if (xmlElement.HasAttribute("name"))
			{
				string attribute = xmlElement.GetAttribute("name");
				UISpriteData uISpriteData = new UISpriteData();
				if (xmlElement.HasAttribute("borderleft"))
				{
					uISpriteData.borderLeft = int.Parse(xmlElement.GetAttribute("borderleft"));
				}
				if (xmlElement.HasAttribute("borderright"))
				{
					uISpriteData.borderRight = int.Parse(xmlElement.GetAttribute("borderright"));
				}
				if (xmlElement.HasAttribute("bordertop"))
				{
					uISpriteData.borderTop = int.Parse(xmlElement.GetAttribute("bordertop"));
				}
				if (xmlElement.HasAttribute("borderbottom"))
				{
					uISpriteData.borderBottom = int.Parse(xmlElement.GetAttribute("borderbottom"));
				}
				dictionary.Add(attribute, uISpriteData);
			}
		}
		return dictionary;
	}
}
