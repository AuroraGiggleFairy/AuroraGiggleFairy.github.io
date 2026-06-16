using System;
using UnityEngine;

public struct BlockActivationCommand(string _text, string _icon, bool _enabled, bool _highlighted = false, string _eventName = null)
{
	public string text = _text;

	public string icon = _icon;

	public Color iconColor = Color.white;

	public bool enabled = _enabled;

	public bool highlighted = _highlighted;

	public string eventName = _eventName;

	public float activateTime = -1f;

	public static readonly BlockActivationCommand[] Empty = Array.Empty<BlockActivationCommand>();
}
