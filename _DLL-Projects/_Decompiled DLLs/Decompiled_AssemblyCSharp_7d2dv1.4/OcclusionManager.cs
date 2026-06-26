using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class OcclusionManager : MonoBehaviour
{
	public struct RenderItem
	{
		public Renderer renderer;

		public ShadowCastingMode shadowMode;
	}

	public class OcclusionEntry
	{
		public Renderer[] allRenderers;

		public RenderItem[] renderItems;

		public int renderItemsUsed;

		public float cullStartDistSq;

		public float extentsTotalMax;

		public int index;

		public int matrixUnitIndex;

		public int matrixSubIndex;

		public bool isAreaFound;

		public Vector3 centerPos;

		public Vector3 size;

		public bool isForceOn;

		public bool isVisible;
	}

	public class OccludeeZone
	{
		public float extentsTotalMax;

		public OccludeeLayer[] layers = new OccludeeLayer[32];

		public List<Transform> addTs = new List<Transform>();

		public List<Transform> removeTs = new List<Transform>();

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cLayerShift = 3;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cLayerH = 8;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cLayerCount = 32;

		public int GetIndex(float y)
		{
			int num = (int)y >> 3;
			if ((uint)num >= 32u)
			{
				if (num <= 0)
				{
					return 0;
				}
				return 31;
			}
			return num;
		}

		public void AddTransform(Transform t)
		{
			int index = GetIndex(t.position.y + Origin.position.y);
			OccludeeLayer occludeeLayer = layers[index];
			if (occludeeLayer == null)
			{
				occludeeLayer = new OccludeeLayer();
				layers[index] = occludeeLayer;
			}
			occludeeLayer.isOld = true;
			OccludeeRenderers occludeeRenderers = occludeeLayer.AddTransform(t);
			t.GetComponentsInChildren(includeInactive: true, tempRenderers);
			if (tempRenderers.Count == 0)
			{
				LogOcclusion("AddTransform {0} tempRenderers 0", t.name);
			}
			foreach (Renderer tempRenderer in tempRenderers)
			{
				if (!(tempRenderer is ParticleSystemRenderer) && tempRenderer.shadowCastingMode != ShadowCastingMode.ShadowsOnly && !tempRenderer.CompareTag("NoOcclude"))
				{
					occludeeRenderers.renderers.Add(tempRenderer);
				}
			}
			tempRenderers.Clear();
		}

		public void RemoveTransform(Transform t)
		{
			int hashCode = t.GetHashCode();
			if ((bool)t)
			{
				int index = GetIndex(t.position.y + Origin.position.y);
				OccludeeLayer occludeeLayer = layers[index];
				if (occludeeLayer != null && occludeeLayer.renderers.Remove(hashCode))
				{
					occludeeLayer.isOld = true;
					return;
				}
			}
			for (int i = 0; i < layers.Length; i++)
			{
				OccludeeLayer occludeeLayer2 = layers[i];
				if (occludeeLayer2 != null && occludeeLayer2.renderers.Remove(hashCode))
				{
					occludeeLayer2.isOld = true;
					break;
				}
			}
		}
	}

	public class OccludeeLayer
	{
		public Dictionary<int, OccludeeRenderers> renderers = new Dictionary<int, OccludeeRenderers>();

		public LinkedListNode<OcclusionEntry> node;

		public bool isOld;

		public OccludeeRenderers AddTransform(Transform t)
		{
			int hashCode = t.GetHashCode();
			if (renderers.TryGetValue(hashCode, out var value))
			{
				Log.Warning("OccludeeLayer AddTransform {0} {1} exists", t ? t.name : "", hashCode);
				return value;
			}
			value = new OccludeeRenderers();
			renderers.Add(hashCode, value);
			return value;
		}
	}

	public class OccludeeRenderers
	{
		public List<Renderer> renderers = new List<Renderer>();
	}

	public class OccludeeEntity
	{
		public Entity entity;

		public Vector3 pos;

		public LinkedListNode<OcclusionEntry> entry;
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDepthRTWidth = 256;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cBoundsScale = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCameraEnableAllAngleCos = 0.94f;

	public static OcclusionManager Instance;

	public bool isEnabled;

	public Material depthTestMat;

	public int visibleEntryCount;

	public int totalEntryCount;

	public bool forceAllVisible;

	public bool forceAllHidden;

	public bool cullEverything;

	public bool cullChunkEntities = true;

	public bool cullDecorations = true;

	public bool cullEntities;

	public bool cullDistantChunks;

	public bool cullDistantTerrain;

	public bool cullPrefabs;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxUnits = 511;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxEntries = 4088;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Matrix4x4 tinyMatrix;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ComputeBuffer[] counterBuffer = new ComputeBuffer[3];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int counterBufferCurrentIndex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public uint[] initialData;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasVisibilityData;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public uint[] visibleData;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Camera sourceDepthCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CommandBuffer depthCopyCmdBuf;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<OcclusionEntry> entries = new List<OcclusionEntry>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LinkedList<OcclusionEntry> freeEntries = new LinkedList<OcclusionEntry>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LinkedList<OcclusionEntry> usedEntries = new LinkedList<OcclusionEntry>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Matrix4x4[]> objectMatrixLists = new List<Matrix4x4[]>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MaterialPropertyBlock[] materialBlocks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject cubeObj;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Mesh cubeMesh;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int errorFrame;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool gpuCullingEnabled;

	public Camera depthCamera;

	public Material depthCopyMat;

	public RenderTexture depthRT;

	public RenderTexture depthCopyRT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOcclusionChecking;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isCameraChanged;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Matrix4x4 areaMatrix = Matrix4x4.identity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int hugeErrorCount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<OccludeeZone> pendingZones = new List<OccludeeZone>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Renderer> tempRenderers = new List<Renderer>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, OccludeeEntity> entities = new Dictionary<int, OccludeeEntity>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 camDirVec;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Action<AsyncGPUReadbackRequest> onRequestDelegate;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDebugView;

	public static void Load()
	{
		UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Occlusion/Occlusion"));
	}

	public void EnableCulling(bool isCulling)
	{
		gpuCullingEnabled = isCulling;
	}

	public void SetMultipleCameras(bool isMultiple)
	{
		if (isEnabled)
		{
			gpuCullingEnabled = !isMultiple;
			if (isMultiple)
			{
				SetRenderersEnabled(isEnabled: true);
			}
		}
	}

	public void WorldChanging(bool isEditWorld)
	{
		isEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxOcclusion);
		if (isEditWorld)
		{
			isEnabled = false;
		}
		if (GameManager.IsDedicatedServer)
		{
			isEnabled = false;
		}
		if (isEnabled)
		{
			if (!SystemInfo.supportsAsyncGPUReadback)
			{
				Log.Warning("Occlusion: !supportsAsyncGPUReadback");
				isEnabled = false;
			}
			if (!SystemInfo.supportsComputeShaders)
			{
				Log.Warning("Occlusion: !supportsComputeShaders");
				isEnabled = false;
			}
		}
		if (isEnabled && !depthTestMat)
		{
			depthTestMat = Resources.Load<Material>("Occlusion/OcclusionDepthTest");
			if (depthTestMat == null)
			{
				Log.Error("Occlusion: Missing OcclusionDepthTest mat");
				isEnabled = false;
			}
		}
		if (!isEnabled)
		{
			base.gameObject.SetActive(value: false);
			Log.Out("Occlusion: Disabled");
		}
		else
		{
			base.gameObject.SetActive(value: true);
			Log.Out("Occlusion: Enabled");
		}
		gpuCullingEnabled = isEnabled;
		if (!isEnabled)
		{
			ClearCullingTypes();
			return;
		}
		cullChunkEntities = true;
		cullDecorations = true;
		if (cullEverything)
		{
			cullChunkEntities = true;
			cullEntities = true;
			cullDistantChunks = true;
			cullDistantTerrain = true;
			cullDecorations = true;
			cullPrefabs = true;
		}
		hugeErrorCount = 0;
	}

	public void AddChunkTransforms(Chunk chunk, List<Transform> transforms)
	{
		OccludeeZone occludeeZone = chunk.occludeeZone;
		if (occludeeZone == null)
		{
			occludeeZone = new OccludeeZone();
			occludeeZone.extentsTotalMax = 24f;
			chunk.occludeeZone = occludeeZone;
		}
		for (int num = transforms.Count - 1; num >= 0; num--)
		{
			Transform transform = transforms[num];
			if ((bool)transform)
			{
				occludeeZone.AddTransform(transform);
			}
		}
		tempRenderers.Clear();
		UpdateZoneRegistration(occludeeZone);
	}

	public void RemoveChunkTransforms(Chunk chunk, List<Transform> transforms)
	{
		OccludeeZone occludeeZone = chunk.occludeeZone;
		if (occludeeZone != null)
		{
			for (int num = transforms.Count - 1; num >= 0; num--)
			{
				Transform t = transforms[num];
				occludeeZone.RemoveTransform(t);
			}
			UpdateZoneRegistration(occludeeZone);
		}
	}

	public void RemoveChunk(Chunk chunk)
	{
		OccludeeZone occludeeZone = chunk.occludeeZone;
		if (occludeeZone != null)
		{
			RemoveFullZone(occludeeZone);
			chunk.occludeeZone = null;
		}
	}

	public void RemoveFullZone(OccludeeZone zone)
	{
		for (int num = zone.layers.Length - 1; num >= 0; num--)
		{
			OccludeeLayer occludeeLayer = zone.layers[num];
			if (occludeeLayer != null && occludeeLayer.node != null)
			{
				UnregisterOccludee(occludeeLayer.node);
				occludeeLayer.node = null;
			}
		}
	}

	public void AddDeco(DecoChunk chunk, List<Transform> addTs)
	{
		OccludeeZone occludeeZone = chunk.occludeeZone;
		if (occludeeZone == null)
		{
			occludeeZone = new OccludeeZone();
			occludeeZone.extentsTotalMax = 189.44f;
			chunk.occludeeZone = occludeeZone;
		}
		if (!pendingZones.Contains(occludeeZone))
		{
			pendingZones.Add(occludeeZone);
		}
		occludeeZone.addTs.AddRange(addTs);
	}

	public void RemoveDeco(DecoChunk chunk, Transform removeT)
	{
		OccludeeZone occludeeZone = chunk.occludeeZone;
		if (occludeeZone == null)
		{
			Log.Error("RemoveDeco !zone");
			return;
		}
		if (!pendingZones.Contains(occludeeZone))
		{
			pendingZones.Add(occludeeZone);
		}
		occludeeZone.addTs.Remove(removeT);
		occludeeZone.removeTs.Add(removeT);
	}

	public void RemoveDecoChunk(DecoChunk chunk)
	{
		OccludeeZone occludeeZone = chunk.occludeeZone;
		if (occludeeZone != null)
		{
			RemoveFullZone(occludeeZone);
			chunk.occludeeZone = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateZones()
	{
		if (pendingZones.Count > 0)
		{
			for (int num = pendingZones.Count - 1; num >= 0; num--)
			{
				OccludeeZone zone = pendingZones[num];
				UpdateZonePending(zone);
			}
			pendingZones.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateZonePending(OccludeeZone zone)
	{
		for (int num = zone.removeTs.Count - 1; num >= 0; num--)
		{
			Transform t = zone.removeTs[num];
			zone.RemoveTransform(t);
		}
		zone.removeTs.Clear();
		for (int num2 = zone.addTs.Count - 1; num2 >= 0; num2--)
		{
			Transform transform = zone.addTs[num2];
			if ((bool)transform)
			{
				zone.AddTransform(transform);
			}
		}
		zone.addTs.Clear();
		UpdateZoneRegistration(zone);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateZoneRegistration(OccludeeZone zone)
	{
		for (int num = zone.layers.Length - 1; num >= 0; num--)
		{
			OccludeeLayer occludeeLayer = zone.layers[num];
			if (occludeeLayer != null && occludeeLayer.isOld)
			{
				occludeeLayer.isOld = false;
				if (occludeeLayer.node != null)
				{
					UnregisterOccludee(occludeeLayer.node);
					occludeeLayer.node = null;
				}
				if (occludeeLayer.renderers.Count > 0)
				{
					List<Renderer> list = new List<Renderer>();
					foreach (KeyValuePair<int, OccludeeRenderers> renderer in occludeeLayer.renderers)
					{
						list.AddRange(renderer.Value.renderers);
					}
					occludeeLayer.node = RegisterOccludee(list.ToArray(), zone.extentsTotalMax);
				}
			}
		}
	}

	public static void AddEntity(EntityAlive _ea, float extentsTotalMax = 32f)
	{
		OcclusionManager instance = Instance;
		if (!(instance != null))
		{
			return;
		}
		_ea.GetComponentsInChildren(includeInactive: true, tempRenderers);
		for (int num = tempRenderers.Count - 1; num >= 0; num--)
		{
			if (tempRenderers[num] is ParticleSystemRenderer)
			{
				tempRenderers.RemoveAt(num);
			}
			else if (tempRenderers[num].gameObject.CompareTag("NoOcclude"))
			{
				tempRenderers.RemoveAt(num);
			}
		}
		if (tempRenderers.Count > 0)
		{
			OccludeeEntity occludeeEntity = new OccludeeEntity();
			occludeeEntity.entity = _ea;
			occludeeEntity.pos = _ea.position;
			occludeeEntity.entry = instance.RegisterOccludee(tempRenderers.ToArray(), extentsTotalMax);
			instance.entities[_ea.entityId] = occludeeEntity;
			tempRenderers.Clear();
		}
	}

	public static void RemoveEntity(EntityAlive _ea)
	{
		OcclusionManager instance = Instance;
		if (!(instance != null))
		{
			return;
		}
		if (!instance.entities.TryGetValue(_ea.entityId, out var value))
		{
			Log.Warning("Occlusion: RemoveEntity {0} missing", _ea);
			return;
		}
		instance.entities.Remove(_ea.entityId);
		if (value.entry != null)
		{
			instance.UnregisterOccludee(value.entry);
			value.entry = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEntities()
	{
		foreach (KeyValuePair<int, OccludeeEntity> entity in entities)
		{
			OccludeeEntity value = entity.Value;
			if ((value.pos - value.entity.position).sqrMagnitude > 1f)
			{
				value.pos = value.entity.position;
				value.entry.Value.isAreaFound = false;
			}
		}
	}

	public LinkedListNode<OcclusionEntry> RegisterOccludee(Renderer[] renderers, float extentsTotalMax = 32f)
	{
		if (!isEnabled)
		{
			return null;
		}
		LinkedListNode<OcclusionEntry> first = freeEntries.First;
		if (first != null)
		{
			freeEntries.RemoveFirst();
			usedEntries.AddFirst(first);
			OcclusionEntry value = first.Value;
			value.allRenderers = renderers;
			float num = extentsTotalMax * 0.55f;
			value.cullStartDistSq = num * num;
			value.extentsTotalMax = extentsTotalMax;
			value.centerPos.y = -9999f;
			value.isAreaFound = false;
			value.isForceOn = true;
			value.isVisible = true;
			totalEntryCount++;
			return first;
		}
		if (Time.frameCount != errorFrame)
		{
			errorFrame = Time.frameCount;
			Log.Warning("Occlusion used all entries");
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcArea(OcclusionEntry entry)
	{
		if (entry.renderItems == null)
		{
			RenderItem[] array = (entry.renderItems = new RenderItem[entry.allRenderers.Length]);
			int num = 0;
			RenderItem renderItem = default(RenderItem);
			for (int num2 = entry.allRenderers.Length - 1; num2 >= 0; num2--)
			{
				Renderer renderer = entry.allRenderers[num2];
				if ((bool)renderer && !renderer.forceRenderingOff)
				{
					ShadowCastingMode shadowCastingMode = renderer.shadowCastingMode;
					if (shadowCastingMode != ShadowCastingMode.ShadowsOnly)
					{
						Vector3 extents = renderer.bounds.extents;
						if (extents.x <= 29f && extents.y <= 35f && extents.z <= 29f)
						{
							renderItem.renderer = renderer;
							renderItem.shadowMode = shadowCastingMode;
							array[num] = renderItem;
							num++;
						}
					}
				}
			}
			entry.renderItemsUsed = num;
		}
		Bounds bounds = default(Bounds);
		bool flag = false;
		for (int num3 = entry.renderItemsUsed - 1; num3 >= 0; num3--)
		{
			Renderer renderer2 = entry.renderItems[num3].renderer;
			if ((bool)renderer2)
			{
				Bounds bounds2 = renderer2.bounds;
				if (bounds2.extents.sqrMagnitude > 0.001f)
				{
					if (!flag)
					{
						bounds = bounds2;
						flag = true;
					}
					else
					{
						bounds.Encapsulate(bounds2);
						if (bounds.extents.x > entry.extentsTotalMax || bounds.extents.z > entry.extentsTotalMax)
						{
							hugeErrorCount++;
							return;
						}
					}
				}
			}
		}
		entry.centerPos = bounds.center;
		Vector3 extents2 = bounds.extents;
		if (extents2.x < 2f)
		{
			extents2.x = 2f;
		}
		if (extents2.y < 2f)
		{
			extents2.y = 2f;
		}
		if (extents2.z < 2f)
		{
			extents2.z = 2f;
		}
		entry.size = extents2 * 4f;
		areaMatrix.m03 = entry.centerPos.x;
		areaMatrix.m13 = entry.centerPos.y;
		areaMatrix.m23 = entry.centerPos.z;
		areaMatrix.m00 = entry.size.x;
		areaMatrix.m11 = entry.size.y;
		areaMatrix.m22 = entry.size.z;
		objectMatrixLists[entry.matrixUnitIndex][entry.matrixSubIndex] = areaMatrix;
		entry.isAreaFound = true;
	}

	public void UnregisterOccludee(LinkedListNode<OcclusionEntry> node)
	{
		if (node == null)
		{
			return;
		}
		OcclusionEntry value = node.Value;
		usedEntries.Remove(node);
		freeEntries.AddFirst(node);
		value.allRenderers = null;
		if (value.renderItems != null)
		{
			for (int num = value.renderItemsUsed - 1; num >= 0; num--)
			{
				RenderItem renderItem = value.renderItems[num];
				if ((bool)renderItem.renderer)
				{
					renderItem.renderer.forceRenderingOff = false;
					renderItem.renderer.shadowCastingMode = renderItem.shadowMode;
				}
			}
			value.renderItems = null;
		}
		objectMatrixLists[value.matrixUnitIndex][value.matrixSubIndex] = tinyMatrix;
		totalEntryCount--;
	}

	public void OriginChanged(Vector3 offset)
	{
		for (LinkedListNode<OcclusionEntry> linkedListNode = usedEntries.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			OcclusionEntry value = linkedListNode.Value;
			value.centerPos += offset;
			areaMatrix.m03 = value.centerPos.x;
			areaMatrix.m13 = value.centerPos.y;
			areaMatrix.m23 = value.centerPos.z;
			areaMatrix.m00 = value.size.x;
			areaMatrix.m11 = value.size.y;
			areaMatrix.m22 = value.size.z;
			objectMatrixLists[value.matrixUnitIndex][value.matrixSubIndex] = areaMatrix;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		Instance = this;
		onRequestDelegate = OnRequest;
		tinyMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(0.0001f, 0.0001f, 0.0001f));
		cubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cubeMesh = cubeObj.GetComponent<MeshFilter>().sharedMesh;
		cubeObj.SetActive(value: false);
		cubeObj.transform.SetParent(base.gameObject.transform);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 4088; i++)
		{
			entries.Add(new OcclusionEntry());
			OcclusionEntry occlusionEntry = entries[i];
			if (num2 == 511)
			{
				num++;
				num2 = 0;
			}
			occlusionEntry.index = i;
			occlusionEntry.matrixUnitIndex = num;
			occlusionEntry.matrixSubIndex = num2++;
			freeEntries.AddLast(occlusionEntry);
		}
		int num3 = 128;
		initialData = new uint[num3];
		visibleData = new uint[num3];
		for (int j = 0; j <= num; j++)
		{
			objectMatrixLists.Add(new Matrix4x4[511]);
		}
		for (int k = 0; k < initialData.Length; k++)
		{
			initialData[k] = 0u;
		}
		for (int l = 0; l < counterBuffer.Length; l++)
		{
			ComputeBuffer computeBuffer = new ComputeBuffer(num3, 4, ComputeBufferType.Default);
			computeBuffer.SetData(initialData);
			counterBuffer[l] = computeBuffer;
		}
		materialBlocks = new MaterialPropertyBlock[objectMatrixLists.Count];
		int num4 = 0;
		for (int m = 0; m < objectMatrixLists.Count; m++)
		{
			materialBlocks[m] = new MaterialPropertyBlock();
			materialBlocks[m].SetInt("_InstanceOffset", num4);
			num4 += objectMatrixLists[m].Length;
		}
		depthCamera = GetComponent<Camera>();
		depthCamera.enabled = false;
		GameOptionsManager.ResolutionChanged += OnResolutionChanged;
		CreateDepthRT();
		Log.Out("Occlusion: Awake");
	}

	public void SetSourceDepthCamera(Camera _camera)
	{
		if (depthCopyCmdBuf != null)
		{
			if (sourceDepthCamera != null)
			{
				sourceDepthCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, depthCopyCmdBuf);
			}
			_camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, depthCopyCmdBuf);
		}
		sourceDepthCamera = _camera;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearCullingTypes()
	{
		cullEverything = false;
		cullChunkEntities = false;
		cullEntities = false;
		cullDistantChunks = false;
		cullDistantTerrain = false;
		cullDecorations = false;
		cullPrefabs = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnResolutionChanged(int _width, int _height)
	{
		CreateDepthRT();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateDepthRT()
	{
		float num = (float)Screen.width / (float)Screen.height;
		if ((bool)depthRT)
		{
			depthRT.Release();
			depthRT = null;
		}
		depthRT = new RenderTexture(256, (int)(256f / num), 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		depthRT.name = "Occlusion";
		depthRT.Create();
		depthCamera.targetTexture = depthRT;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateVisibility(uint[] data)
	{
		visibleEntryCount = 0;
		hasVisibilityData = false;
		for (LinkedListNode<OcclusionEntry> linkedListNode = usedEntries.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			OcclusionEntry value = linkedListNode.Value;
			uint num = 0u;
			uint num2 = (uint)value.index >> 5;
			int num3 = value.index & 0x1F;
			if ((data[num2] & (1 << num3)) != 0L)
			{
				num = 1u;
			}
			if (num != 0 || value.isForceOn)
			{
				if (!value.isVisible)
				{
					value.isVisible = true;
					for (int num4 = value.renderItemsUsed - 1; num4 >= 0; num4--)
					{
						RenderItem renderItem = value.renderItems[num4];
						if ((bool)renderItem.renderer)
						{
							if (renderItem.shadowMode != ShadowCastingMode.Off)
							{
								renderItem.renderer.shadowCastingMode = renderItem.shadowMode;
							}
							else
							{
								renderItem.renderer.forceRenderingOff = false;
							}
						}
					}
				}
				visibleEntryCount++;
			}
			else if (value.isVisible)
			{
				value.isVisible = false;
				for (int num5 = value.renderItemsUsed - 1; num5 >= 0; num5--)
				{
					RenderItem renderItem2 = value.renderItems[num5];
					if ((bool)renderItem2.renderer)
					{
						if (renderItem2.shadowMode != ShadowCastingMode.Off)
						{
							renderItem2.renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
						}
						else
						{
							renderItem2.renderer.forceRenderingOff = true;
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		if (sourceDepthCamera != null && depthCopyCmdBuf != null)
		{
			sourceDepthCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, depthCopyCmdBuf);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		if (sourceDepthCamera != null && depthCopyCmdBuf != null)
		{
			sourceDepthCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, depthCopyCmdBuf);
		}
		SetRenderersEnabled(isEnabled: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		isEnabled = false;
		gpuCullingEnabled = false;
		GameOptionsManager.ResolutionChanged -= OnResolutionChanged;
		if ((bool)depthRT)
		{
			depthRT.Release();
			depthRT = null;
		}
		if ((bool)depthCopyRT)
		{
			depthCopyRT.Release();
			depthCopyRT = null;
		}
		for (int i = 0; i < counterBuffer.Length; i++)
		{
			if (counterBuffer[i] != null)
			{
				counterBuffer[i].Release();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderOccludees(Camera renderCamera, int layer)
	{
		Vector3 position = sourceDepthCamera.transform.position;
		for (LinkedListNode<OcclusionEntry> linkedListNode = usedEntries.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			OcclusionEntry value = linkedListNode.Value;
			if ((position - value.centerPos).sqrMagnitude < value.cullStartDistSq)
			{
				value.isForceOn = true;
			}
			else if (!value.isAreaFound)
			{
				CalcArea(value);
			}
			else
			{
				value.isForceOn = false;
			}
		}
		Graphics.SetRandomWriteTarget(1, counterBuffer[counterBufferCurrentIndex]);
		for (int i = 0; i < objectMatrixLists.Count; i++)
		{
			Matrix4x4[] array = objectMatrixLists[i];
			Graphics.DrawMeshInstanced(cubeMesh, 0, depthTestMat, array, array.Length, materialBlocks[i], ShadowCastingMode.Off, receiveShadows: false, layer, renderCamera);
		}
	}

	public void LocalPlayerOnPreCull()
	{
		if (!gpuCullingEnabled)
		{
			return;
		}
		if (forceAllVisible || forceAllHidden)
		{
			SetRenderersEnabled(forceAllVisible);
			visibleEntryCount = totalEntryCount;
		}
		else if (!isCameraChanged)
		{
			Vector3 forward = Camera.current.transform.forward;
			if (Vector3.Dot(camDirVec, forward) < 0.94f)
			{
				SetRenderersEnabled(isEnabled: true);
			}
			else if (hasVisibilityData)
			{
				UpdateVisibility(visibleData);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnRenderObject()
	{
		if (!isOcclusionChecking && gpuCullingEnabled && GameManager.Instance.World != null && Camera.current == sourceDepthCamera)
		{
			UpdateEntities();
			UpdateZones();
			isOcclusionChecking = true;
			isCameraChanged = false;
			Transform transform = sourceDepthCamera.transform;
			camDirVec = transform.forward;
			depthCamera.transform.position = transform.position;
			depthCamera.transform.rotation = transform.rotation;
			depthCamera.fieldOfView = sourceDepthCamera.fieldOfView;
			depthCamera.nearClipPlane = sourceDepthCamera.nearClipPlane;
			depthCamera.farClipPlane = sourceDepthCamera.farClipPlane;
			if (depthCopyCmdBuf != null)
			{
				Graphics.Blit(depthCopyRT, depthRT, depthCopyMat, 1);
			}
			else
			{
				Graphics.Blit(null, depthRT, depthCopyMat, 2);
			}
			depthCamera.Render();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreCull()
	{
		counterBuffer[counterBufferCurrentIndex].SetData(initialData);
		RenderOccludees(depthCamera, 11);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPostRender()
	{
		Graphics.ClearRandomWriteTargets();
		AsyncGPUReadback.Request(counterBuffer[counterBufferCurrentIndex], onRequestDelegate);
		counterBufferCurrentIndex = (counterBufferCurrentIndex + 1) % counterBuffer.Length;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnRequest(AsyncGPUReadbackRequest req)
	{
		isOcclusionChecking = false;
		if (!isCameraChanged)
		{
			req.GetData<uint>().CopyTo(visibleData);
			hasVisibilityData = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRenderersEnabled(bool isEnabled)
	{
		isCameraChanged = true;
		hasVisibilityData = false;
		for (LinkedListNode<OcclusionEntry> linkedListNode = usedEntries.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			OcclusionEntry value = linkedListNode.Value;
			if (value.isVisible != isEnabled && value.renderItems != null)
			{
				value.isVisible = isEnabled;
				for (int num = value.renderItemsUsed - 1; num >= 0; num--)
				{
					RenderItem renderItem = value.renderItems[num];
					if ((bool)renderItem.renderer)
					{
						if (renderItem.shadowMode != ShadowCastingMode.Off)
						{
							renderItem.renderer.shadowCastingMode = (isEnabled ? renderItem.shadowMode : ShadowCastingMode.ShadowsOnly);
						}
						else
						{
							renderItem.renderer.forceRenderingOff = !isEnabled;
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogOcclusion(string format, params object[] args)
	{
		format = $"{GameManager.frameCount} OC {format}";
		Log.Warning(format, args);
	}

	public void ToggleDebugView()
	{
		isDebugView = !isDebugView;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGUI()
	{
		if (isDebugView && (bool)depthRT)
		{
			GUI.DrawTexture(new Rect(0f, 0f, 256f, 256 * depthRT.height / depthRT.width), depthRT);
			string text = $"{visibleEntryCount} of {usedEntries.Count}, huge {hugeErrorCount}";
			GUI.color = Color.black;
			GUI.Label(new Rect(1f, 1f, 256f, 256f), text);
			GUI.color = Color.white;
			GUI.Label(new Rect(0f, 0f, 256f, 256f), text);
		}
	}
}
