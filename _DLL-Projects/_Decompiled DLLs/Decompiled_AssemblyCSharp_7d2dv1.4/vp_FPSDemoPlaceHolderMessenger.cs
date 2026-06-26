using System;
using UnityEngine;

public class vp_FPSDemoPlaceHolderMessenger : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public vp_FPPlayerEventHandler Player;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_WasSwingingMaceIn3rdPersonLastFrame;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_WasClimbingIn3rdPersonLastFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		Player = base.transform.root.GetComponent<vp_FPPlayerEventHandler>();
		if (Player == null)
		{
			base.enabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Player == null)
		{
			return;
		}
		if (!Player.IsFirstPerson.Get() && Player.Climb.Active)
		{
			if (!m_WasClimbingIn3rdPersonLastFrame)
			{
				m_WasClimbingIn3rdPersonLastFrame = true;
				vp_Timer.In(0f, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					Player.HUDText.Send("PLACEHOLDER CLIMB ANIMATION");
				}, 3, 1f);
			}
		}
		else
		{
			m_WasClimbingIn3rdPersonLastFrame = false;
		}
		if (!Player.IsFirstPerson.Get() && Player.CurrentWeaponIndex.Get() == 4 && Player.Attack.Active)
		{
			if (!m_WasSwingingMaceIn3rdPersonLastFrame)
			{
				m_WasSwingingMaceIn3rdPersonLastFrame = true;
				vp_Timer.In(0f, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					Player.HUDText.Send("PLACEHOLDER MELEE ANIMATION");
				}, 3, 1f);
			}
		}
		else
		{
			m_WasSwingingMaceIn3rdPersonLastFrame = false;
		}
	}
}
