using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ActiveBuffList : XUiController, IEntityUINotificationChanged
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_ActiveBuffEntry> entryList = new List<XUiC_ActiveBuffEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<EntityUINotification> buffNotificationList = new List<EntityUINotification>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ActiveBuffEntry selectedEntry;

	public bool setFirstEntry;

	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action handlePageDownAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action handlePageUpAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	public int Page
	{
		get
		{
			return page;
		}
		set
		{
			if (page != value)
			{
				page = value;
				isDirty = true;
				pager?.SetPage(page);
			}
		}
	}

	public XUiC_ActiveBuffEntry SelectedEntry
	{
		get
		{
			return selectedEntry;
		}
		set
		{
			if (selectedEntry != null)
			{
				selectedEntry.Selected = false;
			}
			selectedEntry = value;
			if (selectedEntry != null)
			{
				selectedEntry.Selected = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetFirstEntry()
	{
		XUiC_BuffInfoWindow childByType = base.WindowGroup.Controller.GetChildByType<XUiC_BuffInfoWindow>();
		SelectedEntry = ((entryList[0].Notification != null) ? entryList[0] : null);
		childByType.SetBuff(SelectedEntry);
	}

	public override void Init()
	{
		base.Init();
		XUiC_BuffInfoWindow childByType = base.WindowGroup.Controller.GetChildByType<XUiC_BuffInfoWindow>();
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i] is XUiC_ActiveBuffEntry)
			{
				XUiC_ActiveBuffEntry xUiC_ActiveBuffEntry = (XUiC_ActiveBuffEntry)children[i];
				xUiC_ActiveBuffEntry.InfoWindow = childByType;
				entryList.Add(xUiC_ActiveBuffEntry);
				length++;
			}
		}
		pager = base.Parent.GetChildByType<XUiC_Paging>();
		if (pager != null)
		{
			pager.OnPageChanged += [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				Page = pager.CurrentPageNumber;
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressBuff(XUiController _sender, int _mouseButton)
	{
		if (_sender is XUiC_ActiveBuffEntry xUiC_ActiveBuffEntry)
		{
			SelectedEntry = xUiC_ActiveBuffEntry;
		}
	}

	public override void Update(float _dt)
	{
		EntityUINotification selectedNotification = base.xui.BuffPopoutList.SelectedNotification;
		if (selectedNotification != null)
		{
			base.xui.BuffPopoutList.SelectedNotification = null;
			for (int i = 0; i < buffNotificationList.Count; i++)
			{
				if (buffNotificationList[i] == selectedNotification)
				{
					Page = i / length;
				}
			}
		}
		if (isDirty)
		{
			pager?.SetLastPageByElementsAndPageLength(buffNotificationList.Count, entryList.Count);
			pager?.SetPage(page);
			for (int j = 0; j < length; j++)
			{
				int num = j + length * page;
				XUiC_ActiveBuffEntry xUiC_ActiveBuffEntry = entryList[j];
				if (xUiC_ActiveBuffEntry != null)
				{
					xUiC_ActiveBuffEntry.OnPress -= OnPressBuff;
					if (num < buffNotificationList.Count)
					{
						xUiC_ActiveBuffEntry.Notification = buffNotificationList[num];
						xUiC_ActiveBuffEntry.OnPress += OnPressBuff;
						xUiC_ActiveBuffEntry.ViewComponent.SoundPlayOnClick = true;
					}
					else
					{
						xUiC_ActiveBuffEntry.Notification = null;
						xUiC_ActiveBuffEntry.ViewComponent.SoundPlayOnClick = false;
					}
				}
			}
			if (setFirstEntry)
			{
				SetFirstEntry();
				setFirstEntry = false;
			}
			isDirty = false;
		}
		base.Update(_dt);
		if (selectedNotification == null)
		{
			return;
		}
		for (int k = 0; k < entryList.Count; k++)
		{
			if (entryList[k].Notification == selectedNotification)
			{
				SelectedEntry = entryList[k];
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetNotificationIndex(EntityUINotification notification)
	{
		for (int i = 0; i < buffNotificationList.Count; i++)
		{
			if (buffNotificationList[i].Subject == notification.Subject)
			{
				if (notification.Subject != EnumEntityUINotificationSubject.Buff)
				{
					return i;
				}
				if (buffNotificationList[i].Buff.BuffClass.Name == notification.Buff.BuffClass.Name)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public void EntityUINotificationAdded(EntityUINotification _notification)
	{
		if (_notification.Buff != null)
		{
			int notificationIndex = GetNotificationIndex(_notification);
			if (notificationIndex == -1)
			{
				buffNotificationList.Add(_notification);
			}
			else
			{
				buffNotificationList[notificationIndex] = _notification;
			}
			isDirty = true;
		}
	}

	public void EntityUINotificationRemoved(EntityUINotification _notification)
	{
		if (_notification.Buff != null)
		{
			buffNotificationList.Remove(_notification);
			if (SelectedEntry != null && SelectedEntry.Notification == _notification)
			{
				SelectedEntry.InfoWindow.SetBuffInfo(null);
				SelectedEntry = null;
			}
			isDirty = true;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		buffNotificationList.Clear();
		List<EntityUINotification> notifications = entityPlayer.PlayerStats.Notifications;
		for (int i = 0; i < notifications.Count; i++)
		{
			if (notifications[i].Buff != null)
			{
				buffNotificationList.Add(notifications[i]);
			}
		}
		entityPlayer.PlayerStats.AddUINotificationChangedDelegate(this);
		isDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.entityPlayer.PlayerStats.RemoveUINotificationChangedDelegate(this);
	}
}
