using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ITileEntitySignable SignTileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool separateLineMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput textInput1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput textInput2;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput textInput3;

	public override void Init()
	{
		base.Init();
		XUiV_Button obj = (XUiV_Button)GetChildById("clickable").ViewComponent;
		textInput1 = GetChildById("input1") as XUiC_TextInput;
		textInput1.OnSubmitHandler += TextInput_OnSubmitHandler;
		textInput1.SupportBbCode = false;
		textInput2 = GetChildById("input2") as XUiC_TextInput;
		if (textInput2 != null)
		{
			textInput2.OnSubmitHandler += TextInput_OnSubmitHandler;
			textInput2.SupportBbCode = false;
			textInput3 = GetChildById("input3") as XUiC_TextInput;
			textInput3.OnSubmitHandler += TextInput_OnSubmitHandler;
			textInput3.SupportBbCode = false;
			textInput1.SelectOnTab = textInput2;
			textInput2.SelectOnTab = textInput3;
			textInput3.SelectOnTab = textInput1;
			separateLineMode = true;
		}
		obj.Controller.OnPress += closeButton_OnPress;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TextInput_OnSubmitHandler(XUiController _sender, string _text)
	{
		if (separateLineMode)
		{
			if (_sender is XUiC_TextInput xUiC_TextInput)
			{
				xUiC_TextInput.SelectOnTab.SelectCursorElement();
				xUiC_TextInput.SelectOnTab.SetSelected();
			}
		}
		else
		{
			base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeButton_OnPress(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	public void SetTileEntitySign(ITileEntitySignable _te)
	{
		SignTileEntity = _te;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		string displayTextImmediately = GeneratedTextManager.GetDisplayTextImmediately(SignTileEntity.GetAuthoredText(), _checkBlockState: true, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
		if (separateLineMode)
		{
			XUiC_TextInput xUiC_TextInput = textInput1;
			XUiC_TextInput xUiC_TextInput2 = textInput2;
			string text = (textInput3.Text = "");
			string text3 = (xUiC_TextInput2.Text = text);
			xUiC_TextInput.Text = text3;
			string[] array = displayTextImmediately.Split('\n');
			if (array.Length != 0)
			{
				textInput1.Text = array[0];
				if (array.Length > 1)
				{
					textInput2.Text = array[1];
					if (array.Length > 2)
					{
						textInput3.Text = array[2];
					}
				}
			}
		}
		else
		{
			textInput1.Text = displayTextImmediately;
		}
		base.xui.playerUI.entityPlayer.PlayOneShot("open_sign");
		base.xui.playerUI.CursorController.SetNavigationLockView(base.ViewComponent);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.entityPlayer.PlayOneShot("close_sign");
		base.xui.playerUI.CursorController.SetNavigationLockView(null);
		if (GameManager.Instance.World.GetTileEntity(SignTileEntity.GetClrIdx(), SignTileEntity.ToWorldPos()).GetSelfOrFeature<ITileEntitySignable>() != SignTileEntity)
		{
			FinishClosing();
			return;
		}
		string text = (separateLineMode ? $"{textInput1.Text}\n{textInput2.Text}\n{textInput3.Text}" : textInput1.Text);
		if (!SignTileEntity.CanRenderString(text))
		{
			GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "uiInvalidCharacters");
			FinishClosing();
			return;
		}
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(base.xui.playerUI.entityPlayer.entityId);
		SignTileEntity.SetText(text, _syncData: true, playerDataFromEntityID?.PrimaryId);
		GeneratedTextManager.GetDisplayText(SignTileEntity.GetAuthoredText(), [PublicizedFrom(EAccessModifier.Private)] (string _) =>
		{
			FinishClosing();
		}, _runCallbackIfReadyNow: true, _checkBlockState: false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.NotSupported);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FinishClosing()
	{
		SignTileEntity.SetUserAccessing(_bUserAccessing: false);
		GameManager.Instance.TEUnlockServer(SignTileEntity.GetClrIdx(), SignTileEntity.ToWorldPos(), SignTileEntity.EntityId);
	}
}
