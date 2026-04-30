using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ShapeStackGrid : XUiController
{
	public class ShapeData
	{
		public Block Block;

		public int Index;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int curPageIdx;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int numPages;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController[] shapeControllers;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bAwakeCalled;

	public XUiC_ShapesWindow Owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public Block selectedBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ShapeData> currentList;

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
		XUiController[] childrenByType = GetChildrenByType<XUiC_ShapeStack>();
		shapeControllers = childrenByType;
		Length = shapeControllers.Length;
		Log.Out("ShapeControllers: " + shapeControllers.Length);
		bAwakeCalled = true;
		IsDirty = false;
		IsDormant = true;
	}

	public void SetShapes(List<ShapeData> shapeIndexList, int newSelectedBlock = -1)
	{
		if (GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled))
		{
			GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled);
		}
		else
			_ = 0;
		XUiC_ShapeInfoWindow childByType = base.xui.GetChildByType<XUiC_ShapeInfoWindow>();
		XUiC_ShapeMaterialInfoWindow childByType2 = base.xui.GetChildByType<XUiC_ShapeMaterialInfoWindow>();
		int count = shapeIndexList.Count;
		currentList = shapeIndexList;
		for (int i = 0; i < Length; i++)
		{
			int num = i + Length * page;
			XUiC_ShapeStack xUiC_ShapeStack = (XUiC_ShapeStack)shapeControllers[i];
			xUiC_ShapeStack.Owner = this;
			xUiC_ShapeStack.InfoWindow = childByType;
			xUiC_ShapeStack.MaterialInfoWindow = childByType2;
			if (num < count)
			{
				xUiC_ShapeStack.BlockData = shapeIndexList[num].Block;
				xUiC_ShapeStack.ShapeIndex = shapeIndexList[num].Index;
				if (xUiC_ShapeStack.BlockData == selectedBlock)
				{
					xUiC_ShapeStack.Selected = true;
				}
				if (xUiC_ShapeStack.Selected && xUiC_ShapeStack.BlockData != selectedBlock)
				{
					xUiC_ShapeStack.Selected = false;
				}
			}
			else
			{
				xUiC_ShapeStack.BlockData = null;
				xUiC_ShapeStack.ShapeIndex = -1;
				if (xUiC_ShapeStack.Selected)
				{
					xUiC_ShapeStack.Selected = false;
				}
			}
		}
		if (selectedBlock == null && newSelectedBlock != -1)
		{
			for (int j = 0; j < shapeControllers.Length; j++)
			{
				XUiC_ShapeStack xUiC_ShapeStack2 = shapeControllers[j] as XUiC_ShapeStack;
				if (xUiC_ShapeStack2.BlockData != null && xUiC_ShapeStack2.ShapeIndex == newSelectedBlock)
				{
					xUiC_ShapeStack2.SetSelectedShapeForItem();
					xUiC_ShapeStack2.Selected = true;
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
			SetShapes(currentList, Owner.ItemValue.Meta);
			isDirty = false;
		}
	}
}
