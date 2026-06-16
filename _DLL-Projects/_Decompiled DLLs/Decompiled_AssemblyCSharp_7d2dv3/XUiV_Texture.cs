using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class XUiV_Texture : XUiV_TextureBased
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] textureUris = Array.Empty<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool autoUnload;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isExternalTexture;

	public override Texture Texture
	{
		set
		{
			if (AutoUnload && base.Texture != null)
			{
				UnloadTexture();
			}
			base.Texture = value;
			if (value == null)
			{
				isExternalTexture = false;
			}
		}
	}

	[XuiXmlAttribute("textures", false)]
	public string[] TextureUris
	{
		get
		{
			return textureUris;
		}
		set
		{
			if (value == null || value.Length == 0)
			{
				textureUris = Array.Empty<string>();
				Texture = null;
				SetDirty();
			}
			else
			{
				textureUris = value;
				loadTexture();
			}
		}
	}

	[XuiXmlAttribute("texture", false)]
	public string Path
	{
		get
		{
			return textureUris[0];
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				TextureUris = null;
			}
			else if (TextureUris.Length != 1 || !(TextureUris[0] == value))
			{
				TextureUris = new string[1] { value };
			}
		}
	}

	[XuiXmlAttribute("autounload", false)]
	public bool AutoUnload
	{
		get
		{
			return autoUnload;
		}
		set
		{
			if (autoUnload != value)
			{
				autoUnload = value;
			}
		}
	}

	public XUiV_Texture(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void loadTexture(int _index = 0)
	{
		if (_index >= TextureUris.Length)
		{
			return;
		}
		string text = TextureUris[_index];
		try
		{
			if (ModManager.TryPatchModPathString(text, out var _modPath))
			{
				FetchWwwTexture(_index, "file://" + _modPath);
			}
			else if (text[0] == '@' && text[1] != ':')
			{
				string text2 = text.Substring(1);
				if (text2.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
				{
					string text3 = text2.Substring(5);
					if (text3[0] != '/' && text3[0] != '\\')
					{
						text2 = new Uri(((Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXServer) ? (Application.dataPath + "/../../") : (Application.dataPath + "/../")) + text3).AbsoluteUri;
					}
				}
				FetchWwwTexture(_index, text2);
			}
			else
			{
				xui.LoadData(text, [PublicizedFrom(EAccessModifier.Private)] (Texture _o) =>
				{
					Texture = _o;
					isExternalTexture = false;
				});
			}
		}
		catch (Exception e)
		{
			Log.Error("[XUi] Could not load texture: " + text + ", on " + GetXuiHierarchy());
			Log.Exception(e);
		}
		SetDirty();
		[PublicizedFrom(EAccessModifier.Private)]
		void FetchWwwTexture(int _currentIndex, string _fetchUri)
		{
			_fetchUri = _fetchUri.Replace("#", "%23").Replace("+", "%2B");
			UnityWebRequest unityWebRequest = UnityWebRequestTexture.GetTexture(_fetchUri);
			unityWebRequest.SendWebRequest();
			ThreadManager.StartCoroutine(WaitForWwwData(_currentIndex, unityWebRequest, _fetchUri));
		}
		[PublicizedFrom(EAccessModifier.Private)]
		IEnumerator WaitForWwwData(int _currentIndex, UnityWebRequest _www, string _fetchUri)
		{
			while (!_www.isDone)
			{
				yield return null;
			}
			if (_www.result != UnityWebRequest.Result.Success)
			{
				if (TextureUris.Length == 1)
				{
					Log.Warning("[XUi] Retrieving texture file from '" + _fetchUri + "' failed (" + _www.error + ").");
				}
				else if (_currentIndex + 1 < TextureUris.Length)
				{
					Log.Warning($"[XUi] Retrieving texture file {_currentIndex + 1} from '{_fetchUri}' failed: ({_www.error}). Trying next URI.");
					loadTexture(_currentIndex + 1);
				}
				else
				{
					Log.Warning($"[XUi] Retrieving texture file {_currentIndex + 1} from '{_fetchUri}' failed: ({_www.error}). No URIs successful.");
				}
			}
			else
			{
				Texture2D texture2D = ((DownloadHandlerTexture)_www.downloadHandler).texture;
				Texture = TextureUtils.CloneTexture(texture2D, _createMipMaps: false, _compress: false, _makeNonReadable: true);
				UnityEngine.Object.DestroyImmediate(texture2D);
				_www.Dispose();
				isExternalTexture = true;
			}
		}
	}

	public override void UnloadTexture()
	{
		Texture texture = Texture;
		base.UnloadTexture();
		TextureUris = null;
		if (!(texture == null))
		{
			if (!isExternalTexture)
			{
				Resources.UnloadAsset(texture);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(texture);
			}
		}
	}
}
