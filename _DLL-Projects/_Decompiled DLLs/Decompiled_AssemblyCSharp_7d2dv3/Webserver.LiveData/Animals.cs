namespace Webserver.LiveData;

public class Animals : EntityFilterList<EntityAnimal>
{
	public static readonly Animals Instance = new Animals();

	[PublicizedFrom(EAccessModifier.Protected)]
	public override EntityAnimal predicate(Entity _e)
	{
		if (_e is EntityAnimal entityAnimal && entityAnimal.IsAlive())
		{
			return entityAnimal;
		}
		return null;
	}
}
