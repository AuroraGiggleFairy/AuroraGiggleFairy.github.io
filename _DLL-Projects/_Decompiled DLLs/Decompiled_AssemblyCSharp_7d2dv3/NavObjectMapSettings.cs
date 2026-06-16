using UnityEngine;

public class NavObjectMapSettings : NavObjectSettings
{
	public int Layer;

	public float IconScale = 1f;

	public Vector3 IconScaleVector = Vector3.one;

	public bool UseRotation;

	public bool AdjustCenter;

	public override void Init()
	{
		base.Init();
		Properties.ParseInt("layer", ref Layer);
		if (Properties.Values.ContainsKey("icon_scale"))
		{
			IconScale = StringParsers.ParseFloat(Properties.Values["icon_scale"]);
			IconScaleVector = new Vector3(IconScale, IconScale, IconScale);
		}
		Properties.ParseBool("adjust_center", ref AdjustCenter);
		Properties.ParseBool("use_rotation", ref UseRotation);
	}
}
