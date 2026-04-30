using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Video;

[Preserve]
public class XUiC_VideoPlayer : XUiController
{
	public delegate void DelegateOnVideoFinished(bool skipped);

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public VideoPlayer videoPlayer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UITexture videoTexture;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Camera videoCamera;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UISprite backgroundSprite;

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject skipPrompt;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Label skipLabel;

	public DelegateOnVideoFinished onVideoFinished;

	[PublicizedFrom(EAccessModifier.Private)]
	public VideoData currentVideo;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture rt;

	[PublicizedFrom(EAccessModifier.Private)]
	public double previousTimestamp;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool skippable;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float skipVisibleDuration = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float skipVisibleTime;

	public static bool IsVideoPlaying = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasVideoSkipped;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool subtitlesEnabled;

	public override void Init()
	{
		base.Init();
		ID = windowGroup.ID;
		videoTexture = GetChildById("videoTexture").ViewComponent.UiTransform.GetComponent<UITexture>();
		backgroundSprite = GetChildById("videoBackground").ViewComponent.UiTransform.GetComponent<UISprite>();
		skipPrompt = GetChildById("skipPrompt").ViewComponent.UiTransform.gameObject;
		skipLabel = (XUiV_Label)GetChildById("lblSkip").ViewComponent;
		videoPlayer = videoTexture.gameObject.AddComponent<VideoPlayer>();
		videoPlayer.playOnAwake = false;
		videoPlayer.isLooping = false;
		videoPlayer.renderMode = VideoRenderMode.RenderTexture;
		videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
		videoPlayer.prepareCompleted += OnVideoPrepared;
		videoPlayer.loopPointReached += OnVideoFinished;
		videoPlayer.errorReceived += OnVideoErrorReceived;
		skipPrompt.SetActive(value: false);
	}

	public static void PlayVideo(XUi _xui, VideoData _videoData, bool _skippable, DelegateOnVideoFinished _videoFinishedCallback = null)
	{
		XUiC_VideoPlayer instance = GetInstance(_xui);
		_xui.playerUI.windowManager.OpenIfNotOpen("VideoPlayer", _bModal: true, _bIsNotEscClosable: false, _bCloseAllOpenWindows: false);
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayVideo(VideoData _videoData, bool _skippable, DelegateOnVideoFinished _videoFinishedCallback = null)
	{
		currentVideo = _videoData;
		subtitlesEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsSubtitlesEnabled);
		wasVideoSkipped = false;
		skippable = _skippable;
		skipPrompt.SetActive(value: false);
		if (_videoFinishedCallback != null)
		{
			onVideoFinished = (DelegateOnVideoFinished)Delegate.Combine(onVideoFinished, _videoFinishedCallback);
		}
		if (rt == null || (rt != null && (Screen.width != rt.width || Screen.height != rt.height)))
		{
			Log.Out("Creating video render texture {0} / {1}", Screen.width, Screen.height);
			rt = new RenderTexture(Screen.width, Screen.height, 16);
			rt.Create();
		}
		videoPlayer.targetTexture = rt;
		videoTexture.mainTexture = videoPlayer.targetTexture;
		string bindingXuiMarkupString = base.xui.playerUI.playerInput.GUIActions.Cancel.GetBindingXuiMarkupString();
		skipLabel.Text = string.Format(Localization.Get("ui_video_skip"), bindingXuiMarkupString);
		previousTimestamp = 0.0;
		videoPlayer.url = Application.streamingAssetsPath + _videoData.url;
		videoPlayer.Prepare();
		IsVideoPlaying = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!base.IsOpen)
		{
			return;
		}
		if (subtitlesEnabled && videoPlayer.isPlaying)
		{
			double time = videoPlayer.time;
			foreach (VideoSubtitle subtitle in currentVideo.subtitles)
			{
				if (subtitle.timestamp >= previousTimestamp && subtitle.timestamp <= time)
				{
					GameManager.ShowSubtitle(LocalPlayerUI.primaryUI.xui, Manager.GetFormattedSubtitleSpeaker(subtitle.subtitleId), Manager.GetFormattedSubtitle(subtitle.subtitleId), subtitle.duration, centerAlign: true);
					break;
				}
			}
			previousTimestamp = time;
		}
		if (videoTexture.mainTexture != rt)
		{
			videoTexture.mainTexture = rt;
		}
		backgroundSprite.color = Color.black;
		if (skippable && base.xui.playerUI.playerInput != null)
		{
			if (!skipPrompt.activeSelf && base.xui.playerUI.playerInput.AnyGUIActionPressed())
			{
				skipPrompt.SetActive(value: true);
				skipVisibleTime = Time.time;
			}
			else if (skipPrompt.activeSelf && base.xui.playerUI.playerInput.GUIActions.Cancel.WasPressed)
			{
				FinishAndClose(_skipped: true);
			}
			if (skipPrompt.activeSelf && Time.time - skipVisibleTime >= 3f)
			{
				skipPrompt.SetActive(value: false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoPrepared(VideoPlayer _source)
	{
		videoPlayer.Play();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoErrorReceived(VideoPlayer _source, string _message)
	{
		Log.Error("Video player encountered an error. Skipping video. Message: {0}", _message);
		FinishAndClose(_skipped: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoFinished(VideoPlayer _source)
	{
		FinishAndClose(_skipped: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FinishAndClose(bool _skipped)
	{
		wasVideoSkipped = _skipped;
		base.xui.playerUI.windowManager.Close(XUiC_SubtitlesDisplay.ID);
		base.xui.playerUI.windowManager.Close("VideoPlayer");
	}

	public override void OnClose()
	{
		base.OnClose();
		if (rt != null)
		{
			UITexture uITexture = videoTexture;
			RenderTexture mainTexture = (videoPlayer.targetTexture = null);
			uITexture.mainTexture = mainTexture;
			rt.Release();
			UnityEngine.Object.Destroy(rt);
		}
		IsVideoPlaying = false;
		if (onVideoFinished != null)
		{
			onVideoFinished(wasVideoSkipped);
		}
	}
}
