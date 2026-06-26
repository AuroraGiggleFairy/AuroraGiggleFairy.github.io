using System.Collections.Generic;
using System.Xml.Linq;

public class MinEffectGroup
{
	public bool OrCompareRequirements;

	public List<IRequirement> Requirements;

	public List<PassiveEffect> PassiveEffects;

	public List<MinEventActionBase> TriggeredEffects;

	public List<EffectGroupDescription> EffectDescriptions;

	public CaseInsensitiveStringDictionary<EffectDisplayValue> EffectDisplayValues;

	public bool OwnerTiered;

	public List<PassiveEffects> PassivesIndices;

	public MinEffectGroup()
	{
		Requirements = null;
		PassiveEffects = new List<PassiveEffect>();
		TriggeredEffects = null;
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

	public bool HasEvents()
	{
		return TriggeredEffects != null;
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _eventParms)
	{
		if (TriggeredEffects == null || !canRun(_eventParms))
		{
			return;
		}
		for (int i = 0; i < TriggeredEffects.Count; i++)
		{
			MinEventActionBase minEventActionBase = TriggeredEffects[i];
			if (minEventActionBase.EventType == _eventType && minEventActionBase.CanExecute(_eventType, _eventParms))
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
			if (OrCompareRequirements)
			{
				for (int i = 0; i < Requirements.Count; i++)
				{
					if (Requirements[i].IsValid(_params))
					{
						return true;
					}
				}
				return false;
			}
			for (int j = 0; j < Requirements.Count; j++)
			{
				if (!Requirements[j].IsValid(_params))
				{
					return false;
				}
			}
			return true;
		}
		return true;
	}

	public bool HasTrigger(MinEventTypes _eventType)
	{
		if (TriggeredEffects != null)
		{
			for (int i = 0; i < TriggeredEffects.Count; i++)
			{
				if (TriggeredEffects[i].EventType == _eventType)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static MinEffectGroup ParseXml(XElement _element)
	{
		MinEffectGroup minEffectGroup = new MinEffectGroup();
		if (_element.HasAttribute("compare_type"))
		{
			minEffectGroup.OrCompareRequirements = _element.GetAttribute("compare_type").EqualsCaseInsensitive("or");
		}
		if (_element.HasAttribute("tiered"))
		{
			minEffectGroup.OwnerTiered = StringParsers.ParseBool(_element.GetAttribute("tiered"));
		}
		foreach (XElement item in _element.Elements())
		{
			if (item.Name == "requirements")
			{
				if (item.HasAttribute("compare_type"))
				{
					minEffectGroup.OrCompareRequirements = item.GetAttribute("compare_type").EqualsCaseInsensitive("or");
				}
				List<IRequirement> list = RequirementBase.ParseRequirements(item);
				if (list.Count > 0)
				{
					if (minEffectGroup.Requirements == null)
					{
						minEffectGroup.Requirements = new List<IRequirement>();
					}
					minEffectGroup.Requirements.AddRange(list);
				}
			}
			else if (item.Name == "requirement")
			{
				IRequirement requirement = RequirementBase.ParseRequirement(item);
				if (requirement != null)
				{
					if (minEffectGroup.Requirements == null)
					{
						minEffectGroup.Requirements = new List<IRequirement>();
					}
					minEffectGroup.Requirements.Add(requirement);
				}
			}
			else if (item.Name == "passive_effect")
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
					if (minEffectGroup.TriggeredEffects == null)
					{
						minEffectGroup.TriggeredEffects = new List<MinEventActionBase>();
					}
					minEffectGroup.TriggeredEffects.Add(minEventActionBase);
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
