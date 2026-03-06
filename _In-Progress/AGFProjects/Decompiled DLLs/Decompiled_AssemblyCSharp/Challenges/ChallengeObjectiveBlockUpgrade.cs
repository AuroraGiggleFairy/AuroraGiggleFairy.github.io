using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveBlockUpgrade : BaseChallengeObjective
{
	public string expectedBlock = "";

	public string heldItemID = "";

	public string neededResourceID = "";

	public int neededResourceCount = 1;

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.BlockUpgrade;

	public override string DescriptionText => Localization.Get("challengeObjectiveUpgrade") + " " + Localization.Get(expectedBlock) + ":";

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
			Owner.SetRequirementGroup(new RequirementObjectiveGroupBlockUpgrade(heldItemID, neededResourceID, neededResourceCount));
		}
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.BlockUpgrade -= Current_BlockUpgrade;
		QuestEventManager.Current.BlockUpgrade += Current_BlockUpgrade;
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.BlockUpgrade -= Current_BlockUpgrade;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockUpgrade(string blockName, Vector3i blockPos)
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
		if (!flag && blockName.Contains(":") && expectedBlock.EqualsCaseInsensitive(blockName.Substring(0, blockName.IndexOf(':'))))
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
		if (e.HasAttribute("held"))
		{
			heldItemID = e.GetAttribute("held");
		}
		if (e.HasAttribute("needed_resource"))
		{
			neededResourceID = e.GetAttribute("needed_resource");
		}
		if (e.HasAttribute("needed_resource_count"))
		{
			neededResourceCount = StringParsers.ParseSInt32(e.GetAttribute("needed_resource_count"));
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveBlockUpgrade
		{
			expectedBlock = expectedBlock,
			heldItemID = heldItemID,
			neededResourceID = neededResourceID,
			neededResourceCount = neededResourceCount
		};
	}
}
