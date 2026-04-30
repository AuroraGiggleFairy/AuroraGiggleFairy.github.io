using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TraderItemList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TraderItemEntry selectedEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selectedIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ItemStack> items = new List<ItemStack>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> indexList = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	public int Length;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showFavorites;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectedColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public string category;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_TraderItemEntry> entryList = new List<XUiC_TraderItemEntry>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ItemStack CurrentItem
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int Page
	{
		get
		{
			return page;
		}
		set
		{
			page = value;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemInfoWindow InfoWindow { get; set; }

	public XUiC_TraderItemEntry SelectedEntry
	{
		get
		{
			return selectedEntry;
		}
		set
		{
			if (selectedEntry != null)
			{
				selectedEntry.Selected = false;
			}
			selectedEntry = value;
			if (selectedEntry != null)
			{
				selectedEntry.Selected = true;
				for (int i = 0; i < indexList.Count; i++)
				{
					if (selectedEntry.SlotIndex == indexList[i])
					{
						selectedIndex = i % entryList.Count;
						break;
					}
				}
				InfoWindow.ViewComponent.IsVisible = true;
				InfoWindow.SetItemStack(selectedEntry);
				CurrentItem = selectedEntry.Item;
				return;
			}
			if (selectedIndex >= 0)
			{
				if (entryList[selectedIndex].Item != null)
				{
					SelectedEntry = entryList[selectedIndex];
				}
				else if (entryList[0].Item != null)
				{
					SelectedEntry = entryList[0];
				}
			}
			if (SelectedEntry == null)
			{
				selectedIndex = -1;
				InfoWindow.SetItemStack((XUiC_TraderItemEntry)null, false);
				CurrentItem = null;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		for (int i = 0; i < children.Count; i++)
		{
			XUiController xUiController = children[i];
			if (xUiController is XUiC_TraderItemEntry)
			{
				entryList.Add((XUiC_TraderItemEntry)xUiController);
			}
		}
		XUiV_Grid xUiV_Grid = (XUiV_Grid)base.ViewComponent;
		if (xUiV_Grid != null)
		{
			Length = xUiV_Grid.Columns * xUiV_Grid.Rows;
		}
		InfoWindow = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressEntry(XUiController _sender, int _mouseButton)
	{
		if (_sender is XUiC_TraderItemEntry xUiC_TraderItemEntry)
		{
			SelectedEntry = xUiC_TraderItemEntry;
			if (InputUtils.ShiftKeyPressed)
			{
				xUiC_TraderItemEntry.InfoWindow.BuySellCounter.SetToMaxCount();
			}
			else
			{
				xUiC_TraderItemEntry.InfoWindow.BuySellCounter.Count = xUiC_TraderItemEntry.Item.itemValue.ItemClass.EconomicBundleSize;
			}
		}
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = true;
		}
		ClearSelection();
		IsDirty = true;
	}

	public override void OnClose()
	{
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = false;
		}
		ClearSelection();
	}

	public void ClearSelection()
	{
		CurrentItem = null;
		selectedIndex = -1;
		SelectedEntry = null;
		InfoWindow.SetItemStack((XUiC_TraderItemEntry)null, false);
	}

	public void SetItems(ItemStack[] _stackList, List<int> _indexList)
	{
		if (_stackList == null)
		{
			return;
		}
		items.Clear();
		items.AddRange(_stackList);
		indexList.Clear();
		indexList.AddRange(_indexList);
		XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
		for (int i = 0; i < Length; i++)
		{
			int num = i + Length * page;
			entryList[i].OnPress -= OnPressEntry;
			entryList[i].InfoWindow = childByType;
			if (num < items.Count)
			{
				entryList[i].SlotIndex = _indexList[num];
				entryList[i].Item = _stackList[num];
				entryList[i].OnPress += OnPressEntry;
				entryList[i].ViewComponent.SoundPlayOnClick = true;
			}
			else
			{
				entryList[i].Item = null;
				entryList[i].ViewComponent.SoundPlayOnClick = false;
			}
		}
		if (CurrentItem == null || !CurrentItem.itemValue.Equals(SelectedEntry?.Item?.itemValue))
		{
			ClearSelection();
		}
	}
}
