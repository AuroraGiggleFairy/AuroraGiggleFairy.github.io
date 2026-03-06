using UnityEngine;

public class XUiM_Player : XUiModel
{
	public static int GetLevel(EntityPlayer player)
	{
		return player.Progression.GetLevel();
	}

	public static float GetLevelPercent(EntityPlayer player)
	{
		return player.Progression.GetLevelProgressPercentage();
	}

	public static int GetXPToNextLevel(EntityPlayer player)
	{
		return player.Progression.ExpToNextLevel;
	}

	public static float GetFood(EntityPlayer player)
	{
		return player.Stats.Food.Value;
	}

	public static float GetModifiedCurrentFood(EntityPlayer player)
	{
		return player.Stats.Food.Value + player.Buffs.GetCustomVar("$foodAmount");
	}

	public static float GetFoodPercent(EntityPlayer player)
	{
		return 1f - player.Stats.Food.Value / player.Stats.Food.ModifiedMax;
	}

	public static int GetFoodMax(EntityPlayer player)
	{
		return (int)player.Stats.Food.Max;
	}

	public static float GetWater(EntityPlayer player)
	{
		return player.Stats.Water.Value;
	}

	public static float GetModifiedCurrentWater(EntityPlayer player)
	{
		return player.Stats.Water.Value + player.Buffs.GetCustomVar("$waterAmount");
	}

	public static float GetWaterPercent(EntityPlayer player)
	{
		return player.Stats.Water.ValuePercentUI * 100f;
	}

	public static int GetWaterMax(EntityPlayer player)
	{
		return (int)player.Stats.Water.Max;
	}

	public static string GetCoreTemp(EntityPlayer player)
	{
		return ValueDisplayFormatters.Temperature(player.Buffs.GetCustomVar("_coretemp"));
	}

	public static string GetOutsideTemp(EntityPlayer player)
	{
		return ValueDisplayFormatters.Temperature(player.Buffs.GetCustomVar("_outsidetemp"));
	}

	public static int GetZombieKills(EntityPlayer player)
	{
		return player.KilledZombies;
	}

	public static int GetPlayerKills(EntityPlayer player)
	{
		return player.KilledPlayers;
	}

	public static int GetDeaths(EntityPlayer player)
	{
		return player.Died;
	}

	public static string GetKMTraveled(EntityPlayer player)
	{
		return (player.distanceWalked / 1000f).ToCultureInvariantString("0.00") + " KM";
	}

	public static int GetItemsCrafted(EntityPlayer player)
	{
		return (int)player.totalItemsCrafted;
	}

	public static string GetLongestLife(EntityPlayer player)
	{
		return XUiM_PlayerBuffs.GetTimeString((float)(int)player.longestLife * 60f);
	}

	public static string GetCurrentLife(EntityPlayer player)
	{
		return XUiM_PlayerBuffs.GetTimeString((float)(int)player.currentLife * 60f);
	}

	public static float GetHealth(EntityPlayer player)
	{
		return player.Stats.Health.Value;
	}

	public static float GetStamina(EntityPlayer player)
	{
		return player.Stats.Stamina.Value;
	}

	public static float GetMaxHealth(EntityPlayer player)
	{
		return player.Stats.Health.Max;
	}

	public static float GetMaxStamina(EntityPlayer player)
	{
		return player.Stats.Stamina.Max;
	}

	public static bool GetHasFullHealth(EntityPlayer player)
	{
		return player.Stats.Health.Max == player.Stats.Health.Value;
	}

	public static EntityPlayer GetPlayer()
	{
		return GameManager.Instance.World.GetPrimaryPlayer();
	}

	public static EntityPlayer GetPlayer(int id)
	{
		if (GameManager.Instance != null && GameManager.Instance.World != null)
		{
			return GameManager.Instance.World.GetEntity(id) as EntityPlayer;
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void CalcDisplayProtectionValues()
	{
	}

	public static string GetStatValue(PassiveEffects effect, EntityPlayer player, DisplayInfoEntry entry, FastTags<TagGroup.Global> overrideMovementTag)
	{
		FastTags<TagGroup.Global> tags = player.generalTags;
		if (entry.TagsSet)
		{
			tags = entry.Tags;
		}
		if (overrideMovementTag.IsEmpty)
		{
			tags |= EntityAlive.MovementTagRunning;
		}
		else
		{
			tags |= overrideMovementTag;
		}
		float value = EffectManager.GetValue(effect, null, 0f, player, null, tags, calcEquipment: true, calcHoldingItem: true, calcProgression: true, calcBuffs: true, calcChallenges: true, 1, useMods: true, _useDurability: true);
		if (entry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
		{
			value *= 100f;
			value = Mathf.Floor(value);
			if (entry.ShowInverted)
			{
				value -= 100f;
			}
			return value.ToString("0") + "%";
		}
		if (entry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
		{
			return XUiM_PlayerBuffs.GetCVarValueAsTimeString(value);
		}
		if (entry.DisplayType == DisplayInfoEntry.DisplayTypes.Integer)
		{
			value = Mathf.Floor(value);
		}
		else
		{
			value *= 100f;
			value = Mathf.Floor(value);
			value /= 100f;
		}
		if (entry.ShowInverted)
		{
			value -= 1f;
		}
		return value.ToString("0.##");
	}
}
