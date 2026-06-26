public sealed class UnderwaterBuffNotification : BuffEntityUINotification
{
	public override float MinValue => 0f;

	public override float MaxValue => 1f;

	public override float MinWarningLevel => 0f;

	public override float MaxWarningLevel => 1f;

	public override float CurrentValue
	{
		get
		{
			if (Buff.BuffClass.DurationMax != 0f)
			{
				return Buff.DurationInSeconds / Buff.BuffClass.DurationMax;
			}
			return 0f;
		}
	}

	public override string Units => "%";

	public override string Description => "You are underwater";

	public override EnumEntityUINotificationDisplayMode DisplayMode => EnumEntityUINotificationDisplayMode.IconPlusCurrentValue;
}
