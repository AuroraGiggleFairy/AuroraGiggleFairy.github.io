using System.Collections.Generic;

public class XUiControllerComparerDepth : IComparer<XUiController>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiControllerComparerDepth instance;

	public static XUiControllerComparerDepth Instance => instance ?? (instance = new XUiControllerComparerDepth());

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiControllerComparerDepth()
	{
	}

	public int Compare(XUiController _x, XUiController _y)
	{
		if (_x == _y)
		{
			return 0;
		}
		if (_y == null)
		{
			return 1;
		}
		return _x?.ViewComponent.Depth.CompareTo(_y.ViewComponent.Depth) ?? (-1);
	}
}
