using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MaterialWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label resultCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_MaterialStackGrid materialGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblPaintEyeDropper;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblCopyBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTotal;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<BlockTextureData> currentItems = new List<BlockTextureData>();

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
				materialGrid.Page = page;
				pager?.SetPage(page);
			}
		}
	}

	public int CurrentPaintId
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (base.xui.playerUI.entityPlayer.inventory.holdingItem is ItemClassBlock)
			{
				return (int)(base.xui.playerUI.entityPlayer.inventory.holdingItemItemValue.TextureFullArray[0] & 0xFF);
			}
			return ((ItemActionTextureBlock.ItemActionTextureBlockData)base.xui.playerUI.entityPlayer.inventory.holdingItemData.actionData[1]).idx;
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
		materialGrid = base.Parent.GetChildByType<XUiC_MaterialStackGrid>();
		XUiController[] childrenByType = materialGrid.GetChildrenByType<XUiC_MaterialStack>();
		XUiController[] array = childrenByType;
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
		lblTotal = Localization.Get("lblTotalItems");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnChangedHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		Page = 0;
		GetMaterialData(txtInput.Text);
		materialGrid.SetMaterials(currentItems, CurrentPaintId);
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
	public void GetMaterialData(string _name)
	{
		if (_name == null)
		{
			_name = "";
		}
		currentItems.Clear();
		length = materialGrid.Length;
		Page = 0;
		FilterByName(_name);
	}

	public void FilterByName(string _name)
	{
		bool flag = GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) && GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled);
		currentItems.Clear();
		for (int i = 0; i < BlockTextureData.list.Length; i++)
		{
			BlockTextureData blockTextureData = BlockTextureData.list[i];
			if (blockTextureData == null || (blockTextureData.Hidden && !flag))
			{
				continue;
			}
			if (_name != "")
			{
				string name = blockTextureData.Name;
				if (_name == "" || name.ContainsCaseInsensitive(_name))
				{
					currentItems.Add(blockTextureData);
				}
			}
			else
			{
				currentItems.Add(blockTextureData);
			}
		}
		pager?.SetLastPageByElementsAndPageLength(currentItems.Count, length);
		resultCount.Text = string.Format(lblTotal, currentItems.Count.ToString());
	}

	public override void OnOpen()
	{
		base.OnOpen();
		GetMaterialData(txtInput.Text);
		IsDirty = true;
		int currentPaintId = CurrentPaintId;
		materialGrid.SetMaterials(currentItems, currentPaintId);
		int holdingItemIdx = base.xui.playerUI.entityPlayer.inventory.holdingItemIdx;
		XUiC_Toolbelt childByType = ((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow("toolbelt")).Controller.GetChildByType<XUiC_Toolbelt>();
		base.xui.dragAndDrop.InMenu = true;
		if (childByType != null)
		{
			childByType.GetSlotControl(holdingItemIdx).AssembleLock = true;
		}
		windowGroup.Controller.GetChildByType<XUiC_WindowNonPagingHeader>().SetHeader(Localization.Get("xuiMaterials").ToUpper());
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.currentToolTip.ToolTip = "";
		int holdingItemIdx = base.xui.playerUI.entityPlayer.inventory.holdingItemIdx;
		XUiController childByType = ((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow("toolbelt")).Controller.GetChildByType<XUiC_Toolbelt>();
		base.xui.dragAndDrop.InMenu = false;
		if (childByType != null)
		{
			(childByType as XUiC_Toolbelt).GetSlotControl(holdingItemIdx).AssembleLock = false;
		}
		if (base.xui.playerUI.windowManager.IsWindowOpen("windowpaging"))
		{
			base.xui.playerUI.windowManager.Close("windowpaging");
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (viewComponent.IsVisible)
		{
			if (null != base.xui.playerUI && base.xui.playerUI.playerInput != null && base.xui.playerUI.playerInput.GUIActions != null)
			{
				_ = base.xui.playerUI.playerInput.GUIActions;
			}
			if (IsDirty)
			{
				RefreshBindings();
				materialGrid.IsDirty = true;
			}
		}
	}
}
