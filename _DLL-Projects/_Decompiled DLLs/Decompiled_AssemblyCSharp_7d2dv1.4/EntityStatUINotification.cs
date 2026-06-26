using UnityEngine;

public abstract class EntityStatUINotification : EntityUINotification
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityStats stats;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool visible;

	[PublicizedFrom(EAccessModifier.Private)]
	public float _displayTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldBeVisible;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstTick = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public const float MaxWaitTime = 2f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float waitTime = 2f;

	public virtual Color WarningColor => Color.white;

	public virtual Color AlertColor => Color.yellow + Color.red;

	public virtual Color EmergencyColor => Color.red;

	public float displayTime
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return _displayTime;
		}
	}

	public bool isPermenentlyVisible
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (visible)
			{
				return _displayTime == 0f;
			}
			return false;
		}
	}

	public virtual float MinValue => 0f;

	public virtual float MaxValue => 0f;

	public abstract float MinWarningLevel { get; }

	public abstract float MaxWarningLevel { get; }

	public abstract float CurrentValue { get; }

	public abstract string Units { get; }

	public abstract string Icon { get; }

	public float FadeOutTime => 0.15f;

	public virtual BuffValue Buff => null;

	public virtual string Description => "";

	public EntityStats EntityStats => stats;

	public virtual bool Visible => visible;

	public virtual bool Expired => !Visible;

	public virtual EnumEntityUINotificationDisplayMode DisplayMode => EnumEntityUINotificationDisplayMode.IconPlusCurrentValue;

	public abstract EnumEntityUINotificationSubject Subject { get; }

	public void Tick(float dt)
	{
		if (visible && shouldBeVisible == visible && _displayTime > 0f)
		{
			_displayTime -= dt;
			if (_displayTime <= 0f)
			{
				shouldBeVisible = false;
			}
		}
		OnTick(dt, firstTick);
		firstTick = false;
		if (shouldBeVisible != visible)
		{
			visible = shouldBeVisible;
			if (shouldBeVisible)
			{
				stats.NotificationAdded(this);
			}
		}
	}

	public virtual Color GetColor()
	{
		if (MinValue != MaxValue)
		{
			if (CurrentValue <= Mathf.Lerp(MinValue, MaxValue, 0.25f))
			{
				return EmergencyColor;
			}
			if (CurrentValue <= Mathf.Lerp(MinValue, MaxValue, 0.5f))
			{
				return AlertColor;
			}
			if (CurrentValue <= Mathf.Lerp(MinValue, MaxValue, 0.75f))
			{
				return WarningColor;
			}
		}
		if (Buff != null && Buff.BuffClass != null)
		{
			return Buff.BuffClass.IconColor;
		}
		return Color.white;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void OnTick(float dt, bool firstTick);

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetVisible(bool visible, float displayTime)
	{
		shouldBeVisible = visible;
		_displayTime = displayTime;
	}

	public void SetBuff(BuffValue _buff)
	{
	}

	public void SetStats(EntityStats _stats)
	{
		stats = _stats;
	}

	public void Reset()
	{
		visible = false;
	}

	public void NotifyBuffRemoved()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityStatUINotification()
	{
	}
}
