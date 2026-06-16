using System.Collections.Generic;
using System.Linq;

public class XUiC_VariableHeightGrid : XUiC_ItemStackGrid
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityCollector tileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRowWidth = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid[] grids;

	[PublicizedFrom(EAccessModifier.Private)]
	public int gridHeight = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool heightChanged = true;

	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Workstation;
		}
	}

	public void SetHeight(int height)
	{
		gridHeight = height;
		heightChanged = true;
	}

	public void InitVariableHeightGrids(XUiV_Grid[] _grids)
	{
		grids = _grids;
	}

	public override void OnOpen()
	{
		base.OnOpen();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (heightChanged)
		{
			heightChanged = false;
			XUiV_Grid[] array = grids;
			foreach (XUiV_Grid grid in array)
			{
				updateGridHeight(grid, gridHeight);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateGridHeight(XUiV_Grid grid, int height)
	{
		grid.Rows = height;
		List<XUiController> list = grid.Controller.Children;
		for (int i = 0; i < list.Count; i++)
		{
			int num = i / 3;
			list[i].ViewComponent.IsVisible = num < height;
		}
	}

	public override XUiC_ItemStack[] GetItemStackControllers()
	{
		int count = 3 * gridHeight;
		return itemControllers.Take(count).ToArray();
	}
}
