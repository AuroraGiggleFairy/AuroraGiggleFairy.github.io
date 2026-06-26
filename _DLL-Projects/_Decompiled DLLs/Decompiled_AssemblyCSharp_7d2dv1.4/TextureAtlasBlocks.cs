using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TextureAtlasBlocks : TextureAtlas
{
	public enum WrapMode
	{
		Mirror,
		Wrap,
		TransparentEdges
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct BlockUVRect(int _textureId, int _x, int _y, int _width, int _height, int _blocksW, int _blocksH, Color _color, bool _bGlobalUV)
	{
		public int textureId = _textureId;

		public int x = _x;

		public int y = _y;

		public int width = _width;

		public int height = _height;

		public int blocksW = _blocksW;

		public int blocksH = _blocksH;

		public Color color = _color;

		public bool bGlobalUV = _bGlobalUV;

		public override string ToString()
		{
			return "texId=" + textureId + " x/y=" + x + "/" + y + " w/h=" + width + "/" + height + " block W/H=" + blocksW + "/" + blocksH;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class BlocksTexture
	{
		public List<BlockUVRect> blocks = new List<BlockUVRect>();

		public string textureName;

		public Texture2D diffuse;

		public Texture2D normal;

		public Texture2D specular;

		public Texture2D height;

		public WrapMode wrapMode;

		public int diffuseW;

		public int diffuseH;

		public Color color;

		public override string ToString()
		{
			string text = "diffuse=" + diffuse?.ToString() + " normal=" + normal?.ToString() + " count=" + blocks.Count + "\n";
			for (int i = 0; i < blocks.Count; i++)
			{
				BlockUVRect blockUVRect = blocks[i];
				string text2 = text;
				BlockUVRect blockUVRect2 = blockUVRect;
				text = text2 + blockUVRect2.ToString() + "\n";
			}
			return text;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cTextureArraySize = 512;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int cTextureBorder = 34;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int cTextureSize = 8192;

	public override bool LoadTextureAtlas(int _idx, MeshDescriptionCollection _tac, bool _bLoadTextures)
	{
		try
		{
			XElement root = new XmlFile(_tac.meshes[_idx].MetaData).XmlDoc.Root;
			int num = 0;
			foreach (XElement item in root.Elements("uv"))
			{
				num = Math.Max(num, int.Parse(item.GetAttribute("id")));
			}
			uvMapping = new UVRectTiling[num + 1];
			foreach (XElement item2 in root.Elements("uv"))
			{
				int num2 = int.Parse(item2.GetAttribute("id"));
				UVRectTiling uVRectTiling = default(UVRectTiling);
				uVRectTiling.FromXML(item2);
				uvMapping[num2] = uVRectTiling;
			}
		}
		catch (Exception ex)
		{
			Log.Error("Parsing file xml file for texture atlas " + _tac.name + " (" + _idx + "): " + ex.Message + ")");
			Log.Exception(ex);
			Log.Error("Loading of textures aborted due to errors!");
			return false;
		}
		base.LoadTextureAtlas(_idx, _tac, _bLoadTextures);
		return true;
	}

	public override void Cleanup()
	{
		if (diffuseTexture != null)
		{
			Resources.UnloadAsset(diffuseTexture);
			diffuseTexture = null;
		}
		if (normalTexture != null)
		{
			Resources.UnloadAsset(normalTexture);
			normalTexture = null;
		}
		if (maskTexture != null)
		{
			Resources.UnloadAsset(maskTexture);
			maskTexture = null;
		}
		if (maskNormalTexture != null)
		{
			Resources.UnloadAsset(maskNormalTexture);
			maskNormalTexture = null;
		}
		if (emissionTexture != null)
		{
			Resources.UnloadAsset(emissionTexture);
			emissionTexture = null;
		}
		if (specularTexture != null)
		{
			Resources.UnloadAsset(specularTexture);
			specularTexture = null;
		}
		if (heightTexture != null)
		{
			Resources.UnloadAsset(heightTexture);
			heightTexture = null;
		}
		if (occlusionTexture != null)
		{
			Resources.UnloadAsset(occlusionTexture);
			occlusionTexture = null;
		}
	}
}
