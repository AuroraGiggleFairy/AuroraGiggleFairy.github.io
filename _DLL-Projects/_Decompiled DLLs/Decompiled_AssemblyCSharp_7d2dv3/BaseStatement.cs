using System.Collections.Generic;

public class BaseStatement : BaseDialogItem
{
	public string Text;

	public List<BaseDialogAction> Actions = new List<BaseDialogAction>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string NextStatementID
	{
		get; [PublicizedFrom(EAccessModifier.Internal)]
		set;
	}

	public override string ToString()
	{
		return Text;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void AddAction(BaseDialogAction action)
	{
		Actions.Add(action);
	}
}
