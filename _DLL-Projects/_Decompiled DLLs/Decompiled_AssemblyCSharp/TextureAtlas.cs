using UnityEngine;

public class TextureAtlas
{
	public Texture diffuseTexture;

	public Texture normalTexture;

	public Texture specularTexture;

	public Texture emissionTexture;

	public Texture heightTexture;

	public Texture occlusionTexture;

	public Texture2D maskTexture;

	public Texture2D maskNormalTexture;

	public UVRectTiling[] uvMapping = new UVRectTiling[0];

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bDestroyTextures;

	public TextureAtlas()
		: this(_bDestroyTextures: true)
	{
	}

	public TextureAtlas(bool _bDestroyTextures)
	{
		bDestroyTextures = _bDestroyTextures;
	}

	public virtual bool LoadTextureAtlas(int _idx, MeshDescriptionCollection _tac, bool _bLoadTextures)
	{
		if (_bLoadTextures)
		{
			MeshDescription meshDescription = _tac.Meshes[_idx];
			diffuseTexture = meshDescription.TexDiffuse;
			normalTexture = meshDescription.TexNormal;
			specularTexture = meshDescription.TexSpecular;
			emissionTexture = meshDescription.TexEmission;
			heightTexture = meshDescription.TexHeight;
			occlusionTexture = meshDescription.TexOcclusion;
			maskTexture = meshDescription.TexMask;
			maskNormalTexture = meshDescription.TexMaskNormal;
		}
		return true;
	}

	public virtual void Cleanup()
	{
	}
}
