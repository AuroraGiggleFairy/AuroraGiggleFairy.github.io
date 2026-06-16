using System;
using System.Collections.Generic;
using UnityEngine;

public class ReleaseAssetsOnDestroy : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<LoadManager.IAssetHandle> assetHandles = new List<LoadManager.IAssetHandle>();

	public void AddAssetHandle(LoadManager.IAssetHandle handle)
	{
		assetHandles.Add(handle);
	}

	public void CopyTo(ReleaseAssetsOnDestroy other)
	{
		foreach (LoadManager.IAssetHandle assetHandle in other.assetHandles)
		{
			assetHandles.Add(assetHandle.Copy());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		foreach (LoadManager.IAssetHandle assetHandle in assetHandles)
		{
			assetHandle.Release();
		}
		assetHandles.Clear();
	}
}
