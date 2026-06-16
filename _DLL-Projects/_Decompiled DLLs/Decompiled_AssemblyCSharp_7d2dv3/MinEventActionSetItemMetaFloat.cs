using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetItemMetaFloat : MinEventActionBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float change;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool relative = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public string metaKey;

	public override void Execute(MinEventParams _params)
	{
		ItemValue itemValue = _params.ItemValue;
		if (!itemValue.HasMetadata(metaKey))
		{
			itemValue.SetMetadata(metaKey, 0f);
		}
		if (itemValue.GetMetadata(metaKey) is float num)
		{
			if (relative)
			{
				itemValue.SetMetadata(metaKey, num + change);
			}
			else
			{
				itemValue.SetMetadata(metaKey, change);
			}
			if (num < 0f)
			{
				itemValue.SetMetadata(metaKey, 0f);
			}
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (!base.CanExecute(_eventType, _params))
		{
			return false;
		}
		if (string.IsNullOrEmpty(metaKey))
		{
			return false;
		}
		return true;
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			switch (_attribute.Name.LocalName)
			{
			case "change":
				change = StringParsers.ParseFloat(_attribute.Value);
				return true;
			case "relative":
				relative = StringParsers.ParseBool(_attribute.Value);
				return true;
			case "key":
				metaKey = _attribute.Value;
				return true;
			}
		}
		return flag;
	}
}
