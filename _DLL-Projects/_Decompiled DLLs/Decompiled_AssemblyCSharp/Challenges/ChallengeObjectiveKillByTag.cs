using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveKillByTag : BaseChallengeObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string entityTag = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> entityTags;

	[PublicizedFrom(EAccessModifier.Private)]
	public string targetName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int isTwitchSpawn = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> killerHasBuffTag;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> killedHasBuffTag;

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.KillByTag;

	public override string DescriptionText
	{
		get
		{
			if (Biome == "")
			{
				return Localization.Get("challengeObjectiveKill") + " " + Localization.Get(targetName) + ":";
			}
			return string.Format(Localization.Get("challengeObjectiveKillIn"), Localization.Get(targetName), Localization.Get("biome_" + Biome));
		}
	}

	public override void Init()
	{
		entityTags = FastTags<TagGroup.Global>.Parse(entityTag);
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.EntityKill -= Current_EntityKill;
		QuestEventManager.Current.EntityKill += Current_EntityKill;
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.EntityKill -= Current_EntityKill;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_EntityKill(EntityAlive killedBy, EntityAlive killedEntity)
	{
		if (entityTags.Test_AnySet(killedEntity.EntityClass.Tags) && !CheckBaseRequirements() && (isTwitchSpawn <= -1 || ((isTwitchSpawn != 0 || killedEntity.spawnById == -1) && (isTwitchSpawn != 1 || killedEntity.spawnById != -1))) && (killerHasBuffTag.IsEmpty || killedBy.Buffs.HasBuffByTag(killerHasBuffTag)) && (killedHasBuffTag.IsEmpty || killedEntity.Buffs.HasBuffByTag(killedHasBuffTag)))
		{
			base.Current++;
			CheckObjectiveComplete();
		}
	}

	public override void ParseElement(XElement e)
	{
		base.ParseElement(e);
		if (e.HasAttribute("entity_tags"))
		{
			entityTag = e.GetAttribute("entity_tags");
		}
		if (e.HasAttribute("target_name_key"))
		{
			targetName = Localization.Get(e.GetAttribute("target_name_key"));
		}
		else if (e.HasAttribute("target_name"))
		{
			targetName = e.GetAttribute("target_name");
		}
		if (e.HasAttribute("is_twitch_spawn"))
		{
			isTwitchSpawn = (StringParsers.ParseBool(e.GetAttribute("is_twitch_spawn")) ? 1 : 0);
		}
		if (e.HasAttribute("killer_has_bufftag"))
		{
			killerHasBuffTag = FastTags<TagGroup.Global>.Parse(e.GetAttribute("killer_has_bufftag"));
		}
		if (e.HasAttribute("killed_has_bufftag"))
		{
			killedHasBuffTag = FastTags<TagGroup.Global>.Parse(e.GetAttribute("killed_has_bufftag"));
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveKillByTag
		{
			entityTag = entityTag,
			entityTags = entityTags,
			Biome = Biome,
			targetName = targetName,
			isTwitchSpawn = isTwitchSpawn,
			killerHasBuffTag = killerHasBuffTag,
			killedHasBuffTag = killedHasBuffTag
		};
	}
}
