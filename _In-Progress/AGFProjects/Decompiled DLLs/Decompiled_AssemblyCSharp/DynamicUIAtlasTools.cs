using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

public static class DynamicUIAtlasTools
{
	public static void Prebake(int _elementWidth, int _elementHeight, int _paddingSize, Dictionary<string, Texture2D> _textures, string _outputfile)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		int count = _textures.Count;
		Vector2i vector2i = findMinimumAtlasSize(_elementWidth, _elementHeight, _paddingSize, count, new Vector2i(1, 1));
		int num = vector2i.x / (_elementWidth + _paddingSize);
		_ = vector2i.y / (_elementHeight + _paddingSize);
		Texture2D texture2D = new Texture2D(vector2i.x, vector2i.y, TextureFormat.ARGB32, mipChain: false);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("{0}\t{1}\t{2}\n", _elementWidth, _elementHeight, _paddingSize);
		texture2D.FillTexture(new Color(0f, 0f, 0f, 0f));
		int num2 = 0;
		foreach (KeyValuePair<string, Texture2D> _texture in _textures)
		{
			if (_texture.Value.width != _elementWidth || _texture.Value.height != _elementHeight)
			{
				Log.Warning($"Sprite {_texture.Key} has incorrect resolution {_texture.Value.width}*{_texture.Value.height}, expected {_elementWidth}*{_elementHeight}");
			}
			else
			{
				int num3 = num2 / num;
				int num4 = num2 % num * (_elementWidth + _paddingSize);
				int num5 = num3 * (_elementHeight + _paddingSize);
				_texture.Value.CopyTexturePart(Vector2i.zero, texture2D, new Vector2i(num4, vector2i.y - (_elementHeight + _paddingSize) - num5), new Vector2i(_elementWidth, _elementHeight));
				stringBuilder.AppendFormat("{0}\t{1}\t{2}\n", _texture.Key, num4, num5);
				num2++;
			}
		}
		SdFile.WriteAllText(_outputfile + ".txt", stringBuilder.ToString());
		SdFile.WriteAllBytes(_outputfile + ".png", texture2D.EncodeToPNG());
		stopwatch.Stop();
		Log.Out("Creating UIAtlas (" + vector2i.x + "*" + vector2i.y + ") took " + stopwatch.ElapsedMilliseconds + " ms");
	}

	public static void AddSprites(int _elementWidth, int _elementHeight, int _paddingSize, Dictionary<string, Texture2D> _textures, ref Texture2D _tex, List<UISpriteData> _spriteList)
	{
		int count = _spriteList.Count;
		Vector2i vector2i = new Vector2i(_tex.width, _tex.height);
		int num = vector2i.x / (_elementWidth + _paddingSize);
		_ = vector2i.y / (_elementHeight + _paddingSize);
		int num2 = ((count > 0) ? ((count - 1) / num + 1) : 0);
		int num3 = count % num;
		Dictionary<string, UISpriteData> dictionary = new Dictionary<string, UISpriteData>();
		for (int i = 0; i < _spriteList.Count; i++)
		{
			UISpriteData uISpriteData = _spriteList[i];
			dictionary.Add(uISpriteData.name, uISpriteData);
		}
		int num4 = count;
		foreach (KeyValuePair<string, Texture2D> _texture in _textures)
		{
			if (!dictionary.ContainsKey(_texture.Key))
			{
				num4++;
			}
		}
		Vector2i vector2i2 = findMinimumAtlasSize(_elementWidth, _elementHeight, _paddingSize, num4, vector2i);
		int num5 = vector2i2.x / (_elementWidth + _paddingSize);
		_ = vector2i2.y / (_elementHeight + _paddingSize);
		if (vector2i2 != vector2i)
		{
			Texture2D texture2D = new Texture2D(vector2i2.x, vector2i2.y, _tex.format, mipChain: false);
			texture2D.FillTexture(new Color(0f, 0f, 0f, 0f));
			_tex.CopyTexturePart(Vector2i.zero, texture2D, new Vector2i(0, vector2i2.y - vector2i.y), vector2i);
			Object.Destroy(_tex);
			_tex = texture2D;
		}
		else
		{
			Log.Out("Atlas got enough free room to fit icons. Old {0} icons, {1}x{2}, now {3} icons", count, vector2i.x, vector2i.y, num4);
		}
		int num6 = 0;
		int num7 = ((num2 > 1) ? num : num3);
		foreach (KeyValuePair<string, Texture2D> _texture2 in _textures)
		{
			if (_texture2.Value.width != _elementWidth || _texture2.Value.height != _elementHeight)
			{
				Log.Warning($"Sprite {_texture2.Key} has incorrect resolution {_texture2.Value.width}*{_texture2.Value.height}, expected {_elementWidth}*{_elementHeight}");
				continue;
			}
			int num8 = -1;
			int num9 = -1;
			if (dictionary.ContainsKey(_texture2.Key))
			{
				num8 = dictionary[_texture2.Key].x;
				num9 = dictionary[_texture2.Key].y;
			}
			else
			{
				while (num7 >= num5)
				{
					num6++;
					num7 = ((num6 + 1 >= num2) ? ((num6 + 1 == num2) ? num3 : 0) : num);
				}
				num8 = num7 * (_elementWidth + _paddingSize);
				num9 = num6 * (_elementHeight + _paddingSize);
				UISpriteData uISpriteData2 = new UISpriteData();
				uISpriteData2.name = _texture2.Key;
				uISpriteData2.SetRect(num8, num9, _elementWidth, _elementHeight);
				dictionary.Add(_texture2.Key, uISpriteData2);
				num7++;
			}
			_texture2.Value.CopyTexturePart(Vector2i.zero, _tex, new Vector2i(num8, vector2i2.y - (_elementHeight + _paddingSize) - num9), new Vector2i(_elementWidth, _elementHeight));
		}
		_spriteList.Clear();
		foreach (KeyValuePair<string, UISpriteData> item in dictionary)
		{
			_spriteList.Add(item.Value);
		}
	}

	public static int GetMaxElementsWithSize(int _atlasWidthMax, int _atlasHeightMax, int _elementWidth, int _elementHeight, int _paddingSize)
	{
		int num = _atlasWidthMax / (_elementWidth + _paddingSize);
		int num2 = _atlasHeightMax / (_elementHeight + _paddingSize);
		return num * num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2i findMinimumAtlasSize(int _elementWidth, int _elementHeight, int _paddingSize, int _elementCount, Vector2i _startSize)
	{
		Vector2i result = _startSize;
		_elementHeight += _paddingSize;
		_elementWidth += _paddingSize;
		int num = result.x / _elementWidth;
		int num2 = result.y / _elementHeight;
		while (num * num2 < _elementCount)
		{
			if (result.x <= result.y)
			{
				result.x *= 2;
			}
			else
			{
				result.y *= 2;
			}
			num = result.x / _elementWidth;
			num2 = result.y / _elementHeight;
		}
		return result;
	}

	public static bool ReadPrebakedAtlasDescriptor(string _resourceName, out List<UISpriteData> _sprites, out int _elementWidth, out int _elementHeight, out int _paddingSize)
	{
		DataLoader.DataPathIdentifier dataPathIdentifier = DataLoader.ParseDataPathIdentifier(_resourceName + ".txt");
		TextAsset textAsset = DataLoader.LoadAsset<TextAsset>(dataPathIdentifier);
		_sprites = null;
		_elementWidth = 0;
		_elementHeight = 0;
		_paddingSize = 1;
		if (textAsset == null)
		{
			Log.Error("Could not open prebaked atlas from {0} (missing sprite list)", _resourceName);
			return false;
		}
		_sprites = new List<UISpriteData>();
		try
		{
			using StringReader stringReader = new StringReader(textAsset.text);
			int num = 1;
			string text = stringReader.ReadLine();
			if (text != null)
			{
				string[] array = text.Split('\t');
				if (array.Length < 2)
				{
					Log.Error("Prebaked atlas from {0}: Invalid descriptor at line 1: {1}", _resourceName, text);
					return false;
				}
				if (!int.TryParse(array[0], out _elementWidth))
				{
					Log.Error("Prebaked atlas from {0}: Invalid descriptor value at line 1 parameter 1: {1}", _resourceName, text);
					return false;
				}
				if (!int.TryParse(array[1], out _elementHeight))
				{
					Log.Error("Prebaked atlas from {0}: Invalid descriptor value at line 1 parameter 2: {1}", _resourceName, text);
					return false;
				}
				if (array.Length > 2 && !int.TryParse(array[2], out _paddingSize))
				{
					Log.Error("Prebaked atlas from {0}: Invalid descriptor value at line 1 parameter 3: {1}", _resourceName, text);
					return false;
				}
			}
			while ((text = stringReader.ReadLine()) != null)
			{
				num++;
				string[] array = text.Split('\t');
				if (array.Length == 3)
				{
					string name = array[0];
					if (!int.TryParse(array[1], out var result))
					{
						Log.Error("Prebaked atlas from {0}: Invalid descriptor value at line {2} parameter 2: {1}", _resourceName, text, num);
						return false;
					}
					if (!int.TryParse(array[2], out var result2))
					{
						Log.Error("Prebaked atlas from {0}: Invalid descriptor value at line {2} parameter 3: {1}", _resourceName, text, num);
						return false;
					}
					UISpriteData uISpriteData = new UISpriteData();
					uISpriteData.name = name;
					uISpriteData.SetRect(result, result2, _elementWidth, _elementHeight);
					_sprites.Add(uISpriteData);
					continue;
				}
				Log.Error("Prebaked atlas from {0}: Invalid descriptor at line {2}: {1}", _resourceName, text, num);
				return false;
			}
		}
		finally
		{
			DataLoader.UnloadAsset(dataPathIdentifier, textAsset);
		}
		return true;
	}

	public static bool ReadPrebakedAtlasTexture(string _resourceName, out Texture2D _tex)
	{
		_tex = DataLoader.LoadAsset<Texture2D>(_resourceName + ".png");
		if (_tex == null)
		{
			Log.Error("Could not open prebaked atlas from {0} (missing texture)", _resourceName);
			return false;
		}
		return true;
	}

	public static void UnloadTex(string _resourceName, Texture2D _tex)
	{
		DataLoader.UnloadAsset(_resourceName, _tex);
	}
}
