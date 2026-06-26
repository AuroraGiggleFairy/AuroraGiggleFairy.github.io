using System;
using UnityEngine;

public class DynamicUIAtlasAssigner : MonoBehaviour
{
	public string AtlasPathInScene;

	public string OptionalSpriteName;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicUIAtlas atlas;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public UISprite[] sprites;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		GameObject gameObject = GameObject.Find(AtlasPathInScene);
		if (gameObject == null)
		{
			Log.Warning("Could not assign atlas: Atlas object not found");
			UnityEngine.Object.Destroy(this);
			return;
		}
		atlas = gameObject.GetComponent<DynamicUIAtlas>();
		if (atlas == null)
		{
			Log.Warning("Could not assign atlas: Atlas component not found");
			UnityEngine.Object.Destroy(this);
			return;
		}
		atlas.AtlasUpdatedEv += AtlasUpdateCallback;
		sprites = GetComponents<UISprite>();
		UISprite[] array = sprites;
		foreach (UISprite uISprite in array)
		{
			uISprite.atlas = atlas;
			if (!string.IsNullOrEmpty(OptionalSpriteName))
			{
				uISprite.spriteName = OptionalSpriteName;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnDestroy()
	{
		if (atlas != null)
		{
			atlas.AtlasUpdatedEv -= AtlasUpdateCallback;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void AtlasUpdateCallback()
	{
		if (!string.IsNullOrEmpty(OptionalSpriteName))
		{
			UISprite[] array = sprites;
			foreach (UISprite obj in array)
			{
				obj.spriteName = null;
				obj.spriteName = OptionalSpriteName;
			}
		}
	}
}
