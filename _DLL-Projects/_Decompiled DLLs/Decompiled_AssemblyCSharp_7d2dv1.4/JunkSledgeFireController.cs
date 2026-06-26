using System;
using Audio;
using UnityEngine;

public class JunkSledgeFireController : MiniTurretFireController
{
	public enum ArmStates
	{
		Idle,
		Extending,
		Retracting
	}

	public ArmStates ArmState;

	public Transform Arm1;

	public float Arm1StartZ;

	public float Arm1EndZ;

	public Transform Arm2;

	public float Arm2StartZ;

	public float Arm2EndZ;

	public float ExtentionTime;

	public float RetractionTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timeCounter;

	public new void Update()
	{
		base.Update();
		if (ArmState == ArmStates.Idle)
		{
			return;
		}
		if (ArmState == ArmStates.Extending)
		{
			float num = Mathf.Clamp01(timeCounter / (burstFireRateMax * 0.5f * 0.25f));
			Arm1.localPosition = new Vector3(Arm1.localPosition.x, Arm1.localPosition.y, Mathf.Lerp(Arm1StartZ, Arm1EndZ, num));
			Arm2.localPosition = new Vector3(Arm2.localPosition.x, Arm2.localPosition.y, Mathf.Lerp(Arm2StartZ, Arm2EndZ, num));
			if (num >= 1f)
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					hitTarget();
				}
				ArmState = ArmStates.Retracting;
				timeCounter = 0f;
			}
		}
		else
		{
			float num2 = Mathf.Clamp01(timeCounter / (burstFireRateMax * 0.5f * 0.75f));
			Arm1.localPosition = new Vector3(Arm1.localPosition.x, Arm1.localPosition.y, Mathf.Lerp(Arm1EndZ, Arm1StartZ, num2));
			Arm2.localPosition = new Vector3(Arm2.localPosition.x, Arm2.localPosition.y, Mathf.Lerp(Arm2EndZ, Arm2StartZ, num2));
			if (num2 >= 1f)
			{
				ArmState = ArmStates.Idle;
				timeCounter = 0f;
			}
		}
		timeCounter += Time.deltaTime;
	}

	public override void Fire()
	{
		ArmState = ArmStates.Extending;
		Manager.Play(entityTurret, fireSound, 1f, wantHandle: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void hitTarget()
	{
		Vector3 position = Cone.transform.position;
		EntityAlive holdingEntity = GameManager.Instance.World.GetEntity(entityTurret.belongsPlayerId) as EntityAlive;
		float distance = base.MaxDistance;
		Vector3 forward = Cone.transform.forward;
		forward *= -1f;
		Voxel.Raycast(ray: new Ray(position + Origin.position, forward), _world: GameManager.Instance.World, distance: distance, _layerMask: -538750981, _hitMask: 128, _sphereRadius: 0.15f);
		ItemActionAttack.Hit(Voxel.voxelRayHitInfo.Clone(), entityTurret.belongsPlayerId, EnumDamageTypes.Bashing, GetDamageBlock(entityTurret.OriginalItemValue, BlockValue.Air, holdingEntity, 1), GetDamageEntity(entityTurret.OriginalItemValue, holdingEntity, 1), 1f, entityTurret.OriginalItemValue.PercentUsesLeft, 0f, 0f, "metal", damageMultiplier, buffActions, new ItemActionAttack.AttackHitInfo(), 1, 0, 0f, null, null, ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, entityTurret.entityId, entityTurret.OriginalItemValue);
	}
}
