using System;
using System.Collections.Generic;
using UnityEngine;

public class TextureLoadingManager : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct AsyncLoadInfo
	{
		public ResourceRequest resRequest;

		public string propName;

		public Material material;

		public string fullPath;

		public Texture lowResTexture;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class TextureInfo
	{
		public bool bPending;

		public int refCounts;

		public Texture tex;
	}

	public static TextureLoadingManager Instance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, TextureInfo> availableTextures = new Dictionary<string, TextureInfo>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<AsyncLoadInfo> runningRequests = new List<AsyncLoadInfo>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTimeChecked;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		Instance = this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Time.time - lastTimeChecked < 1f)
		{
			return;
		}
		lastTimeChecked = Time.time;
		for (int num = runningRequests.Count - 1; num >= 0; num--)
		{
			AsyncLoadInfo asyncLoadInfo = runningRequests[num];
			if (asyncLoadInfo.resRequest.isDone && availableTextures.TryGetValue(asyncLoadInfo.fullPath, out var value))
			{
				value.tex = (Texture)asyncLoadInfo.resRequest.asset;
				value.bPending = false;
				if ((bool)asyncLoadInfo.material)
				{
					asyncLoadInfo.material.SetTexture(asyncLoadInfo.propName, value.tex);
				}
				runningRequests.RemoveAt(num);
			}
		}
	}

	public void Cleanup()
	{
		availableTextures.Clear();
		runningRequests.Clear();
	}

	public void LoadTexture(Material _m, string _propName, string _assetPath, string _texName, Texture _lowResTexture)
	{
		if (!Application.isPlaying)
		{
			Texture value = Resources.Load<Texture2D>(_assetPath + _texName);
			_m.SetTexture(_propName, value);
			return;
		}
		TextureInfo value2 = null;
		string text = _assetPath + _texName;
		if (availableTextures.TryGetValue(text, out value2) && !value2.bPending)
		{
			_m.SetTexture(_propName, value2.tex);
			value2.refCounts++;
			return;
		}
		ResourceRequest resRequest = Resources.LoadAsync<Texture2D>(text);
		AsyncLoadInfo item = new AsyncLoadInfo
		{
			resRequest = resRequest,
			propName = _propName,
			material = _m,
			fullPath = text,
			lowResTexture = _lowResTexture
		};
		runningRequests.Add(item);
		if (value2 == null)
		{
			value2 = new TextureInfo();
			value2.bPending = true;
			value2.refCounts = 1;
			availableTextures.Add(text, value2);
		}
		else
		{
			value2.refCounts++;
		}
	}

	public bool UnloadTexture(string _assetPath, string _texName)
	{
		string key = _assetPath + _texName;
		if (availableTextures.TryGetValue(key, out var value))
		{
			value.refCounts--;
			if (value.refCounts == 0)
			{
				availableTextures.Remove(key);
				Resources.UnloadAsset(value.tex);
				return true;
			}
		}
		return false;
	}

	public int GetHiResTextureCount()
	{
		return availableTextures.Count;
	}
}
