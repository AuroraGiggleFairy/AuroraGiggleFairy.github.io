using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class MeshDescription
{
	public enum EnumRenderMode
	{
		Default = -1,
		Opaque,
		Cutout,
		Fade,
		Transparent
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum ETextureType
	{
		Diffuse,
		Normal,
		Specular
	}

	public const int cIndexOpaque = 0;

	public const int cIndexWater = 1;

	public const int cIndexTransparent = 2;

	public const int cIndexGrass = 3;

	public const int cIndexDecals = 4;

	public const int cIndexTerrain = 5;

	public const int cIndexCount = 6;

	public const int MESH_OPAQUE = 0;

	public const int MESH_WATER = 1;

	public const int MESH_TRANSPARENT = 2;

	public const int MESH_GRASS = 3;

	public const int MESH_DECALS = 4;

	public const int MESH_TERRAIN = 5;

	public const int MESH_MODELS = 0;

	public static MeshDescription[] meshes = new MeshDescription[0];

	public static int GrassQualityPlanes;

	public string Name;

	public string Tag;

	public VoxelMesh.EnumMeshType meshType;

	public bool bCastShadows;

	public bool bReceiveShadows;

	public bool bHasLODs;

	public bool bUseDebugStabilityShader;

	public bool bTerrain;

	public bool bTextureArray;

	public bool bSpecularIsBlack;

	public bool CreateTextureAtlas = true;

	public bool CreateSpecularMap = true;

	public bool CreateNormalMap = true;

	public bool CreateEmissionMap;

	public bool CreateHeightMap;

	public bool CreateOcclusionMap;

	public string MeshLayerName;

	public string ColliderLayerName;

	public AssetReference PrimaryShader;

	public AssetReference SecondaryShader;

	public AssetReference DistantShader;

	public EnumRenderMode BlendMode;

	public Material BaseMaterial;

	public Material SecondaryMaterial;

	public string TextureAtlasClass;

	public Texture TexDiffuse;

	public Texture TexNormal;

	public Texture TexSpecular;

	public Texture TexEmission;

	public Texture TexHeight;

	public Texture TexOcclusion;

	public Texture2D TexMask;

	public Texture2D TexMaskNormal;

	public TextAsset MetaData;

	[HideInInspector]
	public TextureAtlas textureAtlas;

	[NonSerialized]
	public Material[] materials;

	[NonSerialized]
	public Material materialDistant;

	[NonSerialized]
	public Material[] prefabTerrainMaterials;

	[NonSerialized]
	public Material prefabTerrainMaterialDistant;

	public static bool bDebugStability;

	public Material material
	{
		get
		{
			if (materials != null)
			{
				return materials[0];
			}
			return null;
		}
		set
		{
			if (materials != null)
			{
				materials[0] = value;
			}
		}
	}

	public Material prefabPreviewMaterial
	{
		get
		{
			if (prefabTerrainMaterials != null)
			{
				return prefabTerrainMaterials[0];
			}
			return null;
		}
		set
		{
			if (prefabTerrainMaterials != null)
			{
				prefabTerrainMaterials[0] = value;
			}
		}
	}

	public IEnumerator Init(int _idx, TextureAtlas _ta)
	{
		textureAtlas = _ta;
		if (GameManager.IsDedicatedServer)
		{
			yield break;
		}
		if (UseSplatmap(_idx))
		{
			materials = new Material[1];
			LoadManager.AddressableRequestTask<Material> assetRequestTask = LoadManager.LoadAssetFromAddressables<Material>("TerrainTextures", "Microsplat/MicroSplatTerrainInGame.mat", null, null, _deferLoading: false, ThreadManager.IsInSyncCoroutine);
			yield return assetRequestTask;
			materials[0] = UnityEngine.Object.Instantiate(assetRequestTask.Asset);
			materials[0].name = "Near Terrain";
			materials[0].SetFloat("_ShaderMode", 2f);
			materialDistant = new Material(materials[0]);
			materialDistant.SetFloat("_ShaderMode", 1f);
			materialDistant.name = "Distant Terrain";
			ReloadTextureArrays(_isSplatmap: true);
			assetRequestTask.Release();
			if (_idx == 5)
			{
				yield return setupPrefabTerrainMaterials(_idx, _ta);
			}
			yield break;
		}
		if (_idx == 2)
		{
			if (_ta != null)
			{
				Material material = new Material(BaseMaterial);
				Material material2 = new Material(SecondaryMaterial);
				material.SetTexture("_Albedo", _ta.diffuseTexture);
				material.SetTexture("_NormalMap", _ta.normalTexture);
				material.SetTexture("_GME", _ta.specularTexture);
				material2.SetTexture("_Albedo", _ta.diffuseTexture);
				materials = new Material[2];
				materials[0] = material2;
				materials[1] = material;
			}
			yield break;
		}
		if (SecondaryShader?.RuntimeKeyIsValid() ?? false)
		{
			materials = new Material[2];
		}
		else
		{
			materials = new Material[1];
		}
		if (_ta == null)
		{
			yield break;
		}
		Material material3 = BaseMaterial;
		if (!material3)
		{
			if (PrimaryShader == null)
			{
				Log.Out("Null PrimaryShader for " + Name);
			}
			Shader shader = DataLoader.LoadAsset<Shader>(PrimaryShader);
			if (shader == null)
			{
				Log.Error("Can't find shader: " + PrimaryShader.RuntimeKey);
			}
			material3 = new Material(shader);
		}
		materials[0] = material3;
		if (_idx == 3)
		{
			material3.SetTexture("_Albedo", _ta.diffuseTexture);
			material3.SetTexture("_Normal", _ta.normalTexture);
			material3.SetTexture("_Gloss_AO_SSS", _ta.specularTexture);
		}
		else
		{
			if (_idx != 5 || _ta.diffuseTexture is Texture2D)
			{
				material3.SetTexture("_MainTex", _ta.diffuseTexture);
			}
			if (_idx != 5 || _ta.normalTexture is Texture2D)
			{
				material3.SetTexture("_BumpMap", _ta.normalTexture);
			}
			material3.SetTexture("_MetallicGlossMap", _ta.specularTexture);
			material3.SetTexture("_OcclusionMap", _ta.occlusionTexture);
			material3.SetTexture("_MaskTex", _ta.maskTexture);
			material3.SetTexture("_MaskBumpMapTex", _ta.maskNormalTexture);
			material3.SetTexture("_EmissionMap", _ta.emissionTexture);
		}
		if (BlendMode != EnumRenderMode.Default)
		{
			SetupMaterialWithBlendMode(material3, BlendMode);
		}
		if (DistantShader?.RuntimeKeyIsValid() ?? false)
		{
			materialDistant = new Material(DataLoader.LoadAsset<Shader>(DistantShader));
			if (_idx != 5 || _ta.diffuseTexture is Texture2D)
			{
				materialDistant.SetTexture("_MainTex", _ta.diffuseTexture);
			}
			if (_idx != 5 || _ta.normalTexture is Texture2D)
			{
				materialDistant.SetTexture("_BumpMap", _ta.normalTexture);
			}
			materialDistant.SetTexture("_MetallicGlossMap", _ta.specularTexture);
			materialDistant.SetTexture("_OcclusionMap", _ta.occlusionTexture);
			materialDistant.SetTexture("_MaskTex", _ta.maskTexture);
			materialDistant.SetTexture("_MaskBumpMapTex", _ta.maskNormalTexture);
			materialDistant.SetTexture("_EmissionMap", _ta.emissionTexture);
			if (BlendMode != EnumRenderMode.Default)
			{
				SetupMaterialWithBlendMode(materialDistant, BlendMode);
			}
		}
		if (SecondaryShader?.RuntimeKeyIsValid() ?? false)
		{
			Shader shader2 = DataLoader.LoadAsset<Shader>(SecondaryShader);
			if (shader2 == null)
			{
				Log.Error("Can't find secondary shader: " + SecondaryShader.RuntimeKey);
			}
			materials[1] = new Material(shader2);
			materials[1].CopyPropertiesFromMaterial(materials[0]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator setupPrefabTerrainMaterials(int _idx, TextureAtlas _ta)
	{
		yield return null;
		if (SecondaryShader?.RuntimeKeyIsValid() ?? false)
		{
			prefabTerrainMaterials = new Material[2];
		}
		else
		{
			prefabTerrainMaterials = new Material[1];
		}
		if (_ta == null)
		{
			yield break;
		}
		Material material = BaseMaterial;
		if (!material)
		{
			Shader shader = DataLoader.LoadAsset<Shader>(PrimaryShader);
			if (shader == null)
			{
				Log.Error("Can't find shader: " + PrimaryShader.RuntimeKey);
			}
			material = new Material(shader);
		}
		prefabTerrainMaterials[0] = material;
		if (_idx != 5 || _ta.diffuseTexture is Texture2D)
		{
			material.SetTexture("_MainTex", _ta.diffuseTexture);
		}
		if (_idx != 5 || _ta.normalTexture is Texture2D)
		{
			material.SetTexture("_BumpMap", _ta.normalTexture);
		}
		material.SetTexture("_MetallicGlossMap", _ta.specularTexture);
		material.SetTexture("_OcclusionMap", _ta.occlusionTexture);
		material.SetTexture("_MaskTex", _ta.maskTexture);
		material.SetTexture("_MaskBumpMapTex", _ta.maskNormalTexture);
		material.SetTexture("_EmissionMap", _ta.emissionTexture);
		if (BlendMode != EnumRenderMode.Default)
		{
			SetupMaterialWithBlendMode(material, BlendMode);
		}
		if (DistantShader?.RuntimeKeyIsValid() ?? false)
		{
			materialDistant = new Material(DataLoader.LoadAsset<Shader>(DistantShader));
			if (_idx != 5 || _ta.diffuseTexture is Texture2D)
			{
				materialDistant.SetTexture("_MainTex", _ta.diffuseTexture);
			}
			if (_idx != 5 || _ta.normalTexture is Texture2D)
			{
				materialDistant.SetTexture("_BumpMap", _ta.normalTexture);
			}
			materialDistant.SetTexture("_MetallicGlossMap", _ta.specularTexture);
			materialDistant.SetTexture("_OcclusionMap", _ta.occlusionTexture);
			materialDistant.SetTexture("_MaskTex", _ta.maskTexture);
			materialDistant.SetTexture("_MaskBumpMapTex", _ta.maskNormalTexture);
			materialDistant.SetTexture("_EmissionMap", _ta.emissionTexture);
			if (BlendMode != EnumRenderMode.Default)
			{
				SetupMaterialWithBlendMode(materialDistant, BlendMode);
			}
		}
		if (SecondaryShader?.RuntimeKeyIsValid() ?? false)
		{
			Shader shader2 = DataLoader.LoadAsset<Shader>(SecondaryShader);
			if (shader2 == null)
			{
				Log.Error("Can't find secondary shader: " + SecondaryShader.RuntimeKey);
			}
			prefabTerrainMaterials[1] = new Material(shader2);
			prefabTerrainMaterials[1].CopyPropertiesFromMaterial(materials[0]);
		}
	}

	public void ReloadTextureArrays(bool _isSplatmap)
	{
		if (_isSplatmap)
		{
			if (material != null)
			{
				material.SetTexture("_Diffuse", TexDiffuse);
				material.SetTexture("_NormalSAO", TexNormal);
				material.SetTexture("_SmoothAO", TexSpecular);
				Log.Out("Set Microsplat diffuse: " + TexDiffuse);
				Log.Out("Set Microsplat normals: " + TexNormal);
				Log.Out("Set Microsplat smooth:  " + TexSpecular);
			}
			if (materialDistant != null)
			{
				materialDistant.SetTexture("_Diffuse", TexDiffuse);
				materialDistant.SetTexture("_NormalSAO", TexNormal);
				materialDistant.SetTexture("_SmoothAO", TexSpecular);
			}
		}
		else if (bTextureArray && materials != null && materials.Length != 0 && materials[0] != null)
		{
			materials[0].mainTexture = textureAtlas.diffuseTexture;
			materials[0].SetTexture("_BumpMap", textureAtlas.normalTexture);
			materials[0].SetTexture("_MetallicGlossMap", textureAtlas.specularTexture);
			materials[0].SetTexture("_OcclusionMap", textureAtlas.occlusionTexture);
			materials[0].SetTexture("_MaskTex", textureAtlas.maskTexture);
			materials[0].SetTexture("_MaskBumpMapTex", textureAtlas.maskNormalTexture);
			materials[0].SetTexture("_EmissionMap", textureAtlas.emissionTexture);
			if (materialDistant != null)
			{
				materialDistant.mainTexture = textureAtlas.diffuseTexture;
				materialDistant.SetTexture("_BumpMap", textureAtlas.normalTexture);
				materialDistant.SetTexture("_MetallicGlossMap", textureAtlas.specularTexture);
				materialDistant.SetTexture("_OcclusionMap", textureAtlas.occlusionTexture);
				materialDistant.SetTexture("_MaskTex", textureAtlas.maskTexture);
				materialDistant.SetTexture("_MaskBumpMapTex", textureAtlas.maskNormalTexture);
				materialDistant.SetTexture("_EmissionMap", textureAtlas.emissionTexture);
			}
			if (materials.Length > 1 && materials[1] != null)
			{
				materials[1].CopyPropertiesFromMaterial(materials[0]);
			}
		}
	}

	public bool IsSplatmap(int _index)
	{
		return _index == 5;
	}

	public bool UseSplatmap(int _index)
	{
		if (GameManager.IsSplatMapAvailable())
		{
			return IsSplatmap(_index);
		}
		return false;
	}

	public static void SetDebugStabilityShader(bool _bOn)
	{
		if (_bOn)
		{
			Shader shader = Shader.Find("Game/Debug/Stability");
			MeshDescription[] array = meshes;
			foreach (MeshDescription meshDescription in array)
			{
				if (meshDescription.bUseDebugStabilityShader)
				{
					meshDescription.materials[0].shader = shader;
				}
			}
			foreach (Chunk item in GameManager.Instance.World.ChunkCache.GetChunkArray())
			{
				item.NeedsRegeneration = true;
			}
		}
		else
		{
			MeshDescription[] array = meshes;
			foreach (MeshDescription meshDescription2 in array)
			{
				if (meshDescription2.bUseDebugStabilityShader)
				{
					meshDescription2.materials[0].shader = DataLoader.LoadAsset<Shader>(meshDescription2.PrimaryShader);
				}
			}
		}
		bDebugStability = _bOn;
		Camera main = Camera.main;
		if (!main)
		{
			return;
		}
		LightViewer component = main.GetComponent<LightViewer>();
		if (component != null)
		{
			if (bDebugStability)
			{
				component.TurnOffAllLights();
			}
			else
			{
				component.TurnOnAllLights();
			}
		}
	}

	public static void SetGrassQuality()
	{
		if (!GameManager.IsDedicatedServer && meshes != null && 3 < meshes.Length)
		{
			GrassQualityPlanes = 0;
			float value = 45f;
			switch (GamePrefs.GetInt(EnumGamePrefs.OptionsGfxGrassDistance))
			{
			case 1:
				value = 66f;
				break;
			case 2:
				GrassQualityPlanes = 1;
				value = 102f;
				break;
			case 3:
				GrassQualityPlanes = 1;
				value = 123f;
				break;
			}
			meshes[3].material.SetFloat("_FadeDistance", value);
		}
	}

	public static void SetWaterQuality()
	{
		if (!GameManager.IsDedicatedServer && meshes != null && 1 < meshes.Length)
		{
			Material material = meshes[1].materials[0];
			switch (GamePrefs.GetInt(EnumGamePrefs.OptionsGfxWaterQuality))
			{
			case 0:
				material.shader = GlobalAssets.FindShader("Game/Water Distant Surface");
				material.SetFloat("_MinAlpha", 0f);
				break;
			default:
				material.shader = GlobalAssets.FindShader("Game/Water Surface");
				break;
			}
		}
	}

	public static void Cleanup()
	{
		MeshDescription[] array = meshes;
		foreach (MeshDescription obj in array)
		{
			obj.textureAtlas.Cleanup();
			obj.CleanupMats();
		}
		meshes = new MeshDescription[0];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CleanupMats()
	{
		if (materials != null)
		{
			for (int i = 0; i < materials.Length; i++)
			{
				Material material = materials[i];
				if ((bool)material && material != BaseMaterial && material != SecondaryMaterial)
				{
					UnityEngine.Object.Destroy(material);
					materials[i] = null;
				}
			}
		}
		UnityEngine.Object.Destroy(materialDistant);
	}

	public MeshDescription()
	{
	}

	public MeshDescription(MeshDescription other)
	{
		Name = other.Name;
		Tag = other.Tag;
		meshType = other.meshType;
		bCastShadows = other.bCastShadows;
		bReceiveShadows = other.bReceiveShadows;
		bHasLODs = other.bHasLODs;
		bUseDebugStabilityShader = other.bUseDebugStabilityShader;
		bTerrain = other.bTerrain;
		bTextureArray = other.bTextureArray;
		bSpecularIsBlack = other.bSpecularIsBlack;
		CreateTextureAtlas = other.CreateTextureAtlas;
		CreateSpecularMap = other.CreateSpecularMap;
		CreateNormalMap = other.CreateNormalMap;
		CreateEmissionMap = other.CreateEmissionMap;
		CreateHeightMap = other.CreateHeightMap;
		CreateOcclusionMap = other.CreateOcclusionMap;
		MeshLayerName = other.MeshLayerName;
		ColliderLayerName = other.ColliderLayerName;
		PrimaryShader = new AssetReference(other.PrimaryShader.AssetGUID);
		SecondaryShader = new AssetReference(other.SecondaryShader.AssetGUID);
		DistantShader = new AssetReference(other.DistantShader.AssetGUID);
		BlendMode = other.BlendMode;
		BaseMaterial = other.BaseMaterial;
		SecondaryMaterial = other.SecondaryMaterial;
		TextureAtlasClass = other.TextureAtlasClass;
		TexDiffuse = other.TexDiffuse;
		TexNormal = other.TexNormal;
		TexSpecular = other.TexSpecular;
		TexEmission = other.TexEmission;
		TexHeight = other.TexHeight;
		TexOcclusion = other.TexOcclusion;
		TexMask = other.TexMask;
		TexMaskNormal = other.TexMaskNormal;
		MetaData = other.MetaData;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetupMaterialWithBlendMode(Material material, EnumRenderMode blendMode)
	{
		material.SetFloat("_Mode", (float)blendMode);
		switch (blendMode)
		{
		case EnumRenderMode.Opaque:
			material.SetInt("_SrcBlend", 1);
			material.SetInt("_DstBlend", 0);
			material.SetInt("_ZWrite", 1);
			material.DisableKeyword("_ALPHATEST_ON");
			material.DisableKeyword("_ALPHABLEND_ON");
			material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = -1;
			break;
		case EnumRenderMode.Cutout:
			material.SetInt("_SrcBlend", 1);
			material.SetInt("_DstBlend", 0);
			material.SetInt("_ZWrite", 1);
			material.EnableKeyword("_ALPHATEST_ON");
			material.DisableKeyword("_ALPHABLEND_ON");
			material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = 2450;
			break;
		case EnumRenderMode.Fade:
			material.SetInt("_SrcBlend", 5);
			material.SetInt("_DstBlend", 10);
			material.SetInt("_ZWrite", 0);
			material.DisableKeyword("_ALPHATEST_ON");
			material.EnableKeyword("_ALPHABLEND_ON");
			material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = 3000;
			break;
		case EnumRenderMode.Transparent:
			material.SetOverrideTag("RenderType", "Transparent");
			material.SetInt("_SrcBlend", 1);
			material.SetInt("_DstBlend", 10);
			material.SetInt("_ZWrite", 0);
			material.DisableKeyword("_ALPHATEST_ON");
			material.DisableKeyword("_ALPHABLEND_ON");
			material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = 3000;
			break;
		}
		SetMaterialKeywords(material);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetMaterialKeywords(Material material)
	{
		SetKeyword(material, "_NORMALMAP", (bool)material.GetTexture("_BumpMap") || (bool)material.GetTexture("_DetailNormalMap"));
		SetKeyword(material, "_METALLICGLOSSMAP", material.GetTexture("_MetallicGlossMap"));
		int nameID = Shader.PropertyToID("_DetailAlbedoMap");
		int nameID2 = Shader.PropertyToID("_DetailNormalMap");
		SetKeyword(material, "_DETAIL_MULX2", (material.HasProperty(nameID) && (bool)material.GetTexture(nameID)) || (material.HasProperty(nameID2) && (bool)material.GetTexture(nameID2)));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetKeyword(Material m, string keyword, bool state)
	{
		if (state)
		{
			m.EnableKeyword(keyword);
		}
		else
		{
			m.DisableKeyword(keyword);
		}
	}

	public static Material GetOpaqueMaterial()
	{
		if (meshes.Length == 0)
		{
			return new Material(Shader.Find("Diffuse"));
		}
		MeshDescription meshDescription = meshes[0];
		Material material = (meshDescription.bTextureArray ? UnityEngine.Object.Instantiate(Resources.Load<Material>("Materials/DistantPOI_TA")) : UnityEngine.Object.Instantiate(Resources.Load<Material>("Materials/DistantPOI")));
		material.SetTexture("_MainTex", meshDescription.TexDiffuse);
		material.SetTexture("_Normal", meshDescription.TexNormal);
		material.SetTexture("_MetallicGlossMap", meshDescription.TexSpecular);
		material.SetTexture("_OcclusionMap", meshDescription.TexOcclusion);
		return material;
	}

	public IEnumerator LoadTextureArraysForQuality(MeshDescriptionCollection _meshDescriptionCollection, int _index, int _quality, bool _isReload = false)
	{
		bool isSplatmap = IsSplatmap(_index);
		if (!isSplatmap && !bTextureArray)
		{
			yield break;
		}
		yield return loadSingleArray(_quality, isSplatmap, ETextureType.Diffuse);
		yield return null;
		yield return loadSingleArray(_quality, isSplatmap, ETextureType.Normal);
		yield return null;
		yield return loadSingleArray(_quality, isSplatmap, ETextureType.Specular);
		yield return null;
		if (_isReload)
		{
			ReloadTextureArrays(isSplatmap);
			if (meshes.Length != 0)
			{
				textureAtlas.LoadTextureAtlas(_index, _meshDescriptionCollection, !GameManager.IsDedicatedServer);
				ReloadTextureArrays(isSplatmap);
			}
		}
	}

	public void UnloadTextureArrays(int _index)
	{
		if (IsSplatmap(_index) || bTextureArray)
		{
			Unload(ref TexDiffuse);
			Unload(ref TexNormal);
			Unload(ref TexSpecular);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator loadSingleArray(int _quality, bool _isSplatmap, ETextureType _texType)
	{
		string folderAddress = (_isSplatmap ? "TerrainTextures" : "BlockTextureAtlases");
		string path = (_isSplatmap ? ("Microsplat/MicroSplatConfig_" + GetFileSuffixForTextureType(_texType, _isSplatmap) + "_tarray") : ("TextureArrays/" + Constants.cPrefixAtlas + Name + GetFileSuffixForTextureType(_texType, _isSplatmap)));
		while (_quality >= 0)
		{
			string assetPath = path + GetFileSuffixForQuality(_quality, _isSplatmap) + ".asset";
			Texture2DArray asset;
			if (ThreadManager.IsInSyncCoroutine)
			{
				asset = LoadManager.LoadAssetFromAddressables<Texture2DArray>(folderAddress, assetPath, null, null, _deferLoading: false, _loadSync: true).Asset;
			}
			else
			{
				LoadManager.AddressableRequestTask<Texture2DArray> request = LoadManager.LoadAssetFromAddressables<Texture2DArray>(folderAddress, assetPath);
				while (!request.IsDone)
				{
					yield return null;
				}
				asset = request.Asset;
			}
			if (asset != null)
			{
				if (!Application.isEditor && asset.isReadable)
				{
					asset.Apply(updateMipmaps: false, makeNoLongerReadable: true);
				}
				switch (_texType)
				{
				case ETextureType.Diffuse:
					TexDiffuse = asset;
					break;
				case ETextureType.Normal:
					TexNormal = asset;
					break;
				case ETextureType.Specular:
					TexSpecular = asset;
					break;
				}
				yield break;
			}
			_quality--;
		}
		throw new Exception("No Texture2DArray found for " + Name + " " + _texType.ToStringCached());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetFileSuffixForTextureType(ETextureType _type, bool _isSplatmap)
	{
		switch (_type)
		{
		case ETextureType.Diffuse:
			if (!_isSplatmap)
			{
				return "";
			}
			return "diff";
		case ETextureType.Normal:
			if (!_isSplatmap)
			{
				return "_n";
			}
			return "normal";
		case ETextureType.Specular:
			if (!_isSplatmap)
			{
				return "_s";
			}
			return "smoothAO";
		default:
			throw new ArgumentOutOfRangeException("_type", _type, null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetFileSuffixForQuality(int _quality, bool _isSplatmap)
	{
		if (!_isSplatmap)
		{
			return "_" + _quality;
		}
		if (_quality != 0)
		{
			return "_" + _quality;
		}
		return "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Unload(ref Texture tex)
	{
		if ((bool)tex)
		{
			Log.Out("Unload {0}", tex);
			Resources.UnloadAsset(tex);
			LoadManager.ReleaseAddressable(tex);
			tex = null;
		}
	}

	public void SetTextureFilter(int _index, int anisoLevel)
	{
		if (IsSplatmap(_index) || bTextureArray)
		{
			SetAF(TexDiffuse, anisoLevel);
			SetAF(TexNormal, anisoLevel);
			SetAF(TexSpecular, anisoLevel);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetAF(Texture tex, int anisoLevel)
	{
		if ((bool)tex)
		{
			tex.anisoLevel = anisoLevel;
		}
	}
}
