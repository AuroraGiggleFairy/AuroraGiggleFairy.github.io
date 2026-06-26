using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetPartActive : MinEventActionBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string partName;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInvert;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cVarName;

	public override void Execute(MinEventParams _params)
	{
		bool flag = isActive;
		if (cVarName != null)
		{
			flag = _params.Self.GetCVar(cVarName) != 0f;
			if (isInvert)
			{
				flag = !flag;
			}
		}
		_params.Self.SetPartActive(partName, flag);
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (base.CanExecute(_eventType, _params) && _params.Self != null && _params.ItemValue != null)
		{
			return partName != null;
		}
		return false;
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "active")
			{
				if (_attribute.Value.Length >= 2 && _attribute.Value[0] == '@')
				{
					int num = 1;
					if (_attribute.Value[1] == '!')
					{
						num++;
						isInvert = true;
					}
					cVarName = _attribute.Value.Substring(num);
				}
				else
				{
					isActive = StringParsers.ParseBool(_attribute.Value);
				}
				return true;
			}
			if (localName == "part")
			{
				partName = _attribute.Value;
				return true;
			}
		}
		return flag;
	}
}
