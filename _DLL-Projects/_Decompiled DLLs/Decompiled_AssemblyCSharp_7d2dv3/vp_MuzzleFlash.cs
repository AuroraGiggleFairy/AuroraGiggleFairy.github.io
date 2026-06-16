using System;
using UnityEngine;

public class vp_MuzzleFlash : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_FadeSpeed = 0.075f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_ForceShow;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Color m_Color = new Color(1f, 1f, 1f, 0f);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Transform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Light m_Light;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_LightIntensity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Renderer m_Renderer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Material m_Material;

	public float FadeSpeed
	{
		get
		{
			return m_FadeSpeed;
		}
		set
		{
			m_FadeSpeed = value;
		}
	}

	public bool ForceShow
	{
		get
		{
			return m_ForceShow;
		}
		set
		{
			m_ForceShow = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		m_Transform = base.transform;
		m_ForceShow = false;
		m_Light = GetComponent<Light>();
		if (m_Light != null)
		{
			m_LightIntensity = m_Light.intensity;
			m_Light.intensity = 0f;
		}
		m_Renderer = GetComponent<Renderer>();
		if (m_Renderer != null)
		{
			m_Material = GetComponent<Renderer>().material;
			if (m_Material != null)
			{
				m_Color = m_Material.GetColor("_TintColor");
				m_Color.a = 0f;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		GameObject gameObject = GameObject.Find("WeaponCamera");
		if (gameObject != null && gameObject.transform.parent == m_Transform.parent)
		{
			base.gameObject.layer = 10;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (m_ForceShow)
		{
			Show();
		}
		else if (m_Color.a > 0f)
		{
			m_Color.a -= m_FadeSpeed * (Time.deltaTime * 60f);
			if (m_Light != null)
			{
				m_Light.intensity = m_LightIntensity * (m_Color.a * 2f);
			}
		}
		if (m_Material != null)
		{
			m_Material.SetColor("_TintColor", m_Color);
		}
		if (m_Color.a < 0.01f)
		{
			m_Renderer.enabled = false;
			if (m_Light != null)
			{
				m_Light.enabled = false;
			}
		}
	}

	public void Show()
	{
		m_Renderer.enabled = true;
		if (m_Light != null)
		{
			m_Light.enabled = true;
			m_Light.intensity = m_LightIntensity;
		}
		m_Color.a = 0.5f;
	}

	public void Shoot()
	{
		ShootInternal(showMesh: true);
	}

	public void ShootLightOnly()
	{
		ShootInternal(showMesh: false);
	}

	public void ShootInternal(bool showMesh)
	{
		m_Color.a = 0.5f;
		if (showMesh)
		{
			m_Transform.Rotate(0f, 0f, UnityEngine.Random.Range(0, 360));
			m_Renderer.enabled = true;
		}
		if (m_Light != null)
		{
			m_Light.enabled = true;
			m_Light.intensity = m_LightIntensity;
		}
	}

	public void SetFadeSpeed(float fadeSpeed)
	{
		FadeSpeed = fadeSpeed;
	}
}
