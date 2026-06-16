using System;

public static class GameStatsBridge
{
	public static void Init()
	{
		GameStats.OnChangedDelegates -= GameStats_OnChangedDelegates;
		GameStats.OnChangedDelegates += GameStats_OnChangedDelegates;
		UpdateStaticFields(EnumGameStats.XPMultiplier, GameStats.GetInt(EnumGameStats.XPMultiplier));
		UpdateStaticFields(EnumGameStats.GlobalGSModifier, GameStats.GetInt(EnumGameStats.GlobalGSModifier));
		UpdateStaticFields(EnumGameStats.BiomeGSModifier, GameStats.GetInt(EnumGameStats.BiomeGSModifier));
		UpdateStaticFields(EnumGameStats.GlobalLSModifier, GameStats.GetInt(EnumGameStats.GlobalLSModifier));
		UpdateStaticFields(EnumGameStats.BiomeLSModifier, GameStats.GetInt(EnumGameStats.BiomeLSModifier));
		UpdateStaticFields(EnumGameStats.BlockDamageAI, GameStats.GetInt(EnumGameStats.BlockDamageAI));
		UpdateStaticFields(EnumGameStats.BlockDamageAIBM, GameStats.GetInt(EnumGameStats.BlockDamageAIBM));
		UpdateStaticFields(EnumGameStats.LootAbundance, GameStats.GetInt(EnumGameStats.LootAbundance));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void GameStats_OnChangedDelegates(EnumGameStats _gameState, object _newValue)
	{
		UpdateStaticFields(_gameState, _newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float ToFloatPercent(object _value)
	{
		if (_value == null)
		{
			return 0f;
		}
		try
		{
			return Convert.ToSingle(_value) / 100f;
		}
		catch (Exception ex)
		{
			Log.Warning($"GameStatsBridge: unable to convert {_value.GetType()} ({_value}) to float: {ex.Message}");
			return 0f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateStaticFields(EnumGameStats _gameState, object _newValue)
	{
		switch (_gameState)
		{
		case EnumGameStats.XPMultiplier:
			Progression.XPGain = ToFloatPercent(_newValue);
			break;
		case EnumGameStats.GlobalGSModifier:
			EntityPlayer.GlobalGameStageModifier = ToFloatPercent(_newValue);
			break;
		case EnumGameStats.BiomeGSModifier:
			EntityPlayer.BiomeGameStageModifier = ToFloatPercent(_newValue);
			break;
		case EnumGameStats.GlobalLSModifier:
			EntityPlayer.GlobalLootStageModifier = ToFloatPercent(_newValue);
			break;
		case EnumGameStats.BiomeLSModifier:
			EntityPlayer.BiomeLootStageModifier = ToFloatPercent(_newValue);
			break;
		case EnumGameStats.BlockDamageAI:
			ItemActionAttack.EntityBlockDamagePercent = ToFloatPercent(_newValue);
			break;
		case EnumGameStats.BlockDamageAIBM:
			ItemActionAttack.BMBlockDamagePercent = ToFloatPercent(_newValue);
			break;
		case EnumGameStats.LootAbundance:
			LootContainer.GlobalCountModifier = ToFloatPercent(_newValue);
			break;
		}
	}
}
