using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionResetProgression : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool resetLevels;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool resetSkills;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool removeBooks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool removeCrafting;

	public override void Execute(MinEventParams _params)
	{
		EntityPlayerLocal entityPlayerLocal = targets[0] as EntityPlayerLocal;
		if (!(entityPlayerLocal != null))
		{
			return;
		}
		entityPlayerLocal.Progression.ResetProgression(resetLevels || resetSkills, removeBooks, removeCrafting);
		if (resetLevels)
		{
			entityPlayerLocal.Progression.Level = 1;
			entityPlayerLocal.Progression.ExpToNextLevel = entityPlayerLocal.Progression.GetExpForNextLevel();
			entityPlayerLocal.Progression.SkillPoints = entityPlayerLocal.QuestJournal.GetRewardedSkillPoints();
			entityPlayerLocal.Progression.ExpDeficit = 0;
		}
		if (removeCrafting)
		{
			List<Recipe> recipes = CraftingManager.GetRecipes();
			for (int i = 0; i < recipes.Count; i++)
			{
				if (recipes[i].IsLearnable)
				{
					entityPlayerLocal.Buffs.RemoveCustomVar(recipes[i].GetName());
				}
			}
		}
		entityPlayerLocal.Progression.ResetProgression(removeBooks);
		entityPlayerLocal.Progression.bProgressionStatsChanged = true;
		entityPlayerLocal.bPlayerStatsChanged = true;
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			switch (_attribute.Name.LocalName)
			{
			case "reset_books":
				removeBooks = StringParsers.ParseBool(_attribute.Value);
				break;
			case "reset_levels":
				resetLevels = StringParsers.ParseBool(_attribute.Value);
				break;
			case "reset_skills":
				resetSkills = StringParsers.ParseBool(_attribute.Value);
				break;
			case "reset_crafting":
				removeCrafting = StringParsers.ParseBool(_attribute.Value);
				break;
			}
		}
		return flag;
	}
}
