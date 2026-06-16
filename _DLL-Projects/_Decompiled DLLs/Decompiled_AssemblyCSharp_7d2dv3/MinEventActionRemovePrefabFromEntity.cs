using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionRemovePrefabFromEntity : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string prefabName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string parent_transform_path;

	public override void Execute(MinEventParams _params)
	{
		if (!(_params.Self == null))
		{
			Transform transform = null;
			Transform transform2 = null;
			if (parent_transform_path != null)
			{
				transform2 = GameUtils.FindDeepChildActive(_params.Self.RootTransform, parent_transform_path);
			}
			transform = ((!(transform2 == null)) ? GameUtils.FindDeepChildActive(transform2, "tempPrefab_" + prefabName) : GameUtils.FindDeepChildActive(_params.Self.RootTransform, "tempPrefab_" + prefabName));
			if (!(transform == null))
			{
				Object.Destroy(transform.gameObject);
			}
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (base.CanExecute(_eventType, _params) && _params.Self != null)
		{
			return prefabName != null;
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
			case "prefab":
			case "prefab_name":
				prefabName = _attribute.Value;
				if (prefabName.Contains("/"))
				{
					prefabName = prefabName.Substring(prefabName.LastIndexOf("/") + 1);
				}
				return true;
			case "parent_transform":
				parent_transform_path = _attribute.Value;
				return true;
			}
		}
		return flag;
	}
}
