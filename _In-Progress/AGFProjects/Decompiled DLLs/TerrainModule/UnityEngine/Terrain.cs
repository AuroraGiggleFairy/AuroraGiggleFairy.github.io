using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace UnityEngine;

[UsedByNativeCode]
[StaticAccessor("GetITerrainManager()", StaticAccessorType.Arrow)]
[NativeHeader("TerrainScriptingClasses.h")]
[NativeHeader("Runtime/Interfaces/ITerrainManager.h")]
[NativeHeader("Modules/Terrain/Public/Terrain.h")]
public sealed class Terrain : Behaviour
{
	[Obsolete("Enum type MaterialType is not used any more.", false)]
	public enum MaterialType
	{
		BuiltInStandard,
		BuiltInLegacyDiffuse,
		BuiltInLegacySpecular,
		Custom
	}

	public extern TerrainData terrainData
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float treeDistance
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float treeBillboardDistance
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float treeCrossFadeLength
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int treeMaximumFullLODCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float detailObjectDistance
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float detailObjectDensity
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float heightmapPixelError
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int heightmapMaximumLOD
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int heightmapMinimumLODSimplification
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float basemapDistance
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("StaticLightmapIndexInt")]
	public extern int lightmapIndex
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("DynamicLightmapIndexInt")]
	public extern int realtimeLightmapIndex
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("StaticLightmapST")]
	public Vector4 lightmapScaleOffset
	{
		get
		{
			get_lightmapScaleOffset_Injected(out var ret);
			return ret;
		}
		set
		{
			set_lightmapScaleOffset_Injected(ref value);
		}
	}

	[NativeProperty("DynamicLightmapST")]
	public Vector4 realtimeLightmapScaleOffset
	{
		get
		{
			get_realtimeLightmapScaleOffset_Injected(out var ret);
			return ret;
		}
		set
		{
			set_realtimeLightmapScaleOffset_Injected(ref value);
		}
	}

	[NativeProperty("FreeUnusedRenderingResourcesObsolete")]
	[Obsolete("Terrain.freeUnusedRenderingResources is obsolete; use keepUnusedRenderingResources instead.")]
	public extern bool freeUnusedRenderingResources
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("KeepUnusedRenderingResources")]
	public extern bool keepUnusedRenderingResources
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern ShadowCastingMode shadowCastingMode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern ReflectionProbeUsage reflectionProbeUsage
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern Material materialTemplate
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool drawHeightmap
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool allowAutoConnect
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int groupingID
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool drawInstanced
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool enableHeightmapRayTracing
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern RenderTexture normalmapTexture
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod("TryGetNormalMapTexture")]
		get;
	}

	public extern bool drawTreesAndFoliage
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public Vector3 patchBoundsMultiplier
	{
		get
		{
			get_patchBoundsMultiplier_Injected(out var ret);
			return ret;
		}
		set
		{
			set_patchBoundsMultiplier_Injected(ref value);
		}
	}

	public extern float treeLODBiasMultiplier
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool collectDetailPatches
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool ignoreQualitySettings
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern TerrainRenderFlags editorRenderFlags
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern TreeMotionVectorModeOverride treeMotionVectorModeOverride
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool preserveTreePrototypeLayers
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[StaticAccessor("Terrain", StaticAccessorType.DoubleColon)]
	public static extern GraphicsFormat heightmapFormat
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public static TextureFormat heightmapTextureFormat => GraphicsFormatUtility.GetTextureFormat(heightmapFormat);

	public static RenderTextureFormat heightmapRenderTextureFormat => GraphicsFormatUtility.GetRenderTextureFormat(heightmapFormat);

	[StaticAccessor("Terrain", StaticAccessorType.DoubleColon)]
	public static extern GraphicsFormat normalmapFormat
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public static TextureFormat normalmapTextureFormat => GraphicsFormatUtility.GetTextureFormat(normalmapFormat);

	public static RenderTextureFormat normalmapRenderTextureFormat => GraphicsFormatUtility.GetRenderTextureFormat(normalmapFormat);

	[StaticAccessor("Terrain", StaticAccessorType.DoubleColon)]
	public static extern GraphicsFormat holesFormat
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public static RenderTextureFormat holesRenderTextureFormat => GraphicsFormatUtility.GetRenderTextureFormat(holesFormat);

	[StaticAccessor("Terrain", StaticAccessorType.DoubleColon)]
	public static extern GraphicsFormat compressedHolesFormat
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public static TextureFormat compressedHolesTextureFormat => GraphicsFormatUtility.GetTextureFormat(compressedHolesFormat);

	public static extern Terrain activeTerrain
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[NativeProperty("ActiveTerrainsScriptingArray")]
	public static extern Terrain[] activeTerrains
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern Terrain leftNeighbor
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern Terrain rightNeighbor
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern Terrain topNeighbor
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern Terrain bottomNeighbor
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern uint renderingLayerMask
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("splatmapDistance is deprecated, please use basemapDistance instead. (UnityUpgradable) -> basemapDistance", true)]
	public float splatmapDistance
	{
		get
		{
			return basemapDistance;
		}
		set
		{
			basemapDistance = value;
		}
	}

	[Obsolete("castShadows is deprecated, please use shadowCastingMode instead.")]
	public bool castShadows
	{
		get
		{
			return shadowCastingMode != ShadowCastingMode.Off;
		}
		set
		{
			shadowCastingMode = (value ? ShadowCastingMode.TwoSided : ShadowCastingMode.Off);
		}
	}

	[Obsolete("Property materialType is not used any more. Set materialTemplate directly.", false)]
	public MaterialType materialType
	{
		get
		{
			return MaterialType.Custom;
		}
		set
		{
		}
	}

	[Obsolete("Property legacySpecular is not used any more. Set materialTemplate directly.", false)]
	public Color legacySpecular
	{
		get
		{
			return Color.gray;
		}
		set
		{
		}
	}

	[Obsolete("Property legacyShininess is not used any more. Set materialTemplate directly.", false)]
	public float legacyShininess
	{
		get
		{
			return 5f / 64f;
		}
		set
		{
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern bool GetKeepUnusedCameraRenderingResources(int cameraInstanceID);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetKeepUnusedCameraRenderingResources(int cameraInstanceID, bool keepUnused);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void GetClosestReflectionProbes(List<ReflectionProbeBlendInfo> result);

	public float SampleHeight(Vector3 worldPosition)
	{
		return SampleHeight_Injected(ref worldPosition);
	}

	public void AddTreeInstance(TreeInstance instance)
	{
		AddTreeInstance_Injected(ref instance);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetNeighbors(Terrain left, Terrain top, Terrain right, Terrain bottom);

	public Vector3 GetPosition()
	{
		GetPosition_Injected(out var ret);
		return ret;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void Flush();

	internal void RemoveTrees(Vector2 position, float radius, int prototypeIndex)
	{
		RemoveTrees_Injected(ref position, radius, prototypeIndex);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("CopySplatMaterialCustomProps")]
	public extern void SetSplatMaterialPropertyBlock(MaterialPropertyBlock properties);

	public void GetSplatMaterialPropertyBlock(MaterialPropertyBlock dest)
	{
		if (dest == null)
		{
			throw new ArgumentNullException("dest");
		}
		Internal_GetSplatMaterialPropertyBlock(dest);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("GetSplatMaterialCustomProps")]
	private extern void Internal_GetSplatMaterialPropertyBlock(MaterialPropertyBlock dest);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void SetConnectivityDirty();

	public static void GetActiveTerrains(List<Terrain> terrainList)
	{
		Internal_FillActiveTerrainList(terrainList);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Internal_FillActiveTerrainList([NotNull("ArgumentNullException")] object terrainList);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[UsedByNativeCode]
	public static extern GameObject CreateTerrainGameObject(TerrainData assignTerrain);

	[Obsolete("Use TerrainData.SyncHeightmap to notify all Terrain instances using the TerrainData.", false)]
	public void ApplyDelayedHeightmapModification()
	{
		terrainData?.SyncHeightmap();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_lightmapScaleOffset_Injected(out Vector4 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_lightmapScaleOffset_Injected(ref Vector4 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_realtimeLightmapScaleOffset_Injected(out Vector4 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_realtimeLightmapScaleOffset_Injected(ref Vector4 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_patchBoundsMultiplier_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_patchBoundsMultiplier_Injected(ref Vector3 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern float SampleHeight_Injected(ref Vector3 worldPosition);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void AddTreeInstance_Injected(ref TreeInstance instance);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetPosition_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void RemoveTrees_Injected(ref Vector2 position, float radius, int prototypeIndex);
}
