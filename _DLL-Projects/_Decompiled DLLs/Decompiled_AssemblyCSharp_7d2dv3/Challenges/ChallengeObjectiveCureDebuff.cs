using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveCureDebuff : BaseChallengeObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string buffName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] buffNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public string itemName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] itemNames;

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.CureDebuff;

	public override string DescriptionText => Localization.Get("challengeObjectiveCure") + " " + BuffManager.GetBuff(buffName).LocalizedName + ":";

	public override void Init()
	{
		if (buffName != null)
		{
			buffNames = buffName.Split(',');
			if (buffNames.Length > 1)
			{
				buffName = buffNames[0];
			}
		}
		if (itemName != null)
		{
			itemNames = itemName.Split(',');
			if (itemNames.Length > 1)
			{
				itemName = itemNames[0];
			}
		}
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.UseItem += Current_UseItem;
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.UseItem -= Current_UseItem;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_UseItem(ItemValue itemValue)
	{
		if (!CheckBaseRequirements() && itemNames.ContainsCaseInsensitive(itemValue.ItemClass.Name) && PlayerHasBuff())
		{
			base.Current++;
			if (base.Current >= MaxCount)
			{
				base.Current = MaxCount;
				CheckObjectiveComplete();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool PlayerHasBuff()
	{
		EntityBuffs buffs = Owner.Owner.Player.Buffs;
		for (int i = 0; i < buffNames.Length; i++)
		{
			if (buffs.HasBuff(buffNames[i]))
			{
				return true;
			}
		}
		return false;
	}

	public override void ParseElement(XElement e)
	{
		base.ParseElement(e);
		if (e.HasAttribute("debuff"))
		{
			buffName = e.GetAttribute("debuff");
		}
		if (e.HasAttribute("item"))
		{
			itemName = e.GetAttribute("item");
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveCureDebuff
		{
			buffName = buffName,
			buffNames = buffNames,
			itemName = itemName,
			itemNames = itemNames
		};
	}
}
