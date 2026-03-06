using System;
using System.Collections;
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
		string attribute = _element.GetAttribute("name");
		if (attribute.Length > 0)
		{
			buffClass.Name = attribute.ToLower();
			buffClass.NameTag = FastTags<TagGroup.Global>.Parse(attribute);
			attribute = _element.GetAttribute("name_key");
			if (attribute.Length > 0)
			{
				buffClass.LocalizedName = Localization.Get(attribute);
			}
			else
			{
				buffClass.LocalizedName = Localization.Get(buffClass.Name);
			}
			attribute = _element.GetAttribute("description_key");
			if (attribute.Length > 0)
			{
				buffClass.DescriptionKey = attribute;
				buffClass.Description = Localization.Get(attribute);
			}
			attribute = _element.GetAttribute("tooltip_key");
			if (attribute.Length > 0)
			{
				buffClass.TooltipKey = attribute;
				buffClass.Tooltip = Localization.Get(attribute);
			}
			attribute = _element.GetAttribute("icon");
			buffClass.Icon = ((attribute.Length > 0) ? attribute : null);
			attribute = _element.GetAttribute("hidden");
			buffClass.Hidden = attribute.Length > 0 && StringParsers.ParseBool(attribute);
			attribute = _element.GetAttribute("showonhud");
			buffClass.ShowOnHUD = attribute.Length == 0 || StringParsers.ParseBool(attribute);
			attribute = _element.GetAttribute("update_rate");
			buffClass.UpdateRateTicks = ((attribute.Length > 0) ? ((int)(StringParsers.ParseFloat(attribute) * 20f)) : 20);
			attribute = _element.GetAttribute("allow_in_editor");
			buffClass.AllowInEditor = attribute.Length > 0 && StringParsers.ParseBool(attribute);
			attribute = _element.GetAttribute("required_game_stat");
			buffClass.RequiredGameStat = ((attribute.Length > 0) ? Enum.Parse<EnumGameStats>(attribute) : EnumGameStats.Last);
			attribute = _element.GetAttribute("remove_on_death");
			buffClass.RemoveOnDeath = attribute.Length == 0 || StringParsers.ParseBool(attribute);
			attribute = _element.GetAttribute("display_type");
			buffClass.DisplayType = ((attribute.Length > 0) ? EnumUtils.Parse<EnumEntityUINotificationDisplayMode>(attribute) : EnumEntityUINotificationDisplayMode.IconOnly);
			attribute = _element.GetAttribute("icon_color");
			buffClass.IconColor = ((attribute.Length > 0) ? StringParsers.ParseColor32(attribute) : Color.white);
			attribute = _element.GetAttribute("icon_blink");
			buffClass.IconBlink = attribute.Length > 0 && StringParsers.ParseBool(attribute);
			buffClass.DamageSource = EnumDamageSource.Internal;
			buffClass.DamageType = EnumDamageTypes.None;
			buffClass.StackType = BuffEffectStackTypes.Replace;
			buffClass.DurationMax = 0f;
			foreach (XElement item in _element.Elements())
			{
				string localName = item.Name.LocalName;
				attribute = item.GetAttribute("value");
				switch (localName)
				{
				case "display_value":
					buffClass.DisplayValueCVar = ((attribute.Length > 0) ? attribute : null);
					break;
				case "display_value_key":
					buffClass.DisplayValueKey = ((attribute.Length > 0) ? attribute : null);
					break;
				case "display_value_format":
					if (attribute.Length > 0)
					{
						Enum.TryParse<BuffClass.CVarDisplayFormat>(attribute, ignoreCase: true, out buffClass.DisplayValueFormat);
					}
					break;
				case "damage_source":
					if (attribute.Length > 0)
					{
						buffClass.DamageSource = EnumUtils.Parse<EnumDamageSource>(attribute, _ignoreCase: true);
					}
					break;
				case "damage_type":
					if (attribute.Length > 0)
					{
						buffClass.DamageType = EnumUtils.Parse<EnumDamageTypes>(attribute, _ignoreCase: true);
					}
					break;
				case "stack_type":
					if (attribute.Length > 0)
					{
						buffClass.StackType = EnumUtils.Parse<BuffEffectStackTypes>(attribute, _ignoreCase: true);
					}
					break;
				case "tags":
					if (attribute.Length > 0)
					{
						buffClass.Tags = FastTags<TagGroup.Global>.Parse(attribute);
					}
					break;
				case "duration":
					if (attribute.Length > 0)
					{
						buffClass.DurationMax = StringParsers.ParseFloat(attribute);
					}
					break;
				case "update_rate":
					buffClass.UpdateRateTicks = ((attribute.Length > 0) ? ((int)(StringParsers.ParseFloat(attribute) * 20f)) : 20);
					break;
				case "remove_on_death":
					buffClass.RemoveOnDeath = attribute.Length == 0 || StringParsers.ParseBool(attribute);
					break;
				default:
					buffClass.Requirements = RequirementBase.ParseRequirementGroup(_element);
					break;
				}
			}
			buffClass.Effects = MinEffectController.ParseXml(_element, null, MinEffectController.SourceParentType.BuffClass, buffClass.Name);
			BuffManager.AddBuff(buffClass);
			return;
		}
		throw new Exception("buff must have an name!");
	}
}
