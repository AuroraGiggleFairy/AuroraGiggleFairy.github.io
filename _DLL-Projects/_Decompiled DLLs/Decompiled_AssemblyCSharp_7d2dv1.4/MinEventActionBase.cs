using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionBase : IMinEventAction
{
	public MinEventTypes EventType;

	public bool OrCompare;

	public float Delay;

	public List<IRequirement> Requirements;

	public MinEventActionBase()
	{
		Requirements = new List<IRequirement>();
	}

	public virtual void GetInfoStrings(ref List<string> list)
	{
		list.Add(EventType.ToStringCached() + ": " + ToString());
		if (Requirements != null)
		{
			for (int i = 0; i < Requirements.Count; i++)
			{
				Requirements[i].GetInfoStrings(ref list);
			}
		}
	}

	public virtual void Execute(MinEventParams _params)
	{
	}

	public virtual bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (Requirements.Count > 0)
		{
			bool flag = true;
			if (!OrCompare)
			{
				for (int i = 0; i < Requirements.Count; i++)
				{
					flag &= Requirements[i].IsValid(_params);
					if (!flag)
					{
						return false;
					}
				}
			}
			else
			{
				for (int j = 0; j < Requirements.Count; j++)
				{
					flag = Requirements[j].IsValid(_params);
					if (flag)
					{
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			return flag;
		}
		return true;
	}

	public virtual bool ParseXmlAttribute(XAttribute _attribute)
	{
		switch (_attribute.Name.LocalName)
		{
		case "trigger":
			EventType = EnumUtils.Parse<MinEventTypes>(_attribute.Value);
			return true;
		case "anytrue":
			OrCompare = true;
			return true;
		case "compare_type":
			OrCompare = _attribute.Value.EqualsCaseInsensitive("or");
			return true;
		case "delay":
			Delay = float.Parse(_attribute.Value);
			return true;
		default:
			return false;
		}
	}

	public static MinEventActionBase ParseAction(XElement _element)
	{
		if (!_element.HasAttribute("action"))
		{
			return null;
		}
		Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("MinEventAction", _element.GetAttribute("action"));
		if (typeWithPrefix == null)
		{
			Log.Out("Unable to find class: MinEventAction{0}", _element.GetAttribute("action"));
			return null;
		}
		MinEventActionBase minEventActionBase = (MinEventActionBase)Activator.CreateInstance(typeWithPrefix);
		foreach (XAttribute item in _element.Attributes())
		{
			minEventActionBase.ParseXmlAttribute(item);
		}
		foreach (XElement item2 in _element.Elements("requirement"))
		{
			IRequirement requirement = RequirementBase.ParseRequirement(item2);
			if (requirement != null)
			{
				minEventActionBase.Requirements.Add(requirement);
			}
		}
		minEventActionBase.ParseXMLPostProcess();
		return minEventActionBase;
	}

	public virtual void ParseXMLPostProcess()
	{
	}
}
