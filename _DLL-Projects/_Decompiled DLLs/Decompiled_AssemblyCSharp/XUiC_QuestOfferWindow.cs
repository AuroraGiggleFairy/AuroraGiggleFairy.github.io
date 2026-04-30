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
	public bool btnAcceptHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool btnDeclineHovered;

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
	public Action<EntityNPC> OnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quest quest;

	[PublicizedFrom(EAccessModifier.Private)]
	public int variation = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int listIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastAnyKey = true;

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
				if (!base.xui.playerUI.entityPlayer.QuestJournal.CanAddProgression)
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
			if (Quest != null && Quest.QuestClass.AddsToTierComplete && !base.xui.playerUI.entityPlayer.QuestJournal.CanAddProgression)
			{
				value = "true";
			}
			else
			{
				value = "false";
			}
			return true;
		default:
			return false;
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
		btnAcceptHovered = _isOver;
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDecline_OnHover(XUiController _sender, bool _isOver)
	{
		btnDeclineHovered = _isOver;
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
				ItemStackController.ItemStack = ItemStack.Empty.Clone();
				ItemStackController.WindowGroup.Controller.SetAllChildrenDirty();
			}
		}
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (QuestGiverID != -1)
		{
			base.xui.Dialog.Respondent.PlayVoiceSetEntry("quest_accepted", entityPlayer);
		}
		questAccepted = true;
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
		entityPlayer.QuestJournal.AddQuest(quest);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDecline_OnPress(XUiController _sender, int _mouseButton)
	{
		EntityNPC respondent = base.xui.Dialog.Respondent;
		if (QuestGiverID != -1)
		{
			base.xui.Dialog.Respondent.PlayVoiceSetEntry("quest_declined", base.xui.playerUI.entityPlayer);
		}
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
		if (OnCancel != null)
		{
			OnCancel(respondent);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeButton_OnPress(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings(_forceAll: true);
			IsDirty = false;
		}
	}

	public static XUiC_QuestOfferWindow OpenQuestOfferWindow(XUi xui, Quest q, int listIndex = -1, OfferTypes offerType = OfferTypes.Item, int questGiverID = -1, Action<EntityNPC> onCancel = null)
	{
		bool flag = offerType == OfferTypes.Item;
		XUiC_QuestOfferWindow childByType = xui.FindWindowGroupByName("questOffer").GetChildByType<XUiC_QuestOfferWindow>();
		childByType.Quest = q;
		childByType.Variation = -1;
		childByType.listIndex = listIndex;
		childByType.QuestGiverID = questGiverID;
		childByType.OfferType = offerType;
		childByType.OnCancel = onCancel;
		xui.playerUI.windowManager.Open("questOffer", flag, _bIsNotEscClosable: false, flag);
		return childByType;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		questAccepted = false;
		Manager.PlayInsidePlayerHead("quest_note_offer");
		base.xui.playerUI.windowManager.CloseIfOpen("windowpaging");
		base.xui.playerUI.windowManager.CloseIfOpen("toolbelt");
		if (OfferType == OfferTypes.Dialog)
		{
			_ = base.xui.Dialog.DialogWindowGroup.CurrentDialog;
			base.xui.Dialog.DialogWindowGroup.RefreshDialog();
			base.xui.Dialog.DialogWindowGroup.ShowResponseWindow(isVisible: false);
			if (QuestGiverID != -1)
			{
				base.xui.Dialog.Respondent.PlayVoiceSetEntry("quest_offer", base.xui.playerUI.entityPlayer);
			}
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		if (!questAccepted)
		{
			Manager.PlayInsidePlayerHead("quest_note_decline");
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
			if (base.xui.Dialog.Respondent is EntityTrader { activeQuests: not null } entityTrader)
			{
				entityTrader.activeQuests.Remove(Quest);
			}
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageNPCQuestList>().Setup(QuestGiverID, base.xui.playerUI.entityPlayer.entityId, Quest.QuestClass.DifficultyTier, (byte)listIndex));
			}
			if (Quest.QuestTags.Test_AnySet(QuestEventManager.treasureTag) && GameSparksCollector.CollectGamePlayData)
			{
				GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.QuestAcceptedDistance, ((int)Vector3.Distance(Quest.Position, base.xui.Dialog.Respondent.position) / 50 * 50).ToString(), 1);
			}
		}
		Dialog currentDialog = base.xui.Dialog.DialogWindowGroup.CurrentDialog;
		if (currentDialog.CurrentStatement == null || currentDialog.CurrentStatement.NextStatementID == "")
		{
			base.xui.playerUI.windowManager.Close("dialog");
			return;
		}
		currentDialog.CurrentStatement = currentDialog.GetStatement(currentDialog.CurrentStatement.NextStatementID);
		base.xui.Dialog.DialogWindowGroup.RefreshDialog();
		base.xui.Dialog.DialogWindowGroup.ShowResponseWindow(isVisible: true);
	}
}
