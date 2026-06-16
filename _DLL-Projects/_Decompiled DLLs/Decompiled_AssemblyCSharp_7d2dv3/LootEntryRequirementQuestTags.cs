using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class LootEntryRequirementQuestTags : BaseLootEntryRequirement
{
	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> questTags;

	public override void Init(XElement e)
	{
		base.Init(e);
		string _result = "";
		if (e.ParseAttribute("quest_tags", ref _result))
		{
			questTags = FastTags<TagGroup.Global>.Parse(_result);
		}
	}

	public override bool CheckRequirement(EntityPlayer player)
	{
		if (player.QuestJournal.ActiveQuest != null)
		{
			return player.QuestJournal.ActiveQuest.QuestTags.Test_AnySet(questTags);
		}
		return false;
	}
}
