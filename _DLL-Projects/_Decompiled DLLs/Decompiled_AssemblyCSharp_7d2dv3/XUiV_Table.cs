using UnityEngine;

public class XUiV_Table : XUiView
{
	[PublicizedFrom(EAccessModifier.Private)]
	public UITable table;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIWidget.Pivot pivot;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIWidget.Pivot cellAlignment;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstUpdate = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool repositionNextFrame;

	public override UIRect UiRect
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return null;
		}
	}

	[XuiXmlAttribute("sorting", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public UITable.Sorting Sorting { get; set; }

	[XuiXmlAttribute("cols", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int Columns { get; set; } = 1;

	[XuiXmlAttribute("padding", true)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Vector2 Padding { get; set; }

	[XuiXmlAttribute("hide_inactive", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool HideInactive { get; set; } = true;

	[XuiXmlAttribute("always_reposition", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool AlwaysReposition { get; set; }

	[XuiXmlAttribute("reposition_twice", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool RepositionTwice { get; set; }

	[XuiXmlAttribute("pivot", false)]
	public UIWidget.Pivot Pivot
	{
		get
		{
			return pivot;
		}
		set
		{
			if (pivot != value)
			{
				pivot = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("cell_alignment", false)]
	public UIWidget.Pivot CellAlignment
	{
		get
		{
			return cellAlignment;
		}
		set
		{
			if (cellAlignment != value)
			{
				cellAlignment = value;
				SetDirty();
			}
		}
	}

	public override Vector3[] WorldCorners => XUiV_Empty.WorldCornersEmpty;

	public XUiV_Table(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		_go.AddComponent<UITable>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		table = uiTransform.gameObject.GetComponent<UITable>();
	}

	public override void InitView()
	{
		base.InitView();
		table.hideInactive = HideInactive;
		table.sorting = Sorting;
		table.direction = UITable.Direction.Down;
		table.columns = Columns;
		table.padding = Padding;
		table.pivot = pivot;
		table.cellAlignment = cellAlignment;
		table.Reposition();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		table.repositionNow = true;
		if (RepositionTwice)
		{
			repositionNextFrame = true;
		}
	}

	public override void Update(float _dt)
	{
		if (firstUpdate || AlwaysReposition)
		{
			firstUpdate = false;
			table.Reposition();
		}
		if (repositionNextFrame && !table.enabled)
		{
			table.repositionNow = true;
			repositionNextFrame = false;
		}
		base.Update(_dt);
	}

	public void Reposition()
	{
		repositionNextFrame = true;
	}
}
