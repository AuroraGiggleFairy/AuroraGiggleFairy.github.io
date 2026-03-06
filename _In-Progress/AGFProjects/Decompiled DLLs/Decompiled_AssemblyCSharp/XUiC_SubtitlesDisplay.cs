using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SubtitlesDisplay : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label speakerLabel;

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

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool _isDisplaying;

	public static bool IsDisplaying
	{
		get
		{
			return _isDisplaying;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			_isDisplaying = value;
			GameManager.Instance.SetToolTipPause(GameManager.Instance.World.GetPrimaryPlayer()?.PlayerUI.nguiWindowManager, value);
		}
	}

	public override void Init()
	{
		base.Init();
		ID = windowGroup.ID;
		speakerLabel = (XUiV_Label)GetChildById("lblSpeaker").ViewComponent;
		subtitlesLabel = (XUiV_Label)GetChildById("lblSubtitle").ViewComponent;
		background = (XUiV_Panel)GetChildById("bgPanel").ViewComponent;
		bgSprite = background.UiTransform.Find("_background").GetComponentInChildren<UISprite>();
		subtitlesLabel.MaxLineCount = 2;
		subtitlesLabel.Overflow = UILabel.Overflow.ShrinkContent;
	}

	public static void DisplaySubtitle(LocalPlayerUI ui, string speakerText, string contentText, float duration = 3f, bool centerAlign = false)
	{
		XUiC_SubtitlesDisplay instance = GetInstance(ui.xui);
		if (instance == null)
		{
			return;
		}
		ui.windowManager.OpenIfNotOpen("SubtitlesDisplay", _bModal: false, _bIsNotEscClosable: false, _bCloseAllOpenWindows: false);
		instance.SetSubtitle(speakerText, contentText, Mathf.Max(duration, 3f) + 1f, centerAlign);
		if (GameManager.Instance.World != null)
		{
			NGUIWindowManager nGUIWindowManager = GameManager.Instance.World.GetPrimaryPlayer()?.PlayerUI.nguiWindowManager;
			if (nGUIWindowManager != null)
			{
				GameManager.Instance.ClearCurrentTooltip(nGUIWindowManager);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_SubtitlesDisplay GetInstance(XUi _xui)
	{
		return ((XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(ID))?.Controller?.GetChildByType<XUiC_SubtitlesDisplay>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSubtitle(string speaker, string content, float duration, bool centerAlign)
	{
		speakerLabel.Text = speaker;
		subtitlesLabel.Text = content;
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
			if (subtitlesLabel.Label.width != 610)
			{
				subtitlesLabel.Label.width = 610;
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
