using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public class MinEffectController
{
	public enum SourceParentType
	{
		None,
		ItemClass,
		ItemModifierClass,
		EntityClass,
		ProgressionClass,
		BuffClass,
		ChallengeClass,
		ChallengeGroup
	}

	public List<MinEffectGroup> EffectGroups;

	public HashSet<PassiveEffects> PassivesIndex;

	public SourceParentType ParentType;

	public object ParentPointer = -1;

	public bool IsOwnerTiered()
	{
		for (byte b = 0; b < EffectGroups.Count; b++)
		{
			if (EffectGroups[b].OwnerTiered)
			{
				return true;
			}
		}
		return false;
	}

	public void ModifyValue(EntityAlive _self, PassiveEffects _effect, ref float _base_value, ref float _perc_value, float _level = 0f, FastTags<TagGroup.Global> _tags = default(FastTags<TagGroup.Global>), int multiplier = 1)
	{
		if (!PassivesIndex.Contains(_effect))
		{
			return;
		}
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
		for (int i = 0; i < EffectGroups.Count; i++)
		{
			if (!minEventParams.Tags.Equals(_tags))
			{
				minEventParams.Tags = new FastTags<TagGroup.Global>(_tags);
			}
			EffectGroups[i].ModifyValue(minEventParams, _self, _effect, ref _base_value, ref _perc_value, _level, _tags, multiplier);
		}
	}

	public void GetModifiedValueData(List<EffectManager.ModifierValuesAndSources> _modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType _sourceType, EntityAlive _self, PassiveEffects _effect, ref float _base_value, ref float _perc_value, float _level = 0f, FastTags<TagGroup.Global> _tags = default(FastTags<TagGroup.Global>), int multiplier = 1)
	{
		if (PassivesIndex.Contains(_effect))
		{
			for (byte b = 0; b < EffectGroups.Count; b++)
			{
				EffectGroups[b].GetModifiedValueData(_modValueSources, _sourceType, ParentType, _self, _effect, ref _base_value, ref _perc_value, _level, _tags, multiplier, ParentPointer);
			}
		}
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _eventParms)
	{
		_eventParms.ParentType = ParentType;
		for (byte b = 0; b < EffectGroups.Count; b++)
		{
			EffectGroups[b].FireEvent(_eventType, _eventParms);
		}
	}

	public bool HasEvents()
	{
		for (byte b = 0; b < EffectGroups.Count; b++)
		{
			if (EffectGroups[b].HasEvents())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasTrigger(MinEventTypes _eventType)
	{
		for (byte b = 0; b < EffectGroups.Count; b++)
		{
			if (EffectGroups[b].HasTrigger(_eventType))
			{
				return true;
			}
		}
		return false;
	}

	public void AddEffectGroup(MinEffectGroup item, int _order = 0, bool _extends = false)
	{
		if (EffectGroups == null)
		{
			EffectGroups = new List<MinEffectGroup>();
		}
		if (PassivesIndex == null)
		{
			PassivesIndex = new HashSet<PassiveEffects>(EffectManager.PassiveEffectsComparer);
		}
		if (_extends)
		{
			for (int i = 0; i < EffectGroups.Count; i++)
			{
				MinEffectGroup minEffectGroup = EffectGroups[i];
				if (minEffectGroup.Requirements != null)
				{
					continue;
				}
				for (int j = 0; j < minEffectGroup.PassiveEffects.Count; j++)
				{
					PassiveEffect passiveEffect = minEffectGroup.PassiveEffects[j];
					if (!passiveEffect.Tags.IsEmpty || (passiveEffect.Modifier != PassiveEffect.ValueModifierTypes.base_set && passiveEffect.Modifier != PassiveEffect.ValueModifierTypes.perc_set))
					{
						continue;
					}
					for (int num = item.PassiveEffects.Count - 1; num >= 0; num--)
					{
						PassiveEffect passiveEffect2 = item.PassiveEffects[num];
						if (passiveEffect2.Type == passiveEffect.Type && passiveEffect2.Modifier == passiveEffect.Modifier)
						{
							item.PassiveEffects.RemoveAt(num);
						}
					}
				}
			}
		}
		EffectGroups.Insert(_order, item);
		PassivesIndex.UnionWith(item.PassivesIndices);
	}

	public static MinEffectController ParseXml(XElement _element, XElement _elementToExtend = null, SourceParentType _type = SourceParentType.None, object _parentPointer = null)
	{
		bool flag = false;
		MinEffectController minEffectController = new MinEffectController();
		minEffectController.EffectGroups = new List<MinEffectGroup>();
		minEffectController.PassivesIndex = new HashSet<PassiveEffects>(EffectManager.PassiveEffectsComparer);
		minEffectController.ParentType = _type;
		minEffectController.ParentPointer = _parentPointer;
		int num = 0;
		foreach (XElement item in _element.Elements("effect_group"))
		{
			flag = true;
			minEffectController.AddEffectGroup(MinEffectGroup.ParseXml(item), num++);
		}
		if (_elementToExtend != null)
		{
			flag = true;
			XElement xElement = _elementToExtend;
			while (xElement != null)
			{
				num = 0;
				foreach (XElement item2 in xElement.Elements("effect_group"))
				{
					minEffectController.AddEffectGroup(MinEffectGroup.ParseXml(item2), num++, _extends: true);
				}
				XAttribute xAttribute = xElement.Attribute("extends");
				if (xAttribute != null)
				{
					string extendName = xAttribute.Value;
					xElement = _element.Document.Descendants(xElement.Name).FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (XElement e) => (string)e.Attribute("name") == extendName);
					if (xElement == null)
					{
						Log.Warning("Unable to find element to extend '" + extendName + "'");
					}
				}
				else
				{
					xElement = null;
				}
			}
		}
		if (!flag)
		{
			return null;
		}
		return minEffectController;
	}
}
