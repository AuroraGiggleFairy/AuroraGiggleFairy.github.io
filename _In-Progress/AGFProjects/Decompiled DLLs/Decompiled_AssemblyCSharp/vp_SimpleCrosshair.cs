using System;
using UnityEngine;

public class vp_SimpleCrosshair : MonoBehaviour
{
	public Texture m_ImageCrosshair;

	public bool HideOnFirstPersonZoom = true;

	public bool HideOnDeath = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_FPPlayerEventHandler m_Player;

	public virtual Texture OnValue_Crosshair
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return m_ImageCrosshair;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			m_ImageCrosshair = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		m_Player = UnityEngine.Object.FindObjectOfType(typeof(vp_FPPlayerEventHandler)) as vp_FPPlayerEventHandler;
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
	public void OnGUI()
	{
		if (!(m_ImageCrosshair == null) && (!HideOnFirstPersonZoom || !m_Player.Zoom.Active || !m_Player.IsFirstPerson.Get()) && (!HideOnDeath || !m_Player.Dead.Active))
		{
			GUI.color = new Color(1f, 1f, 1f, 0.8f);
			GUI.DrawTexture(new Rect((float)Screen.width * 0.5f - (float)m_ImageCrosshair.width * 0.5f, (float)Screen.height * 0.5f - (float)m_ImageCrosshair.height * 0.5f, m_ImageCrosshair.width, m_ImageCrosshair.height), m_ImageCrosshair);
			GUI.color = Color.white;
		}
	}
}
