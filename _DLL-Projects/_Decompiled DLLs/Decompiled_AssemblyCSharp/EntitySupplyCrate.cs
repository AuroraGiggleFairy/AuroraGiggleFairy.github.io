using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntitySupplyCrate : EntityAlive
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float startRotY;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public new bool wasOnGround;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int showParachuteInTicks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int closeParachuteInTicks;

	public bool isSmokeOn = true;

	public float smokeTimeAfterLanding = 240f;

	public float smokeTimeOnGround;

	public float smokeTimer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform crateT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform parachuteT;

	public override bool IsValidAimAssistSnapTarget => false;

	public override void PostInit()
	{
		base.PostInit();
		ValidateResources();
		base.gameObject.layer = 21;
		Collider component = GetComponent<Collider>();
		if ((bool)component)
		{
			component.enabled = false;
			component.enabled = true;
		}
		if (wasOnGround)
		{
			StopSmokeAndLights();
			if ((bool)parachuteT)
			{
				parachuteT.gameObject.SetActive(value: false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleNavObject()
	{
		if (GameStats.GetBool(EnumGameStats.AirDropMarker))
		{
			NavObjectManager.Instance.UnRegisterNavObjectByEntityID(entityId);
			if (EntityClass.list[entityClass].NavObject != "")
			{
				NavObject = NavObjectManager.Instance.RegisterNavObject(EntityClass.list[entityClass].NavObject, this);
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				Vector3 vector = NavObject.GetPosition() + Origin.position;
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageNavObject>().Setup(NavObject.NavObjectClass.NavObjectClassName, NavObject.DisplayName, vector, _isAdd: true, NavObject.usingLocalizationId, entityId));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		startRotY = rotation.y;
	}

	public override void OnEntityUnload()
	{
		base.OnEntityUnload();
		if (unloadReason == EnumRemoveEntityReason.Killed && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			GameManager.Instance.World.aiDirector.GetComponent<AIDirectorAirDropComponent>().RemoveSupplyCrate(entityId);
		}
	}

	public override EnumMapObjectType GetMapObjectType()
	{
		return EnumMapObjectType.SupplyDrop;
	}

	public override void SetMotionMultiplier(float _motionMultiplier)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void fallHitGround(float _v, Vector3 _fallMotion)
	{
		base.fallHitGround(Mathf.Min(_v, 5f), new Vector3(_fallMotion.x, Mathf.Max(-0.75f, _fallMotion.y), _fallMotion.z));
	}

	public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
	{
		base.MoveEntityHeaded(_direction, _isDirAbsolute);
		if (!(AttachedToEntity != null) && ((EModelSupplyCrate)emodel).parachute.gameObject.activeSelf && !IsInWater())
		{
			motion.y += ScalePhysicsAddConstant(world.Gravity * 0.95f);
		}
	}

	public bool RequiresChunkObserver()
	{
		if (onGround)
		{
			return isSmokeOn;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ValidateResources()
	{
		if (!crateT)
		{
			crateT = base.transform.FindInChilds("SupplyCrateEntityPrefab");
		}
		if (!parachuteT)
		{
			parachuteT = base.transform.FindInChilds("parachute_supplies");
		}
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		if (showParachuteInTicks > 0)
		{
			showParachuteInTicks--;
		}
		if (closeParachuteInTicks > 0)
		{
			closeParachuteInTicks--;
		}
		if (!onGround && wasOnGround)
		{
			showParachuteInTicks = 10;
		}
		if (onGround && !wasOnGround)
		{
			closeParachuteInTicks = 10;
		}
		if ((onGround || IsInWater()) && closeParachuteInTicks <= 0)
		{
			((EModelSupplyCrate)emodel).parachute.gameObject.SetActive(value: false);
		}
		if (onGround && !wasOnGround)
		{
			float lightBrightness = world.GetLightBrightness(GetBlockPosition());
			GameManager.Instance.SpawnParticleEffectClient(new ParticleEffect("supply_crate_impact", GetPosition(), Quaternion.identity, lightBrightness, Color.white), entityId);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				AIDirectorAirDropComponent component = GameManager.Instance.World.aiDirector.GetComponent<AIDirectorAirDropComponent>();
				component.SetSupplyCratePosition(entityId, World.worldToBlockPos(position));
				component.RefreshCrates();
			}
		}
		wasOnGround = onGround;
	}

	public override bool CanUpdateEntity()
	{
		if (!isEntityRemote)
		{
			return base.CanUpdateEntity();
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		float time = Time.time;
		if (!GameManager.IsDedicatedServer)
		{
			if (wasOnGround && isSmokeOn)
			{
				if (smokeTimeOnGround == 0f)
				{
					smokeTimer = time;
				}
				smokeTimeOnGround = time - smokeTimer + 0.0001f;
				if (time > smokeTimer + smokeTimeAfterLanding)
				{
					StopSmokeAndLights();
				}
			}
			ValidateResources();
		}
		if (!onGround)
		{
			Vector3 localEulerAngles = default(Vector3);
			localEulerAngles.x = Mathf.Sin(time) * 8f - 4f;
			localEulerAngles.y = Mathf.Sin(time + 0.3f) * 8f - 4f + startRotY;
			localEulerAngles.z = 0f;
			ModelTransform.localEulerAngles = localEulerAngles;
			SetRotation(localEulerAngles);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StopSmokeAndLights()
	{
		isSmokeOn = false;
		Transform modelTransform = emodel.GetModelTransform();
		List<Transform> list = new List<Transform>();
		GameUtils.FindTagInChilds(modelTransform, "SupplySmoke", list);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			ParticleSystem[] componentsInChildren = list[num].GetComponentsInChildren<ParticleSystem>();
			for (int num2 = componentsInChildren.Length - 1; num2 >= 0; num2--)
			{
				ParticleSystem.MainModule main = componentsInChildren[num2].main;
				main.loop = false;
			}
		}
		list.Clear();
		GameUtils.FindTagInChilds(modelTransform, "SupplyLit", list);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].gameObject.SetActive(value: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isRadiationSensitive()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool canDespawn()
	{
		return false;
	}

	public override bool IsSavedToFile()
	{
		return true;
	}

	public override bool CanCollideWithBlocks()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isGameMessageOnDeath()
	{
		return false;
	}

	public override void OnEntityDeath()
	{
		base.OnEntityDeath();
		GameManager.Instance.World.ObjectOnMapRemove(EnumMapObjectType.SupplyDrop, entityId);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.SupplyDrop, entityId));
			GameManager.Instance.DropContentOfLootContainerServer(BlockValue.Air, Vector3i.zero, entityId);
		}
	}

	public override bool CanBePushed()
	{
		return false;
	}

	public override bool CanCollideWith(Entity _other)
	{
		return false;
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		if (_version > 11)
		{
			wasOnGround = _br.ReadBoolean();
			closeParachuteInTicks = _br.ReadInt32();
			showParachuteInTicks = _br.ReadInt32();
		}
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		base.Write(_bw, _bNetworkWrite);
		_bw.Write(wasOnGround);
		_bw.Write(closeParachuteInTicks);
		_bw.Write(showParachuteInTicks);
	}
}
