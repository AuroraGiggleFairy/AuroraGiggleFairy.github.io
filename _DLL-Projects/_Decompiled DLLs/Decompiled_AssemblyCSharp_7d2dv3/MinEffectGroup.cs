using System;
using System.Collections.Generic;
using System.Xml.Linq;

public class MinEffectGroup
{
	public RequirementGroup Requirements;

	public List<PassiveEffect> PassiveEffects;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<MinEventTypes, List<MinEventActionBase>> TriggeredEffects;

	public List<EffectGroupDescription> EffectDescriptions;

	public CaseInsensitiveStringDictionary<EffectDisplayValue> EffectDisplayValues;

	public bool OwnerTiered;

	public List<PassiveEffects> PassivesIndices;

	public MinEffectGroup()
	{
		Requirements = null;
		PassiveEffects = new List<PassiveEffect>();
		TriggeredEffects = new Dictionary<MinEventTypes, List<MinEventActionBase>>();
		EffectDescriptions = new List<EffectGroupDescription>();
		OwnerTiered = true;
		PassivesIndices = new List<PassiveEffects>();
		EffectDisplayValues = new CaseInsensitiveStringDictionary<EffectDisplayValue>();
	}

	public void ModifyValue(MinEventParams _params, EntityAlive _self, PassiveEffects _effect, ref float _base_value, ref float _perc_value, float level, FastTags<TagGroup.Global> _tags, int _multiplier = 1)
	{
		if (!canRun(_params))
		{
			return;
		}
		int count = PassiveEffects.Count;
		for (int i = 0; i < count; i++)
		{
			PassiveEffect passiveEffect = PassiveEffects[i];
			if (passiveEffect.Type == _effect && passiveEffect.RequirementsMet(_params))
			{
				passiveEffect.ModifyValue(_self, level, ref _base_value, ref _perc_value, _tags, _multiplier);
			}
		}
	}

	public void GetModifiedValueData(List<EffectManager.ModifierValuesAndSources> _modValueSource, EffectManager.ModifierValuesAndSources.ValueSourceType _sourceType, MinEffectController.SourceParentType _parentType, EntityAlive _self, PassiveEffects _effect, ref float _base_value, ref float _perc_value, float level, FastTags<TagGroup.Global> _tags, int _multiplier = 1, object _parentPointer = null)
	{
		MinEventParams minEventParams;
		if (_self == null)
		{
			minEventParams = MinEventParams.CachedEventParam;
			minEventParams.Self = null;
		}
		else
		{
			minEventParams = _self.MinEventContext;
		}
		minEventParams.Tags = _tags;
		if (!canRun(minEventParams))
		{
			return;
		}
		for (int i = 0; i < PassiveEffects.Count; i++)
		{
			PassiveEffect passiveEffect = PassiveEffects[i];
			if (passiveEffect.Type == _effect && passiveEffect.RequirementsMet(minEventParams))
			{
				passiveEffect.GetModifiedValueData(_modValueSource, _sourceType, _parentType, _self, level, ref _base_value, ref _perc_value, _tags, _multiplier, _parentPointer);
			}
		}
	}

	public IReadOnlyList<MinEventActionBase> GetTriggeredEffects(MinEventTypes _eventType)
	{
		if (!TriggeredEffects.TryGetValue(_eventType, out var value))
		{
			return Array.Empty<MinEventActionBase>();
		}
		return value;
	}

	public void AddTriggeredEffect(MinEventActionBase triggeredEffect)
	{
		MinEventTypes eventType = triggeredEffect.EventType;
		if (!TriggeredEffects.TryGetValue(eventType, out var value))
		{
			value = new List<MinEventActionBase>();
			TriggeredEffects.Add(eventType, value);
		}
		value.Add(triggeredEffect);
	}

	public bool HasEvents()
	{
		return TriggeredEffects.Count > 0;
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _eventParms)
	{
		if (TriggeredEffects.Count <= 0)
		{
			return;
		}
		IReadOnlyList<MinEventActionBase> triggeredEffects = GetTriggeredEffects(_eventType);
		if (triggeredEffects.Count <= 0 || !canRun(_eventParms))
		{
			return;
		}
		for (int i = 0; i < triggeredEffects.Count; i++)
		{
			MinEventActionBase minEventActionBase = triggeredEffects[i];
			if (minEventActionBase.CanExecute(_eventType, _eventParms))
			{
				minEventActionBase.Execute(_eventParms);
			}
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

	public bool HasTrigger(MinEventTypes _eventType)
	{
		return GetTriggeredEffects(_eventType).Count > 0;
	}

	public static MinEffectGroup ParseXml(XElement _element)
	{
		MinEffectGroup minEffectGroup = new MinEffectGroup();
		if (_element.HasAttribute("tiered"))
		{
			minEffectGroup.OwnerTiered = StringParsers.ParseBool(_element.GetAttribute("tiered"));
		}
		minEffectGroup.Requirements = RequirementBase.ParseRequirementGroup(_element);
		foreach (XElement item in _element.Elements())
		{
			if (item.Name == "passive_effect")
			{
				PassiveEffect passiveEffect = PassiveEffect.ParsePassiveEffect(item);
				if (passiveEffect != null)
				{
					AddPassiveEffectToGroup(minEffectGroup, passiveEffect);
				}
			}
			else if (item.Name == "triggered_effect")
			{
				MinEventActionBase minEventActionBase = MinEventActionBase.ParseAction(item);
				if (minEventActionBase != null)
				{
					minEffectGroup.AddTriggeredEffect(minEventActionBase);
				}
			}
			else if (item.Name == "effect_description")
			{
				EffectGroupDescription effectGroupDescription = EffectGroupDescription.ParseDescription(item);
				if (effectGroupDescription != null)
				{
					minEffectGroup.EffectDescriptions.Add(effectGroupDescription);
				}
			}
			else if (item.Name == "display_value")
			{
				EffectDisplayValue effectDisplayValue = EffectDisplayValue.ParseDisplayValue(item);
				if (effectDisplayValue != null)
				{
					minEffectGroup.EffectDisplayValues.Add(effectDisplayValue.Name, effectDisplayValue);
				}
			}
		}
		return minEffectGroup;
	}

	public static void AddPassiveEffectToGroup(MinEffectGroup _effectGroup, PassiveEffect _pe)
	{
		_effectGroup.PassivesIndices.Add(_pe.Type);
		_effectGroup.PassiveEffects.Add(_pe);
	}
}
