using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetTransformActive : MinEventActionBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string transformPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public string parent_transform = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isActive;

	public override void Execute(MinEventParams _params)
	{
		Transform transform = null;
		transform = (parent_transform.EqualsCaseInsensitive("#HeldItemRoot") ? _params.Self.inventory.GetHoldingItemTransform() : ((!(parent_transform != "")) ? _params.Self.RootTransform : GameUtils.FindDeepChildActive(_params.Self.RootTransform, parent_transform)));
		if (!(transform == null))
		{
			Transform transform2 = GameUtils.FindDeepChild(transform, transformPath);
			if (!(transform2 == null))
			{
				transform2.gameObject.SetActive(isActive);
				LightManager.LightChanged(transform2.position + Origin.position);
			}
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (base.CanExecute(_eventType, _params) && _params.Self != null && _params.ItemValue != null && transformPath != null)
		{
			return transformPath != "";
		}
		return false;
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			switch (_attribute.Name.LocalName)
			{
			case "active":
				isActive = StringParsers.ParseBool(_attribute.Value);
				return true;
			case "parent_transform":
				parent_transform = _attribute.Value;
				return true;
			case "transform_path":
				transformPath = _attribute.Value;
				return true;
			}
		}
		return flag;
	}
}
