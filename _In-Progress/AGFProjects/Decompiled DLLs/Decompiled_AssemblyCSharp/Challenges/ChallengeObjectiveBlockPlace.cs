using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveBlockPlace : BaseChallengeObjective
{
	public string expectedBlock = "";

	public string alternateItem = "";

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.BlockPlace;

	public override string DescriptionText => Localization.Get("xuiWorldPrefabsPlace") + " " + Localization.Get(expectedBlock) + ":";

	public override void HandleOnCreated()
	{
		base.HandleOnCreated();
		CreateRequirements();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateRequirements()
	{
		if (ShowRequirements)
		{
			Owner.SetRequirementGroup(new RequirementObjectiveGroupPlace((alternateItem != "") ? alternateItem : expectedBlock));
		}
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.BlockPlace += Current_BlockPlace;
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.BlockPlace -= Current_BlockPlace;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockPlace(string blockName, Vector3i blockPos)
	{
		bool flag = false;
		if (CheckBaseRequirements())
		{
			return;
		}
		if (expectedBlock == null || expectedBlock == "" || expectedBlock.EqualsCaseInsensitive(blockName))
		{
			flag = true;
		}
		if (!flag && expectedBlock != null && expectedBlock != "")
		{
			Block blockByName = Block.GetBlockByName(expectedBlock, _caseInsensitive: true);
			if (blockByName != null && blockByName.SelectAlternates && blockByName.ContainsAlternateBlock(blockName))
			{
				flag = true;
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
		if (e.HasAttribute("block"))
		{
			expectedBlock = e.GetAttribute("block");
		}
		if (e.HasAttribute("alternate_item"))
		{
			alternateItem = e.GetAttribute("alternate_item");
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveBlockPlace
		{
			expectedBlock = expectedBlock,
			alternateItem = alternateItem
		};
	}
}
