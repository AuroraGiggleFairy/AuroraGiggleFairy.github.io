using GameSparks.Platforms;
using UnityEngine;

public class GameSparksUnity : MonoBehaviour
{
	public GameSparksSettings settings;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		base.gameObject.AddComponent<DefaultPlatform>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGUI()
	{
		if (GameSparksSettings.PreviewBuild)
		{
			GUILayout.BeginArea(new Rect(0f, 0f, Screen.width, Screen.height));
			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.Space(10f);
			GUILayout.Label("GameSparks Preview mode", GUILayout.Width(200f), GUILayout.Height(25f));
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}
