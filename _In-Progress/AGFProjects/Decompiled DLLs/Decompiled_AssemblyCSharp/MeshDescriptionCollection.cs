using System;
using System.Collections;
using UnityEngine;

public class MeshDescriptionCollection : MonoBehaviour
{
	public MeshDescription[] meshes;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshDescription[] currentMeshes;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int quality = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] filterToAnisoLevel = new int[5] { 1, 2, 4, 8, 9 };

	public MeshDescription[] Meshes => currentMeshes;

	public void Init()
	{
		MeshDescription meshDescription = meshes[0];
		if ((bool)meshDescription.TexDiffuse || (bool)meshDescription.TexNormal || (bool)meshDescription.TexSpecular)
		{
			Log.Error("MeshDescriptionCollection should not have MESH_OPAQUE textures");
		}
		meshDescription = meshes[5];
		if ((bool)meshDescription.TexDiffuse || (bool)meshDescription.TexNormal || (bool)meshDescription.TexSpecular)
		{
			Log.Error("MeshDescriptionCollection should not have MESH_TERRAIN textures");
		}
		currentMeshes = new MeshDescription[meshes.Length];
		for (int i = 0; i < meshes.Length; i++)
		{
			meshDescription = new MeshDescription(meshes[i]);
			currentMeshes[i] = meshDescription;
		}
	}

	public IEnumerator LoadTextureArrays(bool _isReload = false)
	{
		MicroStopwatch ms = new MicroStopwatch(_bStart: true);
		MeshDescription[] mds = currentMeshes;
		for (int i = 0; i < mds.Length; i++)
		{
			mds[i].UnloadTextureArrays(i);
		}
		if (_isReload)
		{
			Resources.UnloadUnusedAssets();
		}
		for (int index = 0; index < mds.Length; index++)
		{
			MeshDescription meshDescription = mds[index];
			yield return meshDescription.LoadTextureArraysForQuality(this, index, quality, _isReload);
			yield return null;
		}
		Log.Out("LoadTextureArraysForQuality took {0}", (float)ms.ElapsedMilliseconds * 0.001f);
		if (GameManager.Instance != null && GameManager.Instance.prefabLODManager != null)
		{
			GameManager.Instance.prefabLODManager.UpdateMaterials();
		}
	}

	public IEnumerator LoadTextureArraysForQuality(bool _isReload = false)
	{
		if (!GameManager.IsDedicatedServer)
		{
			int num = GameOptionsManager.GetTextureQuality();
			if (num >= 3)
			{
				num = 2;
			}
			Log.Out("LoadTextureArraysForQuality quality {0} to {1}, reload {2}", quality, num, _isReload);
			if (!_isReload || num != quality)
			{
				quality = num;
				yield return LoadTextureArrays(_isReload);
			}
		}
	}

	public void SetTextureArraysFilter()
	{
		if (!GameManager.IsDedicatedServer)
		{
			int textureFilter = GameOptionsManager.GetTextureFilter();
			int num = filterToAnisoLevel[textureFilter];
			Log.Out("SetTextureArraysFilter {0}, AF {1}", textureFilter, num);
			MeshDescription[] array = currentMeshes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetTextureFilter(i, num);
			}
		}
	}

	public void Cleanup()
	{
	}
}
