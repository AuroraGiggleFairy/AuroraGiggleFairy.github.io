using System.Collections.Generic;
using UniLinq;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MaterialStackGrid : XUiController
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public int curPageIdx;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int numPages;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController[] materialControllers;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int[] materialIndices;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bAwakeCalled;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockTextureData selectedMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlockTextureData> currentList;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int Length
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
			IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = GetChildrenByType<XUiC_MaterialStack>();
		materialControllers = childrenByType;
		Length = materialControllers.Length;
		bAwakeCalled = true;
	}

	public void SetMaterials(List<BlockTextureData> materialIndexList, int newSelectedMaterial = -1)
	{
		bool isCreative = GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) && GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled);
		materialIndexList = (from m in materialIndexList
			orderby m.GetLocked(xui.playerUI.entityPlayer), m.ID != 0 && isCreative, m.Group, m.SortIndex, m.LocalizedName
			select m).ToList();
		XUiC_MaterialInfoWindow childByType = xui.GetChildByType<XUiC_MaterialInfoWindow>();
		int count = materialIndexList.Count;
		currentList = materialIndexList;
		if (newSelectedMaterial != -1)
		{
			selectedMaterial = BlockTextureData.list[newSelectedMaterial];
			if (selectedMaterial.Hidden && !isCreative)
			{
				selectedMaterial = null;
			}
		}
		for (int num = 0; num < Length; num++)
		{
			int num2 = num + Length * page;
			XUiC_MaterialStack xUiC_MaterialStack = (XUiC_MaterialStack)materialControllers[num];
			xUiC_MaterialStack.InfoWindow = childByType;
			if (num2 < count)
			{
				xUiC_MaterialStack.TextureData = materialIndexList[num2];
				if (xUiC_MaterialStack.TextureData == selectedMaterial)
				{
					xUiC_MaterialStack.IsSelected = true;
				}
				if (xUiC_MaterialStack.IsSelected && xUiC_MaterialStack.TextureData != selectedMaterial)
				{
					xUiC_MaterialStack.IsSelected = false;
				}
			}
			else
			{
				xUiC_MaterialStack.TextureData = null;
				if (xUiC_MaterialStack.IsSelected)
				{
					xUiC_MaterialStack.IsSelected = false;
				}
			}
		}
		if (selectedMaterial != null || newSelectedMaterial == -1)
		{
			return;
		}
		for (int num3 = 0; num3 < materialControllers.Length; num3++)
		{
			XUiC_MaterialStack xUiC_MaterialStack2 = materialControllers[num3] as XUiC_MaterialStack;
			if (xUiC_MaterialStack2.TextureData != null && !xUiC_MaterialStack2.IsLocked)
			{
				xUiC_MaterialStack2.SetSelectedTextureForItem();
				xUiC_MaterialStack2.IsSelected = true;
				break;
			}
		}
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = true;
		}
	}

	public override void OnClose()
	{
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = false;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			SetMaterials(newSelectedMaterial: (int)((!(xui.playerUI.entityPlayer.inventory.holdingItem is ItemClassBlock)) ? ((ItemActionTextureBlock.ItemActionTextureBlockData)xui.playerUI.entityPlayer.inventory.holdingItemData.actionData[1]).idx : (xui.playerUI.entityPlayer.inventory.holdingItemItemValue.TextureFullArray[0] & 0xFF)), materialIndexList: currentList);
			IsDirty = false;
		}
	}
}
