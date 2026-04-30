using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public sealed class AIDirectorSmellMarker : IAIDirectorMarker, IMemoryPoolableObject
{
	public const int kMax = 256;

	[PublicizedFrom(EAccessModifier.Private)]
	public static MemoryPooledObject<AIDirectorSmellMarker> s_pool = new MemoryPooledObject<AIDirectorSmellMarker>(256);

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_radius;

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_strength;

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_speed;

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_ttl;

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_validTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_time;

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_lifetime;

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_effectiveRadius;

	[PublicizedFrom(EAccessModifier.Private)]
	public double m_effectiveStrength;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_priority;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_refCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_pos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_targetPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorPlayerState m_playerState;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_interruptsNonPlayerAttack;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_isDistraction;

	public EntityPlayer Player
	{
		get
		{
			if (m_playerState != null)
			{
				return m_playerState.Player;
			}
			return null;
		}
	}

	public Vector3 Position => m_pos;

	public Vector3 TargetPosition => m_targetPos;

	public bool Valid
	{
		get
		{
			if (m_validTime > 0.0)
			{
				if (!(Player == null))
				{
					return !Player.IsDead();
				}
				return true;
			}
			return false;
		}
	}

	public float MaxRadius => (float)m_radius;

	public float Radius => (float)m_effectiveRadius;

	public float TimeToLive => (float)m_ttl;

	public float ValidTime => (float)m_validTime;

	public float Speed => (float)m_speed;

	public int Priority => m_priority;

	public bool InterruptsNonPlayerAttack => m_interruptsNonPlayerAttack;

	public bool IsDistraction => m_isDistraction;

	public void Reference()
	{
		m_refCount++;
	}

	public bool Release()
	{
		if (--m_refCount == 0)
		{
			Reset();
			s_pool.Free(this);
			return true;
		}
		return false;
	}

	public void Reset()
	{
		m_playerState = null;
	}

	public void Cleanup()
	{
	}

	public void Tick(double dt)
	{
		m_ttl -= dt;
		if (m_ttl < 0.0)
		{
			m_ttl = 0.0;
		}
		m_validTime -= dt;
		if (m_validTime < 0.0)
		{
			m_validTime = 0.0;
		}
		m_time += dt;
		if (m_time > m_lifetime)
		{
			m_time = m_lifetime;
		}
		m_effectiveRadius = ((m_speed > 0.0) ? Math.Min(m_radius, m_speed * m_time) : m_radius);
		m_effectiveStrength = m_strength * (1.0 - m_time / m_lifetime);
	}

	public double IntensityForPosition(Vector3 position)
	{
		double num = (m_pos - position).magnitude;
		if (num > m_effectiveRadius)
		{
			return 0.0;
		}
		double num2 = 1.0;
		if (num > 0.0)
		{
			num2 /= num * num;
		}
		return m_effectiveStrength * num2;
	}

	public static AIDirectorSmellMarker Allocate(AIDirectorPlayerState ps, Vector3 position, Vector3 targetPosition, double radius, double strength, double speed, int priority, double ttl, bool interruptsNonPlayerAttack, bool isDistraction)
	{
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorSmellMarker Construct(AIDirectorPlayerState ps, Vector3 position, Vector3 targetPosition, double radius, double strength, double speed, int priority, double ttl, bool interruptsNonPlayerAttack, bool isDistraction)
	{
		m_refCount = 1;
		m_playerState = ps;
		m_pos = position;
		m_targetPos = targetPosition;
		m_radius = radius;
		m_strength = strength;
		m_speed = speed;
		m_priority = priority;
		m_validTime = ttl;
		m_lifetime = ttl;
		m_time = 0.0;
		m_effectiveRadius = 0.0;
		m_effectiveStrength = strength;
		m_interruptsNonPlayerAttack = interruptsNonPlayerAttack;
		m_isDistraction = isDistraction;
		if (isDistraction)
		{
			m_ttl = Mathf.Max((float)ttl, 20f);
		}
		else
		{
			m_ttl = Constants.cEnemySenseMemory;
		}
		return this;
	}
}
