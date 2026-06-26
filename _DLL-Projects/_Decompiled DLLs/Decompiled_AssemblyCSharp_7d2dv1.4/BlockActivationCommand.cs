using System;

public struct BlockActivationCommand(string _text, string _icon, bool _enabled, bool _highlighted = false)
{
	public readonly string text = _text;

	public string icon = _icon;

	public bool enabled = _enabled;

	public bool highlighted = _highlighted;

	public static readonly BlockActivationCommand[] Empty = Array.Empty<BlockActivationCommand>();
}
