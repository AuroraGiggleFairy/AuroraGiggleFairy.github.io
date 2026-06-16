using System.Collections.Generic;
using Audio;
using UnityEngine;

public class EAIDroneItemModHealWeapon : EAIDroneItemTask
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float healTargetRange = 15f;

	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
	}

	public override void SetData(Dictionary<string, string> data)
	{
		base.SetData(data);
	}

	public override bool CanExecute()
	{
		if (!base.CanExecute())
		{
			return false;
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return false;
		}
		if (drone.IsOnTeleportCooldown())
		{
			if (currentPath.Count > 0)
			{
				OnAttackExit();
			}
			return false;
		}
		if (!drone.CanAttack)
		{
			return false;
		}
		if ((bool)drone.Owner && LockManager.Instance.IsLockedServer(drone, 0))
		{
			return false;
		}
		EntityAlive nearestHealTargetInRange = drone.GetNearestHealTargetInRange(healTargetRange);
		if (!nearestHealTargetInRange)
		{
			return false;
		}
		if (!drone.TargetCanBeHealed(nearestHealTargetInRange))
		{
			return false;
		}
		if (!drone.IsTargetInNeedOfMedical(nearestHealTargetInRange))
		{
			return false;
		}
		DroneWeapons.Weapon installedWeapon = drone.GetInstalledWeapon(ItemKey);
		if (installedWeapon.canFire())
		{
			drone.SetAttackTarget(nearestHealTargetInRange, 1200);
			if ((bool)drone.GetAttackTarget())
			{
				drone.SetActiveWeapon(installedWeapon);
				drone.SetState(EntityDrone.State.Heal, sync: true);
				return true;
			}
		}
		return false;
	}

	public override bool Continue()
	{
		if (!drone)
		{
			return false;
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return false;
		}
		EntityAlive attackTarget = drone.GetAttackTarget();
		if (!attackTarget || attackTarget.IsDead())
		{
			OnAttackExit();
			return false;
		}
		if (drone.WakeupAnimTime > 0f)
		{
			return false;
		}
		DroneWeapons.Weapon installedWeapon = drone.GetInstalledWeapon(ItemKey);
		EntityAlive attackTarget2 = drone.GetAttackTarget();
		if ((bool)attackTarget2 && attackTarget2.AttachedToEntity as EntityVehicle != null)
		{
			installedWeapon?.Fire(attackTarget);
			OnAttackExit();
			return false;
		}
		if (installedWeapon != null && attackEnterTimer <= 0f)
		{
			Vector3 position = drone.position;
			Transform transform = drone.transform;
			Utils.DrawCircleLinesHorzontal(position - Origin.position, drone.EnemyDetectionRadius, Color.white, Color.yellow, 24, 0.05f);
			float pointRadius = 0.1f;
			float duration = 10f;
			if (!weaponDischarged)
			{
				Vector3 chestPosition = attackTarget.getChestPosition();
				if (DoMoveIntoAtkPos(attackTarget, installedWeapon.Range, transform.forward, pointRadius, debugDraw: true, duration) && installedWeapon.canFire())
				{
					Utils.DrawCircleLinesHorzontal(position - Origin.position, installedWeapon.Range, Color.white, Color.red, 24, 5f);
					Utils.DrawLine(position - Origin.position, chestPosition - Origin.position, Color.red, Color.red, 1, 5f);
					installedWeapon.Fire(attackTarget);
					weaponDischarged = true;
					ClearPath();
				}
			}
			else if (actionTimer <= 0f)
			{
				OnAttackExit();
				return false;
			}
		}
		return true;
	}

	public override void Update()
	{
		base.Update();
	}

	public override void Reset()
	{
		base.Reset();
		OnAttackExit();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnAttackEnter()
	{
		base.OnAttackEnter();
		if (GameManager.Instance.GetPersistentLocalPlayer() != null && drone.initSuppressVOTimer <= 0f)
		{
			Manager.Play(drone, "drone_healeffect");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnAttackAction()
	{
		base.OnAttackAction();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnAttackExit()
	{
		base.OnAttackExit();
	}
}
