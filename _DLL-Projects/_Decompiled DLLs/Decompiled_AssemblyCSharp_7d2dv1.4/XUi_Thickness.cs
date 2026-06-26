public class XUi_Thickness
{
	public int left;

	public int top;

	public int right;

	public int bottom;

	public XUi_Thickness(int newLeft, int newTop, int newRight, int newBottom)
	{
		left = newLeft;
		top = newTop;
		right = newRight;
		bottom = newBottom;
	}

	public XUi_Thickness(int newLeftRight, int newTopBottom)
		: this(newLeftRight, newTopBottom, newLeftRight, newTopBottom)
	{
	}

	public XUi_Thickness(int newSides)
		: this(newSides, newSides, newSides, newSides)
	{
	}

	public static XUi_Thickness Parse(string _s)
	{
		string[] array = _s.Split(',');
		return array.Length switch
		{
			1 => new XUi_Thickness(int.Parse(array[0])), 
			2 => new XUi_Thickness(int.Parse(array[0]), int.Parse(array[1])), 
			4 => new XUi_Thickness(int.Parse(array[0]), int.Parse(array[1]), int.Parse(array[2]), int.Parse(array[3])), 
			_ => new XUi_Thickness(0), 
		};
	}
}
