using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public struct EntityActivationCommand
{
	public string commandId;

	public string icon;

	public Color iconColor;

	public bool enabled;

	public string eventName;

	public float activateTime;

	public string commandText;

	public EntityActivationCommand(string _commandId, string _icon, string _eventName = null, string _customCommandText = null)
	{
		commandId = _commandId;
		icon = _icon;
		eventName = _eventName;
		iconColor = Color.white;
		activateTime = -1f;
		enabled = true;
		commandText = commandId;
		if (!string.IsNullOrEmpty(_customCommandText))
		{
			commandText = _customCommandText;
		}
		commandText = Localization.Get("entitycommand_" + commandText);
	}
}
