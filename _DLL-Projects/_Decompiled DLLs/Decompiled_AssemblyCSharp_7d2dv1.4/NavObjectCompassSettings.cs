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
		if (Properties.Values.ContainsKey("icon_clamped"))
		{
			IconClamped = StringParsers.ParseBool(Properties.Values["icon_clamped"]);
		}
		if (Properties.Values.ContainsKey("min_icon_scale"))
		{
			MinCompassIconScale = StringParsers.ParseFloat(Properties.Values["min_icon_scale"]);
		}
		if (Properties.Values.ContainsKey("max_icon_scale"))
		{
			MaxCompassIconScale = StringParsers.ParseFloat(Properties.Values["max_icon_scale"]);
		}
		if (Properties.Values.ContainsKey("min_fade_percent"))
		{
			MinFadePercent = StringParsers.ParseFloat(Properties.Values["min_fade_percent"]);
		}
		if (Properties.Values.ContainsKey("max_scale_distance"))
		{
			MaxScaleDistance = StringParsers.ParseFloat(Properties.Values["max_scale_distance"]);
		}
		else
		{
			MaxScaleDistance = MaxDistance;
		}
		if (Properties.Values.ContainsKey("up_sprite_name"))
		{
			UpSpriteName = Properties.Values["up_sprite_name"];
		}
		if (Properties.Values.ContainsKey("down_sprite_name"))
		{
			DownSpriteName = Properties.Values["down_sprite_name"];
		}
		if (Properties.Values.ContainsKey("show_up_offset"))
		{
			ShowUpOffset = StringParsers.ParseFloat(Properties.Values["show_up_offset"]);
		}
		if (Properties.Values.ContainsKey("show_down_offset"))
		{
			ShowDownOffset = StringParsers.ParseFloat(Properties.Values["show_down_offset"]);
		}
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
