using UnityEngine.Scripting;

[Preserve]
public class EntityZombie : EntityHuman
{
	public override bool AimingGun
	{
		get
		{
			return false;
		}
		set
		{
		}
	}
}
