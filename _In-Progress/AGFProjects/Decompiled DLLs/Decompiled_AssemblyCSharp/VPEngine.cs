using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class VPEngine : VehiclePart
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class Gear
	{
		public float rpmMin;

		public float rpmMax;

		public float rpmDecel;

		public float rpmAccel;

		public float rpmDownShiftPoint;

		public float rpmUpShiftPoint;

		public float rpmDownShiftTo;

		public float rpmUpShiftTo;

		public string accelSoundName;

		public string decelSoundName;

		public SoundRange[] soundRanges;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class SoundRange
	{
		public float pitchMin;

		public float pitchMax;

		public float volumeMin;

		public float volumeMax;

		public float pitchFadeMin;

		public float pitchFadeMax;

		public float pitchFadeRange;

		public string name;

		public Handle soundHandle;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cIdleFuelPercent = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTurboFuelPercent = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fuelKmPerL;

	[PublicizedFrom(EAccessModifier.Private)]
	public float foodDrain;

	[PublicizedFrom(EAccessModifier.Private)]
	public float foodDrainTurbo;

	public bool isRunning;

	[PublicizedFrom(EAccessModifier.Private)]
	public int acceleratePhase;

	[PublicizedFrom(EAccessModifier.Private)]
	public float rpm;

	[PublicizedFrom(EAccessModifier.Private)]
	public int gearIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDecelSoundPlayed;

	[PublicizedFrom(EAccessModifier.Private)]
	public string accelDecelSoundName;

	[PublicizedFrom(EAccessModifier.Private)]
	public float pitchRandTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float pitchRand;

	[PublicizedFrom(EAccessModifier.Private)]
	public float pitchAdd;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTurbo;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Gear> gears = new List<Gear>();

	public override void SetProperties(DynamicProperties _properties)
	{
		base.SetProperties(_properties);
		StringParsers.TryParseFloat(GetProperty("fuelKmPerL"), out fuelKmPerL);
		_properties.ParseVec("foodDrain", ref foodDrain, ref foodDrainTurbo);
		gears.Clear();
		for (int i = 1; i < 9; i++)
		{
			string property = GetProperty("gear" + i);
			if (property.Length == 0)
			{
				break;
			}
			string[] array = property.Split(',');
			Gear gear = new Gear();
			gears.Add(gear);
			int num = 0;
			StringParsers.TryParseFloat(array[num++], out gear.rpmMin);
			StringParsers.TryParseFloat(array[num++], out gear.rpmMax);
			StringParsers.TryParseFloat(array[num++], out gear.rpmDecel);
			StringParsers.TryParseFloat(array[num++], out gear.rpmDownShiftPoint);
			StringParsers.TryParseFloat(array[num++], out gear.rpmDownShiftTo);
			StringParsers.TryParseFloat(array[num++], out gear.rpmAccel);
			StringParsers.TryParseFloat(array[num++], out gear.rpmUpShiftPoint);
			StringParsers.TryParseFloat(array[num++], out gear.rpmUpShiftTo);
			gear.accelSoundName = array[num++].Trim();
			gear.decelSoundName = array[num++].Trim();
			int num2 = (array.Length - num) / 8;
			if (num2 > 0)
			{
				gear.soundRanges = new SoundRange[num2];
				for (int j = 0; j < num2; j++)
				{
					SoundRange soundRange = new SoundRange();
					gear.soundRanges[j] = soundRange;
					int num3 = num + j * 8;
					StringParsers.TryParseFloat(array[num3], out soundRange.pitchMin);
					StringParsers.TryParseFloat(array[num3 + 1], out soundRange.pitchMax);
					StringParsers.TryParseFloat(array[num3 + 2], out soundRange.volumeMin);
					StringParsers.TryParseFloat(array[num3 + 3], out soundRange.volumeMax);
					StringParsers.TryParseFloat(array[num3 + 4], out soundRange.pitchFadeMin);
					StringParsers.TryParseFloat(array[num3 + 5], out soundRange.pitchFadeMax);
					StringParsers.TryParseFloat(array[num3 + 6], out soundRange.pitchFadeRange);
					soundRange.pitchFadeRange += 1E-05f;
					soundRange.name = array[num3 + 7].Trim();
				}
			}
		}
	}

	public override void InitPrefabConnections()
	{
		ParticleEffectUpdate();
	}

	public override void Update(float _dt)
	{
		if (IsBroken())
		{
			stopEngine();
			return;
		}
		EntityAlive entityAlive = vehicle.entity.AttachedMainEntity as EntityAlive;
		if ((bool)entityAlive)
		{
			entityAlive.CurrentMovementTag = EntityAlive.MovementTagDriving;
			float value = 0f;
			if (vehicle.CurrentIsAccel)
			{
				value = foodDrain;
				if (vehicle.IsTurbo)
				{
					value = foodDrainTurbo;
				}
			}
			entityAlive.SetCVar("_vehicleFood", value);
		}
		if (isRunning)
		{
			float magnitude = vehicle.CurrentVelocity.magnitude;
			float num = _dt / (fuelKmPerL * 1000f);
			if (vehicle.IsTurbo)
			{
				num *= 2f;
			}
			num = ((!vehicle.CurrentIsAccel) ? (num * (vehicle.VelocityMaxForward * 0.1f)) : (num * magnitude));
			num *= vehicle.EffectFuelUsePer;
			vehicle.FireEvent(Event.FuelRemove, this, num);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ParticleEffectUpdate()
	{
		SetTransformActive("particleOn", isRunning);
		float healthPercentage = GetHealthPercentage();
		if (healthPercentage <= 0f)
		{
			SetTransformActive("particleDamaged", _active: true);
			SetTransformActive("particleBroken", _active: true);
			return;
		}
		SetTransformActive("particleBroken", _active: false);
		if (healthPercentage <= 0.25f)
		{
			SetTransformActive("particleDamaged", isRunning);
		}
		else
		{
			SetTransformActive("particleDamaged", _active: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateEngineSimulation()
	{
		if (!isRunning)
		{
			return;
		}
		float num = 500f;
		float num2 = 5000f;
		float num3 = -2400f;
		float num4 = 2700f;
		float num5 = 2700f;
		float num6 = 5000f;
		float num7 = 1500f;
		float num8 = 2800f;
		if (gears.Count > 0)
		{
			Gear gear = gears[gearIndex];
			num = gear.rpmMin;
			num2 = gear.rpmMax;
			num3 = gear.rpmDecel;
			num4 = gear.rpmDownShiftPoint;
			num5 = gear.rpmDownShiftTo;
			num8 = gear.rpmAccel;
			num6 = gear.rpmUpShiftPoint;
			num7 = gear.rpmUpShiftTo;
		}
		if (vehicle.CurrentIsAccel)
		{
			rpm += num8 * Time.deltaTime;
			rpm = Mathf.Min(rpm, num2);
			if (rpm >= num6 && gearIndex < gears.Count - 1 && vehicle.CurrentForwardVelocity > 4f)
			{
				gearIndex++;
				rpm = num7;
				vehicle.entity.AddRelativeForce(new Vector3(0f, 0.2f, -2f));
				Gear gear2 = gears[gearIndex];
				playAccelDecelSound(gear2.accelSoundName);
			}
			if (acceleratePhase <= 0)
			{
				if (gears.Count > 0)
				{
					Gear gear3 = gears[gearIndex];
					isDecelSoundPlayed = false;
					playAccelDecelSound(gear3.accelSoundName);
				}
				acceleratePhase = 1;
			}
			float rpmPercent = (rpm - num) / (num2 - num);
			updateEngineSounds(rpmPercent);
		}
		else if (acceleratePhase >= 0)
		{
			float num9 = num3;
			if (Mathf.Abs(vehicle.CurrentForwardVelocity) < 2f)
			{
				num9 *= 2f;
			}
			rpm += num9 * Time.deltaTime;
			if (rpm <= num4)
			{
				if (gears.Count > 0 && !isDecelSoundPlayed)
				{
					isDecelSoundPlayed = true;
					Gear gear4 = gears[gearIndex];
					playAccelDecelSound(gear4.decelSoundName);
				}
				if (gearIndex > 0)
				{
					acceleratePhase = 0;
					gearIndex = 0;
					if (num5 > 0f)
					{
						rpm = num5;
					}
				}
				else
				{
					acceleratePhase = -1;
					updateEngineSounds(0f);
				}
			}
			else
			{
				float rpmPercent2 = (rpm - num) / (num2 - num);
				updateEngineSounds(rpmPercent2);
			}
		}
		else
		{
			updateEngineSounds(0f);
		}
	}

	public override void HandleEvent(Vehicle.Event _event, float _arg)
	{
		switch (_event)
		{
		case Vehicle.Event.Start:
			if (!IsBroken())
			{
				startEngine();
			}
			break;
		case Vehicle.Event.Stop:
		{
			EntityAlive entityAlive = vehicle.entity.AttachedMainEntity as EntityAlive;
			if ((bool)entityAlive)
			{
				entityAlive.SetCVar("_vehicleFood", 0f);
			}
			stopEngine();
			break;
		}
		case Vehicle.Event.SimulationUpdate:
			if (!IsBroken())
			{
				updateEngineSimulation();
			}
			break;
		case Vehicle.Event.HealthChanged:
			ParticleEffectUpdate();
			break;
		case Vehicle.Event.Started:
		case Vehicle.Event.Stopped:
			break;
		}
	}

	public override void HandleEvent(Event _event, VehiclePart _part, float _arg)
	{
		if (_event == Event.FuelEmpty)
		{
			stopEngine(_outOfFuel: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startEngine()
	{
		if (!isRunning)
		{
			isRunning = true;
			if (vehicle.GetFuelLevel() > 0f)
			{
				playSound(properties.Values["sound_start"]);
				gearIndex = 0;
				updateEngineSounds(0f);
			}
			vehicle.entity.IsEngineRunning = true;
			vehicle.FireEvent(Vehicle.Event.Started);
			ParticleEffectUpdate();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stopEngine(bool _outOfFuel = false)
	{
		if (isRunning)
		{
			isRunning = false;
			stopEngineSounds();
			if (!_outOfFuel)
			{
				playSound(properties.Values["sound_shut_off"]);
			}
			else
			{
				playSound(properties.Values["sound_no_fuel_shut_off"]);
			}
			vehicle.entity.IsEngineRunning = false;
			vehicle.FireEvent(Vehicle.Event.Stopped);
			ParticleEffectUpdate();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playSound(string _sound)
	{
		if ((bool)vehicle.entity && !vehicle.entity.isEntityRemote)
		{
			vehicle.entity.PlayOneShot(_sound);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stopSound(string _sound)
	{
		if ((bool)vehicle.entity && !vehicle.entity.isEntityRemote)
		{
			vehicle.entity.StopOneShot(_sound);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void changeSoundLoop(string soundName, ref Handle handle)
	{
		stopSoundLoop(ref handle);
		playSoundLoop(soundName, ref handle);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playSoundLoop(string soundName, ref Handle handle)
	{
		if (handle == null && (bool)vehicle.entity)
		{
			handle = Manager.Play(vehicle.entity, soundName, 1f, wantHandle: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stopSoundLoop(ref Handle handle)
	{
		if (handle != null)
		{
			handle.Stop(vehicle.entity.entityId);
			handle = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playAccelDecelSound(string name)
	{
		if (accelDecelSoundName != null)
		{
			stopSound(accelDecelSoundName);
		}
		if (name != null && name.Length > 0)
		{
			playSound(name);
		}
		accelDecelSoundName = name;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateEngineSounds(float rpmPercent)
	{
		if (gears.Count <= 0)
		{
			return;
		}
		Gear gear;
		for (int i = 0; i < gears.Count; i++)
		{
			if (i == gearIndex)
			{
				continue;
			}
			gear = gears[i];
			for (int j = 0; j < gear.soundRanges.Length; j++)
			{
				SoundRange soundRange = gear.soundRanges[j];
				if (soundRange.soundHandle != null)
				{
					stopSoundLoop(ref soundRange.soundHandle);
				}
			}
		}
		float deltaTime = Time.deltaTime;
		pitchRandTime -= deltaTime;
		if (pitchRandTime <= 0f)
		{
			pitchRandTime = 0.75f;
			pitchRand = vehicle.entity.rand.RandomRange(-1f, 1f) * 0.03f;
		}
		float num = pitchRand;
		if (rpmPercent > 0f && vehicle.IsTurbo)
		{
			num += 0.2f;
			if (!isTurbo)
			{
				playSound("vehicle_turbo");
			}
		}
		isTurbo = rpmPercent > 0f && vehicle.IsTurbo;
		pitchAdd = Mathf.MoveTowards(pitchAdd, num, deltaTime * 0.15f);
		gear = gears[gearIndex];
		for (int k = 0; k < gear.soundRanges.Length; k++)
		{
			SoundRange soundRange2 = gear.soundRanges[k];
			float num2 = Mathf.Lerp(soundRange2.pitchMin, soundRange2.pitchMax, rpmPercent);
			float num3 = Mathf.Lerp(soundRange2.volumeMin, soundRange2.volumeMax, rpmPercent);
			float num4 = 1f;
			float num5 = soundRange2.pitchFadeMin - num2;
			if (num5 > 0f)
			{
				num4 = Mathf.Lerp(1f, 0f, num5 / soundRange2.pitchFadeRange);
			}
			else
			{
				float num6 = num2 - soundRange2.pitchFadeMax;
				if (num6 > 0f)
				{
					num4 = Mathf.Lerp(1f, 0f, num6 / soundRange2.pitchFadeRange);
				}
			}
			float num7 = num3 * num4;
			if (num7 < 0.01f)
			{
				if (soundRange2.soundHandle != null)
				{
					stopSoundLoop(ref soundRange2.soundHandle);
				}
				continue;
			}
			if (soundRange2.soundHandle == null)
			{
				playSoundLoop(soundRange2.name, ref soundRange2.soundHandle);
			}
			if (soundRange2.soundHandle != null)
			{
				soundRange2.soundHandle.SetPitch(num2 + pitchAdd);
				soundRange2.soundHandle.SetVolume(num7);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stopEngineSounds()
	{
		if (gears.Count > 0)
		{
			for (int i = 0; i < gears.Count; i++)
			{
				Gear gear = gears[i];
				for (int j = 0; j < gear.soundRanges.Length; j++)
				{
					SoundRange soundRange = gear.soundRanges[j];
					if (soundRange.soundHandle != null)
					{
						stopSoundLoop(ref soundRange.soundHandle);
					}
				}
			}
		}
		playAccelDecelSound(null);
	}
}
