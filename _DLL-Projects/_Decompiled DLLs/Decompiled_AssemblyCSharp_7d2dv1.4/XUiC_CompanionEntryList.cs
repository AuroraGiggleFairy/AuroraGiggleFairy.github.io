using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CompanionEntryList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_CompanionEntry> entryList = new List<XUiC_CompanionEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float yOffset;

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = GetChildrenByType<XUiC_CompanionEntry>();
		XUiController[] array = childrenByType;
		for (int i = 0; i < array.Length; i++)
		{
			entryList.Add((XUiC_CompanionEntry)array[i]);
		}
		yOffset = viewComponent.Position.y;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RefreshPartyList();
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		CompanionGroup companions = entityPlayer.Companions;
		companions.OnGroupChanged = (OnCompanionGroupChanged)Delegate.Combine(companions.OnGroupChanged, new OnCompanionGroupChanged(RefreshPartyList));
		entityPlayer.PartyJoined += Party_Changed;
		entityPlayer.PartyChanged += Party_Changed;
		entityPlayer.PartyLeave += Party_Changed;
		if (entityPlayer.Party != null)
		{
			entityPlayer.Party.PartyMemberAdded += Party_PartyMemberChanged;
			entityPlayer.Party.PartyMemberRemoved += Party_PartyMemberChanged;
			entityPlayer.Party.PartyLeaderChanged += Party_Changed;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		CompanionGroup companions = entityPlayer.Companions;
		companions.OnGroupChanged = (OnCompanionGroupChanged)Delegate.Remove(companions.OnGroupChanged, new OnCompanionGroupChanged(RefreshPartyList));
		entityPlayer.PartyJoined -= Party_Changed;
		entityPlayer.PartyChanged -= Party_Changed;
		entityPlayer.PartyLeave -= Party_Changed;
		if (entityPlayer.Party != null)
		{
			entityPlayer.Party.PartyMemberAdded -= Party_PartyMemberChanged;
			entityPlayer.Party.PartyMemberRemoved -= Party_PartyMemberChanged;
			entityPlayer.Party.PartyLeaderChanged -= Party_Changed;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Party_Changed(Party _affectedParty, EntityPlayer _player)
	{
		RefreshPartyList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Party_PartyMemberChanged(EntityPlayer player)
	{
		RefreshPartyList();
	}

	public void RefreshPartyList()
	{
		int i = 0;
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		int num = 0;
		if (entityPlayer.Party != null)
		{
			num = (entityPlayer.Party.MemberList.Count - 1) * 40;
		}
		if (entityPlayer.Companions != null)
		{
			num += (entityPlayer.Companions.Count - 1) * 40;
		}
		viewComponent.Position = new Vector2i(viewComponent.Position.x, (int)yOffset - num);
		viewComponent.UiTransform.localPosition = new Vector3(viewComponent.Position.x, viewComponent.Position.y);
		if (entityPlayer.Companions != null)
		{
			for (int j = 0; j < entityPlayer.Companions.Count; j++)
			{
				EntityAlive companion = entityPlayer.Companions[j];
				if (i >= entryList.Count)
				{
					break;
				}
				entryList[i++].SetCompanion(companion);
			}
			for (; i < entryList.Count; i++)
			{
				entryList[i].SetCompanion(null);
			}
		}
		else
		{
			for (int k = 0; k < entryList.Count; k++)
			{
				entryList[k].SetCompanion(null);
			}
		}
	}
}
