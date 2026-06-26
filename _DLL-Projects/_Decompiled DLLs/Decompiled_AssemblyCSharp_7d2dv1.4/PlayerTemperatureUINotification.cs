using UnityEngine;

public class PlayerTemperatureUINotification : EntityStatUINotification
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float previousValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float nextSoundTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] hotBuffs = new string[3] { "overheated", "heat1", "heat2" };

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] coldBuffs = new string[4] { "freezing", "hypo1", "hypo2", "hypo3" };

	public override float MinValue => 0f;

	public override float MaxValue => 0f;

	public override float MinWarningLevel => 40f;

	public override float MaxWarningLevel => 90f;

	public override float CurrentValue => base.EntityStats.CoreTemp.Value;

	public override string Units => "°";

	public override string Description
	{
		get
		{
			if (CurrentValue < 50f)
			{
				return "You are getting cold";
			}
			return "You are getting hot";
		}
	}

	public override string Icon
	{
		get
		{
			float value = base.EntityStats.CoreTemp.Value;
			if (value >= 100f)
			{
				return "ui_game_symbol_hot";
			}
			if (value <= 30f)
			{
				return "ui_game_symbol_cold";
			}
			return "ui_game_symbol_temperature";
		}
	}

	public override EnumEntityUINotificationSubject Subject => EnumEntityUINotificationSubject.CoreTemp;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnTick(float dt, bool firstTick)
	{
		float currentValue = CurrentValue;
		if (currentValue < 45f)
		{
			if (currentValue <= 30f && AnyBuffsPresent(coldBuffs))
			{
				SetVisible(visible: false, 0f);
				PlayWarningSoundIfTime(dt);
			}
			else
			{
				if (previousValue >= 45f || firstTick)
				{
					PlayWarningSound();
				}
				SetVisible(visible: true, 0f);
			}
		}
		else if (currentValue >= 90f)
		{
			if (currentValue >= 100f && AnyBuffsPresent(hotBuffs))
			{
				SetVisible(visible: false, 0f);
				PlayWarningSoundIfTime(dt);
			}
			else
			{
				if (previousValue < 90f || firstTick)
				{
					PlayWarningSound();
				}
				SetVisible(visible: true, 0f);
			}
		}
		else if (base.isPermenentlyVisible)
		{
			SetVisible(visible: true, 3f);
		}
		previousValue = currentValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayWarningSound()
	{
		if (CurrentValue >= 90f)
		{
			base.EntityStats.Entity.PlayOneShot("Player*Hot");
		}
		else
		{
			base.EntityStats.Entity.PlayOneShot("Player*Cold");
		}
		nextSoundTime = 35f + Random.value * 15f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayWarningSoundIfTime(float dt)
	{
		nextSoundTime -= dt;
		if (nextSoundTime <= 0f)
		{
			PlayWarningSound();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool AnyBuffsPresent(string[] buffs)
	{
		for (int i = 0; i < buffs.Length; i++)
		{
			if (base.EntityStats.Entity.Buffs.GetBuff(buffs[i]) != null)
			{
				return true;
			}
		}
		return false;
	}
}
