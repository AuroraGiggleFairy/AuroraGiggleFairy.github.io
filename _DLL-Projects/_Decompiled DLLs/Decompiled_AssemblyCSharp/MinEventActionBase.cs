using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionBase : IMinEventAction
{
	public MinEventTypes EventType;

	public float Delay;

	[PublicizedFrom(EAccessModifier.Protected)]
	public RequirementGroup Requirements;

	public virtual void GetInfoStrings(ref List<string> list)
	{
		list.Add(EventType.ToStringCached() + ": " + ToString());
		Requirements?.GetInfoStrings(ref list);
	}

	public virtual void Execute(MinEventParams _params)
	{
	}

	public virtual bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (Requirements != null)
		{
			return Requirements.IsValid(_params);
		}
		return true;
	}

	public virtual bool ParseXmlAttribute(XAttribute _attribute)
	{
		string localName = _attribute.Name.LocalName;
		if (!(localName == "trigger"))
		{
			if (localName == "delay")
			{
				Delay = float.Parse(_attribute.Value);
				return true;
			}
			return false;
		}
		EventType = EnumUtils.Parse<MinEventTypes>(_attribute.Value);
		return true;
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
		minEventActionBase.Requirements = RequirementBase.ParseRequirementGroup(_element);
		minEventActionBase.ParseXMLPostProcess();
		return minEventActionBase;
	}

	public virtual void ParseXMLPostProcess()
	{
	}
}
