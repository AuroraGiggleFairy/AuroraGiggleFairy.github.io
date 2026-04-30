using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetTransformChildrenActive : MinEventActionBase
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
		transform = ((parent_transform.EqualsCaseInsensitive("#HeldItemRoot") && _params.Transform != null) ? _params.Transform : ((!(parent_transform != "")) ? _params.Self.RootTransform : GameUtils.FindDeepChildActive(_params.Self.RootTransform, parent_transform)));
		if (transform == null)
		{
			return;
		}
		Transform transform2 = GameUtils.FindDeepChildActive(transform, transformPath);
		if (!(transform2 == null))
		{
			for (int i = 0; i < transform2.childCount; i++)
			{
				transform2.GetChild(i).gameObject.SetActive(isActive);
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
