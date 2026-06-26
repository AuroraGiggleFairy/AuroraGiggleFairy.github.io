public class MinEvent
{
	public static MinEventTypes[] Start = new MinEventTypes[3]
	{
		MinEventTypes.onSelfPrimaryActionStart,
		MinEventTypes.onSelfSecondaryActionStart,
		MinEventTypes.onSelfAction2Start
	};

	public static MinEventTypes[] Update = new MinEventTypes[3]
	{
		MinEventTypes.onSelfPrimaryActionUpdate,
		MinEventTypes.onSelfSecondaryActionUpdate,
		MinEventTypes.onSelfAction2Update
	};

	public static MinEventTypes[] End = new MinEventTypes[3]
	{
		MinEventTypes.onSelfPrimaryActionEnd,
		MinEventTypes.onSelfSecondaryActionEnd,
		MinEventTypes.onSelfAction2End
	};
}
