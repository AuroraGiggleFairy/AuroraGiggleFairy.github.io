using System;
using System.Collections.Generic;
using UnityEngine;

public class MotionSensorController : MonoBehaviour, IPowerSystemCamera
{
	public AutoTurretYawLerp YawController;

	public AutoTurretPitchLerp PitchController;

	public Transform Cone;

	public Material ConeMaterial;

	public Color ConeColor;

	public bool IsOn;

	public TileEntityPoweredTrigger TileEntity;

	public bool IsUserAccessing;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float baseConeYaw = 45f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float baseConePitch = 45f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float baseConeDistance = 4f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float maxDistance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 yawRange;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 pitchRange;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Bounds targetingBounds;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float fallAsleepTimeMax = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float fallAsleepTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool initialized;

	public void OnDestroy()
	{
		Cleanup();
		if (ConeMaterial != null)
		{
			UnityEngine.Object.Destroy(ConeMaterial);
		}
	}

	public void Init(DynamicProperties _properties)
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		if (_properties.Values.ContainsKey("MaxDistance"))
		{
			maxDistance = StringParsers.ParseFloat(_properties.Values["MaxDistance"]);
		}
		else
		{
			maxDistance = 16f;
		}
		if (_properties.Values.ContainsKey("YawRange"))
		{
			float num = StringParsers.ParseFloat(_properties.Values["YawRange"]);
			num *= 0.5f;
			yawRange = new Vector2(0f - num, num);
		}
		else
		{
			yawRange = new Vector2(-22.5f, 22.5f);
		}
		if (_properties.Values.ContainsKey("PitchRange"))
		{
			float num2 = StringParsers.ParseFloat(_properties.Values["PitchRange"]);
			num2 *= 0.5f;
			pitchRange = new Vector2(0f - num2, num2);
		}
		else
		{
			pitchRange = new Vector2(-22.5f, 22.5f);
		}
		if (_properties.Values.ContainsKey("FallAsleepTime"))
		{
			fallAsleepTimeMax = StringParsers.ParseFloat(_properties.Values["FallAsleepTime"]);
		}
		Cone.localScale = new Vector3(Cone.localScale.x * (yawRange.y / 45f) * (maxDistance / 4f), Cone.localScale.y * (pitchRange.y / 45f) * (maxDistance / 4f), Cone.localScale.z * (maxDistance / 4f));
		Cone.gameObject.SetActive(value: false);
		WireManager.Instance.AddPulseObject(Cone.gameObject);
		targetingBounds = Cone.GetComponent<MeshRenderer>().bounds;
		YawController.BaseRotation = new Vector3(-90f, 0f, 0f);
		PitchController.BaseRotation = new Vector3(0f, 0f, 0f);
		if (!(Cone != null))
		{
			return;
		}
		MeshRenderer component = Cone.GetComponent<MeshRenderer>();
		if (component != null)
		{
			if (component.material != null)
			{
				ConeMaterial = component.material;
				ConeColor = ConeMaterial.GetColor("_Color");
			}
			else if (component.sharedMaterial != null)
			{
				ConeMaterial = component.sharedMaterial;
				ConeColor = ConeMaterial.GetColor("_Color");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (TileEntity == null)
		{
			return;
		}
		if (!TileEntity.IsPowered || IsUserAccessing)
		{
			if (IsUserAccessing)
			{
				YawController.Yaw = TileEntity.CenteredYaw;
				YawController.UpdateYaw();
				PitchController.Pitch = TileEntity.CenteredPitch;
				PitchController.UpdatePitch();
			}
			else if (!TileEntity.IsPowered)
			{
				if (YawController.Yaw != TileEntity.CenteredYaw)
				{
					YawController.Yaw = TileEntity.CenteredYaw;
					YawController.SetYaw();
				}
				if (PitchController.Pitch != TileEntity.CenteredPitch)
				{
					PitchController.Pitch = TileEntity.CenteredPitch;
					PitchController.SetPitch();
				}
			}
		}
		else if (TileEntity.IsPowered)
		{
			bool flag = hasTarget();
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				_ = (PowerTrigger)TileEntity.GetPowerItem();
				if (flag)
				{
					TileEntity.IsTriggered = true;
				}
			}
			YawController.UpdateYaw();
			PitchController.UpdatePitch();
			UpdateEmissionColor(flag);
		}
		else
		{
			UpdateEmissionColor(isTriggered: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEmissionColor(bool isTriggered)
	{
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
		if (componentsInChildren == null)
		{
			return;
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].material != componentsInChildren[i].sharedMaterial)
			{
				componentsInChildren[i].material = new Material(componentsInChildren[i].sharedMaterial);
			}
			if (TileEntity.IsPowered)
			{
				componentsInChildren[i].material.SetColor("_EmissionColor", isTriggered ? Color.green : Color.red);
			}
			else
			{
				componentsInChildren[i].material.SetColor("_EmissionColor", Color.black);
			}
			componentsInChildren[i].sharedMaterial = componentsInChildren[i].material;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasTarget()
	{
		List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityAlive), new Bounds(TileEntity.ToWorldPos().ToVector3(), Vector3.one * (maxDistance * 2f)), new List<Entity>());
		if (entitiesInBounds.Count > 0)
		{
			for (int i = 0; i < entitiesInBounds.Count; i++)
			{
				if (shouldIgnoreTarget(entitiesInBounds[i]))
				{
					continue;
				}
				Vector3 _targetPos = Vector3.zero;
				float _yaw = TileEntity.CenteredYaw;
				float _pitch = TileEntity.CenteredPitch;
				if (!trackTarget(entitiesInBounds[i], ref _yaw, ref _pitch, out _targetPos))
				{
					continue;
				}
				Ray ray = new Ray(Cone.transform.position + Origin.position, (_targetPos - Cone.transform.position).normalized);
				if (!Voxel.Raycast(GameManager.Instance.World, ray, maxDistance, -538750981, 8, 0.1f) || !Voxel.voxelRayHitInfo.tag.StartsWith("E_"))
				{
					continue;
				}
				if (Voxel.voxelRayHitInfo.tag == "E_Vehicle")
				{
					EntityVehicle entityVehicle = EntityVehicle.FindCollisionEntity(Voxel.voxelRayHitInfo.transform);
					if (entityVehicle != null && entityVehicle.IsAttached(entitiesInBounds[i]))
					{
						YawController.Yaw = _yaw;
						PitchController.Pitch = _pitch;
						return true;
					}
				}
				else
				{
					Transform hitRootTransform = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, Voxel.voxelRayHitInfo.transform);
					if (!(hitRootTransform == null) && hitRootTransform.GetComponent<Entity>() == entitiesInBounds[i])
					{
						YawController.Yaw = _yaw;
						PitchController.Pitch = _pitch;
						return true;
					}
				}
			}
		}
		YawController.Yaw = TileEntity.CenteredYaw;
		PitchController.Pitch = TileEntity.CenteredPitch;
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool trackTarget(Entity _target, ref float _yaw, ref float _pitch, out Vector3 _targetPos)
	{
		Vector3 vector = _target.getHeadPosition();
		if (vector == Vector3.zero)
		{
			vector = _target.position;
		}
		Vector3 vector2 = (_target.position + vector) * 0.5f;
		_targetPos = Vector3.Lerp(vector2, vector, 0.75f);
		EntityAlive entityAlive = _target as EntityAlive;
		if ((bool)entityAlive && entityAlive.GetWalkType() == 21)
		{
			_targetPos = vector2;
		}
		_targetPos -= Origin.position;
		Vector3 normalized = (_targetPos - YawController.transform.position).normalized;
		Vector3 normalized2 = (_targetPos - PitchController.transform.position).normalized;
		float num = Quaternion.LookRotation(normalized).eulerAngles.y - base.transform.rotation.eulerAngles.y;
		float num2 = Quaternion.LookRotation(normalized2).eulerAngles.x - base.transform.rotation.z;
		if (num > 180f)
		{
			num -= 360f;
		}
		if (num2 > 180f)
		{
			num2 -= 360f;
		}
		float num3 = TileEntity.CenteredYaw % 360f;
		float num4 = TileEntity.CenteredPitch % 360f;
		if (num3 > 180f)
		{
			num3 -= 360f;
		}
		if (num4 > 180f)
		{
			num4 -= 360f;
		}
		if (!(num >= num3 + yawRange.x) || !(num <= num3 + yawRange.y) || !(num2 >= num4 + pitchRange.x) || !(num2 <= num4 + pitchRange.y))
		{
			if (fallAsleepTime >= fallAsleepTimeMax)
			{
				YawController.Yaw = TileEntity.CenteredYaw;
				PitchController.Pitch = TileEntity.CenteredPitch;
				fallAsleepTime = 0f;
			}
			else
			{
				fallAsleepTime += Time.deltaTime;
			}
			return false;
		}
		_yaw = num;
		_pitch = num2;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldIgnoreTarget(Entity _target)
	{
		if (Vector3.Dot(_target.position - TileEntity.ToWorldPos().ToVector3(), Cone.transform.forward) > 0f)
		{
			return true;
		}
		if (!_target.IsAlive())
		{
			return true;
		}
		if (_target is EntityVehicle)
		{
			Entity attachedMainEntity = (_target as EntityVehicle).AttachedMainEntity;
			if (attachedMainEntity == null)
			{
				return true;
			}
			_target = attachedMainEntity;
		}
		if (_target is EntityPlayer)
		{
			bool flag = false;
			bool flag2 = false;
			PersistentPlayerList persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
			if (persistentPlayerList != null && persistentPlayerList.EntityToPlayerMap.ContainsKey(_target.entityId) && TileEntity.IsOwner(persistentPlayerList.EntityToPlayerMap[_target.entityId].PrimaryId))
			{
				flag = true;
			}
			if (!flag)
			{
				PersistentPlayerData playerData = persistentPlayerList.GetPlayerData(TileEntity.GetOwner());
				if (playerData != null && persistentPlayerList.EntityToPlayerMap.ContainsKey(_target.entityId))
				{
					PersistentPlayerData persistentPlayerData = persistentPlayerList.EntityToPlayerMap[_target.entityId];
					if (playerData.ACL != null && persistentPlayerData != null && playerData.ACL.Contains(persistentPlayerData.PrimaryId))
					{
						flag2 = true;
					}
				}
			}
			if (flag && !TileEntity.TargetSelf)
			{
				return true;
			}
			if (flag2 && !TileEntity.TargetAllies)
			{
				return true;
			}
			if (!flag && !flag2 && !TileEntity.TargetStrangers)
			{
				return true;
			}
		}
		if (_target is EntityNPC)
		{
			if (!TileEntity.TargetStrangers)
			{
				return true;
			}
			if (_target is EntityDrone)
			{
				return true;
			}
		}
		if (_target is EntityEnemy && !TileEntity.TargetZombies)
		{
			return true;
		}
		if (_target is EntityAnimal && !_target.EntityClass.bIsEnemyEntity)
		{
			return true;
		}
		return false;
	}

	public void SetPitch(float pitch)
	{
		TileEntity.CenteredPitch = pitch;
	}

	public void SetYaw(float yaw)
	{
		TileEntity.CenteredYaw = yaw;
	}

	public float GetPitch()
	{
		return TileEntity.CenteredPitch;
	}

	public float GetYaw()
	{
		return TileEntity.CenteredYaw;
	}

	public Transform GetCameraTransform()
	{
		return Cone;
	}

	public void SetUserAccessing(bool userAccessing)
	{
		IsUserAccessing = userAccessing;
	}

	public void Cleanup()
	{
		if (Cone != null && WireManager.HasInstance)
		{
			WireManager.Instance.RemovePulseObject(Cone.gameObject);
		}
	}

	public void SetConeColor(Color _color)
	{
		if (ConeMaterial != null)
		{
			ConeMaterial.SetColor("_Color", _color);
		}
	}

	public Color GetOriginalConeColor()
	{
		return ConeColor;
	}

	public void SetConeActive(bool _active)
	{
		if (Cone != null)
		{
			Cone.gameObject.SetActive(_active);
		}
	}

	public bool GetConeActive()
	{
		if (Cone != null)
		{
			return Cone.gameObject.activeSelf;
		}
		return false;
	}

	public bool HasCone()
	{
		return Cone != null;
	}

	public bool HasLaser()
	{
		return false;
	}

	public void SetLaserColor(Color _color)
	{
	}

	public Color GetOriginalLaserColor()
	{
		return Color.black;
	}

	public void SetLaserActive(bool _active)
	{
	}

	public bool GetLaserActive()
	{
		return false;
	}
}
