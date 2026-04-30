using System;
using UnityEngine;

public class NavObjectCompassSettings : NavObjectSettings
{
	public class HotZoneSettings
	{
		public enum HotZoneTypes
		{
			None,
			Treasure,
			Custom
		}

		public HotZoneTypes HotZoneType;

		public string SpriteName = "";

		public Color Color = Color.white;

		public float CustomDistance = -1f;
	}

	public bool IconClamped;

	public float MinCompassIconScale = 0.5f;

	public float MaxCompassIconScale = 1.25f;

	public float MaxScaleDistance = -1f;

	public float MinFadePercent = -1f;

	public string UpSpriteName = "";

	public string DownSpriteName = "";

	public float ShowUpOffset = 3f;

	public float ShowDownOffset = -2f;

	public int DepthOffset;

	public HotZoneSettings HotZone;

	public bool ShowVerticalCompassIcons
	{
		get
		{
			if (!(UpSpriteName != ""))
			{
				return DownSpriteName != "";
			}
			return true;
		}
	}

	public override void Init()
	{
		base.Init();
		Properties.ParseBool("icon_clamped", ref IconClamped);
		Properties.ParseFloat("min_icon_scale", ref MinCompassIconScale);
		Properties.ParseFloat("max_icon_scale", ref MaxCompassIconScale);
		Properties.ParseFloat("min_fade_percent", ref MinFadePercent);
		MaxScaleDistance = MaxDistance;
		Properties.ParseFloat("max_scale_distance", ref MaxScaleDistance);
		Properties.ParseString("up_sprite_name", ref UpSpriteName);
		Properties.ParseString("down_sprite_name", ref DownSpriteName);
		Properties.ParseFloat("show_up_offset", ref ShowUpOffset);
		Properties.ParseFloat("show_down_offset", ref ShowDownOffset);
		if (Properties.Values.ContainsKey("hot_zone_type"))
		{
			HotZoneSettings.HotZoneTypes result = HotZoneSettings.HotZoneTypes.None;
			if (!Enum.TryParse<HotZoneSettings.HotZoneTypes>(Properties.Values["hot_zone_type"], out result))
			{
				result = HotZoneSettings.HotZoneTypes.None;
			}
			if (result != HotZoneSettings.HotZoneTypes.None)
			{
				HotZone = new HotZoneSettings();
				HotZone.HotZoneType = result;
				Properties.ParseString("hot_zone_sprite", ref HotZone.SpriteName);
				if (Properties.Values.ContainsKey("hot_zone_sprite"))
				{
					HotZone.SpriteName = Properties.Values["hot_zone_sprite"];
				}
				if (Properties.Values.ContainsKey("hot_zone_color"))
				{
					HotZone.Color = StringParsers.ParseColor32(Properties.Values["hot_zone_color"]);
				}
				if (Properties.Values.ContainsKey("hot_zone_distance"))
				{
					HotZone.CustomDistance = StringParsers.ParseFloat(Properties.Values["hot_zone_distance"]);
				}
			}
		}
		if (Properties.Values.ContainsKey("depth_offset"))
		{
			DepthOffset = StringParsers.ParseSInt32(Properties.Values["depth_offset"]);
		}
	}
}
