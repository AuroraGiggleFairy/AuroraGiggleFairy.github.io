using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

[Preserve]
public class EntityFallingBlock : Entity
{
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
	public BlockValue blockValue;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTerrain;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float terrainScale;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float massKg;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public TextureFullArray textureFull;

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
	public Collider myCollider;

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
		transform.GetComponent<BoxCollider>().enabled = false;
		transform.GetComponent<SphereCollider>().enabled = false;
	}

	public override void InitLocation(Vector3 _pos, Vector3 _rot)
	{
		base.InitLocation(_pos, _rot);
		if (!isEntityRemote)
		{
			rigidBody.position = _pos - Origin.position;
			rigidBody.rotation = Quaternion.Euler(_rot);
		}
	}

	public BlockValue GetBlockValue()
	{
		return blockValue;
	}

	public void SetBlockValue(BlockValue _blockValue)
	{
		blockValue = _blockValue;
		isTerrain = blockValue.Block.shape.IsTerrain();
		if (isTerrain)
		{
			terrainScale = rand.RandomRange(0.3f, 0.98f);
			myCollider = base.transform.GetComponent<SphereCollider>();
		}
		else
		{
			myCollider = base.transform.GetComponent<BoxCollider>();
		}
	}

	public TextureFullArray GetTextureFull()
	{
		return textureFull;
	}

	public void SetTextureFull(TextureFullArray _textureFull)
	{
		textureFull = _textureFull;
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
				if (isTerrain)
				{
					base.transform.localScale = new Vector3(terrainScale, terrainScale, terrainScale);
					TextureAtlasTerrain textureAtlasTerrain = (TextureAtlasTerrain)MeshDescription.meshes[5].textureAtlas;
					int sideTextureId = blockValue.Block.GetSideTextureId(blockValue, BlockFace.Top, 0);
					MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
					materialPropertyBlock.SetTexture("_MainTex", textureAtlasTerrain.diffuse[sideTextureId]);
					materialPropertyBlock.SetTexture("_BumpMap", textureAtlasTerrain.normal[sideTextureId]);
					meshRenderer.SetPropertyBlock(materialPropertyBlock);
				}
			}
		}
		if ((bool)myCollider && !myCollider.enabled)
		{
			myCollider.enabled = true;
			Block block = blockValue.Block;
			massKg = Utils.FastMin(block.blockMaterial.Hardness.Value * (float)block.blockMaterial.Mass.Value, 10f) * 8f;
			massKg *= (isTerrain ? (terrainScale * terrainScale * 1.5f) : (block.isMultiBlock ? 2.2f : 1f));
			if (!isEntityRemote)
			{
				rigidBody.mass = Utils.FastMax(10f, massKg);
				rigidBody.velocity = startVel;
				rigidBody.angularVelocity = rand.RandomOnUnitSphere * startAngularVel;
			}
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
		Block block = blockValue.Block;
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
					Log.Warning("{0} EntityFallingBlock {1} hit {2}, vel {3}, for {4}", GameManager.frameCount, this, entity, velocity, num);
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
			BlockValue block2 = world.GetBlock(pos);
			if (!block2.isair && world.GetStability(pos) > 0)
			{
				float time = Time.time;
				if (time - lastTimeEndParticleSpawned > 0.15f)
				{
					lastTimeEndParticleSpawned = time;
					Block block3 = block;
					string destroyParticle = block3.GetDestroyParticle(blockValue);
					if (destroyParticle != null && block3.blockMaterial.SurfaceCategory != null)
					{
						Vector3i blockPos = World.worldToBlockPos(transform.position + new Vector3(0f, 0.5f, 0f) + Origin.position);
						world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect("blockdestroy_" + destroyParticle, World.blockToTransformPos(blockPos), world.GetLightBrightness(blockPos), block3.GetColorForSide(blockValue, BlockFace.Top), block3.blockMaterial.SurfaceCategory + "destroy", null, _OLDCreateColliders: false), entityId);
					}
				}
				if (GamePrefs.GetBool(EnumGamePrefs.OptionsStabSpawnBlocksOnGround) && (!isTerrain || block2.Block.shape.IsTerrain()))
				{
					if (block.HasItemsToDropForEvent(EnumDropEvent.Fall))
					{
						float overallProb = 1f;
						List<Block.SItemDropProb> list = block.itemsToDrop[EnumDropEvent.Fall];
						if (list.Count > 0)
						{
							overallProb = list[0].prob;
						}
						block.DropItemsOnEvent(world, blockValue, EnumDropEvent.Fall, overallProb, GetPosition(), new Vector3(1.5f, 0f, 1.5f), Constants.cItemExplosionLifetime, -1, _bGetSameItemIfNoneFound: false);
					}
					else if (fallTimeInTicks < 16)
					{
						block.DropItemsOnEvent(world, blockValue, EnumDropEvent.Destroy, 0.7f, GetPosition(), new Vector3(1.5f, 0f, 1.5f), Constants.cItemExplosionLifetime, -1, _bGetSameItemIfNoneFound: false);
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateMesh()
	{
		int num = blockValue.ToItemType();
		ItemClass forId = ItemClass.GetForId(num);
		if (num == 0 || forId == null)
		{
			Log.Error("EntityFallingBlock failed id {0}, type", num);
			SetDead();
			return;
		}
		Transform transform = base.transform;
		Transform transform2 = null;
		if (isTerrain)
		{
			GameObject gameObject = DataLoader.LoadAsset<GameObject>("@:Entities/Debris/Falling/Terrain1.prefab");
			if ((bool)gameObject)
			{
				transform2 = UnityEngine.Object.Instantiate(gameObject).transform;
				transform2.SetParent(transform, worldPositionStays: false);
				transform2.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			}
		}
		else
		{
			transform2 = forId.CloneModel(world, blockValue.ToItemValue(), position, transform, BlockShape.MeshPurpose.Local, textureFull);
		}
		if (!transform2)
		{
			Log.Warning("EntityFallingBlock failed id {0}, mesh", num);
			SetDead();
			return;
		}
		transform2.rotation = blockValue.Block.shape.GetRotation(blockValue);
		meshRenderer = transform2.GetComponentInChildren<Renderer>();
		if (!isTerrain)
		{
			if ((bool)meshRenderer)
			{
				meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			}
			Collider[] componentsInChildren = transform2.GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			Animator[] componentsInChildren2 = transform2.GetComponentsInChildren<Animator>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].enabled = false;
			}
			Utils.SetColliderLayerRecursively(transform.gameObject, 13);
		}
	}

	public override void VisiblityCheck(float _distanceSqr, bool _masterIsZooming)
	{
		if ((bool)meshRenderer)
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
				world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect(text, GetPosition(), Quaternion.identity, lightBrightness, colorForSide, blockValue.Block.blockMaterial.SurfaceCategory + "hit" + block.Block.blockMaterial.SurfaceCategory, null), entityId);
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
		if (!isTerrain && (bool)meshRenderer)
		{
			UnityEngine.Object.Destroy(meshRenderer.material);
		}
	}
}
