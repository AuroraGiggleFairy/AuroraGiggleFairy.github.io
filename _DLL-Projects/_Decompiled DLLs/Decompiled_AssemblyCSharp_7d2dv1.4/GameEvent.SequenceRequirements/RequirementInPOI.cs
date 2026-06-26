using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementInPOI : BaseRequirement
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string poiName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string poiTags = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int poiTier = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPOITier = "tier";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPOITags = "tags";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPOINames = "name";

	public override bool CanPerform(Entity target)
	{
		bool flag = true;
		if (target is EntityPlayer entityPlayer)
		{
			if (entityPlayer.prefab != null)
			{
				Prefab prefab = entityPlayer.prefab.prefab;
				if (poiTags != "" && !prefab.Tags.Test_AnySet(FastTags<TagGroup.Poi>.Parse(poiTags)))
				{
					flag = false;
				}
				if (poiName != "" && !poiName.ContainsCaseInsensitive(prefab.PrefabName) && !poiName.ContainsCaseInsensitive(prefab.LocalizedName))
				{
					flag = false;
				}
				if (poiTier != -1 && prefab.DifficultyTier != poiTier)
				{
					flag = false;
				}
			}
			else
			{
				flag = false;
			}
		}
		else
		{
			flag = false;
		}
		if (!Invert)
		{
			return flag;
		}
		return !flag;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseInt(PropPOITier, ref poiTier);
		properties.ParseString(PropPOITags, ref poiTags);
		properties.ParseString(PropPOINames, ref poiName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementInPOI
		{
			Invert = Invert,
			poiTier = poiTier,
			poiTags = poiTags,
			poiName = poiName
		};
	}
}
