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

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

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
			isDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = GetChildrenByType<XUiC_MaterialStack>();
		materialControllers = childrenByType;
		Length = materialControllers.Length;
		bAwakeCalled = true;
		IsDirty = false;
		IsDormant = true;
	}

	public void SetMaterials(List<BlockTextureData> materialIndexList, int newSelectedMaterial = -1)
	{
		bool isCreative = GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) && GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled);
		materialIndexList = (from m in materialIndexList
			orderby m.GetLocked(base.xui.playerUI.entityPlayer), m.ID != 0 && isCreative, m.Group, m.SortIndex, m.LocalizedName
			select m).ToList();
		XUiC_MaterialInfoWindow childByType = base.xui.GetChildByType<XUiC_MaterialInfoWindow>();
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
					xUiC_MaterialStack.Selected = true;
				}
				if (xUiC_MaterialStack.Selected && xUiC_MaterialStack.TextureData != selectedMaterial)
				{
					xUiC_MaterialStack.Selected = false;
				}
			}
			else
			{
				xUiC_MaterialStack.TextureData = null;
				if (xUiC_MaterialStack.Selected)
				{
					xUiC_MaterialStack.Selected = false;
				}
			}
		}
		if (selectedMaterial == null && newSelectedMaterial != -1)
		{
			for (int num3 = 0; num3 < materialControllers.Length; num3++)
			{
				XUiC_MaterialStack xUiC_MaterialStack2 = materialControllers[num3] as XUiC_MaterialStack;
				if (xUiC_MaterialStack2.TextureData != null && !xUiC_MaterialStack2.IsLocked)
				{
					xUiC_MaterialStack2.SetSelectedTextureForItem();
					xUiC_MaterialStack2.Selected = true;
					return;
				}
			}
		}
		IsDirty = false;
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = true;
		}
		IsDormant = false;
	}

	public override void OnClose()
	{
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = false;
		}
		IsDormant = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (isDirty)
		{
			SetMaterials(newSelectedMaterial: (int)((!(base.xui.playerUI.entityPlayer.inventory.holdingItem is ItemClassBlock)) ? ((ItemActionTextureBlock.ItemActionTextureBlockData)base.xui.playerUI.entityPlayer.inventory.holdingItemData.actionData[1]).idx : (base.xui.playerUI.entityPlayer.inventory.holdingItemItemValue.TextureFullArray[0] & 0xFF)), materialIndexList: currentList);
			isDirty = false;
		}
	}
}
