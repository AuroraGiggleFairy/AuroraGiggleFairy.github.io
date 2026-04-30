using UnityEngine.Scripting;

[Preserve]
public class XUiC_EditorPanelSelector : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList buttons;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		buttons = GetChildByType<XUiC_CategoryList>();
		if (buttons != null)
		{
			buttons.CategoryClickChanged += ButtonsOnCategoryClickChanged;
			buttons.SetCategoryToFirst();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ButtonsOnCategoryClickChanged(XUiC_CategoryEntry _categoryEntry)
	{
		OpenSelectedWindow();
	}

	public void OpenSelectedWindow()
	{
		if (buttons == null)
		{
			return;
		}
		string text = buttons.CurrentCategory?.CategoryName;
		foreach (XUiC_CategoryEntry categoryButton in buttons.CategoryButtons)
		{
			if (text == null || text != categoryButton.CategoryName)
			{
				base.xui.playerUI.windowManager.Close(categoryButton.CategoryName);
			}
		}
		if (text != null)
		{
			base.xui.playerUI.windowManager.OpenIfNotOpen(text, _bModal: false);
		}
	}

	public void SetSelected(string _name)
	{
		buttons?.SetCategory(_name);
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
		if (buttons == null)
		{
			return;
		}
		foreach (XUiC_CategoryEntry categoryButton in buttons.CategoryButtons)
		{
			base.xui.playerUI.windowManager.Close(categoryButton.CategoryName);
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "panelname")
		{
			_value = buttons?.CurrentCategory?.CategoryDisplayName ?? "";
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}
}
