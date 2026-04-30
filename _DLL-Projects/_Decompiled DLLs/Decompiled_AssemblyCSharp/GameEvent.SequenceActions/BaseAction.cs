using System;
using System.Collections;
using System.Collections.Generic;
using GameEvent.SequenceRequirements;
using UnityEngine;

namespace GameEvent.SequenceActions;

public class BaseAction
{
	public enum ActionCompleteStates
	{
		InComplete,
		InCompleteRefund,
		RequirementsNotMet,
		Complete
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Dictionary<string, BaseAction> sLookupByKey = new Dictionary<string, BaseAction>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameEventActionSequence owner;

	public int Phase;

	public int PhaseOnComplete = -1;

	public int PhaseOnDenied = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string actionKey = "";

	public bool IgnoreRefund;

	public bool IsComplete;

	public DynamicProperties Properties;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPhase = "phase";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPhaseOnComplete = "phase_on_complete";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropPhaseOnDenied = "phase_on_denied";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropIgnoreRefund = "ignore_refund";

	public List<BaseRequirement> Requirements;

	public GameEventActionSequence Owner
	{
		get
		{
			return owner;
		}
		set
		{
			owner = value;
			if (Requirements != null)
			{
				for (int i = 0; i < Requirements.Count; i++)
				{
					Requirements[i].Owner = value;
				}
			}
		}
	}

	public virtual bool UseRequirements => true;

	public static BaseAction FindKey(string key)
	{
		BaseAction value = null;
		if (!sLookupByKey.TryGetValue(key, out value))
		{
			value = null;
		}
		return value;
	}

	public virtual void SetActionKeyData(int _actionIndex, BaseAction _parent, string prefix = "")
	{
		if (_parent != null)
		{
			actionKey = _parent.GetActionKey() + ":" + _actionIndex;
		}
		else
		{
			actionKey = prefix + _actionIndex;
		}
		sLookupByKey[actionKey] = this;
	}

	public string GetActionKey()
	{
		return actionKey;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnInit()
	{
	}

	public void Init()
	{
		OnInit();
		IsComplete = false;
	}

	public virtual bool CanPerform(Entity target)
	{
		return true;
	}

	public virtual void OnClientPerform(Entity target)
	{
	}

	public virtual ActionCompleteStates OnPerformAction()
	{
		return ActionCompleteStates.InCompleteRefund;
	}

	public ActionCompleteStates PerformAction()
	{
		if (UseRequirements && Requirements != null)
		{
			for (int i = 0; i < Requirements.Count; i++)
			{
				Requirements[i].Owner = Owner;
				if (!Requirements[i].CanPerform(Owner.Target))
				{
					return ActionCompleteStates.RequirementsNotMet;
				}
			}
		}
		return OnPerformAction();
	}

	public void Reset()
	{
		IsComplete = false;
		OnReset();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnReset()
	{
	}

	public virtual void ParseProperties(DynamicProperties properties)
	{
		Properties = properties;
		Owner.HandleVariablesForProperties(properties);
		properties.ParseInt(PropPhase, ref Phase);
		properties.ParseInt(PropPhaseOnComplete, ref PhaseOnComplete);
		properties.ParseInt(PropPhaseOnDenied, ref PhaseOnDenied);
		properties.ParseBool(PropIgnoreRefund, ref IgnoreRefund);
	}

	public virtual void HandleTemplateInit(GameEventActionSequence seq)
	{
		seq.HandleVariablesForProperties(Properties);
		Owner = seq;
		if (Properties != null)
		{
			ParseProperties(Properties);
		}
		Init();
		if (Requirements == null)
		{
			return;
		}
		for (int i = 0; i < Requirements.Count; i++)
		{
			seq.HandleVariablesForProperties(Requirements[i].Properties);
			if (Requirements[i].Properties != null)
			{
				Requirements[i].ParseProperties(Requirements[i].Properties);
			}
			Requirements[i].Init();
		}
	}

	public void AddRequirement(BaseRequirement req)
	{
		if (Requirements == null)
		{
			Requirements = new List<BaseRequirement>();
		}
		req.Owner = Owner;
		Requirements.Add(req);
	}

	public virtual BaseAction Clone()
	{
		BaseAction baseAction = CloneChildSettings();
		if (Properties != null)
		{
			baseAction.Properties = new DynamicProperties();
			baseAction.Properties.CopyFrom(Properties);
		}
		baseAction.Phase = Phase;
		baseAction.PhaseOnComplete = PhaseOnComplete;
		baseAction.PhaseOnDenied = PhaseOnDenied;
		baseAction.IsComplete = false;
		baseAction.actionKey = actionKey;
		baseAction.IgnoreRefund = IgnoreRefund;
		if (Requirements != null)
		{
			for (int i = 0; i < Requirements.Count; i++)
			{
				baseAction.AddRequirement(Requirements[i].Clone());
			}
		}
		return baseAction;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual BaseAction CloneChildSettings()
	{
		return null;
	}

	public IEnumerator TeleportEntity(Entity entity, Vector3 position, float teleportDelay)
	{
		yield return new WaitForSeconds(teleportDelay);
		EntityPlayer entityPlayer = entity as EntityPlayer;
		if (entityPlayer != null)
		{
			if (entityPlayer.isEntityRemote)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageCloseAllWindows>().Setup(entityPlayer.entityId), _onlyClientsAttachedToAnEntity: false, entityPlayer.entityId);
				SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(entityPlayer.entityId).SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(position));
			}
			else
			{
				((EntityPlayerLocal)entityPlayer).PlayerUI.windowManager.CloseAllOpenWindows();
				((EntityPlayerLocal)entityPlayer).TeleportToPosition(position);
			}
		}
		else if (entity.AttachedToEntity != null)
		{
			entity.AttachedToEntity.SetPosition(position);
		}
		else
		{
			entity.SetPosition(position);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual string ParseTextElement(string element)
	{
		return element;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string GetTextWithElements(string text)
	{
		int num = text.IndexOf("{", StringComparison.Ordinal);
		Dictionary<string, string> dictionary = null;
		while (num != -1)
		{
			int num2 = text.IndexOf("}", num, StringComparison.Ordinal);
			if (num2 == -1)
			{
				break;
			}
			string text2 = text.Substring(num + 1, num2 - num - 1);
			string text3 = ParseTextElement(text2);
			if (text3 != text2)
			{
				if (dictionary == null)
				{
					dictionary = new Dictionary<string, string>();
				}
				dictionary.Add(text.Substring(num, num2 - num + 1), text3);
			}
			num = text.IndexOf("{", num2, StringComparison.Ordinal);
		}
		if (dictionary != null)
		{
			foreach (string key in dictionary.Keys)
			{
				text = text.Replace(key, dictionary[key]);
			}
		}
		return text;
	}

	public virtual BaseAction HandleAssignFrom(GameEventActionSequence newSeq, GameEventActionSequence oldSeq)
	{
		BaseAction baseAction = Clone();
		baseAction.Properties = new DynamicProperties();
		if (Properties != null)
		{
			baseAction.Properties.CopyFrom(Properties);
		}
		baseAction.Owner = newSeq;
		if (baseAction.Requirements != null)
		{
			for (int i = 0; i < baseAction.Requirements.Count; i++)
			{
				baseAction.Requirements[i].Properties = new DynamicProperties();
				if (Requirements[i].Properties != null)
				{
					baseAction.Requirements[i].Properties.CopyFrom(Requirements[i].Properties);
				}
				baseAction.Requirements[i].Owner = newSeq;
				baseAction.Requirements[i].Init();
			}
		}
		return baseAction;
	}
}
