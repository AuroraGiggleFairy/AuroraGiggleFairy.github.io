using System;
using UnityEngine;

public class NavObjectScreenSettings : NavObjectSettings
{
	public enum ShowTextTypes
	{
		None,
		Distance,
		Name,
		SpawnName
	}

	public enum SpriteFillTypes
	{
		None,
		Health
	}

	public ShowTextTypes ShowTextType;

	public int FontSize = 24;

	public Color FontColor = Color.white;

	public float SpriteSize = 32f;

	public float FadePercent = 0.9f;

	public float FadeEndDistance;

	public bool ShowOffScreen;

	public bool UseHeadOffset;

	public SpriteFillTypes SpriteFillType;

	public string SpriteFillName = "";

	public Color SpriteFillColor = Color.white;

	public string SubSpriteName = "";

	public Vector2 SubSpriteOffset = Vector2.zero;

	public float SubSpriteSize = 16f;

	public override void Init()
	{
		base.Init();
		if (Properties.Values.ContainsKey("text_type") && !Enum.TryParse<ShowTextTypes>(Properties.Values["text_type"], out ShowTextType))
		{
			ShowTextType = ShowTextTypes.None;
		}
		if (Properties.Values.ContainsKey("sprite_size"))
		{
			SpriteSize = StringParsers.ParseFloat(Properties.Values["sprite_size"]);
		}
		if (Properties.Values.ContainsKey("fade_percent"))
		{
			FadePercent = StringParsers.ParseFloat(Properties.Values["fade_percent"]);
		}
		if (Properties.Values.ContainsKey("show_offscreen"))
		{
			ShowOffScreen = StringParsers.ParseBool(Properties.Values["show_offscreen"]);
		}
		if (Properties.Values.ContainsKey("use_head_offset"))
		{
			UseHeadOffset = StringParsers.ParseBool(Properties.Values["use_head_offset"]);
		}
		if (Properties.Values.ContainsKey("sprite_fill_color"))
		{
			SpriteFillColor = StringParsers.ParseColor32(Properties.Values["sprite_fill_color"]);
		}
		Properties.ParseEnum("sprite_fill_type", ref SpriteFillType);
		Properties.ParseString("sprite_fill_name", ref SpriteFillName);
		Properties.ParseString("sub_sprite_name", ref SubSpriteName);
		Properties.ParseVec("sub_sprite_offset", ref SubSpriteOffset);
		Properties.ParseFloat("sub_sprite_size", ref SubSpriteSize);
		FadeEndDistance = (MaxDistance - MinDistance) * FadePercent;
		Properties.ParseInt("font_size", ref FontSize);
		if (Properties.Values.ContainsKey("font_color"))
		{
			FontColor = StringParsers.ParseColor32(Properties.Values["font_color"]);
		}
	}
}
