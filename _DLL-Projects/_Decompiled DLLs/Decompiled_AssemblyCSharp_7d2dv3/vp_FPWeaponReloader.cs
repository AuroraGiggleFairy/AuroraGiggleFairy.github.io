using UnityEngine;

[RequireComponent(typeof(vp_FPWeapon))]
public class vp_FPWeaponReloader : vp_WeaponReloader
{
	public AnimationClip AnimationReload;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnStart_Reload()
	{
		base.OnStart_Reload();
		if (!(AnimationReload == null))
		{
			if (m_Player.Reload.AutoDuration == 0f)
			{
				m_Player.Reload.AutoDuration = AnimationReload.length;
			}
			((vp_FPWeapon)m_Weapon).WeaponModel.GetComponent<Animation>().CrossFade(AnimationReload.name);
		}
	}
}
