using UnityEngine;

public static class BlockHighlighter
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string TemplatePath = "@:Entities/Misc/block_highlightPrefab.prefab";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3 halfBlockOffset = new Vector3(0.5f, 0.5f, 0.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject topGameObject;

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject blockPrefab;

	public static void AddBlock(Vector3i _pos)
	{
		EnforceGo();
		EnforceTemplateLoaded();
		Object.Instantiate(blockPrefab, topGameObject.transform).transform.position = _pos.ToVector3() + halfBlockOffset;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void EnforceGo()
	{
		if (!(topGameObject != null))
		{
			topGameObject = new GameObject("BlockHighlighter");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void EnforceTemplateLoaded()
	{
		if (!(blockPrefab != null))
		{
			blockPrefab = DataLoader.LoadAsset<GameObject>("@:Entities/Misc/block_highlightPrefab.prefab");
		}
	}

	public static void Cleanup()
	{
		if (topGameObject != null)
		{
			Object.Destroy(topGameObject);
			topGameObject = null;
		}
	}
}
