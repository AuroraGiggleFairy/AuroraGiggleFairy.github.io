using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;

public class SignDataManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct RuntimeRenderingInfo(int baseDescriptorIndex, int descriptorCount)
	{
		public readonly int BaseDescriptorIndex = baseDescriptorIndex;

		public readonly int DescriptorCount = descriptorCount;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class SignRenderingData : IDisposable
	{
		public static ComputeBuffer DummyBuffer = new ComputeBuffer(1, 4);

		public MaterialPropertyBlock materialPropertyBlock;

		public int layerCount;

		public ComputeBuffer descriptorBuffer;

		public ComputeBuffer circleBuffer;

		public ComputeBuffer polygonBuffer;

		public ComputeBuffer charBuffer;

		public ComputeBuffer noiseBuffer;

		public ComputeBuffer transformBuffer;

		public ComputeBuffer skewBuffer;

		public ComputeBuffer bulgeBuffer;

		public ComputeBuffer twirlBuffer;

		public ComputeBuffer kaleidoBuffer;

		public ComputeBuffer perspectiveBuffer;

		public ComputeBuffer arcBuffer;

		public ComputeBuffer stretchBuffer;

		public ComputeBuffer gridBuffer;

		public readonly Dictionary<SignData.SignLayer, RuntimeRenderingInfo> renderingInfoByLayer = new Dictionary<SignData.SignLayer, RuntimeRenderingInfo>();

		public readonly Dictionary<SignData.SignLayer, LayerComplexityInfo> complexityByLayer = new Dictionary<SignData.SignLayer, LayerComplexityInfo>();

		public float totalComplexity;

		public int totalWarps;

		public int totalDraws;

		public LayerStackInfo stackInfo;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool disposedValue;

		public SignRenderingData()
		{
			materialPropertyBlock = new MaterialPropertyBlock();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					materialPropertyBlock?.Clear();
					materialPropertyBlock = null;
					renderingInfoByLayer.Clear();
					complexityByLayer.Clear();
				}
				DisposeBuffers();
				disposedValue = true;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		~SignRenderingData()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public void Reset()
		{
			layerCount = 0;
			totalComplexity = 0f;
			totalWarps = 0;
			totalDraws = 0;
			stackInfo = default(LayerStackInfo);
			materialPropertyBlock?.Clear();
			renderingInfoByLayer.Clear();
			complexityByLayer.Clear();
			DisposeBuffers();
		}

		public void DisposeBuffers()
		{
			descriptorBuffer?.Dispose();
			descriptorBuffer = null;
			circleBuffer?.Dispose();
			circleBuffer = null;
			polygonBuffer?.Dispose();
			polygonBuffer = null;
			charBuffer?.Dispose();
			charBuffer = null;
			noiseBuffer?.Dispose();
			noiseBuffer = null;
			transformBuffer?.Dispose();
			transformBuffer = null;
			skewBuffer?.Dispose();
			skewBuffer = null;
			bulgeBuffer?.Dispose();
			bulgeBuffer = null;
			twirlBuffer?.Dispose();
			twirlBuffer = null;
			kaleidoBuffer?.Dispose();
			kaleidoBuffer = null;
			perspectiveBuffer?.Dispose();
			perspectiveBuffer = null;
			arcBuffer?.Dispose();
			arcBuffer = null;
			stretchBuffer?.Dispose();
			stretchBuffer = null;
			gridBuffer?.Dispose();
			gridBuffer = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class UpdateListenerMap
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<GlobalSignId, HashSet<ISignRenderingDataUpdateListener>> _listenersById;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<ISignRenderingDataUpdateListener, HashSet<GlobalSignId>> _idsByListener;

		public UpdateListenerMap(int capacity)
		{
			_idsByListener = new Dictionary<ISignRenderingDataUpdateListener, HashSet<GlobalSignId>>(capacity);
			_listenersById = new Dictionary<GlobalSignId, HashSet<ISignRenderingDataUpdateListener>>(capacity);
		}

		public void RegisterListener(GlobalSignId id, ISignRenderingDataUpdateListener listener)
		{
			if (!_listenersById.TryGetValue(id, out var value))
			{
				value = new HashSet<ISignRenderingDataUpdateListener>(5);
				_listenersById[id] = value;
			}
			value.Add(listener);
			if (!_idsByListener.TryGetValue(listener, out var value2))
			{
				value2 = new HashSet<GlobalSignId>(1);
				_idsByListener[listener] = value2;
			}
			value2.Add(id);
		}

		public void DeregisterListener(ISignRenderingDataUpdateListener listener)
		{
			if (!_idsByListener.TryGetValue(listener, out var value))
			{
				return;
			}
			foreach (GlobalSignId item in value)
			{
				if (_listenersById.TryGetValue(item, out var value2))
				{
					value2.Remove(listener);
					if (value2.Count == 0)
					{
						_listenersById.Remove(item);
					}
				}
			}
			_idsByListener.Remove(listener);
		}

		public IEnumerable<ISignRenderingDataUpdateListener> GetListeners(GlobalSignId id)
		{
			if (!_listenersById.TryGetValue(id, out var value))
			{
				return Array.Empty<ISignRenderingDataUpdateListener>();
			}
			return value;
		}

		public IEnumerable<GlobalSignId> GetIds(ISignRenderingDataUpdateListener listener)
		{
			if (!_idsByListener.TryGetValue(listener, out var value))
			{
				return Array.Empty<GlobalSignId>();
			}
			return value;
		}

		public void Clear()
		{
			_listenersById.Clear();
			_idsByListener.Clear();
		}

		public bool HasListeners(GlobalSignId signId)
		{
			return _listenersById.ContainsKey(signId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum LayerTypeID
	{
		UvClear = 0,
		UvPop = 1,
		UvPush = 2,
		MaskBegin = 3,
		MaskFlipPhase = 4,
		MaskEnd = 5,
		WarpTransform = 10,
		WarpSkew = 11,
		WarpBulge = 12,
		WarpTwirl = 13,
		WarpKaleido = 14,
		WarpPerspective = 15,
		WarpArc = 16,
		WarpStretch = 17,
		WarpGrid = 18,
		Polygon = 100,
		Char = 101,
		Circle = 102,
		Noise = 103
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct LayerDescriptor
	{
		public int type;

		public int index;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct CircleLayer
	{
		public Vector4 color;

		public float softness;

		public float dilate;

		public float frequency;

		public int mode;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct PolygonLayer
	{
		public Vector4 color;

		public float sides;

		public float smoothness;

		public float starify;

		public float softness;

		public float dilate;

		public float frequency;

		public int mode;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct CharLayer
	{
		public Vector2 position;

		public Vector2 scale;

		public Vector4 color;

		public int atlasIndex;

		public Vector4 offsetSize;

		public float softness;

		public float dilate;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct NoiseLayer
	{
		public Vector4 color;

		public float offset;

		public int detail;

		public float softness;

		public float dilate;

		public float fade;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct TransformWarp
	{
		public Vector2 position;

		public float rotation;

		public Vector2 scale;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct SkewWarp
	{
		public float rotation;

		public Vector2 amount;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct BulgeWarp
	{
		public Vector2 position;

		public float amount;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct TwirlWarp
	{
		public Vector2 position;

		public float amount;

		public float frequency;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct KaleidoWarp
	{
		public Vector2 position;

		public float angle;

		public float offset;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct PerspectiveWarp
	{
		public Vector3 rotation;

		public float strength;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct ArcWarp
	{
		public float rotation;

		public float radius;

		public float width;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct StretchWarp
	{
		public Vector2 position;

		public float rotation;

		public float distance;

		public float width;

		public float exponent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct GridWarp
	{
		public Vector2 offset;

		public float rotation;

		public float scale;

		public int mode;

		public float aspect;

		public float shift;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class CharAtlasData
	{
		public Vector2 quadOffset;

		public Vector2 quadSize;

		public Vector4 uvOffsetSize;

		public float charWidth;

		public CharAtlasData(Vector2 quadOffset, Vector2 quadSize, Vector4 uvOffsetSize, float charWidth)
		{
			this.quadOffset = quadOffset;
			this.quadSize = quadSize;
			this.uvOffsetSize = uvOffsetSize;
			this.charWidth = charWidth;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class SignFontData
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<uint, CharAtlasData> charToAtlasDataMap = new Dictionary<uint, CharAtlasData>();

		[field: PublicizedFrom(EAccessModifier.Private)]
		public int AtlasIndex
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		[field: PublicizedFrom(EAccessModifier.Private)]
		public Texture2D AtlasTexture
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		[field: PublicizedFrom(EAccessModifier.Private)]
		public float LineHeight
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ConstructAtlasDataMap(TMP_FontAsset fontAsset)
		{
			float num = fontAsset.atlasPadding;
			float num2 = 1f / (float)fontAsset.atlasWidth;
			float num3 = 1f / (float)fontAsset.atlasHeight;
			float num4 = fontAsset.atlasWidth / fontAsset.atlasHeight;
			float num5 = 1f / (num3 * (float)fontAsset.creationSettings.pointSize);
			LineHeight = fontAsset.faceInfo.lineHeight * num4 * num2 * num5;
			foreach (KeyValuePair<uint, TMP_Character> item in fontAsset.characterLookupTable)
			{
				GlyphRect glyphRect = item.Value.glyph.glyphRect;
				Vector4 uvOffsetSize = new Vector4
				{
					x = num2 * ((float)glyphRect.x - num),
					y = num3 * ((float)glyphRect.y - num),
					z = num2 * ((float)glyphRect.width + 2f * num),
					w = num3 * ((float)glyphRect.height + 2f * num)
				};
				float num6 = item.Value.glyph.metrics.horizontalAdvance * num4 * num2 * num5;
				Vector2 quadOffset = new Vector2(0.5f * num6, (-0.5f * ((float)glyphRect.height + fontAsset.faceInfo.capLine) + item.Value.glyph.metrics.horizontalBearingY) * num3 * num5);
				Vector2 quadSize = new Vector2(uvOffsetSize.z * num4, uvOffsetSize.w) * num5;
				CharAtlasData value = new CharAtlasData(quadOffset, quadSize, uvOffsetSize, num6);
				charToAtlasDataMap[item.Key] = value;
			}
		}

		public SignFontData(int atlasIndex, TMP_FontAsset fontAsset)
		{
			AtlasIndex = atlasIndex;
			AtlasTexture = fontAsset.atlasTexture;
			ConstructAtlasDataMap(fontAsset);
		}

		public bool TryGetCharAtlasData(char character, out CharAtlasData atlasData)
		{
			return charToAtlasDataMap.TryGetValue(character, out atlasData);
		}
	}

	public struct ColorOperation
	{
		public Color color;

		public SignData.GroupSignLayer.ColorMode mode;

		public static Color EvaluateColor(Color leafColor, List<ColorOperation> colorOps)
		{
			Color result = leafColor;
			for (int num = colorOps.Count - 1; num >= 0; num--)
			{
				ColorOperation colorOperation = colorOps[num];
				switch (colorOperation.mode)
				{
				case SignData.GroupSignLayer.ColorMode.Multiply:
					result *= colorOperation.color;
					break;
				case SignData.GroupSignLayer.ColorMode.Blend:
				{
					float a = colorOperation.color.a;
					result = new Color(Mathf.Lerp(result.r, colorOperation.color.r, a), Mathf.Lerp(result.g, colorOperation.color.g, a), Mathf.Lerp(result.b, colorOperation.color.b, a), result.a);
					break;
				}
				case SignData.GroupSignLayer.ColorMode.Override:
					result = colorOperation.color;
					break;
				}
			}
			return result;
		}
	}

	public struct GroupOffsets
	{
		public float shapeSoftnessOffset;

		public float shapeDilateOffset;

		public float textSoftnessOffset;

		public float textDilateOffset;

		public float noiseSoftnessOffset;

		public float noiseDilateOffset;

		public GroupOffsets WithOffsets(float softness, float dilate, SignData.GroupSignLayer.OffsetTarget target)
		{
			GroupOffsets result = this;
			switch (target)
			{
			case SignData.GroupSignLayer.OffsetTarget.All:
				result.shapeSoftnessOffset += softness;
				result.shapeDilateOffset += dilate;
				result.textSoftnessOffset += softness;
				result.textDilateOffset += dilate;
				result.noiseSoftnessOffset += softness;
				result.noiseDilateOffset += dilate;
				break;
			case SignData.GroupSignLayer.OffsetTarget.Shapes:
				result.shapeSoftnessOffset += softness;
				result.shapeDilateOffset += dilate;
				break;
			case SignData.GroupSignLayer.OffsetTarget.Text:
				result.textSoftnessOffset += softness;
				result.textDilateOffset += dilate;
				break;
			case SignData.GroupSignLayer.OffsetTarget.Noise:
				result.noiseSoftnessOffset += softness;
				result.noiseDilateOffset += dilate;
				break;
			}
			return result;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public interface IIntermediateDataWrapper
	{
		void UpdateBuffer(SignRenderingData signRenderingData);

		void ApplyToMaterial(SignRenderingData signRenderingData, Material material);

		void ClearStagingList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class IntermediateDataWrapper<T> : IIntermediateDataWrapper where T : struct
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly int _stride = Marshal.SizeOf(typeof(T));

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int _shaderPropertyId;

		public readonly List<T> List;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Func<SignRenderingData, ComputeBuffer> _get;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Action<SignRenderingData, ComputeBuffer> _set;

		public IntermediateDataWrapper(int shaderPropertyId, int capacity, Func<SignRenderingData, ComputeBuffer> getter, Action<SignRenderingData, ComputeBuffer> setter)
		{
			_shaderPropertyId = shaderPropertyId;
			List = new List<T>(capacity);
			_get = getter;
			_set = setter;
		}

		public void UpdateBuffer(SignRenderingData signRenderingData)
		{
			ComputeBuffer computeBuffer = _get(signRenderingData);
			if (List.Count > 0)
			{
				int num = NextPow2(List.Count);
				if (computeBuffer == null || computeBuffer.count != num)
				{
					computeBuffer?.Dispose();
					computeBuffer = new ComputeBuffer(num, _stride)
					{
						name = $"{typeof(T).Name}:{num}"
					};
					_set(signRenderingData, computeBuffer);
				}
				computeBuffer.SetData(List, 0, 0, List.Count);
			}
			else if (computeBuffer != null)
			{
				computeBuffer.Dispose();
				computeBuffer = null;
				_set(signRenderingData, null);
			}
			signRenderingData.materialPropertyBlock.SetBuffer(_shaderPropertyId, computeBuffer ?? SignRenderingData.DummyBuffer);
		}

		public void ApplyToMaterial(SignRenderingData signRenderingData, Material material)
		{
			material.SetBuffer(_shaderPropertyId, _get(signRenderingData) ?? SignRenderingData.DummyBuffer);
		}

		public void ClearStagingList()
		{
			List.Clear();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static int NextPow2(int v)
		{
			if (v <= 4)
			{
				return 4;
			}
			v--;
			v |= v >> 1;
			v |= v >> 2;
			v |= v >> 4;
			v |= v >> 8;
			v |= v >> 16;
			return v + 1;
		}
	}

	public delegate void RenderingDataPatcher(MaterialPropertyBlock mpb);

	[PublicizedFrom(EAccessModifier.Private)]
	public const string fontConfigLoc = "@:Fonts/SignFontConfig.asset";

	[PublicizedFrom(EAccessModifier.Private)]
	public const float baseDescriptorComplexity = 0.5f;

	public const string defaultFontName = "LiberationSans";

	public const string defaultLibraryId = "[D]";

	public const string internalLibraryId = "[I]";

	public const string userLibraryId = "[U]";

	public const float cMaxComplexity = 600f;

	public const int cMaxCompStackIndex = 7;

	public const int cMaxUVStackIndex = 7;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, SignFontData> fontDataByName = new Dictionary<string, SignFontData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, SignLibrary> signLibraries = new Dictionary<string, SignLibrary>(100);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<GlobalSignId, SignRenderingData> renderingDataByID = new Dictionary<GlobalSignId, SignRenderingData>(100);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly UpdateListenerMap updateListenerMap = new UpdateListenerMap(100);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IntermediateDataWrapper<LayerDescriptor> layerDescriptors = new IntermediateDataWrapper<LayerDescriptor>(SignShaderIDs._LayerDescriptors, 100, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd) => srd.descriptorBuffer, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd, ComputeBuffer b) =>
	{
		srd.descriptorBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IntermediateDataWrapper<CircleLayer> circleLayers = new IntermediateDataWrapper<CircleLayer>(SignShaderIDs._CircleLayers, 100, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd) => srd.circleBuffer, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd, ComputeBuffer b) =>
	{
		srd.circleBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IntermediateDataWrapper<PolygonLayer> polygonLayers = new IntermediateDataWrapper<PolygonLayer>(SignShaderIDs._PolygonLayers, 100, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd) => srd.polygonBuffer, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd, ComputeBuffer b) =>
	{
		srd.polygonBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IntermediateDataWrapper<CharLayer> charLayers = new IntermediateDataWrapper<CharLayer>(SignShaderIDs._CharLayers, 100, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd) => srd.charBuffer, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd, ComputeBuffer b) =>
	{
		srd.charBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IntermediateDataWrapper<NoiseLayer> noiseLayers = new IntermediateDataWrapper<NoiseLayer>(SignShaderIDs._NoiseLayers, 100, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd) => srd.noiseBuffer, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd, ComputeBuffer b) =>
	{
		srd.noiseBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IntermediateDataWrapper<TransformWarp> transformWarps = new IntermediateDataWrapper<TransformWarp>(SignShaderIDs._TransformWarps, 100, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd) => srd.transformBuffer, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd, ComputeBuffer b) =>
	{
		srd.transformBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IntermediateDataWrapper<SkewWarp> skewWarps = new IntermediateDataWrapper<SkewWarp>(SignShaderIDs._SkewWarps, 100, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd) => srd.skewBuffer, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd, ComputeBuffer b) =>
	{
		srd.skewBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IntermediateDataWrapper<BulgeWarp> bulgeWarps = new IntermediateDataWrapper<BulgeWarp>(SignShaderIDs._BulgeWarps, 100, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd) => srd.bulgeBuffer, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd, ComputeBuffer b) =>
	{
		srd.bulgeBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IntermediateDataWrapper<TwirlWarp> twirlWarps = new IntermediateDataWrapper<TwirlWarp>(SignShaderIDs._TwirlWarps, 100, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd) => srd.twirlBuffer, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd, ComputeBuffer b) =>
	{
		srd.twirlBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IntermediateDataWrapper<KaleidoWarp> kaleidoWarps = new IntermediateDataWrapper<KaleidoWarp>(SignShaderIDs._KaleidoWarps, 100, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd) => srd.kaleidoBuffer, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd, ComputeBuffer b) =>
	{
		srd.kaleidoBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IntermediateDataWrapper<PerspectiveWarp> perspectiveWarps = new IntermediateDataWrapper<PerspectiveWarp>(SignShaderIDs._PerspectiveWarps, 100, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd) => srd.perspectiveBuffer, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd, ComputeBuffer b) =>
	{
		srd.perspectiveBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IntermediateDataWrapper<ArcWarp> arcWarps = new IntermediateDataWrapper<ArcWarp>(SignShaderIDs._ArcWarps, 100, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd) => srd.arcBuffer, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd, ComputeBuffer b) =>
	{
		srd.arcBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IntermediateDataWrapper<StretchWarp> stretchWarps = new IntermediateDataWrapper<StretchWarp>(SignShaderIDs._StretchWarps, 100, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd) => srd.stretchBuffer, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd, ComputeBuffer b) =>
	{
		srd.stretchBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IntermediateDataWrapper<GridWarp> gridWarps = new IntermediateDataWrapper<GridWarp>(SignShaderIDs._GridWarps, 100, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd) => srd.gridBuffer, [PublicizedFrom(EAccessModifier.Internal)] (SignRenderingData srd, ComputeBuffer b) =>
	{
		srd.gridBuffer = b;
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IIntermediateDataWrapper[] intermediateDataWrappers;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ColorOperation> colorOpStack = new List<ColorOperation>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2DArray fontAtlasTextureArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public static SignDataManager m_Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSignSyncBatchBytes = 1048576;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool signDataDownloadInProgress;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool signDataDownloadComplete;

	public static SignDataManager Instance
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = new SignDataManager();
			}
			return m_Instance;
		}
	}

	public static bool HasInstance => m_Instance != null;

	public IEnumerable<string> FontNames => fontDataByName.Keys;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static GlobalSignId ErrorSignId
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SignDataManager()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		SignFontConfiguration signFontConfiguration = DataLoader.LoadAsset<SignFontConfiguration>("@:Fonts/SignFontConfig.asset", _ignoreDlcEntitlements: true);
		if (signFontConfiguration == null)
		{
			Log.Error("Failed to load font configuration asset from path: @:Fonts/SignFontConfig.asset");
		}
		else
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (SignFontConfiguration.FontInfo fontInfo in signFontConfiguration.fontInfos)
			{
				if (fontInfo.fontAsset?.atlasTexture != null)
				{
					if (num2 == 0 || num3 == 0)
					{
						num2 = fontInfo.fontAsset.atlasTexture.width;
						num3 = fontInfo.fontAsset.atlasTexture.height;
					}
					else if (num2 != fontInfo.fontAsset.atlasTexture.width || num3 != fontInfo.fontAsset.atlasTexture.height)
					{
						Log.Error("SignFontConfiguration asset references fonts with differing font atlas resolutions. This is not supported. Configuration asset prefabLocation: `@:Fonts/SignFontConfig.asset`.");
						continue;
					}
					fontDataByName[fontInfo.name] = new SignFontData(num, fontInfo.fontAsset);
					num++;
				}
			}
			if (SystemInfo.supports2DArrayTextures)
			{
				fontAtlasTextureArray = new Texture2DArray(num2, num3, fontDataByName.Count, TextureFormat.Alpha8, mipChain: false);
				fontAtlasTextureArray.name = "Sign Font Atlas";
				foreach (KeyValuePair<string, SignFontData> item in fontDataByName)
				{
					SignFontData value = item.Value;
					Graphics.CopyTexture(value.AtlasTexture, 0, fontAtlasTextureArray, value.AtlasIndex);
				}
			}
			else
			{
				Log.Error("Texture arrays are not supported on this platform/GPU; sign fonts will not be rendered correctly.");
			}
		}
		intermediateDataWrappers = new IIntermediateDataWrapper[14]
		{
			layerDescriptors, circleLayers, polygonLayers, charLayers, noiseLayers, transformWarps, skewWarps, bulgeWarps, twirlWarps, kaleidoWarps,
			perspectiveWarps, arcWarps, stretchWarps, gridWarps
		};
	}

	public static string GetLibraryNiceName(GlobalSignId signId)
	{
		if (!signId.IsValid)
		{
			return "-";
		}
		string libraryId = signId.libraryId;
		if (!(libraryId == "[D]"))
		{
			if (libraryId == "[U]")
			{
				return Localization.Get("lblSignLibraryUser");
			}
			return signId.libraryId;
		}
		return Localization.Get("lblSignLibraryDefault");
	}

	public static IEnumerator LoadDefaultLibrary(XmlFile file)
	{
		SignLibrary signLibrary = new SignLibrary();
		signLibrary.ReadXml(file);
		Instance.TryRegisterLibrary("[D]", signLibrary);
		ErrorSignId = Instance.AddSignToLibrary("[I]", SignData.GetErrorSignData());
		yield break;
	}

	public void Cleanup()
	{
		foreach (KeyValuePair<GlobalSignId, SignRenderingData> item in renderingDataByID)
		{
			item.Value?.Dispose();
		}
		renderingDataByID.Clear();
		signLibraries.Clear();
		updateListenerMap.Clear();
	}

	public void RegisterListener(GlobalSignId id, ISignRenderingDataUpdateListener listener, bool exclusive = true)
	{
		if (exclusive)
		{
			updateListenerMap.DeregisterListener(listener);
		}
		updateListenerMap.RegisterListener(id, listener);
	}

	public void DeregisterListener(ISignRenderingDataUpdateListener listener)
	{
		updateListenerMap.DeregisterListener(listener);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool LibraryContainsName(string libraryName, string signName)
	{
		if (signLibraries.TryGetValue(libraryName, out var value))
		{
			foreach (SignData value2 in value.signs.Values)
			{
				if (value2.name == signName)
				{
					return true;
				}
			}
		}
		return false;
	}

	public string GetDuplicateName(string libraryName, string signName)
	{
		string text = signName;
		Match match = Regex.Match(signName, "^(.+) \\(\\d+\\)$");
		if (match.Success)
		{
			text = match.Groups[1].Value;
		}
		if (!LibraryContainsName(libraryName, text))
		{
			return text;
		}
		int i;
		for (i = 1; LibraryContainsName(libraryName, $"{text} ({i})"); i++)
		{
		}
		return $"{text} ({i})";
	}

	public bool MoveSignToLibrary(GlobalSignId signId, string libraryName, out GlobalSignId newId)
	{
		if (TryGetSignData(signId, out var signData))
		{
			SignData sign = SignData.Duplicate(signData);
			newId = AddSignToLibrary(libraryName, sign);
			return true;
		}
		newId = GlobalSignId.InvalidId;
		return false;
	}

	public GlobalSignId AddSignToLibrary(string libraryId, SignData sign)
	{
		if (!signLibraries.TryGetValue(libraryId, out var value))
		{
			value = new SignLibrary();
			signLibraries[libraryId] = value;
		}
		value.signs[sign.guid] = sign;
		return new GlobalSignId(libraryId, sign.guid);
	}

	public bool TryDeleteSign(GlobalSignId signId)
	{
		TryClearRenderingData(signId);
		if (signLibraries.TryGetValue(signId.libraryId, out var value))
		{
			return value.signs.Remove(signId.signGuid);
		}
		return false;
	}

	public bool MarkSignDirty(GlobalSignId signId)
	{
		if (updateListenerMap.HasListeners(signId) && TryGetSignData(signId, out var signData))
		{
			UpdateRenderingData(signData, signId);
			foreach (ISignRenderingDataUpdateListener listener in updateListenerMap.GetListeners(signId))
			{
				listener.HandleRenderingDataUpdate();
			}
			return true;
		}
		return TryClearRenderingData(signId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryClearRenderingData(GlobalSignId signId)
	{
		if (renderingDataByID.TryGetValue(signId, out var value))
		{
			value.Dispose();
			renderingDataByID.Remove(signId);
			return true;
		}
		return false;
	}

	public bool TryRegisterLibrary(string libraryId, SignLibrary library, bool replaceExisting = false)
	{
		if (signLibraries.TryGetValue(libraryId, out var value))
		{
			if (!replaceExisting)
			{
				return false;
			}
			foreach (Guid key2 in value.signs.Keys)
			{
				GlobalSignId key = new GlobalSignId(libraryId, key2);
				if (renderingDataByID.TryGetValue(key, out var value2))
				{
					value2.Dispose();
					renderingDataByID.Remove(key);
				}
			}
		}
		signLibraries[libraryId] = library;
		return true;
	}

	public IEnumerator RequestWorldSignDataFromServer()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Error("[SignDataManager] Only client can request to download Sign Data.");
			yield break;
		}
		if (signDataDownloadInProgress)
		{
			Log.Error("[SignDataManager] Sign Data download is already in progress.");
			yield break;
		}
		signDataDownloadInProgress = true;
		signDataDownloadComplete = false;
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageSignDataRequest>());
		while (signDataDownloadInProgress)
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
			{
				signDataDownloadInProgress = false;
				signDataDownloadComplete = false;
				break;
			}
			yield return null;
		}
	}

	public void SendSignDataToClient(ClientInfo client)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			Log.Error("[SignDataManager] Only server can send Sign Data to clients.");
			return;
		}
		List<KeyValuePair<string, SignData>> list = new List<KeyValuePair<string, SignData>>();
		foreach (KeyValuePair<string, SignLibrary> item in EnumeratePublicSignLibraries())
		{
			foreach (SignData value in item.Value.signs.Values)
			{
				list.Add(new KeyValuePair<string, SignData>(item.Key, value));
			}
		}
		if (list.Count == 0)
		{
			client.SendPackage(NetPackageManager.GetPackage<NetPackageSignDataResponse>().Setup(null, _isLastBatch: true));
			return;
		}
		using PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
		using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
		pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
		int i = 0;
		while (i < list.Count)
		{
			pooledExpandableMemoryStream.SetLength(0L);
			PooledBinaryWriter.StreamWriteSizeMarker _sizeMarker = pooledBinaryWriter.ReserveSizeMarker(PooledBinaryWriter.EMarkerSize.UInt32);
			for (; i < list.Count; i++)
			{
				if (pooledExpandableMemoryStream.Length >= 1048576)
				{
					break;
				}
				pooledBinaryWriter.Write(list[i].Key);
				list[i].Value.Write(pooledBinaryWriter);
			}
			pooledBinaryWriter.FinalizeSizeMarker(ref _sizeMarker);
			byte[] data = pooledExpandableMemoryStream.ToArray();
			client.SendPackage(NetPackageManager.GetPackage<NetPackageSignDataResponse>().Setup(data, i >= list.Count));
		}
	}

	public void ProcessSignDataBatchReceived(byte[] data, bool isLastBatch)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Error("[SignDataManager] Only client can download Sign Data.");
			return;
		}
		try
		{
			if (data == null)
			{
				return;
			}
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			using MemoryStream memoryStream = new MemoryStream(data);
			pooledBinaryReader.SetBaseStream(memoryStream);
			PooledBinaryReader.StreamReadSizeMarker streamReadSizeMarker = pooledBinaryReader.ReadSizeMarker(PooledBinaryWriter.EMarkerSize.UInt32);
			long num = streamReadSizeMarker.Position + streamReadSizeMarker.ExpectedSize;
			while (memoryStream.Position < num)
			{
				string libraryId = pooledBinaryReader.ReadString();
				SignData sign = SignData.Read(pooledBinaryReader);
				AddSignToLibrary(libraryId, sign);
			}
		}
		catch
		{
			Log.Error("[SignDataManager] Failed to apply sign data batch from server");
		}
		finally
		{
			if (isLastBatch)
			{
				signDataDownloadInProgress = false;
				signDataDownloadComplete = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerable<KeyValuePair<string, SignLibrary>> EnumeratePublicSignLibraries()
	{
		foreach (KeyValuePair<string, SignLibrary> signLibrary in signLibraries)
		{
			if (!signLibrary.Key.Equals("[I]") || !signLibrary.Key.Equals("[U]"))
			{
				yield return signLibrary;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetRenderingData(GlobalSignId signId, out SignRenderingData signRenderingData)
	{
		if (!renderingDataByID.TryGetValue(signId, out signRenderingData))
		{
			if (!signLibraries.TryGetValue(signId.libraryId, out var value) || !value.signs.TryGetValue(signId.signGuid, out var value2))
			{
				return false;
			}
			signRenderingData = UpdateRenderingData(value2, signId);
		}
		return true;
	}

	public bool TryApplyRenderingData(GlobalSignId signId, RenderingDataPatcher patcher, List<SignRenderer> signRenderers, SignCanvas.SignBlendMode blendMode = SignCanvas.SignBlendMode.Cutout, Camera targetCamera = null)
	{
		if (!TryGetRenderingData(signId, out var signRenderingData))
		{
			return false;
		}
		Vector4 value = new Vector4(2f, 2f, -1f, -1f);
		signRenderingData.materialPropertyBlock.SetVector(SignShaderIDs._AtlasArray_ST, value);
		patcher?.Invoke(signRenderingData.materialPropertyBlock);
		if (signRenderingData.materialPropertyBlock == null)
		{
			Log.Error("Unexpected case in Sign Data Manager: signRenderingData.materialPropertyBlock is null.");
		}
		foreach (SignRenderer signRenderer in signRenderers)
		{
			if (signRenderer == null)
			{
				Log.Warning("Unexpected case in Sign Data Manager: signRenderer is null");
			}
			else if (signRenderer.Renderer == null)
			{
				Log.Warning("Unexpected case in Sign Data Manager: signRenderer.Renderer is null");
			}
			else
			{
				signRenderer.SetRenderParameters(signRenderingData.materialPropertyBlock, blendMode, targetCamera);
			}
		}
		return true;
	}

	public bool TryApplyRenderingData(GlobalSignId signId, float canvasAspect, Material material, SignData.SignLayer isolateLayer = null, SignUIStyle style = SignUIStyle.Standard)
	{
		if (!TryGetRenderingData(signId, out var signRenderingData))
		{
			return false;
		}
		float b = 1f / canvasAspect;
		Mathf.Max(canvasAspect, b);
		if (fontAtlasTextureArray != null)
		{
			material.SetTexture(SignShaderIDs._AtlasArray, fontAtlasTextureArray);
		}
		IIntermediateDataWrapper[] array = intermediateDataWrappers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ApplyToMaterial(signRenderingData, material);
		}
		material.SetVector(value: new Vector4(2f, 2f, -1f, -1f), nameID: SignShaderIDs._AtlasArray_ST);
		if (isolateLayer != null && signRenderingData.renderingInfoByLayer.TryGetValue(isolateLayer, out var value))
		{
			material.SetInteger(SignShaderIDs._BaseLayer, value.BaseDescriptorIndex);
			material.SetInteger(SignShaderIDs._LayerCount, value.DescriptorCount);
		}
		else
		{
			material.SetInteger(SignShaderIDs._BaseLayer, 0);
			material.SetInteger(SignShaderIDs._LayerCount, signRenderingData.layerCount);
		}
		material.SetInteger(SignShaderIDs._UIStyle, (int)style);
		material.SetFloat(SignShaderIDs._CanvasRotation, 0f);
		return true;
	}

	public bool TryGetSignComplexityInfo(GlobalSignId signId, out SignComplexityInfo complexityInfo)
	{
		if (TryGetRenderingData(signId, out var signRenderingData))
		{
			complexityInfo = new SignComplexityInfo(signRenderingData.totalComplexity, signRenderingData.complexityByLayer, signRenderingData.stackInfo);
			return true;
		}
		complexityInfo = SignComplexityInfo.Invalid;
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SignRenderingData UpdateRenderingData(SignData signData, GlobalSignId id)
	{
		ClearIntermediateLists();
		if (renderingDataByID.TryGetValue(id, out var signRenderingData))
		{
			signRenderingData.Reset();
		}
		else
		{
			signRenderingData = new SignRenderingData();
			renderingDataByID[id] = signRenderingData;
		}
		int compStackIndex = 0;
		int uvStackIndex = 0;
		signRenderingData.stackInfo = ProcessLayers(signData.layers, SignData.SignTransform.Identity, colorOpStack);
		signRenderingData.layerCount = layerDescriptors.List.Count;
		if (fontAtlasTextureArray != null)
		{
			signRenderingData.materialPropertyBlock.SetTexture(SignShaderIDs._AtlasArray, fontAtlasTextureArray);
		}
		signRenderingData.materialPropertyBlock.SetInteger(SignShaderIDs._BaseLayer, 0);
		signRenderingData.materialPropertyBlock.SetInteger(SignShaderIDs._LayerCount, signRenderingData.layerCount);
		IIntermediateDataWrapper[] array = intermediateDataWrappers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateBuffer(signRenderingData);
		}
		ClearIntermediateLists();
		return signRenderingData;
		[PublicizedFrom(EAccessModifier.Private)]
		void ClearIntermediateLists()
		{
			colorOpStack.Clear();
			IIntermediateDataWrapper[] array2 = intermediateDataWrappers;
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j].ClearStagingList();
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void PopWarps()
		{
			LayerDescriptor value = layerDescriptors.List[layerDescriptors.List.Count - 1];
			if (value.type == 1)
			{
				value.index--;
				layerDescriptors.List[layerDescriptors.List.Count - 1] = value;
			}
			else
			{
				layerDescriptors.List.Add(new LayerDescriptor
				{
					type = 1,
					index = -1
				});
				signRenderingData.totalComplexity += 0.5f;
			}
			uvStackIndex--;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		LayerStackInfo ProcessLayers(List<SignData.SignLayer> layers, SignData.SignTransform parentTransform, List<ColorOperation> colorOps, GroupOffsets parentOffsets = default(GroupOffsets))
		{
			int num = 0;
			LayerStackInfo result = new LayerStackInfo(compStackIndex, uvStackIndex);
			foreach (SignData.SignLayer layer in layers)
			{
				LayerComplexityInfo value = new LayerComplexityInfo
				{
					BaseOffset = signRenderingData.totalComplexity,
					StartCompStackIndex = compStackIndex,
					MaxCompStackIndex = compStackIndex,
					StartUVStackIndex = uvStackIndex,
					MaxUVStackIndex = uvStackIndex,
					GroupDepth = 0
				};
				int count = layerDescriptors.List.Count;
				int totalDraws = signRenderingData.totalDraws;
				int totalWarps = signRenderingData.totalWarps;
				bool flag = false;
				SignData.SignRenderSettings.Mode mode = layer.renderSettings.mode;
				if (mode != SignData.SignRenderSettings.Mode.ColorOnly && mode - 1 <= SignData.SignRenderSettings.Mode.MaskOnly)
				{
					layerDescriptors.List.Add(new LayerDescriptor
					{
						type = 3,
						index = (int)layer.renderSettings.mode
					});
					flag = true;
					signRenderingData.totalComplexity += 0.5f;
					compStackIndex++;
					value.MaxCompStackIndex = compStackIndex;
				}
				GroupOffsets parentOffsets2;
				int num2;
				if (layer is SignData.GroupSignLayer groupSignLayer)
				{
					parentOffsets2 = parentOffsets.WithOffsets(groupSignLayer.softnessOffset, groupSignLayer.dilateOffset, groupSignLayer.offsetTarget);
					Color color = groupSignLayer.renderSettings.color;
					if (groupSignLayer.colorMode != SignData.GroupSignLayer.ColorMode.Multiply || !(color == Color.white))
					{
						if (groupSignLayer.colorMode == SignData.GroupSignLayer.ColorMode.Blend)
						{
							num2 = ((color.a == 0f) ? 1 : 0);
							if (num2 != 0)
							{
								goto IL_0209;
							}
						}
						else
						{
							num2 = 0;
						}
						colorOps.Add(new ColorOperation
						{
							color = color,
							mode = groupSignLayer.colorMode
						});
					}
					else
					{
						num2 = 1;
					}
					goto IL_0209;
				}
				if (!(layer is SignData.TextSignLayer textLayer))
				{
					if (!(layer is SignData.PolygonSignLayer polygonLayer))
					{
						if (layer is SignData.NoiseSignLayer noiseLayer)
						{
							if (layer.HasTransformOrWarps || parentTransform != SignData.SignTransform.Identity)
							{
								PushWarps(parentTransform, layer);
								value.MaxUVStackIndex = uvStackIndex;
								ProcessNoiseLayer(noiseLayer, colorOps, parentOffsets);
								PopWarps();
							}
							else
							{
								ProcessNoiseLayer(noiseLayer, colorOps, parentOffsets);
							}
						}
					}
					else if (layer.HasTransformOrWarps || parentTransform != SignData.SignTransform.Identity)
					{
						PushWarps(parentTransform, layer);
						value.MaxUVStackIndex = uvStackIndex;
						ProcessPolygonLayer(polygonLayer, colorOps, parentOffsets);
						PopWarps();
					}
					else
					{
						ProcessPolygonLayer(polygonLayer, colorOps, parentOffsets);
					}
				}
				else if (layer.HasTransformOrWarps || parentTransform != SignData.SignTransform.Identity)
				{
					PushWarps(parentTransform, layer);
					value.MaxUVStackIndex = uvStackIndex;
					ProcessTextLayer(textLayer, colorOps, parentOffsets);
					PopWarps();
				}
				else
				{
					ProcessTextLayer(textLayer, colorOps, parentOffsets);
				}
				goto IL_03db;
				IL_0209:
				if (groupSignLayer.HasWarps || !groupSignLayer.transform.HasUniformScale)
				{
					PushWarps(parentTransform, layer);
					LayerStackInfo layerStackInfo = ProcessLayers(groupSignLayer.layers, SignData.SignTransform.Identity, colorOps, parentOffsets2);
					value.MaxCompStackIndex = layerStackInfo.MaxCompStackIndex;
					value.MaxUVStackIndex = layerStackInfo.MaxUVStackIndex;
					value.GroupDepth = layerStackInfo.GroupDepth + 1;
					PopWarps();
				}
				else
				{
					LayerStackInfo layerStackInfo2 = ProcessLayers(groupSignLayer.layers, parentTransform * groupSignLayer.transform, colorOps, parentOffsets2);
					value.MaxCompStackIndex = layerStackInfo2.MaxCompStackIndex;
					value.MaxUVStackIndex = layerStackInfo2.MaxUVStackIndex;
					value.GroupDepth = layerStackInfo2.GroupDepth + 1;
				}
				if (num2 == 0)
				{
					colorOps.RemoveAt(colorOps.Count - 1);
				}
				goto IL_03db;
				IL_03db:
				if (flag)
				{
					layerDescriptors.List.Add(new LayerDescriptor
					{
						type = 4,
						index = -1
					});
					num++;
					signRenderingData.totalComplexity += 0.55f;
				}
				else if (num > 0)
				{
					layerDescriptors.List.Add(new LayerDescriptor
					{
						type = 5,
						index = num
					});
					signRenderingData.totalComplexity += 0.5f + 0.05f * (float)num;
					compStackIndex -= num;
					num = 0;
				}
				int descriptorCount = layerDescriptors.List.Count - count;
				value.EndCompStackIndex = compStackIndex;
				value.EndUVStackIndex = uvStackIndex;
				value.Complexity = signRenderingData.totalComplexity - value.BaseOffset;
				value.WarpCount = signRenderingData.totalWarps - totalWarps;
				value.DrawCount = signRenderingData.totalDraws - totalDraws;
				value.DescriptorCount = descriptorCount;
				signRenderingData.renderingInfoByLayer[layer] = new RuntimeRenderingInfo(count, descriptorCount);
				signRenderingData.complexityByLayer[layer] = value;
				result.MaxCompStackIndex = Mathf.Max(result.MaxCompStackIndex, value.MaxCompStackIndex);
				result.MaxUVStackIndex = Mathf.Max(result.MaxUVStackIndex, value.MaxUVStackIndex);
				result.GroupDepth = Mathf.Max(result.GroupDepth, value.GroupDepth);
			}
			if (num > 0)
			{
				layerDescriptors.List.Add(new LayerDescriptor
				{
					type = 5,
					index = num
				});
				signRenderingData.totalComplexity += 0.5f + 0.05f * (float)num;
				compStackIndex -= num;
				num = 0;
			}
			return result;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void ProcessNoiseLayer(SignData.NoiseSignLayer noiseLayer, List<ColorOperation> colorOps, GroupOffsets offsets)
		{
			LayerDescriptor item = new LayerDescriptor
			{
				type = 103,
				index = noiseLayers.List.Count
			};
			float softness = Mathf.Max(0f, noiseLayer.softness + offsets.noiseSoftnessOffset);
			float dilate = noiseLayer.dilate + offsets.noiseDilateOffset;
			NoiseLayer item2 = new NoiseLayer
			{
				color = ColorOperation.EvaluateColor(noiseLayer.renderSettings.color, colorOps).GammaToLinearSpace(preserveAlpha: true),
				offset = noiseLayer.seed * 10,
				detail = noiseLayer.detail,
				softness = softness,
				dilate = dilate,
				fade = noiseLayer.fade
			};
			layerDescriptors.List.Add(item);
			noiseLayers.List.Add(item2);
			signRenderingData.totalComplexity += 1.5f + 1.1f * (float)noiseLayer.detail;
			signRenderingData.totalDraws++;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void ProcessPolygonLayer(SignData.PolygonSignLayer polygonLayer, List<ColorOperation> colorOps, GroupOffsets offsets)
		{
			float softness = Mathf.Max(0f, polygonLayer.softness + offsets.shapeSoftnessOffset);
			float dilate = polygonLayer.dilate + offsets.shapeDilateOffset;
			Color color = ColorOperation.EvaluateColor(polygonLayer.renderSettings.color, colorOps).GammaToLinearSpace(preserveAlpha: true);
			if (polygonLayer.smoothness > 0.99f)
			{
				LayerDescriptor item = new LayerDescriptor
				{
					type = 102,
					index = circleLayers.List.Count
				};
				CircleLayer item2 = new CircleLayer
				{
					color = color,
					softness = softness,
					dilate = dilate,
					frequency = polygonLayer.frequency,
					mode = (int)polygonLayer.shapeMode
				};
				layerDescriptors.List.Add(item);
				circleLayers.List.Add(item2);
				signRenderingData.totalComplexity += 1.5f;
				signRenderingData.totalDraws++;
			}
			else
			{
				LayerDescriptor item3 = new LayerDescriptor
				{
					type = 100,
					index = polygonLayers.List.Count
				};
				PolygonLayer item4 = new PolygonLayer
				{
					color = color,
					sides = polygonLayer.sides,
					smoothness = polygonLayer.smoothness,
					starify = polygonLayer.starify,
					softness = softness,
					dilate = dilate,
					frequency = polygonLayer.frequency,
					mode = (int)polygonLayer.shapeMode
				};
				layerDescriptors.List.Add(item3);
				polygonLayers.List.Add(item4);
				signRenderingData.totalComplexity += 2.875f;
				signRenderingData.totalDraws++;
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void ProcessTextLayer(SignData.TextSignLayer textLayer, List<ColorOperation> colorOps, GroupOffsets offsets)
		{
			if (textLayer.font == null || !fontDataByName.TryGetValue(textLayer.font, out var value))
			{
				Log.Error("Failed to find font data for font `" + textLayer.font + "`.");
				if (!fontDataByName.TryGetValue("LiberationSans", out value))
				{
					Log.Error("Failed to load default fallback font `LiberationSans`.");
					return;
				}
			}
			int count = charLayers.List.Count;
			Color color = ColorOperation.EvaluateColor(textLayer.renderSettings.color, colorOps).GammaToLinearSpace(preserveAlpha: true);
			Vector2 zero = Vector2.zero;
			Vector2 vector = Vector2.zero;
			float t = 1f - Mathf.Abs(90f - (textLayer.direction + 360f) % 180f) / 90f;
			float f = textLayer.direction * (MathF.PI / 180f);
			Vector2 vector2 = new Vector2(Mathf.Cos(f), 0f - Mathf.Sin(f));
			float num = Mathf.Max(0f, textLayer.spacing - 1f) * value.LineHeight;
			float num2 = Mathf.Clamp01(textLayer.spacing);
			float num3 = Mathf.Clamp01(textLayer.softness + offsets.textSoftnessOffset);
			float num4 = Mathf.Clamp(textLayer.dilate + offsets.textDilateOffset, -1f, 1f);
			float softness = num3 * (1f - Mathf.Abs(num4));
			float dilate = num4 * (1f - 0.5f * num3);
			string text = textLayer.text;
			foreach (char c in text)
			{
				if (!value.TryGetCharAtlasData(c, out var atlasData))
				{
					Log.Warning($"Could not retrieve atlas data for character \"{c}\" in string \"{textLayer.text}\"," + " found in sign \"" + signData.name + "\" (library:\"" + id.libraryId + "\"). \nPlease check \"TextSignLayer\" elements in the relevant _signs.xml for invalid characters, as they may not be visible in the Sign Editor tool.");
				}
				else
				{
					float num5 = Mathf.Lerp(atlasData.charWidth, value.LineHeight, t);
					float num6 = num2 * (num5 + num);
					Vector2 quadOffset = atlasData.quadOffset;
					quadOffset.x *= vector2.x;
					vector = zero + vector2 * Mathf.Lerp(num5, 0f, t);
					if (c == ' ')
					{
						zero += vector2 * num6;
					}
					else
					{
						LayerDescriptor item = new LayerDescriptor
						{
							type = 101,
							index = charLayers.List.Count
						};
						CharLayer item2 = new CharLayer
						{
							position = zero + quadOffset,
							scale = atlasData.quadSize,
							color = color,
							atlasIndex = value.AtlasIndex,
							offsetSize = atlasData.uvOffsetSize,
							softness = softness,
							dilate = dilate
						};
						layerDescriptors.List.Add(item);
						charLayers.List.Add(item2);
						signRenderingData.totalComplexity += 2.275f;
						signRenderingData.totalDraws++;
						zero += vector2 * num6;
					}
				}
			}
			Vector2 vector3 = -0.5f * vector;
			for (int k = count; k < charLayers.List.Count; k++)
			{
				CharLayer value2 = charLayers.List[k];
				value2.position += vector3;
				charLayers.List[k] = value2;
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void PushWarps(SignData.SignTransform parentTransform, SignData.SignLayer layer)
		{
			layerDescriptors.List.Add(new LayerDescriptor
			{
				type = 2,
				index = -1
			});
			signRenderingData.totalComplexity += 0.5f;
			uvStackIndex++;
			layerDescriptors.List.Add(new LayerDescriptor
			{
				type = 10,
				index = transformWarps.List.Count
			});
			SignData.SignTransform signTransform = parentTransform * layer.transform;
			transformWarps.List.Add(new TransformWarp
			{
				position = signTransform.position,
				rotation = signTransform.rotation * (MathF.PI / 180f),
				scale = signTransform.scale
			});
			signRenderingData.totalComplexity += 1.3f;
			signRenderingData.totalWarps++;
			for (int num = layer.warps.Count - 1; num >= 0; num--)
			{
				SignData.SignWarp signWarp = layer.warps[num];
				if (!(signWarp is SignData.SkewWarp skewWarp))
				{
					if (!(signWarp is SignData.BulgeWarp bulgeWarp))
					{
						if (!(signWarp is SignData.TwirlWarp twirlWarp))
						{
							if (!(signWarp is SignData.KaleidoWarp kaleidoWarp))
							{
								if (!(signWarp is SignData.PerspectiveWarp perspectiveWarp))
								{
									if (!(signWarp is SignData.ArcWarp arcWarp))
									{
										if (!(signWarp is SignData.StretchWarp stretchWarp))
										{
											if (signWarp is SignData.GridWarp gridWarp)
											{
												layerDescriptors.List.Add(new LayerDescriptor
												{
													type = 18,
													index = gridWarps.List.Count
												});
												gridWarps.List.Add(new GridWarp
												{
													offset = gridWarp.offset,
													rotation = MathF.PI / 180f * gridWarp.rotation,
													scale = gridWarp.scale,
													mode = (int)gridWarp.mode,
													aspect = gridWarp.aspect,
													shift = gridWarp.shift
												});
												float num2 = gridWarp.mode switch
												{
													SignData.GridWarp.Mode.Column => 1.26f, 
													SignData.GridWarp.Mode.Rectangle => 1.46f, 
													SignData.GridWarp.Mode.Hex => 1.66f, 
													_ => 1.46f, 
												};
												signRenderingData.totalComplexity += 0.5f + num2;
											}
											else
											{
												Log.Error($"Unsupported layer warp type at index {num}.");
											}
										}
										else
										{
											layerDescriptors.List.Add(new LayerDescriptor
											{
												type = 17,
												index = stretchWarps.List.Count
											});
											stretchWarps.List.Add(new StretchWarp
											{
												position = stretchWarp.offset,
												rotation = MathF.PI / 180f * stretchWarp.rotation,
												distance = stretchWarp.distance,
												width = stretchWarp.distance * stretchWarp.width,
												exponent = stretchWarp.exponent
											});
											signRenderingData.totalComplexity += 2f;
										}
									}
									else
									{
										layerDescriptors.List.Add(new LayerDescriptor
										{
											type = 16,
											index = arcWarps.List.Count
										});
										arcWarps.List.Add(new ArcWarp
										{
											rotation = MathF.PI / 180f * arcWarp.rotation,
											radius = arcWarp.radius,
											width = arcWarp.width
										});
										signRenderingData.totalComplexity += 1.95f;
									}
								}
								else
								{
									layerDescriptors.List.Add(new LayerDescriptor
									{
										type = 15,
										index = perspectiveWarps.List.Count
									});
									perspectiveWarps.List.Add(new PerspectiveWarp
									{
										rotation = MathF.PI / 180f * perspectiveWarp.rotation,
										strength = Mathf.Lerp(0.01f, 5f, Mathf.Pow(1f - perspectiveWarp.strength, 2f))
									});
									signRenderingData.totalComplexity += 1.96f;
								}
							}
							else
							{
								layerDescriptors.List.Add(new LayerDescriptor
								{
									type = 14,
									index = kaleidoWarps.List.Count
								});
								kaleidoWarps.List.Add(new KaleidoWarp
								{
									position = kaleidoWarp.offset * kaleidoWarp.offsetScale,
									angle = MathF.PI / 180f * (360f / (float)kaleidoWarp.sides),
									offset = MathF.PI / 180f * (kaleidoWarp.rotation + 360f)
								});
								signRenderingData.totalComplexity += 1.76f;
							}
						}
						else
						{
							layerDescriptors.List.Add(new LayerDescriptor
							{
								type = 13,
								index = twirlWarps.List.Count
							});
							twirlWarps.List.Add(new TwirlWarp
							{
								position = twirlWarp.offset,
								amount = 20f * twirlWarp.amount,
								frequency = 100f * twirlWarp.frequency
							});
							signRenderingData.totalComplexity += 1.45f;
						}
					}
					else
					{
						layerDescriptors.List.Add(new LayerDescriptor
						{
							type = 12,
							index = bulgeWarps.List.Count
						});
						bulgeWarps.List.Add(new BulgeWarp
						{
							position = bulgeWarp.offset,
							amount = bulgeWarp.amount
						});
						signRenderingData.totalComplexity += 1.78f;
					}
				}
				else
				{
					layerDescriptors.List.Add(new LayerDescriptor
					{
						type = 11,
						index = skewWarps.List.Count
					});
					skewWarps.List.Add(new SkewWarp
					{
						rotation = MathF.PI / 180f * skewWarp.rotation,
						amount = skewWarp.amount
					});
					signRenderingData.totalComplexity += 1.475f;
				}
				signRenderingData.totalWarps++;
			}
		}
	}

	public void AddFilteredSignIdsToList(string libraryId, string nameFilter, List<GlobalSignId> signIds)
	{
		if (!signLibraries.TryGetValue(libraryId, out var value))
		{
			Log.Error("Failed to retrieve sign IDs. Library '" + libraryId + "' not found.");
			return;
		}
		foreach (SignData value2 in value.signs.Values)
		{
			if (string.IsNullOrEmpty(nameFilter) || value2.name.ContainsCaseInsensitive(nameFilter))
			{
				GlobalSignId item = new GlobalSignId(libraryId, value2.guid);
				signIds.Add(item);
			}
		}
	}

	public bool TryGetSignData(GlobalSignId signId, out SignData signData)
	{
		if (!signLibraries.TryGetValue(signId.libraryId, out var value))
		{
			signData = null;
			return false;
		}
		return value.signs.TryGetValue(signId.signGuid, out signData);
	}

	public bool HasSignLibrary(string libraryId)
	{
		return signLibraries.ContainsKey(libraryId);
	}

	public bool TryGetSignLibrary(string libraryId, out SignLibrary library)
	{
		return signLibraries.TryGetValue(libraryId, out library);
	}
}
