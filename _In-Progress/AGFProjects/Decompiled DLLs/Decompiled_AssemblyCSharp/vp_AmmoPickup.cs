public class vp_AmmoPickup : vp_Pickup
{
	public int GiveAmount = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool TryGive(vp_FPPlayerEventHandler player)
	{
		if (player.Dead.Active)
		{
			return false;
		}
		for (int i = 0; i < GiveAmount; i++)
		{
			if (!base.TryGive(player))
			{
				if (TryReloadIfEmpty(player))
				{
					base.TryGive(player);
					return true;
				}
				return false;
			}
		}
		TryReloadIfEmpty(player);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryReloadIfEmpty(vp_FPPlayerEventHandler player)
	{
		if (player.CurrentWeaponAmmoCount.Get() > 0)
		{
			return false;
		}
		if (player.CurrentWeaponClipType.Get() != InventoryName)
		{
			return false;
		}
		if (!player.Reload.TryStart())
		{
			return false;
		}
		return true;
	}
}
