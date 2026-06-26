using Audio;
using UnityEngine;

public class PlayerThirstUINotification : EntityStatUINotification
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float previousValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float nextSoundTime;

	public override float MinWarningLevel => 0.25f;

	public override float MaxWarningLevel => float.MaxValue;

	public override float CurrentValue
	{
		get
		{
			if (base.EntityStats == null || base.EntityStats.Water == null)
			{
				return 100f;
			}
			return Mathf.RoundToInt(base.EntityStats.Water.Value + base.EntityStats.Entity.Buffs.GetCustomVar("$waterAmount"));
		}
	}

	public override string Units => "";

	public override string Icon => "ui_game_symbol_thirst";

	public override EnumEntityUINotificationSubject Subject => EnumEntityUINotificationSubject.Water;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnTick(float dt, bool firstTick)
	{
		if (waitTime > 0f)
		{
			waitTime -= dt;
		}
		waitTime = 2f;
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
		else if (Mathf.FloorToInt(currentValue * 100f) > Mathf.FloorToInt(previousValue * 100f))
		{
			SetVisible(visible: true, 2f);
		}
		previousValue = currentValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayWarningSound()
	{
		Manager.BroadcastPlay("Player*Thirsty");
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
}
