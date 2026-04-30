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

	public string SpriteFillName;

	public Color SpriteFillColor = Color.white;

	public string SubSpriteName;

	public Vector2 SubSpriteOffset;

	public float SubSpriteSize = 16f;

	public override void Init()
	{
		base.Init();
		Properties.ParseEnum("text_type", ref ShowTextType);
		Properties.ParseFloat("fade_percent", ref FadePercent);
		Properties.ParseBool("show_offscreen", ref ShowOffScreen);
		Properties.ParseBool("use_head_offset", ref UseHeadOffset);
		Properties.ParseFloat("sprite_size", ref SpriteSize);
		Properties.ParseColor("sprite_fill_color", ref SpriteFillColor);
		Properties.ParseEnum("sprite_fill_type", ref SpriteFillType);
		Properties.ParseString("sprite_fill_name", ref SpriteFillName);
		Properties.ParseString("sub_sprite_name", ref SubSpriteName);
		Properties.ParseVec("sub_sprite_offset", ref SubSpriteOffset);
		Properties.ParseFloat("sub_sprite_size", ref SubSpriteSize);
		Properties.ParseInt("font_size", ref FontSize);
		Properties.ParseColor("font_color", ref FontColor);
		FadeEndDistance = (MaxDistance - MinDistance) * FadePercent;
	}
}
