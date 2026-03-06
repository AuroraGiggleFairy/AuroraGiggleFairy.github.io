using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionTerrainTool : ItemActionRanged
{
	public enum EnumMode
	{
		Grow,
		Shrink
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public class MyInventoryData(ItemInventoryData _invData, int _indexInEntityOfAction, string _particleTransform) : ItemActionDataRanged(_invData, _indexInEntityOfAction)
	{
		public float activateTime;

		public float lastHitTime;

		public bool bActivated;

		public float sphereDistance = 5f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTransparency = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRadiusMin = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRadiusMax = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRadiusStep = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float damage;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumMode mode;

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject modelObj;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Material modelMat;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float sphereRadius = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static BlockValue blockValueSelected = BlockValue.Air;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3[] INNER_POINTS = new Vector3[17]
	{
		new Vector3(0.5f, 0.5f, 0.5f),
		new Vector3(0f, 0f, 0f),
		new Vector3(1f, 0f, 0f),
		new Vector3(0f, 1f, 0f),
		new Vector3(0f, 0f, 1f),
		new Vector3(1f, 1f, 0f),
		new Vector3(0f, 1f, 1f),
		new Vector3(1f, 0f, 1f),
		new Vector3(1f, 1f, 1f),
		new Vector3(0.25f, 0.25f, 0.25f),
		new Vector3(0.75f, 0.25f, 0.25f),
		new Vector3(0.25f, 0.75f, 0.25f),
		new Vector3(0.25f, 0.25f, 0.75f),
		new Vector3(0.75f, 0.75f, 0.25f),
		new Vector3(0.25f, 0.75f, 0.75f),
		new Vector3(0.75f, 0.25f, 0.75f),
		new Vector3(0.75f, 0.75f, 0.75f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3[] INNER_POINTS_XZ = new Vector3[8]
	{
		new Vector3(0f, 0f, 0f),
		new Vector3(1f, 0f, 0f),
		new Vector3(0f, 0f, 1f),
		new Vector3(1f, 0f, 1f),
		new Vector3(0.25f, 0f, 0.25f),
		new Vector3(0.75f, 0f, 0.25f),
		new Vector3(0.25f, 0f, 0.75f),
		new Vector3(0.75f, 0f, 0.75f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlockChangeInfo> blockChanges = new List<BlockChangeInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateModel()
	{
		if (!modelObj)
		{
			modelObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			Transform transform = modelObj.transform;
			Object.Destroy(transform.GetComponent<Collider>());
			transform.SetParent(null);
			modelObj.layer = 0;
			modelObj.SetActive(value: false);
			modelMat = Resources.Load<Material>("Materials/TerrainSmoothing");
			modelObj.GetComponent<Renderer>().material = modelMat;
		}
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (_props.Values.ContainsKey("Mode"))
		{
			mode = EnumUtils.Parse<EnumMode>(_props.Values["Mode"]);
		}
	}

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new MyInventoryData(_invData, _indexInEntityOfAction, null);
	}

	public override int GetInitialMeta(ItemValue _itemValue)
	{
		return 0;
	}

	public override void StartHolding(ItemActionData _actionData)
	{
		showSphere(_actionData);
	}

	public override void StopHolding(ItemActionData _actionData)
	{
		base.StopHolding(_actionData);
		((MyInventoryData)_actionData).bActivated = false;
		hideSphere(_actionData);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showSphere(ItemActionData _actionData)
	{
		CreateModel();
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		Ray lookRay = myInventoryData.invData.holdingEntity.GetLookRay();
		Transform transform = modelObj.transform;
		transform.SetPositionAndRotation(lookRay.origin + lookRay.direction * myInventoryData.sphereDistance - Origin.position, Quaternion.identity);
		transform.SetParent(myInventoryData.invData.holdingEntity.transform);
		transform.localScale = new Vector3(sphereRadius * 2f, sphereRadius * 2f, sphereRadius * 2f);
		modelObj.SetActive(value: true);
		modelObj.layer = 0;
		modelMat.color = new Color(0f, 0f, 0f, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void hideSphere(ItemActionData _actionData)
	{
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		modelObj.SetActive(value: false);
		modelObj.transform.SetParent(myInventoryData.invData.model);
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		if (_bReleased)
		{
			myInventoryData.bActivated = false;
			GameManager.Instance.ItemActionEffectsServer(myInventoryData.invData.holdingEntity.entityId, myInventoryData.invData.slotIdx, myInventoryData.indexInEntityOfAction, 0, Vector3.zero, Vector3.zero);
		}
		else
		{
			myInventoryData.bActivated = true;
			myInventoryData.activateTime = Time.time;
			GameManager.Instance.ItemActionEffectsServer(myInventoryData.invData.holdingEntity.entityId, myInventoryData.invData.slotIdx, myInventoryData.indexInEntityOfAction, 1, Vector3.zero, Vector3.zero);
		}
	}

	public override void ItemActionEffects(GameManager _gameManager, ItemActionData _actionData, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
	{
		switch ((ItemActionFiringState)_firingState)
		{
		case ItemActionFiringState.Start:
			if (mode == EnumMode.Grow)
			{
				modelMat.color = new Color(0f, 1f, 0f, 0f);
			}
			else
			{
				modelMat.color = new Color(1f, 0f, 0f, 0f);
			}
			break;
		case ItemActionFiringState.Off:
			modelMat.color = new Color(0f, 0f, 0f, 0f);
			break;
		case ItemActionFiringState.Loop:
			break;
		}
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		return ((MyInventoryData)_actionData).bActivated;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		EntityPlayerLocal entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;
		if (!entityPlayerLocal || !modelObj)
		{
			return;
		}
		Ray lookRay = entityPlayerLocal.GetLookRay();
		lookRay.origin += lookRay.direction.normalized * 0.1f;
		int hitMask = 256;
		int layerMask = 65536;
		if (!Voxel.RaycastOnVoxels(entityPlayerLocal.world, lookRay, 100f, layerMask, hitMask, 0f))
		{
			modelObj.transform.position = lookRay.origin + lookRay.direction.normalized * 100f - Origin.position;
			if (InputUtils.AltKeyPressed)
			{
				blockValueSelected = BlockValue.Air;
			}
			return;
		}
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		myInventoryData.hitInfo.CopyFrom(Voxel.voxelRayHitInfo);
		Vector3 pos = myInventoryData.hitInfo.hit.pos;
		modelObj.transform.position = Vector3.Lerp(modelObj.transform.position, pos - Origin.position, 0.5f);
		if (!myInventoryData.bActivated || !(Time.time - myInventoryData.lastHitTime > 0.2f))
		{
			return;
		}
		myInventoryData.lastHitTime = Time.time;
		bool shiftKeyPressed = InputUtils.ShiftKeyPressed;
		if (mode == EnumMode.Grow)
		{
			int densityStep = ((!shiftKeyPressed) ? 20 : 5);
			GrowTerrain(_actionData, pos, densityStep);
		}
		else if (mode == EnumMode.Shrink)
		{
			if (!shiftKeyPressed)
			{
				GrowTerrain(_actionData, pos, -8);
			}
			else
			{
				RemoveTerrain(_actionData, pos, damage);
			}
		}
	}

	public override float GetRange(ItemActionData _actionData)
	{
		return 20f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GrowTerrain(ItemActionData _actionData, Vector3 _worldPos, int _densityStep)
	{
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		World world = myInventoryData.invData.world;
		int num = Utils.Fastfloor(_worldPos.x - sphereRadius - 1f);
		int num2 = Utils.Fastfloor(_worldPos.x + sphereRadius + 1f);
		int num3 = Utils.Fastfloor(_worldPos.z - sphereRadius - 1f);
		int num4 = Utils.Fastfloor(_worldPos.z + sphereRadius + 1f);
		int num5 = Utils.FastClamp(Utils.Fastfloor(_worldPos.y - sphereRadius), 0, 255);
		int num6 = Utils.FastClamp(Utils.Fastfloor(_worldPos.y + sphereRadius), 0, 255);
		if (InputUtils.AltKeyPressed)
		{
			BlockValue blockValue = myInventoryData.hitInfo.hit.blockValue;
			if (!blockValue.Block.isMultiBlock)
			{
				blockValueSelected = blockValue;
			}
			return 0;
		}
		BlockValue blockValue2 = blockValueSelected;
		bool flag = !blockValue2.isair;
		if (!flag && _densityStep >= 0)
		{
			blockValue2 = myInventoryData.hitInfo.hit.blockValue;
			if (!blockValue2.Block.shape.IsTerrain())
			{
				return 0;
			}
		}
		bool flag2 = blockValue2.Block.shape.IsTerrain();
		int type = blockValue2.type;
		blockChanges.Clear();
		IChunk _chunk = null;
		Vector3 vector = default(Vector3);
		Vector3i pos = default(Vector3i);
		Vector3i pos2 = default(Vector3i);
		for (int i = num3; i <= num4; i++)
		{
			vector.z = i;
			int z = World.toBlockXZ(i);
			for (int j = num; j <= num2; j++)
			{
				vector.x = j;
				int x = World.toBlockXZ(j);
				world.GetChunkFromWorldPos(j, i, ref _chunk);
				if (_chunk == null)
				{
					continue;
				}
				for (int k = num5; k <= num6; k++)
				{
					vector.y = k;
					int num7 = _densityStep;
					float magnitude = (_worldPos - vector).magnitude;
					if (!(magnitude <= sphereRadius))
					{
						if (!(magnitude - sphereRadius < 1f))
						{
							continue;
						}
						float num8 = 1f - (magnitude - sphereRadius) / 1f;
						num7 = (int)((float)num7 * num8);
						if (num7 == 0)
						{
							continue;
						}
					}
					BlockValue blockNoDamage = _chunk.GetBlockNoDamage(x, k, z);
					bool isair = blockNoDamage.isair;
					if (!blockNoDamage.Block.shape.IsTerrain() && !isair)
					{
						continue;
					}
					int density = world.GetDensity(0, j, k, i);
					BlockChangeInfo blockChangeInfo = new BlockChangeInfo();
					blockChangeInfo.pos.x = j;
					blockChangeInfo.pos.y = k;
					blockChangeInfo.pos.z = i;
					if (num7 > 0)
					{
						if (isair)
						{
							int num9 = Vector3i.AllDirections.Length;
							for (int l = 0; l < num9; l++)
							{
								pos.x = j + Vector3i.AllDirections[l].x;
								pos.y = k + Vector3i.AllDirections[l].y;
								pos.z = i + Vector3i.AllDirections[l].z;
								if (!world.GetBlock(pos).Block.shape.IsTerrain())
								{
									continue;
								}
								if (flag2)
								{
									density -= num7;
									if (density < 0)
									{
										density = -1;
										blockChangeInfo.blockValue.type = type;
										blockChangeInfo.bChangeBlockValue = true;
									}
									blockChangeInfo.density = (sbyte)density;
									blockChangeInfo.bChangeDensity = true;
									blockChanges.Add(blockChangeInfo);
								}
								else
								{
									blockChangeInfo.blockValue.type = type;
									blockChangeInfo.bChangeBlockValue = true;
									blockChanges.Add(blockChangeInfo);
								}
								break;
							}
						}
						else if (flag2 && density > MarchingCubes.DensityTerrain)
						{
							density -= num7;
							if (density < MarchingCubes.DensityTerrain)
							{
								density = MarchingCubes.DensityTerrain;
							}
							blockChangeInfo.density = (sbyte)density;
							blockChangeInfo.bChangeDensity = true;
							if (flag)
							{
								blockChangeInfo.blockValue.type = type;
								blockChangeInfo.bChangeBlockValue = true;
							}
							blockChanges.Add(blockChangeInfo);
						}
						continue;
					}
					if (isair)
					{
						if (density < MarchingCubes.DensityAir)
						{
							density -= num7;
							if (density > MarchingCubes.DensityAir)
							{
								density = MarchingCubes.DensityAir;
							}
							blockChangeInfo.density = (sbyte)density;
							blockChangeInfo.bChangeDensity = true;
							blockChanges.Add(blockChangeInfo);
						}
						continue;
					}
					int num10 = Vector3i.AllDirections.Length;
					for (int m = 0; m < num10; m++)
					{
						pos2.x = j + Vector3i.AllDirections[m].x;
						pos2.y = k + Vector3i.AllDirections[m].y;
						pos2.z = i + Vector3i.AllDirections[m].z;
						if (world.GetBlock(pos2).isair)
						{
							density -= num7;
							if (density > 0)
							{
								density = 1;
								blockChangeInfo.blockValue.type = 0;
								blockChangeInfo.bChangeBlockValue = true;
							}
							blockChangeInfo.density = (sbyte)density;
							blockChangeInfo.bChangeDensity = true;
							blockChanges.Add(blockChangeInfo);
							break;
						}
					}
				}
			}
		}
		if (blockChanges.Count > 0)
		{
			BlockToolSelection.Instance.BeginUndo(0);
			myInventoryData.invData.world.SetBlocksRPC(blockChanges);
			BlockToolSelection.Instance.EndUndo(0);
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int setTerrainOLD(ItemActionData _actionData, Vector3 _worldPos, float _damage = 0f, DamageMultiplier _damageMultiplier = null, bool _bChangeBlocks = true)
	{
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		int num = Utils.Fastfloor(_worldPos.x - sphereRadius);
		int num2 = Utils.Fastfloor(_worldPos.x + sphereRadius);
		int num3 = Utils.Fastfloor(_worldPos.y - sphereRadius);
		int num4 = Utils.Fastfloor(_worldPos.y + sphereRadius);
		int num5 = Utils.Fastfloor(_worldPos.z - sphereRadius);
		int num6 = Utils.Fastfloor(_worldPos.z + sphereRadius);
		blockChanges.Clear();
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				for (int k = num5; k <= num6; k++)
				{
					int num7 = 0;
					for (int l = 0; l < INNER_POINTS.Length; l++)
					{
						Vector3 vector = INNER_POINTS[l];
						if ((new Vector3((float)i + vector.x, (float)j + vector.y, (float)k + vector.z) - _worldPos).magnitude <= sphereRadius)
						{
							num7++;
						}
						if (l == 8)
						{
							switch (num7)
							{
							case 9:
								num7 = INNER_POINTS.Length;
								break;
							default:
								continue;
							case 0:
								break;
							}
							break;
						}
					}
					if (num7 == 0)
					{
						continue;
					}
					Vector3i vector3i = new Vector3i(i, j, k);
					BlockValue block = myInventoryData.invData.world.GetBlock(vector3i);
					BlockValue blockValue = block;
					sbyte density = myInventoryData.invData.world.GetDensity(0, vector3i);
					sbyte b = density;
					if (num7 > INNER_POINTS.Length / 2 || block.Block.shape.IsTerrain())
					{
						blockValue.type = 1;
						b = (sbyte)((float)MarchingCubes.DensityTerrain * (float)(num7 - INNER_POINTS.Length / 2 - 1) / (float)(INNER_POINTS.Length / 2));
					}
					else if (block.isair)
					{
						b = (sbyte)((float)MarchingCubes.DensityAir * (float)(INNER_POINTS.Length / 2 - num7) / (float)(INNER_POINTS.Length / 2));
						if (b >= 0)
						{
							b = -1;
						}
					}
					if (blockValue.type != block.type || b < density)
					{
						BlockChangeInfo blockChangeInfo = new BlockChangeInfo();
						blockChangeInfo.pos = vector3i;
						blockChangeInfo.bChangeDensity = true;
						blockChangeInfo.density = b;
						if (blockValue.type != block.type)
						{
							blockChangeInfo.bChangeBlockValue = true;
							blockChangeInfo.blockValue = blockValue;
						}
						blockChanges.Add(blockChangeInfo);
					}
				}
			}
		}
		myInventoryData.invData.world.SetBlocksRPC(blockChanges);
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int RemoveTerrain(ItemActionData _actionData, Vector3 _worldPos, float _damage = 0f, DamageMultiplier _damageMultiplier = null, bool _bChangeBlocks = true)
	{
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		int num = Utils.Fastfloor(_worldPos.x - sphereRadius);
		int num2 = Utils.Fastfloor(_worldPos.x + sphereRadius);
		int num3 = Utils.Fastfloor(_worldPos.y - sphereRadius);
		int num4 = Utils.Fastfloor(_worldPos.y + sphereRadius);
		int num5 = Utils.Fastfloor(_worldPos.z - sphereRadius);
		int num6 = Utils.Fastfloor(_worldPos.z + sphereRadius);
		blockChanges.Clear();
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				for (int k = num5; k <= num6; k++)
				{
					int num7 = 0;
					for (int l = 0; l < INNER_POINTS.Length; l++)
					{
						Vector3 vector = INNER_POINTS[l];
						if ((new Vector3((float)i + vector.x, (float)j + vector.y, (float)k + vector.z) - _worldPos).magnitude <= sphereRadius)
						{
							num7++;
						}
						if (l == 8)
						{
							switch (num7)
							{
							case 9:
								num7 = INNER_POINTS.Length;
								break;
							default:
								continue;
							case 0:
								break;
							}
							break;
						}
					}
					if (num7 == 0)
					{
						continue;
					}
					Vector3i vector3i = new Vector3i(i, j, k);
					BlockValue block = myInventoryData.invData.world.GetBlock(vector3i);
					if (!block.Block.shape.IsTerrain())
					{
						continue;
					}
					BlockValue blockValue = block;
					sbyte density = myInventoryData.invData.world.GetDensity(0, vector3i);
					sbyte b = density;
					if (num7 > INNER_POINTS.Length / 2)
					{
						blockValue = BlockValue.Air;
						b = (sbyte)((float)MarchingCubes.DensityAir * (float)(num7 - INNER_POINTS.Length / 2 - 1) / (float)(INNER_POINTS.Length / 2));
						if (b <= 0)
						{
							b = 1;
						}
					}
					else if (!block.isair)
					{
						b = (sbyte)((float)MarchingCubes.DensityTerrain * (float)(INNER_POINTS.Length / 2 - num7) / (float)(INNER_POINTS.Length / 2));
						if (b >= 0)
						{
							b = -1;
						}
					}
					if (blockValue.type != block.type || b > density)
					{
						BlockChangeInfo blockChangeInfo = new BlockChangeInfo();
						blockChangeInfo.pos = vector3i;
						blockChangeInfo.bChangeDensity = true;
						blockChangeInfo.density = b;
						if (blockValue.type != block.type)
						{
							blockChangeInfo.bChangeBlockValue = true;
							blockChangeInfo.blockValue = blockValue;
						}
						blockChanges.Add(blockChangeInfo);
					}
				}
			}
		}
		if (blockChanges.Count > 0)
		{
			BlockToolSelection.Instance.BeginUndo(0);
			myInventoryData.invData.world.SetBlocksRPC(blockChanges);
			BlockToolSelection.Instance.EndUndo(0);
		}
		return 0;
	}

	public override bool IsEditingTool()
	{
		return true;
	}

	public override string GetStat(ItemActionData _data)
	{
		if (!blockValueSelected.isair)
		{
			return blockValueSelected.Block.GetLocalizedBlockName();
		}
		return "-";
	}

	public override bool IsStatChanged()
	{
		return true;
	}

	public override bool ConsumeScrollWheel(ItemActionData _actionData, float _scrollWheelInput, PlayerActionsLocal _playerInput)
	{
		_ = (MyInventoryData)_actionData;
		if ((bool)_playerInput.Run && (bool)modelObj)
		{
			float num = _scrollWheelInput * 1f + _scrollWheelInput * sphereRadius * 0.5f;
			sphereRadius = Utils.FastClamp(sphereRadius + num, 0.5f, 30f);
			modelObj.transform.localScale = new Vector3(sphereRadius * 2f, sphereRadius * 2f, sphereRadius * 2f);
			return true;
		}
		return false;
	}
}
