using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionVomit : ItemActionLauncher
{
	public class ItemActionDataVomit : ItemActionDataLauncher
	{
		public float warningTime;

		public int numWarningsPlayed;

		public int numVomits;

		public bool bAttackStarted;

		public bool isDone;

		public ItemActionDataVomit(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRadius = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int animType;

	[PublicizedFrom(EAccessModifier.Private)]
	public float warningDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public int warningMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public string soundWarning;

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionDataVomit(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		_props.ParseInt("AnimType", ref animType);
		warningDelay = 1.2f;
		_props.ParseFloat("WarningDelay", ref warningDelay);
		warningMax = 3;
		_props.ParseInt("WarningMax", ref warningMax);
		_props.ParseString("Sound_warning", ref soundWarning);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetAttack(ItemActionDataVomit _actionData)
	{
		_actionData.numWarningsPlayed = 0;
		_actionData.warningTime = 0f;
		_actionData.bAttackStarted = false;
		_actionData.isDone = false;
	}

	public override void ItemActionEffects(GameManager _gameManager, ItemActionData _actionData, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
	{
		ItemActionDataVomit itemActionDataVomit = (ItemActionDataVomit)_actionData;
		if (itemActionDataVomit.muzzle == null)
		{
			itemActionDataVomit.muzzle = _actionData.invData.holdingEntity.emodel.GetRightHandTransform();
		}
		if (_firingState != 0)
		{
			itemActionDataVomit.numVomits++;
			Vector3 direction = itemActionDataVomit.invData.holdingEntity.GetLookRay().direction;
			int burstCount = GetBurstCount(_actionData);
			for (int i = 0; i < burstCount; i++)
			{
				Vector3 directionRandomOffset = getDirectionRandomOffset(itemActionDataVomit, direction);
				instantiateProjectile(_actionData).GetComponent<ProjectileMoveScript>().Fire(_startPos, directionRandomOffset, _actionData.invData.holdingEntity, hitmaskOverride, 0.2f);
			}
		}
		base.ItemActionEffects(_gameManager, _actionData, _firingState, _startPos, _direction, _userData);
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		ItemActionDataVomit itemActionDataVomit = (ItemActionDataVomit)_actionData;
		if (_bReleased)
		{
			base.ExecuteAction(_actionData, _bReleased);
			resetAttack(itemActionDataVomit);
			return;
		}
		float time = Time.time;
		if (time - itemActionDataVomit.m_LastShotTime < Delay || (itemActionDataVomit.warningTime > 0f && time < itemActionDataVomit.warningTime))
		{
			return;
		}
		if (!itemActionDataVomit.bAttackStarted)
		{
			EntityAlive holdingEntity = _actionData.invData.holdingEntity;
			if (itemActionDataVomit.numWarningsPlayed < warningMax - 1 && holdingEntity.rand.RandomFloat < 0.5f)
			{
				itemActionDataVomit.numWarningsPlayed++;
				itemActionDataVomit.warningTime = time + warningDelay;
				holdingEntity.PlayOneShot(soundWarning);
				holdingEntity.Raging = true;
				return;
			}
			itemActionDataVomit.bAttackStarted = true;
			itemActionDataVomit.numVomits = 0;
			holdingEntity.StartSpecialAttack(animType);
			if (warningMax > 0)
			{
				holdingEntity.PlayOneShot(soundWarning);
				itemActionDataVomit.warningTime = time + warningDelay;
				return;
			}
		}
		if (itemActionDataVomit.numVomits >= GetMaxAmmoCount(itemActionDataVomit))
		{
			itemActionDataVomit.isDone = true;
			return;
		}
		itemActionDataVomit.curBurstCount = 0;
		base.ExecuteAction(_actionData, _bReleased);
	}
}
