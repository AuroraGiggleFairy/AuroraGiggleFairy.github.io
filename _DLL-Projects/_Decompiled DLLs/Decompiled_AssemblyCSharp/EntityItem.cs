using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityItem : Entity
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Rigidbody itemRB;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool useGravity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Renderer[] meshRenderers;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject meshGameObject;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform itemTransform;

	public ItemStack itemStack = ItemStack.Empty.Clone();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack lastCachedItemStack = ItemStack.Empty.Clone();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bMeshCreated;

	public ItemClass itemClass;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float stickPercent;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform stickT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 stickRelativePos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion stickRot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemWorldData itemWorldData;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bWasThrown;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int onGroundCounter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int distractionLifetime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float distractionStrength;

	public int distractionEatTicks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int nextDistractionTick;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float distractionRadiusSq;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Entity> distractionTargets = new List<Entity>();

	public int OwnerId = -1;

	public static int ItemInstanceCount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<ContactPoint> contactPoints;

	public override EnumPositionUpdateMovementType positionUpdateMovementType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return EnumPositionUpdateMovementType.Instant;
		}
	}

	public bool IsDistractionActive => distractionLifetime > 0;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		usePhysicsMaster = true;
		isPhysicsMaster = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		base.Awake();
		yOffset = 0.15f;
		ItemInstanceCount++;
		Collider component = GetComponent<Collider>();
		if ((bool)component)
		{
			component.enabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~EntityItem()
	{
		ItemInstanceCount--;
	}

	public override void Init(int _entityClass)
	{
		base.Init(_entityClass);
		itemRB = GetComponent<Rigidbody>();
	}

	public override void PostInit()
	{
		base.PostInit();
		PhysicsSetRB(itemRB);
		base.transform.eulerAngles = rotation;
		if (itemClass != null)
		{
			stickPercent = itemClass.Properties.GetFloat("StickPercent");
			if (itemStack != null)
			{
				itemWorldData = itemClass.CreateWorldData(GameManager.Instance, this, itemStack.itemValue, belongsPlayerId);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleNavObject()
	{
		if (bWasThrown && itemClass != null && OwnerId != -1 && world.GetEntity(OwnerId) as EntityPlayerLocal != null && itemClass.NavObject != "")
		{
			NavObject = NavObjectManager.Instance.RegisterNavObject(itemClass.NavObject, base.transform);
		}
	}

	public void SetItemStack(ItemStack _itemStack)
	{
		if (itemStack == null)
		{
			itemStack = ItemStack.Empty.Clone();
		}
		lastCachedItemStack = itemStack.Clone();
		itemStack = _itemStack;
		itemClass = ItemClass.GetForId(itemStack.itemValue.type);
		distractionRadiusSq = EffectManager.GetValue(PassiveEffects.DistractionRadius, itemStack.itemValue);
		distractionRadiusSq *= distractionRadiusSq;
		distractionLifetime = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.DistractionLifetime, itemStack.itemValue));
		distractionEatTicks = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.DistractionEatTicks, itemStack.itemValue));
		distractionStrength = EffectManager.GetValue(PassiveEffects.DistractionStrength, itemStack.itemValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new void FixedUpdate()
	{
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		if (!bMeshCreated)
		{
			createMesh();
		}
		if (itemWorldData != null)
		{
			itemClass.OnDroppedUpdate(itemWorldData);
		}
		if (Utils.FastAbs(position.y - prevPos.y) < 0.1f)
		{
			onGroundCounter++;
			if (onGroundCounter > 10)
			{
				onGround = true;
			}
		}
		if (isPhysicsMaster && !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && (ticksExisted & 1) != 0)
		{
			PhysicsMasterSendToServer(base.transform);
		}
		checkGravitySetting(isPhysicsMaster);
		if (!isEntityRemote)
		{
			if (!itemTransform)
			{
				lifetime = 0f;
			}
			lifetime -= 0.05f;
			if (lifetime <= 0f)
			{
				SetDead();
			}
			if (itemClass != null && itemClass.IsEatDistraction && distractionLifetime > 0 && distractionEatTicks <= 0)
			{
				SetDead();
			}
			if (base.transform.position.y + Origin.position.y < 0f)
			{
				SetDead();
			}
			if (!IsDead())
			{
				tickDistraction();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playThrowSound(string _name)
	{
		Manager.Play(this, "throw" + _name);
		Manager.Play(this, "throwdefault");
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale = 1f)
	{
		if (_strength >= 99999)
		{
			lifetime = 0f;
		}
		return base.DamageEntity(_damageSource, _strength, _criticalHit, impulseScale);
	}

	public override void OnDamagedByExplosion()
	{
		if (itemWorldData != null)
		{
			ItemClass.GetForId(itemStack.itemValue.type)?.OnDamagedByExplosion(itemWorldData);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void createMesh()
	{
		if (itemStack.itemValue.type == 0 || itemClass == null)
		{
			Log.Error($"Could not create item with id {itemStack.itemValue.type}");
			SetDead();
			return;
		}
		if (meshGameObject != null && lastCachedItemStack.itemValue.type != 0)
		{
			UnityEngine.Object.Destroy(meshGameObject);
			meshGameObject = null;
		}
		itemTransform = null;
		float num = 0f;
		Vector3 zero = Vector3.zero;
		if (itemClass.IsBlock())
		{
			BlockValue blockValue = itemStack.itemValue.ToBlockValue();
			if (itemTransform == null)
			{
				itemTransform = itemClass.CloneModel(meshGameObject, world, blockValue, null, Vector3.zero, base.transform, BlockShape.MeshPurpose.Drop);
			}
			Block block = blockValue.Block;
			if (block.Properties.Values.ContainsKey("DropScale"))
			{
				num = StringParsers.ParseFloat(block.Properties.Values["DropScale"]);
			}
		}
		else
		{
			if (itemTransform == null)
			{
				itemTransform = itemClass.CloneModel(world, itemStack.itemValue, Vector3.zero, base.transform, BlockShape.MeshPurpose.Drop);
			}
			if (itemClass.Properties.Values.ContainsKey("DropScale"))
			{
				num = StringParsers.ParseFloat(itemClass.Properties.Values["DropScale"]);
			}
		}
		if (num != 0f)
		{
			itemTransform.localScale = new Vector3(num, num, num);
		}
		itemTransform.localEulerAngles = itemClass.GetDroppedCorrectionRotation();
		itemTransform.localPosition = zero;
		bool flag = true;
		Collider[] componentsInChildren = itemTransform.GetComponentsInChildren<Collider>();
		int num2 = 0;
		while (componentsInChildren != null && num2 < componentsInChildren.Length)
		{
			Collider collider = componentsInChildren[num2];
			Rigidbody component = collider.gameObject.GetComponent<Rigidbody>();
			if (((bool)component && component.isKinematic) || (collider is MeshCollider && !((MeshCollider)collider).convex))
			{
				collider.enabled = false;
			}
			else
			{
				collider.gameObject.layer = 13;
				collider.enabled = true;
				flag = false;
				collider.gameObject.AddMissingComponent<RootTransformRefEntity>();
			}
			num2++;
		}
		base.transform.GetComponent<Collider>().enabled = flag;
		meshGameObject = itemTransform.gameObject;
		meshGameObject.SetActive(value: true);
		if (itemWorldData != null)
		{
			itemClass.OnMeshCreated(itemWorldData);
		}
		bMeshCreated = true;
		meshRenderers = itemTransform.GetComponentsInChildren<Renderer>(includeInactive: true);
		VisiblityCheck(0f, _masterIsZooming: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkGravitySetting(bool hasGravity)
	{
		bool flag = hasGravity && !stickT;
		if (useGravity != flag)
		{
			useGravity = flag;
			Rigidbody rigidbody = itemRB;
			if (flag)
			{
				rigidbody.useGravity = true;
				rigidbody.isKinematic = false;
				rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			}
			else
			{
				rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
				rigidbody.useGravity = false;
				rigidbody.isKinematic = true;
			}
		}
	}

	public override void AddVelocity(Vector3 _vel)
	{
		bWasThrown = true;
		SetAirBorne(_b: true);
		if (!bMeshCreated)
		{
			createMesh();
		}
		checkGravitySetting(isPhysicsMaster);
		if (isPhysicsMaster)
		{
			itemRB.angularVelocity = rand.RandomOnUnitSphere * (1f + _vel.magnitude * rand.RandomFloat * 8f);
			itemRB.AddForce(_vel * 6f, ForceMode.Impulse);
		}
	}

	public override void VisiblityCheck(float _distanceSqr, bool _masterIsZooming)
	{
		if ((bool)itemTransform && meshRenderers != null)
		{
			bool flag = _distanceSqr < (float)(_masterIsZooming ? 8100 : 3600);
			if (ticksExisted < 3 && _distanceSqr < 0.64000005f)
			{
				flag = false;
			}
			for (int i = 0; i < meshRenderers.Length; i++)
			{
				meshRenderers[i].enabled = flag;
			}
		}
	}

	public override bool CanCollideWith(Entity _other)
	{
		return true;
	}

	public virtual bool CanCollect()
	{
		if (itemClass == null)
		{
			return false;
		}
		return itemClass.CanCollect(itemStack.itemValue);
	}

	public override void OnLoadedFromEntityCache(EntityCreationData _ecd)
	{
		markedForUnload = false;
		base.transform.name = "Item_" + _ecd.id;
		SetItemStack(_ecd.itemStack);
		bMeshCreated = false;
		if (meshRenderers != null)
		{
			for (int i = 0; i < meshRenderers.Length; i++)
			{
				meshRenderers[i].enabled = false;
			}
		}
		UpdateLightOnChunkMesh updateLightOnChunkMesh = null;
		if (meshGameObject != null && (updateLightOnChunkMesh = meshGameObject.GetComponent<UpdateLightOnChunkMesh>()) != null)
		{
			updateLightOnChunkMesh.Reset();
		}
		itemWorldData = null;
		bDead = false;
		motion = Vector3.zero;
		addedToChunk = false;
		fallDistance = 0f;
	}

	public override Transform GetModelTransform()
	{
		return itemTransform;
	}

	public override void PhysicsMasterBecome()
	{
		checkGravitySetting(hasGravity: true);
		base.PhysicsMasterBecome();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTransform()
	{
		if (!isPhysicsMaster)
		{
			float deltaTime = Time.deltaTime;
			base.transform.position = Vector3.Lerp(base.transform.position, position - Origin.position, deltaTime * 7f);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, qrotation, deltaTime * 3f);
			return;
		}
		Vector3 vector = base.transform.position;
		if (!float.IsNaN(vector.x) && !float.IsNaN(vector.y) && !float.IsNaN(vector.z) && !float.IsInfinity(vector.x) && !float.IsInfinity(vector.y) && !float.IsInfinity(vector.z))
		{
			SetPosition(vector + Origin.position);
		}
		Quaternion quaternion = base.transform.rotation;
		Vector3 eulerAngles = quaternion.eulerAngles;
		if (!float.IsNaN(eulerAngles.x) && !float.IsNaN(eulerAngles.y) && !float.IsNaN(eulerAngles.z) && !float.IsInfinity(eulerAngles.x) && !float.IsInfinity(eulerAngles.y) && !float.IsInfinity(eulerAngles.z))
		{
			SetRotation(eulerAngles);
			qrotation = quaternion;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if ((bool)stickT)
		{
			Vector3 vector = stickT.TransformPoint(stickRelativePos);
			base.transform.position = vector;
			base.transform.rotation = stickT.rotation * stickRot;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void tickDistraction()
	{
		if (itemClass == null || distractionLifetime <= 0 || (!isCollided && itemClass.IsRequireContactDistraction) || !(distractionRadiusSq > 0f) || ++nextDistractionTick <= 20)
		{
			return;
		}
		nextDistractionTick = 0;
		Vector3 center = position;
		Bounds bb = new Bounds(center, new Vector3(distractionRadiusSq, distractionRadiusSq, distractionRadiusSq));
		world.GetEntitiesInBounds(typeof(EntityAlive), bb, distractionTargets);
		for (int i = 0; i < distractionTargets.Count; i++)
		{
			EntityAlive entityAlive = (EntityAlive)distractionTargets[i];
			if (entityAlive.IsSleeping || !(entityAlive.distraction == null))
			{
				continue;
			}
			EntityClass entityClass = EntityClass.list[entityAlive.entityClass];
			if (!itemClass.DistractionTags.IsEmpty && !itemClass.DistractionTags.Test_AnySet(entityClass.Tags))
			{
				continue;
			}
			float distanceSq = GetDistanceSq(entityAlive);
			if (distanceSq <= distractionRadiusSq && (entityAlive.pendingDistraction == null || distanceSq < entityAlive.pendingDistractionDistanceSq))
			{
				float num = entityAlive.distractionResistance - distractionStrength;
				if (num <= 0f || num < rand.RandomFloat * 100f)
				{
					entityAlive.pendingDistraction = this;
					entityAlive.pendingDistractionDistanceSq = distanceSq;
				}
			}
		}
		distractionTargets.Clear();
		if (distractionLifetime > 0)
		{
			distractionLifetime--;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnCollisionEnter(Collision collision)
	{
		if (!CanCollide(collision))
		{
			return;
		}
		CheckStick(collision);
		if (!isCollided)
		{
			isCollided = true;
			if (isPhysicsMaster && !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				PhysicsMasterSendToServer(base.transform);
			}
		}
		if (contactPoints == null)
		{
			contactPoints = new List<ContactPoint>(2);
		}
		for (int num = collision.GetContacts(contactPoints) - 1; num >= 0; num--)
		{
			ContactPoint contactPoint = contactPoints[num];
			if (Utils.FastAbs(Vector3.Dot(collision.relativeVelocity, contactPoint.normal)) < 1f)
			{
				continue;
			}
			Entity hitRootEntity = GameUtils.GetHitRootEntity(contactPoint.otherCollider.transform.tag, contactPoint.otherCollider.transform);
			if ((object)hitRootEntity != null)
			{
				string text = EntityClass.list[hitRootEntity.entityClass].Properties.Values["SurfaceCategory"];
				if (!string.IsNullOrEmpty(text) && itemClass != null && itemClass.MadeOfMaterial != null)
				{
					playThrowSound(itemClass.MadeOfMaterial.id + "hit" + text);
				}
				break;
			}
			Vector3i pos = World.worldToBlockPos(contactPoint.point - 0.25f * contactPoint.normal + Origin.position);
			BlockValue block = world.GetBlock(pos);
			if (block.isair)
			{
				WorldRayHitInfo worldRayHitInfo = new WorldRayHitInfo();
				GameUtils.FindMasterBlockForEntityModelBlock(world, -contactPoint.normal, contactPoint.otherCollider.transform.tag, contactPoint.point + Origin.position, contactPoint.otherCollider.transform, worldRayHitInfo);
				pos = worldRayHitInfo.hit.blockPos;
				block = world.GetBlock(pos);
				if (block.isair)
				{
					continue;
				}
			}
			float num2 = Utils.FastAbs(contactPoint.normal.x);
			float num3 = Utils.FastAbs(contactPoint.normal.y);
			float num4 = Utils.FastAbs(contactPoint.normal.z);
			BlockFace side = BlockFace.Top;
			if (num2 >= num3 && num2 >= num4)
			{
				if (contactPoint.normal.x < 0f)
				{
					side = BlockFace.East;
				}
				else if (contactPoint.normal.x > 0f)
				{
					side = BlockFace.West;
				}
			}
			else if (num4 >= num2 && num4 >= num3)
			{
				if (contactPoint.normal.z < 0f)
				{
					side = BlockFace.North;
				}
				else if (contactPoint.normal.z > 0f)
				{
					side = BlockFace.South;
				}
			}
			else if (contactPoint.normal.y < 0f)
			{
				side = BlockFace.Bottom;
			}
			string surfaceCategory = block.Block.GetMaterialForSide(block, side).SurfaceCategory;
			if (itemClass != null && itemClass.MadeOfMaterial != null)
			{
				playThrowSound(itemClass.MadeOfMaterial.id + "hit" + surfaceCategory);
			}
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CanCollide(Collision collision)
	{
		if (GameManager.Instance.World == null)
		{
			return false;
		}
		if (!bWasThrown && itemClass is ItemClassTimeBomb)
		{
			return false;
		}
		Transform transform = collision.transform;
		if (!transform)
		{
			return false;
		}
		string text = transform.tag;
		if (text != null && text.StartsWith("E_"))
		{
			Transform hitRootTransform = GameUtils.GetHitRootTransform(text, transform);
			if (hitRootTransform != null)
			{
				Entity component = hitRootTransform.GetComponent<Entity>();
				if (component != null && component.entityId == belongsPlayerId)
				{
					return false;
				}
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckStick(Collision collision)
	{
		if (stickPercent <= 0f)
		{
			return;
		}
		float num = 1f - stickPercent;
		itemRB.velocity *= num;
		itemRB.angularVelocity *= num;
		if (stickPercent >= 1f && !stickT)
		{
			stickT = collision.transform;
			stickRelativePos = stickT.InverseTransformPoint(base.transform.position);
			stickRot = Quaternion.Inverse(stickT.rotation) * base.transform.rotation;
			Collider[] componentsInChildren = itemRB.GetComponentsInChildren<Collider>();
			for (int num2 = componentsInChildren.Length - 1; num2 >= 0; num2--)
			{
				componentsInChildren[num2].gameObject.layer = 0;
			}
			checkGravitySetting(isPhysicsMaster);
			PlayOneShot(itemClass.SoundStick);
		}
	}

	public override string ToString()
	{
		if (itemStack.itemValue.HasQuality)
		{
			return $"[type={GetType().Name}, name={itemClass.Name}, cnt={itemStack.count}, quality={itemStack.itemValue.Quality}]";
		}
		return $"[type={GetType().Name}, name={((itemClass != null) ? itemClass.Name : string.Empty)}, cnt={itemStack.count}]";
	}

	public override bool IsQRotationUsed()
	{
		return true;
	}
}
