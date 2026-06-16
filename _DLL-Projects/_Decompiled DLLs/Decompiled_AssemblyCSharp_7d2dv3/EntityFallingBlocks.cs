using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

[Preserve]
public class EntityFallingBlocks : Entity
{
	public static bool Enabled = false;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3 renderOffsetV = new Vector3(-0.5f, -0.5f, -0.5f);

	public static int MaxGroupSize = 3;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxHitsPerEntity = 3;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMinDamageToCountHit = 1;

	public Transform prefabParticleOnFallT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Renderer meshRenderer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Rigidbody rigidBody;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue[] blockValues;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i[] blockPositions;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float massKg;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public TextureFullArray[] textureFullArrays;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isMeshCreated;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int fallTimeInTicks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, int> entityHits = new Dictionary<int, int>(8);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float lastTimeStartParticleSpawned;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static float lastTimeEndParticleSpawned;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int notMovingCount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isGroundHit;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public BoxCollider boxCollider;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<BoxCollider> boxColliders = new List<BoxCollider>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public long chunkKey;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<long, List<EntityFallingBlocks>> fallingBlocksByChunk = new Dictionary<long, List<EntityFallingBlocks>>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startVel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float startAngularVel = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 interpStartPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 interpEndPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float interpTime;

	public override EnumPositionUpdateMovementType positionUpdateMovementType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return EnumPositionUpdateMovementType.Instant;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		yOffset = 0.15f;
		Transform transform = base.transform;
		rigidBody = transform.GetComponent<Rigidbody>();
		if (isEntityRemote)
		{
			UnityEngine.Object.Destroy(rigidBody);
		}
		boxCollider = base.transform.GetComponent<BoxCollider>();
	}

	public override void InitLocation(Vector3 _pos, Vector3 _rot)
	{
		base.InitLocation(_pos, _rot);
		Vector2i vector2i = World.toChunkXZ(_pos);
		chunkKey = WorldChunkCache.MakeChunkKey(vector2i.x, vector2i.y);
		if (!fallingBlocksByChunk.ContainsKey(chunkKey))
		{
			fallingBlocksByChunk[chunkKey] = new List<EntityFallingBlocks>();
		}
		fallingBlocksByChunk[chunkKey].Add(this);
		if (!isEntityRemote)
		{
			rigidBody.position = _pos - Origin.position;
			rigidBody.rotation = Quaternion.Euler(_rot);
		}
	}

	public BlockValue[] GetBlockValues()
	{
		return blockValues;
	}

	public Vector3i[] GetBlockPositions()
	{
		return blockPositions;
	}

	public void SetBlockGroupData(Vector3i[] _blockPositions, BlockValue[] _blockValues)
	{
		blockPositions = _blockPositions;
		blockValues = _blockValues;
	}

	public TextureFullArray[] GetTextureFullArrays()
	{
		return textureFullArrays;
	}

	public void SetTextureFullArrays(TextureFullArray[] _textureFullArrays)
	{
		textureFullArrays = _textureFullArrays;
	}

	public void SetStartVelocity(Vector3 _vel, float _angularVel)
	{
		startVel = _vel;
		startAngularVel = _angularVel;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (!isMeshCreated && !GameManager.IsDedicatedServer)
		{
			CreateMesh();
			if ((bool)meshRenderer)
			{
				isMeshCreated = true;
				meshRenderer.enabled = true;
			}
		}
		if (boxColliders.Count <= 0 || boxColliders[0].enabled)
		{
			return;
		}
		foreach (BoxCollider boxCollider in boxColliders)
		{
			boxCollider.enabled = true;
		}
		massKg = 0f;
		BlockValue[] array = blockValues;
		foreach (BlockValue blockValue in array)
		{
			Block block = blockValue.Block;
			massKg += Utils.FastMin(block.blockMaterial.Hardness.Value * (float)block.blockMaterial.Mass.Value, 10f) * 8f;
		}
		if (!isEntityRemote)
		{
			rigidBody.mass = Utils.FastMax(10f, massKg);
			rigidBody.velocity = startVel;
			rigidBody.angularVelocity = rand.RandomOnUnitSphere * startAngularVel;
		}
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		if (IsDead())
		{
			return;
		}
		fallTimeInTicks++;
		Block block = blockValues[0].Block;
		if (fallTimeInTicks == 1 && Time.time - lastTimeStartParticleSpawned > 0.2f)
		{
			lastTimeStartParticleSpawned = Time.time;
			if (!GameManager.IsDedicatedServer && (bool)prefabParticleOnFallT)
			{
				UnityEngine.Object.Instantiate(prefabParticleOnFallT.gameObject, base.transform.position, Quaternion.identity).GetComponent<ParticleSystem>().Emit(10);
			}
		}
		if (isEntityRemote)
		{
			return;
		}
		Vector3 velocity = rigidBody.velocity;
		if ((fallTimeInTicks & 1) == 0)
		{
			List<Entity> entitiesInBounds = world.GetEntitiesInBounds(this, BoundsUtils.ExpandBounds(BoundsUtils.ExpandDirectional(boundingBox, motion), 0f, 0.2f, 0f));
			for (int i = 0; i < entitiesInBounds.Count; i++)
			{
				Entity entity = entitiesInBounds[i];
				int key = entity.entityId;
				entityHits.TryGetValue(key, out var value);
				if (value < 3 && entity.CanCollideWith(this) && !(position.y < entity.getHeadPosition().y) && velocity.y < -0.8f)
				{
					float originalValue = (int)Utils.FastMin(massKg * velocity.y * -0.05f, 40f);
					originalValue = EffectManager.GetValue(PassiveEffects.FallingBlockDamage, null, originalValue, entity as EntityAlive);
					int num = (int)originalValue;
					entity.DamageEntity(DamageSource.fallingBlock, num, _criticalHit: false);
					if (num >= 1)
					{
						value++;
						entityHits[key] = value;
					}
					Log.Warning("{0} EntityFallingBlocks {1} hit {2}, vel {3}, for {4}", GameManager.frameCount, this, entity, velocity, num);
				}
			}
		}
		bool flag = false;
		Transform transform = base.transform;
		if (fallTimeInTicks < 60 || velocity.sqrMagnitude > 0.0625f)
		{
			notMovingCount = 0;
		}
		else if (++notMovingCount > 3)
		{
			Vector3i pos = World.worldToBlockPos(position + Vector3.down);
			if (!world.GetBlock(pos).isair && world.GetStability(pos) > 0)
			{
				float time = Time.time;
				if (time - lastTimeEndParticleSpawned > 0.15f)
				{
					lastTimeEndParticleSpawned = time;
					Block block2 = block;
					string destroyParticle = block2.GetDestroyParticle(blockValues[0]);
					if (destroyParticle != null && block2.blockMaterial.SurfaceCategory != null)
					{
						Vector3i blockPos = World.worldToBlockPos(transform.position + new Vector3(0f, 0.5f, 0f) + Origin.position);
						world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect("blockdestroy_" + destroyParticle, World.blockToTransformPos(blockPos), world.GetLightBrightness(blockPos), block2.GetColorForSide(blockValues[0], BlockFace.Top), block2.blockMaterial.SurfaceCategory + "destroy", null, _OLDCreateColliders: false), entityId);
					}
				}
				if (GamePrefs.GetBool(EnumGamePrefs.OptionsStabSpawnBlocksOnGround))
				{
					if (block.HasItemsToDropForEvent(EnumDropEvent.Fall))
					{
						float overallProb = 1f;
						List<Block.SItemDropProb> list = block.itemsToDrop[EnumDropEvent.Fall];
						if (list.Count > 0)
						{
							overallProb = list[0].prob;
						}
						block.DropItemsOnEvent(world, blockValues[0], EnumDropEvent.Fall, overallProb, GetPosition(), new Vector3(1.5f, 0f, 1.5f), Constants.cItemExplosionLifetime, -1, _bGetSameItemIfNoneFound: false);
					}
					else if (fallTimeInTicks < 16)
					{
						block.DropItemsOnEvent(world, blockValues[0], EnumDropEvent.Destroy, 0.7f, GetPosition(), new Vector3(1.5f, 0f, 1.5f), Constants.cItemExplosionLifetime, -1, _bGetSameItemIfNoneFound: false);
					}
				}
			}
			flag = true;
		}
		if (fallTimeInTicks > 300)
		{
			flag = true;
		}
		if (transform.position.y + Origin.position.y < 2f)
		{
			flag = true;
		}
		if (flag)
		{
			SetDead();
		}
	}

	public override void SetDead()
	{
		fallingBlocksByChunk[chunkKey].Remove(this);
		base.SetDead();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateMesh()
	{
		int num = blockValues[0].ToItemType();
		ItemClass forId = ItemClass.GetForId(num);
		if (num == 0 || forId == null)
		{
			Log.Error("EntityFallingBlocks failed id {0}, type", num);
			SetDead();
			return;
		}
		Transform transform = base.transform;
		Transform transform2 = null;
		int meshIndex = blockValues[0].Block.MeshIndex;
		Vector3i vector3i = World.worldToBlockPos(blockPositions[0]);
		world.GetSunAndBlockColors(vector3i, out var sunLight, out var blockLight);
		VoxelMesh voxelMesh = VoxelMesh.Create(meshIndex, MeshDescription.meshes[meshIndex].meshType, 1);
		VoxelMesh[] array = new VoxelMesh[MeshDescription.meshes.Length];
		array[meshIndex] = voxelMesh;
		for (int i = 0; i < blockValues.Length; i++)
		{
			BlockValue blockValue = blockValues[i];
			Vector3i vector3i2 = World.worldToBlockPos(blockPositions[i]);
			if (!(blockValue.Block.shape is BlockShapeTerrain))
			{
				blockValue.Block.shape.renderFull(vector3i2, blockValue, vector3i2 - vector3i + renderOffsetV, null, new LightingAround(sunLight, blockLight, 0), textureFullArrays[i], array, BlockShape.MeshPurpose.Local);
				_ = voxelMesh.m_Vertices.Count;
			}
		}
		GameObject gameObject = new GameObject();
		gameObject.transform.SetParent(transform, worldPositionStays: false);
		gameObject.AddComponent<UpdateLightOnChunkMesh>();
		gameObject.name = "Block_" + blockValues[0].type;
		transform2 = gameObject.transform;
		VoxelMesh.CreateMeshFilter(meshIndex, 0, gameObject, "Item", _bAllowLOD: false, out var _mf, out var _mr);
		if (_mf != null)
		{
			voxelMesh.CopyToMesh(_mf, _mr, 0);
			for (int j = 0; j < blockValues.Length; j++)
			{
				_ = ref blockValues[j];
				Vector3i vector3i3 = World.worldToBlockPos(blockPositions[j]);
				GameObject obj = new GameObject($"blockCollider{j}");
				obj.transform.SetParent(transform, worldPositionStays: false);
				obj.transform.localPosition = vector3i3 - vector3i;
				BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
				boxCollider.size = new Vector3(0.9f, 0.9f, 0.9f);
				boxCollider.material = this.boxCollider.material;
				boxColliders.Add(boxCollider);
			}
		}
		if (!transform2)
		{
			Log.Warning("EntityFallingBlock failed id {0}, mesh", num);
			SetDead();
			return;
		}
		meshRenderer = transform2.GetComponentInChildren<Renderer>();
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		Collider[] componentsInChildren = transform2.GetComponentsInChildren<Collider>();
		for (int k = 0; k < componentsInChildren.Length; k++)
		{
			componentsInChildren[k].enabled = false;
		}
		Animator[] componentsInChildren2 = transform2.GetComponentsInChildren<Animator>();
		for (int k = 0; k < componentsInChildren2.Length; k++)
		{
			componentsInChildren2[k].enabled = false;
		}
		Utils.SetColliderLayerRecursively(transform.gameObject, 13);
	}

	public override void VisiblityCheck(float _distanceSqr, bool _masterIsZooming)
	{
		if (meshRenderer != null)
		{
			meshRenderer.enabled = _distanceSqr < (float)(_masterIsZooming ? 14400 : 10000);
		}
	}

	public override bool CanCollideWith(Entity _other)
	{
		return _other is EntityAlive;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTransform()
	{
		Transform transform = base.transform;
		if (isEntityRemote)
		{
			if (targetPos.sqrMagnitude > 0f && (targetPos - interpEndPos).sqrMagnitude > 0.0001f)
			{
				interpStartPos = transform.position + Origin.position;
				interpEndPos = targetPos;
				interpTime = 0.1f;
			}
			Vector3 vector = targetPos;
			float deltaTime = Time.deltaTime;
			if (interpTime > 0f)
			{
				interpTime -= deltaTime;
				float t = 1f - interpTime / 0.1f;
				vector = Vector3.Lerp(interpStartPos, interpEndPos, t);
			}
			Quaternion quaternion = Quaternion.Slerp(transform.rotation, qrotation, deltaTime * 20f);
			transform.SetPositionAndRotation(vector - Origin.position, quaternion);
		}
		else
		{
			SetPosition(transform.position + Origin.position);
			SetRotation(transform.eulerAngles);
			qrotation = transform.rotation;
		}
	}

	public void OnContactEvent()
	{
		if (!isGroundHit && !isEntityRemote)
		{
			Vector3i blockPosition = GetBlockPosition();
			blockPosition.y--;
			BlockValue block = world.GetBlock(blockPosition);
			if (!block.isair)
			{
				isGroundHit = true;
				float lightBrightness = world.GetLightBrightness(blockPosition);
				Color colorForSide = block.Block.GetColorForSide(block, BlockFace.Top);
				string text = "impact_stone_on_" + block.Block.blockMaterial.SurfaceCategory;
				world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect(text, GetPosition(), Quaternion.identity, lightBrightness, colorForSide, blockValues[0].Block.blockMaterial.SurfaceCategory + "hit" + block.Block.blockMaterial.SurfaceCategory, null), entityId);
			}
		}
	}

	public override bool IsQRotationUsed()
	{
		return true;
	}

	public override void OnEntityUnload()
	{
		base.OnEntityUnload();
		if (meshRenderer != null)
		{
			UnityEngine.Object.Destroy(meshRenderer.material);
		}
	}

	public static void ClearFallingBlocksForChunks(HashSetLong chunks)
	{
		List<EntityFallingBlocks> list = new List<EntityFallingBlocks>();
		foreach (long chunk in chunks)
		{
			if (!fallingBlocksByChunk.ContainsKey(chunk))
			{
				continue;
			}
			foreach (EntityFallingBlocks item in fallingBlocksByChunk[chunk])
			{
				list.Add(item);
			}
		}
		foreach (EntityFallingBlocks item2 in list)
		{
			item2.SetDead();
		}
	}
}
