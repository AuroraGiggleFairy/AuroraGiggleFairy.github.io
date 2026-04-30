using System;
using UnityEngine;

[Serializable]
public class PaletteEntry
{
	public Color32 color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 128);

	public string label = "ID 0";

	public Color defaultTint = Color.white;
}
