using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveKill : BaseChallengeObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string entityIDs = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] entityNames;

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.Kill;

	public override string DescriptionText
	{
		get
		{
			if (Biome == "")
			{
				return Localization.Get("challengeObjectiveKill") + " " + Localization.Get(entityIDs) + ":";
			}
			return string.Format(Localization.Get("challengeObjectiveKillIn"), Localization.Get(entityIDs), Localization.Get("biome_" + Biome));
		}
	}

	public override void Init()
	{
		if (entityIDs == null)
		{
			return;
		}
		string[] array = entityIDs.Split(',');
		if (array.Length > 1)
		{
			entityIDs = array[0];
			entityNames = new string[array.Length - 1];
			for (int i = 1; i < array.Length; i++)
			{
				entityNames[i - 1] = array[i];
			}
		}
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
		string entityClassName = killedEntity.EntityClass.entityClassName;
		bool flag = false;
		if (CheckBaseRequirements())
		{
			return;
		}
		if (entityIDs == null || entityClassName.EqualsCaseInsensitive(entityIDs))
		{
			flag = true;
		}
		if (!flag && entityNames != null)
		{
			for (int i = 0; i < entityNames.Length; i++)
			{
				if (entityNames[i].EqualsCaseInsensitive(entityClassName))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			base.Current++;
			CheckObjectiveComplete();
		}
	}

	public override void ParseElement(XElement e)
	{
		base.ParseElement(e);
		if (e.HasAttribute("entity_names"))
		{
			entityIDs = e.GetAttribute("entity_names");
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveKill
		{
			entityIDs = entityIDs,
			entityNames = entityNames,
			Biome = Biome
		};
	}
}
