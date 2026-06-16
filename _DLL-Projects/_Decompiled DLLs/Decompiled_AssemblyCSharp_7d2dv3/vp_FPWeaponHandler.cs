public class vp_FPWeaponHandler : vp_WeaponHandler
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnAttempt_AutoReload()
	{
		if (!ReloadAutomatically)
		{
			return false;
		}
		return m_Player.Reload.TryStart();
	}
}
