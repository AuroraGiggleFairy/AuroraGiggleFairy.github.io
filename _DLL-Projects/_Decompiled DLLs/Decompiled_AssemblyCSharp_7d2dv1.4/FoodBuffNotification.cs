public sealed class FoodBuffNotification : BuffEntityUINotification
{
	public override float MinValue => 0f;

	public override float MaxValue => base.EntityStats.Food.Max;

	public override float MinWarningLevel => base.EntityStats.Food.Max * 0.25f;

	public override float MaxWarningLevel => base.EntityStats.Food.Max;

	public override float CurrentValue => base.EntityStats.Food.Value;

	public override string Units => "%";

	public override string Description => "You are getting hungry";

	public override EnumEntityUINotificationDisplayMode DisplayMode => EnumEntityUINotificationDisplayMode.IconPlusCurrentValue;

	public override EnumEntityUINotificationSubject Subject => EnumEntityUINotificationSubject.Food;
}
