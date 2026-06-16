using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityBandit : EntityHuman
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum InvSlots
	{
		Melee,
		Ranged,
		Thrown,
		Heal
	}

	public readonly AIFocus<AIFocusBody> focusBody = new AIFocus<AIFocusBody>(thisIsDumb: true);

	public readonly AIFocus<AIFocusAim> focusAim = new AIFocus<AIFocusAim>(thisIsDumb: true);

	public override void PostInit()
	{
		string text = EntityClass.list[entityClass].Properties.GetString(EntityClass.PropHandItem);
		if (text.Length > 0)
		{
			SetInventorySlots(text);
		}
		if (inventory.GetItem(0).IsEmpty())
		{
			ItemValue bareHandItemValue = inventory.GetBareHandItemValue();
			bareHandItemValue.Quality = (ushort)rand.RandomRange(1, 3);
			bareHandItemValue.UseTimes = (float)bareHandItemValue.MaxUseTimes * 0.7f - 1f;
			inventory.SetItem(0, bareHandItemValue, 1);
		}
		int num = 1;
		if (!inventory.GetItem(num).IsEmpty())
		{
			inventory.SetHoldingItemIdx(num);
		}
		if (moveHelper != null)
		{
			moveHelper.CanOpenDoors = true;
		}
	}

	public override void OnAddedToWorld()
	{
		base.OnAddedToWorld();
	}

	public override void SetupHandItem()
	{
		ShowHoldingItem(_show: true);
	}

	public override bool UseHoldingItem(int _actionIndex, bool _isReleased)
	{
		if (!_isReleased && inventory.holdingItemData.actionData[0] is ItemActionAttackData itemActionAttackData)
		{
			ItemValue itemValue = itemActionAttackData.invData.itemValue;
			itemValue.UseTimes = (float)itemValue.MaxUseTimes * 0.8f - 1f;
			if (itemActionAttackData is ItemActionRanged.ItemActionDataRanged)
			{
				itemValue.Meta = 2;
			}
		}
		return base.UseHoldingItem(_actionIndex, _isReleased);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTasks()
	{
		base.updateTasks();
		if (AIFocusAim.GetActiveFocus(this, focusAim, out var activeFocus))
		{
			SetLookPosition(activeFocus);
		}
	}

	public override bool GetAimTarget(out Vector3 aimTarget)
	{
		if (AIFocusAim.GetActiveFocus(this, focusAim, out aimTarget))
		{
			return true;
		}
		return base.GetAimTarget(out aimTarget);
	}

	public override bool GetHeadLookTarget(out Vector3 lookTarget, out bool requiresLineOfSight)
	{
		requiresLineOfSight = false;
		if (AIFocusAim.GetActiveFocus(this, focusAim, out lookTarget))
		{
			return true;
		}
		return base.GetHeadLookTarget(out lookTarget, out requiresLineOfSight);
	}

	public override bool CalcStrafeYawOffset(float _moveX, float _moveZ, ref float _desiredyaw, ref float _yawOffset)
	{
		if (AIFocusBody.GetActiveFocus(this, focusBody, out var activeFocus))
		{
			float num = Mathf.Atan2(_moveX, _moveZ) * 57.29578f;
			_desiredyaw = activeFocus;
			_yawOffset = MathUtils.NormalizeAxis(num - _desiredyaw);
			return true;
		}
		return false;
	}
}
