using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct PlayerStealth
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct NoiseData(float _volume, int _ticks)
	{
		public float volume = _volume;

		public int ticks = _ticks;
	}

	public const float cLightLevelMax = 200f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cLightMpyBase = 0.32f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cVersion = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cNextSoundPercent = 0.6f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSleeperNoiseDecay = 50f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSleeperNoiseHear = 360f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSleeperNoiseWaitTicks = 20;

	public float lightLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lightAttackPercent;

	public float noiseVolume;

	public int smell;

	[PublicizedFrom(EAccessModifier.Private)]
	public float speedAverage;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer player;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sendTickDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lightLevelSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public int noiseVolumeSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool alertEnemySent;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sleeperNoiseWaitTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public float sleeperNoiseVolume;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<NoiseData> noises;

	[PublicizedFrom(EAccessModifier.Private)]
	public int alertEnemiesTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool alertEnemy;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 barColorUI;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Entity> entityTempList = new List<Entity>();

	public Color32 ValueColorUI => barColorUI;

	public float ValuePercentUI => Utils.FastClamp01((lightLevel + noiseVolume * 0.5f + (float)(alertEnemy ? 5 : 0)) * 0.01f + 0.005f);

	public void Init(EntityPlayer _player)
	{
		player = _player;
		noises = new List<NoiseData>();
		barColorUI = new Color32(0, 0, 0, byte.MaxValue);
	}

	public void Tick()
	{
		float num = Utils.FastAbs(player.speedForward) + Utils.FastAbs(player.speedStrafe);
		if (num > 0.1f)
		{
			speedAverage = Utils.FastLerpUnclamped(speedAverage, num, 0.2f);
		}
		else
		{
			speedAverage *= 0.5f;
		}
		float stealthLightLevel = LightManager.GetStealthLightLevel(player, out var selfLight);
		float v = selfLight / (stealthLightLevel + 0.05f);
		v = Utils.FastClamp(v, 0.5f, 3.2f);
		stealthLightLevel += selfLight * v;
		if (player.IsCrouching)
		{
			stealthLightLevel *= 0.6f;
		}
		player.Buffs.SetCustomVar("_lightlevel", stealthLightLevel * 100f);
		stealthLightLevel *= 1f + speedAverage * 0.15f;
		float value = EffectManager.GetValue(PassiveEffects.LightMultiplier, null, 1f, player);
		lightAttackPercent = ((selfLight < 0.1f) ? value : 1f);
		value = 0.32f + 0.68f * value;
		float v2 = stealthLightLevel * value * 100f;
		lightLevel = Utils.FastClamp(v2, 0f, 200f);
		ProcNoiseCleanup();
		float num2 = CalcVolume();
		player.Buffs.SetCustomVar("_noiselevel", noiseVolume);
		if (--sleeperNoiseWaitTicks <= 0)
		{
			sleeperNoiseVolume -= 2.5f;
			if (sleeperNoiseVolume < 0f)
			{
				sleeperNoiseVolume = 0f;
			}
		}
		if (num2 > 0f)
		{
			float num3 = num2 * 1.2f;
			float num4 = EAIManager.CalcSenseScale();
			num3 *= 1f + num4 * 1.6f;
			float num5 = 75f + 25f * num4;
			if (num3 > num5)
			{
				num3 = num5;
			}
			Bounds bb = new Bounds(player.position, new Vector3(num3, num3, num3));
			player.world.GetEntitiesInBounds(typeof(EntityEnemy), bb, entityTempList);
			for (int i = 0; i < entityTempList.Count; i++)
			{
				EntityAlive entityAlive = (EntityAlive)entityTempList[i];
				float distance = player.GetDistance(entityAlive);
				float num6 = noiseVolume * (1f + num4 * entityAlive.aiManager.feralSense);
				num6 /= distance * 0.6f + 0.4f;
				num6 *= player.DetectUsScale(entityAlive);
				if (num6 >= 1f)
				{
					bool flag = true;
					if ((bool)entityAlive.noisePlayer)
					{
						flag = num6 > entityAlive.noisePlayerVolume;
					}
					if (flag)
					{
						entityAlive.noisePlayer = player;
						entityAlive.noisePlayerDistance = distance;
						entityAlive.noisePlayerVolume = num6;
					}
				}
			}
			entityTempList.Clear();
		}
		if (--alertEnemiesTicks <= 0)
		{
			alertEnemiesTicks = 20;
			alertEnemy = false;
			player.world.GetEntitiesAround(EntityFlags.Zombie | EntityFlags.Animal | EntityFlags.Bandit, player.position, 12f, entityTempList);
			for (int j = 0; j < entityTempList.Count; j++)
			{
				if (((EntityAlive)entityTempList[j]).IsAlert)
				{
					alertEnemy = true;
					break;
				}
			}
			entityTempList.Clear();
			SetBarColor(alertEnemy);
		}
		if (player.isEntityRemote)
		{
			if (sendTickDelay > 0)
			{
				sendTickDelay--;
			}
			if ((player.IsCrouching && sendTickDelay == 0 && (lightLevelSent != (int)lightLevel || noiseVolumeSent != (int)noiseVolume)) || alertEnemySent != alertEnemy)
			{
				sendTickDelay = 16;
				lightLevelSent = (int)lightLevel;
				noiseVolumeSent = (int)noiseVolume;
				alertEnemySent = alertEnemy;
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityStealth>().Setup(player, lightLevelSent, noiseVolumeSent, alertEnemySent), _onlyClientsAttachedToAnEntity: false, player.entityId);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetBarColor(bool _isAlert)
	{
		barColorUI.r = 50;
		barColorUI.g = 135;
		if (_isAlert)
		{
			barColorUI.r = 180;
			barColorUI.g = 180;
		}
	}

	public void ProcNoiseCleanup()
	{
		for (int i = 0; i < noises.Count; i++)
		{
			NoiseData value = noises[i];
			if (value.ticks > 1)
			{
				value.ticks--;
				noises[i] = value;
			}
			else
			{
				noises.RemoveAt(i);
				i--;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalcVolume()
	{
		float num = 0f;
		float num2 = 1f;
		for (int i = 0; i < noises.Count; i++)
		{
			num += noises[i].volume * num2;
			num2 *= 0.6f;
		}
		noiseVolume = Mathf.Pow(num * 2.35f, 0.86f);
		noiseVolume *= 1.5f;
		noiseVolume *= EffectManager.GetValue(PassiveEffects.NoiseMultiplier, null, 1f, player);
		return num;
	}

	public bool CanSleeperAttackDetect(EntityAlive _e)
	{
		if (player.IsCrouching)
		{
			float num = Utils.FastLerp(3f, 15f, lightAttackPercent);
			if (_e.GetDistance(player) > num)
			{
				return false;
			}
		}
		return true;
	}

	public void SetClientLevels(float _lightLevel, float _noiseVolume, bool _isAlert)
	{
		lightLevel = _lightLevel;
		noiseVolume = _noiseVolume;
		alertEnemy = _isAlert;
		SetBarColor(_isAlert);
	}

	public bool NotifyNoise(float volume, float duration)
	{
		if (volume <= 0f)
		{
			return false;
		}
		AddNoise(noises, volume, (int)(duration * 20f));
		if (volume >= 11f)
		{
			sleeperNoiseWaitTicks = 20;
		}
		float num = volume;
		if (volume > 60f)
		{
			num = 60f + Mathf.Pow(volume - 60f, 1.4f);
		}
		num *= EffectManager.GetValue(PassiveEffects.NoiseMultiplier, null, 1f, player);
		sleeperNoiseVolume += num;
		if (sleeperNoiseVolume >= 360f)
		{
			sleeperNoiseVolume = 360f;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddNoise(List<NoiseData> list, float volume, int ticks)
	{
		NoiseData item = new NoiseData(volume, ticks);
		for (int i = 0; i < list.Count; i++)
		{
			if (volume >= noises[i].volume)
			{
				list.Insert(i, item);
				return;
			}
		}
		list.Insert(list.Count, item);
	}

	public static PlayerStealth Read(EntityPlayer _player, BinaryReader br)
	{
		int num = br.ReadInt32();
		PlayerStealth result = default(PlayerStealth);
		result.Init(_player);
		result.lightLevel = br.ReadInt32();
		int num2 = br.ReadInt32();
		if (num2 > 0)
		{
			if (num >= 3)
			{
				for (int i = 0; i < num2; i++)
				{
					br.ReadSingle();
					float volume = br.ReadSingle();
					int ticks = br.ReadInt32();
					result.AddNoise(result.noises, volume, ticks);
				}
			}
			else if (num >= 2)
			{
				for (int j = 0; j < num2; j++)
				{
					br.ReadSingle();
					br.ReadSingle();
					br.ReadInt32();
				}
			}
			else
			{
				for (int k = 0; k < num2; k++)
				{
					br.ReadInt32();
					br.ReadInt32();
				}
			}
		}
		return result;
	}

	public void Write(BinaryWriter bw)
	{
		bw.Write(3);
		bw.Write(lightLevel);
		bw.Write((noises != null) ? noises.Count : 0);
		if (noises != null)
		{
			for (int i = 0; i < noises.Count; i++)
			{
				NoiseData noiseData = noises[i];
				bw.Write(0f);
				bw.Write(noiseData.volume);
				bw.Write(noiseData.ticks);
			}
		}
	}
}
