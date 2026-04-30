using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeModelEntity : BlockShapeInvisible
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct DamageState
	{
		public string objName;

		public float health;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropDamagedMesh = "MeshDamage";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cMissingPrefabEntityPath = "@:Entities/Misc/block_missingPrefab.prefab";

	public string modelName;

	public Vector3 modelOffset;

	public int censorMode;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string modelNameWithPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public float LODCullScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public new Bounds bounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isCustomBounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<DamageState> damageStates;

	public BlockShapeModelEntity()
	{
		IsRotatable = true;
		IsNotifyOnLoadUnload = true;
	}

	public override void Init(Block _block)
	{
		base.Init(_block);
		modelNameWithPath = _block.Properties.Values["Model"];
		if (modelNameWithPath == null)
		{
			throw new Exception("No model specified on block with name " + _block.GetBlockName());
		}
		if (modelNameWithPath.Length > 0)
		{
			_block.Properties.ParseInt(EntityClass.PropCensor, ref censorMode);
			if (censorMode != 0 && (bool)GameManager.Instance && GameManager.Instance.IsGoreCensored())
			{
				if (modelNameWithPath.Contains("@"))
				{
					modelNameWithPath = modelNameWithPath.Replace(".", "_CGore.");
				}
				else if (!modelNameWithPath.Contains("."))
				{
					modelNameWithPath += "_CGore";
				}
			}
		}
		modelName = GameIO.GetFilenameFromPathWithoutExtension(modelNameWithPath);
		modelOffset = new Vector3(0f, 0.5f, 0f);
		_block.Properties.ParseVec("ModelOffset", ref modelOffset);
		_block.Properties.ParseFloat("LODCullScale", ref LODCullScale);
		_block.Properties.ParseInt("SymType", ref SymmetryType);
		if (_block.Properties.Values.TryGetValue(PropDamagedMesh, out var _value))
		{
			string[] array = _value.Split(',');
			if (array.Length >= 2)
			{
				damageStates = new List<DamageState>();
				DamageState item = default(DamageState);
				for (int i = 0; i < array.Length - 1; i += 2)
				{
					item.objName = array[i].Trim();
					item.health = float.Parse(array[i + 1]);
					damageStates.Add(item);
				}
			}
		}
		GameObjectPool.Instance.AddPooledObject(modelName, PoolLoadCallback, PoolCreateOnceToAllCallBack, PoolCreateCallBack);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform PoolLoadCallback()
	{
		Transform prefab = getPrefab();
		if (prefab == null)
		{
			throw new Exception("Model '" + modelNameWithPath + "' not found on block with name " + block.GetBlockName());
		}
		return prefab;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PoolCreateOnceToAllCallBack(GameObject obj)
	{
		Collider component = obj.transform.GetComponent<Collider>();
		if (component != null)
		{
			if (component is BoxCollider)
			{
				Vector3 center = ((BoxCollider)component).center;
				Vector3 size = ((BoxCollider)component).size;
				bounds = BoundsUtils.BoundsForMinMax(center.x - size.x / 2f, center.y - size.y / 2f, center.z - size.z / 2f, center.x + size.x / 2f, center.y + size.y / 2f, center.z + size.z / 2f);
				boundsArr[0] = bounds;
				isCustomBounds = true;
			}
			else if (component is CapsuleCollider)
			{
				CapsuleCollider capsuleCollider = component as CapsuleCollider;
				Vector3 center2 = capsuleCollider.center;
				Vector3 vector = new Vector3(capsuleCollider.radius * 2f, capsuleCollider.height, capsuleCollider.radius * 2f);
				bounds = BoundsUtils.BoundsForMinMax(center2.x - vector.x / 2f, center2.y - vector.y / 2f, center2.z - vector.z / 2f, center2.x + vector.x / 2f, center2.y + vector.y / 2f, center2.z + vector.z / 2f);
				boundsArr[0] = bounds;
				isCustomBounds = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PoolCreateCallBack(GameObject obj)
	{
		Transform transform = obj.transform;
		LODGroup component = transform.GetComponent<LODGroup>();
		if ((bool)component)
		{
			LODFadeMode fadeMode = component.fadeMode;
			switch (fadeMode)
			{
			case LODFadeMode.SpeedTree:
				return;
			case LODFadeMode.None:
				component.fadeMode = LODFadeMode.CrossFade;
				component.animateCrossFading = true;
				break;
			}
			if (fadeMode == LODFadeMode.CrossFade)
			{
				component.animateCrossFading = true;
			}
			LOD[] lODs = component.GetLODs();
			int num = lODs.Length - 1;
			float num2 = component.size;
			if (num2 < 0.4f)
			{
				num2 *= 3.8f;
				if (num2 < 1f)
				{
					num2 = 1f;
				}
			}
			else if (num2 < 0.65f)
			{
				num2 *= 2.5f;
			}
			else if (num2 < 0.95f)
			{
				num2 *= 1.5f;
			}
			else if (!(num2 < 1.45f))
			{
				num2 = ((num2 < 2.5f) ? (num2 * 0.83f) : ((!(num2 < 6.2f)) ? (num2 * 0.45f) : (num2 * 0.64f)));
			}
			float num3 = num2 * 0.02f * LODCullScale;
			if (num3 > 0.1f)
			{
				num3 = 0.1f;
			}
			lODs[num].screenRelativeTransitionHeight = num3;
			if (num > 0)
			{
				float num4 = num3;
				for (int num5 = num - 1; num5 >= 0; num5--)
				{
					float num6 = lODs[num5].screenRelativeTransitionHeight;
					if (num6 - 0.025f <= num4)
					{
						num6 = num4 + 0.025f;
						lODs[num5].screenRelativeTransitionHeight = num6;
					}
					num4 = num6;
				}
			}
			component.SetLODs(lODs);
			if (num < 2 || GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowDistance) > 2)
			{
				return;
			}
			Renderer[] renderers = lODs[num].renderers;
			foreach (Renderer renderer in renderers)
			{
				if ((bool)renderer)
				{
					renderer.shadowCastingMode = ShadowCastingMode.Off;
				}
			}
		}
		else if (transform.childCount == 0)
		{
			MeshRenderer component2 = obj.GetComponent<MeshRenderer>();
			if ((bool)component2)
			{
				LOD lOD = default(LOD);
				lOD.screenRelativeTransitionHeight = 0.025f;
				lOD.renderers = new Renderer[1] { component2 };
				lOD.fadeTransitionWidth = 0f;
				component = obj.AddComponent<LODGroup>();
				component.fadeMode = LODFadeMode.CrossFade;
				component.animateCrossFading = true;
				component.SetLODs(new LOD[1] { lOD });
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform getPrefab()
	{
		Transform prefab = DataLoader.LoadAsset<Transform>(modelNameWithPath);
		if (prefab == null)
		{
			Log.Error("Model '{0}' not found on block with name {1}", modelNameWithPath, block.GetBlockName());
			prefab = DataLoader.LoadAsset<Transform>("@:Entities/Misc/block_missingPrefab.prefab");
			if (prefab == null)
			{
				return null;
			}
		}
		else
		{
			MeshLodOptimization.Apply(ref prefab);
		}
		string filenameFromPathWithoutExtension = GameIO.GetFilenameFromPathWithoutExtension(modelNameWithPath);
		if (prefab.name != filenameFromPathWithoutExtension)
		{
			Log.Error("Model has a wrong name '{0}'. Maybe check upper/lower case mismatch on block with name {1}?", filenameFromPathWithoutExtension, block.GetBlockName());
		}
		return prefab;
	}

	public Transform CloneModel(BlockValue _blockValue, Transform _parent)
	{
		Transform transform = UnityEngine.Object.Instantiate(getPrefab());
		transform.parent = _parent;
		Block block = _blockValue.Block;
		if (block.tintColor.a > 0f)
		{
			UpdateLight.SetTintColor(transform, block.tintColor);
		}
		Quaternion rotation = GetRotation(_blockValue);
		Vector3 rotatedOffset = GetRotatedOffset(block, rotation);
		transform.localPosition = rotatedOffset + new Vector3(0f, -0.5f, 0f);
		transform.localRotation = rotation;
		return transform;
	}

	public Vector3 GetRotatedOffset(Block block, Quaternion rot)
	{
		Vector3 result = rot * modelOffset;
		Vector3 zero = Vector3.zero;
		zero.y = -0.5f;
		if (block.isMultiBlock)
		{
			if ((block.multiBlockPos.dim.x & 1) == 0)
			{
				zero.x = -0.5f;
			}
			if ((block.multiBlockPos.dim.z & 1) == 0)
			{
				zero.z = -0.5f;
			}
		}
		zero = rot * zero;
		result += zero;
		result.y += 0.5f;
		return result;
	}

	public override Quaternion GetRotation(BlockValue _blockValue)
	{
		return BlockShapeNew.GetRotationStatic(_blockValue.rotation);
	}

	public override Bounds[] GetBounds(BlockValue _blockValue)
	{
		if (!isCustomBounds)
		{
			return base.GetBounds(_blockValue);
		}
		Quaternion rotation = GetRotation(_blockValue);
		Vector3 vector = rotation * bounds.min + modelOffset;
		Vector3 vector2 = rotation * bounds.max + modelOffset;
		boundsArr[0].min = new Vector3((vector2.x > vector.x) ? vector.x : vector2.x, (vector2.y > vector.y) ? vector.y : vector2.y, (vector2.z > vector.z) ? vector.z : vector2.z) + new Vector3(0.5f, 0f, 0.5f);
		boundsArr[0].max = new Vector3((vector2.x < vector.x) ? vector.x : vector2.x, (vector2.y < vector.y) ? vector.y : vector2.y, (vector2.z < vector.z) ? vector.z : vector2.z) + new Vector3(0.5f, 0f, 0.5f);
		return boundsArr;
	}

	public override BlockValue RotateY(bool _bLeft, BlockValue _blockValue, int _rotCount)
	{
		if (_bLeft)
		{
			_rotCount = -_rotCount;
		}
		int rotation = _blockValue.rotation;
		if (rotation >= 24)
		{
			_blockValue.rotation = (byte)(((rotation - 24 + _rotCount) & 3) + 24);
		}
		else
		{
			int num = 90 * _rotCount;
			_blockValue.rotation = (byte)BlockShapeNew.ConvertRotationFree(rotation, Quaternion.AngleAxis(num, Vector3.up));
		}
		return _blockValue;
	}

	public override byte Rotate(bool _bLeft, int _rotation)
	{
		_rotation += ((!_bLeft) ? 1 : (-1));
		if (_rotation > 10)
		{
			_rotation = 0;
		}
		if (_rotation < 0)
		{
			_rotation = 10;
		}
		return (byte)_rotation;
	}

	public override BlockValue MirrorY(bool _bAlongX, BlockValue _blockValue)
	{
		if (!_bAlongX)
		{
			switch (_blockValue.rotation)
			{
			case 0:
				_blockValue.rotation = 2;
				break;
			case 1:
				_blockValue.rotation = 1;
				break;
			case 2:
				_blockValue.rotation = 0;
				break;
			case 3:
				_blockValue.rotation = 3;
				break;
			case 4:
				_blockValue.rotation = 7;
				break;
			case 5:
				_blockValue.rotation = 6;
				break;
			case 6:
				_blockValue.rotation = 5;
				break;
			case 7:
				_blockValue.rotation = 4;
				break;
			case 8:
				_blockValue.rotation = 8;
				break;
			case 9:
				_blockValue.rotation = 9;
				break;
			case 10:
				_blockValue.rotation = 10;
				break;
			case 11:
				_blockValue.rotation = 11;
				break;
			}
		}
		else
		{
			switch (_blockValue.rotation)
			{
			case 0:
				_blockValue.rotation = 0;
				break;
			case 1:
				_blockValue.rotation = 3;
				break;
			case 2:
				_blockValue.rotation = 2;
				break;
			case 3:
				_blockValue.rotation = 1;
				break;
			case 4:
				_blockValue.rotation = 7;
				break;
			case 5:
				_blockValue.rotation = 6;
				break;
			case 6:
				_blockValue.rotation = 5;
				break;
			case 7:
				_blockValue.rotation = 4;
				break;
			case 8:
				_blockValue.rotation = 8;
				break;
			case 9:
				_blockValue.rotation = 11;
				break;
			case 10:
				_blockValue.rotation = 10;
				break;
			case 11:
				_blockValue.rotation = 9;
				break;
			}
		}
		return _blockValue;
	}

	public override void OnBlockValueChanged(WorldBase _world, Vector3i _blockPos, int _clrIdx, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _blockPos, _clrIdx, _oldBlockValue, _newBlockValue);
		ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
		if (chunkCluster == null)
		{
			return;
		}
		Chunk chunk = (Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos.x, _blockPos.y, _blockPos.z);
		if (chunk == null)
		{
			return;
		}
		BlockEntityData blockEntity = chunk.GetBlockEntity(_blockPos);
		if (blockEntity == null || !blockEntity.bHasTransform)
		{
			return;
		}
		Block block = _newBlockValue.Block;
		if (_newBlockValue.rotation != _oldBlockValue.rotation)
		{
			blockEntity.transform.localRotation = block.shape.GetRotation(_newBlockValue);
		}
		blockEntity.blockValue = _newBlockValue;
		if (damageStates != null)
		{
			if (GetDamageStateIndex(_oldBlockValue) != GetDamageStateIndex(_newBlockValue))
			{
				UpdateDamageState(_oldBlockValue, _newBlockValue, blockEntity);
			}
		}
		else
		{
			int num = Mathf.Min(_newBlockValue.damage, block.MaxDamage) - 1;
			blockEntity.SetMaterialValue("_Damage", num);
		}
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
		BlockEntityData blockEntityData = new BlockEntityData(_blockValue, _blockPos);
		blockEntityData.bNeedsTemperature = true;
		_chunk.AddEntityBlockStub(blockEntityData);
		registerSleepers(_blockPos, _blockValue);
	}

	public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		_chunk.RemoveEntityBlockStub(_blockPos);
		if (GameManager.Instance.IsEditMode() && _blockValue.Block.IsSleeperBlock)
		{
			Prefab.TransientSleeperBlockIncrement(_blockPos, -1);
			SleeperVolumeToolManager.UnRegisterSleeperBlock(_blockPos);
		}
	}

	public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
		if (chunkCluster != null)
		{
			Chunk chunk = (Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos);
			if (chunk != null)
			{
				BlockEntityData blockEntityData = new BlockEntityData(_blockValue, _blockPos);
				blockEntityData.bNeedsTemperature = true;
				chunk.AddEntityBlockStub(blockEntityData);
				registerSleepers(_blockPos, _blockValue);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void registerSleepers(Vector3i _blockPos, BlockValue _blockValue)
	{
		if (GameManager.Instance.IsEditMode() && _blockValue.Block.IsSleeperBlock)
		{
			Prefab.TransientSleeperBlockIncrement(_blockPos, 1);
			ThreadManager.AddSingleTaskMainThread("OnBlockAddedOrLoaded.RegisterSleeperBlock", [PublicizedFrom(EAccessModifier.Internal)] (object _003Cp0_003E) =>
			{
				SleeperVolumeToolManager.RegisterSleeperBlock(_blockValue, CloneModel(_blockValue, null), _blockPos);
			});
		}
	}

	public override void OnBlockEntityTransformBeforeActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformBeforeActivated(_world, _blockPos, _blockValue, _ebcd);
		if (!GameManager.IsDedicatedServer)
		{
			if (damageStates != null)
			{
				UpdateDamageState(_blockValue, _blockValue, _ebcd);
			}
			else
			{
				int num = (int)(10f * (float)_blockValue.damage) / _blockValue.Block.MaxDamage;
				_ebcd.SetMaterialValue("_Damage", num);
			}
			if (block.tintColor.a > 0f)
			{
				_ebcd.SetMaterialColor("_Color", block.tintColor);
			}
			else if (block.defaultTintColor.a > 0f)
			{
				_ebcd.SetMaterialColor("_Color", block.defaultTintColor);
			}
		}
	}

	public override bool UseRepairDamageState(BlockValue _blockValue)
	{
		if (damageStates.Count > 1 && GetDamageStateIndex(_blockValue) == damageStates.Count - 1)
		{
			return true;
		}
		return false;
	}

	public void UpdateDamageState(BlockValue _oldBlockValue, BlockValue _newBlockValue, BlockEntityData _data, bool bPlayEffects = true)
	{
		int damageStateIndex = GetDamageStateIndex(_oldBlockValue);
		int damageStateIndex2 = GetDamageStateIndex(_newBlockValue);
		bool flag = damageStateIndex2 > damageStateIndex;
		if (flag)
		{
			Transform transform = _data.transform.Find("FX");
			if ((bool)transform)
			{
				AudioPlayer componentInChildren = transform.GetComponentInChildren<AudioPlayer>();
				if ((bool)componentInChildren)
				{
					componentInChildren.Play();
				}
				ParticleSystem componentInChildren2 = transform.GetComponentInChildren<ParticleSystem>();
				if ((bool)componentInChildren2)
				{
					componentInChildren2.Emit(10);
				}
			}
		}
		for (int i = 0; i < damageStates.Count; i++)
		{
			DamageState damageState = damageStates[i];
			if (damageState.objName == "-")
			{
				continue;
			}
			GameObject gameObject = _data.transform.Find(damageState.objName).gameObject;
			gameObject.SetActive(i == damageStateIndex2);
			if (i == damageStateIndex2 && flag)
			{
				AudioSource component = gameObject.GetComponent<AudioSource>();
				if (component != null)
				{
					component.PlayDelayed(0.15f);
				}
				AudioPlayer component2 = gameObject.GetComponent<AudioPlayer>();
				if (component2 != null)
				{
					component2.Play();
				}
				ParticleSystem component3 = gameObject.GetComponent<ParticleSystem>();
				if ((bool)component3)
				{
					component3.Emit(10);
				}
			}
		}
		UpdateLightOnAllMaterials component4 = _data.transform.GetComponent<UpdateLightOnAllMaterials>();
		if (component4 != null)
		{
			component4.Reset();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetDamageStateIndex(BlockValue _blockValue)
	{
		float num = _blockValue.Block.MaxDamage - _blockValue.damage;
		int num2 = damageStates.Count - 1;
		for (int i = 0; i < num2; i++)
		{
			if (num > damageStates[i + 1].health)
			{
				return i;
			}
		}
		return num2;
	}

	public float GetNextDamageStateDownHealth(BlockValue _blockValue)
	{
		return damageStates[Utils.FastMin(GetDamageStateCount() - 1, GetDamageStateIndex(_blockValue) + 1)].health;
	}

	public float GetNextDamageStateUpHealth(BlockValue _blockValue)
	{
		return damageStates[Utils.FastMax(0, GetDamageStateIndex(_blockValue) - 1)].health;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetDamageStateCount()
	{
		return damageStates.Count;
	}

	public override float GetStepHeight(BlockValue _blockValue, BlockFace crossingFace)
	{
		if (isCustomBounds && _blockValue.Block.IsCollideMovement)
		{
			return boundsArr[0].size.y;
		}
		return base.GetStepHeight(_blockValue, crossingFace);
	}
}
