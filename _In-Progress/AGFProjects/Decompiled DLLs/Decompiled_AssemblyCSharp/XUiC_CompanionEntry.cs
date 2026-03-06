using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CompanionEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite[] barHealth;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite[] barHealthModifiedMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite[] barStamina;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite[] barStaminaModifiedMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite arrowContent;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastHealthValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastStaminaValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float distance;

	[PublicizedFrom(EAccessModifier.Private)]
	public float deltaTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float arrowRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastArrowRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public float oldValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt healthcurrentFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int> healthcurrentWMaxFormatter = new CachedStringFormatter<int, int>([PublicizedFrom(EAccessModifier.Internal)] (int _i, int _i2) => $"{_i}/{_i2}");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat healthfillFormatter = new CachedStringFormatterFloat();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<float> distanceFormatter = new CachedStringFormatter<float>([PublicizedFrom(EAccessModifier.Internal)] (float _f) => (_f > 1000f) ? ((_f / 1000f).ToCultureInvariantString("0.0") + " KM") : (_f.ToCultureInvariantString("0.0") + " M"));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor arrowcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive Companion { get; set; }

	public override void Init()
	{
		base.Init();
		IsDirty = true;
		XUiController[] childrenById = GetChildrenById("BarHealth");
		if (childrenById != null)
		{
			barHealth = new XUiV_Sprite[childrenById.Length];
			for (int i = 0; i < childrenById.Length; i++)
			{
				barHealth[i] = (XUiV_Sprite)childrenById[i].ViewComponent;
			}
		}
		childrenById = GetChildrenById("BarHealthModifiedMax");
		if (childrenById != null)
		{
			barHealthModifiedMax = new XUiV_Sprite[childrenById.Length];
			for (int j = 0; j < childrenById.Length; j++)
			{
				barHealthModifiedMax[j] = (XUiV_Sprite)childrenById[j].ViewComponent;
			}
		}
		childrenById = GetChildrenById("BarStamina");
		if (childrenById != null)
		{
			barStamina = new XUiV_Sprite[childrenById.Length];
			for (int k = 0; k < childrenById.Length; k++)
			{
				barStamina[k] = (XUiV_Sprite)childrenById[k].ViewComponent;
			}
		}
		childrenById = GetChildrenById("BarStaminaModifiedMax");
		if (childrenById != null)
		{
			barStaminaModifiedMax = new XUiV_Sprite[childrenById.Length];
			for (int l = 0; l < childrenById.Length; l++)
			{
				barStaminaModifiedMax[l] = (XUiV_Sprite)childrenById[l].ViewComponent;
			}
		}
		XUiController childById = GetChildById("arrowContent");
		if (childById != null)
		{
			arrowContent = (XUiV_Sprite)childById.ViewComponent;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		deltaTime = _dt;
		if (Companion == null || !XUi.IsGameRunning())
		{
			return;
		}
		RefreshFill();
		if (Time.time > updateTime)
		{
			updateTime = Time.time + 1f;
			if (HasChanged() || IsDirty)
			{
				if (IsDirty)
				{
					RefreshBindings(_forceAll: true);
					IsDirty = false;
				}
				else
				{
					RefreshBindings();
				}
			}
		}
		if (Companion != null && arrowContent != null)
		{
			arrowRotation = ReturnRotation(base.xui.playerUI.entityPlayer, Companion);
			if (lastArrowRotation < 15f && arrowRotation > 345f)
			{
				lastArrowRotation = arrowRotation;
			}
			else if (lastArrowRotation > 345f && arrowRotation < 15f)
			{
				lastArrowRotation = arrowRotation;
			}
			else
			{
				lastArrowRotation = Mathf.Lerp(lastArrowRotation, arrowRotation, _dt * 3f);
			}
			arrowContent.UiTransform.localEulerAngles = new Vector3(0f, 0f, lastArrowRotation - 180f);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetCompanion(EntityAlive entity)
	{
		Companion = entity;
		if (Companion == null)
		{
			RefreshBindings(_forceAll: true);
		}
		else
		{
			IsDirty = true;
		}
	}

	public bool HasChanged()
	{
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		float magnitude = (Companion.GetPosition() - entityPlayer.GetPosition()).magnitude;
		bool result = oldValue != magnitude;
		oldValue = magnitude;
		distance = magnitude;
		return result;
	}

	public void RefreshFill()
	{
		if (Companion == null)
		{
			return;
		}
		float t = Time.deltaTime * 3f;
		if (barHealth != null)
		{
			float valuePercentUI = Companion.Stats.Health.ValuePercentUI;
			float fill = Math.Max(lastHealthValue, 0f) * 1.01f;
			lastHealthValue = Mathf.Lerp(lastHealthValue, valuePercentUI, t);
			for (int i = 0; i < barHealth.Length; i++)
			{
				barHealth[i].Fill = fill;
			}
		}
		if (barHealthModifiedMax != null)
		{
			for (int j = 0; j < barHealthModifiedMax.Length; j++)
			{
				barHealthModifiedMax[j].Fill = Companion.Stats.Health.ModifiedMax / Companion.Stats.Health.Max;
			}
		}
		if (barStamina != null)
		{
			float valuePercentUI2 = Companion.Stats.Stamina.ValuePercentUI;
			float fill2 = Math.Max(lastStaminaValue, 0f) * 1.01f;
			lastStaminaValue = Mathf.Lerp(lastStaminaValue, valuePercentUI2, t);
			for (int k = 0; k < barStamina.Length; k++)
			{
				barStamina[k].Fill = fill2;
			}
		}
		if (barStaminaModifiedMax != null)
		{
			for (int l = 0; l < barStaminaModifiedMax.Length; l++)
			{
				barStaminaModifiedMax[l].Fill = Companion.Stats.Stamina.ModifiedMax / Companion.Stats.Stamina.Max;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "healthcurrent":
			if (Companion == null)
			{
				value = "";
				return true;
			}
			value = healthcurrentFormatter.Format(Companion.Health);
			return true;
		case "healthcurrentwithmax":
			if (Companion == null)
			{
				value = "";
				return true;
			}
			value = healthcurrentWMaxFormatter.Format(Companion.Health, Companion.GetMaxHealth());
			return true;
		case "healthfill":
		{
			if (Companion == null)
			{
				value = "0";
				return true;
			}
			float valuePercentUI = Companion.Stats.Health.ValuePercentUI;
			value = healthfillFormatter.Format(valuePercentUI);
			return true;
		}
		case "healthmodifiedmax":
			if (Companion == null || base.xui.playerUI.entityPlayer.IsDead())
			{
				value = "0";
				return true;
			}
			value = (Companion.Stats.Health.ModifiedMax / Companion.Stats.Health.Max).ToCultureInvariantString();
			return true;
		case "partyvisible":
			if (Companion == null)
			{
				value = "false";
				return true;
			}
			value = "true";
			return true;
		case "showicon":
			if (Companion == null)
			{
				value = "false";
				return true;
			}
			value = Companion.IsDead().ToString();
			return true;
		case "showarrow":
			if (Companion == null)
			{
				value = "false";
				return true;
			}
			value = Companion.IsAlive().ToString();
			return true;
		case "arrowcolor":
		{
			Color32 color = Color.white;
			if (Companion == null)
			{
				value = "";
				return true;
			}
			int num = base.xui.playerUI.entityPlayer.Companions.IndexOf(Companion);
			color = Constants.TrackedFriendColors[num % Constants.TrackedFriendColors.Length];
			value = arrowcolorFormatter.Format(color);
			return true;
		}
		case "icon":
			if (Companion == null || GameStats.GetBool(EnumGameStats.AutoParty))
			{
				value = "";
				return true;
			}
			if (Companion.IsDead())
			{
				value = "ui_game_symbol_death";
			}
			else
			{
				value = "";
			}
			return true;
		case "name":
			if (Companion == null)
			{
				value = "";
				return true;
			}
			value = Companion.EntityName;
			return true;
		case "distance":
			if (Companion == null)
			{
				value = "";
				return true;
			}
			value = distanceFormatter.Format(distance);
			return true;
		case "distancecolor":
		{
			Color32 v = Color.white;
			if (Companion == null)
			{
				value = "";
				return true;
			}
			if (distance > 100f)
			{
				v = Color.grey;
			}
			value = itemicontintcolorFormatter.Format(v);
			return true;
		}
		default:
			return false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		IsDirty = true;
		RefreshBindings(_forceAll: true);
	}

	public override void OnClose()
	{
		base.OnClose();
		SetCompanion(null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float ReturnRotation(EntityAlive _self, EntityAlive _other)
	{
		Vector2 vector = new Vector2(_self.transform.forward.x, _self.transform.forward.z);
		Vector3 normalized = (_self.transform.position - _other.transform.position).normalized;
		Vector2 vector2 = new Vector2(normalized.x, normalized.z);
		Vector3 vector3 = Vector3.Cross(vector, vector2);
		float num = Vector2.Angle(vector, vector2);
		if (vector3.z < 0f)
		{
			num = 360f - num;
		}
		return num;
	}
}
