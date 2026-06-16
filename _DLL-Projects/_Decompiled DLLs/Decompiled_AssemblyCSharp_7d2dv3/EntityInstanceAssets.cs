using UnityEngine;

public class EntityInstanceAssets
{
	[PublicizedFrom(EAccessModifier.Private)]
	public LoadManager.AssetRequestTask<GameObject> prefabHandle;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Transform PrefabT
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsLoadComplete => prefabHandle.IsDone;

	public bool IsLoadSuccessful
	{
		get
		{
			if (!IsLoadComplete)
			{
				return false;
			}
			if (PrefabT == null)
			{
				return false;
			}
			return true;
		}
	}

	public void Load(bool _loadSync, EntityClass _ec, bool isLocalPlayer)
	{
		if (isLocalPlayer)
		{
			prefabHandle = LoadManager.LoadAsset<GameObject>("Prefabs/prefabEntityPlayerLocal", null, null, false, true);
			PrefabT = prefabHandle.Asset.transform;
		}
		else
		{
			prefabHandle = LoadManager.LoadAsset<GameObject>(_ec.prefabPath, OnPrefabLoaded, null, _deferLoading: false, _loadSync);
		}
	}

	public void OnPrefabLoaded(GameObject prefab)
	{
		PrefabT = prefab.transform;
	}

	public void WaitForComplete()
	{
		if (!prefabHandle.IsDone)
		{
			prefabHandle.WaitForComplete();
		}
	}

	public void Release()
	{
		prefabHandle?.Release();
	}
}
