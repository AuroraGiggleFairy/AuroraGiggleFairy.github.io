using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public struct EntityActivationCommand(string _text, string _icon, bool _enabled, string _eventName = null)
{
	public string text = _text;

	public string icon = _icon;

	public Color iconColor = Color.white;

	public bool enabled = _enabled;

	public string eventName = _eventName;

	public float activateTime = -1f;
}
