using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

[Preserve]
public class EntityFallingTree : Entity
{
	[Preserve]
	public class NetPackageTreeFade : NetPackage
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public int entityId;

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

		public NetPackageTreeFade Setup(Entity entity)
		{
			entityId = entity.entityId;
			return this;
		}

		public override void read(PooledBinaryReader _reader)
		{
			entityId = _reader.ReadInt32();
		}

		public override void write(PooledBinaryWriter _writer)
		{
			base.write(_writer);
			_writer.Write(entityId);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			if (_world != null)
			{
				EntityFallingTree entityFallingTree = _world.GetEntity(entityId) as EntityFallingTree;
				if (entityFallingTree != null)
				{
					entityFallingTree.targetFade = 0f;
				}
			}
		}

		public override int GetLength()
		{
			return 4;
		}
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMassScale = 5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i treeBlockPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue treeBV;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 fallTreeDir;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform treeTransform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Rigidbody treeRB;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<MeshRenderer> rendererList = new List<MeshRenderer>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float collHeight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isMeshCreated;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timeToRemoveTree = -1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timeToEnableDamage;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> hitEntities = new List<int>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float fade = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float targetFade = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Material> mats = new List<Material>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		treeRB = GetComponent<Rigidbody>();
		treeRB.useGravity = !isEntityRemote;
		treeRB.isKinematic = isEntityRemote;
	}

	public Vector3i GetBlockPos()
	{
		return treeBlockPos;
	}

	public Vector3 GetFallTreeDir()
	{
		return fallTreeDir;
	}

	public void SetBlockPos(Vector3i _blockPos, Vector3 _fallTreeDir)
	{
		treeBlockPos = _blockPos;
		fallTreeDir = _fallTreeDir;
		if (!isEntityRemote)
		{
			SetAirBorne(_b: true);
		}
		Chunk chunk = (Chunk)world.GetChunkFromWorldPos(treeBlockPos);
		if (chunk == null)
		{
			return;
		}
		treeBV = chunk.GetBlock(World.toBlock(_blockPos));
		if (DecoManager.Instance.IsEnabled && treeBV.Block.IsDistantDecoration)
		{
			treeTransform = DecoManager.Instance.GetDecorationTransform(treeBlockPos, _bDetachTransform: true);
		}
		else
		{
			BlockEntityData blockEntity = chunk.GetBlockEntity(treeBlockPos);
			if (blockEntity != null && blockEntity.bHasTransform)
			{
				treeTransform = blockEntity.transform;
				blockEntity.transform = null;
				blockEntity.bHasTransform = false;
			}
		}
		collHeight = 3f;
		if ((bool)treeTransform)
		{
			Collider[] componentsInChildren = treeTransform.GetComponentsInChildren<Collider>();
			foreach (Collider obj in componentsInChildren)
			{
				obj.enabled = false;
				if (obj is CapsuleCollider capsuleCollider)
				{
					collHeight = Utils.FastMax(collHeight, capsuleCollider.height);
				}
			}
		}
		collHeight *= 0.9f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnCollisionEnter(Collision collision)
	{
		Collide(collision);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnCollisionStay(Collision collision)
	{
		Collide(collision);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Collide(Collision collision)
	{
		if (isEntityRemote || collision.contactCount == 0)
		{
			return;
		}
		float magnitude = collision.relativeVelocity.magnitude;
		if (magnitude > 1f)
		{
			collidedWith(collision.gameObject.transform);
		}
		if (!(magnitude > 0.2f) || !(collision.impulse.magnitude / treeRB.mass > 1.5f))
		{
			return;
		}
		Vector3 point = base.transform.position;
		float num = -1f;
		for (int i = 0; i < collision.contactCount; i++)
		{
			ContactPoint contact = collision.GetContact(i);
			float magnitude2 = contact.impulse.magnitude;
			if (magnitude2 > num)
			{
				num = magnitude2;
				point = contact.point;
			}
		}
		Manager.BroadcastPlay(this, "treefallimpact");
		ParticleEffect pe = new ParticleEffect("treefall", point + Origin.position, base.transform.rotation * Quaternion.AngleAxis(90f, Vector3.forward), 1f, Color.white, null, null);
		GameManager.Instance.SpawnParticleEffectServer(pe, entityId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void collidedWith(Transform _other)
	{
		if (timeToEnableDamage > 0f)
		{
			return;
		}
		Transform transform = _other;
		string text = _other.tag;
		if (text.StartsWith("E_BP_"))
		{
			transform = GameUtils.GetHitRootTransform(text, transform);
			text = transform.tag;
		}
		if (text.StartsWith("E_"))
		{
			Entity component = transform.GetComponent<Entity>();
			if ((bool)component && !component.IsDead() && treeCanDamageEntity(component))
			{
				hitEntities.Add(component.entityId);
				int damage = (int)(treeRB.mass * 0.35999998f);
				StartCoroutine(onEntityDamageLater(component, damage));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool treeCanDamageEntity(Entity _entity)
	{
		if (hitEntities.Contains(_entity.entityId))
		{
			return false;
		}
		if (!(_entity is EntityPlayer))
		{
			return !(_entity is EntitySupplyCrate);
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator onEntityDamageLater(Entity _entity, int _damage)
	{
		yield return new WaitForSeconds(0.05f);
		if (!_entity.IsDead() && _damage > 10)
		{
			_entity.DamageEntity(new DamageSource(EnumDamageSource.External, EnumDamageTypes.Crushing), _damage, _criticalHit: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateMesh()
	{
		if (!isEntityRemote && world.GetBlock(treeBlockPos).type == treeBV.type)
		{
			world.SetBlockRPC(treeBlockPos, BlockValue.Air);
		}
		if (treeBV.isair)
		{
			if ((bool)treeTransform)
			{
				UnityEngine.Object.Destroy(treeTransform.gameObject);
				treeTransform = null;
			}
			return;
		}
		Transform transform = base.transform;
		SetPosition(treeTransform.position + Origin.position);
		SetRotation(treeTransform.eulerAngles);
		transform.SetPositionAndRotation(treeTransform.position, treeTransform.rotation);
		treeTransform.SetParent(transform, worldPositionStays: false);
		treeTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		Transform transform2 = treeTransform.Find("rootBall");
		if ((bool)transform2)
		{
			transform2.gameObject.SetActive(value: true);
			transform2.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
		}
		treeRB.useGravity = !isEntityRemote;
		treeRB.isKinematic = isEntityRemote;
		treeRB.position = treeTransform.position;
		treeRB.rotation = treeTransform.rotation;
		Block block = treeBV.Block;
		float num = ((block.shape is BlockShapeModelEntity blockShapeModelEntity) ? blockShapeModelEntity.modelOffset.y : 0f);
		if (Physics.SphereCast(new Ray(transform.position + 3f * Vector3.up, Vector3.down), 0.25f, out var hitInfo, 5f, -538750981))
		{
			num = transform.position.y - hitInfo.point.y;
		}
		transform.gameObject.layer = 23;
		CapsuleCollider component = transform.GetComponent<CapsuleCollider>();
		component.height = collHeight;
		component.center = new Vector3(0f, collHeight * 0.5f - num, 0f);
		component.enabled = true;
		treeRB.mass = (15f + 7f * collHeight) * 5f;
		treeRB.centerOfMass = new Vector3(0f, collHeight * 0.3f - num, 0f);
		if (!isEntityRemote)
		{
			treeRB.velocity = Vector3.zero;
			treeRB.angularVelocity = Vector3.zero;
			treeRB.solverIterations = 10;
			treeRB.solverVelocityIterations = 3;
			treeRB.AddForceAtPosition(fallTreeDir * ((80f + collHeight * 8f) * 5f), transform.position + Vector3.up * (collHeight * 0.65f - num), ForceMode.Impulse);
			block.SpawnDestroyParticleEffect(world, treeBV, treeBlockPos, world.GetLightBrightness(treeBlockPos), block.GetColorForSide(treeBV, BlockFace.Top), -1);
			lifetime = 3f;
			timeToEnableDamage = 1.5f;
		}
		rendererList.Clear();
		MeshRenderer[] componentsInChildren = transform.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer item in componentsInChildren)
		{
			rendererList.Add(item);
		}
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		if (!isMeshCreated && (bool)treeTransform)
		{
			isMeshCreated = true;
			CreateMesh();
		}
		if (isEntityRemote)
		{
			return;
		}
		timeToEnableDamage -= 0.05f;
		if (lifetime > 0f)
		{
			lifetime -= 0.05f;
			return;
		}
		if (timeToRemoveTree < 0f)
		{
			if (treeRB.angularVelocity.sqrMagnitude < 0.1f && treeRB.velocity.sqrMagnitude < 0.1f)
			{
				timeToRemoveTree = 1f;
				targetFade = 0f;
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageTreeFade>().Setup(this));
			}
		}
		else
		{
			timeToRemoveTree -= 0.05f;
			if (timeToRemoveTree < 0f)
			{
				DestroyTree();
			}
		}
		if (base.transform.position.y + Origin.position.y < 1f)
		{
			DestroyTree();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DestroyTree()
	{
		SetDead();
		if (!isEntityRemote && world.GetBlock(treeBlockPos).type == treeBV.type)
		{
			world.SetBlockRPC(treeBlockPos, BlockValue.Air);
		}
		if (treeTransform != null)
		{
			UnityEngine.Object.Destroy(treeTransform.gameObject);
			treeTransform = null;
		}
	}

	public override bool CanCollideWith(Entity _other)
	{
		return false;
	}

	public override bool IsQRotationUsed()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTransform()
	{
		float deltaTime = Time.deltaTime;
		fade = Mathf.MoveTowards(fade, targetFade, deltaTime);
		if (fade < 1f)
		{
			float num = fade;
			for (int i = 0; i < rendererList.Count; i++)
			{
				Renderer renderer = rendererList[i];
				if (!renderer)
				{
					continue;
				}
				if (fade < 0.5f && renderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly)
				{
					renderer.gameObject.SetActive(value: false);
				}
				renderer.GetMaterials(mats);
				for (int j = 0; j < mats.Count; j++)
				{
					if (num < 1f)
					{
						mats[j].EnableKeyword("ENABLE_FADEOUT");
					}
					else
					{
						mats[j].DisableKeyword("ENABLE_FADEOUT");
					}
					mats[j].SetFloat("_FadeOut", num);
				}
			}
			mats.Clear();
		}
		Transform transform = base.transform;
		Vector3 vector = transform.position;
		if (isEntityRemote)
		{
			float t = deltaTime * 20f;
			vector = Vector3.Lerp(vector, targetPos - Origin.position, t);
			Quaternion quaternion = Quaternion.Slerp(transform.rotation, targetQRot, t);
			transform.SetPositionAndRotation(vector, quaternion);
		}
		else
		{
			SetPosition(vector + Origin.position);
			SetRotation(transform.eulerAngles);
		}
	}

	public override bool IsSavedToFile()
	{
		return false;
	}

	public override void OnEntityUnload()
	{
		if (treeTransform != null)
		{
			UnityEngine.Object.Destroy(treeTransform.gameObject);
			treeTransform = null;
		}
		base.OnEntityUnload();
	}

	public override void MarkToUnload()
	{
		base.MarkToUnload();
		if (!isEntityRemote)
		{
			world.SetBlockRPC(treeBlockPos, BlockValue.Air);
		}
	}
}
