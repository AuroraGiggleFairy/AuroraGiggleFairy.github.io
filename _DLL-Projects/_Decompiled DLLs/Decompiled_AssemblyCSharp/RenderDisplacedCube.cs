using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RenderDisplacedCube
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cWireframeFadeMaxDistance = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTransparencyMax = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCubeScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderCubeType focusType;

	[PublicizedFrom(EAccessModifier.Private)]
	public MeshFilter[] meshFilters = new MeshFilter[6];

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject[] goSides = new GameObject[6];

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] cube = new Vector3[8]
	{
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 1f, 0f),
		new Vector3(1f, 1f, 0f),
		new Vector3(1f, 0f, 0f),
		new Vector3(0f, 0f, 1f),
		new Vector3(0f, 1f, 1f),
		new Vector3(1f, 1f, 1f),
		new Vector3(1f, 0f, 1f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] cubeCopy = new Vector3[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[,] sides = new int[6, 4]
	{
		{ 0, 3, 2, 1 },
		{ 7, 4, 5, 6 },
		{ 4, 0, 1, 5 },
		{ 3, 7, 6, 2 },
		{ 6, 5, 1, 2 },
		{ 4, 7, 3, 0 }
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3[]> vertices = new List<Vector3[]>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2[] uvs = new Vector2[4]
	{
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(1f, 1f),
		new Vector2(0f, 1f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] indices = new int[12]
	{
		0, 2, 1, 3, 2, 0, 0, 1, 2, 3,
		0, 2
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform transformParent;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform transformWireframeCube;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform transformBlockPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform transformFocusCubePrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public Shader objectShader;

	[PublicizedFrom(EAccessModifier.Private)]
	public Shader objectShader_TA;

	[PublicizedFrom(EAccessModifier.Private)]
	public Shader terrainShader;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Material> blockPreviewMats = new List<Material>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTimeFocusTransformMoved;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material previewMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] arrFocusBoxDisplacedVertices = new Vector3[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue focusTransformBlockValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i focusTransformPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bUseFocusCubePrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public TextureFullArray focusedTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public float transparencyFade;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 transformLastPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public float drawStability;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockFace focusedBlockFace;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue cachedFocusBlockValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i cachedFocusBlockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 multiDim;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 localPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject cachedLandClaimBoundary;

	public RenderDisplacedCube(Transform _focusCubePrefab)
	{
		objectShader = GlobalAssets.FindShader("Game/Glow");
		objectShader_TA = GlobalAssets.FindShader("Game/OverlayHighlight_TA");
		terrainShader = GlobalAssets.FindShader("Game/TerrainGlow");
		transformParent = new GameObject("FocusBox").transform;
		Origin.Add(transformParent, 1);
		transformWireframeCube = new GameObject("Wireframe").transform;
		transformWireframeCube.parent = transformParent;
		transformWireframeCube.localPosition = Vector3.zero;
		transformFocusCubePrefab = _focusCubePrefab;
		if (transformFocusCubePrefab != null)
		{
			transformFocusCubePrefab.parent = transformWireframeCube;
			transformFocusCubePrefab.localPosition = new Vector3(0.5f, 0.01f, 0.5f);
			bUseFocusCubePrefab = true;
			return;
		}
		transformWireframeCube.localScale = new Vector3(1.1f, 1.1f, 1.1f);
		vertices.Add(new Vector3[4]);
		vertices.Add(new Vector3[4]);
		vertices.Add(new Vector3[4]);
		vertices.Add(new Vector3[4]);
		vertices.Add(new Vector3[4]);
		vertices.Add(new Vector3[4]);
		Material material = new Material(Shader.Find("Transparent/Diffuse"));
		material.SetTexture("_MainTex", Resources.Load("Textures/focusbox") as Texture2D);
		for (int i = 0; i < 6; i++)
		{
			goSides[i] = new GameObject();
			goSides[i].name = "Side_" + i;
			goSides[i].transform.parent = transformWireframeCube;
			goSides[i].transform.localScale = Vector3.one;
			meshFilters[i] = goSides[i].AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = goSides[i].AddComponent<MeshRenderer>();
			meshRenderer.material = material;
			meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			meshRenderer.receiveShadows = true;
			meshFilters[i].mesh.Clear();
			calcVerticesForSide(i, cube, null);
			meshFilters[i].mesh.uv = uvs;
			meshFilters[i].mesh.triangles = indices;
			meshFilters[i].mesh.RecalculateNormals();
		}
	}

	public void Cleanup()
	{
		if (previewMaterial != null)
		{
			Object.Destroy(previewMaterial);
			previewMaterial = null;
		}
		if (transformParent != null)
		{
			Origin.Remove(transformParent);
			Object.Destroy(transformParent.gameObject);
			transformParent = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void calcVerticesForSide(int _idx, Vector3[] _cube, Vector3[] _offsets)
	{
		for (int i = 0; i < 4; i++)
		{
			vertices[_idx][i] = _cube[sides[_idx, i]] + ((_offsets != null) ? _offsets[sides[_idx, i]] : Vector3.zero);
		}
		meshFilters[_idx].mesh.vertices = vertices[_idx];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void rebuildCubeOnBounds(BlockValue _blockValue)
	{
		BlockShape shape = _blockValue.Block.shape;
		if (!(shape is BlockShapeModelEntity) && !(shape is BlockShapeExt3dModel) && !(shape is BlockShapeNew))
		{
			multiDim = Vector3.one;
			localPos = Vector3.zero;
		}
		else
		{
			Bounds blockPlacementBounds = GameUtils.GetBlockPlacementBounds(_blockValue.Block);
			multiDim = blockPlacementBounds.size;
			localPos = blockPlacementBounds.center;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setFocusType(RenderCubeType _focusType)
	{
		if (!bUseFocusCubePrefab && _focusType != focusType)
		{
			focusType = _focusType;
			GameObject[] array = goSides;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(_focusType == RenderCubeType.FullBlockBothSides);
			}
			if (_focusType < RenderCubeType.FullBlockBothSides)
			{
				goSides[(int)_focusType].SetActive(value: true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GenerateLandClaimPreview(float scale)
	{
		cachedLandClaimBoundary = new GameObject("LandClaimBoundary");
		cachedLandClaimBoundary.transform.parent = transformFocusCubePrefab;
		cachedLandClaimBoundary.transform.localPosition = Vector3.zero;
		cachedLandClaimBoundary.transform.localRotation = Quaternion.identity;
		cachedLandClaimBoundary.transform.localScale = new Vector3(1f, 10000f, 1f) * scale / 1f;
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		Object.Destroy(gameObject.GetComponent<BoxCollider>());
		gameObject.transform.parent = cachedLandClaimBoundary.transform;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		Renderer component = gameObject.GetComponent<Renderer>();
		Material material = Resources.Load("Materials/LandClaimBoundary", typeof(Material)) as Material;
		component.material = material;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool update0(World _world, WorldRayHitInfo _hitInfo, Vector3i _focusBlockPos, EntityAlive _player, PersistentPlayerData _ppLocal, bool _bAlternativeBlockPos, BlockValue _holdingBlockValue, TextureFullArray _texture)
	{
		int clrIdx = _hitInfo.hit.clrIdx;
		_ = _hitInfo.hit;
		BlockPlacement.EnumRotationMode mode = BlockPlacement.EnumRotationMode.Advanced;
		int localRot = 0;
		if (_player.inventory.holdingItemData is ItemClassBlock.ItemBlockInventoryData)
		{
			mode = ((ItemClassBlock.ItemBlockInventoryData)_player.inventory.holdingItemData).mode;
			localRot = ((ItemClassBlock.ItemBlockInventoryData)_player.inventory.holdingItemData).localRot;
		}
		HitInfoDetails hitInfo = _hitInfo.hit.Clone();
		hitInfo.blockPos = _hitInfo.lastBlockPos;
		BlockPlacement.Result result = _holdingBlockValue.Block.BlockPlacementHelper.OnPlaceBlock(mode, localRot, _world, _holdingBlockValue, hitInfo, _player.GetPosition());
		_holdingBlockValue.rotation = result.blockValue.rotation;
		Vector3 zero = Vector3.zero;
		_world.GetBlock(clrIdx, _focusBlockPos);
		rebuildCubeOnBounds(result.blockValue);
		if (focusTransformBlockValue.Block.IsTerrainDecoration && _world.GetBlock(_focusBlockPos - Vector3i.up).Block.shape.IsTerrain())
		{
			sbyte density = _world.GetDensity(0, _focusBlockPos);
			sbyte density2 = _world.GetDensity(0, _focusBlockPos - Vector3i.up);
			zero.y += MarchingCubes.GetDecorationOffsetY(density, density2);
		}
		if (!_bAlternativeBlockPos)
		{
			_focusBlockPos = _holdingBlockValue.Block.GetFreePlacementPosition(_world, clrIdx, _focusBlockPos, _holdingBlockValue, _player);
		}
		Vector3 vector = _focusBlockPos.ToVector3() + new Vector3(0.5f, 0.5f, 0.5f);
		Vector3 vector2 = vector + zero;
		if ((vector2 - transformLastPos).sqrMagnitude > 64f)
		{
			transformLastPos = vector2;
		}
		Vector3 current = Vector3.LerpUnclamped(transformLastPos, vector2, 0.3f);
		current = (transformLastPos = Vector3.MoveTowards(current, vector2, 5f * Time.deltaTime));
		current -= Origin.position;
		bool flag = false;
		if (!focusTransformPosition.Equals(_focusBlockPos))
		{
			focusTransformPosition = _focusBlockPos;
			lastTimeFocusTransformMoved = Time.time;
			flag = true;
		}
		bool result2 = false;
		if (!_bAlternativeBlockPos && ItemClass.GetForId(_holdingBlockValue.type) != null && ItemClass.GetForId(_holdingBlockValue.type).IsBlock())
		{
			int num = (int)(0u | ((transformBlockPreview == null) ? 1u : 0u) | ((!focusTransformBlockValue.Equals(_holdingBlockValue)) ? 1u : 0u) | ((focusTransformBlockValue.rotation != _holdingBlockValue.rotation) ? 1u : 0u) | ((focusedBlockFace != _hitInfo.hit.blockFace) ? 1u : 0u) | ((focusedTexture != _texture) ? 1u : 0u)) | (flag ? 1 : 0);
			result2 = true;
			if (num != 0)
			{
				if (!focusTransformBlockValue.Equals(_holdingBlockValue) || focusedTexture != _texture)
				{
					transparencyFade = 0f;
				}
				DestroyPreview();
				focusedBlockFace = _hitInfo.hit.blockFace;
				for (int i = 0; i < arrFocusBoxDisplacedVertices.Length; i++)
				{
					arrFocusBoxDisplacedVertices[i] = Vector3.zero;
				}
				focusTransformBlockValue = _holdingBlockValue;
				focusedTexture = _texture;
				hitInfo.blockPos = _hitInfo.lastBlockPos;
				transformBlockPreview = ItemClassBlock.CreateMesh(null, _world, focusTransformBlockValue, arrFocusBoxDisplacedVertices, _focusBlockPos.ToVector3(), transformParent, BlockShape.MeshPurpose.Preview, _texture);
				if (transformBlockPreview != null)
				{
					disableAllComponents(transformBlockPreview);
					transformBlockPreview.gameObject.layer = LayerMask.NameToLayer("NotInReflections");
					Block block = focusTransformBlockValue.Block;
					if (block.shape.IsTerrain())
					{
						if (previewMaterial != null)
						{
							Object.DestroyImmediate(previewMaterial);
						}
						Renderer component = transformBlockPreview.GetComponent<Renderer>();
						if ((bool)component)
						{
							component.receiveShadows = true;
							previewMaterial = component.material;
							if ((bool)previewMaterial)
							{
								previewMaterial.shader = terrainShader;
								previewMaterial.SetFloat("_TexI", block.TerrainTAIndex);
							}
						}
					}
					else if (block.shape is BlockShapeModelEntity)
					{
						Color value = new Color(0.7f, 0.7f, 0.7f);
						int nameID = Shader.PropertyToID("_MainTex");
						Renderer[] componentsInChildren = transformBlockPreview.GetComponentsInChildren<Renderer>();
						for (int j = 0; j < componentsInChildren.Length; j++)
						{
							Material[] materials = componentsInChildren[j].materials;
							foreach (Material material in materials)
							{
								material.EnableKeyword("_EMISSION");
								if (material.HasProperty(nameID))
								{
									Texture texture = material.GetTexture(nameID);
									material.SetTexture("_EmissionMap", texture ? texture : Texture2D.whiteTexture);
									material.SetColor("_EmissionColor", value);
								}
							}
							blockPreviewMats.AddRange(materials);
						}
						Utils.SetLayerRecursively(transformBlockPreview.gameObject, 2);
					}
					else
					{
						if (previewMaterial != null)
						{
							Object.DestroyImmediate(previewMaterial);
						}
						Renderer component2 = transformBlockPreview.GetComponent<Renderer>();
						if ((bool)component2)
						{
							component2.receiveShadows = true;
							if (block.MeshIndex == 2)
							{
								component2.materials = new Material[1] { component2.materials[1] };
								previewMaterial = component2.material;
								if ((bool)previewMaterial)
								{
									previewMaterial.shader = objectShader;
									previewMaterial.SetFloat("_Cutoff", 0f);
									previewMaterial.SetTexture("_MainTex", previewMaterial.GetTexture("_Albedo"));
								}
							}
							else
							{
								previewMaterial = component2.material;
								if ((bool)previewMaterial)
								{
									string name = previewMaterial.shader.name;
									if (!name.StartsWith("Game/Debug"))
									{
										if (name.EndsWith("_TA"))
										{
											previewMaterial.shader = objectShader_TA;
										}
										else
										{
											previewMaterial.shader = objectShader;
											if (!block.blockMaterial.SurfaceCategory.EqualsCaseInsensitive("glass"))
											{
												previewMaterial.SetFloat("_Cutoff", 0.038f);
											}
											else
											{
												previewMaterial.SetFloat("_Cutoff", 0f);
											}
										}
									}
									if (block.MeshIndex == 3)
									{
										Texture texture2 = previewMaterial.GetTexture("_Albedo");
										previewMaterial.SetTexture("_MainTex", texture2);
									}
								}
							}
						}
					}
				}
			}
			if (Mathf.Abs(_player.speedForward) + Mathf.Abs(_player.speedStrafe) < 0.45f)
			{
				transparencyFade += Time.deltaTime * 5f;
				transparencyFade = Mathf.Clamp01(transparencyFade);
			}
			else
			{
				transparencyFade = 0f;
			}
			float num2 = 1f * transparencyFade;
			if ((bool)transformBlockPreview)
			{
				bool flag2 = focusTransformBlockValue.Block.CanPlaceBlockAt(_world, clrIdx, _focusBlockPos, _holdingBlockValue);
				bool flag3 = _holdingBlockValue.Block.IndexName == "lpblock";
				bool flag4 = _holdingBlockValue.Block.IndexName == "brBlock";
				if (flag2)
				{
					flag2 = ((!flag3) ? _world.CanPlaceBlockAt(_focusBlockPos, _ppLocal) : _world.CanPlaceLandProtectionBlockAt(_focusBlockPos, _ppLocal));
				}
				if (flag2)
				{
					flag2 &= _player.IsGodMode.Value || !GameUtils.IsColliderWithinBlock(_focusBlockPos, _holdingBlockValue);
				}
				transformWireframeCube.localScale = Vector3.one;
				if (cachedLandClaimBoundary != null)
				{
					Object.DestroyImmediate(cachedLandClaimBoundary);
				}
				if ((bool)transformFocusCubePrefab)
				{
					if (flag3)
					{
						float scale = GameStats.GetInt(EnumGameStats.LandClaimSize);
						GenerateLandClaimPreview(scale);
						transformFocusCubePrefab.localScale = multiDim * 1f;
					}
					else if (flag4)
					{
						float num3 = (float)GamePrefs.GetInt(EnumGamePrefs.BedrollDeadZoneSize) * 1f;
						transformFocusCubePrefab.localScale = new Vector3(num3, num3, num3);
					}
					else
					{
						transformFocusCubePrefab.localScale = multiDim * 1f;
					}
					transformFocusCubePrefab.localPosition = localPos;
					transformFocusCubePrefab.gameObject.SetActive(value: true);
				}
				float num4 = num2;
				float magnitude = (_player.getHeadPosition() - vector).magnitude;
				if (magnitude < 1f)
				{
					num4 *= magnitude / 1f;
				}
				Color color;
				if (flag2)
				{
					color = Color.white;
					switch (_world.GetLandClaimOwner(_focusBlockPos, _ppLocal))
					{
					case EnumLandClaimOwner.Self:
						color = Color.green;
						break;
					case EnumLandClaimOwner.Ally:
						color = Color.yellow;
						break;
					}
					if (cachedFocusBlockPos != _focusBlockPos || !focusTransformBlockValue.Equals(cachedFocusBlockValue))
					{
						drawStability = StabilityCalculator.GetBlockStabilityIfPlaced(_focusBlockPos, focusTransformBlockValue);
						cachedFocusBlockPos = _focusBlockPos;
						cachedFocusBlockValue = focusTransformBlockValue;
					}
				}
				else
				{
					color = Color.red;
					num4 *= 0.5f;
				}
				color.r *= 0.5f;
				color.g *= 0.5f;
				color.b *= 0.5f;
				Renderer[] componentsInChildren2;
				if ((bool)transformFocusCubePrefab)
				{
					Color value2 = color;
					if (flag2 && drawStability <= 0f)
					{
						value2 = new Color(1f, 0f, 0.55f);
					}
					componentsInChildren2 = transformFocusCubePrefab.GetComponentsInChildren<Renderer>();
					for (int l = 0; l < componentsInChildren2.Length; l++)
					{
						componentsInChildren2[l].material.SetColor("_Color", value2);
					}
				}
				if (cachedLandClaimBoundary != null)
				{
					Color color2 = (flag2 ? Color.green : Color.red);
					Renderer componentInChildren = cachedLandClaimBoundary.GetComponentInChildren<Renderer>();
					Color color3 = componentInChildren.material.GetColor("_BaseColor");
					componentInChildren.material.GetColor("_BoundaryColor");
					componentInChildren.material.SetColor("_BaseColor", new Color(color2.r, color2.g, color2.b, color3.a * 10f));
					componentInChildren.material.SetColor("_BoundaryColor", new Color(color2.r, color2.g, color2.b, color3.a * 10f));
				}
				Block block2 = _holdingBlockValue.Block;
				if (block2.tintColor.a > 0f)
				{
					color = block2.tintColor;
					color.a = 0.5f;
				}
				Renderer[] componentsInChildren3 = transformBlockPreview.GetComponentsInChildren<Renderer>();
				bool flag5 = false;
				componentsInChildren2 = componentsInChildren3;
				for (int l = 0; l < componentsInChildren2.Length; l++)
				{
					if (componentsInChildren2[l].shadowCastingMode == ShadowCastingMode.ShadowsOnly)
					{
						flag5 = true;
						break;
					}
				}
				componentsInChildren2 = componentsInChildren3;
				foreach (Renderer renderer in componentsInChildren2)
				{
					Material material2 = renderer.material;
					material2.SetInt("_BlendModeSrc", flag2 ? 1 : 5);
					material2.SetInt("_BlendModeDest", (!flag2) ? 1 : 0);
					material2.SetFloat("_Alpha", num4);
					material2.SetColor("_Color", color);
					material2.SetFloat("_Stability", drawStability);
					if (!flag5)
					{
						renderer.enabled = num4 > 0f;
						renderer.shadowCastingMode = ((num4 > 0.1f) ? ShadowCastingMode.On : ShadowCastingMode.Off);
						continue;
					}
					float num5 = 0f;
					if (renderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly)
					{
						num5 = 0.1f;
					}
					renderer.enabled = num4 > num5;
				}
				transformBlockPreview.position = current;
				transformBlockPreview.localScale = Vector3.one;
			}
		}
		else if (!_bAlternativeBlockPos && (bool)transformFocusCubePrefab)
		{
			transformFocusCubePrefab.localScale = new Vector3(1f, 1f, 1f);
			transformFocusCubePrefab.gameObject.SetActive(Time.time - lastTimeFocusTransformMoved > 1f);
		}
		setFocusType(focusType);
		transformWireframeCube.position = current;
		transformWireframeCube.rotation = focusTransformBlockValue.Block.shape.GetRotation(focusTransformBlockValue);
		return result2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void disableAllComponents(Transform _transform)
	{
		MonoBehaviour[] componentsInChildren = _transform.GetComponentsInChildren<MonoBehaviour>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		Animation[] componentsInChildren2 = _transform.GetComponentsInChildren<Animation>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			componentsInChildren2[j].enabled = false;
		}
		Animator[] componentsInChildren3 = _transform.GetComponentsInChildren<Animator>();
		for (int k = 0; k < componentsInChildren3.Length; k++)
		{
			componentsInChildren3[k].enabled = false;
		}
		SpriteRenderer[] componentsInChildren4 = _transform.GetComponentsInChildren<SpriteRenderer>();
		for (int l = 0; l < componentsInChildren4.Length; l++)
		{
			componentsInChildren4[l].enabled = false;
		}
		Collider[] componentsInChildren5 = _transform.GetComponentsInChildren<Collider>();
		for (int m = 0; m < componentsInChildren5.Length; m++)
		{
			Object.Destroy(componentsInChildren5[m]);
		}
	}

	public void Update(bool _bMeshSelected, World _world, WorldRayHitInfo _hitInfo, Vector3i _focusBlockPos, EntityAlive _player, PersistentPlayerData _ppLocal, bool _bAlternativeBlockPos)
	{
		bool flag = false;
		BlockValue holdingBlockValue = _player.inventory.holdingItemItemValue.ToBlockValue();
		Block block = holdingBlockValue.Block;
		if (block != null)
		{
			TextureFullArray texture = _player.inventory.holdingItemItemValue.TextureFullArray;
			if (block.SelectAlternates)
			{
				holdingBlockValue = block.GetAltBlockValue(_player.inventory.holdingItemItemValue.Meta);
				block = holdingBlockValue.Block;
				texture = ((block.GetAutoShapeType() != EAutoShapeType.None) ? _player.inventory.holdingItemItemValue.TextureFullArray : TextureFullArray.Default);
			}
			if (_player.inventory.holdingItemData is ItemClassBlock.ItemBlockInventoryData)
			{
				holdingBlockValue.rotation = ((ItemClassBlock.ItemBlockInventoryData)_player.inventory.holdingItemData).rotation;
			}
			RenderCubeType renderCubeType = _player.inventory.holdingItem.GetFocusType(_player.inventory.holdingItemData);
			int placementDistanceSq = block.GetPlacementDistanceSq();
			if (_hitInfo.hit.distanceSq > (float)placementDistanceSq)
			{
				renderCubeType = RenderCubeType.None;
			}
			if (_bMeshSelected && renderCubeType != RenderCubeType.None)
			{
				flag = update0(_world, _hitInfo, _focusBlockPos, _player, _ppLocal, _bAlternativeBlockPos, holdingBlockValue, texture);
			}
			if (transformWireframeCube != null && transformWireframeCube.gameObject != null)
			{
				transformWireframeCube.gameObject.SetActive(_bMeshSelected && renderCubeType != RenderCubeType.None && block.bHasPlacementWireframe);
			}
		}
		if (!flag)
		{
			transparencyFade = 0f;
			transformLastPos.x = float.MaxValue;
			DestroyPreview();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DestroyPreview()
	{
		if ((bool)transformBlockPreview)
		{
			if (!(focusTransformBlockValue.Block.shape is BlockShapeModelEntity))
			{
				MeshFilter[] componentsInChildren = transformBlockPreview.GetComponentsInChildren<MeshFilter>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					VoxelMesh.AddPooledMesh(componentsInChildren[i].sharedMesh);
				}
			}
			Object.Destroy(transformBlockPreview.gameObject);
			transformBlockPreview = null;
		}
		for (int j = 0; j < blockPreviewMats.Count; j++)
		{
			Object.Destroy(blockPreviewMats[j]);
		}
		blockPreviewMats.Clear();
	}
}
