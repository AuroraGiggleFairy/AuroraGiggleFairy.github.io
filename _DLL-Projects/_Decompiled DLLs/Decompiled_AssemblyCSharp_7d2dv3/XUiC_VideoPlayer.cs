using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_VideoPlayer : XUiController
{
	public delegate void DelegateOnVideoFinished(bool _skipped);

	[PublicizedFrom(EAccessModifier.Private)]
	public const float SkipVisibleDuration = 3f;

	public static string ID = "";

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiV_Video videoView;

	[XuiBindComponent("videoBackground", true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiV_Sprite backgroundSprite;

	[XuiBindComponent("skipPrompt", true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiView skipPrompt;

	[XuiBindComponent("lblSkip", true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiV_Label skipLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public DelegateOnVideoFinished onVideoFinished;

	[PublicizedFrom(EAccessModifier.Private)]
	public VideoData currentVideo;

	[PublicizedFrom(EAccessModifier.Private)]
	public double previousTimestamp;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool skippable;

	[PublicizedFrom(EAccessModifier.Private)]
	public float skipVisibleTime;

	public static bool IsVideoPlaying;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasVideoSkipped;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool subtitlesEnabled;

	public override void Init()
	{
		base.Init();
		ID = windowGroup.Id;
		skipPrompt.IsVisible = false;
	}

	public static void PlayVideo(XUi _xui, VideoData _videoData, bool _skippable, DelegateOnVideoFinished _videoFinishedCallback = null)
	{
		XUiC_VideoPlayer instance = GetInstance(_xui);
		_xui.playerUI.windowManager.Open("VideoPlayer", _bModal: true);
		instance.PlayVideo(_videoData, _skippable, _videoFinishedCallback);
	}

	public static void EndVideo(XUiC_VideoPlayer _videoPlayer)
	{
		_videoPlayer.FinishAndClose(_skipped: true);
	}

	public static XUiC_VideoPlayer GetInstance(XUi _xui)
	{
		return ((XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(ID))?.Controller?.GetChildByType<XUiC_VideoPlayer>();
	}

	public void PlayVideo(VideoData _videoData, bool _skippable, DelegateOnVideoFinished _videoFinishedCallback = null)
	{
		currentVideo = _videoData;
		subtitlesEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsSubtitlesEnabled);
		wasVideoSkipped = false;
		skippable = _skippable;
		skipPrompt.IsVisible = false;
		if (_videoFinishedCallback != null)
		{
			onVideoFinished = (DelegateOnVideoFinished)Delegate.Combine(onVideoFinished, _videoFinishedCallback);
		}
		string bindingXuiMarkupString = xui.playerUI.playerInput.GUIActions.Cancel.GetBindingXuiMarkupString();
		skipLabel.Text = string.Format(Localization.Get("ui_video_skip"), bindingXuiMarkupString);
		previousTimestamp = 0.0;
		videoView.VideoUri = _videoData.url;
		IsVideoPlaying = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (subtitlesEnabled && videoView.Playing)
		{
			double currentTime = videoView.CurrentTime;
			foreach (VideoSubtitle subtitle in currentVideo.subtitles)
			{
				if (subtitle.timestamp >= previousTimestamp && subtitle.timestamp <= currentTime)
				{
					GameManager.ShowSubtitle(LocalPlayerUI.primaryUI.xui, Manager.GetFormattedSubtitleSpeaker(subtitle.subtitleId), Manager.GetFormattedSubtitle(subtitle.subtitleId), subtitle.duration, centerAlign: true);
					break;
				}
			}
			previousTimestamp = currentTime;
		}
		backgroundSprite.Color = Color.black;
		if (skippable && xui.playerUI.playerInput != null)
		{
			if (!skipPrompt.IsVisible && xui.playerUI.playerInput.AnyGUIActionPressed())
			{
				skipPrompt.IsVisible = true;
				skipVisibleTime = Time.time;
			}
			else if (skipPrompt.IsVisible && xui.playerUI.playerInput.GUIActions.Cancel.WasPressed)
			{
				FinishAndClose(_skipped: true);
			}
			if (skipPrompt.IsVisible && Time.time - skipVisibleTime >= 3f)
			{
				skipPrompt.IsVisible = false;
			}
		}
	}

	[XuiBindEvent("VideoError", "videoView")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoErrorReceived(XUiV_Video _source)
	{
		FinishAndClose(_skipped: true);
	}

	[XuiBindEvent("VideoFinished", "videoView")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoFinished(XUiV_Video _source)
	{
		FinishAndClose(_skipped: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FinishAndClose(bool _skipped)
	{
		wasVideoSkipped = _skipped;
		xui.playerUI.windowManager.Close(XUiC_SubtitlesDisplay.ID);
		xui.playerUI.windowManager.Close(windowGroup);
	}

	public override void OnClose()
	{
		base.OnClose();
		IsVideoPlaying = false;
		onVideoFinished?.Invoke(wasVideoSkipped);
	}
}
