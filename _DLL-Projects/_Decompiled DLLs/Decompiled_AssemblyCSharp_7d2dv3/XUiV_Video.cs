using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class XUiV_Video : XUiV_TextureBased
{
	public delegate void VideoErrorDelegate(XUiV_Video _sender);

	public delegate void VideoFinishedDelegate(XUiV_Video _sender);

	[PublicizedFrom(EAccessModifier.Private)]
	public VideoPlayer videoPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture rt;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool loop;

	[PublicizedFrom(EAccessModifier.Private)]
	public string videoUri;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool videoUriChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public VideoAspectRatio videoAspect = VideoAspectRatio.FitInside;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumGamePrefs volumeSetting = EnumGamePrefs.OptionsMenuMusicVolumeLevel;

	[XuiXmlAttribute("loop", false)]
	public bool Loop
	{
		get
		{
			return loop;
		}
		set
		{
			if (loop != value)
			{
				loop = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("uri", false)]
	public string VideoUri
	{
		get
		{
			return videoUri;
		}
		set
		{
			if (!(videoUri == value))
			{
				videoUri = value;
				videoUriChanged = true;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("videoaspect", false)]
	public VideoAspectRatio VideoAspect
	{
		get
		{
			return videoAspect;
		}
		set
		{
			if (videoAspect != value)
			{
				videoAspect = value;
				SetDirty();
			}
		}
	}

	public bool Playing
	{
		get
		{
			return videoPlayer.isPlaying;
		}
		set
		{
			if (videoPlayer.isPlaying != value)
			{
				if (value)
				{
					videoPlayer.Play();
				}
				else
				{
					videoPlayer.Stop();
				}
			}
		}
	}

	public EnumGamePrefs VolumeSetting
	{
		get
		{
			return volumeSetting;
		}
		set
		{
			if (volumeSetting != value)
			{
				volumeSetting = value;
				SetDirty();
			}
		}
	}

	public double CurrentTime
	{
		get
		{
			return videoPlayer.time;
		}
		set
		{
			videoPlayer.time = value;
		}
	}

	public event VideoErrorDelegate VideoError;

	public event VideoFinishedDelegate VideoFinished;

	public XUiV_Video(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		base.createComponents(_go);
		_go.AddComponent<VideoPlayer>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		videoPlayer = uiTransform.gameObject.GetComponent<VideoPlayer>();
	}

	public override void InitView()
	{
		base.InitView();
		videoPlayer.playOnAwake = false;
		videoPlayer.renderMode = VideoRenderMode.RenderTexture;
		videoPlayer.prepareCompleted += OnVideoPrepared;
		videoPlayer.loopPointReached += OnVideoFinished;
		videoPlayer.errorReceived += OnVideoErrorReceived;
		GamePrefs.OnGamePrefChanged += OnGamePrefChanged;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		GamePrefs.OnGamePrefChanged -= OnGamePrefChanged;
		destroyRenderTexture();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		ThreadManager.StartCoroutine(startVideoEndOfFrame());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startVideoEndOfFrame()
	{
		yield return new WaitForEndOfFrame();
		startVideo();
	}

	public override void OnClose()
	{
		base.OnClose();
		destroyRenderTexture();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGamePrefChanged(EnumGamePrefs _pref)
	{
		if (_pref == VolumeSetting)
		{
			updateVolumeLevel();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startVideo()
	{
		if (string.IsNullOrEmpty(VideoUri))
		{
			destroyRenderTexture();
			return;
		}
		updateRenderTexture();
		string text = null;
		if (ModManager.TryPatchModPathString(VideoUri, out var _modPath))
		{
			text = _modPath;
		}
		else if (VideoUri[0] == '@' && VideoUri[1] != ':')
		{
			string text2 = VideoUri.Substring(1);
			if (text2.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
			{
				text = text2.Substring(5);
				if (text[0] != '/' && text[0] != '\\')
				{
					text = new Uri(((Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXServer) ? (Application.dataPath + "/../../") : (Application.dataPath + "/../")) + text).AbsoluteUri;
				}
			}
			if (text == null)
			{
				videoPlayer.url = text2;
			}
		}
		else
		{
			text = Application.streamingAssetsPath + VideoUri;
		}
		if (text != null)
		{
			string extension = Path.GetExtension(text);
			if (string.IsNullOrEmpty(extension) || extension.Length > 5)
			{
				string text3 = ((Application.platform != RuntimePlatform.PS5) ? ".webm" : ".mp4");
				string text4 = text3;
				text3 = ((Application.platform != RuntimePlatform.PS5) ? ".mp4" : ".webm");
				string text5 = text3;
				text = (File.Exists(text + text4) ? (text + text4) : ((!File.Exists(text + text5)) ? null : (text + text5)));
			}
			videoPlayer.url = text;
		}
		videoPlayer.Prepare();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stopVideo()
	{
		if (videoPlayer.isPlaying)
		{
			videoPlayer.Stop();
			videoPlayer.url = null;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		updateRenderTexture();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		base.updateData();
		uiTexture.enabled = true;
		videoPlayer.aspectRatio = VideoAspect;
		videoPlayer.isLooping = Loop;
		if (videoUriChanged)
		{
			stopVideo();
			startVideo();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void updateVolumeLevel()
	{
		if (videoPlayer.isPrepared && videoPlayer.audioTrackCount >= 1)
		{
			float volume = GamePrefs.GetFloat(VolumeSetting);
			videoPlayer.SetDirectAudioVolume(0, volume);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateRenderTexture()
	{
		Vector3[] worldCorners = uiTexture.worldCorners;
		Vector3 vector = xui.playerUI.camera.WorldToScreenPoint(worldCorners[2]);
		Vector3 vector2 = xui.playerUI.camera.WorldToScreenPoint(worldCorners[0]);
		int num = Mathf.RoundToInt(vector.x - vector2.x);
		int num2 = Mathf.RoundToInt(vector.y - vector2.y);
		if (num >= 2 && num2 >= 2 && (!(rt != null) || rt.width != num || rt.height != num2))
		{
			destroyRenderTexture();
			rt = new RenderTexture(num, num2, 16);
			rt.Create();
			RenderTexture renderTexture = (videoPlayer.targetTexture = rt);
			Texture = renderTexture;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void destroyRenderTexture()
	{
		if (!(rt == null))
		{
			RenderTexture renderTexture = (videoPlayer.targetTexture = null);
			Texture = renderTexture;
			rt.Release();
			UnityEngine.Object.Destroy(rt);
			rt = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoPrepared(VideoPlayer _source)
	{
		videoPlayer.Play();
		updateVolumeLevel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoErrorReceived(VideoPlayer _source, string _message)
	{
		Log.Error("[XUi] Video player encountered an error. Skipping video. Message: " + _message);
		this.VideoError?.Invoke(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoFinished(VideoPlayer _source)
	{
		this.VideoFinished?.Invoke(this);
	}
}
