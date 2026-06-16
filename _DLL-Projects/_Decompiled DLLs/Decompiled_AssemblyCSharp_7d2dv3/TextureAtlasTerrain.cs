using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TextureAtlasTerrain : TextureAtlasBlocks
{
	public Texture2D[] diffuse;

	public Texture2D[] normal;

	public Texture2D[] specular;

	public TextureAtlasTerrain()
	{
		bDestroyTextures = false;
	}

	public override bool LoadTextureAtlas(int _idx, MeshDescriptionCollection _tac, bool _bLoadTextures)
	{
		if (base.LoadTextureAtlas(_idx, _tac, _bLoadTextures))
		{
			diffuse = new Texture2D[uvMapping.Length];
			normal = new Texture2D[uvMapping.Length];
			specular = new Texture2D[uvMapping.Length];
			if (_bLoadTextures)
			{
				for (int i = 0; i < uvMapping.Length; i++)
				{
					if (uvMapping[i].textureName == null)
					{
						continue;
					}
					string text = GameIO.RemoveFileExtension(uvMapping[i].textureName);
					string fileExtension = GameIO.GetFileExtension(uvMapping[i].textureName);
					Texture2D asset = LoadManager.LoadAssetFromAddressables<Texture2D>("TerrainTextures", uvMapping[i].textureName, null, null, _deferLoading: false, _loadSync: true).Asset;
					if (asset == null)
					{
						throw new Exception("TextureAtlasTerrain: couldn't load diffuse texture '" + uvMapping[i].textureName + "'");
					}
					Texture2D asset2 = LoadManager.LoadAssetFromAddressables<Texture2D>("TerrainTextures", text + "_n" + fileExtension, null, null, _deferLoading: false, _loadSync: true).Asset;
					if (asset2 == null)
					{
						throw new Exception("TextureAtlasTerrain: couldn't load normal texture '" + text + "_n" + fileExtension + "'");
					}
					Texture2D asset3 = LoadManager.LoadAssetFromAddressables<Texture2D>("TerrainTextures", text + "_s" + fileExtension, null, null, _deferLoading: false, _loadSync: true).Asset;
					if (!Application.isEditor)
					{
						if (asset != null && asset.isReadable)
						{
							asset.Apply(updateMipmaps: false, makeNoLongerReadable: true);
						}
						if (asset2 != null && asset2.isReadable)
						{
							asset2.Apply(updateMipmaps: false, makeNoLongerReadable: true);
						}
						if (asset3 != null && asset3.isReadable)
						{
							asset3.Apply(updateMipmaps: false, makeNoLongerReadable: true);
						}
					}
					diffuse[i] = asset;
					normal[i] = asset2;
					specular[i] = asset3;
				}
			}
		}
		return true;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		for (int i = 0; i < diffuse.Length; i++)
		{
			if (diffuse[i] != null)
			{
				LoadManager.ReleaseAddressable(diffuse[i]);
			}
		}
		for (int j = 0; j < normal.Length; j++)
		{
			if (normal[j] != null)
			{
				LoadManager.ReleaseAddressable(normal[j]);
			}
		}
		for (int k = 0; k < specular.Length; k++)
		{
			if (specular[k] != null)
			{
				LoadManager.ReleaseAddressable(specular[k]);
			}
		}
	}
}
