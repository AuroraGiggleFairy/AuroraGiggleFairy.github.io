using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class vp_FPEarthquake : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_CameraEarthQuakeForce;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_Endtime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 m_Magnitude = Vector2.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_FPPlayerEventHandler m_FPPlayer;

	public vp_FPPlayerEventHandler FPPlayer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (m_FPPlayer == null)
			{
				m_FPPlayer = UnityEngine.Object.FindObjectOfType(typeof(vp_FPPlayerEventHandler)) as vp_FPPlayerEventHandler;
			}
			return m_FPPlayer;
		}
	}

	public virtual Vector3 OnValue_CameraEarthQuakeForce
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return m_CameraEarthQuakeForce;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			m_CameraEarthQuakeForce = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (FPPlayer != null)
		{
			FPPlayer.Register(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		if (FPPlayer != null)
		{
			FPPlayer.Unregister(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void FixedUpdate()
	{
		if (Time.timeScale != 0f)
		{
			UpdateEarthQuake();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateEarthQuake()
	{
		if (!FPPlayer.CameraEarthQuake.Active)
		{
			m_CameraEarthQuakeForce = Vector3.zero;
			return;
		}
		m_CameraEarthQuakeForce = Vector3.Scale(vp_SmoothRandom.GetVector3Centered(1f), m_Magnitude.x * (Vector3.right + Vector3.forward) * Mathf.Min(m_Endtime - Time.time, 1f) * Time.timeScale);
		m_CameraEarthQuakeForce.y = 0f;
		if (UnityEngine.Random.value < 0.3f * Time.timeScale)
		{
			m_CameraEarthQuakeForce.y = UnityEngine.Random.Range(0f, m_Magnitude.y * 0.35f) * Mathf.Min(m_Endtime - Time.time, 1f);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_CameraEarthQuake()
	{
		Vector3 vector = (Vector3)FPPlayer.CameraEarthQuake.Argument;
		m_Magnitude.x = vector.x;
		m_Magnitude.y = vector.y;
		m_Endtime = Time.time + vector.z;
		FPPlayer.CameraEarthQuake.AutoDuration = vector.z;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_CameraBombShake(float impact)
	{
		FPPlayer.CameraEarthQuake.TryStart(new Vector3(impact * 0.5f, impact * 0.5f, 1f));
	}
}
