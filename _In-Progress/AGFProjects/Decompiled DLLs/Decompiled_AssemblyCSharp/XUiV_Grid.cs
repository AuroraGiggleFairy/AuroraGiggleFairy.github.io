using System;
using System.Collections.Generic;
using UnityEngine;

public class XUiV_Grid : XUiView
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int columns;

	[PublicizedFrom(EAccessModifier.Private)]
	public int rows;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIWidget widget;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public UIGrid Grid { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public UIGrid.Arrangement Arrangement { get; set; }

	public int Columns
	{
		get
		{
			return columns;
		}
		set
		{
			if (initialized && Arrangement == UIGrid.Arrangement.Horizontal && Grid.maxPerLine != value)
			{
				Grid.maxPerLine = value;
				Grid.Reposition();
			}
			columns = value;
			isDirty = true;
		}
	}

	public int Rows
	{
		get
		{
			return rows;
		}
		set
		{
			if (initialized && Arrangement != UIGrid.Arrangement.Horizontal && Grid.maxPerLine != value)
			{
				Grid.maxPerLine = value;
				Grid.Reposition();
			}
			rows = value;
			isDirty = true;
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

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int CellWidth { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int CellHeight { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool HideInactive { get; set; }

	public event UIGrid.OnSizeChanged OnSizeChanged;

	public event Action OnSizeChangedSimple;

	public XUiV_Grid(string _id)
		: base(_id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateComponents(GameObject _go)
	{
		_go.AddComponent<UIWidget>();
		_go.AddComponent<UIGrid>();
	}

	public override void InitView()
	{
		base.InitView();
		widget = uiTransform.gameObject.GetComponent<UIWidget>();
		widget.pivot = pivot;
		widget.depth = base.Depth + 2;
		widget.autoResizeBoxCollider = true;
		Grid = uiTransform.gameObject.GetComponent<UIGrid>();
		Grid.hideInactive = HideInactive;
		Grid.arrangement = Arrangement;
		Grid.pivot = pivot;
		Grid.onSizeChanged = OnGridSizeChanged;
		if (Arrangement == UIGrid.Arrangement.Horizontal)
		{
			Grid.maxPerLine = Columns;
		}
		else
		{
			Grid.maxPerLine = Rows;
		}
		Grid.cellWidth = CellWidth;
		Grid.cellHeight = CellHeight;
		uiTransform.localScale = Vector3.one;
		uiTransform.localPosition = new Vector3(base.PaddedPosition.x, base.PaddedPosition.y, 0f);
		initialized = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGridSizeChanged(Vector2Int _cells, Vector2 _size)
	{
		widget.width = Mathf.RoundToInt(_size.x);
		widget.height = Mathf.RoundToInt(_size.y);
		this.OnSizeChanged?.Invoke(_cells, _size);
		this.OnSizeChangedSimple?.Invoke();
	}

	public override void Update(float _dt)
	{
		if (isDirty)
		{
			if (Arrangement == UIGrid.Arrangement.Horizontal)
			{
				Grid.maxPerLine = Columns;
			}
			else
			{
				Grid.maxPerLine = Rows;
			}
		}
		Grid.cellWidth = CellWidth;
		Grid.cellHeight = CellHeight;
		Grid.repositionNow = true;
		base.Update(_dt);
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		Columns = 0;
		Rows = 0;
		CellWidth = 0;
		CellHeight = 0;
		Arrangement = UIGrid.Arrangement.Horizontal;
		HideInactive = true;
	}

	public override bool ParseAttribute(string attribute, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(attribute, value, _parent);
		if (!flag)
		{
			switch (attribute)
			{
			case "cols":
				Columns = int.Parse(value);
				break;
			case "rows":
				Rows = int.Parse(value);
				break;
			case "cell_width":
				CellWidth = int.Parse(value);
				break;
			case "cell_height":
				CellHeight = int.Parse(value);
				break;
			case "arrangement":
				Arrangement = EnumUtils.Parse<UIGrid.Arrangement>(value, _ignoreCase: true);
				break;
			case "hide_inactive":
				HideInactive = StringParsers.ParseBool(value);
				break;
			default:
				return false;
			}
			return true;
		}
		return flag;
	}

	public override void setRepeatContentTemplateParams(Dictionary<string, object> _templateParams, int _curRepeatNum)
	{
		base.setRepeatContentTemplateParams(_templateParams, _curRepeatNum);
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
