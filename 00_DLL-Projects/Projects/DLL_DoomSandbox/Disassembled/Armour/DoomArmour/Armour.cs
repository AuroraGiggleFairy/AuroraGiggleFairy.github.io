namespace DoomArmour;

public static class Armour
{
	public const string GreenVar = "armourType";

	public const string BlueVar = "armourTypeBlue";

	public const float GreenShare = 0.4f;

	public const float BlueShare = 0.6f;

	public const string ReinforcedPerk = "perkReinforcedArmour";

	public const string AdrenalinePerk = "perkAdrenalineRush";

	public const int AdrenalineLevel = 3;

	public const int AdrenalineHealth = 50;

	public const int AdrenalineGain = 1;

	public static float Share(EntityAlive entity)
	{
		EntityBuffs buffs = ((entity == null) ? null : entity.Buffs);
		if (buffs == null)
		{
			return 0f;
		}
		if (buffs.GetCustomVar("armourTypeBlue") >= 1f)
		{
			return 0.6f;
		}
		if (!(buffs.GetCustomVar("armourType") >= 1f))
		{
			return 0f;
		}
		return 0.4f;
	}

	public static Stat Pool(EntityAlive entity)
	{
		return ((entity == null) ? null : entity.Stats)?.Stamina;
	}

	public static int Level(EntityAlive entity, string perk)
	{
		Progression progression = ((entity == null) ? null : entity.Progression);
		if (progression == null)
		{
			return 0;
		}
		ProgressionValue value = progression.GetProgressionValue(perk);
		if (value != null)
		{
			return (int)value.GetCalculatedLevel(entity);
		}
		return 0;
	}

	public static int Refund(EntityAlive entity)
	{
		if (entity.Health > 50)
		{
			return 0;
		}
		return (Level(entity, "perkAdrenalineRush") >= 3) ? 1 : 0;
	}
}
