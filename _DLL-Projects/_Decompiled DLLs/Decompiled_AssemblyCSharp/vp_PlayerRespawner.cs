using System;
using UnityEngine;

public class vp_PlayerRespawner : vp_Respawner
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_PlayerEventHandler m_Player;

	public vp_PlayerEventHandler Player
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (m_Player == null)
			{
				m_Player = base.transform.GetComponent<vp_PlayerEventHandler>();
			}
			return m_Player;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEnable()
	{
		if (Player != null)
		{
			Player.Register(this);
		}
		base.OnEnable();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDisable()
	{
		if (Player != null)
		{
			Player.Unregister(this);
		}
	}

	public override void Reset()
	{
		if (Application.isPlaying && !(Player == null))
		{
			Player.Position.Set(Placement.Position);
			Player.Rotation.Set(Placement.Rotation.eulerAngles);
			Player.Stop.Send();
		}
	}
}
