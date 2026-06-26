public sealed class TemperatureBuffNotification : BuffEntityUINotification
{
	public override float MinValue => float.MinValue;

	public override float MaxValue => float.MaxValue;

	public override float MinWarningLevel => 30f;

	public override float MaxWarningLevel => 100f;

	public override float CurrentValue => base.EntityStats.CoreTemp.Value;

	public override string Units => "°";

	public override EnumEntityUINotificationDisplayMode DisplayMode => EnumEntityUINotificationDisplayMode.IconPlusCurrentValue;
}
