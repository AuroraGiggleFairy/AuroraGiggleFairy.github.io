using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class HasTrackedEntity : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> trackerTags;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasAllTags;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		EntityPlayerLocal entityPlayerLocal = target as EntityPlayerLocal;
		if (entityPlayerLocal == null)
		{
			return false;
		}
		bool flag = false;
		float num = EffectManager.GetValue(PassiveEffects.TrackDistance, null, 0f, entityPlayerLocal);
		if (num >= 0f)
		{
			List<Entity> entitiesInBounds = entityPlayerLocal.world.GetEntitiesInBounds(entityPlayerLocal, new Bounds(entityPlayerLocal.position, Vector3.one * (2f * num)));
			for (int i = 0; i < entitiesInBounds.Count; i++)
			{
				if (hasAllTags)
				{
					if (entitiesInBounds[i].HasAllTags(trackerTags))
					{
						flag = true;
						break;
					}
				}
				else if (entitiesInBounds[i].HasAnyTags(trackerTags))
				{
					flag = true;
					break;
				}
			}
		}
		return flag == !invert;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Is {0} Tracking Entity", invert ? "NOT " : ""));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "tags")
			{
				trackerTags = FastTags<TagGroup.Global>.Parse(_attribute.Value);
				return true;
			}
			if (localName == "has_all_tags")
			{
				hasAllTags = StringParsers.ParseBool(_attribute.Value);
				return true;
			}
		}
		return flag;
	}
}
