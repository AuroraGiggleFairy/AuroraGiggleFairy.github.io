public class EntityLookHelper
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entity;

	public EntityLookHelper(EntityAlive _e)
	{
		entity = _e;
	}

	public void onUpdateLook()
	{
		if (entity.rotation.x > 1f)
		{
			entity.rotation.x -= 1f;
		}
		else if (entity.rotation.x < -1f)
		{
			entity.rotation.x += 1f;
		}
	}
}
