public class BaseDialogItem
{
	public string ID;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public virtual string HeaderName
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Dialog OwnerDialog { get; set; }
}
