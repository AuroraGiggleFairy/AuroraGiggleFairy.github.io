using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SubtitlesDisplay : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label subtitlesLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel background;

	[PublicizedFrom(EAccessModifier.Private)]
	public UISprite bgSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public float openTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float duration;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float minDuration = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float durationAdd = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int labelPadding = 28;

	[PublicizedFrom(EAccessModifier.Private)]
	public int targetHeight = 64;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pendingUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public string pendingText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public NGUIText.Alignment alignment = NGUIText.Alignment.Left;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static bool IsDisplaying
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Init()
	{
		base.Init();
		ID = windowGroup.ID;
		subtitlesLabel = (XUiV_Label)GetChildById("lblSubtitle").ViewComponent;
		background = (XUiV_Panel)GetChildById("bgPanel").ViewComponent;
		bgSprite = background.UiTransform.Find("_background").GetComponentInChildren<UISprite>();
		subtitlesLabel.MaxLineCount = 2;
		subtitlesLabel.Overflow = UILabel.Overflow.ShrinkContent;
	}

	public static void DisplaySubtitle(LocalPlayerUI ui, string text, float duration = 3f, bool centerAlign = false)
	{
		XUiC_SubtitlesDisplay instance = GetInstance(ui.xui);
		ui.windowManager.OpenIfNotOpen("SubtitlesDisplay", _bModal: false, _bIsNotEscClosable: false, _bCloseAllOpenWindows: false);
		instance.SetSubtitle(text, Mathf.Max(duration, 3f) + 1f, centerAlign);
	}

	public static XUiC_SubtitlesDisplay GetInstance(XUi _xui)
	{
		return ((XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(ID))?.Controller?.GetChildByType<XUiC_SubtitlesDisplay>();
	}

	public void SetSubtitle(string text, float duration, bool centerAlign)
	{
		subtitlesLabel.Text = text;
		alignment = ((!centerAlign) ? NGUIText.Alignment.Left : NGUIText.Alignment.Center);
		subtitlesLabel.Label.alignment = alignment;
		subtitlesLabel.Label.ProcessText();
		openTime = Time.time;
		this.duration = duration;
		IsDisplaying = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.IsOpen)
		{
			if (subtitlesLabel.Label.alignment != alignment)
			{
				subtitlesLabel.Label.alignment = alignment;
			}
			if (subtitlesLabel.Label.overflowMethod != UILabel.Overflow.ShrinkContent)
			{
				subtitlesLabel.Label.overflowMethod = UILabel.Overflow.ShrinkContent;
			}
			if (subtitlesLabel.Label.width != 1152)
			{
				subtitlesLabel.Label.width = 1152;
			}
			if (Time.time - openTime >= duration)
			{
				base.xui.playerUI.windowManager.CloseIfOpen("SubtitlesDisplay");
				IsDisplaying = false;
			}
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		IsDisplaying = false;
	}
}
