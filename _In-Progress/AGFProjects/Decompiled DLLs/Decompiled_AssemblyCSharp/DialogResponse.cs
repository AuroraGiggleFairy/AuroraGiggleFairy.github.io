using System.Collections.Generic;

public class DialogResponse : BaseStatement
{
	public List<BaseDialogRequirement> RequirementList = new List<BaseDialogRequirement>();

	public string ReturnStatementID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static DialogResponse nextStatementEntry = new DialogResponse("__nextStatementEntry");

	public DialogResponse(string newID)
	{
		ID = newID;
		HeaderName = $"Response : {newID}";
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static DialogResponse NextStatementEntry(string nextStatementID)
	{
		nextStatementEntry.NextStatementID = nextStatementID;
		nextStatementEntry.Text = "[" + Localization.Get("xuiNext") + "]";
		return nextStatementEntry;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void AddRequirement(BaseDialogRequirement requirement)
	{
		RequirementList.Add(requirement);
	}

	public string GetRequiredDescription(EntityPlayer player)
	{
		if (RequirementList.Count == 0)
		{
			return "";
		}
		return RequirementList[0].GetRequiredDescription(player);
	}
}
