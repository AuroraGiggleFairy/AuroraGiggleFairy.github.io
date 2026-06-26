using UnityEngine.Scripting;

[Preserve]
public struct EntityActivationCommand(string _text, string _icon, bool _enabled)
{
	public string text = _text;

	public string icon = _icon;

	public bool enabled = _enabled;
}
