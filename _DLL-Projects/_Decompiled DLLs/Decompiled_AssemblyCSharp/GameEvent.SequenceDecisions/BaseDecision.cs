using System.Collections.Generic;
using GameEvent.SequenceActions;
using UnityEngine.Scripting;

namespace GameEvent.SequenceDecisions;

[Preserve]
public class BaseDecision : BaseAction
{
	public List<BaseAction> Actions = new List<BaseAction>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public int currentPhase;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int phaseMax;

	public override bool UseRequirements => false;

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
			phaseMax = list[list.Count - 1] + 1;
		}
		else
		{
			phaseMax = 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnReset()
	{
		base.OnReset();
		for (int i = 0; i < Actions.Count; i++)
		{
			Actions[i].Reset();
		}
		currentPhase = 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ActionCompleteStates HandleActions()
	{
		bool flag = false;
		int num = currentPhase;
		for (int i = 0; i < Actions.Count; i++)
		{
			if (Actions[i].Phase != currentPhase || Actions[i].IsComplete)
			{
				continue;
			}
			ActionCompleteStates actionCompleteStates = ActionCompleteStates.InComplete;
			flag = true;
			Actions[i].Owner = base.Owner;
			actionCompleteStates = Actions[i].PerformAction();
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
			currentPhase++;
		}
		else if (currentPhase != num)
		{
			currentPhase = num;
			for (int j = 0; j < Actions.Count; j++)
			{
				if (Actions[j].Phase >= currentPhase)
				{
					Actions[j].Reset();
				}
			}
		}
		if (currentPhase >= phaseMax)
		{
			IsComplete = true;
			return ActionCompleteStates.Complete;
		}
		return ActionCompleteStates.InComplete;
	}

	public override void HandleTemplateInit(GameEventActionSequence seq)
	{
		base.HandleTemplateInit(seq);
		for (int i = 0; i < Actions.Count; i++)
		{
			seq.HandleVariablesForProperties(Actions[i].Properties);
			Actions[i].ParseProperties(Actions[i].Properties);
		}
	}

	public override BaseAction Clone()
	{
		BaseDecision baseDecision = (BaseDecision)base.Clone();
		for (int i = 0; i < Actions.Count; i++)
		{
			BaseAction baseAction = Actions[i].Clone();
			baseAction.Owner = base.Owner;
			baseDecision.Actions.Add(baseAction);
		}
		baseDecision.phaseMax = phaseMax;
		return baseDecision;
	}
}
