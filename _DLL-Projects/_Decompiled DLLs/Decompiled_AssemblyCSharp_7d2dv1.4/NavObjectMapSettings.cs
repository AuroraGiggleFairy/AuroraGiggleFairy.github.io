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
		if (Properties.Values.ContainsKey("layer"))
		{
			Layer = StringParsers.ParseSInt32(Properties.Values["layer"]);
		}
		if (Properties.Values.ContainsKey("icon_scale"))
		{
			IconScale = StringParsers.ParseFloat(Properties.Values["icon_scale"]);
			IconScaleVector = new Vector3(IconScale, IconScale, IconScale);
		}
		if (Properties.Values.ContainsKey("adjust_center"))
		{
			AdjustCenter = StringParsers.ParseBool(Properties.Values["adjust_center"]);
		}
		if (Properties.Values.ContainsKey("use_rotation"))
		{
			UseRotation = StringParsers.ParseBool(Properties.Values["use_rotation"]);
		}
	}
}
