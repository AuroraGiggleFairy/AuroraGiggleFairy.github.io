using UnityEngine;

public class NavObjectSettings
{
	public DynamicProperties Properties;

	public string SpriteName = "";

	public float MinDistance;

	public float MaxDistance = -1f;

	public Vector3 Offset = Vector3.zero;

	public Color Color = Color.white;

	public bool HasPulse;

	public virtual void Init()
	{
		if (Properties.Values.ContainsKey("sprite_name"))
		{
			SpriteName = Properties.Values["sprite_name"];
		}
		if (Properties.Values.ContainsKey("min_distance"))
		{
			MinDistance = StringParsers.ParseFloat(Properties.Values["min_distance"]);
		}
		if (Properties.Values.ContainsKey("max_distance"))
		{
			MaxDistance = StringParsers.ParseFloat(Properties.Values["max_distance"]);
		}
		if (Properties.Values.ContainsKey("offset"))
		{
			Offset = StringParsers.ParseVector3(Properties.Values["offset"]);
		}
		if (Properties.Values.ContainsKey("color"))
		{
			Color = StringParsers.ParseColor32(Properties.Values["color"]);
		}
		if (Properties.Values.ContainsKey("has_pulse"))
		{
			HasPulse = StringParsers.ParseBool(Properties.Values["has_pulse"]);
		}
	}
}
