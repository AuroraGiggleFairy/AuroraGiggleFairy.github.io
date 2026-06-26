using UnityEngine;

public class BuffEntityUINotification : EntityUINotification
{
	[PublicizedFrom(EAccessModifier.Private)]
	public BuffValue buff;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityStats stats;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool expired;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive owner;

	public virtual float MinValue => 0f;

	public virtual float MaxValue => 0f;

	public virtual float MinWarningLevel => float.MinValue;

	public virtual float MaxWarningLevel => float.MaxValue;

	public virtual float CurrentValue
	{
		get
		{
			if (buff != null && buff.BuffClass != null && buff.BuffClass.DisplayValueCVar != null)
			{
				return owner.Buffs.GetCustomVar(buff.BuffClass.DisplayValueCVar);
			}
			return 0f;
		}
	}

	public virtual string Units
	{
		get
		{
			if (buff != null && buff.BuffClass != null && buff.BuffClass.DisplayValueCVar != null)
			{
				if (buff.BuffClass.DisplayValueCVar.StartsWith("$") || buff.BuffClass.DisplayValueCVar.StartsWith(".") || buff.BuffClass.DisplayValueCVar.StartsWith("_"))
				{
					return "cvar";
				}
				return buff.BuffClass.DisplayValueCVar;
			}
			return "";
		}
	}

	public virtual string Icon => buff.BuffClass.Icon;

	public virtual bool IconBlink
	{
		get
		{
			if (!buff.BuffClass.IconBlink)
			{
				return EffectManager.GetValue(PassiveEffects.BuffBlink, null, 0f, owner, null, buff.BuffClass.NameTag, calcEquipment: false, calcHoldingItem: false, calcProgression: false) >= 1f;
			}
			return true;
		}
	}

	public virtual float FadeOutTime => 0.15f;

	public virtual BuffValue Buff => buff;

	public virtual Color WarningColor => Color.yellow;

	public virtual Color AlertColor => Color.yellow + Color.red;

	public virtual Color EmergencyColor => Color.red;

	public virtual string Description
	{
		get
		{
			if (buff == null)
			{
				return "";
			}
			return buff.BuffClass.Description;
		}
	}

	public EntityStats EntityStats => stats;

	public virtual bool Expired => expired;

	public virtual bool Visible
	{
		get
		{
			if (buff == null)
			{
				return false;
			}
			if (!buff.BuffClass.Hidden)
			{
				return !buff.Paused;
			}
			return false;
		}
	}

	public virtual EnumEntityUINotificationDisplayMode DisplayMode
	{
		get
		{
			EnumEntityUINotificationDisplayMode result = ((buff != null && buff.BuffClass != null) ? buff.BuffClass.DisplayType : EnumEntityUINotificationDisplayMode.IconOnly);
			if (buff.BuffClass.DisplayValueCVar != null && buff.BuffClass.DisplayValueCVar != "")
			{
				result = EnumEntityUINotificationDisplayMode.IconPlusCurrentValue;
			}
			return result;
		}
	}

	public virtual EnumEntityUINotificationSubject Subject => EnumEntityUINotificationSubject.Buff;

	public virtual void Tick(float dt)
	{
	}

	public void SetBuff(BuffValue buff)
	{
		this.buff = buff;
	}

	public void SetStats(EntityStats stats)
	{
		this.stats = stats;
		owner = stats.Entity;
	}

	public void NotifyBuffRemoved()
	{
		expired = true;
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
		return Buff.BuffClass.IconColor;
	}
}
