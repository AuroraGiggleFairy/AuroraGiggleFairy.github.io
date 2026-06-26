using System;
using System.Collections.Generic;
using UnityEngine;

public class MultiSourceAtlasManager : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class BaseAtlas
	{
		public GameObject Parent;

		public UIAtlas Atlas;

		public bool IsLoadedInGame;
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<BaseAtlas> atlases = new List<BaseAtlas>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, BaseAtlas> atlasesForSprites = new Dictionary<string, BaseAtlas>();

	public UIAtlas GetAtlasForSprite(string _spriteName)
	{
		if (atlasesForSprites.TryGetValue(_spriteName, out var value))
		{
			return value.Atlas;
		}
		if (atlases.Count <= 0)
		{
			return null;
		}
		return atlases[0].Atlas;
	}

	public void AddAtlas(UIAtlas _atlas, bool _isLoadingInGame)
	{
		BaseAtlas item = new BaseAtlas
		{
			Parent = _atlas.gameObject,
			Atlas = _atlas,
			IsLoadedInGame = _isLoadingInGame
		};
		atlases.Add(item);
		_atlas.name = base.name;
		recalcSpriteSources();
	}

	public void CleanupAfterGame()
	{
		for (int num = atlases.Count - 1; num >= 0; num--)
		{
			BaseAtlas baseAtlas = atlases[num];
			if (baseAtlas.IsLoadedInGame)
			{
				atlases.RemoveAt(num);
				UnityEngine.Object.Destroy(baseAtlas.Atlas.spriteMaterial.mainTexture);
				UnityEngine.Object.Destroy(baseAtlas.Parent);
			}
		}
		recalcSpriteSources();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void recalcSpriteSources()
	{
		foreach (BaseAtlas atlase in atlases)
		{
			foreach (UISpriteData sprite in atlase.Atlas.spriteList)
			{
				atlasesForSprites[sprite.name] = atlase;
			}
		}
	}

	public static MultiSourceAtlasManager Create(GameObject _parent, string _name)
	{
		GameObject obj = new GameObject(_name);
		obj.transform.parent = _parent.transform;
		return obj.AddComponent<MultiSourceAtlasManager>();
	}
}
