using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace UnityEngine;

[NativeHeader("TerrainScriptingClasses.h")]
[NativeHeader("Modules/Terrain/Public/TerrainDataScriptingInterface.h")]
[UsedByNativeCode]
public sealed class TerrainData : Object
{
	private enum BoundaryValueType
	{
		MaxHeightmapRes,
		MinDetailResPerPatch,
		MaxDetailResPerPatch,
		MaxDetailPatchCount,
		MaxCoveragePerRes,
		MinAlphamapRes,
		MaxAlphamapRes,
		MinBaseMapRes,
		MaxBaseMapRes
	}

	private const string k_ScriptingInterfaceName = "TerrainDataScriptingInterface";

	private const string k_ScriptingInterfacePrefix = "TerrainDataScriptingInterface::";

	private const string k_HeightmapPrefix = "GetHeightmap().";

	private const string k_DetailDatabasePrefix = "GetDetailDatabase().";

	private const string k_TreeDatabasePrefix = "GetTreeDatabase().";

	private const string k_SplatDatabasePrefix = "GetSplatDatabase().";

	internal static readonly int k_MaximumResolution = GetBoundaryValue(BoundaryValueType.MaxHeightmapRes);

	internal static readonly int k_MinimumDetailResolutionPerPatch = GetBoundaryValue(BoundaryValueType.MinDetailResPerPatch);

	internal static readonly int k_MaximumDetailResolutionPerPatch = GetBoundaryValue(BoundaryValueType.MaxDetailResPerPatch);

	internal static readonly int k_MaximumDetailPatchCount = GetBoundaryValue(BoundaryValueType.MaxDetailPatchCount);

	internal static readonly int k_MinimumAlphamapResolution = GetBoundaryValue(BoundaryValueType.MinAlphamapRes);

	internal static readonly int k_MaximumAlphamapResolution = GetBoundaryValue(BoundaryValueType.MaxAlphamapRes);

	internal static readonly int k_MinimumBaseMapResolution = GetBoundaryValue(BoundaryValueType.MinBaseMapRes);

	internal static readonly int k_MaximumBaseMapResolution = GetBoundaryValue(BoundaryValueType.MaxBaseMapRes);

	[Obsolete("Please use heightmapResolution instead. (UnityUpgradable) -> heightmapResolution", false)]
	public int heightmapWidth => heightmapResolution;

	[Obsolete("Please use heightmapResolution instead. (UnityUpgradable) -> heightmapResolution", false)]
	public int heightmapHeight => heightmapResolution;

	public extern RenderTexture heightmapTexture
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetHeightmap().GetHeightmapTexture")]
		get;
	}

	public int heightmapResolution
	{
		get
		{
			return internalHeightmapResolution;
		}
		set
		{
			int num = value;
			if (value < 0 || value > k_MaximumResolution)
			{
				Debug.LogWarning("heightmapResolution is clamped to the range of [0, " + k_MaximumResolution + "].");
				num = Math.Min(k_MaximumResolution, Math.Max(value, 0));
			}
			internalHeightmapResolution = num;
		}
	}

	private extern int internalHeightmapResolution
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetHeightmap().GetResolution")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetHeightmap().SetResolution")]
		set;
	}

	public Vector3 heightmapScale
	{
		[NativeName("GetHeightmap().GetScale")]
		get
		{
			get_heightmapScale_Injected(out var ret);
			return ret;
		}
	}

	public Texture holesTexture
	{
		get
		{
			if (IsHolesTextureCompressed())
			{
				return GetCompressedHolesTexture();
			}
			return GetHolesTexture();
		}
	}

	public extern bool enableHolesTextureCompression
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetHeightmap().GetEnableHolesTextureCompression")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetHeightmap().SetEnableHolesTextureCompression")]
		set;
	}

	internal RenderTexture holesRenderTexture => GetHolesTexture();

	public int holesResolution => heightmapResolution - 1;

	public Vector3 size
	{
		[NativeName("GetHeightmap().GetSize")]
		get
		{
			get_size_Injected(out var ret);
			return ret;
		}
		[NativeName("GetHeightmap().SetSize")]
		set
		{
			set_size_Injected(ref value);
		}
	}

	public Bounds bounds
	{
		[NativeName("GetHeightmap().CalculateBounds")]
		get
		{
			get_bounds_Injected(out var ret);
			return ret;
		}
	}

	[Obsolete("Terrain thickness is no longer required by the physics engine. Set appropriate continuous collision detection modes to fast moving bodies.")]
	public float thickness
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public extern float wavingGrassStrength
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetDetailDatabase().GetWavingGrassStrength")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("TerrainDataScriptingInterface::SetWavingGrassStrength", HasExplicitThis = true)]
		set;
	}

	public extern float wavingGrassAmount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetDetailDatabase().GetWavingGrassAmount")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("TerrainDataScriptingInterface::SetWavingGrassAmount", HasExplicitThis = true)]
		set;
	}

	public extern float wavingGrassSpeed
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetDetailDatabase().GetWavingGrassSpeed")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("TerrainDataScriptingInterface::SetWavingGrassSpeed", HasExplicitThis = true)]
		set;
	}

	public Color wavingGrassTint
	{
		[NativeName("GetDetailDatabase().GetWavingGrassTint")]
		get
		{
			get_wavingGrassTint_Injected(out var ret);
			return ret;
		}
		[FreeFunction("TerrainDataScriptingInterface::SetWavingGrassTint", HasExplicitThis = true)]
		set
		{
			set_wavingGrassTint_Injected(ref value);
		}
	}

	public extern int detailWidth
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetDetailDatabase().GetWidth")]
		get;
	}

	public extern int detailHeight
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetDetailDatabase().GetHeight")]
		get;
	}

	public extern int maxDetailScatterPerRes
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetDetailDatabase().GetMaximumScatterPerRes")]
		get;
	}

	public extern int detailPatchCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetDetailDatabase().GetPatchCount")]
		get;
	}

	public extern int detailResolution
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetDetailDatabase().GetResolution")]
		get;
	}

	public extern int detailResolutionPerPatch
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetDetailDatabase().GetResolutionPerPatch")]
		get;
	}

	public extern DetailScatterMode detailScatterMode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetDetailDatabase().GetDetailScatterMode")]
		get;
	}

	public extern DetailPrototype[] detailPrototypes
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("TerrainDataScriptingInterface::GetDetailPrototypes", HasExplicitThis = true)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("TerrainDataScriptingInterface::SetDetailPrototypes", HasExplicitThis = true, ThrowsException = true)]
		[param: Unmarshalled]
		set;
	}

	public TreeInstance[] treeInstances
	{
		get
		{
			return Internal_GetTreeInstances();
		}
		set
		{
			SetTreeInstances(value, snapToHeightmap: false);
		}
	}

	public extern int treeInstanceCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetTreeDatabase().GetInstances().size")]
		get;
	}

	public extern TreePrototype[] treePrototypes
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("TerrainDataScriptingInterface::GetTreePrototypes", HasExplicitThis = true)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("TerrainDataScriptingInterface::SetTreePrototypes", HasExplicitThis = true, ThrowsException = true)]
		[param: Unmarshalled]
		set;
	}

	public extern int alphamapLayers
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetSplatDatabase().GetSplatCount")]
		get;
	}

	public int alphamapResolution
	{
		get
		{
			return Internal_alphamapResolution;
		}
		set
		{
			int internal_alphamapResolution = value;
			if (value < k_MinimumAlphamapResolution || value > k_MaximumAlphamapResolution)
			{
				Debug.LogWarning("alphamapResolution is clamped to the range of [" + k_MinimumAlphamapResolution + ", " + k_MaximumAlphamapResolution + "].");
				internal_alphamapResolution = Math.Min(k_MaximumAlphamapResolution, Math.Max(value, k_MinimumAlphamapResolution));
			}
			Internal_alphamapResolution = internal_alphamapResolution;
		}
	}

	private extern int Internal_alphamapResolution
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetSplatDatabase().GetAlphamapResolution")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetSplatDatabase().SetAlphamapResolution")]
		set;
	}

	public int alphamapWidth => alphamapResolution;

	public int alphamapHeight => alphamapResolution;

	public int baseMapResolution
	{
		get
		{
			return Internal_baseMapResolution;
		}
		set
		{
			int internal_baseMapResolution = value;
			if (value < k_MinimumBaseMapResolution || value > k_MaximumBaseMapResolution)
			{
				Debug.LogWarning("baseMapResolution is clamped to the range of [" + k_MinimumBaseMapResolution + ", " + k_MaximumBaseMapResolution + "].");
				internal_baseMapResolution = Math.Min(k_MaximumBaseMapResolution, Math.Max(value, k_MinimumBaseMapResolution));
			}
			Internal_baseMapResolution = internal_baseMapResolution;
		}
	}

	private extern int Internal_baseMapResolution
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetSplatDatabase().GetBaseMapResolution")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetSplatDatabase().SetBaseMapResolution")]
		set;
	}

	public extern int alphamapTextureCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetSplatDatabase().GetAlphaTextureCount")]
		get;
	}

	public Texture2D[] alphamapTextures
	{
		get
		{
			Texture2D[] array = new Texture2D[alphamapTextureCount];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = GetAlphamapTexture(i);
			}
			return array;
		}
	}

	[Obsolete("TerrainData.splatPrototypes is obsolete. Use TerrainData.terrainLayers instead.", false)]
	public extern SplatPrototype[] splatPrototypes
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("TerrainDataScriptingInterface::GetSplatPrototypes", HasExplicitThis = true)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("TerrainDataScriptingInterface::SetSplatPrototypes", HasExplicitThis = true, ThrowsException = true)]
		[param: Unmarshalled]
		set;
	}

	public extern TerrainLayer[] terrainLayers
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("TerrainDataScriptingInterface::GetTerrainLayers", HasExplicitThis = true)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("TerrainDataScriptingInterface::SetTerrainLayers", HasExplicitThis = true)]
		[param: Unmarshalled]
		set;
	}

	internal extern TextureFormat atlasFormat
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeName("GetDetailDatabase().GetAtlasTexture()->GetTextureFormat")]
		get;
	}

	internal extern Terrain[] users
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	private static bool SupportsCopyTextureBetweenRTAndTexture => (SystemInfo.copyTextureSupport & (CopyTextureSupport.TextureToRT | CopyTextureSupport.RTToTexture)) == (CopyTextureSupport.TextureToRT | CopyTextureSupport.RTToTexture);

	public static string AlphamapTextureName => "alphamap";

	public static string HolesTextureName => "holes";

	[MethodImpl(MethodImplOptions.InternalCall)]
	[ThreadSafe]
	[StaticAccessor("TerrainDataScriptingInterface", StaticAccessorType.DoubleColon)]
	private static extern int GetBoundaryValue(BoundaryValueType type);

	public TerrainData()
	{
		Internal_Create(this);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::Create")]
	private static extern void Internal_Create([Writable] TerrainData terrainData);

	[Obsolete("Please use DirtyHeightmapRegion instead.", false)]
	public void UpdateDirtyRegion(int x, int y, int width, int height, bool syncHeightmapTextureImmediately)
	{
		DirtyHeightmapRegion(new RectInt(x, y, width, height), syncHeightmapTextureImmediately ? TerrainHeightmapSyncControl.HeightOnly : TerrainHeightmapSyncControl.None);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetHeightmap().IsHolesTextureCompressed")]
	internal extern bool IsHolesTextureCompressed();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetHeightmap().GetHolesTexture")]
	internal extern RenderTexture GetHolesTexture();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetHeightmap().GetCompressedHolesTexture")]
	internal extern Texture2D GetCompressedHolesTexture();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetHeightmap().GetHeight")]
	public extern float GetHeight(int x, int y);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetHeightmap().GetInterpolatedHeight")]
	public extern float GetInterpolatedHeight(float x, float y);

	public float[,] GetInterpolatedHeights(float xBase, float yBase, int xCount, int yCount, float xInterval, float yInterval)
	{
		if (xCount <= 0)
		{
			throw new ArgumentOutOfRangeException("xCount");
		}
		if (yCount <= 0)
		{
			throw new ArgumentOutOfRangeException("yCount");
		}
		float[,] array = new float[yCount, xCount];
		Internal_GetInterpolatedHeights(array, xCount, 0, 0, xBase, yBase, xCount, yCount, xInterval, yInterval);
		return array;
	}

	public void GetInterpolatedHeights(float[,] results, int resultXOffset, int resultYOffset, float xBase, float yBase, int xCount, int yCount, float xInterval, float yInterval)
	{
		if (results == null)
		{
			throw new ArgumentNullException("results");
		}
		if (xCount <= 0)
		{
			throw new ArgumentOutOfRangeException("xCount");
		}
		if (yCount <= 0)
		{
			throw new ArgumentOutOfRangeException("yCount");
		}
		if (resultXOffset < 0 || resultXOffset + xCount > results.GetLength(1))
		{
			throw new ArgumentOutOfRangeException("resultXOffset");
		}
		if (resultYOffset < 0 || resultYOffset + yCount > results.GetLength(0))
		{
			throw new ArgumentOutOfRangeException("resultYOffset");
		}
		Internal_GetInterpolatedHeights(results, results.GetLength(1), resultXOffset, resultYOffset, xBase, yBase, xCount, yCount, xInterval, yInterval);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::GetInterpolatedHeights", HasExplicitThis = true)]
	private extern void Internal_GetInterpolatedHeights([Unmarshalled] float[,] results, int resultXDimension, int resultXOffset, int resultYOffset, float xBase, float yBase, int xCount, int yCount, float xInterval, float yInterval);

	public float[,] GetHeights(int xBase, int yBase, int width, int height)
	{
		if (xBase < 0 || yBase < 0 || xBase + width < 0 || yBase + height < 0 || xBase + width > heightmapResolution || yBase + height > heightmapResolution)
		{
			throw new ArgumentException("Trying to access out-of-bounds terrain height information.");
		}
		return Internal_GetHeights(xBase, yBase, width, height);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::GetHeights", HasExplicitThis = true)]
	private extern float[,] Internal_GetHeights(int xBase, int yBase, int width, int height);

	public void SetHeights(int xBase, int yBase, float[,] heights)
	{
		if (heights == null)
		{
			throw new NullReferenceException();
		}
		if (xBase + heights.GetLength(1) > heightmapResolution || xBase + heights.GetLength(1) < 0 || yBase + heights.GetLength(0) < 0 || xBase < 0 || yBase < 0 || yBase + heights.GetLength(0) > heightmapResolution)
		{
			throw new ArgumentException(UnityString.Format("X or Y base out of bounds. Setting up to {0}x{1} while map size is {2}x{2}", xBase + heights.GetLength(1), yBase + heights.GetLength(0), heightmapResolution));
		}
		Internal_SetHeights(xBase, yBase, heights.GetLength(1), heights.GetLength(0), heights);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::SetHeights", HasExplicitThis = true)]
	private extern void Internal_SetHeights(int xBase, int yBase, int width, int height, float[,] heights);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::GetPatchMinMaxHeights", HasExplicitThis = true)]
	public extern PatchExtents[] GetPatchMinMaxHeights();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::OverrideMinMaxPatchHeights", HasExplicitThis = true)]
	public extern void OverrideMinMaxPatchHeights(PatchExtents[] minMaxHeights);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::GetMaximumHeightError", HasExplicitThis = true)]
	public extern float[] GetMaximumHeightError();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::OverrideMaximumHeightError", HasExplicitThis = true)]
	public extern void OverrideMaximumHeightError(float[] maxError);

	public void SetHeightsDelayLOD(int xBase, int yBase, float[,] heights)
	{
		if (heights == null)
		{
			throw new ArgumentNullException("heights");
		}
		int length = heights.GetLength(0);
		int length2 = heights.GetLength(1);
		if (xBase < 0 || xBase + length2 < 0 || xBase + length2 > heightmapResolution)
		{
			throw new ArgumentException(UnityString.Format("X out of bounds - trying to set {0}-{1} but the terrain ranges from 0-{2}", xBase, xBase + length2, heightmapResolution));
		}
		if (yBase < 0 || yBase + length < 0 || yBase + length > heightmapResolution)
		{
			throw new ArgumentException(UnityString.Format("Y out of bounds - trying to set {0}-{1} but the terrain ranges from 0-{2}", yBase, yBase + length, heightmapResolution));
		}
		Internal_SetHeightsDelayLOD(xBase, yBase, length2, length, heights);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::SetHeightsDelayLOD", HasExplicitThis = true)]
	private extern void Internal_SetHeightsDelayLOD(int xBase, int yBase, int width, int height, float[,] heights);

	public bool IsHole(int x, int y)
	{
		if (x < 0 || x >= holesResolution || y < 0 || y >= holesResolution)
		{
			throw new ArgumentException("Trying to access out-of-bounds terrain holes information.");
		}
		return Internal_IsHole(x, y);
	}

	public bool[,] GetHoles(int xBase, int yBase, int width, int height)
	{
		if (xBase < 0 || yBase < 0 || width <= 0 || height <= 0 || xBase + width > holesResolution || yBase + height > holesResolution)
		{
			throw new ArgumentException("Trying to access out-of-bounds terrain holes information.");
		}
		return Internal_GetHoles(xBase, yBase, width, height);
	}

	public void SetHoles(int xBase, int yBase, bool[,] holes)
	{
		if (holes == null)
		{
			throw new ArgumentNullException("holes");
		}
		int length = holes.GetLength(0);
		int length2 = holes.GetLength(1);
		if (xBase < 0 || xBase + length2 > holesResolution)
		{
			throw new ArgumentException(UnityString.Format("X out of bounds - trying to set {0}-{1} but the terrain ranges from 0-{2}", xBase, xBase + length2, holesResolution));
		}
		if (yBase < 0 || yBase + length > holesResolution)
		{
			throw new ArgumentException(UnityString.Format("Y out of bounds - trying to set {0}-{1} but the terrain ranges from 0-{2}", yBase, yBase + length, holesResolution));
		}
		Internal_SetHoles(xBase, yBase, holes.GetLength(1), holes.GetLength(0), holes);
	}

	public void SetHolesDelayLOD(int xBase, int yBase, bool[,] holes)
	{
		if (holes == null)
		{
			throw new ArgumentNullException("holes");
		}
		int length = holes.GetLength(0);
		int length2 = holes.GetLength(1);
		if (xBase < 0 || xBase + length2 > holesResolution)
		{
			throw new ArgumentException(UnityString.Format("X out of bounds - trying to set {0}-{1} but the terrain ranges from 0-{2}", xBase, xBase + length2, holesResolution));
		}
		if (yBase < 0 || yBase + length > holesResolution)
		{
			throw new ArgumentException(UnityString.Format("Y out of bounds - trying to set {0}-{1} but the terrain ranges from 0-{2}", yBase, yBase + length, holesResolution));
		}
		Internal_SetHolesDelayLOD(xBase, yBase, length2, length, holes);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::SetHoles", HasExplicitThis = true)]
	private extern void Internal_SetHoles(int xBase, int yBase, int width, int height, bool[,] holes);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::GetHoles", HasExplicitThis = true)]
	private extern bool[,] Internal_GetHoles(int xBase, int yBase, int width, int height);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::IsHole", HasExplicitThis = true)]
	private extern bool Internal_IsHole(int x, int y);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::SetHolesDelayLOD", HasExplicitThis = true)]
	private extern void Internal_SetHolesDelayLOD(int xBase, int yBase, int width, int height, bool[,] holes);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetHeightmap().GetSteepness")]
	public extern float GetSteepness(float x, float y);

	[NativeName("GetHeightmap().GetInterpolatedNormal")]
	public Vector3 GetInterpolatedNormal(float x, float y)
	{
		GetInterpolatedNormal_Injected(x, y, out var ret);
		return ret;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetHeightmap().GetAdjustedSize")]
	internal extern int GetAdjustedSize(int size);

	public void SetDetailResolution(int detailResolution, int resolutionPerPatch)
	{
		if (detailResolution < 0)
		{
			Debug.LogWarning("detailResolution must not be negative.");
			detailResolution = 0;
		}
		if (resolutionPerPatch < k_MinimumDetailResolutionPerPatch || resolutionPerPatch > k_MaximumDetailResolutionPerPatch)
		{
			Debug.LogWarning("resolutionPerPatch is clamped to the range of [" + k_MinimumDetailResolutionPerPatch + ", " + k_MaximumDetailResolutionPerPatch + "].");
			resolutionPerPatch = Math.Min(k_MaximumDetailResolutionPerPatch, Math.Max(resolutionPerPatch, k_MinimumDetailResolutionPerPatch));
		}
		int num = detailResolution / resolutionPerPatch;
		if (num > k_MaximumDetailPatchCount)
		{
			Debug.LogWarning("Patch count (detailResolution / resolutionPerPatch) is clamped to the range of [0, " + k_MaximumDetailPatchCount + "].");
			num = Math.Min(k_MaximumDetailPatchCount, Math.Max(num, 0));
		}
		Internal_SetDetailResolution(num, resolutionPerPatch);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetDetailDatabase().SetDetailResolution")]
	private extern void Internal_SetDetailResolution(int patchCount, int resolutionPerPatch);

	public void SetDetailScatterMode(DetailScatterMode scatterMode)
	{
		Internal_SetDetailScatterMode(scatterMode);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetDetailDatabase().SetDetailScatterMode")]
	private extern void Internal_SetDetailScatterMode(DetailScatterMode scatterMode);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetDetailDatabase().ResetDirtyDetails")]
	internal extern void ResetDirtyDetails();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::RefreshPrototypes", HasExplicitThis = true)]
	public extern void RefreshPrototypes();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::GetSupportedLayers", HasExplicitThis = true)]
	public extern int[] GetSupportedLayers(int xBase, int yBase, int totalWidth, int totalHeight);

	public int[] GetSupportedLayers(Vector2Int positionBase, Vector2Int size)
	{
		return GetSupportedLayers(positionBase.x, positionBase.y, size.x, size.y);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::GetDetailLayer", HasExplicitThis = true)]
	public extern int[,] GetDetailLayer(int xBase, int yBase, int width, int height, int layer);

	public int[,] GetDetailLayer(Vector2Int positionBase, Vector2Int size, int layer)
	{
		return GetDetailLayer(positionBase.x, positionBase.y, size.x, size.y, layer);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::ComputeDetailInstanceTransforms", HasExplicitThis = true)]
	public extern DetailInstanceTransform[] ComputeDetailInstanceTransforms(int patchX, int patchY, int layer, float density, out Bounds bounds);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::ComputeDetailCoverage", HasExplicitThis = true)]
	public extern float ComputeDetailCoverage(int detailPrototypeIndex);

	public void SetDetailLayer(int xBase, int yBase, int layer, int[,] details)
	{
		Internal_SetDetailLayer(xBase, yBase, details.GetLength(1), details.GetLength(0), layer, details);
	}

	public void SetDetailLayer(Vector2Int basePosition, int layer, int[,] details)
	{
		SetDetailLayer(basePosition.x, basePosition.y, layer, details);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::SetDetailLayer", HasExplicitThis = true)]
	private extern void Internal_SetDetailLayer(int xBase, int yBase, int totalWidth, int totalHeight, int detailIndex, int[,] data);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetTreeDatabase().GetInstances")]
	private extern TreeInstance[] Internal_GetTreeInstances();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::SetTreeInstances", HasExplicitThis = true)]
	public extern void SetTreeInstances([NotNull("ArgumentNullException")] TreeInstance[] instances, bool snapToHeightmap);

	public TreeInstance GetTreeInstance(int index)
	{
		if (index < 0 || index >= treeInstanceCount)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return Internal_GetTreeInstance(index);
	}

	[FreeFunction("TerrainDataScriptingInterface::GetTreeInstance", HasExplicitThis = true)]
	private TreeInstance Internal_GetTreeInstance(int index)
	{
		Internal_GetTreeInstance_Injected(index, out var ret);
		return ret;
	}

	[FreeFunction("TerrainDataScriptingInterface::SetTreeInstance", HasExplicitThis = true)]
	[NativeThrows]
	public void SetTreeInstance(int index, TreeInstance instance)
	{
		SetTreeInstance_Injected(index, ref instance);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetTreeDatabase().RemoveTreePrototype")]
	internal extern void RemoveTreePrototype(int index);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetDetailDatabase().RemoveDetailPrototype")]
	public extern void RemoveDetailPrototype(int index);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetTreeDatabase().NeedUpgradeScaledPrototypes")]
	internal extern bool NeedUpgradeScaledTreePrototypes();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::UpgradeScaledTreePrototype", HasExplicitThis = true)]
	internal extern void UpgradeScaledTreePrototype();

	public float[,,] GetAlphamaps(int x, int y, int width, int height)
	{
		if (x < 0 || y < 0 || width < 0 || height < 0)
		{
			throw new ArgumentException("Invalid argument for GetAlphaMaps");
		}
		return Internal_GetAlphamaps(x, y, width, height);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::GetAlphamaps", HasExplicitThis = true)]
	private extern float[,,] Internal_GetAlphamaps(int x, int y, int width, int height);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetSplatDatabase().GetAlphamapResolution")]
	[RequiredByNativeCode]
	internal extern float GetAlphamapResolutionInternal();

	public void SetAlphamaps(int x, int y, float[,,] map)
	{
		if (map.GetLength(2) != alphamapLayers)
		{
			throw new Exception(UnityString.Format("Float array size wrong (layers should be {0})", alphamapLayers));
		}
		Internal_SetAlphamaps(x, y, map.GetLength(1), map.GetLength(0), map);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("TerrainDataScriptingInterface::SetAlphamaps", HasExplicitThis = true)]
	private extern void Internal_SetAlphamaps(int x, int y, int width, int height, float[,,] map);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetSplatDatabase().SetBaseMapsDirty")]
	public extern void SetBaseMapDirty();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetSplatDatabase().GetAlphaTexture")]
	public extern Texture2D GetAlphamapTexture(int index);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetTreeDatabase().AddTree")]
	internal extern void AddTree(ref TreeInstance tree);

	[NativeName("GetTreeDatabase().RemoveTrees")]
	internal int RemoveTrees(Vector2 position, float radius, int prototypeIndex)
	{
		return RemoveTrees_Injected(ref position, radius, prototypeIndex);
	}

	[NativeName("GetHeightmap().CopyHeightmapFromActiveRenderTexture")]
	private void Internal_CopyActiveRenderTextureToHeightmap(RectInt rect, int destX, int destY, TerrainHeightmapSyncControl syncControl)
	{
		Internal_CopyActiveRenderTextureToHeightmap_Injected(ref rect, destX, destY, syncControl);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetHeightmap().DirtyHeightmapRegion")]
	private extern void Internal_DirtyHeightmapRegion(int x, int y, int width, int height, TerrainHeightmapSyncControl syncControl);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetHeightmap().SyncHeightmapGPUModifications")]
	public extern void SyncHeightmap();

	[NativeName("GetHeightmap().CopyHolesFromActiveRenderTexture")]
	private void Internal_CopyActiveRenderTextureToHoles(RectInt rect, int destX, int destY, bool allowDelayedCPUSync)
	{
		Internal_CopyActiveRenderTextureToHoles_Injected(ref rect, destX, destY, allowDelayedCPUSync);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetHeightmap().DirtyHolesRegion")]
	private extern void Internal_DirtyHolesRegion(int x, int y, int width, int height, bool allowDelayedCPUSync);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetHeightmap().SyncHolesGPUModifications")]
	private extern void Internal_SyncHoles();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetSplatDatabase().MarkDirtyRegion")]
	private extern void Internal_MarkAlphamapDirtyRegion(int alphamapIndex, int x, int y, int width, int height);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetSplatDatabase().ClearDirtyRegion")]
	private extern void Internal_ClearAlphamapDirtyRegion(int alphamapIndex);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetSplatDatabase().SyncGPUModifications")]
	private extern void Internal_SyncAlphamaps();

	public void CopyActiveRenderTextureToHeightmap(RectInt sourceRect, Vector2Int dest, TerrainHeightmapSyncControl syncControl)
	{
		RenderTexture active = RenderTexture.active;
		if (active == null)
		{
			throw new InvalidOperationException("Active RenderTexture is null.");
		}
		if (sourceRect.x < 0 || sourceRect.y < 0 || sourceRect.xMax > active.width || sourceRect.yMax > active.height)
		{
			throw new ArgumentOutOfRangeException("sourceRect");
		}
		if (dest.x < 0 || dest.x + sourceRect.width > heightmapResolution)
		{
			throw new ArgumentOutOfRangeException("dest.x");
		}
		if (dest.y < 0 || dest.y + sourceRect.height > heightmapResolution)
		{
			throw new ArgumentOutOfRangeException("dest.y");
		}
		Internal_CopyActiveRenderTextureToHeightmap(sourceRect, dest.x, dest.y, syncControl);
		TerrainCallbacks.InvokeHeightmapChangedCallback(this, new RectInt(dest.x, dest.y, sourceRect.width, sourceRect.height), syncControl == TerrainHeightmapSyncControl.HeightAndLod);
	}

	public void DirtyHeightmapRegion(RectInt region, TerrainHeightmapSyncControl syncControl)
	{
		int num = heightmapResolution;
		if (region.x < 0 || region.x >= num)
		{
			throw new ArgumentOutOfRangeException("region.x");
		}
		if (region.width <= 0 || region.xMax > num)
		{
			throw new ArgumentOutOfRangeException("region.width");
		}
		if (region.y < 0 || region.y >= num)
		{
			throw new ArgumentOutOfRangeException("region.y");
		}
		if (region.height <= 0 || region.yMax > num)
		{
			throw new ArgumentOutOfRangeException("region.height");
		}
		Internal_DirtyHeightmapRegion(region.x, region.y, region.width, region.height, syncControl);
		TerrainCallbacks.InvokeHeightmapChangedCallback(this, region, syncControl == TerrainHeightmapSyncControl.HeightAndLod);
	}

	public void CopyActiveRenderTextureToTexture(string textureName, int textureIndex, RectInt sourceRect, Vector2Int dest, bool allowDelayedCPUSync)
	{
		if (string.IsNullOrEmpty(textureName))
		{
			throw new ArgumentNullException("textureName");
		}
		RenderTexture active = RenderTexture.active;
		if (active == null)
		{
			throw new InvalidOperationException("Active RenderTexture is null.");
		}
		int num = 0;
		int num2 = 0;
		if (textureName == HolesTextureName)
		{
			if (textureIndex != 0)
			{
				throw new ArgumentOutOfRangeException("textureIndex");
			}
			if (active == holesTexture)
			{
				throw new ArgumentException("source", "Active RenderTexture cannot be holesTexture.");
			}
			num = (num2 = holesResolution);
		}
		else
		{
			if (!(textureName == AlphamapTextureName))
			{
				throw new ArgumentException("Unrecognized terrain texture name: \"" + textureName + "\"");
			}
			if (textureIndex < 0 || textureIndex >= alphamapTextureCount)
			{
				throw new ArgumentOutOfRangeException("textureIndex");
			}
			num = (num2 = alphamapResolution);
		}
		if (sourceRect.x < 0 || sourceRect.y < 0 || sourceRect.xMax > active.width || sourceRect.yMax > active.height)
		{
			throw new ArgumentOutOfRangeException("sourceRect");
		}
		if (dest.x < 0 || dest.x + sourceRect.width > num)
		{
			throw new ArgumentOutOfRangeException("dest.x");
		}
		if (dest.y < 0 || dest.y + sourceRect.height > num2)
		{
			throw new ArgumentOutOfRangeException("dest.y");
		}
		if (textureName == HolesTextureName)
		{
			Internal_CopyActiveRenderTextureToHoles(sourceRect, dest.x, dest.y, allowDelayedCPUSync);
			return;
		}
		Texture2D alphamapTexture = GetAlphamapTexture(textureIndex);
		allowDelayedCPUSync = allowDelayedCPUSync && SupportsCopyTextureBetweenRTAndTexture && QualitySettings.globalTextureMipmapLimit == 0;
		if (allowDelayedCPUSync)
		{
			if (alphamapTexture.mipmapCount > 1)
			{
				RenderTextureDescriptor desc = new RenderTextureDescriptor(alphamapTexture.width, alphamapTexture.height, active.graphicsFormat, active.depthStencilFormat);
				desc.sRGB = false;
				desc.useMipMap = true;
				desc.autoGenerateMips = false;
				RenderTexture temporary = RenderTexture.GetTemporary(desc);
				if (!temporary.IsCreated())
				{
					temporary.Create();
				}
				Graphics.CopyTexture(alphamapTexture, 0, 0, temporary, 0, 0);
				Graphics.CopyTexture(active, 0, 0, sourceRect.x, sourceRect.y, sourceRect.width, sourceRect.height, temporary, 0, 0, dest.x, dest.y);
				temporary.GenerateMips();
				Graphics.CopyTexture(temporary, alphamapTexture);
				RenderTexture.ReleaseTemporary(temporary);
			}
			else
			{
				Graphics.CopyTexture(active, 0, 0, sourceRect.x, sourceRect.y, sourceRect.width, sourceRect.height, alphamapTexture, 0, 0, dest.x, dest.y);
			}
			Internal_MarkAlphamapDirtyRegion(textureIndex, dest.x, dest.y, sourceRect.width, sourceRect.height);
		}
		else
		{
			alphamapTexture.ReadPixels(new Rect(sourceRect.x, sourceRect.y, sourceRect.width, sourceRect.height), dest.x, dest.y);
			alphamapTexture.Apply(updateMipmaps: true);
			Internal_ClearAlphamapDirtyRegion(textureIndex);
		}
		TerrainCallbacks.InvokeTextureChangedCallback(this, textureName, new RectInt(dest.x, dest.y, sourceRect.width, sourceRect.height), !allowDelayedCPUSync);
	}

	public void DirtyTextureRegion(string textureName, RectInt region, bool allowDelayedCPUSync)
	{
		if (string.IsNullOrEmpty(textureName))
		{
			throw new ArgumentNullException("textureName");
		}
		int num = 0;
		if (textureName == AlphamapTextureName)
		{
			num = alphamapResolution;
		}
		else
		{
			if (!(textureName == HolesTextureName))
			{
				throw new ArgumentException("Unrecognized terrain texture name: \"" + textureName + "\"");
			}
			num = holesResolution;
		}
		if (region.x < 0 || region.x >= num)
		{
			throw new ArgumentOutOfRangeException("region.x");
		}
		if (region.width <= 0 || region.xMax > num)
		{
			throw new ArgumentOutOfRangeException("region.width");
		}
		if (region.y < 0 || region.y >= num)
		{
			throw new ArgumentOutOfRangeException("region.y");
		}
		if (region.height <= 0 || region.yMax > num)
		{
			throw new ArgumentOutOfRangeException("region.height");
		}
		if (textureName == HolesTextureName)
		{
			Internal_DirtyHolesRegion(region.x, region.y, region.width, region.height, allowDelayedCPUSync);
			return;
		}
		Internal_MarkAlphamapDirtyRegion(-1, region.x, region.y, region.width, region.height);
		if (!allowDelayedCPUSync)
		{
			SyncTexture(textureName);
		}
		else
		{
			TerrainCallbacks.InvokeTextureChangedCallback(this, textureName, region, synched: false);
		}
	}

	public void SyncTexture(string textureName)
	{
		if (string.IsNullOrEmpty(textureName))
		{
			throw new ArgumentNullException("textureName");
		}
		if (textureName == AlphamapTextureName)
		{
			Internal_SyncAlphamaps();
			return;
		}
		if (textureName == HolesTextureName)
		{
			if (IsHolesTextureCompressed())
			{
				throw new InvalidOperationException("Holes texture is compressed. Compressed holes texture can not be read back from GPU. Use TerrainData.enableHolesTextureCompression to disable holes texture compression.");
			}
			Internal_SyncHoles();
			return;
		}
		throw new ArgumentException("Unrecognized terrain texture name: \"" + textureName + "\"");
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_heightmapScale_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_size_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_size_Injected(ref Vector3 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_bounds_Injected(out Bounds ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetInterpolatedNormal_Injected(float x, float y, out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_wavingGrassTint_Injected(out Color ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_wavingGrassTint_Injected(ref Color value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void Internal_GetTreeInstance_Injected(int index, out TreeInstance ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetTreeInstance_Injected(int index, ref TreeInstance instance);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern int RemoveTrees_Injected(ref Vector2 position, float radius, int prototypeIndex);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void Internal_CopyActiveRenderTextureToHeightmap_Injected(ref RectInt rect, int destX, int destY, TerrainHeightmapSyncControl syncControl);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void Internal_CopyActiveRenderTextureToHoles_Injected(ref RectInt rect, int destX, int destY, bool allowDelayedCPUSync);
}
