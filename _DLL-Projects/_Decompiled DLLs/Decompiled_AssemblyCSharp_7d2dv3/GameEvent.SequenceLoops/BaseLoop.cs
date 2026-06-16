using System.Collections.Generic;
using GameEvent.SequenceActions;
using UnityEngine.Scripting;

namespace GameEvent.SequenceLoops;

[Preserve]
public class BaseLoop : BaseAction
{
	public List<BaseAction> Actions = new List<BaseAction>();

	public int PhaseMax = 1;

	public int CurrentPhase;

	public override void SetActionKeyData(int _actionIndex, BaseAction _parent, string prefix = "")
	{
		base.SetActionKeyData(_actionIndex, _parent, prefix);
		for (int i = 0; i < Actions.Count; i++)
		{
			Actions[i].SetActionKeyData(i, this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
		base.OnInit();
		List<int> list = new List<int>();
		for (int i = 0; i < Actions.Count; i++)
		{
			if (!list.Contains(Actions[i].Phase))
			{
				list.Add(Actions[i].Phase);
			}
		}
		list.Sort();
		if (list.Count > 0)
		{
			PhaseMax = list[list.Count - 1] + 1;
		}
		else
		{
			PhaseMax = 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ActionCompleteStates HandleActions()
	{
		bool flag = false;
		int num = CurrentPhase;
		for (int i = 0; i < Actions.Count; i++)
		{
			if (Actions[i].Phase != CurrentPhase || Actions[i].IsComplete)
			{
				continue;
			}
			ActionCompleteStates actionCompleteStates = ActionCompleteStates.InComplete;
			flag = true;
			if (!Actions[i].IsComplete)
			{
				Actions[i].Owner = base.Owner;
				actionCompleteStates = Actions[i].PerformAction();
			}
			if (actionCompleteStates == ActionCompleteStates.Complete || (actionCompleteStates == ActionCompleteStates.InCompleteRefund && Actions[i].IgnoreRefund))
			{
				Actions[i].IsComplete = true;
				if (Actions[i].PhaseOnComplete != -1)
				{
					num = Actions[i].PhaseOnComplete;
				}
			}
			else if (actionCompleteStates == ActionCompleteStates.RequirementsNotMet)
			{
				Actions[i].IsComplete = true;
				if (Actions[i].PhaseOnDenied != -1)
				{
					num = Actions[i].PhaseOnDenied;
				}
			}
			else if (base.Owner.AllowRefunds && actionCompleteStates == ActionCompleteStates.InCompleteRefund)
			{
				return ActionCompleteStates.InCompleteRefund;
			}
		}
		if (!flag)
		{
			CurrentPhase++;
		}
		else if (CurrentPhase != num)
		{
			CurrentPhase = num;
			for (int j = 0; j < Actions.Count; j++)
			{
				if (Actions[j].Phase >= CurrentPhase)
				{
					Actions[j].Reset();
				}
			}
		}
		if (CurrentPhase >= PhaseMax)
		{
			return ActionCompleteStates.Complete;
		}
		return ActionCompleteStates.InComplete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnReset()
	{
		base.OnReset();
		CurrentPhase = 0;
		for (int i = 0; i < Actions.Count; i++)
		{
			Actions[i].Reset();
		}
	}

	public override void HandleTemplateInit(GameEventActionSequence seq)
	{
		base.HandleTemplateInit(seq);
		for (int i = 0; i < Actions.Count; i++)
		{
			Actions[i].HandleTemplateInit(seq);
		}
	}

	public override BaseAction Clone()
	{
		BaseLoop baseLoop = (BaseLoop)base.Clone();
		for (int i = 0; i < Actions.Count; i++)
		{
			baseLoop.Actions.Add(Actions[i].Clone());
		}
		baseLoop.PhaseMax = PhaseMax;
		return baseLoop;
	}
}
