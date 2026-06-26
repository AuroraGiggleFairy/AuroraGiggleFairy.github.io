using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EditorPanelSelector : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> windowNames = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button selected;

	public new XUiV_Button Selected
	{
		get
		{
			return selected;
		}
		set
		{
			if (selected != value)
			{
				if (selected != null)
				{
					selected.Selected = false;
				}
				selected = value;
				if (selected != null)
				{
					selected.Selected = true;
				}
				IsDirty = true;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		XUiController childById = GetChildById("buttons");
		if (childById == null)
		{
			return;
		}
		windowNames.Clear();
		for (int i = 0; i < childById.Children.Count; i++)
		{
			XUiController xUiController = childById.Children[i];
			if (xUiController.ViewComponent.EventOnPress)
			{
				xUiController.OnPress += HandleOnPress;
				XUiV_Button xUiV_Button = xUiController.ViewComponent as XUiV_Button;
				windowNames.Add(xUiV_Button.ID);
				if (i == 0)
				{
					SetSelected(xUiV_Button.ID);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnPress(XUiController _sender, int _mouseButton)
	{
		Selected = (XUiV_Button)_sender.ViewComponent;
		OpenSelectedWindow();
	}

	public void OpenSelectedWindow()
	{
		string text = ((Selected != null) ? Selected.ID : null);
		for (int i = 0; i < windowNames.Count; i++)
		{
			if (text == null || text != windowNames[i])
			{
				base.xui.playerUI.windowManager.Close(windowNames[i]);
			}
		}
		if (text != null)
		{
			base.xui.playerUI.windowManager.OpenIfNotOpen(text, _bModal: false);
		}
	}

	public void SetSelected(string name)
	{
		XUiController childById = GetChildById(name);
		if (childById != null && childById.ViewComponent is XUiV_Button)
		{
			Selected = (XUiV_Button)childById.ViewComponent;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (PrefabEditModeManager.Instance.IsActive() && PrefabEditModeManager.Instance.VoxelPrefab != null)
		{
			PrefabEditModeManager.Instance.VoxelPrefab.RenderingCostStats = WorldStats.CaptureWorldStats();
		}
		OpenSelectedWindow();
	}

	public override void OnClose()
	{
		base.OnClose();
		for (int i = 0; i < windowNames.Count; i++)
		{
			base.xui.playerUI.windowManager.Close(windowNames[i]);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings();
			IsDirty = false;
		}
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "panelname")
		{
			value = ((selected != null) ? Selected.ToolTip : "");
			return true;
		}
		return base.GetBindingValue(ref value, bindingName);
	}
}
