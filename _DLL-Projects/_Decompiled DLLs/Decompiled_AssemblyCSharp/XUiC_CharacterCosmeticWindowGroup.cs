using UnityEngine.Scripting;

[Preserve]
public class XUiC_CharacterCosmeticWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CharacterCosmeticWindow characterCosmeticWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CharacterCosmeticsListWindow characterCosmeticListWindow;

	public override void Init()
	{
		base.Init();
		if (GetChildByType<XUiC_WindowNonPagingHeader>() != null)
		{
			nonPagingHeaderWindow = GetChildByType<XUiC_WindowNonPagingHeader>();
		}
		if (GetChildByType<XUiC_CharacterCosmeticWindow>() != null)
		{
			characterCosmeticWindow = GetChildByType<XUiC_CharacterCosmeticWindow>();
		}
		if (GetChildByType<XUiC_CharacterCosmeticsListWindow>() != null)
		{
			characterCosmeticListWindow = GetChildByType<XUiC_CharacterCosmeticsListWindow>();
		}
	}

	public static void Open(XUi xui, EquipmentSlots slot)
	{
		GUIWindowManager windowManager = xui.playerUI.windowManager;
		windowManager.Open("cosmetics", _bModal: true);
		((XUiC_CharacterCosmeticWindowGroup)((XUiWindowGroup)windowManager.GetWindow("cosmetics")).Controller).SetEquipSlot(slot);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (nonPagingHeaderWindow != null)
		{
			nonPagingHeaderWindow.SetHeader(Localization.Get("Cosmetics"));
		}
		if (base.xui.PlayerEquipment != null)
		{
			base.xui.PlayerEquipment.IsOpen = true;
		}
	}

	public void SetEquipSlot(EquipmentSlots equipSlot)
	{
		if (equipSlot != EquipmentSlots.Count)
		{
			characterCosmeticListWindow.SetCategory(equipSlot);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		if (base.xui.PlayerEquipment != null)
		{
			base.xui.PlayerEquipment.IsOpen = false;
		}
	}

	public void ResetPreview()
	{
		characterCosmeticWindow.MakePreview();
	}
}
