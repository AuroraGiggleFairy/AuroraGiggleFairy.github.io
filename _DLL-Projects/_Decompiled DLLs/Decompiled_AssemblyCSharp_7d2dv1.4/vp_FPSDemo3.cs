using System;
using UnityEngine;

public class vp_FPSDemo3 : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_FPCamera m_FPSCamera;

	public GameObject PlayerGameObject;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_FPSDemoManager m_Demo;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_BodyAnimator m_BodyAnimator;

	public Texture ImageLeftArrow;

	public Texture ImageRightArrow;

	public Texture ImageCheckmark;

	public Texture ImagePresetDialogs;

	public Texture ImageShooter;

	public Texture ImageAllParams;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_StartPos = new Vector3(113f, 106f, -87f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_OverviewPos = new Vector3(113f, 106f, -87f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_OutroPos = new Vector3(135f, 105.8f, -70.7f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_StartAngle = new Vector2(13f, 153.5f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_OverviewAngle = new Vector2(13f, 153.5f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 m_OutroAngle = new Vector2(-19.3f, 241.7f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float m_OutroStartTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_LoadingNextLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		m_FPSCamera = (vp_FPCamera)UnityEngine.Object.FindObjectOfType(typeof(vp_FPCamera));
		m_Demo = new vp_FPSDemoManager(PlayerGameObject);
		m_Demo.CurrentFullScreenFadeTime = Time.time;
		m_Demo.DrawCrosshair = false;
		m_Demo.FadeGUIOnCursorLock = false;
		m_Demo.Input.MouseCursorZones = new Rect[2];
		m_Demo.Input.MouseCursorZones[0] = new Rect((float)Screen.width * 0.5f - 370f, 40f, 80f, 80f);
		m_Demo.Input.MouseCursorZones[1] = new Rect((float)Screen.width * 0.5f + 290f, 40f, 80f, 80f);
		vp_Utility.LockCursor = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		m_Demo.Update();
		if (Vector3.Distance(PlayerGameObject.transform.position, m_StartPos) > 100f)
		{
			m_Demo.Teleport(m_StartPos, m_StartAngle);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoIntro()
	{
		if (m_Demo.FirstFrame)
		{
			m_Demo.FirstFrame = false;
			m_Demo.DrawCrosshair = false;
			m_Demo.FreezePlayer(m_OverviewPos, m_OverviewAngle, freezeCamera: true);
			m_Demo.Input.MouseCursorForced = true;
			m_BodyAnimator = (vp_BodyAnimator)UnityEngine.Object.FindObjectOfType(typeof(vp_BodyAnimator));
			if (m_BodyAnimator != null)
			{
				m_BodyAnimator.gameObject.SetActive(value: false);
			}
		}
		m_Demo.DrawBoxes("welcome", "Featuring the SMOOTHEST CONTROLS and the most POWERFUL FPS CAMERA\navailable for Unity, Ultimate FPS is an awesome script pack for achieving that special\n 'AAA FPS' feeling. This demo will walk you through some of its core features ...\n", null, ImageRightArrow);
		m_Demo.ForceCameraShake();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoGameplay()
	{
		if (m_Demo.FirstFrame)
		{
			m_Demo.FirstFrame = false;
			m_Demo.DrawCrosshair = true;
			m_Demo.UnFreezePlayer();
			m_Demo.Teleport(m_StartPos, m_StartAngle);
			vp_Utility.LockCursor = true;
			m_Demo.Input.MouseCursorForced = false;
			if (m_BodyAnimator != null)
			{
				m_BodyAnimator.gameObject.SetActive(value: true);
			}
		}
		m_Demo.DrawBoxes("part i: some examples", "This level has some basic gameplay features.\n• Press SHIFT to SPRINT, C to CROUCH, and the RIGHT MOUSE BUTTON to AIM.\n• To SWITCH WEAPONS, press Q, E or 1-3.\n• Press R to RELOAD, F to INTERACT and V for 3RD PERSON.", ImageLeftArrow, ImageRightArrow, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			m_Demo.LoadLevel(1);
		});
		if (m_Demo.ShowGUI && vp_Utility.LockCursor && !m_LoadingNextLevel && !m_Demo.ClosingDown)
		{
			GUI.color = new Color(1f, 1f, 1f, m_Demo.ClosingDown ? m_Demo.GlobalAlpha : 1f);
			GUI.Label(new Rect(Screen.width / 2 - 200, 140f, 400f, 20f), "(Press ENTER to reenable mouse cursor.)", m_Demo.CenterStyle);
			GUI.color = new Color(1f, 1f, 1f, 1f * m_Demo.GlobalAlpha);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoOutro()
	{
		if (m_Demo.FirstFrame)
		{
			m_Demo.FirstFrame = false;
			m_Demo.DrawCrosshair = false;
			m_Demo.FreezePlayer(m_OutroPos, m_OutroAngle, freezeCamera: true);
			m_Demo.Input.MouseCursorForced = true;
			m_OutroStartTime = Time.time;
		}
		m_FPSCamera.Angle = new Vector2(m_OutroAngle.x, m_OutroAngle.y + Mathf.Cos((Time.time - m_OutroStartTime + 50f) * 0.03f) * 20f);
		m_Demo.DrawBoxes("putting it all together", "Included in the package is full, well commented C# source code, an in-depth 70-page MANUAL in PDF format, a game-ready FPS PLAYER prefab along with all the scripts and content used in this demo. A FANTASTIC starting point (or upgrade) for any FPS project.\nBest part? It can be yours in a minute. GET IT NOW on visionpunk.com!", ImageLeftArrow, ImageCheckmark, [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			m_Demo.LoadLevel(0);
		});
		m_Demo.DrawImage(ImageAllParams);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGUI()
	{
		m_Demo.OnGUI();
		switch (m_Demo.CurrentScreen)
		{
		case 1:
			DemoIntro();
			break;
		case 2:
			DemoGameplay();
			break;
		case 3:
			DemoOutro();
			break;
		}
	}
}
