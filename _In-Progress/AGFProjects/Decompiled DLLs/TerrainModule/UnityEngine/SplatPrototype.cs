using System;
using System.Runtime.InteropServices;
using UnityEngine.Scripting;

namespace UnityEngine;

[StructLayout(LayoutKind.Sequential)]
[UsedByNativeCode]
[Obsolete("SplatPrototype is obsolete. Use TerrainLayer instead.", false)]
public sealed class SplatPrototype
{
	internal Texture2D m_Texture;

	internal Texture2D m_NormalMap;

	internal Vector2 m_TileSize = new Vector2(15f, 15f);

	internal Vector2 m_TileOffset = new Vector2(0f, 0f);

	internal Vector4 m_SpecularMetallic = new Vector4(0f, 0f, 0f, 0f);

	internal float m_Smoothness = 0f;

	public Texture2D texture
	{
		get
		{
			return m_Texture;
		}
		set
		{
			m_Texture = value;
		}
	}

	public Texture2D normalMap
	{
		get
		{
			return m_NormalMap;
		}
		set
		{
			m_NormalMap = value;
		}
	}

	public Vector2 tileSize
	{
		get
		{
			return m_TileSize;
		}
		set
		{
			m_TileSize = value;
		}
	}

	public Vector2 tileOffset
	{
		get
		{
			return m_TileOffset;
		}
		set
		{
			m_TileOffset = value;
		}
	}

	public Color specular
	{
		get
		{
			return new Color(m_SpecularMetallic.x, m_SpecularMetallic.y, m_SpecularMetallic.z);
		}
		set
		{
			m_SpecularMetallic.x = value.r;
			m_SpecularMetallic.y = value.g;
			m_SpecularMetallic.z = value.b;
		}
	}

	public float metallic
	{
		get
		{
			return m_SpecularMetallic.w;
		}
		set
		{
			m_SpecularMetallic.w = value;
		}
	}

	public float smoothness
	{
		get
		{
			return m_Smoothness;
		}
		set
		{
			m_Smoothness = value;
		}
	}
}
