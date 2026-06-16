namespace Webserver.LiveData;

public class Hostiles : EntityFilterList<EntityEnemy>
{
	public static readonly Hostiles Instance = new Hostiles();

	[PublicizedFrom(EAccessModifier.Protected)]
	public override EntityEnemy predicate(Entity _e)
	{
		if (_e is EntityEnemy entityEnemy && entityEnemy.IsAlive())
		{
			return entityEnemy;
		}
		return null;
	}
}
