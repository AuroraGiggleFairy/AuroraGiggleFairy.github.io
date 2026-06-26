using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveEntityKill : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string localizedName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int neededKillCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] entityNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> targetTags = FastTags<TagGroup.Global>.none;

	public static string PropObjectiveKey = "objective_name_key";

	public static string PropNeededCount = "needed_count";

	public static string PropEntityNames = "entity_names";

	public static string PropTargetTags = "target_tags";

	public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Number;

	public override bool RequiresZombies => true;

	public override void SetupObjective()
	{
		neededKillCount = Convert.ToInt32(Value);
		if (ID == null)
		{
			return;
		}
		string[] array = ID.Split(',');
		if (array.Length > 1)
		{
			ID = array[0];
			entityNames = new string[array.Length - 1];
			for (int i = 1; i < array.Length; i++)
			{
				entityNames[i - 1] = array[i];
			}
		}
	}

	public override void SetupDisplay()
	{
		keyword = Localization.Get("ObjectiveZombieKill_keyword");
		if (localizedName == "")
		{
			localizedName = ((ID != null && ID != "") ? Localization.Get(ID) : "Any Zombie");
		}
		base.Description = string.Format(keyword, localizedName);
		StatusText = $"{base.CurrentValue}/{neededKillCount}";
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.EntityKill += Current_EntityKill;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.EntityKill -= Current_EntityKill;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_EntityKill(EntityAlive killedBy, EntityAlive killedEntity)
	{
		if (base.Complete)
		{
			return;
		}
		bool flag = false;
		string entityClassName = killedEntity.EntityClass.entityClassName;
		if (targetTags.IsEmpty)
		{
			if (ID == null || entityClassName.EqualsCaseInsensitive(ID))
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
		}
		else
		{
			flag = killedEntity.EntityClass.Tags.Test_AnySet(targetTags);
		}
		if (flag && base.OwnerQuest.CheckRequirements())
		{
			base.CurrentValue++;
			Refresh();
		}
	}

	public override void Refresh()
	{
		if (!base.Complete)
		{
			base.Complete = base.CurrentValue >= neededKillCount;
			if (base.Complete)
			{
				base.OwnerQuest.RefreshQuestCompletion();
			}
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveEntityKill objectiveEntityKill = new ObjectiveEntityKill();
		CopyValues(objectiveEntityKill);
		objectiveEntityKill.localizedName = localizedName;
		objectiveEntityKill.targetTags = targetTags;
		return objectiveEntityKill;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropObjectiveKey))
		{
			localizedName = Localization.Get(properties.Values[PropObjectiveKey]);
		}
		properties.ParseString(PropEntityNames, ref ID);
		properties.ParseString(PropNeededCount, ref Value);
		string optionalValue = "";
		properties.ParseString(PropTargetTags, ref optionalValue);
		targetTags = FastTags<TagGroup.Global>.Parse(optionalValue);
	}

	public override string ParseBinding(string bindingName)
	{
		string iD = ID;
		string value = Value;
		if (localizedName == "")
		{
			localizedName = ((iD != null && iD != "") ? Localization.Get(iD) : "Any Zombie");
		}
		if (!(bindingName == "target"))
		{
			if (bindingName == "targetwithcount")
			{
				return Convert.ToInt32(value) + " " + localizedName;
			}
			return "";
		}
		return localizedName;
	}
}
