using UnityEngine;
using UnityEngine.AddressableAssets;

public class SkinTextureOverride : MonoBehaviour
{
	public AssetReference skinAlbedo;

	public AssetReference skinNormal;

	public AssetReference skinRMOE;

	public void ApplyOverrides()
	{
		Texture2D value = skinAlbedo.LoadAssetAsync<Texture2D>().WaitForCompletion();
		Texture2D value2 = skinNormal.LoadAssetAsync<Texture2D>().WaitForCompletion();
		Texture2D value3 = skinRMOE.LoadAssetAsync<Texture2D>().WaitForCompletion();
		SkinnedMeshRenderer component = base.transform.GetComponent<SkinnedMeshRenderer>();
		Material[] materials = component.materials;
		for (int i = 0; i < materials.Length; i++)
		{
			if (materials[i] != null && materials[i].shader.name == "Game/SDCS/Skin")
			{
				string text = materials[i].name;
				if (text.Contains("_Body") || text.Contains("_Head") || text.Contains("_Hand"))
				{
					materials[i] = new Material(materials[i]);
					materials[i].SetTexture("_Albedo", value);
					materials[i].SetTexture("_NormalMap", value2);
					materials[i].SetTexture("_Masks", value3);
				}
			}
		}
		component.materials = materials;
	}
}
