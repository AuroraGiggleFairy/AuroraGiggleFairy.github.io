using UnityEngine;

public class XUiV_Table : XUiView
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstUpdate = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool repositionNextFrame;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public UITable Table { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public UITable.Sorting Sorting { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int Columns { get; set; } = 1;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Vector2 Padding { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool HideInactive { get; set; } = true;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool AlwaysReposition { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool RepositionTwice { get; set; }

	public XUiV_Table(string _id)
		: base(_id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateComponents(GameObject _go)
	{
		_go.AddComponent<UITable>();
	}

	public override void InitView()
	{
		base.InitView();
		Table = uiTransform.gameObject.GetComponent<UITable>();
		Table.hideInactive = HideInactive;
		Table.sorting = Sorting;
		Table.direction = UITable.Direction.Down;
		Table.columns = Columns;
		Table.padding = Padding;
		uiTransform.localScale = Vector3.one;
		uiTransform.localPosition = new Vector3(base.PaddedPosition.x, base.PaddedPosition.y, 0f);
		Table.Reposition();
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		IsVisible = false;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		Table.repositionNow = true;
		if (RepositionTwice)
		{
			repositionNextFrame = true;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (firstUpdate || AlwaysReposition)
		{
			firstUpdate = false;
			Table.Reposition();
		}
		if (repositionNextFrame && !Table.enabled)
		{
			Table.repositionNow = true;
			repositionNextFrame = false;
		}
	}

	public override bool ParseAttribute(string attribute, string value, XUiController _parent)
	{
		switch (attribute)
		{
		case "columns":
			Columns = int.Parse(value);
			break;
		case "padding":
			Padding = StringParsers.ParseVector2(value);
			break;
		case "sorting":
			Sorting = EnumUtils.Parse<UITable.Sorting>(value, _ignoreCase: true);
			break;
		case "hide_inactive":
			HideInactive = StringParsers.ParseBool(value);
			break;
		case "always_reposition":
			AlwaysReposition = StringParsers.ParseBool(value);
			break;
		case "reposition_twice":
			RepositionTwice = StringParsers.ParseBool(value);
			break;
		default:
			return base.ParseAttribute(attribute, value, _parent);
		}
		return true;
	}
}
