using System;
using Audio;
using UnityEngine;

public class SpinningBladeTrapController : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum BladeTrapStates
	{
		IsOff,
		RandomWaitToStart,
		IsStarting,
		IsOn,
		IsOnPartlyBroken,
		IsOnBroken,
		IsStopping
	}

	public Transform BladeControllerTransform;

	public Transform BladeBottomTransform;

	public SpinningBladeTrapBladeController BladeController;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastHealthRatio;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float healthRatio = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastIsOn;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOn;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float degreesPerSecondMax = 720f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float degreesPerSecond;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float windUpTimeMax = 5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float windDownTimeMax = 7.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float windUpDownTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float randomStartDelayMax = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float randomStartDelay;

	public Vector3i BlockPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk chunk;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string startSound = "Electricity/BladeTrap/bladetrap_startup";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string stopSound = "Electricity/BladeTrap/bladetrap_stop";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string runningSound = "Electricity/BladeTrap/bladetrap_fire_lp";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string runningSoundPartlyBroken = "Electricity/BladeTrap/bladetrap_dm1_lp";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string runningSoundBroken = "Electricity/BladeTrap/bladetrap_dm2_lp";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool initialized;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float brokenPercentage;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float breakingPercentage;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public BladeTrapStates currentState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float totalDamage;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string currentRunningSound;

	public float HealthRatio
	{
		get
		{
			return healthRatio;
		}
		set
		{
			lastHealthRatio = healthRatio;
			healthRatio = value;
			if (healthRatio != lastHealthRatio)
			{
				CheckHealthChanged();
			}
		}
	}

	public float CurrentSpeedRatio => windUpDownTime / windUpTimeMax * (windUpDownTime / windUpTimeMax);

	public bool IsOn
	{
		get
		{
			return isOn;
		}
		set
		{
			lastIsOn = isOn;
			isOn = value;
			BladeController.IsOn = value;
		}
	}

	public BladeTrapStates CurrentState
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return currentState;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			EnterState(currentState, value);
			currentState = value;
		}
	}

	public void Init(DynamicProperties _properties, Block _block)
	{
		if (!initialized)
		{
			initialized = true;
			breakingPercentage = 0.5f;
			if (_properties.Values.ContainsKey("BreakingPercentage"))
			{
				breakingPercentage = Mathf.Clamp01(StringParsers.ParseFloat(_properties.Values["BreakingPercentage"]));
			}
			brokenPercentage = 0.25f;
			if (_properties.Values.ContainsKey("BrokenPercentage"))
			{
				brokenPercentage = Mathf.Clamp01(StringParsers.ParseFloat(_properties.Values["BrokenPercentage"]));
			}
			if (_properties.Values.ContainsKey("StartSound"))
			{
				startSound = _properties.Values["StartSound"];
			}
			if (_properties.Values.ContainsKey("StopSound"))
			{
				stopSound = _properties.Values["StopSound"];
			}
			if (_properties.Values.ContainsKey("RunningSound"))
			{
				runningSound = _properties.Values["RunningSound"];
			}
			if (_properties.Values.ContainsKey("RunningSoundBreaking"))
			{
				runningSoundPartlyBroken = _properties.Values["RunningSoundBreaking"];
			}
			if (_properties.Values.ContainsKey("RunningSoundBroken"))
			{
				runningSoundBroken = _properties.Values["RunningSoundBroken"];
			}
			if (BladeController != null)
			{
				BladeController.Init(_properties, _block);
			}
			randomStartDelayMax = GameManager.Instance.World.GetGameRandom().RandomFloat * randomStartDelayMax;
		}
	}

	public void DamageSelf(float damage)
	{
		totalDamage += damage;
		if (!(totalDamage < 1f))
		{
			damage = (int)totalDamage;
			totalDamage = 0f;
			if (chunk == null)
			{
				chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(BlockPosition);
			}
			BlockValue block = GameManager.Instance.World.GetBlock(BlockPosition);
			HealthRatio = 1f - (float)block.damage / (float)block.Block.MaxDamage;
			block.damage = Mathf.Clamp(block.damage + (int)damage, 0, block.Block.MaxDamage);
			GameManager.Instance.World.SetBlock(chunk.ClrIdx, BlockPosition, block, bNotify: false, updateLight: false);
		}
	}

	public void StopAllSounds()
	{
		Manager.BroadcastStop(BlockPosition.ToVector3(), runningSound);
		Manager.BroadcastStop(BlockPosition.ToVector3(), runningSoundPartlyBroken);
		Manager.BroadcastStop(BlockPosition.ToVector3(), runningSoundBroken);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnterState(BladeTrapStates oldState, BladeTrapStates newState)
	{
		switch (newState)
		{
		case BladeTrapStates.RandomWaitToStart:
			randomStartDelay = 0f;
			break;
		case BladeTrapStates.IsStarting:
			StopAllSounds();
			Manager.BroadcastPlay(BlockPosition.ToVector3(), startSound);
			break;
		case BladeTrapStates.IsOn:
			Manager.BroadcastStop(BlockPosition.ToVector3(), runningSoundPartlyBroken);
			Manager.BroadcastStop(BlockPosition.ToVector3(), runningSoundBroken);
			Manager.BroadcastPlay(BlockPosition.ToVector3(), runningSound);
			degreesPerSecond = degreesPerSecondMax;
			break;
		case BladeTrapStates.IsOnPartlyBroken:
			Manager.BroadcastStop(BlockPosition.ToVector3(), runningSound);
			Manager.BroadcastStop(BlockPosition.ToVector3(), runningSoundBroken);
			Manager.BroadcastPlay(BlockPosition.ToVector3(), runningSoundPartlyBroken);
			degreesPerSecond = degreesPerSecondMax;
			break;
		case BladeTrapStates.IsOnBroken:
			Manager.BroadcastStop(BlockPosition.ToVector3(), runningSound);
			Manager.BroadcastStop(BlockPosition.ToVector3(), runningSoundPartlyBroken);
			Manager.BroadcastPlay(BlockPosition.ToVector3(), runningSoundBroken);
			degreesPerSecond = degreesPerSecondMax;
			break;
		case BladeTrapStates.IsStopping:
			StopAllSounds();
			Manager.BroadcastPlay(BlockPosition.ToVector3(), stopSound);
			windUpDownTime = windDownTimeMax;
			if (HealthRatio <= brokenPercentage)
			{
				HandleParticlesForBroken();
			}
			break;
		case BladeTrapStates.IsOff:
			StopAllSounds();
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		BlockValue block = GameManager.Instance.World.GetBlock(BlockPosition);
		if (block.isair)
		{
			return;
		}
		HealthRatio = 1f - (float)block.damage / (float)block.Block.MaxDamage;
		if (lastIsOn)
		{
			_ = !isOn;
		}
		else
			_ = 0;
		if (isOn)
		{
			_ = !lastIsOn;
		}
		else
			_ = 0;
		BladeTrapStates initialState = GetInitialState();
		if (initialState != currentState)
		{
			CurrentState = initialState;
			return;
		}
		switch (currentState)
		{
		case BladeTrapStates.IsOff:
			if (IsOn && HealthRatio >= brokenPercentage)
			{
				CurrentState = BladeTrapStates.RandomWaitToStart;
			}
			break;
		case BladeTrapStates.RandomWaitToStart:
			if (randomStartDelay < randomStartDelayMax)
			{
				randomStartDelay += Time.deltaTime;
				break;
			}
			windUpDownTime = 0f;
			CurrentState = BladeTrapStates.IsStarting;
			break;
		case BladeTrapStates.IsStarting:
			if (degreesPerSecond < degreesPerSecondMax)
			{
				if (HealthRatio > breakingPercentage)
				{
					degreesPerSecond = Mathf.Lerp(0f, degreesPerSecondMax, CurrentSpeedRatio);
				}
				else
				{
					degreesPerSecond = Mathf.Lerp(0f, degreesPerSecondMax * (Mathf.Clamp(HealthRatio, 0f, breakingPercentage) * 2f), CurrentSpeedRatio);
					if (HealthRatio <= brokenPercentage)
					{
						degreesPerSecond = 0f;
					}
				}
			}
			windUpDownTime += Time.deltaTime;
			windUpDownTime = Mathf.Clamp(windUpDownTime, 0f, windUpTimeMax);
			if (degreesPerSecond == degreesPerSecondMax)
			{
				CheckHealthChanged();
			}
			if (!isOn)
			{
				CurrentState = BladeTrapStates.IsStopping;
			}
			break;
		case BladeTrapStates.IsOn:
		case BladeTrapStates.IsOnPartlyBroken:
		case BladeTrapStates.IsOnBroken:
			if (degreesPerSecond < degreesPerSecondMax)
			{
				if (HealthRatio > breakingPercentage)
				{
					degreesPerSecond = Mathf.Lerp(0f, degreesPerSecondMax, CurrentSpeedRatio);
				}
				else
				{
					degreesPerSecond = Mathf.Lerp(0f, degreesPerSecondMax * (Mathf.Clamp(HealthRatio, 0f, breakingPercentage) * 2f), CurrentSpeedRatio);
					if (HealthRatio <= brokenPercentage)
					{
						degreesPerSecond = 0f;
					}
				}
			}
			if (!isOn)
			{
				CurrentState = BladeTrapStates.IsStopping;
			}
			break;
		case BladeTrapStates.IsStopping:
			if (degreesPerSecond > 0f)
			{
				degreesPerSecond = Mathf.Lerp(0f, degreesPerSecond, CurrentSpeedRatio);
			}
			windUpDownTime -= Time.deltaTime;
			windUpDownTime = Mathf.Clamp(windUpDownTime, 0f, windDownTimeMax);
			degreesPerSecond = Mathf.Lerp(0f, degreesPerSecond, CurrentSpeedRatio);
			if (windUpDownTime <= 0f)
			{
				CurrentState = BladeTrapStates.IsOff;
			}
			if (IsOn && HealthRatio > brokenPercentage)
			{
				CurrentState = BladeTrapStates.IsStarting;
			}
			break;
		}
		float y = BladeControllerTransform.localRotation.eulerAngles.y - degreesPerSecond * Time.deltaTime;
		float x = Utils.FastLerp(-15f, 0f, Utils.FastClamp(HealthRatio, 0f, breakingPercentage) * 2f);
		BladeControllerTransform.localRotation = Quaternion.Euler(x, y, 0f);
		BladeBottomTransform.localRotation = Quaternion.Euler(0f, y, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckHealthChanged()
	{
		if (CurrentState == BladeTrapStates.IsStarting || CurrentState == BladeTrapStates.IsOn || currentState == BladeTrapStates.IsOnPartlyBroken || currentState == BladeTrapStates.IsOnBroken)
		{
			BladeTrapStates stateByHealthRange = GetStateByHealthRange();
			if (stateByHealthRange != currentState)
			{
				CurrentState = stateByHealthRange;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BladeTrapStates GetInitialState()
	{
		if (isOn)
		{
			if (HealthRatio >= 0.75f)
			{
				return BladeTrapStates.IsOn;
			}
			if (HealthRatio >= breakingPercentage)
			{
				return BladeTrapStates.IsOnPartlyBroken;
			}
			if (HealthRatio > brokenPercentage)
			{
				return BladeTrapStates.IsOnBroken;
			}
		}
		return currentState;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BladeTrapStates GetStateByHealthRange()
	{
		if (isOn)
		{
			if (HealthRatio >= 0.75f)
			{
				return BladeTrapStates.IsOn;
			}
			if (HealthRatio >= breakingPercentage)
			{
				return BladeTrapStates.IsOnPartlyBroken;
			}
			if (HealthRatio > brokenPercentage)
			{
				return BladeTrapStates.IsOnBroken;
			}
			if (currentState == BladeTrapStates.IsOff)
			{
				return BladeTrapStates.IsOff;
			}
			return BladeTrapStates.IsStopping;
		}
		return currentState;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleParticlesForBroken()
	{
		float lightValue = GameManager.Instance.World.GetLightBrightness(World.worldToBlockPos(BlockPosition.ToVector3())) / 2f;
		ParticleEffect pe = new ParticleEffect("big_smoke", new Vector3(0f, 0.25f, 0f), lightValue, new Color(1f, 1f, 1f, 0.3f), null, base.transform, _OLDCreateColliders: false);
		GameManager.Instance.SpawnParticleEffectServer(pe, -1);
		ParticleEffect pe2 = new ParticleEffect("electric_fence_sparks", new Vector3(0f, 0.25f, 0f), lightValue, new Color(1f, 1f, 1f, 0.3f), "electric_fence_impact", base.transform, _OLDCreateColliders: false);
		GameManager.Instance.SpawnParticleEffectServer(pe2, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		Cleanup();
	}

	public void Cleanup()
	{
		StopAllSounds();
		IsOn = false;
		lastIsOn = false;
		currentState = BladeTrapStates.IsOff;
		degreesPerSecond = 0f;
		windUpDownTime = 0f;
		initialized = false;
	}
}
