using UnityEngine;

public class vp_Spring
{
	public enum UpdateMode
	{
		Position,
		PositionAdditiveLocal,
		PositionAdditiveGlobal,
		PositionAdditiveSelf,
		Rotation,
		RotationAdditiveLocal,
		RotationAdditiveGlobal,
		Scale,
		ScaleAdditiveLocal
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public delegate void UpdateDelegate();

	[PublicizedFrom(EAccessModifier.Protected)]
	public UpdateMode Mode;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_AutoUpdate = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UpdateDelegate m_UpdateFunc;

	public Vector3 State = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_Velocity = Vector3.zero;

	public Vector3 RestState = Vector3.zero;

	public Vector3 Stiffness = new Vector3(0.5f, 0.5f, 0.5f);

	public Vector3 Damping = new Vector3(0.75f, 0.75f, 0.75f);

	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_VelocityFadeInCap = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_VelocityFadeInEndTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_VelocityFadeInLength;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3[] m_SoftForceFrame = new Vector3[120];

	public float MaxVelocity = 10000f;

	public float MinVelocity = 1E-07f;

	public Vector3 MaxState = new Vector3(10000f, 10000f, 10000f);

	public Vector3 MinState = new Vector3(-10000f, -10000f, -10000f);

	public float SdtdMinDeltaState = 0.0001f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Transform;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool SdtdStopping { get; set; }

	public Transform Transform
	{
		get
		{
			return m_Transform;
		}
		set
		{
			m_Transform = value;
			RefreshUpdateMode();
		}
	}

	public vp_Spring(Transform transform, UpdateMode mode, bool autoUpdate = true)
	{
		Mode = mode;
		Transform = transform;
		m_AutoUpdate = autoUpdate;
	}

	public void FixedUpdate()
	{
		if (m_VelocityFadeInEndTime > Time.time)
		{
			m_VelocityFadeInCap = Mathf.Clamp01(1f - (m_VelocityFadeInEndTime - Time.time) / m_VelocityFadeInLength);
		}
		else
		{
			m_VelocityFadeInCap = 1f;
		}
		if (m_SoftForceFrame[0] != Vector3.zero)
		{
			AddForceInternal(m_SoftForceFrame[0]);
			for (int i = 0; i < 120; i++)
			{
				m_SoftForceFrame[i] = ((i < 119) ? m_SoftForceFrame[i + 1] : Vector3.zero);
				if (m_SoftForceFrame[i] == Vector3.zero)
				{
					break;
				}
			}
		}
		Calculate();
		m_UpdateFunc();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Position()
	{
		m_Transform.localPosition = State;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Rotation()
	{
		m_Transform.localEulerAngles = State;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Scale()
	{
		m_Transform.localScale = State;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PositionAdditiveLocal()
	{
		m_Transform.localPosition += State;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PositionAdditiveGlobal()
	{
		m_Transform.position += State;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PositionAdditiveSelf()
	{
		m_Transform.Translate(State, m_Transform);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RotationAdditiveLocal()
	{
		m_Transform.localEulerAngles += State;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RotationAdditiveGlobal()
	{
		m_Transform.eulerAngles += State;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ScaleAdditiveLocal()
	{
		m_Transform.localScale += State;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void None()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void RefreshUpdateMode()
	{
		m_UpdateFunc = None;
		switch (Mode)
		{
		case UpdateMode.Position:
			State = m_Transform.localPosition;
			if (m_AutoUpdate)
			{
				m_UpdateFunc = Position;
			}
			break;
		case UpdateMode.Rotation:
			State = m_Transform.localEulerAngles;
			if (m_AutoUpdate)
			{
				m_UpdateFunc = Rotation;
			}
			break;
		case UpdateMode.Scale:
			State = m_Transform.localScale;
			if (m_AutoUpdate)
			{
				m_UpdateFunc = Scale;
			}
			break;
		case UpdateMode.PositionAdditiveLocal:
			State = m_Transform.localPosition;
			if (m_AutoUpdate)
			{
				m_UpdateFunc = PositionAdditiveLocal;
			}
			break;
		case UpdateMode.PositionAdditiveGlobal:
			State = m_Transform.position;
			if (m_AutoUpdate)
			{
				m_UpdateFunc = PositionAdditiveGlobal;
			}
			break;
		case UpdateMode.RotationAdditiveLocal:
			State = m_Transform.localEulerAngles;
			if (m_AutoUpdate)
			{
				m_UpdateFunc = RotationAdditiveLocal;
			}
			break;
		case UpdateMode.RotationAdditiveGlobal:
			State = m_Transform.eulerAngles;
			if (m_AutoUpdate)
			{
				m_UpdateFunc = RotationAdditiveGlobal;
			}
			break;
		case UpdateMode.PositionAdditiveSelf:
			State = m_Transform.position;
			if (m_AutoUpdate)
			{
				m_UpdateFunc = PositionAdditiveSelf;
			}
			break;
		case UpdateMode.ScaleAdditiveLocal:
			State = m_Transform.localScale;
			if (m_AutoUpdate)
			{
				m_UpdateFunc = ScaleAdditiveLocal;
			}
			break;
		}
		RestState = State;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Calculate()
	{
		bool num;
		if (SdtdStopping)
		{
			if (!(m_Velocity.sqrMagnitude <= MinVelocity * MinVelocity))
			{
				goto IL_0066;
			}
			num = (RestState - State).sqrMagnitude <= SdtdMinDeltaState * SdtdMinDeltaState;
		}
		else
		{
			num = State == RestState;
		}
		if (num)
		{
			return;
		}
		goto IL_0066;
		IL_0066:
		m_Velocity += Vector3.Scale(RestState - State, Stiffness);
		m_Velocity = Vector3.Scale(m_Velocity, Damping);
		m_Velocity = Vector3.ClampMagnitude(m_Velocity, MaxVelocity);
		if (m_Velocity.sqrMagnitude > MinVelocity * MinVelocity || (SdtdStopping && (RestState - State).sqrMagnitude > SdtdMinDeltaState * SdtdMinDeltaState))
		{
			Move();
		}
		else
		{
			Reset();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddForceInternal(Vector3 force)
	{
		force *= m_VelocityFadeInCap;
		m_Velocity += force;
		m_Velocity = Vector3.ClampMagnitude(m_Velocity, MaxVelocity);
		Move();
	}

	public void AddForce(Vector3 force)
	{
		if (Time.timeScale < 1f)
		{
			AddSoftForce(force, 1f);
		}
		else
		{
			AddForceInternal(force);
		}
	}

	public void AddSoftForce(Vector3 force, float frames)
	{
		force /= Time.timeScale;
		frames = Mathf.Clamp(frames, 1f, 120f);
		AddForceInternal(force / frames);
		for (int i = 0; i < Mathf.RoundToInt(frames) - 1; i++)
		{
			m_SoftForceFrame[i] += force / frames;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Move()
	{
		State += m_Velocity * Time.timeScale;
		State.x = Mathf.Clamp(State.x, MinState.x, MaxState.x);
		State.y = Mathf.Clamp(State.y, MinState.y, MaxState.y);
		State.z = Mathf.Clamp(State.z, MinState.z, MaxState.z);
	}

	public void Reset()
	{
		m_Velocity = Vector3.zero;
		State = RestState;
	}

	public void Stop(bool includeSoftForce = false)
	{
		m_Velocity = Vector3.zero;
		if (includeSoftForce)
		{
			StopSoftForce();
		}
	}

	public void StopSoftForce()
	{
		for (int i = 0; i < 120; i++)
		{
			m_SoftForceFrame[i] = Vector3.zero;
		}
	}

	public void ForceVelocityFadeIn(float seconds)
	{
		m_VelocityFadeInLength = seconds;
		m_VelocityFadeInEndTime = Time.time + seconds;
		m_VelocityFadeInCap = 0f;
	}
}
