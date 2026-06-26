public abstract class BaseDialogAction
{
	public enum ActionTypes
	{
		AddBuff,
		AddItem,
		AddQuest,
		CompleteQuest,
		Trader,
		Voice
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string ID { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Value { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Dialog OwnerDialog { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public DialogResponse Owner { get; set; }

	public virtual ActionTypes ActionType => ActionTypes.AddBuff;

	public BaseDialogAction()
	{
		ID = "";
		Value = "";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void CopyValues(BaseDialogAction action)
	{
		action.ID = ID;
		action.Value = Value;
	}

	public virtual void SetupAction()
	{
	}

	public virtual void PerformAction(EntityPlayer player)
	{
	}

	public virtual BaseDialogAction Clone()
	{
		return null;
	}
}
