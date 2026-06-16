using UnityEngine;

public class BuffEntityUINotification : EntityUINotification
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public BuffValue buff;

	public BuffValue Buff => buff;

	public string Icon => buff.BuffClass.Icon;

	public float CurrentValue
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

	public string Units
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

	public EnumEntityUINotificationDisplayMode DisplayMode
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

	public EnumEntityUINotificationSubject Subject => EnumEntityUINotificationSubject.Buff;

	public BuffEntityUINotification(EntityAlive _owner, BuffValue _buff)
	{
		owner = _owner;
		buff = _buff;
	}

	public Color GetColor()
	{
		return Buff.BuffClass.IconColor;
	}
}
