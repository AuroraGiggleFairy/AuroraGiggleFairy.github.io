using UnityEngine;

public sealed class WetBuffNotification : BuffEntityUINotification
{
	public override float MinValue => 0f;

	public override float MaxValue => 1f;

	public override float MinWarningLevel => MinValue;

	public override float MaxWarningLevel => MaxValue;

	public override float CurrentValue => Mathf.Clamp01(base.EntityStats.WaterLevel + 0.01f);

	public override string Units => "%";

	public override EnumEntityUINotificationDisplayMode DisplayMode => EnumEntityUINotificationDisplayMode.IconPlusCurrentValue;
}
