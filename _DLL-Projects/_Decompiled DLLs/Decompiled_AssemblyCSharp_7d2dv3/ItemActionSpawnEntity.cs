using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionSpawnEntity : ItemAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public class ItemActionDataSpawnEntity : ItemActionAttackData
	{
		public enum State
		{
			None,
			Anim,
			Spawn,
			End
		}

		public State state;

		public float stateTime;

		public ItemActionDataSpawnEntity(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int animType;

	[PublicizedFrom(EAccessModifier.Private)]
	public float animWait;

	[PublicizedFrom(EAccessModifier.Private)]
	public string soundWarn;

	[PublicizedFrom(EAccessModifier.Private)]
	public string soundAttack;

	[PublicizedFrom(EAccessModifier.Private)]
	public string entityToSpawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 entityOffset;

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionDataSpawnEntity(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		animType = _props.GetInt("AnimType");
		animWait = _props.GetFloat("AnimWait");
		soundWarn = _props.GetString("SoundWarn");
		soundAttack = _props.GetString("SoundAttack");
		entityToSpawn = _props.GetString("Entity");
		_props.ParseVec("EntityOffset", ref entityOffset);
	}

	public override void StartHolding(ItemActionData _actionData)
	{
	}

	public override void StopHolding(ItemActionData _actionData)
	{
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		ItemActionDataSpawnEntity itemActionDataSpawnEntity = (ItemActionDataSpawnEntity)_actionData;
		itemActionDataSpawnEntity.stateTime += 0.05f;
		switch (itemActionDataSpawnEntity.state)
		{
		case ItemActionDataSpawnEntity.State.Anim:
			if (!(itemActionDataSpawnEntity.stateTime < animWait))
			{
				itemActionDataSpawnEntity.state = ItemActionDataSpawnEntity.State.Spawn;
			}
			break;
		case ItemActionDataSpawnEntity.State.Spawn:
			Spawn(itemActionDataSpawnEntity);
			itemActionDataSpawnEntity.state = ItemActionDataSpawnEntity.State.End;
			break;
		}
	}

	public override void CancelAction(ItemActionData _actionData)
	{
		((ItemActionDataSpawnEntity)_actionData).state = ItemActionDataSpawnEntity.State.None;
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (!holdingEntity)
		{
			return;
		}
		ItemActionDataSpawnEntity itemActionDataSpawnEntity = (ItemActionDataSpawnEntity)_actionData;
		if (!_bReleased)
		{
			if (itemActionDataSpawnEntity.state == ItemActionDataSpawnEntity.State.None)
			{
				itemActionDataSpawnEntity.state = ItemActionDataSpawnEntity.State.Anim;
				itemActionDataSpawnEntity.stateTime = 0f;
				holdingEntity.StartAnimAction(animType + 3000);
				holdingEntity.PlayOneShot(soundWarn);
			}
		}
		else
		{
			itemActionDataSpawnEntity.state = ItemActionDataSpawnEntity.State.None;
		}
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		return ((ItemActionDataSpawnEntity)_actionData).state != ItemActionDataSpawnEntity.State.None;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Spawn(ItemActionData _actionData)
	{
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if ((bool)holdingEntity && holdingEntity.IsAttackValid())
		{
			Vector3 headPosition = holdingEntity.getHeadPosition();
			headPosition += holdingEntity.qrotation * entityOffset;
			Entity entity = EntityFactory.CreateEntity(EntityClass.GetId(entityToSpawn), headPosition, new Vector3(0f, holdingEntity.rotation.y, 0f));
			entity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
			GameManager.Instance.World.SpawnEntityInWorld(entity);
			if (entity is EntityAlive entityAlive)
			{
				entityAlive.SetAttackTarget(holdingEntity.GetAttackTarget(), 600);
			}
			holdingEntity.PlayOneShot(soundAttack);
		}
	}
}
