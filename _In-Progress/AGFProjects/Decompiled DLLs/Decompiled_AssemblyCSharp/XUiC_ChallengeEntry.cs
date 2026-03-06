using Challenges;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ChallengeEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string enabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string disabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string rowColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string hoverColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string selectedColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string trackedColor = "255, 180, 0, 255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string redeemableColor = "0,255,0,255";

	public new bool Selected;

	public bool IsHovered;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite itemIconSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i iconSize;

	public bool IsRedeemBlinking;

	public bool IsChallengeVisible;

	[PublicizedFrom(EAccessModifier.Private)]
	public Challenge entry;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChallengeClass challengeClass;

	[PublicizedFrom(EAccessModifier.Protected)]
	public TweenScale tweenScale;

	public XUiC_ChallengeEntryList Owner;

	public Challenge Entry
	{
		get
		{
			return entry;
		}
		set
		{
			base.ViewComponent.Enabled = value != null;
			entry = value;
			challengeClass = ((entry != null) ? entry.ChallengeClass : null);
			if (challengeClass != null)
			{
				IsChallengeVisible = challengeClass.ChallengeGroup.IsVisible(entry.Owner.Player);
			}
			else
			{
				IsChallengeVisible = true;
			}
			IsDirty = true;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ChallengeWindowGroup JournalUIHandler { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool Tracked { get; set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = entry != null;
		switch (bindingName)
		{
		case "iconname":
			value = "";
			if (flag)
			{
				value = (IsChallengeVisible ? challengeClass.Icon : "ui_game_symbol_other");
			}
			return true;
		case "iconcolor":
			value = "255,255,255,255";
			if (flag)
			{
				if (!IsChallengeVisible)
				{
					value = disabledColor;
				}
				else if (entry.ChallengeState == Challenge.ChallengeStates.Redeemed)
				{
					value = disabledColor;
				}
				else if (entry.ReadyToComplete)
				{
					value = redeemableColor;
				}
				else if (entry.IsTracked)
				{
					value = trackedColor;
				}
				else if (IsHovered)
				{
					value = hoverColor;
				}
				else
				{
					value = enabledColor;
				}
			}
			return true;
		case "rowstatecolor":
			value = rowColor;
			if (flag)
			{
				if (Selected)
				{
					value = selectedColor;
				}
				else if (IsHovered)
				{
					value = hoverColor;
				}
			}
			return true;
		case "hasentry":
			value = (flag ? "true" : "false");
			return true;
		case "tracked":
			value = (flag ? entry.IsTracked.ToString() : "false");
			return true;
		case "fillactive":
			if (flag)
			{
				if (!IsChallengeVisible)
				{
					value = "false";
				}
				else
				{
					value = entry.IsActive.ToString();
				}
			}
			else
			{
				value = "false";
			}
			return true;
		case "fillamount":
			value = ((flag && entry.IsActive && IsChallengeVisible) ? entry.FillAmount.ToString() : "0");
			return true;
		default:
			return false;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("itemIcon");
		if (childById != null)
		{
			itemIconSprite = childById.ViewComponent as XUiV_Sprite;
			iconSize = itemIconSprite.Size;
		}
		tweenScale = itemIconSprite.UiTransform.gameObject.AddComponent<TweenScale>();
		IsDirty = true;
	}

	public override void OnCursorSelected()
	{
		base.OnCursorSelected();
		Owner.SelectedEntry = this;
	}

	public override void OnCursorUnSelected()
	{
		base.OnCursorUnSelected();
		Selected = false;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		if (!IsChallengeVisible)
		{
			IsHovered = false;
			return;
		}
		base.OnHovered(_isOver);
		if (Entry == null)
		{
			IsHovered = false;
			return;
		}
		if (Entry != null && !IsRedeemBlinking)
		{
			if (_isOver)
			{
				tweenScale.from = Vector3.one;
				tweenScale.to = Vector3.one * 1.1f;
				tweenScale.enabled = true;
				tweenScale.duration = 0.5f;
			}
			else
			{
				tweenScale.from = Vector3.one * 1.1f;
				tweenScale.to = Vector3.one;
				tweenScale.enabled = true;
				tweenScale.duration = 0.5f;
			}
		}
		if (IsHovered != _isOver)
		{
			IsHovered = _isOver;
			RefreshBindings();
		}
	}

	public override void Update(float _dt)
	{
		if (IsDirty || (entry != null && entry.NeedsUIUpdate))
		{
			if (challengeClass != null)
			{
				IsChallengeVisible = challengeClass.ChallengeGroup.IsVisible(entry.Owner.Player);
			}
			else
			{
				IsChallengeVisible = true;
			}
			base.ViewComponent.SoundPlayOnHover = IsChallengeVisible;
			base.ViewComponent.SoundPlayOnClick = IsChallengeVisible;
		}
		RefreshBindings(IsDirty);
		IsDirty = false;
		base.Update(_dt);
		if (IsRedeemBlinking && !Selected)
		{
			tweenScale.enabled = false;
			float num = Mathf.PingPong(Time.time, 0.5f);
			float num2 = 1f;
			if (num > 0.25f)
			{
				num2 = 1f + num - 0.25f;
			}
			itemIconSprite.Sprite.SetDimensions((int)((float)iconSize.x * num2), (int)((float)iconSize.y * num2));
		}
		else if (Selected)
		{
			itemIconSprite.Sprite.SetDimensions(iconSize.x, iconSize.y);
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		switch (name)
		{
		case "enabled_color":
			enabledColor = value;
			return true;
		case "disabled_color":
			disabledColor = value;
			return true;
		case "row_color":
			rowColor = value;
			return true;
		case "hover_color":
			hoverColor = value;
			return true;
		case "selected_color":
			selectedColor = value;
			return true;
		default:
			return base.ParseAttribute(name, value, _parent);
		}
	}
}
