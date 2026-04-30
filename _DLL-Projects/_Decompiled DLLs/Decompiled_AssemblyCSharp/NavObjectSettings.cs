using UnityEngine;

public class NavObjectSettings
{
	public DynamicProperties Properties;

	public string SpriteName = "";

	public float MinDistance;

	public float MaxDistance = -1f;

	public Vector3 Offset;

	public Color Color = Color.white;

	public bool HasPulse;

	public virtual void Init()
	{
		Properties.ParseString("sprite_name", ref SpriteName);
		Properties.ParseFloat("min_distance", ref MinDistance);
		Properties.ParseFloat("max_distance", ref MaxDistance);
		Properties.ParseVec("offset", ref Offset);
		Properties.ParseColor("color", ref Color);
		Properties.ParseBool("has_pulse", ref HasPulse);
	}
}
