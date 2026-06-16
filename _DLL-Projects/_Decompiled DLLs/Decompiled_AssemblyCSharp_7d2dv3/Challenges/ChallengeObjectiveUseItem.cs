using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveUseItem : BaseChallengeObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string itemName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] itemNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public string overrideText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> itemTags = FastTags<TagGroup.Global>.none;

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.Use;

	public override string DescriptionText
	{
		get
		{
			string text = ((overrideText != "") ? overrideText : Localization.Get(itemName));
			return Localization.Get("challengeObjectiveUse") + " " + text + ":";
		}
	}

	public override void Init()
	{
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
		if (!CheckBaseRequirements() && (itemNames.ContainsCaseInsensitive(itemValue.ItemClass.Name) || (!itemTags.IsEmpty && itemValue.ItemClass.ItemTags.Test_AnySet(itemTags))))
		{
			base.Current++;
			if (base.Current >= MaxCount)
			{
				base.Current = MaxCount;
				CheckObjectiveComplete();
			}
		}
	}

	public override void ParseElement(XElement e)
	{
		base.ParseElement(e);
		if (e.HasAttribute("item"))
		{
			itemName = e.GetAttribute("item");
		}
		if (e.HasAttribute("item_tags"))
		{
			itemTags = FastTags<TagGroup.Global>.Parse(e.GetAttribute("item_tags"));
		}
		if (e.HasAttribute("override_text_key"))
		{
			overrideText = Localization.Get(e.GetAttribute("override_text_key"));
		}
		else if (e.HasAttribute("override_text"))
		{
			overrideText = e.GetAttribute("override_text");
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveUseItem
		{
			itemName = itemName,
			itemNames = itemNames,
			itemTags = itemTags,
			overrideText = overrideText
		};
	}
}
