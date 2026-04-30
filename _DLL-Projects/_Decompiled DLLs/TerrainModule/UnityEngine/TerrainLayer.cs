using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine;

[StructLayout(LayoutKind.Sequential)]
[NativeHeader("TerrainScriptingClasses.h")]
[NativeHeader("Modules/Terrain/Public/TerrainLayerScriptingInterface.h")]
[UsedByNativeCode]
public sealed class TerrainLayer : Object
{
	public extern Texture2D diffuseTexture
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern Texture2D normalMapTexture
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern Texture2D maskMapTexture
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public Vector2 tileSize
	{
		get
		{
			get_tileSize_Injected(out var ret);
			return ret;
		}
		set
		{
			set_tileSize_Injected(ref value);
		}
	}

	public Vector2 tileOffset
	{
		get
		{
			get_tileOffset_Injected(out var ret);
			return ret;
		}
		set
		{
			set_tileOffset_Injected(ref value);
		}
	}

	[NativeProperty("SpecularColor")]
	public Color specular
	{
		get
		{
			get_specular_Injected(out var ret);
			return ret;
		}
		set
		{
			set_specular_Injected(ref value);
		}
	}

	public extern float metallic
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float smoothness
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float normalScale
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public Vector4 diffuseRemapMin
	{
		get
		{
			get_diffuseRemapMin_Injected(out var ret);
			return ret;
		}
		set
		{
			set_diffuseRemapMin_Injected(ref value);
		}
	}

	public Vector4 diffuseRemapMax
	{
		get
		{
			get_diffuseRemapMax_Injected(out var ret);
			return ret;
		}
		set
		{
			set_diffuseRemapMax_Injected(ref value);
		}
	}

	public Vector4 maskMapRemapMin
	{
		get
		{
			get_maskMapRemapMin_Injected(out var ret);
			return ret;
		}
		set
		{
			set_maskMapRemapMin_Injected(ref value);
		}
	}

	public Vector4 maskMapRemapMax
	{
		get
		{
			get_maskMapRemapMax_Injected(out var ret);
			return ret;
		}
		set
		{
			set_maskMapRemapMax_Injected(ref value);
		}
	}

	public TerrainLayer()
	{
		Internal_Create(this);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainLayerScriptingInterface::Create")]
	private static extern void Internal_Create([Writable] TerrainLayer layer);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_tileSize_Injected(out Vector2 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_tileSize_Injected(ref Vector2 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_tileOffset_Injected(out Vector2 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_tileOffset_Injected(ref Vector2 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_specular_Injected(out Color ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_specular_Injected(ref Color value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_diffuseRemapMin_Injected(out Vector4 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_diffuseRemapMin_Injected(ref Vector4 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_diffuseRemapMax_Injected(out Vector4 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_diffuseRemapMax_Injected(ref Vector4 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_maskMapRemapMin_Injected(out Vector4 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_maskMapRemapMin_Injected(ref Vector4 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_maskMapRemapMax_Injected(out Vector4 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_maskMapRemapMax_Injected(ref Vector4 value);
}
