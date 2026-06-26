using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class BuffsFromXml
{
	public static IEnumerator CreateBuffs(XmlFile xmlFile)
	{
		BuffManager.Buffs = new CaseInsensitiveStringDictionary<BuffClass>();
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <buffs> found!");
		}
		MicroStopwatch msw = new MicroStopwatch(_bStart: true);
		foreach (XElement item in root.Elements("buff"))
		{
			ParseBuff(item);
			if (msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
		clearBuffValueLinks();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void clearBuffValueLinks()
	{
		if (!GameManager.Instance)
		{
			return;
		}
		DictionaryList<int, Entity> dictionaryList = GameManager.Instance.World?.Entities;
		if (dictionaryList == null || dictionaryList.Count == 0)
		{
			return;
		}
		foreach (Entity item in dictionaryList.list)
		{
			if (item is EntityAlive entityAlive)
			{
				entityAlive.Buffs.ClearBuffClassLinks();
			}
		}
	}

	public static void Reload(XmlFile xmlFile)
	{
		ThreadManager.RunCoroutineSync(CreateBuffs(xmlFile));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseBuff(XElement _element)
	{
		BuffClass buffClass = new BuffClass();
		if (_element.HasAttribute("name"))
		{
			buffClass.Name = _element.GetAttribute("name").ToLower();
			buffClass.NameTag = FastTags<TagGroup.Global>.Parse(_element.GetAttribute("name"));
			if (_element.HasAttribute("name_key"))
			{
				buffClass.LocalizedName = Localization.Get(_element.GetAttribute("name_key"));
			}
			else
			{
				buffClass.LocalizedName = Localization.Get(buffClass.Name);
			}
			if (_element.HasAttribute("description_key"))
			{
				buffClass.DescriptionKey = _element.GetAttribute("description_key");
				buffClass.Description = Localization.Get(buffClass.DescriptionKey);
			}
			if (_element.HasAttribute("tooltip_key"))
			{
				buffClass.TooltipKey = _element.GetAttribute("tooltip_key");
				buffClass.Tooltip = Localization.Get(buffClass.TooltipKey);
			}
			if (_element.HasAttribute("icon"))
			{
				buffClass.Icon = _element.GetAttribute("icon");
			}
			if (_element.HasAttribute("hidden"))
			{
				buffClass.Hidden = StringParsers.ParseBool(_element.GetAttribute("hidden"));
			}
			else
			{
				buffClass.Hidden = false;
			}
			if (_element.HasAttribute("showonhud"))
			{
				buffClass.ShowOnHUD = StringParsers.ParseBool(_element.GetAttribute("showonhud"));
			}
			else
			{
				buffClass.ShowOnHUD = true;
			}
			if (_element.HasAttribute("update_rate"))
			{
				buffClass.UpdateRate = StringParsers.ParseFloat(_element.GetAttribute("update_rate"));
			}
			else
			{
				buffClass.UpdateRate = 1f;
			}
			if (_element.HasAttribute("remove_on_death"))
			{
				buffClass.RemoveOnDeath = StringParsers.ParseBool(_element.GetAttribute("remove_on_death"));
			}
			if (_element.HasAttribute("display_type"))
			{
				buffClass.DisplayType = EnumUtils.Parse<EnumEntityUINotificationDisplayMode>(_element.GetAttribute("display_type"));
			}
			else
			{
				buffClass.DisplayType = EnumEntityUINotificationDisplayMode.IconOnly;
			}
			if (_element.HasAttribute("icon_color"))
			{
				buffClass.IconColor = StringParsers.ParseColor32(_element.GetAttribute("icon_color"));
			}
			else
			{
				buffClass.IconColor = Color.white;
			}
			if (_element.HasAttribute("icon_blink"))
			{
				buffClass.IconBlink = StringParsers.ParseBool(_element.GetAttribute("icon_blink"));
			}
			buffClass.DamageSource = EnumDamageSource.Internal;
			buffClass.DamageType = EnumDamageTypes.None;
			buffClass.StackType = BuffEffectStackTypes.Replace;
			buffClass.DurationMax = 0f;
			foreach (XElement item in _element.Elements())
			{
				if (item.Name == "display_value" && item.HasAttribute("value"))
				{
					buffClass.DisplayValueCVar = item.GetAttribute("value");
				}
				if (item.Name == "display_value_key" && item.HasAttribute("value"))
				{
					buffClass.DisplayValueKey = item.GetAttribute("value");
				}
				if (item.Name == "display_value_format" && item.HasAttribute("value") && !Enum.TryParse<BuffClass.CVarDisplayFormat>(item.GetAttribute("value"), ignoreCase: true, out buffClass.DisplayValueFormat))
				{
					buffClass.DisplayValueFormat = BuffClass.CVarDisplayFormat.None;
				}
				if (item.Name == "damage_source" && item.HasAttribute("value"))
				{
					buffClass.DamageSource = EnumUtils.Parse<EnumDamageSource>(item.GetAttribute("value"), _ignoreCase: true);
				}
				if (item.Name == "damage_type" && item.HasAttribute("value"))
				{
					buffClass.DamageType = EnumUtils.Parse<EnumDamageTypes>(item.GetAttribute("value"), _ignoreCase: true);
				}
				if (item.Name == "stack_type" && item.HasAttribute("value"))
				{
					buffClass.StackType = EnumUtils.Parse<BuffEffectStackTypes>(item.GetAttribute("value"), _ignoreCase: true);
				}
				if (item.Name == "tags" && item.HasAttribute("value"))
				{
					buffClass.Tags = FastTags<TagGroup.Global>.Parse(item.GetAttribute("value"));
				}
				if (item.Name == "cures")
				{
					if (item.HasAttribute("value"))
					{
						buffClass.Cures = new List<string>(item.GetAttribute("value").Split(','));
					}
					else
					{
						buffClass.Cures = new List<string>();
					}
				}
				else
				{
					buffClass.Cures = new List<string>();
				}
				if (item.Name == "duration" && item.HasAttribute("value"))
				{
					buffClass.DurationMax = StringParsers.ParseFloat(item.GetAttribute("value"));
				}
				if (item.Name == "update_rate" && item.HasAttribute("value"))
				{
					buffClass.UpdateRate = StringParsers.ParseFloat(item.GetAttribute("value"));
				}
				if (item.Name == "remove_on_death" && item.HasAttribute("value"))
				{
					buffClass.RemoveOnDeath = StringParsers.ParseBool(item.GetAttribute("value"));
				}
				if (item.Name == "requirement")
				{
					IRequirement requirement = RequirementBase.ParseRequirement(_element);
					if (requirement != null)
					{
						buffClass.Requirements.Add(requirement);
					}
				}
				if (item.Name == "requirements")
				{
					parseBuffRequirements(buffClass, item);
				}
			}
			buffClass.Effects = MinEffectController.ParseXml(_element, null, MinEffectController.SourceParentType.BuffClass, buffClass.Name);
			BuffManager.AddBuff(buffClass);
			return;
		}
		throw new Exception("buff must have an name!");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseBuffRequirements(BuffClass _buff, XElement _element)
	{
		if (_element.HasAttribute("compare_type") && _element.GetAttribute("compare_type").EqualsCaseInsensitive("or"))
		{
			_buff.OrCompare = true;
		}
		foreach (XElement item in _element.Elements("requirement"))
		{
			_ = item;
			IRequirement requirement = RequirementBase.ParseRequirement(_element);
			if (requirement != null)
			{
				_buff.Requirements.Add(requirement);
			}
		}
	}
}
