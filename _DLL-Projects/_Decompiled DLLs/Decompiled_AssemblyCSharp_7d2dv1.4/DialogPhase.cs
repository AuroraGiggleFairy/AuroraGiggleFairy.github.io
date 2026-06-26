using System.Collections.Generic;

public class DialogPhase : BaseDialogItem
{
	public List<BaseDialogRequirement> RequirementList = new List<BaseDialogRequirement>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string StartStatementID { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string StartResponseID { get; set; }

	public DialogPhase(string newID)
	{
		ID = newID;
		HeaderName = $"Phase : {newID}";
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void AddRequirement(BaseDialogRequirement requirement)
	{
		RequirementList.Add(requirement);
	}

	public override string ToString()
	{
		return HeaderName;
	}
}
