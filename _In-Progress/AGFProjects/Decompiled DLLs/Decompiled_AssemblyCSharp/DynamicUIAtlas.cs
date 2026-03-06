using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using UnityEngine;

public class DynamicUIAtlas : UIAtlas
{
	public Shader shader;

	public string PrebakedAtlas;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int elementWidth;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int elementHeight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int paddingSize;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<UISpriteData> origSpriteData;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D currentTex;

	public event Action AtlasUpdatedEv;

	public void Awake()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		if (PrebakedAtlas.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
		{
			PrebakedAtlas = PrebakedAtlas.Substring(0, PrebakedAtlas.Length - 4);
		}
		if (!DynamicUIAtlasTools.ReadPrebakedAtlasDescriptor(PrebakedAtlas, out origSpriteData, out elementWidth, out elementHeight, out paddingSize))
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		base.spriteMaterial = new Material(shader);
		base.spriteList = new List<UISpriteData>();
		ResetAtlas();
		base.pixelSize = 1f;
		stopwatch.Stop();
		Log.Out("Atlas load took " + stopwatch.ElapsedMilliseconds + " ms");
		if (this.AtlasUpdatedEv != null)
		{
			this.AtlasUpdatedEv();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LoadBaseTexture()
	{
		if (!DynamicUIAtlasTools.ReadPrebakedAtlasTexture(PrebakedAtlas, out var _tex))
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		currentTex = new Texture2D(_tex.width, _tex.height, _tex.format, _tex.mipmapCount > 1);
		NativeArray<byte> rawTextureData = _tex.GetRawTextureData<byte>();
		NativeArray<byte> rawTextureData2 = currentTex.GetRawTextureData<byte>();
		rawTextureData.CopyTo(rawTextureData2);
		DynamicUIAtlasTools.UnloadTex(PrebakedAtlas, _tex);
	}

	public void LoadAdditionalSprites(Dictionary<string, Texture2D> _nameToTex)
	{
		DynamicUIAtlasTools.AddSprites(elementWidth, elementHeight, paddingSize, _nameToTex, ref currentTex, base.spriteList);
		base.spriteMaterial.mainTexture = currentTex;
		currentTex.Apply();
		if (this.AtlasUpdatedEv != null)
		{
			this.AtlasUpdatedEv();
		}
	}

	public void ResetAtlas()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		if (currentTex != null)
		{
			UnityEngine.Object.Destroy(currentTex);
		}
		base.spriteList.Clear();
		LoadBaseTexture();
		base.spriteMaterial.mainTexture = currentTex;
		currentTex.Apply();
		base.spriteList.AddRange(origSpriteData);
		stopwatch.Stop();
		Log.Out("Atlas reset took " + stopwatch.ElapsedMilliseconds + " ms");
		if (this.AtlasUpdatedEv != null)
		{
			this.AtlasUpdatedEv();
		}
	}

	public void Compress()
	{
		currentTex.Compress(highQuality: true);
		currentTex.Apply(updateMipmaps: false, makeNoLongerReadable: true);
	}

	public static DynamicUIAtlas Create(GameObject _parent, string _prebakedAtlasResourceName, Shader _shader)
	{
		string text = _prebakedAtlasResourceName;
		int num;
		if ((num = _prebakedAtlasResourceName.IndexOf('?')) >= 0)
		{
			text = text.Substring(num + 1);
		}
		GameObject gameObject = new GameObject(text);
		gameObject.transform.parent = _parent.transform;
		gameObject.SetActive(value: false);
		DynamicUIAtlas dynamicUIAtlas = gameObject.AddComponent<DynamicUIAtlas>();
		dynamicUIAtlas.PrebakedAtlas = _prebakedAtlasResourceName;
		dynamicUIAtlas.shader = _shader;
		gameObject.SetActive(value: true);
		return dynamicUIAtlas;
	}
}
