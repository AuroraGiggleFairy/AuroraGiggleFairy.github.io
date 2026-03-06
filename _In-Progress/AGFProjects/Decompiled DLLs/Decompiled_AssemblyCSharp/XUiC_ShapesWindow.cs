using System;
using System.Collections.Generic;
using GUI_2;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ShapesWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label resultCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ShapeStackGrid shapeGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showFavorites;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button favoritesBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController paintbrushButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController paintrollerButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblPaintEyeDropper;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblCopyBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTotal;

	public ItemValue ItemValue = ItemValue.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public Block[] altBlocks;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ShapesFromXml.ShapeCategory> shapeCategories = new List<ShapesFromXml.ShapeCategory>();

	public XUiC_ItemStack StackController;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_ShapeStackGrid.ShapeData> currentItems = new List<XUiC_ShapeStackGrid.ShapeData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openedBefore;

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
				shapeGrid.Page = page;
				pager?.SetPage(page);
			}
		}
	}

	public override void Init()
	{
		base.Init();
		resultCount = (XUiV_Label)GetChildById("resultCount").ViewComponent;
		pager = GetChildByType<XUiC_Paging>();
		if (pager != null)
		{
			pager.OnPageChanged += [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				Page = pager.CurrentPageNumber;
			};
		}
		for (int num = 0; num < children.Count; num++)
		{
			children[num].OnScroll += HandleOnScroll;
		}
		base.OnScroll += HandleOnScroll;
		shapeGrid = base.Parent.GetChildByType<XUiC_ShapeStackGrid>();
		XUiController[] childrenByType = shapeGrid.GetChildrenByType<XUiC_ShapeStack>();
		XUiController[] array = childrenByType;
		shapeGrid.Owner = this;
		for (int num2 = 0; num2 < array.Length; num2++)
		{
			array[num2].OnScroll += HandleOnScroll;
		}
		length = array.Length;
		txtInput = (XUiC_TextInput)windowGroup.Controller.GetChildById("searchInput");
		if (txtInput != null)
		{
			txtInput.OnChangeHandler += HandleOnChangedHandler;
			txtInput.Text = "";
		}
		XUiController childById = GetChildById("favorites");
		if (childById != null)
		{
			favoritesBtn = childById.ViewComponent as XUiV_Button;
			if (favoritesBtn != null)
			{
				childById.OnPress += HandleFavoritesChanged;
			}
		}
		lblTotal = Localization.Get("lblTotalItems");
		categoryList = (XUiC_CategoryList)GetChildById("categories");
		if (categoryList != null)
		{
			categoryList.AllowUnselect = true;
			categoryList.CategoryChanged += CategoryList_CategoryChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleFavoritesChanged(XUiController _sender, int _mouseButton)
	{
		showFavorites = !showFavorites;
		favoritesBtn.Selected = showFavorites;
		UpdateAll();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnChangedHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		Page = 0;
		UpdateShapesList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			pager?.PageDown();
		}
		else
		{
			pager?.PageUp();
		}
	}

	public void UpgradeDowngradeShapes(BlockValue _targetBv)
	{
		string blockName = _targetBv.Block.GetBlockName();
		Block autoShapeHelperBlock = _targetBv.Block.GetAutoShapeHelperBlock();
		ItemValue itemValue = new BlockValue((uint)autoShapeHelperBlock.blockID).ToItemValue();
		itemValue.Meta = autoShapeHelperBlock.GetAlternateBlockIndex(blockName);
		ItemValue = itemValue;
		UpdateAll();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateAltList()
	{
		altBlocks = ItemValue.ToBlockValue().Block.GetAltBlocks();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCategories()
	{
		string text = categoryList.CurrentCategory?.CategoryName;
		Block.GetShapeCategories(altBlocks, shapeCategories);
		for (int i = 0; i < categoryList.Children.Count; i++)
		{
			if (i < shapeCategories.Count)
			{
				categoryList.SetCategoryEntry(i, shapeCategories[i].Name, shapeCategories[i].Icon, shapeCategories[i].LocalizedName);
			}
			else
			{
				categoryList.SetCategoryEmpty(i);
			}
		}
		if (text != null)
		{
			categoryList.SetCategory(text);
		}
		else if (!openedBefore)
		{
			categoryList.SetCategoryToFirst();
			openedBefore = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateShapesList()
	{
		List<string> favoriteShapes = base.xui.playerUI.entityPlayer.favoriteShapes;
		currentItems.Clear();
		length = shapeGrid.Length;
		string text = txtInput.Text;
		string text2 = categoryList.CurrentCategory?.CategoryName;
		ShapesFromXml.ShapeCategory shapeCategory = null;
		if (!string.IsNullOrEmpty(text2))
		{
			shapeCategory = ShapesFromXml.shapeCategories[text2];
		}
		for (int i = 0; i < altBlocks.Length; i++)
		{
			Block block = altBlocks[i];
			string blockName = block.GetBlockName();
			string localizedBlockName = block.GetLocalizedBlockName();
			if ((!showFavorites || favoriteShapes.Contains(XUiC_ShapeStack.GetFavoritesEntryName(block))) && (string.IsNullOrEmpty(text) || blockName.ContainsCaseInsensitive(text) || localizedBlockName.ContainsCaseInsensitive(text)) && (shapeCategory == null || block.ShapeCategories.Contains(shapeCategory)) && !block.Properties.GetString("ShapeMenu").EqualsCaseInsensitive("false"))
			{
				currentItems.Add(new XUiC_ShapeStackGrid.ShapeData
				{
					Block = block,
					Index = i
				});
			}
		}
		currentItems.Sort([PublicizedFrom(EAccessModifier.Internal)] (XUiC_ShapeStackGrid.ShapeData _shapeA, XUiC_ShapeStackGrid.ShapeData _shapeB) => StringComparer.Ordinal.Compare(_shapeA.Block.SortOrder, _shapeB.Block.SortOrder));
		pager?.SetLastPageByElementsAndPageLength(currentItems.Count, length);
		shapeGrid.SetShapes(currentItems, ItemValue.Meta);
		int num = currentItems.FindIndex([PublicizedFrom(EAccessModifier.Private)] (XUiC_ShapeStackGrid.ShapeData _data) => _data.Index == ItemValue.Meta);
		if (num < 0)
		{
			Page = 0;
		}
		else
		{
			Page = num / length;
		}
		resultCount.Text = string.Format(lblTotal, currentItems.Count.ToString());
	}

	public void UpdateAll()
	{
		updateAltList();
		updateCategories();
		UpdateShapesList();
		IsDirty = true;
	}

	public void RefreshItemStack()
	{
		_ = StackController;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftTrigger, "igcoCategoryLeft", XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightTrigger, "igcoCategoryRight", XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, "igcoToggleFavorite", XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
		int holdingItemIdx = base.xui.playerUI.entityPlayer.inventory.holdingItemIdx;
		XUiC_Toolbelt childByType = ((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow("toolbelt")).Controller.GetChildByType<XUiC_Toolbelt>();
		base.xui.dragAndDrop.InMenu = true;
		if (childByType != null)
		{
			StackController = childByType.GetSlotControl(holdingItemIdx);
			StackController.AssembleLock = true;
		}
		windowGroup.Controller.GetChildByType<XUiC_WindowNonPagingHeader>().SetHeader(Localization.Get("xuiShapes").ToUpper());
		UpdateAll();
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.currentToolTip.ToolTip = "";
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
		XUiC_Toolbelt childByType = ((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow("toolbelt")).Controller.GetChildByType<XUiC_Toolbelt>();
		base.xui.dragAndDrop.InMenu = false;
		if (childByType != null)
		{
			StackController.AssembleLock = false;
			StackController.ItemStack = new ItemStack(ItemValue, StackController.ItemStack.count);
			ItemValue = ItemValue.None;
			StackController.ForceRefreshItemStack();
		}
		if (base.xui.playerUI.windowManager.IsWindowOpen("windowpaging"))
		{
			base.xui.playerUI.windowManager.Close("windowpaging");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CategoryList_CategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		UpdateShapesList();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (viewComponent.IsVisible && IsDirty)
		{
			RefreshBindings();
			shapeGrid.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "keyboardonly")
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
			{
				_value = "true";
			}
			else
			{
				_value = "false";
			}
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}
}
