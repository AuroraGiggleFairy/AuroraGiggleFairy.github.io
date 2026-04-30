using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PartyEntryList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_PartyEntry> entryList = new List<XUiC_PartyEntry>();

	public override void Init()
	{
		base.Init();
		XUiC_PartyEntry[] childrenByType = GetChildrenByType<XUiC_PartyEntry>();
		for (int i = 0; i < childrenByType.Length; i++)
		{
			entryList.Add(childrenByType[i]);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RefreshPartyList();
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		entityPlayer.PartyJoined += EntityPlayer_PartyJoined;
		entityPlayer.PartyChanged += EntityPlayer_PartyJoined;
		entityPlayer.PartyLeave += EntityPlayer_PartyJoined;
		if (entityPlayer.Party != null)
		{
			entityPlayer.Party.PartyMemberAdded += Party_PartyMemberChanged;
			entityPlayer.Party.PartyMemberRemoved += Party_PartyMemberChanged;
			entityPlayer.Party.PartyLeaderChanged += Party_PartyLeaderChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Party_PartyLeaderChanged(Party _affectedParty, EntityPlayer _player)
	{
		RefreshPartyList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Party_PartyMemberChanged(EntityPlayer _player)
	{
		RefreshPartyList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EntityPlayer_PartyJoined(Party _affectedParty, EntityPlayer _player)
	{
		RefreshPartyList();
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		if (entityPlayer.Party != null)
		{
			entityPlayer.Party.PartyMemberAdded -= Party_PartyMemberChanged;
			entityPlayer.Party.PartyMemberRemoved -= Party_PartyMemberChanged;
			entityPlayer.Party.PartyLeaderChanged -= Party_PartyLeaderChanged;
			entityPlayer.Party.PartyMemberAdded += Party_PartyMemberChanged;
			entityPlayer.Party.PartyMemberRemoved += Party_PartyMemberChanged;
			entityPlayer.Party.PartyLeaderChanged += Party_PartyLeaderChanged;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		entityPlayer.PartyJoined -= EntityPlayer_PartyJoined;
		entityPlayer.PartyChanged -= EntityPlayer_PartyJoined;
		entityPlayer.PartyLeave -= EntityPlayer_PartyJoined;
		if (entityPlayer.Party != null)
		{
			entityPlayer.Party.PartyMemberAdded -= Party_PartyMemberChanged;
			entityPlayer.Party.PartyMemberRemoved -= Party_PartyMemberChanged;
			entityPlayer.Party.PartyLeaderChanged -= Party_PartyLeaderChanged;
		}
	}

	public void RefreshPartyList()
	{
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		int i = 0;
		if (entityPlayer.Party != null)
		{
			for (int j = 0; j < entityPlayer.Party.MemberList.Count; j++)
			{
				EntityPlayer entityPlayer2 = entityPlayer.Party.MemberList[j];
				if (i >= entryList.Count)
				{
					break;
				}
				if (entityPlayer2 != entityPlayer)
				{
					entryList[i++].SetPlayer(entityPlayer2);
				}
			}
			for (; i < entryList.Count; i++)
			{
				entryList[i].SetPlayer(null);
			}
		}
		else
		{
			for (int k = 0; k < entryList.Count; k++)
			{
				entryList[k].SetPlayer(null);
			}
		}
	}
}
