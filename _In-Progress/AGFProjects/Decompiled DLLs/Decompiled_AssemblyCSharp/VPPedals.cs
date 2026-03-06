using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class VPPedals : VehiclePart
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform crankT;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform[] pedalTs = new Transform[2];

	[PublicizedFrom(EAccessModifier.Private)]
	public float rot;

	[PublicizedFrom(EAccessModifier.Private)]
	public float rotSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	public float backPedalTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public string pedalSoundName;

	[PublicizedFrom(EAccessModifier.Private)]
	public float pedalSoundTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool didPedal;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool didRun;

	[PublicizedFrom(EAccessModifier.Private)]
	public float staminaCheckTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float staminaDrain;

	[PublicizedFrom(EAccessModifier.Private)]
	public float staminaDrainTurbo;

	[PublicizedFrom(EAccessModifier.Private)]
	public float staminaCooldownDelay;

	public override void InitPrefabConnections()
	{
		initPedal("L", 0);
		initPedal("R", 1);
		ParticleEffectUpdate();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initPedal(string name, int index)
	{
		crankT = GetTransform();
		Transform transform = crankT.Find("Pedal" + name);
		pedalTs[index] = transform;
		InitIKTarget((AvatarIKGoal)(0 + index), transform);
	}

	public override void SetProperties(DynamicProperties _properties)
	{
		base.SetProperties(_properties);
		_properties.ParseString("pedalSound", ref pedalSoundName);
		_properties.ParseVec("staminaDrain", ref staminaDrain, ref staminaDrainTurbo);
	}

	public override void Update(float deltaTime)
	{
		if (!vehicle.entity.HasDriver)
		{
			return;
		}
		EntityAlive entityAlive = vehicle.entity.AttachedMainEntity as EntityAlive;
		if (!entityAlive)
		{
			return;
		}
		float currentMotorTorquePercent = vehicle.CurrentMotorTorquePercent;
		float currentForwardVelocity = vehicle.CurrentForwardVelocity;
		if (currentMotorTorquePercent > 0f)
		{
			if (currentForwardVelocity > 0f)
			{
				rotSpeed += deltaTime * 10f * currentForwardVelocity;
				didPedal = true;
				didRun |= vehicle.IsTurbo;
			}
			backPedalTime = 0f;
		}
		else if (Random.value < 0.3f * deltaTime)
		{
			backPedalTime = Random.value * 1.2f;
		}
		entityAlive.CurrentMovementTag = (didRun ? EntityAlive.MovementTagRunning : EntityAlive.MovementTagIdle);
		staminaCheckTime += deltaTime;
		if (staminaCheckTime >= 0.2f)
		{
			staminaCheckTime = 0f;
			if (didPedal)
			{
				float num = (didRun ? staminaDrainTurbo : staminaDrain);
				entityAlive.AddStamina((0f - num) * 0.2f);
				didPedal = false;
				didRun = false;
			}
		}
		if (currentForwardVelocity != 0f && backPedalTime > 0f)
		{
			backPedalTime -= deltaTime;
			rotSpeed += -15f * deltaTime;
		}
		rotSpeed *= 0.8f;
		if (Mathf.Abs(rotSpeed) > 0.1f)
		{
			rot += rotSpeed;
			crankT.localEulerAngles = new Vector3(rot, 0f, 0f);
			Quaternion localRotation = Quaternion.Inverse(crankT.localRotation);
			for (int i = 0; i < pedalTs.Length; i++)
			{
				pedalTs[i].localRotation = localRotation;
			}
			if (rotSpeed > 1f)
			{
				pedalSoundTime += deltaTime;
				float num2 = (vehicle.IsTurbo ? 0.55f : 0.75f);
				if (pedalSoundTime > num2)
				{
					playSound(pedalSoundName);
					pedalSoundTime = 0f;
				}
			}
		}
		if (entityAlive.Stamina < 1f)
		{
			staminaCooldownDelay = 2f;
		}
		if (staminaCooldownDelay > 0f)
		{
			staminaCooldownDelay -= deltaTime;
			vehicle.CanTurbo = false;
		}
		else
		{
			vehicle.CanTurbo = true;
		}
	}

	public override void HandleEvent(Vehicle.Event _event, float _arg)
	{
		if (_event == Vehicle.Event.Start || _event == Vehicle.Event.Stop || _event == Vehicle.Event.HealthChanged)
		{
			ParticleEffectUpdate();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ParticleEffectUpdate()
	{
		float healthPercentage = GetHealthPercentage();
		if (healthPercentage <= 0f)
		{
			SetTransformActive("chain", _active: false);
			SetTransformActive("particleDamaged", _active: true);
			SetTransformActive("particleBroken", _active: true);
			return;
		}
		SetTransformActive("chain", _active: true);
		SetTransformActive("particleBroken", _active: false);
		if (healthPercentage <= 0.25f)
		{
			SetTransformActive("particleDamaged", vehicle.entity.HasDriver);
		}
		else
		{
			SetTransformActive("particleDamaged", _active: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playSound(string _sound)
	{
		if (vehicle.entity != null && !vehicle.entity.isEntityRemote)
		{
			vehicle.entity.PlayOneShot(_sound);
		}
	}
}
