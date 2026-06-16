using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BuffClass
{
	public enum CVarDisplayFormat
	{
		None,
		Float,
		FlooredToInt,
		RoundedToInt,
		CeiledToInt,
		Time,
		Percentage,
		Degrees
	}

	public string Name;

	public string LocalizedName;

	public string Description;

	public string DescriptionKey;

	public string Tooltip;

	public string TooltipKey;

	public string Icon;

	public string DisplayValueCVar;

	public string DisplayValueKey;

	public CVarDisplayFormat DisplayValueFormat;

	public Color IconColor;

	public bool IconBlink;

	public EnumEntityUINotificationDisplayMode DisplayType;

	public RequirementGroup Requirements;

	public MinEffectController Effects;

	public bool Hidden;

	public bool ShowOnHUD = true;

	public bool AllowInEditor;

	public EnumGameStats RequiredGameStat = EnumGameStats.Last;

	[PublicizedFrom(EAccessModifier.Private)]
	public float durationMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public float initialDurationMax;

	public int UpdateRateTicks = 20;

	public EnumDamageTypes DamageType;

	public EnumDamageSource DamageSource;

	public BuffEffectStackTypes StackType;

	public bool RemoveOnDeath = true;

	public FastTags<TagGroup.Global> NameTag;

	public FastTags<TagGroup.Global> Tags = FastTags<TagGroup.Global>.none;

	public float DurationMax
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return durationMax;
		}
		set
		{
			if (initialDurationMax == 0f && value > 0f)
			{
				initialDurationMax = value;
			}
			durationMax = value;
		}
	}

	public float InitialDurationMax => initialDurationMax;

	public BuffClass(string _name = "")
	{
		Name = _name.ToLower();
		LocalizedName = string.Empty;
		DescriptionKey = string.Empty;
		TooltipKey = string.Empty;
		Icon = string.Empty;
		IconBlink = false;
		Requirements = null;
		Hidden = false;
		DamageType = EnumDamageTypes.None;
		StackType = BuffEffectStackTypes.Replace;
		durationMax = 0f;
		initialDurationMax = 0f;
	}

	public void Tick(BuffValue _ev)
	{
		_ev.DurationTick();
		if (DurationMax > 0f && _ev.DurationInSeconds >= DurationMax)
		{
			_ev.Finished = true;
		}
	}

	public void ModifyValue(EntityAlive _self, PassiveEffects _effect, BuffValue _bv, ref float _base_value, ref float _perc_value, FastTags<TagGroup.Global> _tags)
	{
		if (!_bv.Remove)
		{
			_self.MinEventContext.Tags |= _tags;
			if (canRun(_self.MinEventContext) && Effects != null)
			{
				Effects.ModifyValue(_self, _effect, ref _base_value, ref _perc_value, _bv.DurationInSeconds, _tags, (StackType != BuffEffectStackTypes.Effect) ? 1 : _bv.StackEffectMultiplier);
			}
		}
	}

	public void GetModifiedValueData(List<EffectManager.ModifierValuesAndSources> _modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType _sourceType, EntityAlive _self, PassiveEffects _effect, BuffValue _bv, ref float _base_value, ref float _perc_value, FastTags<TagGroup.Global> _tags)
	{
		if (!_bv.Remove)
		{
			_self.MinEventContext.Tags |= _tags;
			if (canRun(_self.MinEventContext) && Effects != null)
			{
				Effects.GetModifiedValueData(_modValueSources, _sourceType, _self, _effect, ref _base_value, ref _perc_value, _bv.DurationInSeconds, _tags, (StackType != BuffEffectStackTypes.Effect) ? 1 : _bv.StackEffectMultiplier);
			}
		}
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _params)
	{
		if (Effects != null && canRun(_params))
		{
			Effects.FireEvent(_eventType, _params);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canRun(MinEventParams _params)
	{
		if (Requirements != null)
		{
			return Requirements.IsValid(_params);
		}
		return true;
	}
}
