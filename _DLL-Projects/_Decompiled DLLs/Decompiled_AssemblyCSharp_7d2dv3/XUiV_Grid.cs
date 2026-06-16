using System.Collections.Generic;
using UnityEngine;

public class XUiV_Grid : XUiView_WidgetBased
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int columns;

	[PublicizedFrom(EAccessModifier.Private)]
	public int rows;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIGrid grid;

	[XuiXmlAttribute("arrangement", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public UIGrid.Arrangement Arrangement { get; set; }

	[XuiXmlAttribute("cols", false)]
	public int Columns
	{
		get
		{
			return columns;
		}
		set
		{
			if (columns != value)
			{
				columns = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("rows", false)]
	public int Rows
	{
		get
		{
			return rows;
		}
		set
		{
			if (rows != value)
			{
				rows = value;
				SetDirty();
			}
		}
	}

	public override int RepeatCount
	{
		get
		{
			return Columns * Rows;
		}
		set
		{
		}
	}

	[XuiXmlAttribute("cell_width", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int CellWidth { get; set; }

	[XuiXmlAttribute("cell_height", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int CellHeight { get; set; }

	[XuiXmlAttribute("hide_inactive", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool HideInactive { get; set; }

	public override Vector2i InnerSize => new Vector2i(CellWidth, CellHeight);

	public event UIGrid.OnSizeChanged OnSizeChanged;

	public XUiV_Grid(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		_go.AddComponent<UIWidget>();
		_go.AddComponent<UIGrid>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		widget = uiTransform.gameObject.GetComponent<UIWidget>();
		grid = uiTransform.gameObject.GetComponent<UIGrid>();
	}

	public override void InitView()
	{
		base.InitView();
		widget.autoResizeBoxCollider = true;
		grid.hideInactive = HideInactive;
		grid.arrangement = Arrangement;
		grid.pivot = pivot;
		grid.onSizeChanged = OnGridSizeChanged;
		grid.maxPerLine = ((Arrangement == UIGrid.Arrangement.Horizontal) ? Columns : Rows);
		grid.cellWidth = CellWidth;
		grid.cellHeight = CellHeight;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGridSizeChanged(Vector2Int _cells, Vector2 _size)
	{
		widget.width = Mathf.RoundToInt(_size.x);
		widget.height = Mathf.RoundToInt(_size.y);
		size = new Vector2i(widget.width, widget.height);
		this.OnSizeChanged?.Invoke(_cells, _size);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void refreshBoxCollider()
	{
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		grid.repositionNow = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		grid.maxPerLine = ((Arrangement == UIGrid.Arrangement.Horizontal) ? Columns : Rows);
		grid.cellWidth = CellWidth;
		grid.cellHeight = CellHeight;
		base.updateData();
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		Columns = 0;
		Rows = 0;
		CellWidth = int.MinValue;
		CellHeight = int.MinValue;
		Arrangement = UIGrid.Arrangement.Horizontal;
		HideInactive = true;
	}

	public override void SetPostParsingDefaults(XUiController _parent)
	{
		base.SetPostParsingDefaults(_parent);
		if (CellWidth == int.MinValue)
		{
			CellWidth = base.Width;
		}
		if (CellHeight == int.MinValue)
		{
			CellHeight = base.Height;
		}
	}

	public override void SetRepeatContentTemplateParams(Dictionary<string, object> _templateParams, int _curRepeatNum)
	{
		base.SetRepeatContentTemplateParams(_templateParams, _curRepeatNum);
		int num;
		int num2;
		if (Arrangement == UIGrid.Arrangement.Horizontal)
		{
			num = _curRepeatNum % Columns;
			num2 = _curRepeatNum / Columns;
		}
		else
		{
			num = _curRepeatNum / Rows;
			num2 = _curRepeatNum % Rows;
		}
		_templateParams["repeat_col"] = num;
		_templateParams["repeat_row"] = num2;
	}
}
