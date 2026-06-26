using System;

public class vp_SpeedPickup : vp_Pickup
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Timer.Handle m_Timer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		UpdateMotion();
		if (m_Depleted && !m_Audio.isPlaying)
		{
			Remove();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool TryGive(vp_FPPlayerEventHandler player)
	{
		if (m_Timer.Active)
		{
			return false;
		}
		player.SetState("MegaSpeed");
		vp_Timer.In(RespawnDuration, [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			player.SetState("MegaSpeed", setActive: false);
		}, m_Timer);
		return true;
	}
}
