public class vp_WeaponPickup : vp_Pickup
{
	public int AmmoIncluded;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool TryGive(vp_FPPlayerEventHandler player)
	{
		if (player.Dead.Active)
		{
			return false;
		}
		if (!base.TryGive(player))
		{
			return false;
		}
		player.SetWeaponByName.Try(InventoryName);
		if (AmmoIncluded > 0)
		{
			player.AddAmmo.Try(new object[2] { InventoryName, AmmoIncluded });
		}
		return true;
	}
}
