using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestOfferWindow : XUiController
{
	public enum OfferTypes
	{
		Item,
		Dialog
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnAccept;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnDecline;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnAccept_Background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnDecline_Background;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool questAccepted;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<EntityNPC> OnDenied;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quest quest;

	[PublicizedFrom(EAccessModifier.Private)]
	public int variation = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int listIndex = -1;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public OfferTypes OfferType
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public Quest Quest
	{
		get
		{
			return quest;
		}
		set
		{
			quest = value;
			IsDirty = true;
		}
	}

	public int Variation
	{
		get
		{
			return variation;
		}
		set
		{
			variation = value;
			IsDirty = true;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemStack ItemStackController { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int QuestGiverID
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "questname":
			value = ((Quest != null) ? Quest.GetParsedText(Quest.QuestClass.Name) : "");
			return true;
		case "questicon":
			value = ((Quest != null) ? Quest.QuestClass.Icon : "");
			return true;
		case "questoffer":
			value = ((Quest != null) ? Quest.GetParsedText(Quest.QuestClass.Offer) : "");
			return true;
		case "questdifficulty":
			value = ((Quest != null) ? Quest.QuestClass.Difficulty : "");
			return true;
		case "tieradd":
			if (Quest != null && Quest.QuestClass.AddsToTierComplete)
			{
				if (!xui.playerUI.entityPlayer.QuestJournal.CanAddProgression)
				{
					value = "";
				}
				else
				{
					string arg = ((Quest.QuestClass.DifficultyTier > 0) ? "+" : "-") + Quest.QuestClass.DifficultyTier;
					value = string.Format(Localization.Get("xuiQuestTierAdd"), arg);
				}
			}
			else
			{
				value = "";
			}
			return true;
		case "tieraddlimited":
			if (Quest != null && Quest.QuestClass.AddsToTierComplete && !xui.playerUI.entityPlayer.QuestJournal.CanAddProgression)
			{
				value = "true";
			}
			else
			{
				value = "false";
			}
			return true;
		default:
			return base.GetBindingValueInternal(ref value, bindingName);
		}
	}

	public override void Init()
	{
		base.Init();
		btnAccept = GetChildById("btnAccept");
		btnAccept_Background = (XUiV_Button)btnAccept.GetChildById("clickable").ViewComponent;
		btnAccept_Background.Controller.OnPress += btnAccept_OnPress;
		btnAccept_Background.Controller.OnHover += btnAccept_OnHover;
		btnDecline = GetChildById("btnDecline");
		btnDecline_Background = (XUiV_Button)btnDecline.GetChildById("clickable").ViewComponent;
		btnDecline_Background.Controller.OnPress += btnDecline_OnPress;
		btnDecline_Background.Controller.OnHover += btnDecline_OnHover;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnAccept_OnHover(XUiController _sender, bool _isOver)
	{
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDecline_OnHover(XUiController _sender, bool _isOver)
	{
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnAccept_OnPress(XUiController _sender, int _mouseButton)
	{
		Quest quest = Quest;
		quest.QuestGiverID = QuestGiverID;
		if (OfferType == OfferTypes.Item)
		{
			ItemStack itemStack = ItemStackController.ItemStack;
			if (itemStack.count > 1)
			{
				itemStack.count--;
				ItemStackController.ForceSetItemStack(itemStack.Clone());
				ItemStackController.WindowGroup.Controller.SetAllChildrenDirty();
			}
			else
			{
				ItemStackController.ItemStack = ItemStack.Empty;
				ItemStackController.WindowGroup.Controller.SetAllChildrenDirty();
			}
		}
		EntityPlayerLocal entityPlayer = xui.playerUI.entityPlayer;
		if (QuestGiverID != -1)
		{
			xui.Dialog.Respondent.PlayVoiceSetEntry("quest_accepted", entityPlayer);
		}
		questAccepted = true;
		xui.playerUI.windowManager.Close(base.WindowGroup);
		entityPlayer.QuestJournal.AddQuest(quest);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDecline_OnPress(XUiController _sender, int _mouseButton)
	{
		if (QuestGiverID != -1)
		{
			xui.Dialog.Respondent.PlayVoiceSetEntry("quest_declined", xui.playerUI.entityPlayer);
		}
		questAccepted = false;
		xui.playerUI.windowManager.Close(base.WindowGroup);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings();
			IsDirty = false;
		}
	}

	public static XUiC_QuestOfferWindow OpenQuestOfferWindow(XUi xui, Quest q, int listIndex = -1, OfferTypes offerType = OfferTypes.Item, int questGiverID = -1, Action<EntityNPC> onDenied = null)
	{
		bool bModal = offerType == OfferTypes.Item;
		XUiC_QuestOfferWindow childByType = xui.FindWindowGroupByName("questOffer").GetChildByType<XUiC_QuestOfferWindow>();
		childByType.Quest = q;
		childByType.Variation = -1;
		childByType.listIndex = listIndex;
		childByType.QuestGiverID = questGiverID;
		childByType.OfferType = offerType;
		childByType.OnDenied = onDenied;
		xui.playerUI.windowManager.Open("questOffer", bModal);
		return childByType;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		questAccepted = false;
		Manager.PlayInsidePlayerHead("quest_note_offer");
		xui.playerUI.windowManager.Close("windowpaging");
		xui.playerUI.windowManager.Close("toolbelt");
		if (OfferType == OfferTypes.Dialog)
		{
			_ = xui.Dialog.DialogWindowGroup.CurrentDialog;
			xui.Dialog.DialogWindowGroup.RefreshDialog();
			xui.Dialog.DialogWindowGroup.ShowResponseWindow(isVisible: false);
			if (QuestGiverID != -1)
			{
				xui.Dialog.Respondent.PlayVoiceSetEntry("quest_offer", xui.playerUI.entityPlayer);
			}
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		if (!questAccepted)
		{
			Manager.PlayInsidePlayerHead("quest_note_decline");
			if (OnDenied != null)
			{
				OnDenied(xui.Dialog.Respondent);
			}
		}
		if (ItemStackController != null)
		{
			ItemStackController.QuestLock = false;
			ItemStackController = null;
		}
		if (OfferType != OfferTypes.Dialog)
		{
			return;
		}
		if (questAccepted)
		{
			if (xui.Dialog.Respondent is EntityTrader { activeQuests: not null } entityTrader)
			{
				entityTrader.activeQuests.Remove(Quest);
			}
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageNPCQuestList>().Setup(QuestGiverID, xui.playerUI.entityPlayer.entityId, Quest.QuestClass.DifficultyTier, (byte)listIndex));
			}
			if (Quest.QuestTags.Test_AnySet(QuestEventManager.treasureTag) && GameSparksCollector.CollectGamePlayData)
			{
				GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.QuestAcceptedDistance, ((int)Vector3.Distance(Quest.Position, xui.Dialog.Respondent.position) / 50 * 50).ToString(), 1);
			}
		}
		Dialog currentDialog = xui.Dialog.DialogWindowGroup.CurrentDialog;
		if (string.IsNullOrEmpty(currentDialog.CurrentStatement.NextStatementID) && string.IsNullOrEmpty(xui.Dialog.ReturnStatement))
		{
			xui.playerUI.windowManager.Close("dialog");
			return;
		}
		if (!string.IsNullOrEmpty(xui.Dialog.ReturnStatement))
		{
			currentDialog.CurrentStatement.NextStatementID = xui.Dialog.ReturnStatement;
			xui.Dialog.ReturnStatement = "";
		}
		currentDialog.CurrentStatement = currentDialog.GetStatement(currentDialog.CurrentStatement.NextStatementID);
		xui.Dialog.DialogWindowGroup.RefreshDialog();
		xui.Dialog.DialogWindowGroup.ShowResponseWindow(isVisible: true);
	}
}
