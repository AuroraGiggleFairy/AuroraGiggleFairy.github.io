using UnityEngine;

public class EModelInstanceAssets
{
	[PublicizedFrom(EAccessModifier.Private)]
	public LoadManager.AssetRequestTask<GameObject> meshHandle;

	[PublicizedFrom(EAccessModifier.Private)]
	public LoadManager.AssetRequestTask<Material> altMaterialHandle;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Transform Mesh
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Material AltMaterial
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsLoadComplete
	{
		get
		{
			LoadManager.AssetRequestTask<GameObject> assetRequestTask = meshHandle;
			if (assetRequestTask == null || assetRequestTask.IsDone)
			{
				return altMaterialHandle?.IsDone ?? true;
			}
			return false;
		}
	}

	public bool IsLoadSuccessful
	{
		get
		{
			if (!IsLoadComplete)
			{
				return false;
			}
			if (meshHandle != null && Mesh == null)
			{
				return false;
			}
			return true;
		}
	}

	public void Load(bool _loadSync, EntityCreationData _ecd, EntityClass _ec)
	{
		if (!string.IsNullOrEmpty(_ec.meshPath))
		{
			meshHandle = LoadManager.LoadAsset(_ec.meshPath, [PublicizedFrom(EAccessModifier.Internal)] (GameObject mesh) =>
			{
				OnMeshLoaded(mesh, _ec.meshPath, _ec.entityClassName);
			}, null, _deferLoading: false, _loadSync);
		}
		if (_ec.AltMatNames == null)
		{
			return;
		}
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(_ecd.id);
		int num = gameRandom.RandomRange(_ec.AltMatNames.Length + 1) - 1;
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
		if (num >= 0)
		{
			string assetPath = _ec.AltMatNames[num];
			altMaterialHandle = LoadManager.LoadAsset(assetPath, [PublicizedFrom(EAccessModifier.Internal)] (Material mat) =>
			{
				OnAltMaterialLoaded(mat, assetPath, _ec.entityClassName);
			}, null, _deferLoading: false, _loadSync);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnMeshLoaded(GameObject mesh, string assetPath, string entityClassName)
	{
		if (mesh == null)
		{
			Log.Error("Could not load mesh '" + assetPath + "' for entity_class '" + entityClassName + "'");
		}
		else
		{
			Mesh = mesh.transform;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnAltMaterialLoaded(Material material, string assetPath, string entityClassName)
	{
		if (material == null)
		{
			Log.Error("Could not load alt material '" + assetPath + "' for entity_class '" + entityClassName + "'");
		}
		else
		{
			AltMaterial = material;
		}
	}

	public void WaitForComplete()
	{
		if (meshHandle != null && !meshHandle.IsDone)
		{
			meshHandle.WaitForComplete();
		}
		if (altMaterialHandle != null && !altMaterialHandle.IsDone)
		{
			altMaterialHandle.WaitForComplete();
		}
	}

	public void Release()
	{
		meshHandle?.Release();
		altMaterialHandle?.Release();
	}
}
