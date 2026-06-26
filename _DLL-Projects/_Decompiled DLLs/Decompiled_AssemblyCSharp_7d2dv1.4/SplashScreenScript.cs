using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SplashScreenScript : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string MainSceneName = "SceneGame";

	public Transform wdwSplashScreen;

	public UILabel labelEaWarning;

	public VideoPlayer videoPlayer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool videoFinished;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		if (GameEntrypoint.EntrypointSuccess)
		{
			if (GameManager.IsDedicatedServer)
			{
				SceneManager.LoadScene(MainSceneName);
				return;
			}
			if (GameUtils.GetLaunchArgument("skipintro") != null)
			{
				SceneManager.LoadScene(MainSceneName);
				return;
			}
			GameOptionsManager.ApplyTextureQuality();
			labelEaWarning.text = Localization.Get("splashMessageEarlyAccessWarning");
			videoPlayer.prepareCompleted += OnVideoPrepared;
			videoPlayer.loopPointReached += OnVideoFinished;
			videoPlayer.errorReceived += OnVideoErrorReceived;
			videoPlayer.url = Application.streamingAssetsPath + "/Video/TFP_Intro.webm";
			videoPlayer.Prepare();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if ((videoPlayer.isPlaying && Input.anyKey) || videoFinished)
		{
			SceneManager.LoadScene(MainSceneName);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnVideoPrepared(VideoPlayer player)
	{
		StartCoroutine(DelayVideoRoutine());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator DelayVideoRoutine()
	{
		yield return new WaitForSecondsRealtime(0.3f);
		videoPlayer.Play();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnVideoFinished(VideoPlayer player)
	{
		videoFinished = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoErrorReceived(VideoPlayer player, string message)
	{
		Log.Error("SplashScreen video error: " + message);
		videoFinished = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnGUI()
	{
		GUI.contentColor = new Color(0f, 0f, 0f, 0f);
		GUILayout.Label("Test");
	}
}
