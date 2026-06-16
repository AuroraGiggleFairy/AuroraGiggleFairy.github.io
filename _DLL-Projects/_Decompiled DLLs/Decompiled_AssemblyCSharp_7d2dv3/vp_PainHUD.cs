using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_PainHUD : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public class Inflictor
	{
		public Transform Transform;

		public float DamageTime;

		public Inflictor(Transform transform, float damageTime)
		{
			Transform = transform;
			DamageTime = damageTime;
		}
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<Inflictor> m_Inflictors = new List<Inflictor>();

	public Texture PainTexture;

	public Texture DeathTexture;

	public Texture ArrowTexture;

	public float PainIntensity = 0.2f;

	[Range(0.01f, 0.5f)]
	public float ArrowScale = 0.083f;

	public float ArrowAngleOffset = -135f;

	public float ArrowVisibleDuration = 1.5f;

	public float ArrowShakeDuration = 0.125f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_LastInflictorTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Color m_PainColor = new Color(0.8f, 0f, 0f, 0f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Color m_ArrowColor = new Color(0.8f, 0f, 0f, 0f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Color m_FlashInvisibleColor = new Color(1f, 0f, 0f, 0f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Color m_SplatColor = Color.white;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Rect m_SplatRect;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_FPPlayerEventHandler m_Player;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		m_Player = base.transform.GetComponent<vp_FPPlayerEventHandler>();
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnGUI()
	{
		UpdatePainFlash();
		UpdateInflictorArrows();
		UpdateDeathTexture();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdatePainFlash()
	{
		if (m_PainColor.a < 0.01f)
		{
			m_PainColor.a = 0f;
			return;
		}
		m_PainColor = Color.Lerp(m_PainColor, m_FlashInvisibleColor, Time.deltaTime * 0.4f);
		GUI.color = m_PainColor;
		if (PainTexture != null)
		{
			GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), PainTexture);
		}
		GUI.color = Color.white;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateInflictorArrows()
	{
		if (ArrowTexture == null)
		{
			return;
		}
		for (int num = m_Inflictors.Count - 1; num > -1; num--)
		{
			if (m_Inflictors[num] == null || m_Inflictors[num].Transform == null || !vp_Utility.IsActive(m_Inflictors[num].Transform.gameObject))
			{
				m_Inflictors.Remove(m_Inflictors[num]);
			}
			else
			{
				m_ArrowColor.a = (ArrowVisibleDuration - (Time.time - m_Inflictors[num].DamageTime)) / ArrowVisibleDuration;
				if (!(m_ArrowColor.a < 0f))
				{
					Vector2 pivotPoint = new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
					float angle = vp_3DUtility.LookAtAngleHorizontal(base.transform.position, base.transform.forward, m_Inflictors[num].Transform.position) + ArrowAngleOffset;
					float num2 = (float)Screen.width * ArrowScale;
					float t = (ArrowShakeDuration - (Time.time - m_LastInflictorTime)) / ArrowShakeDuration;
					t = Mathf.Lerp(0f, 1f, t);
					num2 += (float)(Screen.width / 100) * t;
					Matrix4x4 matrix = GUI.matrix;
					GUIUtility.RotateAroundPivot(angle, pivotPoint);
					GUI.color = m_ArrowColor;
					GUI.DrawTexture(new Rect(pivotPoint.x, pivotPoint.y, num2, num2), ArrowTexture);
					GUI.matrix = matrix;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDeathTexture()
	{
		if (!(DeathTexture == null) && m_Player.Dead.Active)
		{
			GUI.color = m_SplatColor;
			GUI.DrawTexture(m_SplatRect, DeathTexture);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_HUDDamageFlash(vp_DamageInfo damageInfo)
	{
		if (damageInfo == null || damageInfo.Damage == 0f)
		{
			m_PainColor.a = 0f;
			return;
		}
		m_PainColor.a += damageInfo.Damage * PainIntensity;
		if (!(damageInfo.Source != null))
		{
			return;
		}
		m_LastInflictorTime = Time.time;
		bool flag = true;
		foreach (Inflictor inflictor in m_Inflictors)
		{
			if (inflictor.Transform == damageInfo.Source.transform)
			{
				inflictor.DamageTime = Time.time;
				flag = false;
			}
		}
		if (flag)
		{
			m_Inflictors.Add(new Inflictor(damageInfo.Source, Time.time));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnStart_Dead()
	{
		float num = UnityEngine.Random.value * 0.6f + 0.4f;
		m_SplatColor = new Color(num, num, num, 1f);
		float num2 = ((UnityEngine.Random.value < 0.5f) ? (Screen.width / UnityEngine.Random.Range(5, 10)) : (Screen.width / UnityEngine.Random.Range(4, 7)));
		m_SplatRect = new Rect(UnityEngine.Random.Range(0f - num2, 0f), UnityEngine.Random.Range(0f - num2, 0f), (float)Screen.width + num2, (float)Screen.height + num2);
		if (UnityEngine.Random.value < 0.5f)
		{
			m_SplatRect.x = (float)Screen.width - m_SplatRect.x;
			m_SplatRect.width = 0f - m_SplatRect.width;
		}
		if (UnityEngine.Random.value < 0.125f)
		{
			num *= 0.5f;
			m_SplatColor = new Color(num, num, num, 1f);
			m_SplatRect.y = (float)Screen.height - m_SplatRect.y;
			m_SplatRect.height = 0f - m_SplatRect.height;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnStop_Dead()
	{
		m_PainColor.a = 0f;
		for (int num = m_Inflictors.Count - 1; num > -1; num--)
		{
			m_Inflictors[num].DamageTime = 0f;
		}
	}
}
