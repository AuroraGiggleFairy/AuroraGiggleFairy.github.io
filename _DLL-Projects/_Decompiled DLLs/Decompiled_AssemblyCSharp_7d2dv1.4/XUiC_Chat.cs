using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Chat : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<EChatType> cbxTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> entityIdsFriends = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> entityIdsParty = new List<int>();

	public override void Init()
	{
		ID = windowGroup.ID;
		base.Init();
		cbxTarget = GetChildByType<XUiC_ComboBoxList<EChatType>>();
		txtInput = GetChildByType<XUiC_TextInput>();
		txtInput.OnSubmitHandler += TextInput_OnSubmitHandler;
		txtInput.OnInputAbortedHandler += TextInput_OnInputAbortedHandler;
		txtInput.SupportBbCode = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TextInput_OnInputAbortedHandler(XUiController _sender)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TextInput_OnSubmitHandler(XUiController _sender, string _text)
	{
		if (_text.Length > 0 && _text != " ")
		{
			_text = _text.Replace('\n', ' ');
			List<int> recipientEntityIds = null;
			EChatType value = cbxTarget.Value;
			switch (value)
			{
			case EChatType.Friends:
				recipientEntityIds = entityIdsFriends;
				break;
			case EChatType.Party:
				recipientEntityIds = entityIdsParty;
				break;
			case EChatType.Whisper:
				throw new NotImplementedException("Whisper not yet implemented");
			}
			GameManager.Instance.ChatMessageServer(null, value, base.xui.playerUI.entityPlayer.entityId, _text, recipientEntityIds, EMessageSender.SenderIdAsPlayer);
			txtInput.Text = "";
		}
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	public override void OnOpen()
	{
		cbxTarget.Enabled = PermissionsManager.IsCommunicationAllowed();
		txtInput.Enabled = PermissionsManager.IsCommunicationAllowed();
		entityIdsFriends.Clear();
		entityIdsFriends.Add(base.xui.playerUI.entityPlayer.entityId);
		foreach (EntityPlayer item in GameManager.Instance.World.Players.list)
		{
			if (item.IsFriendOfLocalPlayer)
			{
				entityIdsFriends.Add(item.entityId);
			}
		}
		entityIdsParty.Clear();
		entityIdsParty.Add(base.xui.playerUI.entityPlayer.entityId);
		if (base.xui.playerUI.entityPlayer.Party != null)
		{
			foreach (EntityPlayer member in base.xui.playerUI.entityPlayer.Party.MemberList)
			{
				if (member != base.xui.playerUI.entityPlayer)
				{
					entityIdsParty.Add(member.entityId);
				}
			}
		}
		cbxTarget.Elements.Clear();
		cbxTarget.Elements.Add(EChatType.Global);
		if (entityIdsFriends.Count > 1)
		{
			cbxTarget.Elements.Add(EChatType.Friends);
		}
		if (entityIdsParty.Count > 1)
		{
			cbxTarget.Elements.Add(EChatType.Party);
		}
		base.OnOpen();
	}

	public override void UpdateInput()
	{
		base.UpdateInput();
		if (base.xui.playerUI.playerInput == null)
		{
			return;
		}
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			PlayerActionsGUI gUIActions = base.xui.playerUI.playerInput.GUIActions;
			if (gUIActions.Up.WasPressed)
			{
				cbxTarget.ChangeIndex(-1);
			}
			if (gUIActions.Down.WasPressed)
			{
				cbxTarget.ChangeIndex(1);
			}
		}
		if (base.xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed)
		{
			base.xui.playerUI.windowManager.Close(windowGroup.ID);
		}
	}
}
