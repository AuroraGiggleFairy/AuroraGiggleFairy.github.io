using System;
using Discord.Utils;

namespace Discord;

internal class ButtonBuilder
{
	public const int MaxButtonLabelLength = 80;

	private string _label;

	private string _customId;

	public string Label
	{
		get
		{
			return _label;
		}
		set
		{
			int? num = value?.Length;
			if (num.HasValue)
			{
				int valueOrDefault = num.GetValueOrDefault();
				if (valueOrDefault > 80)
				{
					throw new ArgumentOutOfRangeException("value", $"Label length must be less or equal to {80}.");
				}
				if (valueOrDefault == 0)
				{
					throw new ArgumentOutOfRangeException("value", "Label length must be at least 1.");
				}
			}
			_label = value;
		}
	}

	public string CustomId
	{
		get
		{
			return _customId;
		}
		set
		{
			int? num = value?.Length;
			if (num.HasValue)
			{
				int valueOrDefault = num.GetValueOrDefault();
				if (valueOrDefault > 100)
				{
					throw new ArgumentOutOfRangeException("value", $"Custom Id length must be less or equal to {100}.");
				}
				if (valueOrDefault == 0)
				{
					throw new ArgumentOutOfRangeException("value", "Custom Id length must be at least 1.");
				}
			}
			_customId = value;
		}
	}

	public ButtonStyle Style { get; set; }

	public IEmote Emote { get; set; }

	public string Url { get; set; }

	public bool IsDisabled { get; set; }

	public ButtonBuilder()
	{
	}

	public ButtonBuilder(string label = null, string customId = null, ButtonStyle style = ButtonStyle.Primary, string url = null, IEmote emote = null, bool isDisabled = false)
	{
		CustomId = customId;
		Style = style;
		Url = url;
		Label = label;
		IsDisabled = isDisabled;
		Emote = emote;
	}

	public ButtonBuilder(ButtonComponent button)
	{
		CustomId = button.CustomId;
		Style = button.Style;
		Url = button.Url;
		Label = button.Label;
		IsDisabled = button.IsDisabled;
		Emote = button.Emote;
	}

	public static ButtonBuilder CreateLinkButton(string label, string url, IEmote emote = null)
	{
		return new ButtonBuilder(label, null, ButtonStyle.Link, url, emote);
	}

	public static ButtonBuilder CreateDangerButton(string label, string customId, IEmote emote = null)
	{
		return new ButtonBuilder(label, customId, ButtonStyle.Danger, null, emote);
	}

	public static ButtonBuilder CreatePrimaryButton(string label, string customId, IEmote emote = null)
	{
		return new ButtonBuilder(label, customId, ButtonStyle.Primary, null, emote);
	}

	public static ButtonBuilder CreateSecondaryButton(string label, string customId, IEmote emote = null)
	{
		return new ButtonBuilder(label, customId, ButtonStyle.Secondary, null, emote);
	}

	public static ButtonBuilder CreateSuccessButton(string label, string customId, IEmote emote = null)
	{
		return new ButtonBuilder(label, customId, ButtonStyle.Success, null, emote);
	}

	public ButtonBuilder WithLabel(string label)
	{
		Label = label;
		return this;
	}

	public ButtonBuilder WithStyle(ButtonStyle style)
	{
		Style = style;
		return this;
	}

	public ButtonBuilder WithEmote(IEmote emote)
	{
		Emote = emote;
		return this;
	}

	public ButtonBuilder WithUrl(string url)
	{
		Url = url;
		return this;
	}

	public ButtonBuilder WithCustomId(string id)
	{
		CustomId = id;
		return this;
	}

	public ButtonBuilder WithDisabled(bool isDisabled)
	{
		IsDisabled = isDisabled;
		return this;
	}

	public ButtonComponent Build()
	{
		if (string.IsNullOrEmpty(Label) && Emote == null)
		{
			throw new InvalidOperationException("A button must have an Emote or a label!");
		}
		if (!(string.IsNullOrEmpty(Url) ^ string.IsNullOrEmpty(CustomId)))
		{
			throw new InvalidOperationException("A button must contain either a URL or a CustomId, but not both!");
		}
		if (Style == (ButtonStyle)0)
		{
			throw new ArgumentException("A button must have a style.", "Style");
		}
		if (Style == ButtonStyle.Link)
		{
			if (string.IsNullOrEmpty(Url))
			{
				throw new InvalidOperationException("Link buttons must have a link associated with them");
			}
			UrlValidation.ValidateButton(Url);
		}
		else if (string.IsNullOrEmpty(CustomId))
		{
			throw new InvalidOperationException("Non-link buttons must have a custom id associated with them");
		}
		return new ButtonComponent(Style, Label, Emote, CustomId, Url, IsDisabled);
	}
}
