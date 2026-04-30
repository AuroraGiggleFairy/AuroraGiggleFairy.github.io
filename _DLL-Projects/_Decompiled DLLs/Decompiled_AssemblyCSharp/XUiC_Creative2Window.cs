using System;
using System.Collections.Generic;
using System.Text;
using GUI_2;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Creative2Window : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string FilterTypeAutoshapes = "autoshapes";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string FilterTextAutoshapesOther = "$other";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string FilterTypeItemgroups = "itemgroups";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string FilterTypeBlocktags = "blockfiltertags";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string FilterTypeShapecategory = "shapecategory";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button simpleClickButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button devBlockButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button favorites;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button hideshapes;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Creative2StackGrid creativeGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_CategoryList subCategoryListShapes;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_CategoryEntry currentMainCategory;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_CategoryEntry currentSubCategory;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpening;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool simpleClick;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool allowDevBlocks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showDevBlocks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showFavorites;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hideShapes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool RefreshList;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringBuilder headerNameBuilder = new StringBuilder();

	[PublicizedFrom(EAccessModifier.Private)]
	public string filterCrumbs;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ItemClass.FilterItem[] currentFilterExcludeFuncs = new ItemClass.FilterItem[3];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ItemClass> filteredItems = new List<ItemClass>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ItemStack> itemStacks = new List<ItemStack>();

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumCreativeMode currentCreativeMode = EnumCreativeMode.Player;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Block> otherShapeBlocks = new List<Block>();

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
				creativeGrid.Page = page;
				pager?.SetPage(page);
			}
		}
	}

	public override void Init()
	{
		base.Init();
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
		XUiController childById = GetChildById("simplepickup");
		if (childById != null)
		{
			simpleClickButton = childById.ViewComponent as XUiV_Button;
			if (simpleClickButton != null)
			{
				childById.OnPress += SimpleClickButton_OnPress;
			}
		}
		allowDevBlocks = !(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() || !Submission.Enabled;
		if (allowDevBlocks)
		{
			XUiController childById2 = GetChildById("devblocks");
			if (childById2 != null)
			{
				devBlockButton = childById2.ViewComponent as XUiV_Button;
				if (devBlockButton != null)
				{
					childById2.OnPress += DevBlockButton_OnPress;
				}
			}
		}
		XUiController childById3 = GetChildById("favorites");
		if (childById3 != null)
		{
			favorites = childById3.ViewComponent as XUiV_Button;
			if (favorites != null)
			{
				childById3.OnPress += HandleFavoritesChanged;
			}
		}
		XUiController childById4 = GetChildById("hideshapes");
		if (childById4 != null)
		{
			hideshapes = childById4.ViewComponent as XUiV_Button;
			if (hideshapes != null)
			{
				childById4.OnPress += Hideshapes_Changed;
			}
		}
		creativeGrid = base.Parent.GetChildByType<XUiC_Creative2StackGrid>();
		XUiC_ItemStack[] childrenByType = creativeGrid.GetChildrenByType<XUiC_ItemStack>();
		for (int num2 = 0; num2 < childrenByType.Length; num2++)
		{
			childrenByType[num2].OnScroll += HandleOnScroll;
		}
		txtInput = windowGroup.Controller.GetChildById("searchInput") as XUiC_TextInput;
		if (txtInput != null)
		{
			txtInput.OnChangeHandler += TxtInput_OnChange;
			txtInput.Text = "";
		}
		setupMainCategories((XUiC_CategoryList)GetChildById("categories"));
		List<XUiC_CategoryList> list = new List<XUiC_CategoryList>();
		GetChildrenByType(list);
		foreach (XUiC_CategoryList item in list)
		{
			if (!item.ViewComponent.ID.StartsWith("sub", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			item.CategoryChanged += SubCategory_CategoryChanged;
			item.OnVisiblity += [PublicizedFrom(EAccessModifier.Private)] (XUiController _sender, bool _visible) =>
			{
				if (!isOpening && (_visible || currentSubCategory?.CategoryList == _sender))
				{
					((XUiC_CategoryList)_sender)?.HandleCategoryChanged();
				}
			};
		}
		subCategoryListShapes = (XUiC_CategoryList)GetChildById("subCategoriesShapes");
		currentCreativeMode = ((!GameManager.Instance.IsEditMode()) ? EnumCreativeMode.Player : EnumCreativeMode.Dev);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Hideshapes_Changed(XUiController _sender, int _mouseButton)
	{
		hideShapes = !hideShapes;
		hideshapes.Selected = hideShapes;
		RefreshList = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleFavoritesChanged(XUiController _sender, int _mouseButton)
	{
		showFavorites = !showFavorites;
		favorites.Selected = showFavorites;
		RefreshList = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CategoryList_CategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		currentFilterExcludeFuncs[0] = null;
		currentFilterExcludeFuncs[1] = null;
		currentMainCategory = _categoryEntry;
		RefreshList = true;
		IsDirty = true;
		if (_categoryEntry != null)
		{
			currentFilterExcludeFuncs[0] = buildFilter(_categoryEntry);
			Block block = categoryIsBlockShapes(_categoryEntry);
			if (block != null)
			{
				setupSubcategoriesForShapes(block);
			}
			else if (categoryIsOtherShapes(_categoryEntry))
			{
				setupSubcategoriesForShapes(otherShapeBlocks[0]);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SubCategory_CategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		currentFilterExcludeFuncs[1] = null;
		currentSubCategory = _categoryEntry;
		IsDirty = true;
		RefreshList = true;
		if (currentMainCategory != null)
		{
			currentFilterExcludeFuncs[1] = buildFilter(currentSubCategory);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtInput_OnChange(XUiController _sender, string _text, bool _changeFromCode)
	{
		RefreshList = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DevBlockButton_OnPress(XUiController _sender, int _mouseButton)
	{
		showDevBlocks = !showDevBlocks;
		devBlockButton.Selected = showDevBlocks;
		RefreshList = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SimpleClickButton_OnPress(XUiController _sender, int _mouseButton)
	{
		simpleClick = !simpleClick;
		simpleClickButton.Selected = simpleClick;
		XUiC_ItemStack[] childrenByType = creativeGrid.GetChildrenByType<XUiC_ItemStack>();
		for (int i = 0; i < childrenByType.Length; i++)
		{
			childrenByType[i].SimpleClick = simpleClick;
		}
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateHeader()
	{
		if (currentMainCategory == null)
		{
			filterCrumbs = Localization.Get("lblAll");
			return;
		}
		headerNameBuilder.Clear();
		headerNameBuilder.Append(currentMainCategory.CategoryDisplayName);
		if (currentSubCategory == null)
		{
			filterCrumbs = headerNameBuilder.ToString();
			return;
		}
		headerNameBuilder.Append(" | ");
		headerNameBuilder.Append(currentSubCategory.CategoryDisplayName);
		filterCrumbs = headerNameBuilder.ToString();
	}

	public void Refresh()
	{
		updateHeader();
		string nameFilter = txtInput?.Text;
		currentFilterExcludeFuncs[2] = null;
		if (hideShapes && currentMainCategory == null)
		{
			currentFilterExcludeFuncs[2] = [PublicizedFrom(EAccessModifier.Internal)] (ItemClass _class, Block _block) => _block != null && _block.GetAutoShapeType() != EAutoShapeType.None;
		}
		filteredItems.Clear();
		ItemClass.GetItemsAndBlocks(filteredItems, -1, -1, currentFilterExcludeFuncs, nameFilter, showDevBlocks, currentCreativeMode, showFavorites, _sortBySortOrder: true, base.xui);
		length = creativeGrid.Length;
		Page = 0;
		IsDirty = true;
		ItemClass.CreateItemStacks(filteredItems, itemStacks);
		creativeGrid.SetSlots(itemStacks.ToArray());
		pager?.SetLastPageByElementsAndPageLength(itemStacks.Count, length);
	}

	public void RefreshView()
	{
		IsDirty = true;
		creativeGrid.IsDirty = true;
	}

	public override void OnOpen()
	{
		isOpening = true;
		base.OnOpen();
		isOpening = false;
		Refresh();
		if (!GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) && !GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled))
		{
			base.xui.playerUI.windowManager.CloseAllOpenWindows();
		}
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftTrigger, "igcoCategoryLeft", XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightTrigger, "igcoCategoryRight", XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.currentToolTip.ToolTip = "";
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			IsDirty = false;
			RefreshBindings();
		}
		if (RefreshList)
		{
			RefreshList = false;
			Refresh();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "filter_crumb":
			_value = filterCrumbs ?? "";
			return true;
		case "result_count":
			_value = itemStacks.Count.ToString();
			return true;
		case "main_category_is_shapes":
			_value = "autoshapes".EqualsCaseInsensitive(categoryFilterType(currentMainCategory)).ToString();
			return true;
		case "main_category_name":
			_value = currentMainCategory?.CategoryName ?? "";
			return true;
		case "allow_dev":
			_value = allowDevBlocks.ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass.FilterItem buildFilter(XUiC_CategoryEntry _categoryEntry)
	{
		string text = categoryFilterType(_categoryEntry);
		if (text == null)
		{
			if (_categoryEntry != null)
			{
				Log.Warning("[XUi] CreativeMenu: No filtertype on sub category button '" + _categoryEntry.CategoryName + "'");
			}
			return null;
		}
		string filtertext = categoryFilterText(_categoryEntry);
		if (text.EqualsCaseInsensitive("autoshapes"))
		{
			if (filtertext == null)
			{
				return [PublicizedFrom(EAccessModifier.Internal)] (ItemClass _class, Block _block) => _block == null || _block.GetAutoShapeType() == EAutoShapeType.None;
			}
			if (filtertext.EqualsCaseInsensitive("$other"))
			{
				return [PublicizedFrom(EAccessModifier.Internal)] (ItemClass _class, Block _block) =>
				{
					if (_block == null || _block.GetAutoShapeType() == EAutoShapeType.None)
					{
						return true;
					}
					for (int i = 0; i < otherShapeBlocks.Count; i++)
					{
						Block block = otherShapeBlocks[i];
						if (_block == block || _block.GetAutoShapeHelperBlock() == block)
						{
							return false;
						}
					}
					return true;
				};
			}
			string text2 = filtertext + ":" + ShapesFromXml.VariantHelperName;
			Block shapesHelper = Block.GetBlockByName(text2);
			if (shapesHelper == null)
			{
				Log.Warning("[XUi] CreativeMenu: Filtering on autoshapes with baseblock '" + text2 + "' failed: Block not found!");
				return null;
			}
			return [PublicizedFrom(EAccessModifier.Internal)] (ItemClass _class, Block _block) => _block == null || _block.GetAutoShapeType() == EAutoShapeType.None || (_block != shapesHelper && _block.GetAutoShapeHelperBlock() != shapesHelper);
		}
		if (text.EqualsCaseInsensitive("shapecategory"))
		{
			if (filtertext == null)
			{
				return null;
			}
			if (!ShapesFromXml.shapeCategories.TryGetValue(filtertext, out var shapeCategory))
			{
				Log.Warning("[XUi] CreativeMenu: Filtering on autoshapes with category '" + filtertext + "' failed: Unknown shape category!");
				return null;
			}
			return [PublicizedFrom(EAccessModifier.Internal)] (ItemClass _class, Block _block) => _block?.ShapeCategories == null || !_block.ShapeCategories.Contains(shapeCategory);
		}
		if (text.EqualsCaseInsensitive("itemgroups"))
		{
			if (string.IsNullOrEmpty(filtertext))
			{
				return [PublicizedFrom(EAccessModifier.Internal)] (ItemClass _class, Block _block) => _block != null;
			}
			return [PublicizedFrom(EAccessModifier.Internal)] (ItemClass _class, Block _block) =>
			{
				if (_block != null)
				{
					return true;
				}
				for (int i = 0; i < _class.Groups.Length; i++)
				{
					if (_class.Groups[i] == filtertext)
					{
						return false;
					}
				}
				return true;
			};
		}
		if (text.EqualsCaseInsensitive("blockfiltertags"))
		{
			if (string.IsNullOrEmpty(filtertext))
			{
				return [PublicizedFrom(EAccessModifier.Internal)] (ItemClass _class, Block _block) => _block == null;
			}
			return [PublicizedFrom(EAccessModifier.Internal)] (ItemClass _class, Block _block) =>
			{
				if (_block?.FilterTags != null)
				{
					for (int i = 0; i < _block.FilterTags.Length; i++)
					{
						if (_block.FilterTags[i] == filtertext)
						{
							return false;
						}
					}
				}
				return true;
			};
		}
		Log.Warning("[XUi] CreativeMenu: Unknown filtertype '" + text + "' on sub category button '" + _categoryEntry.CategoryName + "'");
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupMainCategories(XUiC_CategoryList _categoryList)
	{
		if (_categoryList == null)
		{
			return;
		}
		List<Block> individualButtonShapes = new List<Block>();
		for (int i = 0; i < _categoryList.MaxCategories; i++)
		{
			XUiC_CategoryEntry categoryByIndex = _categoryList.GetCategoryByIndex(i);
			Block block = categoryIsBlockShapes(categoryByIndex);
			if (block != null)
			{
				categoryByIndex.CategoryName = block.GetBlockName();
				categoryByIndex.CategoryDisplayName = block.blockMaterial.GetLocalizedMaterialName();
				individualButtonShapes.Add(block);
			}
		}
		List<ItemClass> list = new List<ItemClass>();
		ItemClass.GetItemsAndBlocks(list, -1, Block.ItemsStartHere, new ItemClass.FilterItem[1]
		{
			[PublicizedFrom(EAccessModifier.Internal)] (ItemClass _class, Block _block) => _block == null || _block.GetAutoShapeType() != EAutoShapeType.Helper || individualButtonShapes.Contains(_block)
		}, null, _bShowUserHidden: true);
		foreach (ItemClass item in list)
		{
			otherShapeBlocks.Add(item.GetBlock());
		}
		_categoryList.CategoryChanged += CategoryList_CategoryChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Block categoryIsBlockShapes(XUiC_CategoryEntry _categoryEntry)
	{
		if (!"autoshapes".EqualsCaseInsensitive(categoryFilterType(_categoryEntry)))
		{
			return null;
		}
		string text = categoryFilterText(_categoryEntry);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		return Block.GetBlockByName(text + ":" + ShapesFromXml.VariantHelperName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool categoryIsOtherShapes(XUiC_CategoryEntry _categoryEntry)
	{
		if ("autoshapes".EqualsCaseInsensitive(categoryFilterType(_categoryEntry)))
		{
			return "$other".EqualsCaseInsensitive(categoryFilterText(_categoryEntry));
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string categoryFilterType(XUiC_CategoryEntry _categoryEntry)
	{
		if (_categoryEntry == null)
		{
			return null;
		}
		_categoryEntry.CustomAttributes.TryGetValue("filtertype", out var value);
		return value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string categoryFilterText(XUiC_CategoryEntry _categoryEntry)
	{
		if (_categoryEntry == null)
		{
			return null;
		}
		_categoryEntry.CustomAttributes.TryGetValue("filtertext", out var value);
		return value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupSubcategoriesForShapes(Block _shapesHelper)
	{
		if (subCategoryListShapes == null)
		{
			return;
		}
		subCategoryListShapes.CategoryChanged -= SubCategory_CategoryChanged;
		string category = subCategoryListShapes.CurrentCategory?.CategoryName;
		Block[] altBlocks = _shapesHelper.GetAltBlocks();
		List<ShapesFromXml.ShapeCategory> list = new List<ShapesFromXml.ShapeCategory>();
		Block.GetShapeCategories(altBlocks, list);
		for (int i = 0; i < subCategoryListShapes.Children.Count; i++)
		{
			if (i < list.Count)
			{
				subCategoryListShapes.SetCategoryEntry(i, list[i].Name, list[i].Icon, list[i].LocalizedName);
				subCategoryListShapes.GetCategoryByIndex(i).CustomAttributes["filtertype"] = "shapecategory";
				subCategoryListShapes.GetCategoryByIndex(i).CustomAttributes["filtertext"] = list[i].Name;
			}
			else
			{
				subCategoryListShapes.SetCategoryEmpty(i);
			}
		}
		subCategoryListShapes.CategoryChanged += SubCategory_CategoryChanged;
		subCategoryListShapes.SetCategory(category);
	}
}
