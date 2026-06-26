using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ITileEntitySignable SignTileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput textInput;

	public override void Init()
	{
		base.Init();
		XUiV_Button obj = (XUiV_Button)GetChildById("clickable").ViewComponent;
		textInput = GetChildByType<XUiC_TextInput>();
		textInput.OnSubmitHandler += TextInput_OnSubmitHandler;
		textInput.SupportBbCode = false;
		obj.Controller.OnPress += closeButton_OnPress;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TextInput_OnSubmitHandler(XUiController _sender, string _text)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
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
		textInput.Text = GeneratedTextManager.GetDisplayTextImmediately(SignTileEntity.GetAuthoredText(), _checkBlockState: true, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
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
		if (!SignTileEntity.CanRenderString(textInput.Text))
		{
			GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "uiInvalidCharacters");
			FinishClosing();
			return;
		}
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(base.xui.playerUI.entityPlayer.entityId);
		SignTileEntity.SetText(textInput.Text, _syncData: true, playerDataFromEntityID?.PrimaryId);
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
