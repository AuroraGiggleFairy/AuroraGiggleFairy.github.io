using System;
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

	[PublicizedFrom(EAccessModifier.Private)]
	public float speedAverage;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer player;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal playerLocal;

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
	public static readonly List<Entity> entityTempList = new List<Entity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSmellStartDelay = 11;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSmellUpdateItemsTicksMax = 40;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSmellCountMin = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSmellCountMax = 50f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSmellRadiusMin = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSmellRadiusMax = 100f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSmellRadiusPerSecondUp = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSmellRadiusPerSecondDown = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSmellEatRadiusPerSecondDown = 1f / 7f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSmellShelterRadiusPerSecondDown = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSmellBleedRadius = 25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSmellDysenteryRadius = 35f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSmellEmitRate = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSmellEmitChance = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSmellDuration = 90;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSmellWetClear = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSmellWetRateMin = 0.01f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int smellUpdateItemsTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int smellUpdateCVarTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int smellEmitTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public float smellRadiusTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public float smellRadius;

	[PublicizedFrom(EAccessModifier.Private)]
	public float smellCountRadius;

	[PublicizedFrom(EAccessModifier.Private)]
	public float smellEatRadius;

	[PublicizedFrom(EAccessModifier.Private)]
	public int smellEatTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool smellSheltered;

	[PublicizedFrom(EAccessModifier.Private)]
	public float smellWet;

	[PublicizedFrom(EAccessModifier.Private)]
	public float smellWetRate;

	[PublicizedFrom(EAccessModifier.Private)]
	public int smellRadiusTargetSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool smellShelteredSent;

	public Color32 ValueColorUI => barColorUI;

	public float ValuePercentUI => Utils.FastClamp01((lightLevel + noiseVolume * 0.5f + smellRadius / 100f * 50f + (float)(alertEnemy ? 5 : 0)) * 0.01f + 0.005f);

	public void Init(EntityPlayer _player)
	{
		player = _player;
		playerLocal = _player as EntityPlayerLocal;
		noises = new List<NoiseData>();
		barColorUI = new Color32(0, 0, 0, byte.MaxValue);
	}

	public void TickServer()
	{
		float num = player.speedForward * player.speedForward + player.speedStrafe * player.speedStrafe;
		if (num > 0.01f)
		{
			speedAverage = Utils.FastLerpUnclamped(speedAverage, (float)Math.Sqrt(num), 0.2f);
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
		NoiseCleanup();
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
			float num3 = num2 * 0.6f;
			float num4 = EAIManager.CalcSenseScale();
			num3 *= 1f + num4 * 1.6f;
			num3 = Utils.FastMin(num3, 40f + 15f * num4);
			player.world.GetEntitiesAround(EntityFlags.AIHearing, player.position, num3, entityTempList);
			for (int i = 0; i < entityTempList.Count; i++)
			{
				EntityAlive entityAlive = (EntityAlive)entityTempList[i];
				float distance = player.GetDistance(entityAlive);
				float num5 = noiseVolume * (1f + num4 * entityAlive.aiManager.feralSense);
				if (entityAlive is EntityHuman entityHuman && entityHuman.IsStormEffected())
				{
					num5 *= 2f;
				}
				num5 /= distance * 0.6f + 0.4f;
				num5 *= player.DetectUsScale(entityAlive);
				if (num5 >= 1f)
				{
					bool flag = true;
					if ((bool)entityAlive.noisePlayer)
					{
						flag = num5 > entityAlive.noisePlayerVolume;
					}
					if (flag)
					{
						entityAlive.noisePlayer = player;
						entityAlive.noisePlayerDistance = distance;
						entityAlive.noisePlayerVolume = num5;
					}
				}
			}
			entityTempList.Clear();
		}
		if (GamePrefs.GetInt(EnumGamePrefs.AISmellMode) != 0)
		{
			SmellTickServer();
		}
		if (--alertEnemiesTicks <= 0)
		{
			alertEnemiesTicks = 20;
			alertEnemy = false;
			player.world.GetEntitiesAround(EntityFlags.AIHearing, player.position, 12f, entityTempList);
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

	public void TickLocalClient()
	{
		if (GamePrefs.GetInt(EnumGamePrefs.AISmellMode) != 0)
		{
			SmellTickClient();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SmellTickServer()
	{
		if (player.IsSpectator || player.IsGodMode.Value)
		{
			SmellClear();
			player.Buffs.SetCustomVar("smell", 0f);
			return;
		}
		SmellTickWet();
		if (--smellUpdateItemsTicks <= 0)
		{
			smellUpdateItemsTicks = 40;
			if (!player.isEntityRemote)
			{
				SmellUpdateItemsAndBlood();
			}
		}
		if (smellRadius < 1f)
		{
			if (smellRadiusTarget < 1f)
			{
				smellRadius = 0f;
			}
			else
			{
				smellRadius = Utils.FastMoveTowards(smellRadius, smellRadiusTarget, 0.0045454544f);
				if (smellRadius >= 1f)
				{
					smellRadius = 10f;
					smellUpdateCVarTicks = 0;
				}
			}
		}
		else
		{
			float num = 5f;
			if (smellRadiusTarget < smellRadius)
			{
				num = ((!smellSheltered) ? 2f : 10f);
			}
			smellRadius = Utils.FastMoveTowards(smellRadius, smellRadiusTarget, num * 0.05f);
			if (smellRadius < 1f)
			{
				smellRadius = 0f;
				smellUpdateCVarTicks = 0;
			}
		}
		if (--smellUpdateCVarTicks <= 0)
		{
			smellUpdateCVarTicks = 20;
			float value = ((smellRadius < 5f) ? smellRadius : ((float)(int)(smellRadius + 0.5f)));
			player.Buffs.SetCustomVar("smell", value);
			player.Buffs.GetBuff("buffSmellCheck")?.DurationTriggerUpdate();
		}
		if (smellRadius >= 5f && --smellEmitTicks <= 0)
		{
			smellEmitTicks = 40;
			float num2 = EAIManager.CalcSenseScale();
			float radius = smellRadius * (1f + num2);
			player.world.GetEntitiesAround(EntityFlags.AISmelling, player.position, radius, entityTempList);
			for (int i = 0; i < entityTempList.Count; i++)
			{
				if (!(player.rand.RandomFloat < 0.2f))
				{
					continue;
				}
				EntityAlive entityAlive = (EntityAlive)entityTempList[i];
				if (player.DetectUsScale(entityAlive) >= 1f)
				{
					float distance = player.GetDistance(entityAlive);
					bool flag = true;
					if ((bool)entityAlive.smellPlayer && entityAlive.smellPlayer != player)
					{
						flag = distance > entityAlive.smellPlayerDistance;
					}
					if (flag)
					{
						entityAlive.smellPlayer = player;
						entityAlive.smellPlayerDistance = distance;
						entityAlive.smellPlayerTimeoutTicks = 240;
					}
				}
			}
			entityTempList.Clear();
		}
		SmellTickEat();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SmellTickClient()
	{
		SmellTickWet();
		if (--smellUpdateItemsTicks <= 0)
		{
			smellUpdateItemsTicks = 40;
			SmellUpdateItemsAndBlood();
			int num = (int)smellRadiusTarget;
			if (num != smellRadiusTargetSent || smellSheltered != smellShelteredSent)
			{
				smellRadiusTargetSent = num;
				smellShelteredSent = smellSheltered;
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityStealth>().Setup(playerLocal, num, smellEatRadius > 0f, smellSheltered));
			}
		}
		SmellTickEat();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SmellTickWet()
	{
		smellWetRate = player.Buffs.GetCustomVar("_wetnessrate");
		if (smellWetRate >= 0.01f)
		{
			smellWet += smellWetRate;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SmellTickEat()
	{
		if (smellEatTicks > 0)
		{
			if (smellEatTicks <= 1640)
			{
				smellEatRadius -= 0.0071428576f;
			}
			if (--smellEatTicks <= 0 || smellEatRadius < 1f)
			{
				smellEatRadius = 0f;
				smellUpdateItemsTicks = 0;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SmellUpdateItemsAndBlood()
	{
		if (smellWet >= 3f || player.IsDead())
		{
			SmellClear();
			return;
		}
		if (player.Buffs.GetCustomVar(".dysenterySmell") > 0f)
		{
			player.Buffs.RemoveCustomVar(".dysenterySmell");
			SetSmellEat(35f);
		}
		int num = SmellCountItems();
		if (smellWetRate >= 0.01f)
		{
			num = 0;
		}
		smellCountRadius = SmellCountToRadius(num);
		smellRadiusTarget = Utils.FastMax(smellCountRadius, smellEatRadius);
		smellSheltered = false;
		if (playerLocal.shelterPercent > 0f)
		{
			smellRadiusTarget *= 0.2f;
			smellSheltered = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SmellClear()
	{
		smellEatRadius = 0f;
		smellEatTicks = 0;
		smellRadius = 0f;
		smellRadiusTarget = 0f;
		smellWet = 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int SmellCountItems()
	{
		float num = 0f;
		ItemStack currentStack = playerLocal.PlayerUI.xui.dragAndDrop.CurrentStack;
		if (!currentStack.IsEmpty())
		{
			num += currentStack.itemValue.ItemClass.Smell * (float)currentStack.count;
		}
		Inventory inventory = player.inventory;
		int slotCount = inventory.GetSlotCount();
		for (int i = 0; i < slotCount; i++)
		{
			ItemStack itemStack = inventory.GetItemStack(i);
			if (itemStack != null && itemStack.count > 0)
			{
				num += itemStack.itemValue.ItemClass.Smell * (float)itemStack.count;
			}
		}
		ItemStack[] slots = player.bag.GetSlots();
		foreach (ItemStack itemStack2 in slots)
		{
			if (itemStack2 != null && itemStack2.count > 0)
			{
				ItemClass itemClass = itemStack2.itemValue.ItemClass;
				if (itemClass != null)
				{
					num += itemClass.Smell * (float)itemStack2.count;
				}
			}
		}
		num = Utils.FastMin(num, 50f);
		return (int)num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float SmellCountToRadius(float _count)
	{
		_count -= 5f;
		float result = 0f;
		if (_count >= 0f)
		{
			result = Utils.FastLerp(10f, 100f, _count / 45f);
		}
		return result;
	}

	public void SetSmellRadiusTarget(int _radius, bool _eating, bool _sheltered)
	{
		smellRadiusTarget = _radius;
		if (_eating)
		{
			smellRadius = Utils.FastMax(smellRadius, 1f);
		}
		smellSheltered = _sheltered;
	}

	public void SetSmellEat(float _distance)
	{
		smellEatRadius = Utils.FastMin(smellEatRadius + _distance, 100f);
		smellEatTicks = 1800;
		smellRadius = Utils.FastMax(smellRadius, 1f);
		smellUpdateItemsTicks = 0;
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void NoiseCleanup()
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
					if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						result.AddNoise(result.noises, volume, ticks);
					}
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
