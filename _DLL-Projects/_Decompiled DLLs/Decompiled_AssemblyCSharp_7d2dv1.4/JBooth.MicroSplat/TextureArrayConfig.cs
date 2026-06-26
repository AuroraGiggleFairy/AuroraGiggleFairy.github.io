using System;
using System.Collections.Generic;
using UnityEngine;

namespace JBooth.MicroSplat;

[CreateAssetMenu(menuName = "MicroSplat/Texture Array Config", order = 1)]
[ExecuteInEditMode]
public class TextureArrayConfig : ScriptableObject
{
	public enum AllTextureChannel
	{
		R,
		G,
		B,
		A,
		Custom
	}

	public enum TextureChannel
	{
		R,
		G,
		B,
		A
	}

	public enum Compression
	{
		AutomaticCompressed,
		ForceDXT,
		ForcePVR,
		ForceETC2,
		ForceASTC,
		ForceCrunch,
		Uncompressed
	}

	public enum TextureSize
	{
		k4096 = 4096,
		k2048 = 2048,
		k1024 = 1024,
		k512 = 512,
		k256 = 256,
		k128 = 128,
		k64 = 64,
		k32 = 32
	}

	[Serializable]
	public class TextureArraySettings
	{
		public TextureSize textureSize;

		public Compression compression;

		public FilterMode filterMode;

		[Range(0f, 16f)]
		public int Aniso = 1;

		public TextureArraySettings(TextureSize s, Compression c, FilterMode f, int a = 1)
		{
			textureSize = s;
			compression = c;
			filterMode = f;
			Aniso = a;
		}
	}

	public enum PBRWorkflow
	{
		Metallic,
		Specular
	}

	public enum PackingMode
	{
		Fastest,
		Quality
	}

	public enum SourceTextureSize
	{
		Unchanged = 0,
		k32 = 0x20,
		k256 = 0x100
	}

	public enum TextureMode
	{
		Basic,
		PBR
	}

	public enum ClusterMode
	{
		None,
		TwoVariations,
		ThreeVariations
	}

	[Serializable]
	public class TextureArrayGroup
	{
		public TextureArraySettings diffuseSettings = new TextureArraySettings(TextureSize.k1024, Compression.AutomaticCompressed, FilterMode.Bilinear);

		public TextureArraySettings normalSettings = new TextureArraySettings(TextureSize.k1024, Compression.AutomaticCompressed, FilterMode.Trilinear);

		public TextureArraySettings smoothSettings = new TextureArraySettings(TextureSize.k1024, Compression.AutomaticCompressed, FilterMode.Bilinear);

		public TextureArraySettings antiTileSettings = new TextureArraySettings(TextureSize.k1024, Compression.AutomaticCompressed, FilterMode.Bilinear);

		public TextureArraySettings emissiveSettings = new TextureArraySettings(TextureSize.k1024, Compression.AutomaticCompressed, FilterMode.Bilinear);

		public TextureArraySettings specularSettings = new TextureArraySettings(TextureSize.k1024, Compression.AutomaticCompressed, FilterMode.Bilinear);

		public TextureArraySettings traxDiffuseSettings = new TextureArraySettings(TextureSize.k1024, Compression.AutomaticCompressed, FilterMode.Bilinear);

		public TextureArraySettings traxNormalSettings = new TextureArraySettings(TextureSize.k1024, Compression.AutomaticCompressed, FilterMode.Bilinear);

		public TextureArraySettings decalSplatSettings = new TextureArraySettings(TextureSize.k1024, Compression.AutomaticCompressed, FilterMode.Bilinear);
	}

	[Serializable]
	public class PlatformTextureOverride
	{
		public TextureArrayGroup settings = new TextureArrayGroup();
	}

	[Serializable]
	public class TextureEntry
	{
		public Texture2D diffuse;

		public Texture2D height;

		public TextureChannel heightChannel = TextureChannel.G;

		public Texture2D normal;

		public Texture2D smoothness;

		public TextureChannel smoothnessChannel = TextureChannel.G;

		public bool isRoughness;

		public Texture2D ao;

		public TextureChannel aoChannel = TextureChannel.G;

		public Texture2D emis;

		public Texture2D metal;

		public TextureChannel metalChannel = TextureChannel.G;

		public Texture2D specular;

		public Texture2D noiseNormal;

		public Texture2D detailNoise;

		public TextureChannel detailChannel = TextureChannel.G;

		public Texture2D distanceNoise;

		public TextureChannel distanceChannel = TextureChannel.G;

		public Texture2D traxDiffuse;

		public Texture2D traxHeight;

		public TextureChannel traxHeightChannel = TextureChannel.G;

		public Texture2D traxNormal;

		public Texture2D traxSmoothness;

		public TextureChannel traxSmoothnessChannel = TextureChannel.G;

		public bool traxIsRoughness;

		public Texture2D traxAO;

		public TextureChannel traxAOChannel = TextureChannel.G;

		public Texture2D splat;

		public void Reset()
		{
			diffuse = null;
			height = null;
			normal = null;
			smoothness = null;
			specular = null;
			ao = null;
			isRoughness = false;
			detailNoise = null;
			distanceNoise = null;
			metal = null;
			emis = null;
			heightChannel = TextureChannel.G;
			smoothnessChannel = TextureChannel.G;
			aoChannel = TextureChannel.G;
			distanceChannel = TextureChannel.G;
			detailChannel = TextureChannel.G;
			traxDiffuse = null;
			traxNormal = null;
			traxHeight = null;
			traxSmoothness = null;
			traxAO = null;
			traxHeightChannel = TextureChannel.G;
			traxSmoothnessChannel = TextureChannel.G;
			traxAOChannel = TextureChannel.G;
			splat = null;
		}

		public bool HasTextures(PBRWorkflow wf)
		{
			if (wf == PBRWorkflow.Specular)
			{
				if (!(diffuse != null) && !(height != null) && !(normal != null) && !(smoothness != null) && !(specular != null))
				{
					return ao != null;
				}
				return true;
			}
			if (!(diffuse != null) && !(height != null) && !(normal != null) && !(smoothness != null) && !(metal != null))
			{
				return ao != null;
			}
			return true;
		}
	}

	public bool diffuseIsLinear;

	[HideInInspector]
	public bool antiTileArray;

	[HideInInspector]
	public bool emisMetalArray;

	public bool traxArray;

	[HideInInspector]
	public TextureMode textureMode = TextureMode.PBR;

	[HideInInspector]
	public ClusterMode clusterMode;

	[HideInInspector]
	public PackingMode packingMode;

	[HideInInspector]
	public PBRWorkflow pbrWorkflow;

	[HideInInspector]
	public int hash;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<TextureArrayConfig> sAllConfigs = new List<TextureArrayConfig>();

	[HideInInspector]
	public Texture2DArray splatArray;

	[HideInInspector]
	public Texture2DArray diffuseArray;

	[HideInInspector]
	public Texture2DArray normalSAOArray;

	[HideInInspector]
	public Texture2DArray smoothAOArray;

	[HideInInspector]
	public Texture2DArray specularArray;

	[HideInInspector]
	public Texture2DArray diffuseArray2;

	[HideInInspector]
	public Texture2DArray normalSAOArray2;

	[HideInInspector]
	public Texture2DArray smoothAOArray2;

	[HideInInspector]
	public Texture2DArray specularArray2;

	[HideInInspector]
	public Texture2DArray diffuseArray3;

	[HideInInspector]
	public Texture2DArray normalSAOArray3;

	[HideInInspector]
	public Texture2DArray smoothAOArray3;

	[HideInInspector]
	public Texture2DArray specularArray3;

	[HideInInspector]
	public Texture2DArray emisArray;

	[HideInInspector]
	public Texture2DArray emisArray2;

	[HideInInspector]
	public Texture2DArray emisArray3;

	public TextureArrayGroup defaultTextureSettings = new TextureArrayGroup();

	public List<PlatformTextureOverride> platformOverrides = new List<PlatformTextureOverride>();

	public SourceTextureSize sourceTextureSize;

	[HideInInspector]
	public AllTextureChannel allTextureChannelHeight = AllTextureChannel.G;

	[HideInInspector]
	public AllTextureChannel allTextureChannelSmoothness = AllTextureChannel.G;

	[HideInInspector]
	public AllTextureChannel allTextureChannelAO = AllTextureChannel.G;

	[HideInInspector]
	public List<TextureEntry> sourceTextures = new List<TextureEntry>();

	[HideInInspector]
	public List<TextureEntry> sourceTextures2 = new List<TextureEntry>();

	[HideInInspector]
	public List<TextureEntry> sourceTextures3 = new List<TextureEntry>();

	public bool IsScatter()
	{
		return false;
	}

	public bool IsDecal()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		sAllConfigs.Add(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		sAllConfigs.Remove(this);
	}

	public static TextureArrayConfig FindConfig(Texture2DArray diffuse)
	{
		for (int i = 0; i < sAllConfigs.Count; i++)
		{
			if (sAllConfigs[i].diffuseArray == diffuse)
			{
				return sAllConfigs[i];
			}
		}
		return null;
	}
}
