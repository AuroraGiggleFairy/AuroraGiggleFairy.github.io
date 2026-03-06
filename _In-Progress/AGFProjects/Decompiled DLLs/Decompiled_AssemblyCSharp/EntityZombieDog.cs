using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityZombieDog : EntityEnemyAnimal
{
	public ulong timeToDie;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		Transform transform = base.transform.Find("Graphics/BlobShadowProjector");
		if ((bool)transform)
		{
			transform.gameObject.SetActive(value: false);
		}
	}

	public override void Init(int _entityClass)
	{
		base.Init(_entityClass);
		timeToDie = world.worldTime + 1800 + (ulong)(22000f * rand.RandomFloat);
	}

	public override void InitFromPrefab(int _entityClass)
	{
		base.InitFromPrefab(_entityClass);
		timeToDie = world.worldTime + 1800 + (ulong)(22000f * rand.RandomFloat);
	}

	public override void OnUpdateLive()
	{
		base.OnUpdateLive();
		if (world.worldTime >= timeToDie && !isEntityRemote)
		{
			Kill(DamageResponse.New(_fatal: true));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDetailedHeadBodyColliders()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override int GetMaxAttackTime()
	{
		return 30;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEntityTargeted(EntityAlive target)
	{
		base.OnEntityTargeted(target);
	}
}
