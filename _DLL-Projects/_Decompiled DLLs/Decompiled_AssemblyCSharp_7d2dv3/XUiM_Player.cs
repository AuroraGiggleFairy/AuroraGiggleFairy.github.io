using UnityEngine;

public class XUiM_Player : XUiModel
{
	public static int GetLevel(EntityPlayer _player)
	{
		return _player.Progression.GetLevel();
	}

	public static float GetLevelPercent(EntityPlayer _player)
	{
		return _player.Progression.GetLevelProgressPercentage();
	}

	public static int GetXPToNextLevel(EntityPlayer _player)
	{
		return _player.Progression.ExpToNextLevel;
	}

	public static float GetFood(EntityPlayer _player)
	{
		return _player.Stats.Food.Value;
	}

	public static float GetModifiedCurrentFood(EntityPlayer _player)
	{
		return _player.Stats.Food.Value + _player.Buffs.GetCustomVar("$foodAmount");
	}

	public static float GetFoodPercent(EntityPlayer _player)
	{
		return 1f - _player.Stats.Food.Value / _player.Stats.Food.ModifiedMax;
	}

	public static int GetFoodMax(EntityPlayer _player)
	{
		return (int)_player.Stats.Food.Max;
	}

	public static float GetWater(EntityPlayer _player)
	{
		return _player.Stats.Water.Value;
	}

	public static float GetModifiedCurrentWater(EntityPlayer _player)
	{
		return _player.Stats.Water.Value + _player.Buffs.GetCustomVar("$waterAmount");
	}

	public static float GetWaterPercent(EntityPlayer _player)
	{
		return _player.Stats.Water.ValuePercentUI * 100f;
	}

	public static int GetWaterMax(EntityPlayer _player)
	{
		return (int)_player.Stats.Water.Max;
	}

	public static string GetCoreTemp(EntityPlayer _player)
	{
		return ValueDisplayFormatters.Temperature(_player.Buffs.GetCustomVar("_coretemp"));
	}

	public static string GetOutsideTemp(EntityPlayer _player)
	{
		return ValueDisplayFormatters.Temperature(_player.Buffs.GetCustomVar("_outsidetemp"));
	}

	public static int GetZombieKills(EntityPlayer _player)
	{
		return _player.KilledZombies;
	}

	public static int GetPlayerKills(EntityPlayer _player)
	{
		return _player.KilledPlayers;
	}

	public static int GetDeaths(EntityPlayer _player)
	{
		return _player.Died;
	}

	public static string GetKMTraveled(EntityPlayer _player)
	{
		return (_player.distanceWalked / 1000f).ToCultureInvariantString("0.00") + " KM";
	}

	public static int GetItemsCrafted(EntityPlayer _player)
	{
		return (int)_player.totalItemsCrafted;
	}

	public static string GetLongestLife(EntityPlayer _player)
	{
		return XUiM_PlayerBuffs.GetTimeString((float)(int)_player.longestLife * 60f);
	}

	public static string GetCurrentLife(EntityPlayer _player)
	{
		return XUiM_PlayerBuffs.GetTimeString((float)(int)_player.currentLife * 60f);
	}

	public static float GetHealth(EntityPlayer _player)
	{
		return _player.Stats.Health.Value;
	}

	public static float GetStamina(EntityPlayer _player)
	{
		return _player.Stats.Stamina.Value;
	}

	public static float GetMaxHealth(EntityPlayer _player)
	{
		return _player.Stats.Health.Max;
	}

	public static float GetMaxStamina(EntityPlayer _player)
	{
		return _player.Stats.Stamina.Max;
	}

	public static bool GetHasFullHealth(EntityPlayer _player)
	{
		return Mathf.Approximately(_player.Stats.Health.Max, _player.Stats.Health.Value);
	}

	public static EntityPlayer GetPlayer()
	{
		return GameManager.Instance.World.GetPrimaryPlayer();
	}

	public static EntityPlayer GetPlayer(int _id)
	{
		if (GameManager.Instance != null && GameManager.Instance.World != null)
		{
			return GameManager.Instance.World.GetEntity(_id) as EntityPlayer;
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void CalcDisplayProtectionValues()
	{
	}

	public static string GetStatValue(PassiveEffects _effect, EntityPlayer _player, DisplayInfoEntry _entry, FastTags<TagGroup.Global> _overrideMovementTag)
	{
		FastTags<TagGroup.Global> tags = _player.generalTags;
		if (_entry.TagsSet)
		{
			tags = _entry.Tags;
		}
		if (_overrideMovementTag.IsEmpty)
		{
			tags |= EntityAlive.MovementTagRunning;
		}
		else
		{
			tags |= _overrideMovementTag;
		}
		float value = EffectManager.GetValue(_effect, null, 0f, _player, null, tags, calcEquipment: true, calcHoldingItem: true, calcProgression: true, calcBuffs: true, calcChallenges: true, 1, useMods: true, _useDurability: true);
		if (_entry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
		{
			value *= 100f;
			value = Mathf.Floor(value);
			if (_entry.ShowInverted)
			{
				value -= 100f;
			}
			return value.ToString("0") + "%";
		}
		if (_entry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
		{
			return XUiM_PlayerBuffs.GetCVarValueAsTimeString(value);
		}
		if (_entry.DisplayType == DisplayInfoEntry.DisplayTypes.Integer)
		{
			value = Mathf.Floor(value);
		}
		else
		{
			value *= 100f;
			value = Mathf.Floor(value);
			value /= 100f;
		}
		if (_entry.ShowInverted)
		{
			value -= 1f;
		}
		return value.ToString("0.##");
	}
}
