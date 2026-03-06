using UnityEngine;

public interface EntityUINotification
{
	BuffValue Buff { get; }

	string Icon { get; }

	float CurrentValue { get; }

	string Units { get; }

	EnumEntityUINotificationDisplayMode DisplayMode { get; }

	EnumEntityUINotificationSubject Subject { get; }

	Color GetColor();
}
