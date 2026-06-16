using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class HasAttachedPrefab : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string prefabName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string parent_transform_path;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		Transform transform = null;
		Transform transform2 = null;
		if (parent_transform_path != null)
		{
			transform2 = GameUtils.FindDeepChildActive(_params.Self.RootTransform, parent_transform_path);
		}
		transform = ((!(transform2 == null)) ? GameUtils.FindDeepChildActive(transform2, "tempPrefab_" + prefabName) : GameUtils.FindDeepChildActive(_params.Self.RootTransform, "tempPrefab_" + prefabName));
		if (transform != null)
		{
			return !invert;
		}
		return invert;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Does {0}Have Attached Prefab", invert ? "NOT " : ""));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
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
