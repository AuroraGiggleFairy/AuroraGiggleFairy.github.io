using Audio;
using UnityEngine;

public class PlayerConsumableUINotification : EntityStatUINotification
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Stat liveStat;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumEntityUINotificationSubject subject;

	[PublicizedFrom(EAccessModifier.Private)]
	public float previousValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float nextSoundTime;

	public override float MinWarningLevel => 0.25f;

	public override float MaxWarningLevel => float.MaxValue;

	public override float CurrentValue => base.EntityStats.Water.ValuePercent;

	public override string Units => "%";

	public override string Icon
	{
		get
		{
			if (subject == EnumEntityUINotificationSubject.Food)
			{
				return "ui_game_symbol_hunger";
			}
			return "ui_game_symbol_thirst";
		}
	}

	public override EnumEntityUINotificationSubject Subject => subject;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnTick(float dt, bool firstTick)
	{
		float currentValue = CurrentValue;
		if (currentValue < 0.1f)
		{
			if (previousValue >= 0.1f)
			{
				PlayWarningSound();
			}
			else
			{
				PlayWarningSoundIfTime(dt);
			}
			SetVisible(visible: true, 0f);
		}
		else if (currentValue <= 0.25f && (previousValue > 0.25f || firstTick))
		{
			SetVisible(visible: true, 10f);
			PlayWarningSound();
		}
		else if (currentValue <= 0.5f && (previousValue > 0.5f || firstTick))
		{
			SetVisible(visible: true, 10f);
		}
		else if (currentValue <= 0.75f && (previousValue > 0.75f || firstTick))
		{
			SetVisible(visible: true, 10f);
		}
		else if (currentValue > previousValue)
		{
			SetVisible(visible: true, 10f);
		}
		previousValue = currentValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayWarningSound()
	{
		if (subject == EnumEntityUINotificationSubject.Food)
		{
			Manager.BroadcastPlay("Player*Hungry");
		}
		else
		{
			Manager.BroadcastPlay("Player*Thirsty");
		}
		nextSoundTime = 60f + Random.value * 15f;
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
