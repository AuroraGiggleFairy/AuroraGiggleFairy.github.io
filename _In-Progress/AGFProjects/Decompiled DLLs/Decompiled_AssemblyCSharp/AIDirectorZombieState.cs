using UnityEngine.Scripting;

[Preserve]
public class AIDirectorZombieState : IMemoryPoolableObject
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityEnemy m_zombie;

	public EntityEnemy Zombie => m_zombie;

	public AIDirectorZombieState Construct(EntityEnemy zombie)
	{
		m_zombie = zombie;
		return this;
	}

	public void Reset()
	{
		m_zombie = null;
	}

	public void Cleanup()
	{
	}
}
