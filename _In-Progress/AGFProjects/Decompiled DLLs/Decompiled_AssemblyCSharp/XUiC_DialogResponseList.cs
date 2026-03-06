using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DialogResponseList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Dialog conversation;

	[PublicizedFrom(EAccessModifier.Private)]
	public string xuiQuestDescriptionLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_DialogResponseEntry> entryList = new List<XUiC_DialogResponseEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> uniqueResponseIDs = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dialog currentDialog;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblResponderName;

	public Dialog CurrentDialog
	{
		get
		{
			return currentDialog;
		}
		set
		{
			currentDialog = value;
			RefreshBindings(_forceAll: true);
			IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		xuiQuestDescriptionLabel = Localization.Get("xuiDescriptionLabel");
		lblResponderName = GetChildById("lblName").ViewComponent as XUiV_Label;
		XUiController childById = GetChildById("items");
		for (int i = 0; i < childById.Children.Count; i++)
		{
			if (childById.Children[i] is XUiC_DialogResponseEntry item)
			{
				entryList.Add(item);
				length++;
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		lblResponderName.Text = Localization.Get(base.xui.Dialog.Respondent.EntityName);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!IsDirty)
		{
			return;
		}
		List<BaseResponseEntry> list = new List<BaseResponseEntry>();
		uniqueResponseIDs.Clear();
		if (currentDialog != null)
		{
			list = currentDialog.CurrentStatement.GetResponses();
		}
		int num = 0;
		for (int i = 0; i < entryList.Count; i++)
		{
			XUiC_DialogResponseEntry xUiC_DialogResponseEntry = entryList[i];
			if (xUiC_DialogResponseEntry == null)
			{
				continue;
			}
			xUiC_DialogResponseEntry.OnPress -= OnPressResponse;
			if (num < list.Count)
			{
				xUiC_DialogResponseEntry.ViewComponent.SoundPlayOnClick = true;
				if (list[num].UniqueID == "" || !uniqueResponseIDs.Contains(list[num].UniqueID))
				{
					xUiC_DialogResponseEntry.CurrentResponse = list[num].Response;
					xUiC_DialogResponseEntry.OnPress += OnPressResponse;
				}
				else
				{
					xUiC_DialogResponseEntry.CurrentResponse = null;
				}
				if (xUiC_DialogResponseEntry.CurrentResponse == null)
				{
					i--;
				}
				else if (list[num].UniqueID != "")
				{
					uniqueResponseIDs.Add(list[num].UniqueID);
				}
				num++;
			}
			else
			{
				xUiC_DialogResponseEntry.ViewComponent.SoundPlayOnClick = false;
				xUiC_DialogResponseEntry.CurrentResponse = null;
			}
		}
		if (list.Count > 0)
		{
			entryList[0].SelectCursorElement(_withDelay: true);
		}
		IsDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressResponse(XUiController _sender, int _mouseButton)
	{
		if (((XUiC_DialogResponseEntry)_sender).HasRequirement)
		{
			DialogResponse currentResponse = ((XUiC_DialogResponseEntry)_sender).CurrentResponse;
			currentDialog.SelectResponse(currentResponse, base.xui.playerUI.entityPlayer);
			((XUiC_DialogWindowGroup)windowGroup.Controller).RefreshDialog();
		}
	}

	public void Refresh()
	{
		IsDirty = true;
		RefreshBindings(_forceAll: true);
	}
}
