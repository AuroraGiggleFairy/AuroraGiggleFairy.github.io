using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LaunchSceneScript : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string MainSceneName = "SceneGame";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string SplashSceneName = "SceneSplash";

	public UIPanel fadeInUIPanel;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float fadeInDuration = 0.6f;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		Cursor.visible = false;
		StartCoroutine(GoToNextSceneCo());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator GoToNextSceneCo()
	{
		string nextScene;
		bool flag;
		if (GameStartupHelper.GetCommandLineArgs().ContainsCaseInsensitive("-skipintro"))
		{
			nextScene = "SceneGame";
			flag = true;
		}
		else
		{
			nextScene = "SceneSplash";
			flag = false;
		}
		fadeInUIPanel.alpha = 0f;
		if (flag)
		{
			float timer = 0.6f;
			while (timer > 0f)
			{
				fadeInUIPanel.alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01(timer / 0.6f));
				timer -= Time.deltaTime;
				yield return null;
			}
			fadeInUIPanel.alpha = 1f;
		}
		yield return new WaitForEndOfFrame();
		yield return GameEntrypoint.EntrypointCoroutine();
		if (GameEntrypoint.EntrypointSuccess)
		{
			SceneManager.LoadScene(nextScene);
		}
	}
}
