using System;
using GUI_2;
using InControl;
using UnityEngine;

public class XUi_FallThrough : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public XUi xui;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool canDrop;

	public void SetXUi(XUi _xui)
	{
		xui = _xui;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		UICamera.fallThrough = base.gameObject;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		bool flag = xui.dragAndDrop != null && !xui.dragAndDrop.CurrentStack.IsEmpty();
		bool flag2 = flag && UICamera.hoveredObject != null && UICamera.hoveredObject.name.EqualsCaseInsensitive("xui") && xui.dragAndDrop.CurrentStack.itemValue.ItemClassOrMissing.CanDrop();
		int num = ((xui.dragAndDrop != null) ? xui.dragAndDrop.CurrentStack.count : 0);
		bool flag3 = false;
		LocalPlayerUI playerUI = xui.playerUI;
		if (null != playerUI && null != playerUI.uiCamera && playerUI.playerInput != null && playerUI.playerInput.GUIActions != null)
		{
			PlayerActionsGUI gUIActions = playerUI.playerInput.GUIActions;
			bool flag4 = false;
			bool flag5 = false;
			if (gUIActions.LastInputType == BindingSourceType.DeviceBindingSource)
			{
				flag4 |= gUIActions.Submit.WasReleased;
				flag5 |= gUIActions.HalfStack.WasReleased;
			}
			else
			{
				flag4 |= playerUI.CursorController.GetMouseButtonDown(UICamera.MouseButton.LeftButton);
				flag5 |= playerUI.CursorController.GetMouseButtonDown(UICamera.MouseButton.RightButton);
			}
			if (flag2)
			{
				if (flag4 || (num == 1 && flag5))
				{
					xui.dragAndDrop.DropCurrentItem();
					flag3 = true;
				}
				else if (num > 1 && flag5)
				{
					xui.dragAndDrop.DropCurrentItem(1);
					num--;
					flag3 = true;
				}
			}
		}
		if (!(flag2 != canDrop || flag3))
		{
			return;
		}
		canDrop = flag2;
		xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverAir);
		if (flag && canDrop)
		{
			if (num > 1)
			{
				xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoDropAll", XUiC_GamepadCalloutWindow.CalloutType.MenuHoverAir);
				xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonWest, "igcoDropOne", XUiC_GamepadCalloutWindow.CalloutType.MenuHoverAir);
			}
			else
			{
				xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoDrop", XUiC_GamepadCalloutWindow.CalloutType.MenuHoverAir);
			}
			xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem);
			xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverAir);
		}
		else
		{
			xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverAir);
			xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem);
		}
	}
}
