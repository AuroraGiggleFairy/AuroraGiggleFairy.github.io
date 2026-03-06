using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionPOIReset : BaseAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum State
	{
		Start,
		Wait
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ActionCompleteStates _retVal;

	[PublicizedFrom(EAccessModifier.Private)]
	public State _state;

	public override ActionCompleteStates OnPerformAction()
	{
		if (_state == State.Start)
		{
			_state = State.Wait;
			_retVal = ActionCompleteStates.InComplete;
			GameManager.Instance.StartCoroutine(onPerformAction());
		}
		return _retVal;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator onPerformAction()
	{
		_ = base.Owner.POIPosition;
		World world = GameManager.Instance.World;
		if (base.Owner.POIInstance == null)
		{
			_retVal = ActionCompleteStates.InCompleteRefund;
			yield break;
		}
		List<PrefabInstance> prefabsIntersecting = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabsIntersecting(base.Owner.POIInstance);
		int entityID = -1;
		if (!GameManager.Instance.IsEditMode() && !GameUtils.IsPlaytesting())
		{
			entityID = ((base.Owner.Requester != null) ? base.Owner.Requester.entityId : (-1));
		}
		yield return world.ResetPOIS(prefabsIntersecting, QuestEventManager.manualResetTag, entityID, null, null);
		_retVal = ActionCompleteStates.Complete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator UpdateBlocks(List<BlockChangeInfo> blockChanges)
	{
		yield return new WaitForSeconds(1f);
		GameManager.Instance.World.SetBlocksRPC(blockChanges);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionPOIReset();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnReset()
	{
		_state = State.Start;
	}
}
