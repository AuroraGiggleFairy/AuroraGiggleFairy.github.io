public sealed class WaterBuffNotification : BuffEntityUINotification
{
	public override float MinValue => 0f;

	public override float MaxValue => base.EntityStats.Water.Max;

	public override float MinWarningLevel => base.EntityStats.Water.Max * 0.25f;

	public override float MaxWarningLevel => base.EntityStats.Water.Max;

	public override float CurrentValue => base.EntityStats.Water.Value;

	public override string Units => "";

	public override string Description => "You are getting thirsty";

	public override EnumEntityUINotificationDisplayMode DisplayMode => EnumEntityUINotificationDisplayMode.IconPlusCurrentValue;

	public override EnumEntityUINotificationSubject Subject => EnumEntityUINotificationSubject.Water;
}
