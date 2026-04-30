using System;
using System.Collections;
using UnityEngine;

public class AnimatorMeleeAttackState : StateMachineBehaviour
{
	public float RaycastTime = 0.3f;

	public float ImpactDuration = 0.01f;

	public float ImpactPlaybackSpeed = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float calculatedRaycastTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float calculatedImpactDuration;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float calculatedImpactPlaybackSpeed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasFired;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int actionIndex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float originalMeleeAttackSpeed;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool playingImpact;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float attacksPerMinute;

	public static int FistHoldHash = Animator.StringToHash("fistHold");

	public static int FpvFistHoldHash = Animator.StringToHash("fpvFistHold");

	public AnimatorMeleeAttackState()
	{
		FistHoldHash = Animator.StringToHash("fistHold");
		FpvFistHoldHash = Animator.StringToHash("fpvFistHold");
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (playingImpact)
		{
			return;
		}
		hasFired = false;
		actionIndex = animator.GetInteger(AvatarController.itemActionIndexHash);
		AnimationEventBridge component = animator.GetComponent<AnimationEventBridge>();
		entity = component.entity;
		if (actionIndex < 0 || actionIndex >= entity.inventory.holdingItemData.actionData.Count || entity.inventory.holdingItemData.actionData[actionIndex] == null)
		{
			return;
		}
		AnimatorClipInfo[] array = animator.GetNextAnimatorClipInfo(layerIndex);
		if (array.Length == 0)
		{
			array = animator.GetCurrentAnimatorClipInfo(layerIndex);
			if (array.Length == 0)
			{
				return;
			}
		}
		AnimationClip clip = array[0].clip;
		float length = clip.length;
		attacksPerMinute = (int)(60f / length);
		FastTags<TagGroup.Global> tags = ((actionIndex == 0) ? ItemActionAttack.PrimaryTag : ItemActionAttack.SecondaryTag);
		ItemValue holdingItemItemValue = entity.inventory.holdingItemItemValue;
		ItemClass itemClass = holdingItemItemValue.ItemClass;
		if (itemClass != null)
		{
			tags |= itemClass.ItemTags;
		}
		originalMeleeAttackSpeed = EffectManager.GetValue(PassiveEffects.AttacksPerMinute, holdingItemItemValue, attacksPerMinute, entity, null, tags) / 60f * length;
		animator.SetFloat("MeleeAttackSpeed", originalMeleeAttackSpeed);
		ItemClass holdingItem = entity.inventory.holdingItem;
		holdingItem.Properties.ParseFloat((actionIndex == 0) ? "Action0.RaycastTime" : "Action1.RaycastTime", ref RaycastTime);
		float optionalValue = -1f;
		holdingItem.Properties.ParseFloat((actionIndex == 0) ? "Action0.ImpactDuration" : "Action1.ImpactDuration", ref optionalValue);
		if (optionalValue >= 0f)
		{
			ImpactDuration = optionalValue * originalMeleeAttackSpeed;
		}
		holdingItem.Properties.ParseFloat((actionIndex == 0) ? "Action0.ImpactPlaybackSpeed" : "Action1.ImpactPlaybackSpeed", ref ImpactPlaybackSpeed);
		if (originalMeleeAttackSpeed != 0f)
		{
			calculatedRaycastTime = RaycastTime / originalMeleeAttackSpeed;
			calculatedImpactDuration = ImpactDuration / originalMeleeAttackSpeed;
			calculatedImpactPlaybackSpeed = ImpactPlaybackSpeed / originalMeleeAttackSpeed;
		}
		else
		{
			calculatedRaycastTime = 0.001f;
			calculatedImpactDuration = 0.001f;
			calculatedImpactPlaybackSpeed = 0.001f;
		}
		if (entity.inventory.holdingItemData.actionData[actionIndex] is ItemActionDynamicMelee.ItemActionDynamicMeleeData itemActionDynamicMeleeData)
		{
			itemActionDynamicMeleeData.HasFinished = false;
		}
		GameManager.Instance.StartCoroutine(impactStart(animator, animator.GetNextAnimatorStateInfo(layerIndex), clip, layerIndex));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator impactStart(Animator animator, AnimatorStateInfo stateInfo, AnimationClip clip, int layerIndex)
	{
		yield return new WaitForSeconds(calculatedRaycastTime);
		if (!hasFired)
		{
			hasFired = true;
			if (entity != null && !entity.isEntityRemote && actionIndex >= 0 && entity.inventory.holdingItemData.actionData[actionIndex] is ItemActionDynamicMelee.ItemActionDynamicMeleeData actionData && (entity.inventory.holdingItem.Actions[actionIndex] as ItemActionDynamicMelee).Raycast(actionData))
			{
				animator.SetFloat("MeleeAttackSpeed", calculatedImpactPlaybackSpeed);
				playingImpact = true;
				GameManager.Instance.StartCoroutine(impactStop(animator, stateInfo, clip, layerIndex));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator impactStop(Animator animator, AnimatorStateInfo stateInfo, AnimationClip clip, int layerIndex)
	{
		animator.Play(0, layerIndex, Mathf.Min(1f, calculatedRaycastTime * originalMeleeAttackSpeed / clip.length));
		yield return new WaitForSeconds(calculatedImpactDuration);
		animator.SetFloat("MeleeAttackSpeed", originalMeleeAttackSpeed);
		playingImpact = false;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (entity != null && !entity.isEntityRemote && actionIndex >= 0 && entity.inventory.holdingItemData.actionData[actionIndex] is ItemActionDynamicMelee.ItemActionDynamicMeleeData itemActionDynamicMeleeData)
		{
			itemActionDynamicMeleeData.HasFinished = true;
			animator.SetFloat("MeleeAttackSpeed", originalMeleeAttackSpeed);
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		float normalizedTime = stateInfo.normalizedTime;
		if (float.IsInfinity(normalizedTime) || float.IsNaN(normalizedTime))
		{
			if (animator.HasState(layerIndex, FistHoldHash))
			{
				animator.Play(FistHoldHash, layerIndex, 1f);
			}
			else if (animator.HasState(layerIndex, FpvFistHoldHash))
			{
				animator.Play(FpvFistHoldHash, layerIndex, 1f);
			}
			else
			{
				animator.Play(animator.GetNextAnimatorStateInfo(layerIndex).shortNameHash, layerIndex);
			}
		}
	}
}
