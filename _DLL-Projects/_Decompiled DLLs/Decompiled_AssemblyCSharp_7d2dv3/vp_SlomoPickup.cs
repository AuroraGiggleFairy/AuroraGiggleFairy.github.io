using System;
using UnityEngine;

public class vp_SlomoPickup : vp_Pickup
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_FPPlayerEventHandler m_Player;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		UpdateMotion();
		if (m_Depleted)
		{
			if (m_Player != null && m_Player.Dead.Active && !m_RespawnTimer.Active)
			{
				Respawn();
			}
			else if (Time.timeScale > 0.2f && !vp_TimeUtility.Paused)
			{
				vp_TimeUtility.FadeTimeScale(0.2f, 0.1f);
			}
			else if (!m_Audio.isPlaying)
			{
				Remove();
			}
		}
		else if (Time.timeScale < 1f && !vp_TimeUtility.Paused)
		{
			vp_TimeUtility.FadeTimeScale(1f, 0.05f);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool TryGive(vp_FPPlayerEventHandler player)
	{
		m_Player = player;
		if (m_Depleted || Time.timeScale != 1f)
		{
			return false;
		}
		return true;
	}
}
