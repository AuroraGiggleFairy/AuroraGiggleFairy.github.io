using System;
using UnityEngine;

public class vp_SimpleHUD : MonoBehaviour
{
	public bool ShowHUD = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPPlayerEventHandler m_Player;

	public Font BigFont;

	public Font SmallFont;

	public Font MessageFont;

	public float BigFontOffset = 69f;

	public float SmallFontOffset = 56f;

	public Texture Background;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 m_DrawPos = Vector2.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 m_DrawSize = Vector2.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Rect m_DrawLabelRect = new Rect(0f, 0f, 0f, 0f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Rect m_DrawShadowRect = new Rect(0f, 0f, 0f, 0f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_TargetHealthOffset;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_CurrentHealthOffset;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_TargetAmmoOffset = 200f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_CurrentAmmoOffset = 200f;

	public Texture2D HealthIcon;

	public float HealthMultiplier = 10f;

	public Color HealthColor = Color.white;

	public float HealthLowLevel = 2.5f;

	public Color HealthLowColor = new Color(0.75f, 0f, 0f, 1f);

	public AudioClip HealthLowSound;

	public float HealthLowSoundInterval = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_FormattedHealth;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_NextAllowedPlayHealthLowSoundTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioSource m_Audio;

	public Color AmmoColor = Color.white;

	public Color AmmoLowColor = new Color(0f, 0f, 0f, 1f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public string m_PickupMessage = "";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Color m_MessageColor = new Color(1f, 1f, 1f, 2f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Color m_InvisibleColor = new Color(0f, 0f, 0f, 0f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Color m_TranspBlack = new Color(0f, 0f, 0f, 0.5f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Color m_TranspWhite = new Color(1f, 1f, 1f, 0.5f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static GUIStyle m_MessageStyle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public GUIStyle m_HealthStyle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public GUIStyle m_AmmoStyle;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public GUIStyle m_AmmoStyleSmall;

	public float m_HealthWidth
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return HealthStyle.CalcSize(new GUIContent(FormattedHealth)).x;
		}
	}

	public GUIStyle MessageStyle
	{
		get
		{
			if (m_MessageStyle == null)
			{
				m_MessageStyle = new GUIStyle("Label");
				m_MessageStyle.alignment = TextAnchor.MiddleCenter;
				m_MessageStyle.font = MessageFont;
			}
			return m_MessageStyle;
		}
	}

	public GUIStyle HealthStyle
	{
		get
		{
			if (m_HealthStyle == null)
			{
				m_HealthStyle = new GUIStyle("Label");
				m_HealthStyle.font = BigFont;
				m_HealthStyle.alignment = TextAnchor.MiddleRight;
				m_HealthStyle.fontSize = 28;
				m_HealthStyle.wordWrap = false;
			}
			return m_HealthStyle;
		}
	}

	public GUIStyle AmmoStyle
	{
		get
		{
			if (m_AmmoStyle == null)
			{
				m_AmmoStyle = new GUIStyle("Label");
				m_AmmoStyle.font = BigFont;
				m_AmmoStyle.alignment = TextAnchor.MiddleRight;
				m_AmmoStyle.fontSize = 28;
				m_AmmoStyle.wordWrap = false;
			}
			return m_AmmoStyle;
		}
	}

	public GUIStyle AmmoStyleSmall
	{
		get
		{
			if (m_AmmoStyleSmall == null)
			{
				m_AmmoStyleSmall = new GUIStyle("Label");
				m_AmmoStyleSmall.font = SmallFont;
				m_AmmoStyleSmall.alignment = TextAnchor.UpperLeft;
				m_AmmoStyleSmall.fontSize = 15;
				m_AmmoStyleSmall.wordWrap = false;
			}
			return m_AmmoStyleSmall;
		}
	}

	public string FormattedHealth
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			m_FormattedHealth = m_Player.Health.Get() * HealthMultiplier;
			if (m_FormattedHealth < 1f)
			{
				m_FormattedHealth = (m_Player.Dead.Active ? Mathf.Min(m_FormattedHealth, 0f) : 1f);
			}
			if (m_Player.Dead.Active && m_FormattedHealth > 0f)
			{
				m_FormattedHealth = 0f;
			}
			return ((int)m_FormattedHealth).ToString();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		m_Player = base.transform.GetComponent<vp_FPPlayerEventHandler>();
		m_Audio = m_Player.transform.GetComponent<AudioSource>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (m_Player != null)
		{
			m_Player.Register(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		if (m_Player != null)
		{
			m_Player.Unregister(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		m_CurrentAmmoOffset = Mathf.SmoothStep(m_CurrentAmmoOffset, m_TargetAmmoOffset, Time.deltaTime * 10f);
		m_CurrentHealthOffset = Mathf.SmoothStep(m_CurrentHealthOffset, m_TargetHealthOffset, Time.deltaTime * 10f);
		if (m_Player.CurrentWeaponIndex.Get() == 0 || m_Player.CurrentWeaponType.Get() == 2)
		{
			m_TargetAmmoOffset = 200f;
		}
		else
		{
			m_TargetAmmoOffset = 10f;
		}
		if (m_Player.Dead.Active)
		{
			HealthColor = Color.black;
		}
		else if (m_Player.Health.Get() < HealthLowLevel)
		{
			HealthColor = Color.Lerp(Color.white, HealthLowColor, vp_MathUtility.Sinus(6f, 0.1f) * 5f + 0.5f);
			if (HealthLowSound != null && Time.time >= m_NextAllowedPlayHealthLowSoundTime)
			{
				m_NextAllowedPlayHealthLowSoundTime = Time.time + HealthLowSoundInterval;
				m_Audio.pitch = 1f;
				m_Audio.PlayOneShot(HealthLowSound);
			}
		}
		else
		{
			HealthColor = Color.white;
		}
		if (m_Player.CurrentWeaponAmmoCount.Get() < 1 && m_Player.CurrentWeaponType.Get() != 3)
		{
			AmmoColor = Color.Lerp(Color.white, AmmoLowColor, vp_MathUtility.Sinus(8f, 0.1f) * 5f + 0.5f);
		}
		else
		{
			AmmoColor = Color.white;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnGUI()
	{
		if (ShowHUD)
		{
			DrawHealth();
			DrawAmmo();
			DrawText();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawHealth()
	{
		DrawLabel("", new Vector2(m_CurrentHealthOffset, Screen.height - 68), new Vector2(80f + m_HealthWidth, 52f), AmmoStyle, Color.white, m_TranspBlack, null);
		if (HealthIcon != null)
		{
			DrawLabel("", new Vector2(m_CurrentHealthOffset + 10f, Screen.height - 58), new Vector2(32f, 32f), AmmoStyle, Color.white, HealthColor, HealthIcon);
		}
		DrawLabel(FormattedHealth, new Vector2(m_CurrentHealthOffset - 18f - (45f - m_HealthWidth), (float)Screen.height - BigFontOffset), new Vector2(110f, 60f), HealthStyle, HealthColor, Color.clear, null);
		DrawLabel("%", new Vector2(m_CurrentHealthOffset + 50f + m_HealthWidth, (float)Screen.height - SmallFontOffset), new Vector2(110f, 60f), AmmoStyleSmall, HealthColor, Color.clear, null);
		GUI.color = Color.white;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawAmmo()
	{
		if (m_Player.CurrentWeaponType.Get() == 3)
		{
			DrawLabel("", new Vector2(m_CurrentAmmoOffset + (float)Screen.width - 93f - AmmoStyle.CalcSize(new GUIContent(m_Player.CurrentWeaponAmmoCount.Get().ToString())).x, Screen.height - 68), new Vector2(200f, 52f), AmmoStyle, AmmoColor, m_TranspBlack, null);
			if (m_Player.CurrentAmmoIcon.Get() != null)
			{
				DrawLabel("", new Vector2(m_CurrentAmmoOffset + (float)Screen.width - 83f - AmmoStyle.CalcSize(new GUIContent(m_Player.CurrentWeaponAmmoCount.Get().ToString())).x, Screen.height - 58), new Vector2(32f, 32f), AmmoStyle, Color.white, AmmoColor, m_Player.CurrentAmmoIcon.Get());
			}
			DrawLabel((m_Player.CurrentWeaponAmmoCount.Get() + m_Player.CurrentWeaponClipCount.Get()).ToString(), new Vector2(m_CurrentAmmoOffset + (float)Screen.width - 145f, (float)Screen.height - BigFontOffset), new Vector2(110f, 60f), AmmoStyle, AmmoColor, Color.clear, null);
			return;
		}
		DrawLabel("", new Vector2(m_CurrentAmmoOffset + (float)Screen.width - 115f - AmmoStyle.CalcSize(new GUIContent(m_Player.CurrentWeaponAmmoCount.Get().ToString())).x, Screen.height - 68), new Vector2(200f, 52f), AmmoStyle, AmmoColor, m_TranspBlack, null);
		if (m_Player.CurrentAmmoIcon.Get() != null)
		{
			DrawLabel("", new Vector2(m_CurrentAmmoOffset + (float)Screen.width - 105f - AmmoStyle.CalcSize(new GUIContent(m_Player.CurrentWeaponAmmoCount.Get().ToString())).x, Screen.height - 58), new Vector2(32f, 32f), AmmoStyle, Color.white, AmmoColor, m_Player.CurrentAmmoIcon.Get());
		}
		DrawLabel(m_Player.CurrentWeaponAmmoCount.Get().ToString(), new Vector2(m_CurrentAmmoOffset + (float)Screen.width - 177f, (float)Screen.height - BigFontOffset), new Vector2(110f, 60f), AmmoStyle, AmmoColor, Color.clear, null);
		DrawLabel("/ " + m_Player.CurrentWeaponClipCount.Get(), new Vector2(m_CurrentAmmoOffset + (float)Screen.width - 60f, (float)Screen.height - SmallFontOffset), new Vector2(110f, 60f), AmmoStyleSmall, AmmoColor, Color.clear, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawText()
	{
		if (m_PickupMessage != null && !(m_MessageColor.a < 0.01f))
		{
			m_MessageColor = Color.Lerp(m_MessageColor, m_InvisibleColor, Time.deltaTime * 0.4f);
			GUI.color = m_MessageColor;
			GUI.Box(new Rect(200f, 150f, Screen.width - 400, Screen.height - 400), m_PickupMessage, MessageStyle);
			GUI.color = Color.white;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_HUDText(string message)
	{
		m_MessageColor = Color.white;
		m_PickupMessage = message;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawLabel(string text, Vector2 position, Vector2 scale, GUIStyle textStyle, Color textColor, Color bgColor, Texture texture)
	{
		if (texture == null)
		{
			texture = Background;
		}
		if (scale.x == 0f)
		{
			scale.x = textStyle.CalcSize(new GUIContent(text)).x;
		}
		if (scale.y == 0f)
		{
			scale.y = textStyle.CalcSize(new GUIContent(text)).y;
		}
		m_DrawLabelRect.x = (m_DrawPos.x = position.x);
		m_DrawLabelRect.y = (m_DrawPos.y = position.y);
		m_DrawLabelRect.width = (m_DrawSize.x = scale.x);
		m_DrawLabelRect.height = (m_DrawSize.y = scale.y);
		if (bgColor != Color.clear)
		{
			GUI.color = bgColor;
			if (texture != null)
			{
				GUI.DrawTexture(m_DrawLabelRect, texture);
			}
		}
		GUI.color = textColor;
		GUI.Label(m_DrawLabelRect, text, textStyle);
		GUI.color = Color.white;
		m_DrawPos.x += m_DrawSize.x;
		m_DrawPos.y += m_DrawSize.y;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnStart_SetWeapon()
	{
		m_TargetAmmoOffset = 200f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnStop_SetWeapon()
	{
		m_TargetAmmoOffset = 10f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnStop_Dead()
	{
		m_CurrentHealthOffset = -200f;
		m_TargetHealthOffset = 0f;
		HealthColor = Color.white;
	}
}
