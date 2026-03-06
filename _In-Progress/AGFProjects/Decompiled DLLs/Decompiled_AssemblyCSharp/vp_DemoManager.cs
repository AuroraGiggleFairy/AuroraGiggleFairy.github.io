using UnityEngine;
using UnityEngine.SceneManagement;

public class vp_DemoManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum FadeState
	{
		None,
		FadeOut,
		FadeIn
	}

	public delegate void LoadLevelCallback();

	public GUIStyle UpStyle;

	public GUIStyle LabelStyle;

	public GUIStyle DownStyle;

	public GUIStyle CenterStyle;

	public int CurrentScreen = 1;

	public Resolution DesktopResolution;

	public bool FirstFrame = true;

	public bool EditorPreviewSectionExpanded = true;

	public bool ShowGUI = true;

	public float ButtonColumnClickTime;

	public float ButtonColumnArrowY = -100f;

	public float ButtonColumnArrowFadeoutTime;

	public int ButtonSelection;

	public float LastInputTime;

	public bool FadeGUIOnCursorLock = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_FadeSpeed = 0.03f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_FadeToScreen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_StylesInitialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public FadeState m_FadeState;

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D m_FullScreenFadeTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_FullScreenFadeOutDuration = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_FullScreenFadeInDuration = 0.75f;

	public float CurrentFullScreenFadeTime;

	public bool ClosingDown;

	public float GlobalAlpha = 0.35f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_TextAlpha = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_EditorPreviewScreenshotTextAlpha;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_FullScreenTextAlpha = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_BigArrowFadeAlpha = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_SimulateLowFPS;

	public vp_DemoManager()
	{
		DesktopResolution = Screen.currentResolution;
		LastInputTime = Time.time;
		m_FullScreenFadeTexture = new Texture2D(1, 1, TextureFormat.RGB24, mipChain: false);
	}

	public virtual void Update()
	{
		if (double.IsNaN(Camera.main.fieldOfView))
		{
			Camera.main.fieldOfView = 60f;
		}
		if (Input.GetKeyDown(KeyCode.U))
		{
			Screen.fullScreen = !Screen.fullScreen;
		}
		if (Screen.fullScreen && Screen.currentResolution.width != DesktopResolution.width)
		{
			Screen.SetResolution(DesktopResolution.width, DesktopResolution.height, fullscreen: true);
		}
		if (Input.GetKeyDown(KeyCode.L))
		{
			m_SimulateLowFPS = !m_SimulateLowFPS;
		}
		if (m_SimulateLowFPS)
		{
			for (int i = 0; i < 20000000; i++)
			{
			}
		}
	}

	public bool ButtonToggle(Rect rect, string label, bool state, bool arrow, Texture imageUpPointer)
	{
		if (!ShowGUI)
		{
			return false;
		}
		GUIStyle style = UpStyle;
		GUIStyle style2 = DownStyle;
		float num = 0f;
		if (state)
		{
			style = DownStyle;
			style2 = UpStyle;
			num = rect.width * 0.5f + 2f;
		}
		GUI.Label(new Rect(rect.x, rect.y - 30f, rect.width, rect.height), label, CenterStyle);
		if (GUI.Button(new Rect(rect.x, rect.y, rect.width * 0.5f - 2f, rect.height), "OFF", style2))
		{
			state = false;
		}
		if (GUI.Button(new Rect(rect.x + rect.width * 0.5f + 2f, rect.y, rect.width * 0.5f, rect.height), "ON", style))
		{
			state = true;
		}
		if (arrow)
		{
			GUI.Label(new Rect(rect.x + rect.width * 0.5f * 0.5f - 14f + num, rect.y + rect.height, 32f, 32f), imageUpPointer);
		}
		return state;
	}

	public void DrawBoxes(string caption, string description, Texture imageLeftArrow, Texture imageRightArrow, LoadLevelCallback nextLevelCallback = null, LoadLevelCallback prevLevelCallback = null, bool drawBox = true)
	{
		if (!ShowGUI)
		{
			return;
		}
		GUI.color = new Color(1f, 1f, 1f, 1f * GlobalAlpha);
		GUILayout.BeginArea(new Rect((float)(Screen.width / 2) - 400f, 30f, 800f, 100f));
		if (imageLeftArrow != null)
		{
			GUI.Box(new Rect(30f, 10f, 80f, 80f), "");
		}
		if (drawBox)
		{
			GUI.Box(new Rect(120f, 0f, 560f, 100f), "");
		}
		GUI.color = new Color(1f, 1f, 1f, m_TextAlpha * GlobalAlpha);
		for (int i = 0; i < 3; i++)
		{
			GUILayout.BeginArea(new Rect(130f, 10f, 540f, 80f));
			GUILayout.Label("--- " + caption.ToUpper() + " ---\n" + description, LabelStyle);
			GUILayout.EndArea();
		}
		GUI.color = new Color(1f, 1f, 1f, 1f * GlobalAlpha);
		if (imageRightArrow != null)
		{
			GUI.Box(new Rect(690f, 10f, 80f, 80f), "");
		}
		if (imageLeftArrow != null && GUI.Button(new Rect(35f, 15f, 80f, 80f), imageLeftArrow, "Label"))
		{
			if (prevLevelCallback == null)
			{
				m_FadeToScreen = Mathf.Max(CurrentScreen - 1, 1);
				m_FadeState = FadeState.FadeOut;
			}
			else
			{
				prevLevelCallback();
			}
		}
		if (Time.time < LastInputTime + 30f)
		{
			m_BigArrowFadeAlpha = 1f;
		}
		else
		{
			m_BigArrowFadeAlpha = 0.5f - Mathf.Sin((Time.time - 0.5f) * 6f) * 1f;
		}
		GUI.color = new Color(1f, 1f, 1f, m_BigArrowFadeAlpha * GlobalAlpha);
		if (imageRightArrow != null && GUI.Button(new Rect(700f, 15f, 80f, 80f), imageRightArrow, "Label"))
		{
			if (nextLevelCallback == null)
			{
				m_FadeToScreen = CurrentScreen + 1;
				m_FadeState = FadeState.FadeOut;
			}
			else
			{
				nextLevelCallback();
			}
		}
		GUI.color = new Color(1f, 1f, 1f, 1f * GlobalAlpha);
		GUILayout.EndArea();
		GUI.color = new Color(1f, 1f, 1f, m_TextAlpha * GlobalAlpha);
	}

	public int ToggleColumn(int width, int y, int sel, string[] strings, bool center, bool arrow, Texture imageRightPointer, Texture imageLeftPointer)
	{
		if (!ShowGUI)
		{
			return 0;
		}
		float num = strings.Length * 30;
		Vector2 vector = new Vector2(Screen.width / 2, Screen.height / 2);
		Rect position = ((!center) ? new Rect(Screen.width - width - 10, vector.y - num / 2f, width, 30f) : new Rect(vector.x - (float)width, y, width, 30f));
		int num2 = 0;
		foreach (string text in strings)
		{
			if (center)
			{
				position.x = vector.x - (float)(width / 2);
			}
			else
			{
				position.x = 10f;
			}
			position.width = width;
			GUIStyle style = UpStyle;
			if (num2 == sel)
			{
				Color color = GUI.color;
				GUI.color = new Color(1f, 1f, 1f, 1f * GlobalAlpha);
				style = DownStyle;
				if (center)
				{
					position.x = vector.x - (float)(width / 2) + 10f;
				}
				else
				{
					position.x = 20f;
				}
				position.width = width - 20;
				if (arrow && !ClosingDown)
				{
					if (center)
					{
						GUI.Label(new Rect(position.x - 27f, position.y, 32f, 32f), imageRightPointer);
					}
					else
					{
						GUI.Label(new Rect(position.x + position.width + 5f, position.y, 32f, 32f), imageLeftPointer);
					}
				}
				GUI.color = color;
			}
			if (GUI.Button(position, text, style))
			{
				sel = num2;
			}
			position.y += 35f;
			num2++;
		}
		return sel;
	}

	public int ButtonColumn(int y, int sel, string[] strings, Texture imagePointer)
	{
		if (!ShowGUI)
		{
			return 0;
		}
		float num = Screen.width / 2;
		Rect position = new Rect(num - 100f, y, 200f, 30f);
		int num2 = 0;
		foreach (string text in strings)
		{
			position.x = num - 100f;
			position.width = 200f;
			if (GUI.Button(position, text))
			{
				sel = num2;
				ButtonColumnClickTime = Time.time;
				ButtonColumnArrowY = position.y;
			}
			position.y += 35f;
			num2++;
		}
		if (Time.time < ButtonColumnArrowFadeoutTime)
		{
			ButtonColumnClickTime = Time.time;
		}
		GUI.color = new Color(1f, 1f, 1f, Mathf.Max(0f, 1f - (Time.time - ButtonColumnClickTime) * 1f * GlobalAlpha));
		GUI.Label(new Rect(position.x - 27f, ButtonColumnArrowY, 32f, 32f), imagePointer);
		GUI.color = new Color(1f, 1f, 1f, 1f * GlobalAlpha);
		return sel;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Reset()
	{
		ButtonSelection = 0;
		FirstFrame = true;
		LastInputTime = Time.time;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitGUIStyles()
	{
		LabelStyle = new GUIStyle("Label");
		LabelStyle.alignment = TextAnchor.LowerCenter;
		UpStyle = new GUIStyle("Button");
		DownStyle = new GUIStyle("Button");
		DownStyle.normal = DownStyle.active;
		CenterStyle = new GUIStyle("Label");
		CenterStyle.alignment = TextAnchor.MiddleCenter;
		m_StylesInitialized = true;
	}

	public void DrawImage(Texture image, float xOffset, float yOffset)
	{
		if (ShowGUI && !(image == null))
		{
			float num = Screen.width / 2;
			float num2 = Mathf.Min(image.width, Screen.width);
			GUI.DrawTexture(new Rect(height: num2 * ((float)image.height / (float)image.width), x: num - num2 / 2f + xOffset, y: 140f + yOffset, width: num2), image);
		}
	}

	public void DrawImage(Texture image)
	{
		DrawImage(image, 0f, 0f);
	}

	public void DrawEditorPreview(Texture section, Texture imageEditorPreview, Texture imageEditorScreenshot)
	{
		if (!ShowGUI)
		{
			return;
		}
		Color color = GUI.color;
		Vector2 vector = new Vector2(Input.mousePosition.x, (float)Screen.height - Input.mousePosition.y);
		float num = 0f;
		if (EditorPreviewSectionExpanded)
		{
			Texture texture = imageEditorPreview;
			float num2 = Screen.height - section.height - texture.height;
			float num3 = Screen.height - section.height;
			GUI.DrawTexture(new Rect(num, num2, texture.width, texture.height), texture);
			GUI.DrawTexture(new Rect(num, num3, section.width, section.height), section);
			if (vector.x > num && vector.x < num + (float)section.width && vector.y > num2 && vector.y < (float)(Screen.height - texture.height))
			{
				m_EditorPreviewScreenshotTextAlpha = Mathf.Min(1f, m_EditorPreviewScreenshotTextAlpha + 0.01f);
				if (Input.GetMouseButtonDown(0))
				{
					EditorPreviewSectionExpanded = false;
				}
			}
			else
			{
				m_EditorPreviewScreenshotTextAlpha = Mathf.Max(0f, m_EditorPreviewScreenshotTextAlpha - 0.03f);
			}
			GUI.color = new Color(1f, 1f, 1f, color.a * 0.5f * m_EditorPreviewScreenshotTextAlpha * GlobalAlpha);
			GUI.DrawTexture(new Rect(num + 48f, num3 + (float)(section.height / 2) - (float)(imageEditorScreenshot.height / 2), imageEditorScreenshot.width, imageEditorScreenshot.height), imageEditorScreenshot);
		}
		else
		{
			Texture texture = imageEditorPreview;
			float num4 = Screen.height - texture.height;
			GUI.DrawTexture(new Rect(num, num4, texture.width, texture.height), texture);
			if (vector.x > num && vector.x < num + (float)section.width && vector.y > num4 && Input.GetMouseButtonUp(0))
			{
				EditorPreviewSectionExpanded = true;
			}
		}
		GUI.color = color;
	}

	public void DrawFullScreenText(Texture imageFullScreen)
	{
		if (ShowGUI && !(Time.realtimeSinceStartup > 5f))
		{
			if (Time.realtimeSinceStartup > 3f)
			{
				m_FullScreenTextAlpha -= m_FadeSpeed * Time.deltaTime * 15f;
			}
			GUI.color = new Color(1f, 1f, 1f, m_FullScreenTextAlpha * GlobalAlpha);
			GUI.DrawTexture(new Rect(Screen.width / 2 - 120, Screen.height / 2 - 16, 240f, 32f), imageFullScreen);
			GUI.color = new Color(1f, 1f, 1f, 1f * GlobalAlpha);
		}
	}

	public void DoScreenTransition()
	{
		if (!ShowGUI)
		{
			return;
		}
		if (m_FadeState == FadeState.FadeOut)
		{
			m_TextAlpha -= m_FadeSpeed;
			if (m_TextAlpha <= 0f)
			{
				m_TextAlpha = 0f;
				Reset();
				CurrentScreen = m_FadeToScreen;
				m_FadeState = FadeState.FadeIn;
			}
		}
		else if (m_FadeState == FadeState.FadeIn && !ClosingDown)
		{
			m_TextAlpha += m_FadeSpeed;
			if (m_TextAlpha >= 1f)
			{
				m_TextAlpha = 1f;
				m_FadeState = FadeState.None;
			}
		}
	}

	public void SetScreen(int screen)
	{
		m_FadeToScreen = screen;
		m_FadeState = FadeState.FadeOut;
	}

	public void OnGUI()
	{
		if (!m_StylesInitialized)
		{
			InitGUIStyles();
		}
		DoScreenTransition();
		if (vp_Utility.LockCursor && FadeGUIOnCursorLock)
		{
			GlobalAlpha = 0.35f;
		}
		else if (!ClosingDown)
		{
			GlobalAlpha = 1f;
		}
		if (Time.time - CurrentFullScreenFadeTime < m_FullScreenFadeInDuration)
		{
			GlobalAlpha = Time.time - CurrentFullScreenFadeTime;
			GUI.color = new Color(0f, 0f, 0f, (m_FullScreenFadeInDuration - GlobalAlpha) / m_FullScreenFadeInDuration);
			GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), m_FullScreenFadeTexture);
			GUI.color = Color.white;
		}
		if (CurrentFullScreenFadeTime > Time.time)
		{
			GlobalAlpha = CurrentFullScreenFadeTime - Time.time;
			GUI.color = new Color(0f, 0f, 0f, (m_FullScreenFadeOutDuration - GlobalAlpha) / m_FullScreenFadeOutDuration);
			GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), m_FullScreenFadeTexture);
			GUI.color = Color.white;
		}
	}

	public void LoadLevel(int level)
	{
		ClosingDown = true;
		vp_Timer.CancelAll();
		vp_TimeUtility.TimeScale = 1f;
		m_FadeState = FadeState.FadeOut;
		CurrentFullScreenFadeTime = Time.time + m_FullScreenFadeOutDuration;
		vp_Timer.In(m_FullScreenFadeOutDuration, [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			SceneManager.LoadScene(level);
		});
	}
}
