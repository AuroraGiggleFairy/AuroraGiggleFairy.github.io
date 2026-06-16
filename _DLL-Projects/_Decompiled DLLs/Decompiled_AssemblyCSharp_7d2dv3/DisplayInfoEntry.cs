public class DisplayInfoEntry
{
	public enum DisplayTypes
	{
		Integer,
		Decimal1,
		Decimal2,
		Bool,
		Percent,
		Time
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> tags = FastTags<TagGroup.Global>.none;

	public bool TagsSet;

	public PassiveEffects StatType;

	public string CustomName = "";

	public string TitleOverride;

	public DisplayTypes DisplayType;

	public bool ShowInverted;

	public bool NegativePreferred;

	public bool DisplayLeadingPlus;

	public FastTags<TagGroup.Global> Tags
	{
		get
		{
			return tags;
		}
		set
		{
			tags = value;
			TagsSet = true;
		}
	}
}
